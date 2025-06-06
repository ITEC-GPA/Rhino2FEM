using SAP2000v1;

using Grasshopper.Kernel;
using System;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class SAPApplyEditedTableComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public SAPApplyEditedTableComponent()
			: base("Apply Editing Table", "AET", "Apply SAP2000 Editing Table", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVESAP)
		{
		}

		private bool _run = false;

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Apply");
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
			pManager.AddTextParameter("Model Path", "MP", "SAP2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
			pManager[0].Optional = true;
			pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
			pManager[1].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
			pManager.AddTextParameter("Log", "L", "SAP2000 log", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string modelPath = "";
			bool attachToInstance = false;

			//input
			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);

			if (_run)
			{
				if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
				{
					modelPath = string.Empty;
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Sap model path not valid, opening an empty file");
				}

				cSapModel sapModel = F2RModelHelper.GetSapModel(out _, modelPath, attachToInstance);

				/// Input per la funzione "ApplyEditedTables"
				bool FillImportLog = true;
				int NumFatalErrors = 0;
				int NumErrorMsgs = 0;
				int NumWarnMsgs = 0;
				int NumInfoMsgs = 0;
				string ImportLog = "";

				if (sapModel.DatabaseTables.ApplyEditedTables(FillImportLog, ref NumFatalErrors, ref NumErrorMsgs, ref NumWarnMsgs, ref NumInfoMsgs, ref ImportLog) != 0)
				{
					DA.SetData(0, ImportLog);
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to apply edited tables");
					_run = false;
					Message = "Fail";
					return;
				}

				DA.SetData(0, ImportLog);

				if (sapModel.View.RefreshView() != 0)
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to refresh the view");

				_run = false;
				Message = "Done";
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RApplyEditingTable;

		public override Guid ComponentGuid => new Guid("58fbd2d3-a1e2-46ca-955d-12d868203c23");
	}
}
