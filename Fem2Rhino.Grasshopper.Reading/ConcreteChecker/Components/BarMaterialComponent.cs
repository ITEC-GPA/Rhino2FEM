using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Data.Steel;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class BarMaterialComponent : GH_Component
    {
        protected Dictionary<int, string> _steelList = new Dictionary<int, string>() {
            { 0, "Y1180C" },
            { 1, "Y1240C" },
            { 2, "Y1280C" },
            { 3, "Y1370C" },
            { 4, "Y1420C" },
            { 5, "Y1470C" },
        };

        public BarMaterialComponent()
            : base("Database Bar Steel", "DBS", "Database Bar Steel", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            int i = pManager.AddIntegerParameter("Type", "T", "The property type", GH_ParamAccess.item, 0);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _steelList)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Bar Steel Material", "BS", "Bar Steel Material", GH_ParamAccess.item);
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
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1180C));
                else if (type == 1)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1240C));
                else if (type == 2)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1280C));
                else if (type == 3)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1370C));
                else if (type == 4)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1420C));
                else if (type == 5)
                    DA.SetData(0, new GH_SteelMaterial(SteelMaterialEN1992Data.Y1470C));
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to get data");
                    return;
                }
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("23875d38-d43a-4051-b19c-a6ae39139f7b");
    }
}
