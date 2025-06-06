using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
    public class SAPSetEditingTableComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public SAPSetEditingTableComponent()
            : base("Set Editing Table", "WET", "Set SAP2000 Editing Table", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVESAP)
        {
        }

        private bool _run = false;

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Write");
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

                cSapModel sapModel = F2RModelHelper.GetSapModel(out _, modelPath, attachToInstance);

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

        public override Guid ComponentGuid => new Guid("5097d4b9-c9f6-4e5e-a6d7-f5b47c9279fd");
    }
}
