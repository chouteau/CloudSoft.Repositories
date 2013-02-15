using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CloudSoft.Repositories.Tests
{
	[TestFixture]
	public class MemoryRepositoryTests
	{
		private TestDbContext m_DbContext;
		private MemoryRepository<TestDbContext> m_MRepository;

		[SetUp]
		public void Setup()
		{
			m_DbContext = new TestDbContext();
			m_MRepository = new MemoryRepository<TestDbContext>();
		}

		[Test]
		public void Create_Insert_Get_Delete()
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


	}
}
