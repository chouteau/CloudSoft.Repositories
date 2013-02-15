using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Data.Entity;

namespace CloudSoft.Repositories
{
	public class DbContextFactory<TContext> : IDbContextFactory<TContext>
		where TContext : System.Data.Entity.DbContext, IObjectContextAdapter, new()
	{
		private TContext m_DataContext;

		public DbContextFactory()
		{
			Database.SetInitializer<TContext>(null);
		}

		#region IDbContextFactory<TContext> Members

		public virtual TContext GetDbContext()
		{
			return CreateDbContext();
		}

		protected virtual TContext CreateDbContext()
		{
			return new TContext();
		}

		public virtual System.Data.Entity.Infrastructure.IObjectContextAdapter GetObjectContextAdapter()
		{
			var dbContext = GetDbContext();
			return (IObjectContextAdapter)dbContext;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (m_DataContext != null
				&& m_DataContext.Database != null
				&& m_DataContext.Database.Connection != null)
			{
				m_DataContext.Database.Connection.Close();
				m_DataContext.Dispose();
				m_DataContext = null;
			}
		}

		#endregion
	}
}
