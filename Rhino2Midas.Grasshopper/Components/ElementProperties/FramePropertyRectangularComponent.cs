using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyRectangularComponent : GH_Component
    {
        public FramePropertyRectangularComponent()
            : base("Frame property solid rectangular", "Frame property solid rectangular", "Frame property solid rectangular", Helper.Constants.Tabname, Helper.Constants.ElementProperties)
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
            ((GH_ParamManager)pManager)[1].Optional = true;
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int offset = 0; int number = 0;
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
            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref dimension) && DA.GetData(3, ref dimension2) && DA.GetData(4, ref number))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.SB, (FramePropertyModel.OffsetTypes)offset, dimension, dimension2, dimension3, dimension4, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
                frameProperty.Number = number;
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_rectangular;

        public override Guid ComponentGuid => new Guid("8e380f7e-ba42-478c-9cee-96b493ee6970");
    }
}