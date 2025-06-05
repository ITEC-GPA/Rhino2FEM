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
	public class InteractiveUpdateAreaComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveUpdateAreaComponent()
			: base("Update Area", "UA", "Update area to interactive model", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEUPDATE)
		{
		}

		public override void CreateAttributes()
		{
			ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Update");
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
			pManager.AddGeometryParameter("Areas To Update", "UF", "F2R areas to update", GH_ParamAccess.list);
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

									for (int j = 0; j < rhinoObj.Attributes.UserStringCount; j++)
									{
										System.Collections.Specialized.NameValueCollection objAttributesList = rhinoObj.Attributes.GetUserStrings();
										foreach (object objName in objAttributesList.Keys)
										{
											if (((string)objName).StartsWith("F2R_JOINT"))
											{
												rhinoObj.Attributes.DeleteUserString((string)objName);
											}
										}
									}

									rhinoObj.Attributes.SetUserString("F2R_VERTICESCOUNT", jointNames.Length.ToString());
									for (int j = 0; j < jointNames.Length; j++)
										rhinoObj.Attributes.SetUserString($"F2R_JOINT{j + 1}", jointNames[j]);
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

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoInteractiveUpdateArea;

		public override Guid ComponentGuid => new Guid("bf04670d-6cd0-42ac-bc3d-5f2c6cbaba32");
	}
}
