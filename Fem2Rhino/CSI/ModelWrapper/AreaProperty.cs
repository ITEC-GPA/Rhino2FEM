using Fem2Rhino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.CSI.ModelWrapper
{
	public class AreaProperty : PlateProperty
	{
		public bool IsNone { get; set; } = false;

		public AreaProperty(bool IsNone = false)
		{
			this.IsNone = IsNone;
		}

		public AreaProperty(string name, Material material)
			: base(name, material)
		{
		}

		public AreaProperty(string name, Material material, double membraneThickness, double bendingThickness)
			: base(name, material, membraneThickness, bendingThickness)
		{
		}
	}
}
