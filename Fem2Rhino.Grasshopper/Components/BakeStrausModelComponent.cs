using Fem2Rhino.Common;
using Fem2Rhino.Grasshopper.DataTypes.F2R;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ProxyHelper.CSI.ApiWrapper;
using Rhino;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
    public class BakeStrausModelComponent : GH_Component
    {
        protected Dictionary<int, string> _outputPresets = new Dictionary<int, string>()
        {
            { 0, "All" },
            { 1, "Geometry assignements" },
            { 2, "Section properties" },
        };

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BakeStrausModelComponent()
            : base("Bake Straus7 R3 Model", "BS7M", "Bake Straus7 model", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_BAKE)
        {
        }

        private bool _run = false;

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Bake");
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
            pManager.AddTextParameter("Model Path", "MP", "Straus7 model path to open. If empty it will open an empty file", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Output preferences", "O", "", GH_ParamAccess.item, 2);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _outputPresets)
                mtParam.AddNamedValue(v.Value, v.Key);
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
            bool visible = false;
            int type = 0;

            //input
            if (!DA.GetData(0, ref modelPath))
                return;
            if (!DA.GetData(1, ref type))
                return;

            if (_run)
            {
                if (!System.IO.File.Exists(modelPath)) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                {
                    modelPath = string.Empty;
                    visible = true;
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Straus model path not valid, opening an empty file");
                }

                int units;

                if (RhinoDoc.ActiveDoc != null)
                {
                    string rhinoUnits = RhinoDoc.ActiveDoc.GetUnitSystemName(true, true, true, true);
                    if (rhinoUnits.ToLower() == "millimeter" || rhinoUnits.ToLower() == "mm")
                    {
                        units = Units.kN_mm_C;
                    }
                    else if (rhinoUnits.ToLower() == "meter" || rhinoUnits.ToLower() == "m")
                    {
                        units = Units.kN_m_C;
                    }
                    else if (rhinoUnits.ToLower() == "centimeter" || rhinoUnits.ToLower() == "cm")
                    {
                        units = Units.kN_cm_C;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rhino lenght units not recognized. The model will be open using kN, mm, C");
                        units = Units.kN_mm_C;
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rhino active doc is null");
                    units = Units.kN_mm_C;
                }

                Model.Options options = new Model.Options();
                if (type == 0)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = true;
                    options.ExportGeometryDatas = true;
                    options.ExportLoads = true;
                }
                if (type == 1)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = true;
                    options.ExportGeometryDatas = true;
                    options.ExportLoads = false;
                }
                if (type == 2)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = true;
                    options.ExportGeometryDatas = false;
                    options.ExportLoads = false;
                }
                if (type == 3)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = false;
                    options.ExportGeometryDatas = false;
                    options.ExportLoads = false;
                }

                GH_StrausModel modelType = new GH_StrausModel(units, modelPath, visible);
                modelType.Value.Option = options;

                try
                {
                    modelType.Value.Process();
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to read the model");
                }

                try
                {
                    modelType.BakeGeometryCustom(RhinoDoc.ActiveDoc);
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to bake geometries");
                }

                _run = false;
            }
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoBakeStrausIcon;

        public override Guid ComponentGuid => new Guid("1fa3c129-3308-46c8-a529-67d87502956c");
    }
}
