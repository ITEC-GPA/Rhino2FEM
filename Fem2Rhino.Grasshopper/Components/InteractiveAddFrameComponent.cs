using FeMM.Grasshopper.Helpers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FeMM.Grasshopper.Components.F2R
{
	public class InteractiveAddFrameComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveAddFrameComponent()
			: base("Add Frame to Interactive Table", "AF", "Add frame to interactive model", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEADD)
		{
		}

		public override void CreateAttributes()
		{
			ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Add");
			buttonAttributes.ButtonPressed += () =>
			{
				_run = true;
				ExpireSolution(true);
			};

			m_attributes = buttonAttributes;
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Joint Layer", "LJ", "F2R joints", GH_ParamAccess.item);
			pManager.AddTextParameter("Frame Layer", "FJ", "F2R frams", GH_ParamAccess.item);
			int i = pManager.AddLineParameter("New GH Frames", "GHF", "F2R new joints", GH_ParamAccess.list);
			pManager[i].Optional = true;
			int j = pManager.AddGeometryParameter("New Rhino Frames", "RHF", "F2R new joints", GH_ParamAccess.list);
			pManager[j].Optional = true;
			pManager.AddTextParameter("Joint Prefix", "JP", "New joints ID prefix", GH_ParamAccess.item);
			pManager.AddTextParameter("Frame Prefix", "FP", "New frames ID prefix", GH_ParamAccess.item);
			pManager.AddTextParameter("Frame Section", "Fs", "New frames section", GH_ParamAccess.item);
			pManager.AddNumberParameter("Tolerance", "T", "Tolerance", GH_ParamAccess.item, 0.1);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{

		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string jointLayer = string.Empty;
			string frameLayer = string.Empty;
			List<IGH_GeometricGoo> newGHFrames = new List<IGH_GeometricGoo>();
			List<IGH_GeometricGoo> newRHFrames = new List<IGH_GeometricGoo>();
			string jointPrefix = string.Empty;
			string framePrefix = string.Empty;
			string frameSection = string.Empty;
			double tolerance = 0.1;

			if (!DA.GetData(0, ref jointLayer))
				return;
			if (!DA.GetData(1, ref frameLayer))
				return;
			DA.GetDataList(2, newGHFrames);
			DA.GetDataList(3, newRHFrames);
			if (!DA.GetData(4, ref jointPrefix))
				return;
			if (!DA.GetData(5, ref framePrefix))
				return;
			if (!DA.GetData(6, ref frameSection))
				return;
			DA.GetData(7, ref tolerance);

			if (_run)
			{
				try
				{
					if (newGHFrames.Any(i => i.GetType() != typeof(GH_Curve) && i.GetType() != typeof(GH_Line)))
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input frames must be Line or LineCurve");
						return;
					}

					RhinoDoc doc = RhinoDoc.ActiveDoc;

					int jointLayerIndex = RhinoDoc.ActiveDoc.Layers.FindByFullPath(jointLayer, -1);
					int frameLayerIndex = RhinoDoc.ActiveDoc.Layers.FindByFullPath(frameLayer, -1);

					Layer[] jointLayerChildBuffer = F2RModelHelper.GetAllLayerChildren(doc.Layers.FindIndex(jointLayerIndex));
					int[] jointLayersChildIndex = jointLayerChildBuffer.Select(i => i.Index).ToArray();
					Layer[] frameLayerChildBuffer = F2RModelHelper.GetAllLayerChildren(doc.Layers.FindIndex(frameLayerIndex));
					int[] frameLayersChildIndex = jointLayerChildBuffer.Select(i => i.Index).ToArray();

					if (jointLayerIndex == -1)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Joint layer don't exist");
						return;
					}
					if (frameLayerIndex == -1)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Frame layer don't exist");
						return;
					}

					RhinoObject[] rhinoObjPoints = doc.Objects.FindByObjectType(ObjectType.Point);
					RhinoObject[] rhinoObjCurves = doc.Objects.FindByObjectType(ObjectType.Curve);

					Dictionary<Point3d, PointObject> pointDictionary = Fem2Rhino.Common.F2RModelHelper.SetPointDictionary(rhinoObjPoints, jointLayerIndex, out List<string> f2rJointId, out List<string> log);

					for (int i = 0; i < log.Count; i++)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

					int jointCount = Fem2Rhino.Common.F2RModelHelper.GetNewElementId(f2rJointId, jointPrefix);
					List<string> f2rFrameId = Fem2Rhino.Common.F2RModelHelper.GetObjectIds(rhinoObjCurves, frameLayersChildIndex);
					int frameCount = Fem2Rhino.Common.F2RModelHelper.GetNewElementId(f2rFrameId, framePrefix);

					for (int i = 0; i < newGHFrames.Count; i++)
					{
						Point3d startPoint = Point3d.Unset;
						Point3d endPoint = Point3d.Unset;

						if (newGHFrames[i].GetType() == typeof(GH_Line))
						{
							startPoint = ((GH_Line)newGHFrames[i]).Value.From;
							endPoint = ((GH_Line)newGHFrames[i]).Value.To;
						}
						else if (newGHFrames[i].GetType() == typeof(GH_Curve))
						{
							startPoint = ((GH_Curve)newGHFrames[i]).Value.PointAtStart;
							endPoint = ((GH_Curve)newGHFrames[i]).Value.PointAtEnd;
						}
						else
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"GH Frames n° {i} type not implemented");
							continue;
						}

						bool existStart = false;
						bool existEnd = false;
						string startId = string.Empty;
						string endId = string.Empty;

						foreach (Point3d pp in pointDictionary.Keys)
						{
							if (!existStart)
								existStart = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, startPoint, pointDictionary, tolerance, ref startId);
							if (!existEnd)
								existEnd = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, endPoint, pointDictionary, tolerance, ref endId);
							if (existEnd && existStart)
								break;
						}
						if (!existStart)
							startId = Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, startPoint, doc, jointPrefix, jointLayerIndex, ref jointCount);
						if (!existEnd)
							endId = Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, endPoint, doc, jointPrefix, jointLayerIndex, ref jointCount);

						if (startId != string.Empty && endId != string.Empty)
						{
							string frameId = framePrefix + frameCount;

							ObjectAttributes objAtt = new ObjectAttributes { LayerIndex = frameLayerIndex };
							objAtt.SetUserString("F2R_ID", frameId);
							objAtt.SetUserString("F2R_JOINT1", startId);
							objAtt.SetUserString("F2R_JOINT2", endId);
							objAtt.SetUserString("F2R_PROPERTY", frameSection);

							doc.Objects.AddLine(new Line(startPoint, endPoint), objAtt);

							frameCount++;
						}
					}

					for (int i = 0; i < newRHFrames.Count; i++)
					{
						IGH_GeometricGoo geom = newRHFrames[i];
						Guid guid = geom.ReferenceID;
						if (guid != Guid.Empty)
						{
							RhinoObject rhinoObj = doc.Objects.FindId(guid);
							if (rhinoObj != null)
							{
								if (rhinoObj.GetType() == typeof(CurveObject))
								{
									CurveObject curveObj = (CurveObject)rhinoObj;

									Point3d startPoint = Point3d.Unset;
									Point3d endPoint = Point3d.Unset;

									if (curveObj.Geometry is Curve cv)
									{
										startPoint = cv.PointAtStart;
										endPoint = cv.PointAtEnd;
									}

									bool existStart = false;
									bool existEnd = false;
									string startId = string.Empty;
									string endId = string.Empty;

									foreach (Point3d pp in pointDictionary.Keys)
									{
										if (!existStart)
										{
											existStart = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, startPoint, pointDictionary, tolerance, ref startId);
										}
										if (!existEnd)
										{
											existEnd = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, endPoint, pointDictionary, tolerance, ref endId);
										}
										if (existEnd && existStart)
										{
											break;
										}
									}

									if (!existStart)
									{
										startId = Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, startPoint, doc, jointPrefix, jointLayerIndex, ref jointCount);
									}
									if (!existEnd)
									{
										endId = Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, endPoint, doc, jointPrefix, jointLayerIndex, ref jointCount);
									}

									if (startId != string.Empty && endId != string.Empty)
									{
										string frameId = framePrefix + frameCount;

										ObjectAttributes newAttr = rhinoObj.Attributes;
										newAttr.SetUserString("F2R_ID", frameId);
										newAttr.SetUserString("F2R_JOINT1", startId);
										newAttr.SetUserString("F2R_JOINT2", endId);
										newAttr.SetUserString("F2R_PROPERTY", frameSection);
										newAttr.LayerIndex = frameLayerIndex;

										doc.Objects.ModifyAttributes(guid, newAttr, true);

										frameCount++;
									}
								}
								else
								{
									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Rhino element n° {i} is not a Curve");
								}
							}
							else
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Rhino element n° {i} don't exist");
							}
						}
						else
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Rhino element n° {i} don't have a valid GUID");
						}
					}

					_run = false;
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
					_run = false;
				}
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoInteractiveAddFrame;

		public override Guid ComponentGuid => new Guid("fea2ad8f-5a69-4934-9dae-574b1460ae44");
	}
}
