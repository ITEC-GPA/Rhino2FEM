using CSiAPIv1;
using FeMM.Grasshopper.Helpers;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace FeMM.Grasshopper.Components.F2R
{
	public class ETABSSelectElementsListComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public ETABSSelectElementsListComponent()
			: base("Select Elements List", "SEL", "Select in ETABS the rhino elements", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEETABS)
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
					cSapModel etabsModel = F2RModelHelper.GetCSIEtabsModel(out _, "", true);

					if (!addToSelection)
						etabsModel.SelectObj.ClearSelection();

					string nameCustom = (DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + DateTime.Now.Ticks).ToString();
					etabsModel.GroupDef.SetGroup(nameCustom);

					for (int i = 0; i < points.Count; i++)
					{
						etabsModel.PointObj.SetGroupAssign(points[i], nameCustom, false);
					}

					for (int i = 0; i < frames.Count; i++)
					{
						etabsModel.FrameObj.SetGroupAssign(frames[i], nameCustom, false);
					}

					for (int i = 0; i < areas.Count; i++)
					{
						etabsModel.AreaObj.SetGroupAssign(areas[i], nameCustom, false);
					}

					etabsModel.SelectObj.Group(nameCustom, false);
					etabsModel.GroupDef.Delete(nameCustom);

					etabsModel.View.RefreshWindow();

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

		public override Guid ComponentGuid => new Guid("e19f3416-74ce-4050-9a55-8d471b45cf6a");
	}
}
