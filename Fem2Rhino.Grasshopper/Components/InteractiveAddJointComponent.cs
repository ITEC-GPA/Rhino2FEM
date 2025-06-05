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
	public class InteractiveAddJointComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveAddJointComponent()
			: base("Add Joint to Interactive Table", "AJ", "Add joint to interactive model", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEADD)
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
			pManager.AddTextParameter("Joint Layer", "LJ", "F2R Joints", GH_ParamAccess.item);
			int i = pManager.AddPointParameter("New GH Joints", "NGJ", "F2R New Joints", GH_ParamAccess.list);
			pManager[i].Optional = true;
			int j = pManager.AddGeometryParameter("New Rhino Joints", "NRJ", "F2R New Joints", GH_ParamAccess.list);
			pManager[j].Optional = true;
			pManager.AddTextParameter("Prefix", "P", "New joints ID prefix", GH_ParamAccess.item, string.Empty);
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
			string layerString = string.Empty;
			List<Point3d> newGHPoints = new List<Point3d>();
			List<IGH_GeometricGoo> newRHPoints = new List<IGH_GeometricGoo>();
			string jointPrefix = string.Empty;
			double tolerance = 0.1;

			if (!DA.GetData(0, ref layerString))
				return;
			DA.GetDataList(1, newGHPoints);
			DA.GetDataList(2, newRHPoints);
			if (!DA.GetData(3, ref jointPrefix))
				return;
			DA.GetData(4, ref tolerance);

			if (_run)
			{
				try
				{
					RhinoDoc doc = RhinoDoc.ActiveDoc;
					List<string> f2rId = new List<string>();
					List<string> f2rIdBuffer = new List<string>();

					int jointLayerIndex = RhinoDoc.ActiveDoc.Layers.FindByFullPath(layerString, -1);

					if (jointLayerIndex == -1)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Layer don't exist");

					Layer[] layersChildBuffer = F2RModelHelper.GetAllLayerChildren(doc.Layers.FindIndex(jointLayerIndex));
					int[] layersChildIndex = layersChildBuffer.Select(i => i.Index).ToArray();

					RhinoObject[] rhinoObjPoints = doc.Objects.FindByObjectType(ObjectType.Point);
					Dictionary<Point3d, PointObject> pointDictionary = Fem2Rhino.Common.F2RModelHelper.SetPointDictionary(rhinoObjPoints, jointLayerIndex, out List<string> f2rJointId, out List<string> log);

					for (int i = 0; i < log.Count; i++)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

					int jointCount = Fem2Rhino.Common.F2RModelHelper.GetNewElementId(f2rJointId, jointPrefix);

					for (int i = 0; i < newGHPoints.Count; i++)
					{
						bool exist = false;

						foreach (Point3d pp in pointDictionary.Keys)
						{
							exist = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, newGHPoints[i], pointDictionary, doc, tolerance, jointPrefix, jointLayerIndex, ref jointCount);
							if (exist)
								break;
						}
						if (!exist)
							Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, newGHPoints[i], doc, jointPrefix, jointLayerIndex, ref jointCount);
					}

					for (int i = 0; i < newRHPoints.Count; i++)
					{
						IGH_GeometricGoo geom = newRHPoints[i];
						Guid guid = geom.ReferenceID;
						if (guid != Guid.Empty)
						{
							RhinoObject rhinoObj = doc.Objects.FindId(guid);
							if (rhinoObj != null)
							{
								if (rhinoObj.GetType() == typeof(PointObject))
								{
									PointObject pointObj = (PointObject)rhinoObj;
									Point pointGeom = (Point)pointObj.Geometry;
									bool exist = false;

									foreach (Point3d pp in pointDictionary.Keys)
									{
										if (pp.DistanceTo(pointGeom.Location) < tolerance)
										{
											if (!pointObj.Attributes.UserDictionary.ContainsKey("F2R_ID"))
											{
												var newAttr = pointObj.Attributes;
												newAttr.LayerIndex = jointLayerIndex;
												newAttr.SetUserString("F2R_ID", jointPrefix + jointCount);
												doc.Objects.ModifyAttributes(pointObj, newAttr, true);

												jointCount++;

												exist = true;
												break;
											}
										}
									}
									if (!exist)
										Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, newGHPoints[i], doc, jointPrefix, jointLayerIndex, ref jointCount);
								}
								else
								{
									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Rhino element n° {i} is not a Point");
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

					foreach (string ss in log)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ss);

					_run = false;
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
					_run = false;
				}
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoInteractiveAddJoint;

		public override Guid ComponentGuid => new Guid("7867f971-bc45-4815-a9e6-4a3a980dc2ee");
	}
}
