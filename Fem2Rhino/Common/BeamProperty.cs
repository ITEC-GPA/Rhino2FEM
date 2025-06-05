using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Fem2Rhino.Common
{
	public class BeamProperty : Property
	{
		public Section Section { get; set; }

		public BeamProperty()
		{
		}

		public BeamProperty(string name)
			: base(name)
		{
		}

		public BeamProperty(string name, Material material)
			: base(name, material)
		{
		}

		public BeamProperty(string name, Material material, Section section)
			: base(name, material)
		{
			Section = section;
		}
	}
}
