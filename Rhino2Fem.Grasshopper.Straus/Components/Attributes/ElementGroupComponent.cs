using System;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.Attributes
{
    public class ElementGroupComponent : GH_Component
    {
        public ElementGroupComponent()
            : base("Element Group", "Element Group", "Element Group", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ATTRIBUTES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Group Name", "Group Name", "Group Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Group Parent", "Group Parent", "Group Parent", GH_ParamAccess.item);
            pManager[pManager.ParamCount-1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Group", "Group", "Group", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            GH_ElementGroup parent = null;

            if (!DA.GetData(0, ref name))
                return;
            DA.GetData(1, ref parent);

            ElementGroupModel model = new ElementGroupModel(name);
            if(parent != null)            
                model.GroupParent = parent.Value;
            
            DA.SetData(0, new GH_ElementGroup(model));
        }

        //protected override Bitmap Icon => Resources.support;

        public override Guid ComponentGuid => new Guid("ad161a62-810b-4367-8979-e0e7725abb36");
    }
}