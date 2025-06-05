using CSiAPIv1;
using FeMM.Grasshopper.Helpers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace FeMM.Grasshopper.Components.F2R
{
	public class ETABSSelectElementsComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public ETABSSelectElementsComponent()
			: base("ETABS Select Elements", "ES", "Select in ETABS the rhino elements", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEETABS)
		{
		}

		public override void CreateAttributes()
		{
			ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Select");
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
					cSapModel sapModel = F2RModelHelper.GetCSIEtabsModel(out _, "", true);

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

		public override Guid ComponentGuid => new Guid("1f5c230e-f208-4618-83f0-b82f78cbfbe9");
	}
}
