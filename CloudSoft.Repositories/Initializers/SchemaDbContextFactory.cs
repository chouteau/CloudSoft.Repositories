using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CloudSoft.Repositories.Initializers
{
	internal class SchemaDbContextFactory<TContext> : DbContextFactory<TContext>
		where TContext : SchemaDbContext, IObjectContextAdapter, new()
	{
		private SchemaDbContext m_DbContext;
		private string m_SchemaTableName;
		private System.Data.Common.DbConnection m_DbConnection;

		public SchemaDbContextFactory(string tableName, System.Data.Common.DbConnection dbConnection)
		{
			m_SchemaTableName = tableName;
			m_DbConnection = dbConnection;
		}

		public override TContext GetDbContext()
		{
			if (m_DbContext == null)
			{
				var builder = new DbModelBuilder(DbModelBuilderVersion.Latest);
				builder.Configurations.Add(new SchemaMap());
				builder.Entity<Schema>().ToTable(m_SchemaTableName);
				var compiledBuilder = builder.Build(m_DbConnection).Compile();
				m_DbContext = new SchemaDbContext(m_DbConnection.ConnectionString, compiledBuilder);
				m_DbContext.Configuration.ProxyCreationEnabled = false;
				Database.SetInitializer<TContext>(null);
			}

			return (TContext) m_DbContext;
		}
	}
}
