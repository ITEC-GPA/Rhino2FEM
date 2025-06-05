using Fem2Rhino.Common;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.CSI.ModelWrapper
{
	public class Area : Plate
	{
		public Joint[] GetPoints => (Joint[])base.Nodes;
		public List<AreaLoadUniform> LoadUniform { get; set; }
		public List<AreaLoadUniformToFrame> AreaLoadUniformToFrames { get; set; }
		public PlateModifiers PlateModifier { get; set; }
		public PlatePropertyModifiers PlatePropertyModifier { get; set; }
		public bool AdvancedLocalAxis { get; set; }
		public double AdvancedLocalAngle { get; set; }
		public AdvanceLocalAxes AdvanceLocalCoordinateSystem { get; set; }

		public Area(string id)
			: base(id, Guid.NewGuid())
		{
			LoadUniform = new List<AreaLoadUniform>();
			AreaLoadUniformToFrames = new List<AreaLoadUniformToFrame>();
		}

		public Area(string id, Guid guid)
			: base(id, guid)
		{
			LoadUniform = new List<AreaLoadUniform>();
			AreaLoadUniformToFrames = new List<AreaLoadUniformToFrame>();
		}

		public Area(string id, Guid guid, Joint[] joints)
			: base(id, guid, joints)
		{
			LoadUniform = new List<AreaLoadUniform>();
			AreaLoadUniformToFrames = new List<AreaLoadUniformToFrame>();
		}

		public class PlateModifiers
		{
			public bool IsModified { get; set; }
			public double MembraneF11 { get; set; }
			public double MembraneF22 { get; set; }
			public double MembraneF12 { get; set; }
			public double BendingM11 { get; set; }
			public double BendingM22 { get; set; }
			public double BendingM12 { get; set; }
			public double ShearV13 { get; set; }
			public double ShearV23 { get; set; }
			public double Mass { get; set; }
			public double Weight { get; set; }

			public PlateModifiers()
			{
				MembraneF11 = 1;
				MembraneF22 = 1;
				MembraneF12 = 1;
				BendingM11 = 1;
				BendingM22 = 1;
				BendingM12 = 1;
				ShearV13 = 1;
				ShearV23 = 1;
				Mass = 1;
				Weight = 1;
				IsModified = false;
			}

			public override string ToString()
			{
				return MembraneF11.ToString() + ";" + MembraneF22.ToString() + ";" + MembraneF12.ToString() + ";" + BendingM11.ToString() + ";" + BendingM22.ToString() + ";" +
					BendingM12.ToString() + ";" + ShearV13.ToString() + ";" + ShearV23.ToString() + ";" + Mass.ToString() + ";" + Weight.ToString();
			}
		}

		public class PlatePropertyModifiers
		{
			public bool IsModified { get; set; }
			public double MembraneF11 { get; set; }
			public double MembraneF22 { get; set; }
			public double MembraneF12 { get; set; }
			public double BendingM11 { get; set; }
			public double BendingM22 { get; set; }
			public double BendingM12 { get; set; }
			public double ShearV13 { get; set; }
			public double ShearV23 { get; set; }
			public double Mass { get; set; }
			public double Weight { get; set; }

			public PlatePropertyModifiers()
			{
				MembraneF11 = 1;
				MembraneF22 = 1;
				MembraneF12 = 1;
				BendingM11 = 1;
				BendingM22 = 1;
				BendingM12 = 1;
				ShearV13 = 1;
				ShearV23 = 1;
				Mass = 1;
				Weight = 1;
				IsModified = false;
			}

			public override string ToString()
			{
				return MembraneF11.ToString() + ";" + MembraneF22.ToString() + ";" + MembraneF12.ToString() + ";" + BendingM11.ToString() + ";" + BendingM22.ToString() + ";" +
					BendingM12.ToString() + ";" + ShearV13.ToString() + ";" + ShearV23.ToString() + ";" + Mass.ToString() + ";" + Weight.ToString();
			}
		}

		public class AdvanceLocalAxes
		{
			public bool IsActive { get; set; }
			public int Plane { get; set; }
			public string Joint1 { get; set; }
			public string Joint2 { get; set; }

			public AdvanceLocalAxes()
			{
				IsActive = false;
				Plane = 31;
				Joint1 = "";
				Joint2 = "";
			}

			public override string ToString()
			{
				return Plane.ToString() + ";" + Joint1.ToString() + ";" + Joint2.ToString();
			}
		}
	}
}