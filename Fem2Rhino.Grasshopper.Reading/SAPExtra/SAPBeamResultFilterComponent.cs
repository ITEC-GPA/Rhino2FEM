//using FeMM.Common.Helpers;
//using FeMM.Common.Models;
//using FeMM.Grasshopper.DataTypes.FeMM;
//using FeMM.Grasshopper.Helpers;
//using Grasshopper.Kernel;
//using Rhino.Geometry;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;

//namespace FeMM.Grasshopper.Components.SAPExtra
//{
//	public class SAPBeamResultFilterComponent : GH_Component
//	{
//		/// <summary>
//		/// Initializes a new instance of the BeamResultFilterComponent class.
//		/// </summary>
//		public SAPBeamResultFilterComponent()
//		  : base("Beams Result filter", "BRF", "Filter results of a beam", CategoryNameConstants.CATEGORY_CHECKS, CategoryNameConstants.SUBCATEGORY_SAPEXTRA)
//		{
//		}

//		/// <summary>
//		/// Registers all the input parameters for this component.
//		/// </summary>
//		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Beams", "Bs", "Beams with results to filter", GH_ParamAccess.list);
//			pManager.AddBooleanParameter("Sap?", "S?", "Use SAP2000 local axis", GH_ParamAccess.item);
//			pManager.AddBooleanParameter("OnlyEnds", "OnlyEnds", "Filter only ends values", GH_ParamAccess.item, false);
//		}

//		/// <summary>
//		/// Registers all the output parameters for this component.
//		/// </summary>
//		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Filtered beams", "FBs", "Beams with results filtered", GH_ParamAccess.list);
//		}

//		/// <summary>
//		/// This is the method that actually does the work.
//		/// </summary>
//		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//		protected override void SolveInstance(IGH_DataAccess DA)
//		{
//			List<GH_Beam> beams = new List<GH_Beam>();
//			bool sapSdr = false;
//			bool onlyEnds = true;
//			if (!DA.GetDataList(0, beams))
//				return;
//			if (!DA.GetData(1, ref sapSdr))
//				return;
//			if (!DA.GetData(2, ref onlyEnds))
//				return;

//			try
//			{
//				//building stations
//				Dictionary<GH_Beam, List<double>> stations = new Dictionary<GH_Beam, List<double>>();
//				BeamPropertyModel bpm = null;

//				Dictionary<GH_Beam, double> iniStations = new Dictionary<GH_Beam, double>();
//				Dictionary<GH_Beam, double> endStations = new Dictionary<GH_Beam, double>();
//				Dictionary<GH_Beam, double> middleStations = new Dictionary<GH_Beam, double>();

//				List<BeamResult> iniRess = new List<BeamResult>();
//				List<BeamResult> endRess = new List<BeamResult>();
//				List<BeamResult> allRess = new List<BeamResult>();

//				for (int i = 0; i < beams.Count; i++)
//				{
//					GH_Beam b = beams[i];
//					double iniSec = double.MaxValue;
//					double endSec = double.MinValue;

//					for (int j = 0; j < b.Value.Results.Count; j++)
//					{
//						ResultModel rm = b.Value.Results[j];
//						if (rm is BeamForceResultModel bfrm)
//						{
//							if (bfrm.Station > endSec)
//							{
//								endSec = bfrm.Station;
//							}
//							if (bfrm.Station < iniSec)
//							{
//								iniSec = bfrm.Station;
//							}
//						}
//					}

//					iniStations.Add(b, iniSec);
//					endStations.Add(b, endSec);
//					middleStations.Add(b, (iniSec + endSec) * 0.5);

//					if (bpm == null)
//					{
//						bpm = b.Value.BeamProperty;
//					}
//					else
//					{
//						if (bpm.Name != b.Value.BeamProperty.Name)
//						{
//							throw new NotSupportedException("Beam with different properties not admitted");
//						}
//					}
//					for (int j = 0; j < b.Value.Results.Count; j++)
//					{
//						ResultModel rm = b.Value.Results[j];
//						if (rm is BeamForceResultModel bfrm)
//						{
//							if (EqualsStations(bfrm.Station, iniSec))
//							{
//								iniRess.Add(new BeamResult(b, bfrm));
//							}
//							else if (EqualsStations(bfrm.Station, endSec))
//							{
//								endRess.Add(new BeamResult(b, bfrm));
//							}
//							allRess.Add(new BeamResult(b, bfrm));
//						}
//					}
//				}

//				List<Point2d> sectionPoints = GetSectionPoints(bpm);

//				//inisec
//				GetWorstResults(iniRess, bpm, sectionPoints, sapSdr,
//					out BeamResult iniWorstMxPos, out BeamResult iniWorstMxNeg,
//					out BeamResult iniWorstMyPos, out BeamResult iniWorstMyNeg,
//					out BeamResult iniWorstNPos, out BeamResult iniWorstNNeg,
//					out BeamResult iniWorstVxPos, out BeamResult iniWorstVxNeg,
//					out BeamResult iniWorstVyPos, out BeamResult iniWorstVyNeg,
//					out BeamResult iniWorstTPos, out BeamResult iniWorstTNeg,
//					out BeamResult iniWorstSigmaMin, out BeamResult iniWorstSigmaMax);

//				//endsec
//				GetWorstResults(endRess, bpm, sectionPoints, sapSdr,
//					out BeamResult endWorstMxPos, out BeamResult endWorstMxNeg,
//					out BeamResult endWorstMyPos, out BeamResult endWorstMyNeg,
//					out BeamResult endWorstNPos, out BeamResult endWorstNNeg,
//					out BeamResult endWorstVxPos, out BeamResult endWorstVxNeg,
//					out BeamResult endWorstVyPos, out BeamResult endWorstVyNeg,
//					out BeamResult endWorstTPos, out BeamResult endWorstTNeg,
//					out BeamResult endWorstSigmaMin, out BeamResult endWorstSigmaMax);

//				//allRes
//				GetWorstResults(allRess, bpm, sectionPoints, sapSdr,
//					out BeamResult allWorstMxPos, out BeamResult allWorstMxNeg,
//					out BeamResult allWorstMyPos, out BeamResult allWorstMyNeg,
//					out BeamResult allWorstNPos, out BeamResult allWorstNNeg,
//					out BeamResult allWorstVxPos, out BeamResult allWorstVxNeg,
//					out BeamResult allWorstVyPos, out BeamResult allWorstVyNeg,
//					out BeamResult allWorstTPos, out BeamResult allWorstTNeg,
//					out BeamResult allWorstSigmaMin, out BeamResult allWorstSigmaMax);

//				//assigning worst results to first and last stations, ricalculating station pos
//				List<GH_Beam> result = new List<GH_Beam>(beams.Count);
//				BeamResult[] iniRes = new BeamResult[]{ iniWorstVxPos, iniWorstVxNeg,iniWorstMxPos,iniWorstMxNeg, iniWorstVyPos, iniWorstVyNeg,iniWorstMyPos, iniWorstMyNeg,
//					iniWorstNPos, iniWorstNNeg,iniWorstTPos,iniWorstTNeg, iniWorstSigmaMin, iniWorstSigmaMax};
//				BeamResult[] allRes = new BeamResult[]{ allWorstVxPos, allWorstVxNeg,allWorstMxPos,allWorstMxNeg, allWorstVyPos, allWorstVyNeg,allWorstMyPos, allWorstMyNeg,
//					allWorstNPos, allWorstNNeg,allWorstTPos,allWorstTNeg, allWorstSigmaMin, allWorstSigmaMax};
//				BeamResult[] endRes = new BeamResult[] { endWorstVxPos, endWorstVxNeg,endWorstMxPos,endWorstMxNeg, endWorstVyPos, endWorstVyNeg,endWorstMyPos, endWorstMyNeg,
//					endWorstNPos, endWorstNNeg,endWorstTPos,endWorstTNeg, endWorstSigmaMin, endWorstSigmaMax};

//				for (int i = 0; i < beams.Count; i++)
//				{
//					GH_Beam beam = beams[i];
//					GH_Beam clone = new GH_Beam(beam);

//					double iniSec = iniStations[beam];
//					double endSec = endStations[beam];
//					double middleSec = middleStations[beam];
//					clone.Value.Results.Clear();
//					for (int j = 0; j < iniRes.Length; j++)
//					{
//						BeamResult br = iniRes[j];
//						if (beam == br.Beam)
//						{
//							//result on this beam.
//							BeamForceResultModel cloneRes = new BeamForceResultModel(br.Result.LoadCase, iniSec, br.Result.StepNum, br.Result.StepType)
//							{
//								AxialForce = br.Result.AxialForce,
//								BendingMoment = br.Result.BendingMoment,
//								ShearForce = br.Result.ShearForce,
//								Torque = br.Result.Torque
//							};
//							clone.Value.Results.Add(cloneRes);
//						}
//					}
//					if (onlyEnds == false)
//					{
//						for (int j = 0; j < allRes.Length; j++)
//						{
//							BeamResult br = allRes[j];
//							if (beam == br.Beam)
//							{
//								//result on this beam.
//								BeamForceResultModel cloneRes = new BeamForceResultModel(br.Result.LoadCase, middleSec, br.Result.StepNum, br.Result.StepType)
//								{
//									AxialForce = br.Result.AxialForce,
//									BendingMoment = br.Result.BendingMoment,
//									ShearForce = br.Result.ShearForce,
//									Torque = br.Result.Torque
//								};
//								clone.Value.Results.Add(cloneRes);
//							}
//						}
//					}
//					for (int j = 0; j < endRes.Length; j++)
//					{
//						BeamResult br = endRes[j];
//						if (beam == br.Beam)
//						{
//							//result on this beam.
//							BeamForceResultModel cloneRes = new BeamForceResultModel(br.Result.LoadCase, endSec, br.Result.StepNum, br.Result.StepType)
//							{
//								AxialForce = br.Result.AxialForce,
//								BendingMoment = br.Result.BendingMoment,
//								ShearForce = br.Result.ShearForce,
//								Torque = br.Result.Torque
//							};
//							clone.Value.Results.Add(cloneRes);
//						}
//					}
//					result.Add(clone);
//				}

//				DA.SetDataList(0, result);
//			}
//			catch (Exception ex)
//			{
//				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
//				return;
//			}
//		}

//		public static bool EqualsStations(double station, double otherStation)
//		{
//			const double TOLL = 1e-6;
//			return Math.Abs(otherStation - station) < TOLL;
//		}

//		public static void GetWorstResults(List<BeamResult> results, BeamPropertyModel bpm, List<Point2d> sectionPoints, bool sapSdr, out BeamResult worstMxPos,
//			out BeamResult worstMxNeg, out BeamResult worstMyPos, out BeamResult worstMyNeg, out BeamResult worstNPos, out BeamResult worstNNeg,
//			out BeamResult worstVxPos, out BeamResult worstVxNeg, out BeamResult worstVyPos, out BeamResult worstVyNeg, out BeamResult worstTPos,
//			out BeamResult worstTNeg, out BeamResult worstSigmaMin, out BeamResult worstSigmaMax)
//		{
//			worstMxPos = null;
//			worstMxNeg = null;
//			worstMyPos = null;
//			worstMyNeg = null;
//			worstNPos = null;
//			worstNNeg = null;
//			worstVxPos = null;
//			worstVxNeg = null;
//			worstVyPos = null;
//			worstVyNeg = null;
//			worstTPos = null;
//			worstTNeg = null;
//			worstSigmaMin = null;
//			worstSigmaMax = null;
//			double sigmaMin = double.MaxValue;
//			double sigmaMax = double.MinValue;
//			double mxMax = double.MinValue;
//			double mxMin = double.MaxValue;
//			double myMax = double.MinValue;
//			double myMin = double.MaxValue;
//			double vxMax = double.MinValue;
//			double vxMin = double.MaxValue;
//			double vyMax = double.MinValue;
//			double vyMin = double.MaxValue;

//			for (int i = 0; i < results.Count; i++)
//			{
//				BeamResult bfrm = results[i];
//				double mx, my, vy, vx;
//				mx = bfrm.Result.BendingMoment.X;
//				my = bfrm.Result.BendingMoment.Y;
//				vx = bfrm.Result.ShearForce.X;
//				vy = bfrm.Result.ShearForce.Y;

//				if (sapSdr)
//					ExtraSapHelper.ConvertStressesFromSAP2000(bpm, ref mx, ref my, ref vx, ref vy);

//				if ((worstMxPos == null) || (mxMax < mx))
//				{
//					mxMax = mx;
//					worstMxPos = bfrm;
//				}
//				if ((worstMxNeg == null) || (mxMin > mx))
//				{
//					mxMin = mx;
//					worstMxNeg = bfrm;
//				}
//				if ((worstMyPos == null) || (myMax < my))
//				{
//					myMax = my;
//					worstMyPos = bfrm;
//				}
//				if ((worstMyNeg == null) || (myMin > my))
//				{
//					myMin = my;
//					worstMyNeg = bfrm;
//				}
//				if ((worstNPos == null) || (worstNPos.Result.AxialForce < bfrm.Result.AxialForce))
//				{
//					worstNPos = bfrm;
//				}
//				if ((worstNNeg == null) || (worstNNeg.Result.AxialForce > bfrm.Result.AxialForce))
//				{
//					worstNNeg = bfrm;
//				}
//				if ((worstVxPos == null) || (vxMax < vx))
//				{
//					vxMax = vx;
//					worstVxPos = bfrm;
//				}
//				if ((worstVxNeg == null) || (vxMin > vx))
//				{
//					vxMin = vx;
//					worstVxNeg = bfrm;
//				}
//				if ((worstVyPos == null) || (vyMax < vy))
//				{
//					vyMax = vy;
//					worstVyPos = bfrm;
//				}
//				if ((worstVyNeg == null) || (vyMin > vy))
//				{
//					vyMin = vy;
//					worstVyNeg = bfrm;
//				}
//				if ((worstTPos == null) || (worstTPos.Result.Torque < bfrm.Result.Torque))
//				{
//					worstTPos = bfrm;
//				}
//				if ((worstTNeg == null) || (worstTNeg.Result.Torque > bfrm.Result.Torque))
//				{
//					worstTNeg = bfrm;
//				}

//				GetSigmas(bfrm.Result, bpm, sectionPoints, out double minSigma, out double maxSigma);

//				if ((worstSigmaMin == null) || (minSigma < sigmaMin))
//				{
//					worstSigmaMin = bfrm;
//					sigmaMin = minSigma;
//				}
//				if ((worstSigmaMax == null) || (maxSigma > sigmaMax))
//				{
//					worstSigmaMax = bfrm;
//					sigmaMax = maxSigma;
//				}
//			}
//		}

//		public static List<Point2d> GetSectionPoints(BeamPropertyModel bpm)
//		{
//			List<Point2d> sectionPoints;
//			switch (bpm.SectionType)
//			{
//				case SectionModel.SectionTypes.HollowCircle:
//				case SectionModel.SectionTypes.SolidCircle:
//					return null;
//				case SectionModel.SectionTypes.SolidRectangle:
//				case SectionModel.SectionTypes.HollowRectangle:
//					if (bpm.Mirror != SectionModel.MirrorTypes.None) throw new NotImplementedException("Mirror not implemented for this section");
//					sectionPoints = new List<Point2d>
//					{
//						new Point2d(-bpm.B * 0.5, -bpm.D * 0.5),
//						new Point2d(bpm.B * 0.5, -bpm.D * 0.5),
//						new Point2d(bpm.B * 0.5, bpm.D * 0.5),
//						new Point2d(-bpm.B * 0.5, bpm.D * 0.5)
//					};
//					break;
//				case SectionModel.SectionTypes.I:
//					if (bpm.Mirror != SectionModel.MirrorTypes.None) throw new NotImplementedException("Mirror not implemented for this section");
//					sectionPoints = new List<Point2d>
//					{
//						new Point2d(-bpm.B1 * 0.5, -bpm.Centroid.Y),
//						new Point2d(bpm.B1 * 0.5, -bpm.Centroid.Y),
//						new Point2d(bpm.B2 * 0.5, -bpm.Centroid.Y + bpm.D),
//						new Point2d(-bpm.B2 * 0.5, -bpm.Centroid.Y + bpm.D)
//					};
//					break;
//				case SectionModel.SectionTypes.T:
//					if (bpm.Mirror != SectionModel.MirrorTypes.None) throw new NotImplementedException("Mirror not implemented for this section");
//					sectionPoints = new List<Point2d>
//					{
//						new Point2d(-bpm.T2 * 0.5, -bpm.Centroid.Y),
//						new Point2d(bpm.T2 * 0.5, -bpm.Centroid.Y),
//						new Point2d(bpm.B * 0.5, -bpm.Centroid.Y + bpm.D),
//						new Point2d(-bpm.B * 0.5, -bpm.Centroid.Y + bpm.D),
//						new Point2d(bpm.B * 0.5, -bpm.Centroid.Y + bpm.D - bpm.T1),
//						new Point2d(-bpm.B * 0.5, -bpm.Centroid.Y + bpm.D - bpm.T1)
//					};
//					break;
//				case SectionModel.SectionTypes.C:
//					if (bpm.Mirror == SectionModel.MirrorTypes.None)
//					{
//						sectionPoints = new List<Point2d>
//						{
//							new Point2d(-bpm.B * 0.5, -bpm.D * 0.5),
//							new Point2d(bpm.B * 0.5, -bpm.D * 0.5),
//							new Point2d(bpm.B * 0.5, bpm.D * 0.5),
//							new Point2d(-bpm.B * 0.5, bpm.D * 0.5)
//						};
//					}
//					else if (bpm.Mirror == SectionModel.MirrorTypes.Left)
//					{
//						sectionPoints = new List<Point2d>
//						{
//							new Point2d(0.5 * bpm.MirrorGapA + bpm.B, -bpm.D * 0.5),
//							new Point2d(0.5 * bpm.MirrorGapA + bpm.B, bpm.D * 0.5),
//							new Point2d(-0.5 * bpm.MirrorGapA - bpm.B, -bpm.D * 0.5),
//							new Point2d(-0.5 * bpm.MirrorGapA - bpm.B, bpm.D * 0.5)
//						};
//					}
//					else
//					{
//						throw new NotImplementedException("Type of mirror not supported for beam " + bpm.Section.Code + " " + bpm.Name + " " + bpm.ToString());
//					}
//					break;
//				case SectionModel.SectionTypes.Omega:
//					throw new NotImplementedException("Omega sections not supported");
//				case SectionModel.SectionTypes.Angle:
//					if (bpm.Mirror == SectionModel.MirrorTypes.None)
//					{
//						throw new NotImplementedException("Not suppoerted");
//					}
//					else if (bpm.Mirror == SectionModel.MirrorTypes.Left)
//					{
//						sectionPoints = new List<Point2d>
//						{
//							new Point2d(bpm.B + 0.5 * bpm.MirrorGapA, -bpm.Centroid.Y),
//							new Point2d(bpm.B + 0.5 * bpm.MirrorGapA, -bpm.Centroid.Y + bpm.T1),
//							new Point2d(0.5 * bpm.MirrorGapA + bpm.T2, -bpm.Centroid.Y + bpm.D),
//							new Point2d(-bpm.B - 0.5 * bpm.MirrorGapA, -bpm.Centroid.Y),
//							new Point2d(-bpm.B - 0.5 * bpm.MirrorGapA, -bpm.Centroid.Y + bpm.T1),
//							new Point2d(-0.5 * bpm.MirrorGapA - bpm.T2, -bpm.Centroid.Y + bpm.D)
//						};
//					}
//					else
//					{
//						throw new NotImplementedException("Type of mirror not supported" + bpm.Section.Code + " " + bpm.Name + " " + bpm.ToString());
//					}
//					break;
//				case SectionModel.SectionTypes.Z:
//					throw new NotImplementedException("Z sections not supported");
//				case SectionModel.SectionTypes.Generic:
//					return null;
//				case SectionModel.SectionTypes.GenericShapes:
//					{
//						throw new NotSupportedException("Type of section not supported");
//					}
//				default:
//					throw new NotSupportedException("Type of section unknown");
//			}
//			return sectionPoints;
//		}

//		public static void GetSigmas(BeamForceResultModel bfrm, BeamPropertyModel bpm, List<Point2d> sectionPoints, out double minSigma, out double maxSigma)
//		{
//			double m11 = -bfrm.BendingMoment.Y;
//			double m22 = bfrm.BendingMoment.X;
//			switch (bpm.SectionType)
//			{
//				case SectionModel.SectionTypes.HollowCircle:
//				case SectionModel.SectionTypes.SolidCircle:
//					{
//						double m = Math.Sqrt(bfrm.BendingMoment.X * bfrm.BendingMoment.X + bfrm.BendingMoment.Y * bfrm.BendingMoment.Y);

//						if (Math.Abs(bpm.I11 - bpm.I22) > 0.001)
//							throw new NotSupportedException("Inertia moment Ixx,Iyy must be equal for circular sections");

//						minSigma = bfrm.AxialForce / bpm.SectionArea - m / bpm.I11;
//						maxSigma = bfrm.AxialForce / bpm.SectionArea + m / bpm.I11;
//						return;
//					}
//				case SectionModel.SectionTypes.Generic:
//					{
//						double[] S = new double[4];
//						S[0] = bfrm.AxialForce / bpm.SectionArea + m11 / bpm.W11 + m22 / bpm.W22;
//						S[1] = bfrm.AxialForce / bpm.SectionArea - m11 / bpm.W11 + m22 / bpm.W22;
//						S[2] = bfrm.AxialForce / bpm.SectionArea + m11 / bpm.W11 - m22 / bpm.W22;
//						S[3] = bfrm.AxialForce / bpm.SectionArea - m11 / bpm.W11 - m22 / bpm.W22;

//						minSigma = S.Min();
//						maxSigma = S.Max();
//						break;
//					}
//				default:
//					{
//						minSigma = double.MaxValue;
//						maxSigma = double.MinValue;
//						if (sectionPoints == null)
//							throw new NotSupportedException("Type of section unknown");
//						foreach (Point2d p in sectionPoints)
//						{
//							double sigma = bfrm.AxialForce / bpm.SectionArea + m11 / bpm.I11 * p.Y - m22 / bpm.I22 * p.X;
//							if (sigma < minSigma)
//								minSigma = sigma;
//							if (sigma > maxSigma)
//								maxSigma = sigma;
//						}
//						return;
//					}
//			}
//		}

//		public class BeamResult
//		{
//			public readonly GH_Beam Beam;
//			public readonly BeamForceResultModel Result;

//			public BeamResult(GH_Beam beam, BeamForceResultModel result)
//			{
//				Beam = beam;
//				Result = result;
//			}
//		}

//		/// <summary>
//		/// Provides an Icon for the component.
//		/// </summary>
//		protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPBeamResultFilterIcon;

//		/// <summary>
//		/// Gets the unique ID for this component. Do not change this ID after release.
//		/// </summary>
//		public override Guid ComponentGuid => new Guid("80a94ce7-5273-4714-9dda-2e6e6605182a");
//	}
//}