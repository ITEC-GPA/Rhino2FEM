using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FrameSectionBoxComponent : GH_Component
    {
        public FrameSectionBoxComponent()
            : base("Frame Section box", "Frame Section box", "Frame Section box", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness flange top", "Thickness flange top", "Thickness flange top", GH_ParamAccess.item);
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

            if (DA.GetData(0, ref name) && DA.GetData(1, ref h) && DA.GetData(2, ref w) && DA.GetData(3, ref tw) && DA.GetData(4, ref tft) && DA.GetData(5, ref tfb))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.B, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC,
                    h, w, tw, tft, tfb);
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_box;

        public override Guid ComponentGuid => new Guid("693c6939-1888-40c1-af94-dc5f2c20009f");
    }
}