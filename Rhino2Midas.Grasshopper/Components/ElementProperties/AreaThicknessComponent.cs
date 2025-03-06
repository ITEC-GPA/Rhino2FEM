using System;
using Grasshopper.Kernel;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class AreaThicknessComponent : GH_Component
    {
        public AreaThicknessComponent()
            : base("Area thickness", "Area thickness", "Area thickness", Helper.Constants.Rhino2Midas, Helper.Constants.ElementProperties)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness", "Thickness", "Thickness", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset", "Offset", "Offset", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area thickness", "Area thickness", "Area thickness", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int number = 0;
            double thickness = 0.0;
            double offset = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref thickness) && DA.GetData(2, ref offset) && DA.GetData(3, ref number))
            {
                AreaThicknessModel areaThickness = new AreaThicknessModel(name, thickness) { Offset = offset, Number = number };                
                DA.SetData(0, new GH_AreaThickness(areaThickness));
            }
        }

        //protected override Bitmap Icon => Resources.area_thickness;

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public override Guid ComponentGuid => new Guid("AA3699DB-E573-42A1-8627-5243977EEA40");
    }
}
