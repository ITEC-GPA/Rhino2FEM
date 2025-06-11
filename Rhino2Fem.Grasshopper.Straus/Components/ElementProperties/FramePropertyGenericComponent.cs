using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Grasshopper.Straus.Components.ElementProperties
{
    public class FramePropertyGenericComponent : GH_Component
    {
        public FramePropertyGenericComponent()
            : base("Frame property Generic", "Frame property Generic", "Frame property Generic", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
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
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }

        //protected override Bitmap Icon => Resources.frame_property_box;

        public override Guid ComponentGuid => new Guid("204d7ecc-d676-4b29-b2b2-8fa330afdf84");
    }
}