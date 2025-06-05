using Fem2Rhino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.CSI.ModelWrapper
{
	public class FrameProperty : BeamProperty
	{
		public enum PropertyTypes
		{
			Column, 
			Beam, 
			Brace, 
			Null, 
			Other,
		}

		public bool IsNone { get; set; } = false;
		public bool IsNonPrismatic { get; set; }
		public double TotalLenght { get; set; }
		public PropertyTypes PropertyType { get; set; }
		public double RelativeDistance { get; set; }
		/// <summary>
		/// The cross-sectional area. [L2]
		/// </summary>
		public double Area { get; set; }
		/// <summary>
		/// The shear area for forces in the section local 2-axis direction. [L2]
		/// </summary>
		public double As2  { get; set; }
		/// <summary>
		/// The shear area for forces in the section local 3-axis direction. [L2]
		/// </summary>
		public double As3  { get; set; }
		/// <summary>
		/// The torsional constant. [L4]
		/// </summary>
		public double Torsion  { get; set; }
		/// <summary>
		/// The moment of inertia for bending about the local 2 axis. [L4]
		/// </summary>
		public double I22  { get; set; }
		/// <summary>
		/// The moment of inertia for bending about the local 3 axis. [L4]
		/// </summary>
		public double I33  { get; set; }
		/// <summary>
		/// The section modulus for bending about the local 2 axis. [L3]
		/// </summary>
		public double S22  { get; set; }
		/// <summary>
		/// The section modulus for bending about the local 3 axis. [L3]
		/// </summary>
		public double S33  { get; set; }
		/// <summary>
		/// The plastic modulus for bending about the local 2 axis. [L3]
		/// </summary>
		public double Z22  { get; set; }
		/// <summary>
		/// The plastic modulus for bending about the local 3 axis. [L3]
		/// </summary>
		public double Z33  { get; set; }
		/// <summary>
		/// The radius of gyration about the local 2 axis. [L]
		/// </summary>
		public double R22  { get; set; }
		/// <summary>
		/// The radius of gyration about the local 3 axis. [L]
		/// </summary>
		public double R33 { get; set; }
		public string SectionType { get; set; }

		public FrameProperty(bool isNone = false)
		{
			IsNone = isNone;
			IsNonPrismatic = false;
			PropertyType = PropertyTypes.Other;
			Area = 0;
			As2 = 0;
			As3 = 0;
			Torsion = 0;
			I22 = 0;
			I33 = 0;
			S22 = 0;
			S33 = 0;
			Z22 = 0;
			Z33 = 0;
			R22 = 0;
			R33 = 0;
			SectionType = "";
		}

		public FrameProperty(bool isNonPrismatic, double totalLenght, double relativeDistance, bool isNone = false)
		{
			IsNone = isNone;
			IsNonPrismatic = isNonPrismatic;
			TotalLenght = totalLenght;
			RelativeDistance = relativeDistance;
			PropertyType = PropertyTypes.Other;
			Area = 0;
			As2 = 0;
			As3 = 0;
			Torsion = 0;
			I22 = 0;
			I33 = 0;
			S22 = 0;
			S33 = 0;
			Z22 = 0;
			Z33 = 0;
			R22 = 0;
			R33 = 0;
			SectionType = "";
		}

		public FrameProperty(string name, Material material)
			: base(name, material)
		{
			PropertyType = PropertyTypes.Other;
			Area = 0;
			As2 = 0;
			As3 = 0;
			Torsion = 0;
			I22 = 0;
			I33 = 0;
			S22 = 0;
			S33 = 0;
			Z22 = 0;
			Z33 = 0;
			R22 = 0;
			R33 = 0;
			SectionType = "";
		}
	}
}
