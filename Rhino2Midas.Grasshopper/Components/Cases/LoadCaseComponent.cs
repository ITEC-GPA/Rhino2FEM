using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.Cases
{
    public class LoadCaseComponent : GH_Component
    {
        public LoadCaseComponent()
            : base("Load case", "Load case", "Load case", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_CASES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type", "Type", GH_ParamAccess.item);
            foreach (LoadCaseModel.LoadCaseTypes v in Enum.GetValues(typeof(LoadCaseModel.LoadCaseTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddTextParameter("Description", "Description", "Description", GH_ParamAccess.item, "");
            ((GH_ParamManager)pManager)[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "";
            int type = 0;
            string description = "";
            if (DA.GetData(0, ref text) && DA.GetData(1, ref type) && DA.GetData(2, ref description))
            {
                if (text.Length > 40)
                {
                    AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Load case name must be < 40 chars");
                    return;
                }
                LoadCaseModel loadCase = new LoadCaseModel(text, (LoadCaseModel.LoadCaseTypes)type, description);
                DA.SetData(0, new GH_LoadCase(loadCase));
            }
        }

        //protected override Bitmap Icon => Resources.load_case;

        public override Guid ComponentGuid => new Guid("9C423408-78A2-4EE9-8AF3-8AA6492A3F29");
    }
}
