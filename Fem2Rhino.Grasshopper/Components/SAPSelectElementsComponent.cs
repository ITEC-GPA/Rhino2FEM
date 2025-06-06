using SAP2000v1;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class SAPSelectElementsComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public SAPSelectElementsComponent()
			: base("Select Elements", "SE", "Select in SAP2000 the rhino elements", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVESAP)
		{
		}

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Select");
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
			pManager.AddGeometryParameter("Elements", "E", "F2R element to select", GH_ParamAccess.list);
			pManager.AddBooleanParameter("Add to selection", "A", "If true, add the selected element to the previous selection. Otherwise it replace the selection", GH_ParamAccess.item, false);
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
			List<IGH_GeometricGoo> elements = new List<IGH_GeometricGoo>();
			bool addToSelection = false;

			if (!DA.GetDataList(0, elements))
				return;
			DA.GetData(1, ref addToSelection);

			if (_run)
			{
				try
				{
					cSapModel sapModel = F2RModelHelper.GetSapModel(out _, "", true);

					if (!addToSelection)
						sapModel.SelectObj.ClearSelection();

					string nameCustom = (DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + DateTime.Now.Ticks).ToString();
					sapModel.GroupDef.SetGroup(nameCustom);

					RhinoDoc doc = RhinoDoc.ActiveDoc;
					for (int i = 0; i < elements.Count; i++)
					{
						Guid guid = elements[i].ReferenceID;
						if (guid != Guid.Empty)
						{
							RhinoObject rhinoObj = doc.Objects.FindId(guid);

							if (rhinoObj != null)
							{
								string id = rhinoObj.Attributes.GetUserString("F2R_ID");
								if (rhinoObj.GetType() == typeof(PointObject))
								{
									sapModel.PointObj.SetGroupAssign(id, nameCustom, false);
								}
								else if (rhinoObj.GetType() == typeof(CurveObject))
								{
									sapModel.FrameObj.SetGroupAssign(id, nameCustom, false);
								}
								else if (rhinoObj.GetType() == typeof(SurfaceObject) || rhinoObj.GetType() == typeof(BrepObject))
								{
									sapModel.AreaObj.SetGroupAssign(id, nameCustom, false);
								}
								else
								{
									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element n° {i} is not a point or a curve or a surface");
								}
							}
							else
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element n° {i} don't exist in Rhino");
							}
						}
						else
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element {i} don't have a valid GUID");
						}
					}

					sapModel.SelectObj.Group(nameCustom, false);
					sapModel.GroupDef.Clear(nameCustom);
                    sapModel.GroupDef.Delete(nameCustom);

					sapModel.View.RefreshWindow();

					_run = false;
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
					_run = false;
				}
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RSelectElementsIcon;

		public override Guid ComponentGuid => new Guid("3388df52-adc9-4d92-978a-98e92406eba5");
	}
}
