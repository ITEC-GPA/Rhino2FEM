using System;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;
using Rhino2Midas.Grasshopper.Properties;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyIComponent : GH_Component
    {
        public FramePropertyIComponent()
            : base("Frame property I", "Frame property I", "Frame property I", "Rhino2Midas", "Frame")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Offset", "Offset", "Offset LT/CT/RT/LC/CC/RC/LB/CB/RB", GH_ParamAccess.item, 4);
            foreach (FramePropertyModel.OffsetTypes v in Enum.GetValues(typeof(FramePropertyModel.OffsetTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width top flange", "Width top flange", "Width top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness top flange", "Thickness top flange", "Thickness top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width bottom flange", "Width bottom flange", "Width bottom flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness bottom flange", "Thickness", "Thickness", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius inner", "Radius inner", "Radius inner", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Radius outer", "Radius outer", "Radius outer", GH_ParamAccess.item, 0.0);
            ((GH_ParamManager)pManager)[1].Optional = true;
            ((GH_ParamManager)pManager)[8].Optional = true;
            ((GH_ParamManager)pManager)[9].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int offset = 0;
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

            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref dimension) && DA.GetData(3, ref dimension2) && DA.GetData(4, ref dimension3) &&
                DA.GetData(5, ref dimension4) && DA.GetData(6, ref dimension5) && DA.GetData(7, ref dimension6) && DA.GetData(8, ref dimension7) && DA.GetData(9, ref dimension8))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.H, (FramePropertyModel.OffsetTypes)offset, dimension, dimension2, dimension3, dimension4, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }


        //protected override Bitmap Icon => Resources.frame_property_I;

        public override Guid ComponentGuid => new Guid("2249ca75-767d-40fb-8693-301e3d5b0795");
    }
}
