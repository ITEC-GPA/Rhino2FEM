using SAP2000v1;

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class SAPSelectElementsListComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public SAPSelectElementsListComponent()
			: base("Select Elements List", "SEL", "Select in SAP2000 the rhino elements", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVESAP)
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
			pManager.AddTextParameter("Points", "P", "The list of Id to select", GH_ParamAccess.list);
			pManager[0].Optional = true;
			pManager.AddTextParameter("Frames", "F", "The list of Id to select", GH_ParamAccess.list);
			pManager[1].Optional = true;
			pManager.AddTextParameter("Areas", "A", "The list of Id to select", GH_ParamAccess.list);
			pManager[2].Optional = true;
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
			List<string> points = new List<string>();
			List<string> frames = new List<string>();
			List<string> areas = new List<string>();
			bool addToSelection = false;

			DA.GetDataList(0, points);
			DA.GetDataList(1, frames);
			DA.GetDataList(2, areas);
			DA.GetData(3, ref addToSelection);

			if (_run)
			{
				try
				{
					cSapModel sapModel = F2RModelHelper.GetSapModel(out _, "", true);

					if (!addToSelection)
						sapModel.SelectObj.ClearSelection();

					string nameCustom = (DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + DateTime.Now.Ticks).ToString();
					sapModel.GroupDef.SetGroup(nameCustom);

					for (int i = 0; i < points.Count; i++)
					{
						sapModel.PointObj.SetGroupAssign(points[i], nameCustom, false);
					}

					for (int i = 0; i < frames.Count; i++)
					{
						sapModel.FrameObj.SetGroupAssign(frames[i], nameCustom, false);
					}

					for (int i = 0; i < areas.Count; i++)
					{
						sapModel.AreaObj.SetGroupAssign(areas[i], nameCustom, false);
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

		public override Guid ComponentGuid => new Guid("6ed3854c-2c09-457e-8ced-49c5e39717f9");
	}
}
