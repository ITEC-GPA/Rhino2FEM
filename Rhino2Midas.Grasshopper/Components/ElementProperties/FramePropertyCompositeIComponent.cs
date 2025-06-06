using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyCompositeIComponent : GH_Component
    {
        public FramePropertyCompositeIComponent()
            : base("Composite I Frame property", "Composite I Frame property", "Composite I Frame property", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
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
            pManager.AddNumberParameter("Slab Width", "Slab Width", "Slab Width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Slab Thickness", "Slab Thickness", "Slab Thickness", GH_ParamAccess.item);
            pManager.AddNumberParameter("Slab Offset", "Slab Offset", "Slab Offset", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Es/Ec", "Es/Ec", "Es/Ec", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ds/Dc", "Ds/Dc", "Ds/Dc", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ps", "Ps", "Ps", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Pc", "Pc", "Pc", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("Ts/Tc", "Ts/Tc", "Ts/Tc", GH_ParamAccess.item, 1.2);
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
            double s1 = 0.0;
            double s2 = 0.0;
            double s3 = 0.0;
            double c1 = 0.0;
            double c2 = 0.0;
            double c3 = 0.0;
            double c4 = 0.0;
            double c5 = 0.0;

            if (DA.GetData(0, ref name) && DA.GetData(1, ref offset) && DA.GetData(2, ref h) && DA.GetData(3, ref b1) && DA.GetData(4, ref tf1) &&
                DA.GetData(5, ref b2) && DA.GetData(6, ref tf2) && DA.GetData(7, ref tw) && DA.GetData(8, ref s1) && DA.GetData(9, ref s2) && DA.GetData(10, ref s3) &&
                DA.GetData(11, ref c1) && DA.GetData(12, ref c2) && DA.GetData(13, ref c3) && DA.GetData(14, ref c4) && DA.GetData(15, ref c5)  && DA.GetData(16, ref number))
            {
                FramePropertyModel frameProperty = new FramePropertyModel(name, FramePropertyModel.FramePropertyTypes.H, FramePropertyModel.Types.COMPOSITE_I, (FramePropertyModel.OffsetTypes)offset,
                    h, tw, b1, tf1, b2, tf2, s1, s2, s3, 0, c1, c2, c3, c4, c5)
                { Id = number, };
                DA.SetData(0, new GH_FrameProperty(frameProperty));
            }
        }


        //protected override Bitmap Icon => Resources.frame_property_I;

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        public override Guid ComponentGuid => new Guid("304d11e0-05d4-4d7c-ae66-0e58e13c0e3e");
    }
}
