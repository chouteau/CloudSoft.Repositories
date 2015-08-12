using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSoft.Repositories
{
	public class DbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
	{
		private readonly IEnumerator<T> m_Enumerator;

		public DbAsyncEnumerator(IEnumerator<T> inner)
		{
			m_Enumerator = inner;
		}

		public T Current
		{
			get { return m_Enumerator.Current; }
		}

		object IDbAsyncEnumerator.Current
		{
			get { return Current; }
		}

		public Task<bool> MoveNextAsync(System.Threading.CancellationToken cancellationToken)
		{
			return Task.FromResult(m_Enumerator.MoveNext());
		}

		public void Dispose()
		{
			m_Enumerator.Dispose();
		}
	}
}
