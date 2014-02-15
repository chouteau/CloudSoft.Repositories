using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSoft.Repositories
{
	public class GlobalConfiguration
	{
		public static Action<string> Logger { get; set; }
	}
}
