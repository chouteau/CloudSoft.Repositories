using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace CloudSoft.Repositories.Initializers
{
	public class SqlSchemaInitializer<TContext> : IRepositoryInitializer
		where TContext : System.Data.Entity.DbContext, IObjectContextAdapter, new()
	{
		private class SqlPatch
		{
			public int SchemaId { get; set; }
			public string Name { get; set; }
			public string Script { get; set; }
		}

		private DbContext m_DbContext;
		private string m_SchemaTableName;
		private string m_EmbededScriptNamespace;
		private SchemaRepository m_SchemaSqlRepository;

		public SqlSchemaInitializer(IDbContextFactory<TContext> dbContextFactory)
		{
			m_DbContext = dbContextFactory.GetDbContext();
		}

		//public SqlSchemaInitializer(DbContext dbContext)
		//{
		//    if (dbContext == null)
		//    {
		//        throw new ArgumentNullException("dbContext");
		//    }
		//    m_DbContext = dbContext;
		//}

		public void Initialize(string schemaTableName, string embededScriptNameSpace)
		{
			Database.SetInitializer<TContext>(null);

			m_SchemaTableName = schemaTableName;
			m_EmbededScriptNamespace = embededScriptNameSpace ?? m_DbContext.GetType().Namespace + ".Scripts";

			var dbContextFactory = new SchemaDbContextFactory<SchemaDbContext>(schemaTableName, m_DbContext.Database.Connection);
			m_SchemaSqlRepository = new SchemaRepository(dbContextFactory);
			// m_SchemaSqlRepository = new SchemaRepository(schemaTableName, m_DbContext.Database.Connection);

			var currentDbSchemaId = GetSchemaId();
			var patchList = GetSqlPatchList();
			var nextSchemaId = patchList.Max(i => i.SchemaId);
			if (currentDbSchemaId == nextSchemaId)
			{
				return;
			}

			ApplyPatchs(currentDbSchemaId + 1, patchList);
			if (m_DbContext is ISeedDatabase)
			{
				((ISeedDatabase)m_DbContext).Seed();
			}
		}

		private int GetSchemaId()
		{
			if (!SchemaTableExists())
			{
				CreateSchemaTable();
			}
			var lastSchema = (from schema in m_SchemaSqlRepository.Query<Schema>()
							  orderby schema.Id descending
							  select schema).FirstOrDefault();

			var result = 0;
			if (lastSchema != null)
			{
				result = lastSchema.Id;
			}
			return result;
		}

		private bool SchemaTableExists()
		{
			var tableNameParameter = new System.Data.SqlClient.SqlParameter() { ParameterName = "@TableName", Value = m_SchemaTableName };
			var result = m_SchemaSqlRepository.ExecuteStoreQuery<int>("select count(*) from sysobjects where name = @TableName and xtype = 'U'", tableNameParameter);
			return (result.First() != 0);
		}

		private void CreateSchemaTable()
		{
			var schemaTable = m_SchemaSqlRepository.CreateTableScript();
			m_SchemaSqlRepository.ExecuteStoreCommand(schemaTable);
		}

		private void ApplyPatchs(int startFrom, List<SqlPatch> patchList)
		{
			foreach (var sqlPatch in patchList.OrderBy(i => i.SchemaId))
			{
				if (sqlPatch.SchemaId < startFrom)
				{
					continue;
				}

				var dbca = GetObjectContextAdapter();
				dbca.ObjectContext.CommandTimeout = 600;
				using (var ts = GetNewReadCommittedTransaction())
				{
					ApplyPatch(sqlPatch, dbca);
					RefreshAllViews(dbca);
					UpdateSchema(sqlPatch);
					ts.Complete();
				}
			}
		}

		private void ApplyPatch(SqlPatch patch, IObjectContextAdapter objectContextAdapter)
		{
			var reader = new StringReader(patch.Script);
			while (true)
			{
				var sql = ReadNextStatementFromStream(reader);
				if (sql == null)
				{
					break;
				}
				try
				{
					objectContextAdapter.ObjectContext.ExecuteStoreCommand(sql);
				}
				catch (Exception ex)
				{
					var message = ex.Message + " " + patch.Name + " " + sql;
					var crashEx = new Exception(message, ex);
					crashEx.Data.Add("RepositoryInitializer.ApplyPatch." + patch.Name, sql);
					throw crashEx;
				}
			}

			reader.Close();
		}

		private void RefreshAllViews(IObjectContextAdapter objectContextAdapter)
		{
			string sql =
				@"
select 
name as vname
into #temp
from sysobjects 
where 
	xtype = 'V'

declare my_cursor Cursor
for 
select vname from #temp
open my_cursor
declare @Name varchar(1024)
fetch next from my_cursor into @Name
while (@@Fetch_status <> -1)
begin
	exec sp_refreshview @ViewName = @Name
	fetch next from my_cursor into @Name
end
close my_cursor
deallocate my_cursor

drop table #temp
";
			objectContextAdapter.ObjectContext.ExecuteStoreCommand(sql);

		}

		private string ReadNextStatementFromStream(StringReader reader)
		{
			var sb = new StringBuilder();

			string lineOfText;

			while (true)
			{
				lineOfText = reader.ReadLine();
				if (lineOfText == null)
				{

					if (sb.Length > 0)
					{
						return sb.ToString();
					}
					else
					{
						return null;
					}
				}

				if (lineOfText.TrimEnd().ToUpper() == "GO")
				{
					break;
				}

				sb.Append(lineOfText + Environment.NewLine);
			}

			return sb.ToString();
		}

		private List<SqlPatch> GetSqlPatchList()
		{
			var result = new List<SqlPatch>();
			var assembly = System.Reflection.Assembly.GetAssembly(m_DbContext.GetType());
			var embededList = assembly.GetManifestResourceNames();
			foreach (var script in embededList)
			{
				if (script.IndexOf(m_EmbededScriptNamespace) == -1)
				{
					continue;
				}

				var scriptName = script.Replace(m_EmbededScriptNamespace + ".", "");
				var match = System.Text.RegularExpressions.Regex.Match(scriptName, @"^(?<version>\d+)-(?<name>[^\.]+).sql");
				if (!match.Success)
				{
					continue;
				}
				var version = match.Groups["version"].Value;
				var name = match.Groups["name"].Value;
				string content = null;
				using (var stream = assembly.GetManifestResourceStream(script))
				{
					if (stream == null)
					{
						continue;
					}
					var buffer = new byte[stream.Length];

					stream.Read(buffer, 0, buffer.Length);
					content = System.Text.Encoding.UTF8.GetString(buffer);
					content = content.Substring(1);
				}
				result.Add(new SqlPatch()
				{
					SchemaId = Convert.ToInt32(version),
					Name = name,
					Script = content,
				});
			}

			return result;
		}

		private void UpdateSchema(SqlPatch patch)
		{
			var schema = new Schema();
			schema.Id = patch.SchemaId;
			schema.Name = patch.Name;
			schema.CreationDate = DateTime.Now;
			schema.Script = patch.Script;

			m_SchemaSqlRepository.Insert(schema);
		}

		public IObjectContextAdapter GetObjectContextAdapter()
		{
			return (IObjectContextAdapter)m_DbContext;
		}

		private System.Transactions.TransactionScope GetNewReadCommittedTransaction()
		{
			return new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew
							, new System.Transactions.TransactionOptions()
							{
								IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
							});
		}
	}
}
