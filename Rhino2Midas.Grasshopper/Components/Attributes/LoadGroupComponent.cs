using System;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Components.Attributes
{
    public class LoadGroupComponent : GH_Component
    {
        public LoadGroupComponent()
            : base("Load Group", "Load Group", "Load Group", Helper.Constants.PlugInName_Rhino2Fem, Helper.Constants.TabName_Attributes)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Load Group Name", "Load Group Name", "Load Group Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Suffix", "Suffix", "Suffix", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load Group", "Load Group", "Load Group", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            string suffix = "";

            if (!DA.GetData(0, ref name))
                return;
            if (!DA.GetData(1, ref suffix))
                return;

            LoadGroupModel model = new LoadGroupModel(name, suffix);
            DA.SetData(0, new GH_LoadGroup(model));
        }

        //protected override Bitmap Icon => Resources.support;

        public override Guid ComponentGuid => new Guid("361589b5-4694-460d-8048-2a8753e2494d");
    }
}