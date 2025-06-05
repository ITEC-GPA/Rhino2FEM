using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class NodePointLoad : Load
	{
		public double F1 { get; set; }
		public double F2 { get; set; }
		public double F3 { get; set; }
		public double M1 { get; set; }
		public double M2 { get; set; }
		public double M3 { get; set; }

		public NodePointLoad(string name)
			: base(name)
		{
		}

		public string GetF()
		{
			return F1.ToString() + ";" + F2.ToString() + ";" + F3.ToString();
		}

		public string GetM()
		{
			return M1.ToString() + ";" + M2.ToString() + ";" + M3.ToString();
		}
	}
}
