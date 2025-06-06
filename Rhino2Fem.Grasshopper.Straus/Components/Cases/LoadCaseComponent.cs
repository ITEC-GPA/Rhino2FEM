using Grasshopper.Kernel;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Grasshopper.Datatype;
using System;

namespace Rhino2Fem.Grasshopper.Straus.Components.Cases
{
    public class LoadCaseComponent : GH_Component
    {
        public LoadCaseComponent()
            : base("Load case", "Load case", "Load case", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_CASES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Gravity NSM", "NSM", "Apply gravity to non-structural mass", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            bool nsm = false;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref nsm))
            {
                LoadCaseModel loadCase = new LoadCaseModel(name, LoadCaseModel.LoadCaseTypes.USER, "", nsm);
                DA.SetData(0, new GH_LoadCase(loadCase));
            }
        }

        //protected override Bitmap Icon => Resources.load_case;

        public override Guid ComponentGuid => new Guid("65f6274a-354a-4965-a40e-73af273912fc");
    }
}
