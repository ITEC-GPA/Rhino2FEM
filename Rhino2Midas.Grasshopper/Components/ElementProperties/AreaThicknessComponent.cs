using System;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Grasshopper.Datatype;
using Rhino2Midas.Grasshopper.Properties;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class AreaThicknessComponent : GH_Component
    {
        public AreaThicknessComponent()
            : base("Area thickness", "Area thickness", "Area thickness", "Rhino2Midas", "Area")
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

        public override Guid ComponentGuid => new Guid("AA3699DB-E573-42A1-8627-5243977EEA40");
    }
}
