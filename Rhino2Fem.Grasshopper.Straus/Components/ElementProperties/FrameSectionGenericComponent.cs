using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FrameSectionGenericComponent : GH_Component
    {
        public FrameSectionGenericComponent()
            : base("Frame Section Generic", "Frame Section Generic", "Frame Section Generic", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item, "");
            pManager.AddNumberParameter("Area", "Height", "Height", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("J11", "Width", "Width", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("J22", "Thickness web", "Thickness web", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("J", "Thickness flange top", "Thickness flange top", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shear L1", "Shear L1", "Shear L1", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shear L2", "Shear L2", "Shear L2", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shear A1", "Shear A1", "Shear A1", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shear A2", "Shear A2", "Shear A2", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Centroid x", "Centroid x", "Centroid x", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Centroid y", "Centroid y", "Centroid y", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Angle", "Angle", "Angle", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame Section", "Frame Section", "Frame Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            double area = 0.0;
            double j11 = 0.0;
            double j22 = 0.0;
            double J = 0.0;
            double shearL1 = 0.0;
            double shearL2 = 0.0;
            double shearA1 = 0.0;
            double shearA2 = 0.0;
            double centroidX = 0.0;
            double CentroidY = 0.0;
            double angle = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref area) && DA.GetData(2, ref j11) && DA.GetData(3, ref j22) && DA.GetData(4, ref J) && DA.GetData(5, ref shearL1) && DA.GetData(6, ref shearL2)
                && DA.GetData(7, ref shearA1) && DA.GetData(8, ref shearA2) && DA.GetData(9, ref centroidX) && DA.GetData(10, ref CentroidY) && DA.GetData(11, ref angle))
            {
                FrameSectionModel frameProperty = new FrameSectionModel(name, FrameSectionModel.SectionGeometryTypes.Generic, FrameSectionModel.Types.DBUSER, FrameSectionModel.OffsetTypes.CC)
                {
                    SectionArea = area,
                    I11 = j11,
                    I22 = j22,
                    J = J,
                    ShearL1 = shearL1,
                    ShearL2 = shearL2,
                    ShearA1 = shearA1,
                    ShearA2 = shearA2,
                    Centroid = new Rhino.Geometry.Point2d(centroidX, CentroidY),
                    Angle = angle,
                };
                DA.SetData(0, new GH_FrameSection(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_box;

        public override Guid ComponentGuid => new Guid("204d7ecc-d676-4b29-b2b2-8fa330afdf84");
    }
}