using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CloudSoft.Repositories
{
	public class DbAsyncEnumerableQuery<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
	{
		public DbAsyncEnumerableQuery(IEnumerable<T> enumerable)
			: base(enumerable)
		{
		}

		public DbAsyncEnumerableQuery(Expression expression)
			: base(expression)
		{
		}

		public void Add(T item)
		{

		}

		public IDbAsyncEnumerator<T> GetAsyncEnumerator()
		{
			return new DbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
		}

		IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
		{
			return GetAsyncEnumerator();
		}

		IQueryProvider IQueryable.Provider
		{
			get { return new DbAsyncQueryProvider<T>(this); }
		} 
	}
}
