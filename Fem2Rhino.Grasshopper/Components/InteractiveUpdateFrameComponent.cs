
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class InteractiveUpdateFrameComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveUpdateFrameComponent()
			: base("Update Frame", "UF", "Update frame to interactive model", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVEUPDATE)
		{
		}

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Update");
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
			pManager.AddGeometryParameter("Frames To Update", "UF", "F2R frames to update", GH_ParamAccess.list);
			pManager.AddTextParameter("Joint Prefix", "JP", "New joints ID prefix", GH_ParamAccess.item);
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
			List<IGH_GeometricGoo> newRHFrames = new List<IGH_GeometricGoo>();
			string jointPrefix = string.Empty;
			double tolerance = 0.1;

			if (!DA.GetData(0, ref jointLayer))
				return;
			if (!DA.GetDataList(1, newRHFrames))
				return;
			if (!DA.GetData(2, ref jointPrefix))
				return;
			DA.GetData(3, ref tolerance);

			if (_run)
			{
				try
				{
					RhinoDoc doc = RhinoDoc.ActiveDoc;

					int jointLayerIndex = RhinoDoc.ActiveDoc.Layers.FindByFullPath(jointLayer, -1);

					if (jointLayerIndex == -1)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Joint layer don't exist");

					RhinoObject[] rhinoObjPoints = doc.Objects.FindByObjectType(ObjectType.Point);

					Dictionary<Point3d, PointObject> pointDictionary = Fem2Rhino.Common.F2RModelHelper.SetPointDictionary(rhinoObjPoints, jointLayerIndex, out List<string> f2rJointId, out List<string> log);

					for (int i = 0; i < log.Count; i++)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

					int jointCount = Fem2Rhino.Common.F2RModelHelper.GetNewElementId(f2rJointId, jointPrefix);

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
										rhinoObj.Attributes.SetUserString("F2R_JOINT1", startId);
										rhinoObj.Attributes.SetUserString("F2R_JOINT2", endId);
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

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoInteractiveUpdateFrame;

		public override Guid ComponentGuid => new Guid("d61e4d3d-35c4-4dad-8d20-70f664562d5a");
	}
}
