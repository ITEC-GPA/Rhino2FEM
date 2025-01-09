using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyAngleComponent : GH_Component
    {
        public FramePropertyAngleComponent()
            : base("Frame property angle", "Frame property angle", "Frame property angle", Helper.Constants.Tabname, Helper.Constants.ElementProperties)
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
            pManager.AddNumberParameter("Thickness flange", "Thickness flange", "Thickness flange", GH_ParamAccess.item);
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
            double dimension3 = 0.0;
            double dimension4 = 0.0;
            double dimension5 = 0.0;
            double dimension6 = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;

            int count = 0;
            if (DA.GetData(count++, ref name))
                return;
            if (DA.GetData(count++, ref offset))
                return;
            if (DA.GetData(count++, ref dimension))
                return;
            if (DA.GetData(count++, ref dimension2))
                return;
            if (DA.GetData(count++, ref dimension3))
                return;
            if (DA.GetData(count++, ref dimension4))
                return;

            FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.L, (FramePropertyModel.OffsetTypes)offset, dimension, dimension2,
                dimension3, dimension4, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
            frameProperty.Number = number;
            DA.SetData(0, new GH_FrameProperty(frameProperty));
        }

        //protected override Bitmap Icon => Resources.frame_property_angle;

        public override Guid ComponentGuid => new Guid("f33d4f9b-d5bf-4e18-ba11-cfba645db396");
    }
}