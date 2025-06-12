using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FrameSectionTeeComponent : GH_Component
    {
        public FrameSectionTeeComponent()
            : base("Frame Section tee", "Frame Section tee", "Frame Section tee", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Height", "Height", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness web", "Thickness web", "Thickness web", GH_ParamAccess.item);
            pManager.AddNumberParameter("Thickness flange", "Thickness flange", "Thickness flange", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame Section", "Frame Section", "Frame Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            double dimension = 0.0;
            double dimension2 = 0.0;
            double dimension3 = 0.0;
            double dimension4 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref dimension) && DA.GetData(2, ref dimension2) &&
                DA.GetData(3, ref dimension3) && DA.GetData(4, ref dimension4))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.T, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC,
                    dimension, dimension2, dimension3, dimension4);
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_tee;

        public override Guid ComponentGuid => new Guid("39510213-ea4b-43d9-8e48-5c2861f8b102");
    }
}