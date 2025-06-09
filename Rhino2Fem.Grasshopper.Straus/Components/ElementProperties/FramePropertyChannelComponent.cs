using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FramePropertyChannelComponent : GH_Component
    {
        public FramePropertyChannelComponent()
            : base("Frame property channel", "Frame property channel", "Frame property channel", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width top flange", "Width top flange", "Width top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness top flange", "Thickness top flange", "Thickness top flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width bottom flange", "Width bottom flange", "Width bottom flange", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness bottom flange", "Thickness", "Thickness", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            double h = 0.0;
            double topFlangeWidth = 0.0;
            double webThick = 0.0;
            double topFlangeThick = 0.0;
            double bottomFlangeWidth = 0.0;
            double bottomFlangeThick = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;
            if (DA.GetData(0, ref name) && DA.GetData(1, ref h) && DA.GetData(2, ref topFlangeWidth) &&
                DA.GetData(3, ref webThick) && DA.GetData(4, ref topFlangeThick) && DA.GetData(5, ref bottomFlangeWidth) && DA.GetData(7, ref bottomFlangeThick))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.C, FramePropertyModel.Types.DBUSER, FramePropertyModel.OffsetTypes.CC,
                    h, topFlangeWidth, webThick, topFlangeThick, bottomFlangeWidth, bottomFlangeThick, dimension7, dimension8, dimension9, dimension10);
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_channel;

        public override Guid ComponentGuid => new Guid("2615abe4-fea4-4fad-9693-89174d6e3347");
    }
}