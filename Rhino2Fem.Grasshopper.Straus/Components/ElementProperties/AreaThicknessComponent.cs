using Grasshopper.Kernel;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Grasshopper.Datatype;
using System;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class AreaThicknessComponent : GH_Component
    {
        public AreaThicknessComponent()
            : base("Area thickness", "Area thickness", "Area thickness", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area thickness", "Area thickness", "Area thickness", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            double thickness = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref thickness))
            {
                AreaThicknessModel areaThickness = new AreaThicknessModel(name, thickness);
                DA.SetData(0, new GH_AreaThickness(areaThickness));
            }
        }

        //protected override Bitmap Icon => Resources.area_thickness;

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        public override Guid ComponentGuid => new Guid("26b3ac8a-fd84-4968-bb61-a3b1963382b8");
    }
}
