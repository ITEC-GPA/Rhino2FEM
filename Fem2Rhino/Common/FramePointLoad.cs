using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class FramePointLoad : FrameLoad
	{
		public double RelativeDistanceIEnd { get; set; }
		public double DistanceIEnd { get; set; }
		public double Value { get; set; }

		public FramePointLoad(string name)
			: base(name)
		{
		}
	}
}
