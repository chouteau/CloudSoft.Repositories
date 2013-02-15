using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Infrastructure;

namespace CloudSoft.Repositories
{
	public interface IRepositoryInitializer
	{
		void Initialize(string schemaTableName, string scriptNameSpace = null);
	}
}
