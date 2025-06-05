using System;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Components.Attributes
{
    public class BoundaryGroupComponent : GH_Component
    {
        public BoundaryGroupComponent()
            : base("Boundary Group", "Boundary Group", "Boundary Group", Helper.Constants.PlugInName_Rhino2Fem, Helper.Constants.TabName_Attributes)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Boundary Group Name", "Boundary Group Name", "Boundary Group Name", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Boundary Group", "Boundary Group", "Boundary Group", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";

            if (!DA.GetData(0, ref name))
                return;

            BoundaryGroupModel model = new BoundaryGroupModel(name);
            DA.SetData(0, new GH_BoundaryGroup(model));
        }

        //protected override Bitmap Icon => Resources.support;

        public override Guid ComponentGuid => new Guid("51245ede-46f5-4f47-8cea-d2e038bd9e95");
    }
}