using Fem2Rhino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.CSI.ModelWrapper
{
	public class AreaLoadUniformToFrame : AreaLoad
	{
		public enum DistributionTypes
		{
			OneWay = 1,
			TwoWay = 2,
		}

		public DistributionTypes DistributionType { get; set; }

		public AreaLoadUniformToFrame(string name)
			: base(name)
		{
		}
	}
}
