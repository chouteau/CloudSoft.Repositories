using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;

namespace CloudSoft.Repositories
{
	public interface IRepository<TContext>
		where TContext : System.Data.Entity.DbContext, System.Data.Entity.Infrastructure.IObjectContextAdapter, new()
	{
		int Delete<T>(T entity) where T : class;
		int DeleteAll<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;
		void Dispose();
		int ExecuteStoreCommand(string cmdText, params object[] parameters);
		ObjectResult<T> ExecuteStoreQuery<T>(string cmdText, params object[] parameters);
		T Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;
		TContext GetDbContext();
		int Insert<T>(T entity) where T : class;
		System.Linq.IQueryable<T> Query<T, TKey>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, System.Linq.Expressions.Expression<Func<T, TKey>> orderBy) where T : class;
		System.Linq.IQueryable<T> Query<T, TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderBy) where T : class;
		System.Linq.IQueryable<T> Query<T>() where T : class;
		System.Linq.IQueryable<T> Query<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;
		int Update<T>(T entity) where T : class;
		void BulkInsert<T>(IEnumerable<T> list) where T : class;
	}
}
