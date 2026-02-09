using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class TendonMaterialComponent : GH_Component
    {
        protected Dictionary<int, string> _steelList = new Dictionary<int, string>() {
            { 0, "Y1570C" },
            { 1, "Y1620C" },
            { 2, "Y1670C" },
            { 3, "Y1770C" },
            { 4, "Y1860C" },
            { 5, "Y1960C" },
            { 6, "Y2060C" },
            { 7, "Y2160C" },
            { 8, "Y2260C" },
            { 9, "Y2360C" },
        };

        public TendonMaterialComponent()
            : base("Database Tendon Steel", "Database Tendon Steel", "Database Tendon Steel", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER   )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item, 4);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _steelList)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Tendon Steel Material", "TS", "Tendon Steel Material", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int type = 0;

            if (DA.GetData(0, ref type))
            {
                if (!_steelList.ContainsKey(type))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid type");
                    return;
                }
                if (type == 0)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1570C));
                else if (type == 1)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1620C));
                else if (type == 2)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1670C));
                else if (type == 3)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1770C));
                else if (type == 4)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1860C));
                else if (type == 5)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1960C));
                else if (type == 6)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y2060C));
                else if (type == 7)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y2160C));
                else if (type == 8)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y2260C));
                else if (type == 9)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y2360C));
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get data");
                    return;
                }
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("c709c2fa-5e63-4cf6-b3a2-bd505a149a4d");
    }
}
