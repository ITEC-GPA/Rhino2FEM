using System;
using Grasshopper.Kernel;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Cases
{
    public class LoadFactorComponent : GH_Component
    {
        public LoadFactorComponent()
            : base("Load factor", "Load factor", "Load factor", Helper.Constants.Rhino2Midas, Helper.Constants.Cases)
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

        public override Guid ComponentGuid => new Guid("A38443FF-CFD4-4293-8CE4-655CC3752A06");
    }
}
