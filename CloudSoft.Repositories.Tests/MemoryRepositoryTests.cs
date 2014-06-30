using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudSoft.Repositories.Tests
{
	[TestClass]
	public class MemoryRepositoryTests
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
		public void CRUD()
		{
			var model = new MyModel();
			model.Name = Guid.NewGuid().ToString();

			m_MRepository.Insert(model);

			var existingId = model.Id;

			model = m_MRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.IsNotNull(model);

			var name = Guid.NewGuid().ToString();
			model.Name = name;

			m_MRepository.Update(model);

			model = m_MRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.AreEqual(name, model.Name);

			m_MRepository.Delete(model);

			model = m_MRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.IsNull(model);
		}

		[TestMethod]
		public void Bulk_Insert()
		{
			var bulk = new List<MyModel>();
			for (int i = 0; i < 100; i++)
			{
				var model = new MyModel();
				model.Name = Guid.NewGuid().ToString();
				bulk.Add(model);
			}

			m_MRepository.BulkInsert(bulk);

			Assert.AreEqual(bulk.Last().Id, 100);
		}


	}
}
