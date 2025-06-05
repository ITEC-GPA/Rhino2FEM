using Fem2Rhino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.CSI.ModelWrapper
{
	public class Frame : Beam
	{
		public List<FramePointLoad> PointLoads { get; set; }
		public List<FrameDistributedLoad> LoadDistributeds { get; set; }
		public FrameProperty Property { get; set; }
		public bool AdvancedLocalAxis { get; set; }
		public double AdvancedLocalAngle { get; set; }
		public InsertionPoints InsertionPoint { get; set; }
		public Releases Release { get; set; }
		public DesignSteelOverwrites DesignSteelOverwrite { get; set; }
		public bool LoadTransfer { get; set; }
		public BeamModifiers BeamModifier { get; set; }
		public BeamPropertyModifiers BeamPropertyModifier { get; set; }
		public AutoMeshAttribute AutoMesh { get; set; }
		public double BeamVolume { get; set; }
		public double BeamWeight { get; set; }
		public double BeamMass { get; set; }

		public Frame(Joint nodeA, Joint nodeB, string id, Guid guid)
			: base(nodeA, nodeB, id, guid)
		{
			Groups = new List<Group>();
			PointLoads = new List<FramePointLoad>();
			LoadDistributeds = new List<FrameDistributedLoad>();
			DesignSteelOverwrite = new DesignSteelOverwrites();
			BeamPropertyModifier = new BeamPropertyModifiers();
			AutoMesh = new AutoMeshAttribute();
			Release = new Releases();
			InsertionPoint = new InsertionPoints();
		}

		public Frame(Joint nodeA, Joint nodeB, string id)
			: this(nodeA, nodeB, id, Guid.NewGuid())
		{
		}


		public class Releases
		{
			public bool[] IEndReleases { get; set; }
			public bool[] JEndReleases { get; set; }
			public double[] IEndPartialFixity { get; set; }
			public double[] JEndPartialFixity { get; set; }

			public string GetReleasesIEndReleases()
			{
				string outString = string.Empty;
				for (int i = 0; i < IEndReleases.Length; i++)
				{
					if (i < IEndReleases.Length - 1)
						outString += IEndReleases[i].ToString() + ";";
					else
						outString += IEndReleases[i].ToString();
				}

				return outString;
			}

			public string GetReleasesJEndReleases()
			{
				string outString = string.Empty;
				for (int i = 0; i < JEndReleases.Length; i++)
				{
					if (i < JEndReleases.Length - 1)
						outString += JEndReleases[i].ToString() + ";";
					else
						outString += JEndReleases[i].ToString();
				}
				return outString;
			}

			public string GetReleasesIEndPartialFixity()
			{
				string outString = string.Empty;
				for (int i = 0; i < IEndPartialFixity.Length; i++)
				{
					if (i < IEndPartialFixity.Length - 1)
						outString += IEndPartialFixity[i].ToString() + ";";
					else
						outString += IEndPartialFixity[i].ToString();
				}

				return outString;
			}

			public string GetReleasesJEndPartialFixity()
			{
				string outString = string.Empty;
				for (int i = 0; i < JEndPartialFixity.Length; i++)
				{
					if (i < JEndPartialFixity.Length - 1)
						outString += JEndPartialFixity[i].ToString() + ";";
					else
						outString += JEndPartialFixity[i].ToString();
				}

				return outString;
			}
		}

		public class InsertionPoints
		{
			public enum CardinalPoints
			{
				None = 0,
				BottomLeft = 1,
				BottomCenter = 2,
				BottomRight = 3,
				MiddleLeft = 4,
				MiddleCenter = 5,
				MiddleRight = 6,
				TopLeft = 7,
				TopCenter = 8,
				TopRight = 9,
				Centroid = 10,
				ShearCenter = 11,
			}

			public CardinalPoints CardinalPoint { get; set; }
			public bool StiffnessTransform { get; set; }
			public bool Mirror2 { get; set; }
			public bool Mirror3 { get; set; }
			public double[] Offset1 { get; set; }
			public double[] Offset2 { get; set; }
			public string CoordinateSystem { get; set; }

			public string GetOffset1()
			{
				string outString = string.Empty;
				for (int i = 0; i < Offset1.Length; i++)
				{
					if (i != Offset1.Length - 1)
						outString += Offset1[i].ToString() + ";";
					else
						outString += Offset1[i].ToString();
				}

				return outString;
			}

			public string GetOffset2()
			{
				string outString = string.Empty;
				for (int i = 0; i < Offset2.Length; i++)
				{
					if (i != Offset2.Length - 1)
						outString += Offset2[i].ToString() + ";";
					else
						outString += Offset2[i].ToString();
				}

				return outString;
			}
		}

		public class DesignSteelOverwrites
		{
			public double UnbracedLengthRatioMajor { get; set; }
			public double UnbracedLengthRatioMinor { get; set; }
			public double UnbracedLengthRatioLateralTorsionalBuckling { get; set; }
			public double EffectiveLengthFactorK1Major { get; set; }
			public double EffectiveLengthFactorK1Minor { get; set; }
			public double EffectiveLengthFactorK2Major { get; set; }
			public double EffectiveLengthFactorK2Minor { get; set; }
			public double EffectiveLengthFactorKLateralTorsionalBuckling { get; set; }
			public double MomentCoefficientCmMajor { get; set; }
			public double MomentCoefficientCmMinor { get; set; }
			public double BendingCoefficientCb { get; set; }
			public double NonswayMomentFactorB1Major { get; set; }
			public double NonswayMomentFactorB1Minor { get; set; }
			public double SwayMomentFactorB2Major { get; set; }
			public double SwayMomentFactorB2Minor { get; set; }
		}

		public class BeamPropertyModifiers
		{
			public bool IsModified { get; set; }
			public double CrossSectionalArea { get; set; }
			public double ShearAreaInLocal2Direction { get; set; }
			public double ShearAreaInLocal3Direction { get; set; }
			public double TorsionalConstant { get; set; }
			public double MomentOfInertiaAboutLocal2Axis { get; set; }
			public double MomentOfInertiaAboutLocal3Axis { get; set; }
			public double Mass { get; set; }
			public double Weight { get; set; }

			public BeamPropertyModifiers()
			{
				CrossSectionalArea = 1;
				ShearAreaInLocal2Direction = 1;
				ShearAreaInLocal3Direction = 1;
				TorsionalConstant = 1;
				MomentOfInertiaAboutLocal2Axis = 1;
				MomentOfInertiaAboutLocal3Axis = 1;
				Mass = 1;
				Weight = 1;
				IsModified = false;
			}

			public override string ToString()
			{
				return CrossSectionalArea.ToString() + ";" + ShearAreaInLocal2Direction.ToString() + ";" + ShearAreaInLocal3Direction.ToString() + ";" +
					TorsionalConstant.ToString() + ";" + MomentOfInertiaAboutLocal2Axis.ToString() + ";" + MomentOfInertiaAboutLocal3Axis.ToString() + ";" +
					Mass.ToString() + ";" + Weight.ToString();
			}
		}

		public class BeamModifiers
		{
			public bool IsModified { get; set; }
			public double CrossSectionalArea { get; set; }
			public double ShearAreaInLocal2Direction { get; set; }
			public double ShearAreaInLocal3Direction { get; set; }
			public double TorsionalConstant { get; set; }
			public double MomentOfInertiaAboutLocal2Axis { get; set; }
			public double MomentOfInertiaAboutLocal3Axis { get; set; }
			public double Mass { get; set; }
			public double Weight { get; set; }

			public BeamModifiers()
			{
				CrossSectionalArea = 1;
				ShearAreaInLocal2Direction = 1;
				ShearAreaInLocal3Direction = 1;
				TorsionalConstant = 1;
				MomentOfInertiaAboutLocal2Axis = 1;
				MomentOfInertiaAboutLocal3Axis = 1;
				Mass = 1;
				Weight = 1;
				IsModified = false;
			}

			public override string ToString()
			{
				return CrossSectionalArea.ToString() + ";" + ShearAreaInLocal2Direction.ToString() + ";" + ShearAreaInLocal3Direction.ToString() + ";" +
					TorsionalConstant.ToString() + ";" + MomentOfInertiaAboutLocal2Axis.ToString() + ";" + MomentOfInertiaAboutLocal3Axis.ToString() + ";" +
					Mass.ToString() + ";" + Weight.ToString();
			}
		}

		public class AutoMeshAttribute
		{
			public bool Active { get; set; }
			public bool AutoMeshAtPoints { get; set; }
			public bool AutoMeshAtLines { get; set; }
			public int MinimumNumberOfSegments { get; set; }
			public double AutoMeshMaxLength { get; set; }

			public AutoMeshAttribute()
			{
				Active = false;
				AutoMeshAtPoints = false;
				AutoMeshAtLines = false;
				MinimumNumberOfSegments = 0;
				AutoMeshMaxLength = 0;
			}

			public override string ToString()
			{
				return Active.ToString() + ";" + AutoMeshAtPoints.ToString() + ";" + AutoMeshAtLines.ToString() + ";" +
					MinimumNumberOfSegments.ToString() + ";" + AutoMeshMaxLength.ToString();
			}
		}
	}
}
