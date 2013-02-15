using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations;

namespace CloudSoft.Repositories.Tests
{
	public class MyModelMap : EntityTypeConfiguration<MyModel>
	{
		public MyModelMap ()
		{
			this.HasKey(t => t.Id);

			this.Property(e => e.Id)
				.IsRequired()
				.HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);

			this.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(100);

			ToTable("MyModel");
		}
	}
}
