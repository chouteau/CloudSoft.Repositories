using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSoft.Repositories
{
	public class DbAsyncQueryProvider<T> : IDbAsyncQueryProvider
	{
		private readonly IQueryProvider m_QueryProvider;
		private readonly IEnumerator<T> m_Enumerator;

		public DbAsyncQueryProvider(IQueryable<T> innerQueryable)
		{
			m_QueryProvider = innerQueryable.Provider;
			m_Enumerator = innerQueryable.GetEnumerator();
		}

		public Task<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute<TResult>(expression));
		}

		public Task<object> ExecuteAsync(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute(expression));
		}

		public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
		{
			return new DbAsyncEnumerableQuery<TElement>(expression);
		}

		public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
		{
			return new DbAsyncEnumerableQuery<T>(expression);
		}

		public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
		{
			return m_QueryProvider.Execute<TResult>(expression);
		}

		public object Execute(System.Linq.Expressions.Expression expression)
		{
			return m_QueryProvider.Execute(expression);
		}
	}
}
