using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Data.Entity;

namespace CloudSoft.Repositories.Tests
{
	[TestFixture]
	public class SqlRepositoryTest
	{
		private SqlTestDbContext m_DbContext;
		private SqlRepository<SqlTestDbContext> m_SqlRepository;

		[SetUp]
		public void Setup()
		{
			m_DbContext = new SqlTestDbContext();
			m_SqlRepository = new SqlRepository<SqlTestDbContext>();
			var dbContextFactory = new DbContextFactory<SqlTestDbContext>();
			var initializer = new Repositories.Initializers.SqlSchemaInitializer<SqlTestDbContext>(dbContextFactory);
			initializer.Initialize("_schema_test", null);
		}

		[Test]
		public void Create_Insert()
		{
			var myModel = new MyModel();
			myModel.Name = Guid.NewGuid().ToString();

			m_SqlRepository.Insert(myModel);

			var existingId = myModel.Id;
			Assert.AreNotEqual(existingId, 0);
		}

		[Test]
		public void Create_Insert_Update()
		{
			var myModel = new MyModel();
			myModel.Name = Guid.NewGuid().ToString();

			m_SqlRepository.Insert(myModel);

			var existingId = myModel.Id;
			var existingName = myModel.Name;
			Assert.AreNotEqual(existingId, 0);

			myModel.Name = Guid.NewGuid().ToString();

			m_SqlRepository.Update(myModel);

			Assert.AreEqual(myModel.Id, existingId);
			Assert.AreNotEqual(myModel.Name, existingName);
		}

		[Test]
		public void Create_Insert_Get_Delete()
		{
			var myModel = new MyModel();
			myModel.Name = Guid.NewGuid().ToString();

			m_SqlRepository.Insert(myModel);

			var existingId = myModel.Id;

			var model = m_SqlRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.IsNotNull(model);

			m_SqlRepository.Delete(model);

			model = m_SqlRepository.Get<MyModel>(i => i.Id == existingId);

			Assert.IsNull(model);
		}
	}
}
