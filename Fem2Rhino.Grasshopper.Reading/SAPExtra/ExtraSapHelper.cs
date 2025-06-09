//using SAP2000v1;
//using FeMM.Common.Models;
//using Rhino.Geometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace FeMM.Grasshopper.Components.SAPExtra
//{
//	internal class ExtraSapHelper
//	{
//		#region Label

//		public static string GetNodeLabel(NodeModel node)
//		{
//			string userName = GetElementName(node);
//			if (userName == "")
//			{
//				return node.NodeId;
//			}
//			else
//			{
//				return userName;
//			}
//		}

//		public static string GetBeamLabel(BeamModel beam)
//		{
//			string userName = GetElementName(beam);
//			if (userName == "")
//			{
//				return beam.BeamId;
//			}
//			else
//			{
//				return userName;
//			}
//		}

//		public static string GetPlateLabel(PlateModel plate)
//		{
//			string userName = GetElementName(plate);
//			if (userName == "")
//			{
//				return plate.PlateId;
//			}
//			else
//			{
//				return userName;
//			}
//		}

//		private static string GetElementName(ElementModel el)
//		{
//			ElementIdAttributeModel attr = (ElementIdAttributeModel)el.Attributes.FirstOrDefault(a => a is ElementIdAttributeModel);
//			if (attr == null)
//			{
//				return "";
//			}
//			else
//			{
//				return attr.Value;
//			}
//		}

//		#endregion

//		private static void CreateFrameProp(BeamPropertyModel property, string propName, cSapModel sapModel, Dictionary<string, string> materials_map, double multiplierX,
//			double multiplierY, ref int ret, ref List<string> errors, ref List<string> warnings)
//		{
//			switch (property.SectionType)
//			{
//				case SectionModel.SectionTypes.SolidCircle:
//					if (Math.Abs(multiplierX - multiplierY) > 0.001)
//					{
//						errors.Add($"Unable to apply a different taper coefficient to property {propName}");
//					}
//					ret = sapModel.PropFrame.SetCircle(propName, materials_map[property.Material.Name], property.D * multiplierX);
//					break;

//				case SectionModel.SectionTypes.HollowCircle:
//					if (Math.Abs(multiplierX - multiplierY) > 0.001)
//					{
//						errors.Add($"Unable to apply a different taper coefficient to property {propName}");
//					}
//					ret = sapModel.PropFrame.SetPipe(propName, materials_map[property.Material.Name], property.D * multiplierX, property.T);
//					break;

//				case SectionModel.SectionTypes.SolidRectangle:
//					ret = sapModel.PropFrame.SetRectangle(propName, materials_map[property.Material.Name], property.D * multiplierY, property.B * multiplierX);
//					break;

//				case SectionModel.SectionTypes.HollowRectangle:
//					ret = sapModel.PropFrame.SetTube(propName, materials_map[property.Material.Name],
//						property.D * multiplierY, property.B * multiplierX, property.T1, property.T2);
//					break;

//				case SectionModel.SectionTypes.I:
//					ret = sapModel.PropFrame.SetISection(propName, materials_map[property.Material.Name],
//						property.D * multiplierY, property.B2 * multiplierX, property.T2, property.T3,
//						property.B1 * multiplierX, property.T1);
//					break;

//				case SectionModel.SectionTypes.T:
//					ret = sapModel.PropFrame.SetTee(propName, materials_map[property.Material.Name],
//						property.D * multiplierY, property.B * multiplierX, property.T1, property.T2);
//					break;

//				case SectionModel.SectionTypes.C:
//				case SectionModel.SectionTypes.Omega:
//					if (property.Mirror == SectionModel.MirrorTypes.None)
//						ret = sapModel.PropFrame.SetChannel(propName, materials_map[property.Material.Name],
//							property.D * multiplierY, property.B * multiplierX, property.T1, property.T2);
//					else
//						ret = sapModel.PropFrame.SetDblChannel(propName, materials_map[property.Material.Name],
//							property.D * multiplierY, 2 * property.B * multiplierX, property.T1, property.T2, property.MirrorGapA);
//					if (property.L > 0 || property.T3 > 0)
//						warnings.Add($"Warning: section {propName} not fully supported");
//					break;

//				case SectionModel.SectionTypes.Angle:
//					if (property.Mirror == SectionModel.MirrorTypes.None)
//						ret = sapModel.PropFrame.SetAngle(propName, materials_map[property.Material.Name],
//							property.D * multiplierY, property.B * multiplierX, property.T1, property.T2);
//					else
//						ret = sapModel.PropFrame.SetDblAngle(propName, materials_map[property.Material.Name],
//							property.D * multiplierY, 2 * property.B * multiplierX, property.T1, property.T2, property.MirrorGapA);
//					break;

//				case SectionModel.SectionTypes.Z:
//					ret = 1;
//					break;

//				case SectionModel.SectionTypes.Generic:
//					double t2, t3 = t2 = Math.Sqrt(property.SectionArea);

//					if (Math.Abs(multiplierX - 1) > 0.001 || Math.Abs(multiplierY - 1) > 0.001)
//						warnings.Add("Unable to taper a generic section");

//					ret = sapModel.PropFrame.SetGeneral(propName, materials_map[property.Material.Name],
//						t3, t2, property.SectionArea, property.ShearA1, property.ShearA2, property.J,
//						property.I11, property.I22, property.W11, property.W22, 0, 0,
//						   Math.Sqrt(property.I11 / property.SectionArea),
//						   Math.Sqrt(property.I22 / property.SectionArea));

//					break;

//				// SECTION DESIGNER
//				case SectionModel.SectionTypes.GenericShapes:
//					int design_type = 0; // 0 : NOCheck; 1 = Design as general steel section; 2 = Design as a concrete column (check the reinforcing); Design as a concrete column; design the reinforcing

//					string material = materials_map[property.Material.Name];
//					eMatType type_material = eMatType.NoDesign;
//					int uninterest_int = 0;
//					string uninterest_string = string.Empty;
//					ret = sapModel.PropMaterial.GetMaterial(material, ref type_material, ref uninterest_int, ref uninterest_string, ref uninterest_string);

//					if (type_material == eMatType.Steel)
//					{
//						design_type = 1;
//					}
//					else if (type_material == eMatType.Concrete)
//					{
//						design_type = 2;
//					}

//					ret = sapModel.PropFrame.SetSDSection(propName, material, design_type);
//					if (ret != 0)
//					{
//						string error = string.Format($"Error {ret} creating SD propery {propName}");
//						errors.Add(error);
//						throw new Exception(error);
//					}


//					Maffeis.Geometry.Shape2d[] shapes = property.GenericShapes.ToShapes2d();
//					for (int j = 0; j < shapes.Length; j++)
//					{
//						Maffeis.Geometry.Shape2d shape = shapes[j];
//						double[] xs = new double[shape.Fill.Count];
//						double[] ys = new double[shape.Fill.Count];

//						for (int i = 0; i < xs.Length; i++)
//						{
//							xs[i] = shape.Fill[i].X * multiplierX;
//							ys[i] = shape.Fill[i].Y * multiplierY;
//						}

//						double[] radius = new double[xs.Length];

//						string shapeName = string.Empty;
//						ret = sapModel.PropFrame.SDShape.SetPolygon(propName, ref shapeName, material, string.Empty, xs.Length, ref xs, ref ys, ref radius);

//						if (ret != 0)
//						{
//							string error = string.Format($"Error {ret} creating SD property {propName} shapes");
//							errors.Add(error);
//							throw new Exception(error);
//						}
//					}


//					break;
//			}
//			if (ret != 0)
//			{
//				string error = string.Format($"Error {ret} creating frame property {propName}");
//				errors.Add(error);
//				throw new Exception(error);
//			}
//		}

//		public static double GetSapPlateAngleFromDefault(PlateModel plate)
//		{
//			//pag.186 SAP2000 Analysis Reference Manual 
//			plate.GetLocalAxes(out Vector3d femmAxesX, out Vector3d femmAxesY, out Vector3d femmAxesZ);
//			Vector3d sapDefaultAxesZ = femmAxesZ;
//			bool horizontalSap = Math.Sin(Math.Acos(sapDefaultAxesZ.Z)) < 0.001;
//			Vector3d sapDefaultAxesY;
//			//Plane defined by sapAxesY-Z vertical
//			if (horizontalSap)
//			{
//				sapDefaultAxesY = Vector3d.CrossProduct(sapDefaultAxesZ, Vector3d.XAxis);
//				sapDefaultAxesY.Unitize();
//				if (Vector3d.Multiply(sapDefaultAxesY, Vector3d.YAxis) < 0)
//				{
//					sapDefaultAxesY.Reverse();
//				}
//			}
//			else
//			{
//				Vector3d buffer = Vector3d.CrossProduct(sapDefaultAxesZ, Vector3d.ZAxis);
//				buffer.Unitize();
//				sapDefaultAxesY = Vector3d.CrossProduct(buffer, sapDefaultAxesZ);
//				sapDefaultAxesY.Unitize();
//				if (Vector3d.Multiply(sapDefaultAxesY, Vector3d.ZAxis) < 0)
//				{
//					sapDefaultAxesY.Reverse();
//				}
//			}
//			Vector3d sapDefaultAxesX = Vector3d.CrossProduct(sapDefaultAxesY, sapDefaultAxesZ);
//			sapDefaultAxesX.Unitize();
//			return Math.Atan2(Vector3d.Multiply(sapDefaultAxesY, femmAxesX), Vector3d.Multiply(sapDefaultAxesX, femmAxesX)) * 180 / Math.PI;
//		}

//		public static bool NeedRotate180(BeamModel beam)
//		{
//			return NeedRotate180(beam.BeamProperty);
//		}

//		public static bool NeedRotate180(BeamPropertyModel bpm)
//		{
//			return bpm.SectionType == SectionModel.SectionTypes.Angle && bpm.Mirror != SectionModel.MirrorTypes.None;
//		}

//		/// <summary>
//		/// Get the frame name from the given point
//		/// </summary>
//		/// <param name="sapModel">The model object</param>
//		/// <param name="node_i">The node I coordinates</param>
//		/// <param name="node_j">The node J coordinates</param>
//		/// <param name="name">The name of the frame</param>
//		/// <returns>True if exists</returns>
//		private static bool GetNameFrameByPoints(cSapModel sapModel, Point3d node_i, Point3d node_j, ref string name)
//		{
//			int count = 0;
//			string[] names = new string[0];
//			sapModel.FrameObj.GetNameList(ref count, ref names);
//			for (int i = 0; i < count; i++)
//			{
//				string name_i = "", name_j = "";
//				sapModel.FrameObj.GetPoints(names[i], ref name_i, ref name_j);
//				double x_i = 0, y_i = 0, z_i = 0;
//				sapModel.PointObj.GetCoordCartesian(name_i, ref x_i, ref y_i, ref z_i);
//				Point3d pt_i = new Point3d(x_i, y_i, z_i);
//				double x_j = 0, y_j = 0, z_j = 0;
//				sapModel.PointObj.GetCoordCartesian(name_j, ref x_j, ref y_j, ref z_j);
//				Point3d pt_j = new Point3d(x_j, y_j, z_j);
//				if (node_i.Equals(pt_i) && node_j.Equals(pt_j))
//				{
//					name = names[i];
//					return true;
//				}
//			}

//			return false;
//		}

//		public static void ConvertStressesFromSAP2000(BeamModel beam, ref double m1, ref double m2, ref double t1, ref double t2)
//		{
//			ConvertStressesFromSAP2000(beam.BeamProperty, ref m1, ref m2, ref t1, ref t2);
//		}

//		public static void ConvertStressesFromSAP2000(BeamPropertyModel bpm, ref double m1, ref double m2, ref double t1, ref double t2)
//		{
//			double cosAlfaXTo1 = Math.Cos(bpm.AngleX1Rad);
//			double senAlfaXTo1 = Math.Sin(bpm.AngleX1Rad);
//			double mPrinc1, mPrinc2, vx, vy, vPrinc1, vPrinc2, mx, my;

//			// Getting axes moment values, on local sdr x-y-z
//			mx = -m2;
//			my = -m1;
//			mPrinc1 = mx * cosAlfaXTo1 + my * senAlfaXTo1;
//			mPrinc2 = -mx * senAlfaXTo1 + my * cosAlfaXTo1;
//			// Moving to straus conventions
//			m1 = mPrinc2;
//			m2 = -mPrinc1;
//			// Shear
//			vx = -t2;
//			vy = t1;
//			vPrinc1 = vx * cosAlfaXTo1 + vy * senAlfaXTo1;
//			vPrinc2 = -vx * senAlfaXTo1 + vy * cosAlfaXTo1;
//			t1 = -vPrinc1;
//			t2 = -vPrinc2;
//			if (NeedRotate180(bpm))
//			{
//				m1 *= -1;
//				m2 *= -1;
//				t1 *= -1;
//				t2 *= -1;
//			}
//		}

//		public static void ConvertStressesToSAP2000(BeamModel beam, ref double m1To3, ref double m2To2, ref double t1To3, ref double t2To2)
//		{
//			ConvertStressesToSAP2000(beam.BeamProperty, ref m1To3, ref m2To2, ref t1To3, ref t2To2);
//		}

//		public static void ConvertStressesToSAP2000(BeamPropertyModel bpm, ref double m1To3, ref double m2To2, ref double t1To3, ref double t2To2)
//		{
//			double cosAlfa1ToX = Math.Cos(-bpm.AngleX1Rad);
//			double senAlfa1ToX = Math.Sin(-bpm.AngleX1Rad);
//			double mPrinc1, mPrinc2, vx, vy, vPrinc1, vPrinc2, mx, my;

//			// Getting axes moment values, on local sdr x-y-z
//			mPrinc1 = -m2To2;
//			mPrinc2 = m1To3;
//			if (NeedRotate180(bpm))
//			{
//				mPrinc1 *= -1;
//				mPrinc2 *= -1;
//			}
//			mx = mPrinc1 * cosAlfa1ToX + mPrinc2 * senAlfa1ToX;
//			my = -mPrinc1 * senAlfa1ToX + mPrinc2 * cosAlfa1ToX;
//			//Moving to sap conventions
//			m1To3 = -mx;
//			m2To2 = -my;

//			// Shear
//			vPrinc1 = -t1To3;
//			vPrinc2 = -t2To2;
//			if (NeedRotate180(bpm))
//			{
//				vPrinc1 *= -1;
//				vPrinc2 *= -1;
//			}
//			vx = vPrinc1 * cosAlfa1ToX + vPrinc2 * senAlfa1ToX;
//			vy = -vPrinc1 * senAlfa1ToX + vPrinc2 * cosAlfa1ToX;
//			t1To3 = -vx;
//			t2To2 = vy;
//		}

//		public static int ConvertDirectionToSap(int dir_straus, bool absolute = true)
//		{
//			int dir_sap = -1;
//			if (dir_straus == 3)
//			{
//				dir_sap = 1;
//			}
//			else if (dir_straus == 2)
//			{
//				dir_sap = 2;
//			}
//			else if (dir_straus == 1)
//			{
//				dir_sap = -3;
//			}
//			else if (dir_straus == 6)
//			{
//				dir_sap = 4;
//			}
//			else if (dir_straus == 5)
//			{
//				dir_sap = 5;
//			}
//			else if (dir_straus == 4)
//			{
//				dir_sap = -6;
//			}

//			if (absolute == true)
//			{
//				return Math.Abs(dir_sap);
//			}
//			else
//			{
//				return dir_sap;
//			}
//		}

//		public static int ConvertDirectionToStrauss(int dir_sap, bool absolute = true)
//		{
//			int dir_strauss = -1;
//			if (dir_sap == 1)
//			{
//				dir_strauss = 3;
//			}
//			else if (dir_sap == 2)
//			{
//				dir_strauss = 2;
//			}
//			else if (dir_sap == 3)
//			{
//				dir_strauss = 1;
//			}
//			else if (dir_sap == 4)
//			{
//				dir_strauss = 6;
//			}
//			else if (dir_sap == 5)
//			{
//				dir_strauss = 5;
//			}
//			else if (dir_sap == 6)
//			{
//				dir_strauss = -4;
//			}

//			if (absolute == true)
//			{
//				return Math.Abs(dir_strauss);
//			}
//			else
//			{
//				return dir_strauss;
//			}
//		}
//	}
//}
