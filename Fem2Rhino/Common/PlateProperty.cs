using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class PlateProperty : Property
	{
		public enum PropertyTypes
		{
			Floor,
			Wall,
			Other,
			Null
		}

		public double MembraneThickness { get; set; }
		public double BendingThickness { get; set; }
		public PropertyTypes PropertyType { get; set; }
		public bool IsOpening { get; set; }

		public PlateProperty()
		{
		}

		public PlateProperty(string name)
			: base(name)
		{
		}

		public PlateProperty(string name, Material material)
			: this(name, material, 0, 0)
		{
		}

		public PlateProperty(string name, Material material, double membraneThickness, double bendingThickness, PropertyTypes propertyType = PropertyTypes.Floor)
			: base(name, material)
		{
			MembraneThickness = membraneThickness;
			BendingThickness = bendingThickness;
			PropertyType = propertyType;
		}
	}
}
