using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FramePropertyAngleComponent : GH_Component
    {
        public FramePropertyAngleComponent()
            : base("Frame property angle", "Frame property angle", "Frame property angle", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness flange", "Thickness flange", "Thickness flange", GH_ParamAccess.item);
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
            double tf = 0.0;
            double dimension5 = 0.0;
            double dimension6 = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;

            int count = 0;
            if (!DA.GetData(count++, ref name))
                return;
            if (!DA.GetData(count++, ref h))
                return;
            if (!DA.GetData(count++, ref w))
                return;
            if (!DA.GetData(count++, ref tw))
                return;
            if (!DA.GetData(count++, ref tf))
                return;

            FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.L, FramePropertyModel.Types.DBUSER, FramePropertyModel.OffsetTypes.CC,
                h, w, tw, tf, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
            DA.SetData(0, new GH_FrameProperty(frameProperty));
        }

        //protected override Bitmap Icon => Resources.frame_property_angle;

        public override Guid ComponentGuid => new Guid("96a3255f-e896-4403-915f-a846f156643b");
    }
}