using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.Objects.DataClasses;

namespace CloudSoft.Repositories
{
	/// <summary>
	/// For tests mocking
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	public class MemoryRepository<TContext> : IRepository<TContext>
		where TContext : System.Data.Entity.DbContext, IObjectContextAdapter, new()
	{
		private Dictionary<string, object> m_Database;

		public MemoryRepository()
		{
			m_Database = new Dictionary<string, object>();
		}

		#region IRepository<TContext> Members

		public int Delete<T>(T entity) where T : class
		{
			var table = GetOrCreateList<T>() as IList<T>;
			if (table.Contains(entity))
			{
				table.Remove(entity);
				return 1;
			}
			return 0;
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
				(table as IList<T>).Remove(item);
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

		public System.Data.Objects.ObjectResult<T> ExecuteStoreQuery<T>(string cmdText, params object[] parameters)
		{
			throw new NotImplementedException();
		}

		public T Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
		{
			var table = GetOrCreateList<T>();
			return table.AsQueryable().FirstOrDefault(predicate);
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
				pik.SetValue(entity, list.Count, null);
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
			var table = GetOrCreateList<T>() as IList<T>;
			var index = table.IndexOf(entity);
			if (index > 0)
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

		#endregion

		private IList<T> GetOrCreateList<T>()
		{
			var key = typeof(T).AssemblyQualifiedName;
			IList<T> list = null;
			if (!m_Database.ContainsKey(key))
			{
				list = new List<T>();
				m_Database.Add(key, list);
			}
			else
			{
				list = m_Database[key] as IList<T>;
			}
			return list;
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
