using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Midas.Components.ElementProperties
{
    public class FramePropertyBoxComponent : GH_Component
    {
        public FramePropertyBoxComponent()
            : base("Frame property box", "Frame property box", "Frame property box", Constants.CATEGORY_RHINO2MIDAS, Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Offset", "Offset", "Offset LT/CT/RT/LC/CC/RC/LB/CB/RB", GH_ParamAccess.item, 4);
            foreach (FrameSectionModel.OffsetTypes v in Enum.GetValues(typeof(FrameSectionModel.OffsetTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness flange top", "Thickness flange top", "Thickness flange top", GH_ParamAccess.item);
            pManager.AddNumberParameter("Center to center web", "Center to center web", "Center to center web", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Thickness flange bottom", "Thickness flange bottom", "Thickness flange bottom", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int offset = 4; int propNumber = 0;
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

            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref h) && DA.GetData(3, ref w) &&
                DA.GetData(4, ref tw) && DA.GetData(5, ref tft) && DA.GetData(6, ref tfb) && DA.GetData(7, ref ctcw) && DA.GetData(8, ref propNumber))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.B, FrameSectionModel.Types.DBUSER, (FrameSectionModel.OffsetTypes)offset,
                    h, w, tw, tft, tfb, ctcw, dimension5, dimension6, dimension7, dimension8)
                {
                    Id = propNumber
                };
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_box;

        public override Guid ComponentGuid => new Guid("3ebdcc34-738e-499e-a764-ae87bc440608");
    }
}