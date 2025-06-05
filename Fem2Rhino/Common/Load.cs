using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class Load
	{
		public string Name { get; set; }
		public string LoadPattern { get; set; }
		public string CoordinateSystem { get; set; }

		public Load(string name)
		{
			Name = name;
		}
	}
}
