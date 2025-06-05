using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class Property
	{
		public string Name { get; set; }
		public Material Material { get; set; }

		public Property()
		{

		}
		public Property(string name)
		{
			Name = name;
		}

		public Property(string name, Material material)
			: this(name)
		{
			Material = material;
		}
	}
}
