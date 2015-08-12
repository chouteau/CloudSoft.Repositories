using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Collections.Concurrent;
using System.Data.Entity.Core.Objects;
using System.Threading.Tasks;

namespace CloudSoft.Repositories
{
	/// <summary>
	/// For tests 
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	public class MemoryRepository<TContext> : IRepository<TContext>
		where TContext : System.Data.Entity.DbContext, IObjectContextAdapter, new()
	{
		private ConcurrentDictionary<string, object> m_Database;

		public MemoryRepository()
		{
			m_Database = new ConcurrentDictionary<string, object>();
		}

		#region IRepository<TContext> Members

		public int Delete<T>(T entity) where T : class
		{
			var table = GetOrCreateList<T>() as SynchronizedCollection<T>;
			if (table.Contains(entity))
			{
				lock (table.SyncRoot)
				{
					table.Remove(entity);
				}
				return 1;
			}
			return 0;
		}

		public async Task<int> DeleteAsync<T>(T entity) where T : class
		{
			return await Task<int>.Run(() =>
				{
					return Delete(entity);
				});
		}

		public int DeleteAll<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
		{
			var table = GetOrCreateList<T>();
			var result = 0;
			while (true)
			{
				var item = table.AsQueryable().FirstOrDefault(predicate);
				if (item == null)
				{
					break;
				}
				result++;
				var list = table as SynchronizedCollection<T>;
				lock(list.SyncRoot)
				{
					list.Remove(item);
				}
			}
			return result;
		}

		public void Dispose()
		{
			m_Database.Clear();
		}

		public int ExecuteStoreCommand(string cmdText, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public async Task<int> ExecuteStoreCommandAsync(string cmdText, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public ObjectResult<T> ExecuteStoreQuery<T>(string cmdText, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public async Task<ObjectResult<T>> ExecuteStoreQueryAsync<T>(string cmdText, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public T Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
		{
			var table = GetOrCreateList<T>();
			T result = default(T);
			result = table.AsQueryable().FirstOrDefault(predicate);
			return result;
		}

		public async Task<T> GetAsync<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
		{
			return await Task<T>.Run(() =>
				{
					return Get(predicate);
				});
		}

		public TContext GetDbContext()
		{
			return Activator.CreateInstance<TContext>();
		}

		public int Insert<T>(T entity) where T : class
		{
			var list = GetOrCreateList<T>();
			if (list.Contains(entity))
			{
				throw new Exception("entity already exists");
			}
			list.Add(entity);
			var pkName = "Id"; // TODO : Use GetPrimaryKeyName from DbContext
			var pik = entity.GetType().GetProperty(pkName, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
			if (pik != null)
			{
				if (pik.PropertyType == typeof(int))
				{
					var id = list.Count();
					pik.SetValue(entity, id, null);
				}
				else if (pik.PropertyType == typeof(string))
				{
					var existingValue = pik.GetValue(entity, null);
					if (existingValue == null)
					{
						pik.SetValue(entity, Guid.NewGuid().ToString(), null);
					}
				}
			}
			var timeStampName = "version";
			var pits = entity.GetType().GetProperty(timeStampName, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
			if (pits != null)
			{
				var timestamp = System.Text.Encoding.Default.GetBytes(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
				pits.SetValue(entity, timestamp, null);
			}
			return 1;

		}

		public async Task<int> InsertAsync<T>(T entity) where T : class
		{
			return await Task<int>.Run(() =>
				{
					return Insert(entity);
				});
		}


		public IQueryable<T> Query<T, TKey>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, System.Linq.Expressions.Expression<Func<T, TKey>> orderBy) where T : class
		{
			var table = GetOrCreateList<T>();
			return table.AsQueryable();
		}

		public IQueryable<T> Query<T, TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> orderBy) where T : class
		{
			var table = GetOrCreateList<T>();
			return table.AsQueryable().OrderBy(orderBy);
		}

		public IQueryable<T> Query<T>() where T : class
		{
			var table = GetOrCreateList<T>();
			return table.AsQueryable();
		}

		public IQueryable<T> Query<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
		{
			var table = GetOrCreateList<T>();
			return table.AsQueryable().Where(predicate);
		}

		public int Update<T>(T entity) where T : class
		{
			var table = GetOrCreateList<T>() as SynchronizedCollection<T>;
			var index = table.IndexOf(entity);
			if (index > -1)
			{
				table[index] = entity;
				var timeStampName = "version";
				var pits = entity.GetType().GetProperty(timeStampName, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
				if (pits != null)
				{
					var timestamp = System.Text.Encoding.Default.GetBytes(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
					pits.SetValue(entity, timestamp, null);
				}
				return 1;
			}
			return 0;
		}

		public async Task<int> UpdateAsync<T>(T entity) where T : class
		{
			return await Task<int>.Run(() =>
				{
					return Update(entity);
				});
		}
		public virtual void BulkInsert<T>(IEnumerable<T> list) where T : class
		{
			foreach (var item in list)
			{
				Insert(item);
			}
		}

		#endregion

		private SynchronizedCollection<T> GetOrCreateList<T>()
		{
			var key = typeof(T).AssemblyQualifiedName;
			SynchronizedCollection<T> qc = null;
			if (!m_Database.ContainsKey(key))
			{
				var query = new List<T>();
				qc = new SynchronizedCollection<T>(query.AsEnumerable());
				m_Database.TryAdd(key, qc);
			}
			else
			{
				qc = m_Database[key] as SynchronizedCollection<T>;
			}
			return qc;
		}
		
		private string GetPrimaryKeyName<T>() where T : class
		{
			var dbSet = GetDbContext().ObjectContext;
			var objectSet = dbSet.CreateObjectSet<T>();
			var element = objectSet.EntitySet.ElementType;
			var pk = element.KeyMembers.First();
			return pk.Name;
		}


	}
}
