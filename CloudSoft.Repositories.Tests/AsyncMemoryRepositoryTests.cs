using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CloudSoft.Repositories.Tests
{
	[TestClass]
	public class AsyncMemoryRepositoryTests
	{
		private TestDbContext m_DbContext;
		private MemoryRepository<TestDbContext> m_MRepository;

		[TestInitialize]
		public void Setup()
		{
			m_DbContext = new TestDbContext();
			m_MRepository = new MemoryRepository<TestDbContext>();
		}

		[TestMethod]
		public async Task CRUD()
		{
			var model = new MyModel();
			model.Name = Guid.NewGuid().ToString();

			await m_MRepository.InsertAsync(model);

			var existingId = model.Id;

			model = await m_MRepository.GetAsync<MyModel>(i => i.Id == existingId);

			Assert.IsNotNull(model);

			var name = Guid.NewGuid().ToString();
			model.Name = name;

			await m_MRepository.UpdateAsync(model);

			model = await m_MRepository.GetAsync<MyModel>(i => i.Id == existingId);

			Assert.AreEqual(name, model.Name);

			await m_MRepository.DeleteAsync(model);

			model = await m_MRepository.GetAsync<MyModel>(i => i.Id == existingId);

			Assert.IsNull(model);
		}

		[TestMethod]
		public async Task Async_List()
		{
			for (int i = 0; i < 10; i++)
			{
				var model = new MyModel();
				model.Name = Guid.NewGuid().ToString();
				await m_MRepository.InsertAsync(model);
			}

			var query = m_MRepository.Query<MyModel>();
			var result = await query.Take(5).ToListAsync();

			Assert.AreEqual(5, result.Count);
		}

	}
}
