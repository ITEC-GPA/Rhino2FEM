using System;
using Grasshopper.Kernel;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Core.Loads;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Cases
{
    public class SelfWeightComponent : GH_Component
    {
        public SelfWeightComponent()
            : base("Selfweight", "Selfweight", "Selfweight", Helper.Constants.Tabname, Helper.Constants.Cases)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddNumberParameter("Factor x", "Factor x", "Factor x", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Factor y", "Factor y", "Factor y", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Factor z", "Factor z", "Factor z", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Selfweight", "Selfweight", "Selfweight", (GH_ParamAccess)0);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            LoadCaseModel loadCase = null;
            double factorX = 0.0;
            double factorY = 0.0;
            double factorZ = 0.0;

            if (DA.GetData(0, ref loadCase) && DA.GetData(1, ref factorX) && DA.GetData(2, ref factorY) && DA.GetData(3, ref factorZ))
            {
                SelfWeightModel selfWeight = new SelfWeightModel(loadCase, factorX, factorY, factorZ);
                DA.SetData(0, new GH_SelfWeight(selfWeight));
            }
        }

        //protected override Bitmap Icon => Resources.selfweight;

        public override Guid ComponentGuid => new Guid("5460A21E-02BA-4045-92DD-BA10E69BEF12");
    }
}
