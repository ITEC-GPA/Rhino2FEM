using System;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;
using Rhino2Midas.Grasshopper.Properties;

namespace Rhino2Midas.Grasshopper.Components.ElementProperties
{
    public class FramePropertyGeneralComponent : GH_Component
    {
        public FramePropertyGeneralComponent()
            : base("Frame property general", "Frame property general", "Frame property general", "Rhino2Midas", "Frame")
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
            ((GH_ParamManager)pManager)[2].Optional = true;
            ((GH_ParamManager)pManager)[3].Optional = true;
            ((GH_ParamManager)pManager)[4].Optional = true;
            ((GH_ParamManager)pManager)[5].Optional = true;
            ((GH_ParamManager)pManager)[6].Optional = true;
            ((GH_ParamManager)pManager)[7].Optional = true;
            ((GH_ParamManager)pManager)[8].Optional = true;
            ((GH_ParamManager)pManager)[9].Optional = true;
            ((GH_ParamManager)pManager)[10].Optional = true;
            ((GH_ParamManager)pManager)[11].Optional = true;
            ((GH_ParamManager)pManager)[12].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame property", "Frame property", "Frame property", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "";
            int text = 0;
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
                !DA.GetData(9, ref dimension7) || !DA.GetData(10, ref dimension8) || !DA.GetData(11, ref dimension9) || !DA.GetData(12, ref dimension10))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input error");
                return;
            }

            FramePropertyModel frameProperty = new FramePropertyModel(name, (FramePropertyModel.FramePropertyTypes)text, (FramePropertyModel.OffsetTypes)offset,
                dimension, dimension2, dimension3, dimension4, dimension5, dimension6, dimension7, dimension8, dimension9, dimension10);
            DA.SetData(0, new GH_FrameProperty(frameProperty));
        }

        //protected override Bitmap Icon => Resources.frame_property_general;

        public override Guid ComponentGuid => new Guid("ea69b9df-419b-4466-a96f-e34949739426");
    }
}