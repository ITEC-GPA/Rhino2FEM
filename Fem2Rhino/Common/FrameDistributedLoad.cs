using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class FrameDistributedLoad : FrameLoad
	{
		public double RelativeDistanceIEnd { get; set; }
		public double RelativeDistanceJEnd { get; set; }
		public double DistanceIEnd { get; set; }
		public double DistanceJEnd { get; set; }
		public double ValueStart { get; set; }
		public double ValueEnd { get; set; }

		public FrameDistributedLoad(string name)
			: base(name)
		{
		}
	}
}
