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
	public class InteractiveAddAreaComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveAddAreaComponent()
			: base("Add Area to Interactive Table", "AA", "Add area to interactive model", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEADD)
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
			pManager.AddTextParameter("Joint Layer", "JL", "F2R joint layer", GH_ParamAccess.item);
			pManager.AddTextParameter("Area Layer", "AL", "F2R areas layer", GH_ParamAccess.item);
			int i = pManager.AddBrepParameter("New GH Areas", "NGA", "F2R new areas", GH_ParamAccess.list);
			pManager[i].Optional = true;
			int j = pManager.AddGeometryParameter("New Rhino Areas", "NRA", "F2R new areas", GH_ParamAccess.list);
			pManager[j].Optional = true;
			pManager.AddTextParameter("Joint Prefix", "JP", "New joints ID prefix", GH_ParamAccess.item);
			pManager.AddTextParameter("Area Prefix", "AP", "New areas ID prefix", GH_ParamAccess.item);
			pManager.AddTextParameter("Area Property", "P", "New areas property", GH_ParamAccess.item, "None");
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
			string areaLayer = string.Empty;
			List<IGH_GeometricGoo> newRHAreas = new List<IGH_GeometricGoo>();
			List<Brep> newGHAreas = new List<Brep>();
			string jointPrefix = string.Empty;
			string areaPrefix = string.Empty;
			string areaProp = string.Empty;
			double tolerance = 0.1;

			if (!DA.GetData(0, ref jointLayer))
				return;
			if (!DA.GetData(1, ref areaLayer))
				return;
			DA.GetDataList(2, newGHAreas);
			DA.GetDataList(3, newRHAreas);
			if (!DA.GetData(4, ref jointPrefix))
				return;
			if (!DA.GetData(5, ref areaPrefix))
				return;
			DA.GetData(6, ref areaProp);
			DA.GetData(7, ref tolerance);

			if (_run)
			{
				try
				{
					RhinoDoc doc = RhinoDoc.ActiveDoc;

					int jointLayerIndex = RhinoDoc.ActiveDoc.Layers.FindByFullPath(jointLayer, -1);
					int areaLayerIndex = RhinoDoc.ActiveDoc.Layers.FindByFullPath(areaLayer, -1);

					if (jointLayerIndex == -1)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Joint layer don't exist");

					if (areaLayerIndex == -1)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Area layer don't exist");

					Layer[] jointLayerChildBuffer = F2RModelHelper.GetAllLayerChildren(doc.Layers.FindIndex(jointLayerIndex));
					int[] jointLayersChildIndex = jointLayerChildBuffer.Select(i => i.Index).ToArray();
					Layer[] areaLayerChildBuffer = F2RModelHelper.GetAllLayerChildren(doc.Layers.FindIndex(areaLayerIndex));
					int[] areaLayersChildIndex = jointLayerChildBuffer.Select(i => i.Index).ToArray();

					RhinoObject[] rhinoObjPoints = doc.Objects.FindByObjectType(ObjectType.Point);
					RhinoObject[] rhinoObjAreas = doc.Objects.FindByObjectType(ObjectType.Brep);

					Dictionary<Point3d, PointObject> pointDictionary = Fem2Rhino.Common.F2RModelHelper.SetPointDictionary(rhinoObjPoints, jointLayerIndex, out List<string> f2rJointId, out List<string> log);

					for (int i = 0; i < log.Count; i++)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

					int jointCount = Fem2Rhino.Common.F2RModelHelper.GetNewElementId(f2rJointId, jointPrefix);
					List<string> f2rAreaId = Fem2Rhino.Common.F2RModelHelper.GetObjectIds(rhinoObjAreas, areaLayersChildIndex);
					int areaCount = Fem2Rhino.Common.F2RModelHelper.GetNewElementId(f2rAreaId, areaPrefix);

					for (int i = 0; i < newGHAreas.Count; i++)
					{
						string[] jointNames = new string[newGHAreas[i].Vertices.Count];
						var curve3d = newGHAreas[i].Curves3D;
						Curve[] curve = Curve.JoinCurves(curve3d).FirstOrDefault().DuplicateSegments();

						for (int j = 0; j < curve.Length; j++)
						{
							bool existJoint = false;
							string nameJoint = string.Empty;

							foreach (Point3d pp in pointDictionary.Keys)
							{
								existJoint = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, curve[j].PointAtStart, pointDictionary, tolerance, ref nameJoint);
								if (existJoint)
									break;
							}

							if (!existJoint)
								nameJoint = Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, curve[j].PointAtStart, doc, jointPrefix, jointLayerIndex, ref jointCount);

							jointNames[j] = nameJoint;
						}

						ObjectAttributes objAtt = new ObjectAttributes { LayerIndex = areaLayerIndex };

						string areaId = areaPrefix + areaCount;
						objAtt.SetUserString("F2R_ID", areaId);
						objAtt.SetUserString("F2R_PROPERTY", areaProp);
						objAtt.SetUserString("F2R_VERTICESCOUNT", jointNames.Length.ToString());
						for (int j = 0; j < jointNames.Length; j++)
							objAtt.SetUserString($"F2R_JOINT{j + 1}", jointNames[j]);

						doc.Objects.AddBrep(newGHAreas[i], objAtt);

						areaCount++;
					}

					for (int i = 0; i < newRHAreas.Count; i++)
					{
						IGH_GeometricGoo geom = newRHAreas[i];
						Guid guid = geom.ReferenceID;
						if (guid != Guid.Empty)
						{
							RhinoObject rhinoObj = doc.Objects.FindId(guid);
							if (rhinoObj != null)
							{
								if (rhinoObj.GetType() == typeof(BrepObject))
								{
									BrepObject brepObj = (BrepObject)rhinoObj;
									var curve3d = brepObj.BrepGeometry.Curves3D;
									Curve[] curve = Curve.JoinCurves(curve3d).FirstOrDefault().DuplicateSegments();

									string[] jointNames = new string[curve.Length];

									for (int j = 0; j < curve.Length; j++)
									{
										bool existJoint = false;
										string nameJoint = string.Empty;

										foreach (Point3d pp in pointDictionary.Keys)
										{
											existJoint = Fem2Rhino.Common.F2RModelHelper.CheckExistingPoint(pp, curve[j].PointAtStart, pointDictionary, tolerance, ref nameJoint);
											if (existJoint)
												break;
										}

										if (!existJoint)
											nameJoint = Fem2Rhino.Common.F2RModelHelper.AddPoint(pointDictionary, curve[j].PointAtStart, doc, jointPrefix, jointLayerIndex, ref jointCount);

										jointNames[j] = nameJoint;
									}

									ObjectAttributes newAttr = rhinoObj.Attributes;
									string areaId = areaPrefix + areaCount;
									newAttr.LayerIndex = areaLayerIndex;
									newAttr.SetUserString("F2R_ID", areaId);
									newAttr.SetUserString("F2R_PROPERTY", areaProp);
									newAttr.SetUserString("F2R_VERTICESCOUNT", jointNames.Length.ToString());
									for (int j = 0; j < jointNames.Length; j++)
										newAttr.SetUserString($"F2R_JOINT{j + 1}", jointNames[j]);

									doc.Objects.ModifyAttributes(guid, newAttr, true);

									areaCount++;
								}
								else
								{
									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Rhino element n° {i} is not a Brep");
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

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoInteractiveAddArea;

		public override Guid ComponentGuid => new Guid("96b1a7a5-4c89-4321-94e7-0e6a1ff12298");
	}
}
