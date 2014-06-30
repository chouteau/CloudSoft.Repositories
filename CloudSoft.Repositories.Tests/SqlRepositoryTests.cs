using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudSoft.Repositories.Tests
{
	[TestClass]
	public class SqlRepositoryTests
	{
		private TestDbContext m_DbContext;
		private SqlRepository<TestDbContext> m_SqlRepository;

		[TestInitialize]
		public void Setup()
		{
			m_DbContext = new TestDbContext();
			m_SqlRepository = new SqlRepository<TestDbContext>();
			var dbContextFactory = new DbContextFactory<TestDbContext>();
			var initializer = new Repositories.Initializers.SqlSchemaInitializer<TestDbContext>(dbContextFactory);
			initializer.Initialize("_schema_test", null);
			GlobalConfiguration.Logger = Console.WriteLine;
		}

		[TestMethod]
		public void Create_Insert_Get_Delete()
		{
			var model = new MyModel();
			model.Name = Guid.NewGuid().ToString();

			m_SqlRepository.Insert(model);

			var existingId = model.Id;

			model = m_SqlRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.IsNotNull(model);

			var name = Guid.NewGuid().ToString();
			model.Name = name;

			m_SqlRepository.Update(model);

			model = m_SqlRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.AreEqual(name, model.Name);

			m_SqlRepository.Delete(model);

			model = m_SqlRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.IsNull(model);
		}

		[TestMethod]
		public void Create_Insert_Get_Delete_With_Thread()
		{
			var task = System.Threading.Tasks.Task.Run(()=>
				{
					Create_Insert_Get_Delete();
				}).Wait(10 * 1000);
		}


		[TestMethod]
		public async void Create_Insert_Get_Delete_Async()
		{
			var model = new MyModel();
			model.Name = Guid.NewGuid().ToString();

			await m_SqlRepository.InsertAsync(model);

			var existingId = model.Id;

			model = await m_SqlRepository.GetAsync<MyModel>(i => i.Id == existingId);

			Assert.IsNotNull(model);

			var name = Guid.NewGuid().ToString();
			model.Name = name;

			await m_SqlRepository.UpdateAsync(model);

			model = await m_SqlRepository.GetAsync<MyModel>(i => i.Id == existingId);

			Assert.AreEqual(name, model.Name);

			await m_SqlRepository.DeleteAsync(model);

			model = await m_SqlRepository.GetAsync<MyModel>(i => i.Id == existingId);

			Assert.IsNull(model);
		}

	}
}
