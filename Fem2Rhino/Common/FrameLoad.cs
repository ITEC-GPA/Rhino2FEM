using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class FrameLoad : Load
	{
		public enum Types
		{
			Force = 1,
			Moment = 2,
		}

		public enum Directions
		{
			Local1Axis = 1,
			Local2Axis = 2,
			Local3Axis = 3,
			XDirection = 4,
			YDirection = 5,
			ZDirection = 6,
			ProjectedXDirection = 7,
			ProjectedYDirection = 8,
			ProjectedZDirection = 9,
			GravityDirection = 10,
			ProjectedGravityDirection = 11,
		}

		public Types Type { get; set; }
		public Directions Direction { get; set; }

		public FrameLoad(string name)
			: base(name)
		{
		}
	}
}
