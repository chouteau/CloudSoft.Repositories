using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CloudSoft.Repositories.Initializers
{
	internal class SchemaDbContext : DbContext
	{
		public SchemaDbContext()
		{

		}

		public SchemaDbContext(string connectionString, DbCompiledModel compiledModel)
			: base(connectionString, compiledModel)
		{
		}

		public DbSet<Schema> Schemas { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			// modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
		}
	}
}
