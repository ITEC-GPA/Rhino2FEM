using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyIComponent : GH_Component
    {
        public FramePropertyIComponent()
            : base("Frame property I", "Frame property I", "Frame property I", Helper.Constants.Rhino2Midas, Helper.Constants.ElementProperties)
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
            pManager.AddNumberParameter("Thickness top flange", "Thickness top flange", "Thickness top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width bottom flange", "Width bottom flange", "Width bottom flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness bottom flange", "Thickness", "Thickness", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius inner", "Radius inner", "Radius inner", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Radius outer", "Radius outer", "Radius outer", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int offset = 0; int number = 0;
            double h = 0.0;
            double b1 = 0.0;
            double tw = 0.0;
            double tf1 = 0.0;
            double b2 = 0.0;
            double tf2 = 0.0;
            double r1 = 0.0;
            double r2 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref h) && DA.GetData(3, ref b1) && DA.GetData(4, ref tf1) &&
                DA.GetData(5, ref b2) && DA.GetData(6, ref tf2) && DA.GetData(7, ref tw) && DA.GetData(8, ref r1) && DA.GetData(9, ref r2) && DA.GetData(10, ref number))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.H, (FramePropertyModel.OffsetTypes)offset, h, b1, tw, tf1, b2, tf2, r1, r2, dimension9, dimension10);
                frameProperty.Id = number;
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }


        //protected override Bitmap Icon => Resources.frame_property_I;

        public override Guid ComponentGuid => new Guid("2249ca75-767d-40fb-8693-301e3d5b0795");
    }
}
