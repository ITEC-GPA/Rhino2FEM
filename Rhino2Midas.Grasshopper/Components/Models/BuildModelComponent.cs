using System;
using Grasshopper.Kernel;

namespace Rhino2Midas.Grasshopper.Components.Models
{
    public class BuildModelComponent : GH_Component
    {
        public BuildModelComponent()
            : base("Build Model", "Build Model", "Build Model", Helper.Constants.Tabname, Helper.Constants.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Unit", "Unit", "Unit", GH_ParamAccess.item);
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.list);
            pManager.AddGenericParameter("Frame element", "Frame element", "Frame element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Area element", "Area element", "Area element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load combination", "Load combination", "Load combination", GH_ParamAccess.list);
            pManager.AddGenericParameter("Selfweight", "Selfweight", "Selfweight", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "v", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("ea8dc877-11be-4934-822c-e58058ab81d5");

    }
}
