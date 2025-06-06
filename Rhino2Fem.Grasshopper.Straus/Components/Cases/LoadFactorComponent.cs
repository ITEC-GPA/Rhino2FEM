using System;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.Cases
{
    public class LoadFactorComponent : GH_Component
    {
        public LoadFactorComponent()
            : base("Load factor", "Load factor", "Load factor", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_CASES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor", "Factor", "Factor", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load factor", "Load factor", "Load factor", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_LoadCase loadCase = null;
            double factor = 0.0;
            if (DA.GetData(0, ref loadCase) && DA.GetData(1, ref factor))
            {
                LoadFactorModel loadFactor = new LoadFactorModel(loadCase.Value, factor);
                DA.SetData(0, new GH_LoadFactor(loadFactor));
            }
        }
        //protected override Bitmap Icon => Resources.load_factor;

        public override Guid ComponentGuid => new Guid("15f097b1-56a3-4aca-aff8-c9291d9fabb4");
    }
}
