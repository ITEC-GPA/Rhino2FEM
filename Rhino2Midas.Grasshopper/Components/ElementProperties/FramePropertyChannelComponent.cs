using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyChannelComponent : GH_Component
    {
        public FramePropertyChannelComponent()
            : base("Frame property channel", "Frame property channel", "Frame property channel", Helper.Constants.Rhino2Midas, Helper.Constants.ElementProperties)
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
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int offset = 4; int number = 0;
            double h = 0.0;
            double wtf = 0.0;
            double wt = 0.0;
            double ttf = 0.0;
            double wbf = 0.0;
            double tbf = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;
            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref h) && DA.GetData(3, ref wtf) &&
                DA.GetData(4, ref wt) && DA.GetData(5, ref ttf) && DA.GetData(6, ref wbf) && DA.GetData(7, ref tbf) && DA.GetData(8, ref number))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.C, FramePropertyModel.Types.DBUSER, (FramePropertyModel.OffsetTypes)offset,
                    h, wtf, wt, ttf, wbf, tbf, dimension7, dimension8, dimension9, dimension10);
                frameProperty.Id = number;
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_channel;

        public override Guid ComponentGuid => new Guid("fb8be93a-f263-4d4c-9d47-3d7368efd9ff");
    }
}