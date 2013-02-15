using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;

namespace CloudSoft.Repositories
{
	public interface IDbContextFactory<TContext> : IDisposable
		where TContext : System.Data.Entity.DbContext, IObjectContextAdapter, new()
	{
		TContext GetDbContext();
		IObjectContextAdapter GetObjectContextAdapter();
	}
}
