using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyGeneralComponent : GH_Component
    {
        public FramePropertyGeneralComponent()
            : base("Frame property DBUser", "Frame property DBUser", "Frame property DBUser", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_ELEMENTPROPERTIES)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type L/C/H/T/B/P/SB/SR", "Type", GH_ParamAccess.item);
            foreach (FramePropertyModel.FramePropertyTypes v in Enum.GetValues(typeof(FramePropertyModel.FramePropertyTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Offset", "Offset", "Offset LT/CT/RT/LC/CC/RC/LB/CB/RB", GH_ParamAccess.item, 4);
            foreach (FramePropertyModel.OffsetTypes v in Enum.GetValues(typeof(FramePropertyModel.OffsetTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Dimension 1", "Dimension 1", "Dimension 1", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 2", "Dimension 2", "Dimension 2", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 3", "Dimension 3", "Dimension 3", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 4", "Dimension 4", "Dimension 4", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 5", "Dimension 5", "Dimension 5", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 6", "Dimension 6", "Dimension 6", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 7", "Dimension 7", "Dimension 7", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 8", "Dimension 8", "Dimension 8", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 9", "Dimension 9", "Dimension 9", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dimension 10", "Dimension 10", "Dimension 10", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("Property Number", "Property Number", "Property Number", GH_ParamAccess.item, ModelObjectId.UNASSIGNED);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int text = 0; int number = 0;
            int offset = 0;
            double dimension = 0.0;
            double dimension2 = 0.0;
            double dimension3 = 0.0;
            double dimension4 = 0.0;
            double dimension5 = 0.0;
            double dimension6 = 0.0;
            double dimension7 = 0.0;
            double dimension8 = 0.0;
            double dimension9 = 0.0;
            double dimension10 = 0.0;

            if (!DA.GetData(0, ref name) || !DA.GetData(1, ref text) || !DA.GetData(2, ref offset) || !DA.GetData(3, ref dimension) || !DA.GetData(4, ref dimension2) ||
                !DA.GetData(5, ref dimension3) || !DA.GetData(6, ref dimension4) || !DA.GetData(7, ref dimension5) || !DA.GetData(8, ref dimension6) ||
                !DA.GetData(9, ref dimension7) || !DA.GetData(10, ref dimension8) || !DA.GetData(11, ref dimension9) || !DA.GetData(12, ref dimension10) && DA.GetData(13, ref number))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input error");
                return;
            }

            FramePropertyModel frameProperty = new FramePropertyModel(name, (FramePropertyModel.FramePropertyTypes)text, FramePropertyModel.Types.DBUSER, (FramePropertyModel.OffsetTypes)offset,
                dimension, dimension2, dimension3, dimension4, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
            frameProperty.Id = number;
            DA.SetData(0, new GH_FrameProperty(frameProperty));
        }

        //protected override Bitmap Icon => Resources.frame_property_general;

        public override Guid ComponentGuid => new Guid("ea69b9df-419b-4466-a96f-e34949739426");
    }
}