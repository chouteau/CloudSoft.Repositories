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
			m_SqlRepository.TraceEnabled = true;
		}

		[Test]
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
	}
}
