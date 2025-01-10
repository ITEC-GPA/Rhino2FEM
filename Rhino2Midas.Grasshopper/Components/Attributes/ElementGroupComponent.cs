using System;
using Grasshopper.Kernel;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Attributes
{
    public class ElementGroupComponent : GH_Component
    {
        public ElementGroupComponent()
            : base("Element Group", "Element Group", "Element Group", Helper.Constants.Tabname, Helper.Constants.Attributes)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Group Name", "Group Name", "Group Name", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Group", "Group", "Group", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";

            if (!DA.GetData(0, ref name))
                return;

            ElementGroupModel model = new ElementGroupModel(name);
            DA.SetData(0, new GH_ElementGroup(model));
        }

        //protected override Bitmap Icon => Resources.support;

        public override Guid ComponentGuid => new Guid("390a1af2-979a-4b72-bee5-c3a78477e41a");
    }
}