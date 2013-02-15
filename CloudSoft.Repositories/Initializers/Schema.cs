using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudSoft.Repositories.Initializers
{
	internal class Schema
	{
		public int Id { get; set; }
		public DateTime CreationDate { get; set; }
		public string Name { get; set; }
		public string Script { get; set; }
	}
}
