using CSiAPIv1;
using FeMM.Grasshopper.Helpers;
using Grasshopper.Kernel;
using System;

namespace FeMM.Grasshopper.Components.F2R
{
	public class SAPGetEditingTableNameComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public SAPGetEditingTableNameComponent()
			: base("Get Editing Table Name", "ETN", "Get SAP2000 Editing Table Name", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVESAP)
		{
		}

		private bool _run = false;

		public override void CreateAttributes()
		{
			ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Read");
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
			pManager.AddTextParameter("Model Path", "MP", "SAP2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
			pManager[0].Optional = true;
			pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
			pManager[1].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddTextParameter("Table Keys", "TK", "The key for the available tables", GH_ParamAccess.list);
			pManager.AddTextParameter("Table Names", "TN", "The table names for the available tables", GH_ParamAccess.list);
			pManager.AddIntegerParameter("Import Type", "IT", "This is either 0, 1, 2 or 3 indicating the import type for the table. " +
				"\n0= not importable. " +
				"\n1= importable, but not interactively importable. " +
				"\n2= importable and interactive importable when he model is unlocked. " +
				"\n3= importable and interactive importable when he model is unlocked and locked", GH_ParamAccess.list);
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

				// Gets table of available tables
				int numberTables = 0;
				string[] tableKeys = null;
				string[] tableNames = null;
				int[] importTypes = null;
				sapModel.DatabaseTables.GetAvailableTables(ref numberTables, ref tableKeys, ref tableNames, ref importTypes);

				DA.SetDataList(0, tableKeys);
				DA.SetDataList(1, tableNames);
				DA.SetDataList(2, importTypes);
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RGetEditingTableName;

		public override Guid ComponentGuid => new Guid("3fd2ee2a-4d7a-4e47-ba33-58627fcc55b8");
	}
}
