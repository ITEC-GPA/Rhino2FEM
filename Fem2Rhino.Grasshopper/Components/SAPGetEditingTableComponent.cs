using CSiAPIv1;
using FeMM.Grasshopper.Helpers;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;

namespace FeMM.Grasshopper.Components.F2R
{
	public class SAPGetEditingTableComponent : GH_Component
	{
		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public SAPGetEditingTableComponent()
			: base("Get Editing Table", "GET", "Get SAP2000 Editing Table", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVESAP)
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
			pManager.AddTextParameter("Table", "T", "The name of the Table", GH_ParamAccess.item);
			pManager.AddTextParameter("Group", "G", "Sap2000 group for selection element. Default value: ALL", GH_ParamAccess.item, "ALL");
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddTextParameter("Field Keys", "FK", "The key for set each field", GH_ParamAccess.list);
			pManager.AddTextParameter("Field Names", "FN", "The name of each field", GH_ParamAccess.list);
			pManager.AddTextParameter("Descriptions", "D", "The field descriptions", GH_ParamAccess.list);
			pManager.AddTextParameter("Units", "U", "The units", GH_ParamAccess.list);
			pManager.AddBooleanParameter("Are Importables", "I", "If true, the field is settable", GH_ParamAccess.list);
			pManager.AddTextParameter("Table", "T", "The table", GH_ParamAccess.tree);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string modelPath = "";
			bool attachToInstance = false;
			string table = string.Empty;
			string groupName = "";

			//input
			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);
			DA.GetData(2, ref table);
			DA.GetData(3, ref groupName);

			if (_run)
			{
				if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
				{
					modelPath = string.Empty;
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Sap model path not valid, opening an empty file");
				}

				cSapModel sapModel = F2RModelHelper.GetSapModel(out _, modelPath, attachToInstance);

				// Read table
				int tableVersion = 0;
				int numberRecords = 0;
				string[] tableData = null;
				string[] fieldKeysIncluded = new string[0];

				if (sapModel.DatabaseTables.GetTableForEditingArray(table, groupName, ref tableVersion, ref fieldKeysIncluded, ref numberRecords, ref tableData) == 0)
				{
					// Populate result
					string[][] retData = new string[numberRecords][];
					for (int i = 0; i < retData.Length; i++)
						retData[i] = new string[fieldKeysIncluded.Length];

					for (int i = 0; i < tableData.Length; i++)
						retData[i / fieldKeysIncluded.Length][i % fieldKeysIncluded.Length] = tableData[i];

					DataTree<string> tableDataTree = new DataTree<string>();

					for (int i = 0; i < retData.GetLength(0); i++)
						tableDataTree.AddRange(retData[i], new GH_Path(i));

					int numberFileds = 0;
					string[] fieldKeys = null;
					string[] fieldNames = null;
					string[] descriptions = null;
					string[] unitsStrings = null;
					bool[] areImportables = null;
					if (sapModel.DatabaseTables.GetAllFieldsInTable(table, ref tableVersion, ref numberFileds, ref fieldKeys, ref fieldNames,
						ref descriptions, ref unitsStrings, ref areImportables) != 0)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to read table {table}");
						_run = false;
						return;
					}

					DA.SetDataList(0, fieldKeys);
					DA.SetDataList(1, fieldNames);
					DA.SetDataList(2, descriptions);
					DA.SetDataList(3, unitsStrings);
					DA.SetDataList(4, areImportables);
					DA.SetDataTree(5, tableDataTree);

					_run = false;
				}
				else
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to read table {table}");
					_run = false;
					return;
				}
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RGetEditingTable;

		public override Guid ComponentGuid => new Guid("a4482c7f-d4d9-421e-b78d-449f656b5004");
	}
}
