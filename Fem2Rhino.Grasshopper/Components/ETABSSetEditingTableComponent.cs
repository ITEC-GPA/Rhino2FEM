using ETABSv1;
using FeMM.Grasshopper.Helpers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FeMM.Grasshopper.Components.F2R
{
	public class ETABSSetEditingTableComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public ETABSSetEditingTableComponent()
			: base("Set Editing Table", "SET", "Set ETABS Editing Table", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEETABS)
		{
		}

		private bool _run = false;

		public override void CreateAttributes()
		{
			ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Set");
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
			pManager.AddTextParameter("Table key", "T", "The table name", GH_ParamAccess.item);
			pManager.AddTextParameter("Table Field Keys", "T", "The key for each field to set", GH_ParamAccess.list);
			pManager.AddTextParameter("Table", "T", "The Table", GH_ParamAccess.tree);
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
			string modelPath = "";
			bool attachToInstance = false;
			string tableName = string.Empty;
			List<string> fieldName = new List<string>();

			//input
			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);
			DA.GetData(2, ref tableName);
			if (!DA.GetDataList(3, fieldName))
				return;
			if (!DA.GetDataTree(4, out GH_Structure<GH_String> dataTree))
				return;

			if (_run)
			{
				if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
				{
					modelPath = string.Empty;
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Sap model path not valid, opening an empty file");
				}

				cSapModel sapModel = F2RModelHelper.GetEtabsModel(out _, modelPath, attachToInstance);

				// Read table
				int tableVersion = 0;
				int numberRecords = 0;

				string[] tableData = new string[dataTree.DataCount];
				string[] fieldKeysIncluded = fieldName.ToArray();

				List<GH_String> list = dataTree.FlattenData();
				tableData = list.Select(i => i.Value).ToArray();

				if (sapModel.DatabaseTables.SetTableForEditingArray(tableName, ref tableVersion, ref fieldKeysIncluded, numberRecords, ref tableData) != 0)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set table {tableName}");
					Message = "Fail";
					_run = false;
					return;
				}

				_run = false;
				Message = "Done";
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RSetEditingTable;

		public override Guid ComponentGuid => new Guid("6ee32d24-6192-46a9-b325-8423eae80c09");
	}
}
