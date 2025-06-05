using Fem2Rhino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class Joint : Node
	{
		public List<Group> Groups { get; set; }
		public List<NodePointLoad> PointLoads { get; set; }
		public bool SpecialJoint { get; set; }
		public bool[] Restraints { get; set; }
		public bool U1 => Restraints[0];
		public bool U2 => Restraints[1];
		public bool U3 => Restraints[2];
		public bool R1 => Restraints[3];
		public bool R2 => Restraints[4];
		public bool R3 => Restraints[5];

		public Joint(Node n)
			: base(n)
		{
			PointLoads = new List<NodePointLoad>();
			Groups = new List<Group>();
		}

		public Joint(Joint j)
			: base(j)
		{
			PointLoads = new List<NodePointLoad>();
		}

		public Joint(double x, double y, double z)
			: this(x, y, z, "-1")
		{
		}

		public Joint(double x, double y, double z, string id)
			: this(x, y, z, id, Guid.NewGuid())
		{
		}

		public Joint(double x, double y, double z, string id, Guid guid)
			: base(x, y, z, id, guid)
		{
			PointLoads = new List<NodePointLoad>();
			Groups = new List<Group>();
		}

		public string GetRestraintsTraslation()
		{
			return U1.ToString() + ";" + U2.ToString() + ";" + U3.ToString();
		}

		public string GetRestraintsRotation()
		{
			return R1.ToString() + ";" + R2.ToString() + ";" + R3.ToString();
		}
	}
}
