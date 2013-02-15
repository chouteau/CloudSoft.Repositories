using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace CloudSoft.Repositories.Initializers
{
	internal class SchemaRepository : SqlRepository<SchemaDbContext>
	{
		private IDbContextFactory<SchemaDbContext> m_DbContextFactory;

		public SchemaRepository(IDbContextFactory<SchemaDbContext> dbContextFactory)
			: base(dbContextFactory)
		{
			m_DbContextFactory = dbContextFactory;
		}

		public override SchemaDbContext GetDbContext()
		{
			return m_DbContextFactory.GetDbContext();
		}

		public string CreateTableScript()
		{
			var oca = (IObjectContextAdapter)GetDbContext();
			var result = oca.ObjectContext.CreateDatabaseScript();
			return result;
		}
	}
}
