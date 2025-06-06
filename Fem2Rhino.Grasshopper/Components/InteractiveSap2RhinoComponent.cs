using Fem2Rhino.Grasshopper.DataTypes.F2R;
using Grasshopper.Kernel;
using Rhino;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class InteractiveSap2RhinoComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveSap2RhinoComponent()
			: base("SAP2Rhino", "S2R", "Open interactive model of current open istance", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVESAP)
		{
		}

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "SAP2Rhino");
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
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Group", "G", "Sap2000 group for selection element. Default value: ALL", GH_ParamAccess.item, "ALL");
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{

		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string groupName = "";

			DA.GetData(0, ref groupName);

			if (_run)
			{
				GH_SAPInteractiveDatabaseModel modelType = new GH_SAPInteractiveDatabaseModel();
				modelType.Value.Group = groupName;
				try
				{
					modelType.Value.Process();
				}
				catch (Exception ex)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to process the model: {ex.Message}");
				}

				List<string> log = modelType.GetLog();
				if (log.Count > 0)
				{
					foreach (string logItem in log)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, logItem);
					}
				}

				try
				{
					modelType.BakeGeometryCustom(RhinoDoc.ActiveDoc);
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to bake the model");
				}

				_run = false;
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RSAP2Rhino;

		public override Guid ComponentGuid => new Guid("ac891bf5-4cbe-4832-bc19-e04c21895538");
	}
}
