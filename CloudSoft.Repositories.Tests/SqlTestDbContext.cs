using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CloudSoft.Repositories.Tests
{
	public class SqlTestDbContext : DbContext
	{
		public SqlTestDbContext()
			: base("name=testConnectionString")
		{

		}

		public DbSet<MyModel> MyModels { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
			modelBuilder.Configurations.Add(new MyModelMap());
		}
	}
}
