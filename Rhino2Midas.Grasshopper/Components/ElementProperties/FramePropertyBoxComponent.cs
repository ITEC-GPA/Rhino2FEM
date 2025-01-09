using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyBoxComponent : GH_Component
    {
        public FramePropertyBoxComponent()
            : base("Frame property box", "Frame property box", "Frame property box", Helper.Constants.Tabname, Helper.Constants.ElementProperties)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Offset", "Offset", "Offset LT/CT/RT/LC/CC/RC/LB/CB/RB", GH_ParamAccess.item, 4);
            foreach (FramePropertyModel.OffsetTypes v in Enum.GetValues(typeof(FramePropertyModel.OffsetTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness flange top", "Thickness flange top", "Thickness flange top", GH_ParamAccess.item);
            pManager.AddNumberParameter("Center to center web", "Center to center web", "Center to center web", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Thickness flange bottom", "Thickness flange bottom", "Thickness flange bottom", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int offset = 4; int number = 0;
            double dimension = 0.0;
            double dimension2 = 0.0;
            double num = 0.0;
            double dimension3 = 0.0;
            double num2 = 0.0;
            double dimension4 = 0.0;
            double dimension5 = 0.0;
            double dimension6 = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref dimension) && DA.GetData(3, ref dimension2) &&
                DA.GetData(4, ref num) && DA.GetData(5, ref dimension3) && DA.GetData(6, ref num2) && DA.GetData(7, ref dimension4) && DA.GetData(8, ref number))
            {
                if (num2 <= num)
                {
                    AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Center to center flange must be > thickness flange!");
                    return;
                }
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.B, (FramePropertyModel.OffsetTypes)offset,
                    dimension, dimension2, num, dimension3, num2, dimension4, dimension5, dimension6, dimension7, dimension8);
                frameProperty.Number = number;
                DA.SetData(0, frameProperty);
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_box;

        public override Guid ComponentGuid => new Guid("3ebdcc34-738e-499e-a764-ae87bc440608");
    }
}