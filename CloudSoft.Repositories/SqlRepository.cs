using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace CloudSoft.Repositories
{
	public class SqlRepository<TContext> : IDisposable, IRepository<TContext>
		where TContext : System.Data.Entity.DbContext, IObjectContextAdapter, new()
	{
		private IDbContextFactory<TContext> m_dbContextFactory;

		public SqlRepository()
			: this(new DbContextFactory<TContext>())
		{
			TraceEnabled = false;	 
		}

		public bool TraceEnabled { get; set; }

		public SqlRepository(IDbContextFactory<TContext> factory)
		{
			m_dbContextFactory = factory;
		}

		public virtual TContext GetDbContext()
		{
			return m_dbContextFactory.GetDbContext();
		}

		public virtual T Get<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			if (predicate == null)
			{
				throw new ArgumentException("predicate does not be null.");
			}
			var dbContext = GetDbContext();
			var query = dbContext.Set<T>().Where(predicate); 
			//if (TraceEnabled)
			//{
			//	System.Diagnostics.Debug.WriteLine(query.ToTraceString());
			//}
			var result = query.FirstOrDefault();
			return result;
		}

		public virtual IQueryable<T> Query<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			var dbContext = GetDbContext();
			var query = dbContext.Set<T>().Where(predicate);
			//if (TraceEnabled)
			//{
			//	System.Diagnostics.Debug.WriteLine(query.ToTraceString());
			//}
			return query;
		}

		public virtual IQueryable<T> Query<T, TKey>(Expression<Func<T, bool>> predicate,
			Expression<Func<T, TKey>> orderBy)  where T : class
		{
			var query = Query(predicate).OrderBy(orderBy);
			if (TraceEnabled)
			{
				System.Diagnostics.Debug.WriteLine(query.ToString());
			}
			return query;
		}

		public virtual IQueryable<T> Query<T, TKey>(Expression<Func<T, TKey>> orderBy) where T : class
		{
			var query = Query<T>().OrderBy(orderBy);
			if (TraceEnabled)
			{
				System.Diagnostics.Debug.WriteLine(query.ToString());
			}
			return query;
		}

		public virtual IQueryable<T> Query<T>() where T : class
		{
			var dbContext = GetDbContext();
			return dbContext.Set<T>();
		}

		public virtual int Insert<T>(T entity) where T : class
		{
			int result = 0;
			var dbContext = GetDbContext();
			try
			{
				var dbSet = dbContext.Set<T>();
				var entry = dbContext.Entry(entity);
				if (entry.State == EntityState.Detached)
				{
					dbSet.Attach(entity);
				}
				dbSet.Add(entity);
				dbContext.ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Added);

				//if (TraceEnabled)
				//{
				//	string sql = dbContext.ObjectContext.ToTraceString();
				//	System.Diagnostics.Debug.WriteLine(sql);
				//}

				result = dbContext.SaveChanges();
			}
			catch (System.Data.Entity.Validation.DbEntityValidationException vex)
			{
				int errorId = 0;
				foreach (var item in vex.EntityValidationErrors)
				{
					foreach (var error in item.ValidationErrors)
					{
						errorId++;
						string key = string.Format("{0}{1}", error.PropertyName, errorId);
						vex.Data.Add(key, error.ErrorMessage);
					}
				}
				throw;
			}
			catch (Exception exp)
			{
				// string sql = dbContext.ObjectContext.ToTraceString();
				// exp.Data.Add("SqlRepository:SqlScript", sql);
				exp.Data.Add("SqlRepository:Insert:Entity", entity.ToString());
				throw;
			}

			return result;
		}

		public virtual void BulkInsert<T>(IEnumerable<T> list) where T : class
		{
			var dbContext = GetDbContext();
			var transactionOptions = new System.Transactions.TransactionOptions
			{
				IsolationLevel = System.Transactions.IsolationLevel.Serializable,
				Timeout = TimeSpan.FromSeconds(0)
			};
			try
			{
				using (var transactionScope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.RequiresNew, transactionOptions))
				{
					var dbSet = dbContext.Set<T>();
					foreach (var entity in list)
					{
						if (dbContext.Entry(entity).State == EntityState.Detached)
						{
							dbSet.Attach(entity);
						}
						dbSet.Add(entity);
						dbContext.ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Added);
					}

					//if (TraceEnabled)
					//{
					//	string sql = dbContext.ObjectContext.ToTraceString();
					//	System.Diagnostics.Debug.WriteLine(sql);
					//}

					dbContext.SaveChanges();
					transactionScope.Complete();
				}
			}
			catch (System.Data.Entity.Validation.DbEntityValidationException vex)
			{
				int errorId = 0;
				foreach (var item in vex.EntityValidationErrors)
				{
					foreach (var error in item.ValidationErrors)
					{
						errorId++;
						string key = string.Format("{0}{1}", error.PropertyName, errorId);
						vex.Data.Add(key, error.ErrorMessage);
					}
				}
				throw;
			}
			catch (Exception exp)
			{
				// string sql = dbContext.ObjectContext.ToTraceString();
				// exp.Data.Add("SqlRepository:SqlScript", sql);
				throw;
			}
		}

		public virtual int Update<T>(T entity) where T : class
		{
			int result = 0;
			var dbContext = GetDbContext();
			try
			{
				if (dbContext.Entry(entity).State == EntityState.Detached)
				{
					dbContext.Set<T>().Attach(entity);
				}
				dbContext.ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);

				//if (TraceEnabled)
				//{
				//	string sql = dbContext.ObjectContext.ToTraceString();
				//	System.Diagnostics.Debug.WriteLine(sql);
				//}

				result = dbContext.SaveChanges();
			}
			catch (Exception exp)
			{
				// string sql = dbContext.ObjectContext.ToTraceString();
				// exp.Data.Add("SqlRepository:SqlScript", sql);
				exp.Data.Add("SqlRepository:Update:Entity", entity.ToString());
				throw;
			}

			return result;
		}

		public virtual int Delete<T>(T entity) where T : class
		{
			int result = 0;
			var dbContext = GetDbContext();
			var loop = 0;
retry:
			try
			{
				if (dbContext.Entry(entity).State == EntityState.Detached)
				{
					dbContext.Set<T>().Attach(entity);
				}
				dbContext.ObjectContext.DeleteObject(entity);

				//if (TraceEnabled)
				//{
				//	string sql = dbContext.ObjectContext.ToTraceString();
				//	System.Diagnostics.Debug.WriteLine(sql);
				//}

				result = dbContext.SaveChanges();
			}
			catch (DbUpdateConcurrencyException ex)
			{
				dbContext.ObjectContext.Refresh(RefreshMode.StoreWins, entity);
				loop++;
				if (loop < 2)
				{
					System.Threading.Thread.Sleep(100);
					goto retry;
				}
			}
			catch (Exception exp)
			{
				// string sql = dbContext.ObjectContext.ToTraceString();
				// exp.Data.Add("SqlRepository:SqlScript", sql);
				exp.Data.Add("SqlRepository:Delete:Entity", entity.ToString());
				throw;
			}

			return result;
		}

		public virtual int DeleteAll<T>(Expression<Func<T, bool>> predicate) where T : class
		{
			int result = 0;
			var dbContext = GetDbContext();

			var query = Query<T>(predicate);
			foreach (var entity in query)
			{
				var loop = 0;
			retry:
				try
				{
					if (dbContext.Entry(entity).State == EntityState.Detached)
					{
						dbContext.Set<T>().Attach(entity);
					}
					dbContext.ObjectContext.DeleteObject(entity);
					//if (TraceEnabled)
					//{
					//	string sql = dbContext.ObjectContext.ToTraceString();
					//	System.Diagnostics.Debug.WriteLine(sql);
					//}
					result = dbContext.SaveChanges();
				}
				catch (DbUpdateConcurrencyException)
				{
					dbContext.ObjectContext.Refresh(RefreshMode.StoreWins, entity);
					loop++;
					if (loop < 2)
					{
						System.Threading.Thread.Sleep(100);
						goto retry;
					}
				}
				catch (Exception exp)
				{
					// string sql = dbContext.ObjectContext.ToTraceString();
					// exp.Data.Add("SqlRepository:SqlScript", sql);
					exp.Data.Add("SqlRepository:Delete:Entity", entity.ToString());
					throw;
				}
			}

			return result;
		}


		public int ExecuteStoreCommand(string cmdText, params object[] parameters)
		{
			int result = 0;
			try
			{
				var dbContext = GetDbContext();
				if (TraceEnabled)
				{
					System.Diagnostics.Debug.WriteLine(cmdText);
				}
				result = dbContext.Database.ExecuteSqlCommand(cmdText, parameters);
			}
			catch (Exception exp)
			{
				exp.Data.Add("SqlRepository:ExecuteStoreCommand:script", cmdText);
				if (parameters != null)
				{
					for (int i = 0; i < parameters.Length; i++)
					{
						exp.Data.Add(string.Format("SqlRepository:ExecuteStoreCommand:script:P{0}", i), parameters[i]);
					}
				}
				throw;
			}
			return result;
		}

		public ObjectResult<T> ExecuteStoreQuery<T>(string cmdText, params object[] parameters)
		{
			var dbContext = GetDbContext();
			if (TraceEnabled)
			{
				System.Diagnostics.Debug.WriteLine(cmdText);
			}
			var result = dbContext.ObjectContext.ExecuteStoreQuery<T>(cmdText, parameters);
			return result;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (m_dbContextFactory != null)
			{
				m_dbContextFactory.Dispose();
			}
		}

		#endregion
	}
}
