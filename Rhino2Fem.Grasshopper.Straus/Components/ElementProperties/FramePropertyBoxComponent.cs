using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FramePropertyBoxComponent : GH_Component
    {
        public FramePropertyBoxComponent()
            : base("Frame property box", "Frame property box", "Frame property box", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness flange top", "Thickness flange top", "Thickness flange top", GH_ParamAccess.item);
            pManager.AddNumberParameter("Center to center web", "Center to center web", "Center to center web", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Thickness flange bottom", "Thickness flange bottom", "Thickness flange bottom", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            double h = 0.0;
            double w = 0.0;
            double tw = 0.0;
            double tft = 0.0;
            double tfb = 0.0;
            double ctcw = 0.0;
            double dimension5 = 0.0;
            double dimension6 = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref h) && DA.GetData(2, ref w) && DA.GetData(3, ref tw) && DA.GetData(4, ref tft) && DA.GetData(5, ref tfb) && DA.GetData(6, ref ctcw))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.B, FramePropertyModel.Types.DBUSER, FramePropertyModel.OffsetTypes.CC,
                    h, w, tw, tft, tfb, ctcw, dimension5, dimension6, dimension7, dimension8);
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_box;

        public override Guid ComponentGuid => new Guid("3ebdcc34-738e-499e-a764-ae87bc440608");
    }
}