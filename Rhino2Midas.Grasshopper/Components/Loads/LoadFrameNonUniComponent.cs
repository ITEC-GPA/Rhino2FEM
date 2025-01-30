using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Core.Loads;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Loads
{
    public class LoadFrameNonUniComponent : GH_Component
    {
        public LoadFrameNonUniComponent()
            : base("Load frame non-uniform", "Load frame non-uniform", "Load frame non-uniform", Helper.Constants.Rhino2Midas, Helper.Constants.Loads)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame", "Frame", "Frame", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type (Force or Moment)", "Type (Force or Moment)", GH_ParamAccess.item, 0);
            foreach (FrameLoadModel.FrameLoadTypes v in Enum.GetValues(typeof(FrameLoadModel.FrameLoadTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Direction", "Direction", "Lx/Ly/Lz/Gx/Gy/Gz", GH_ParamAccess.item, 5);
            foreach (FrameLoadModel.LoadDirections v in Enum.GetValues(typeof(FrameLoadModel.LoadDirections)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddBooleanParameter("Projected?", "Projected?", "Bool", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Load at start", "Load at start", "Load at start", GH_ParamAccess.item);
            pManager.AddNumberParameter("Load at end", "Load at end", "Load at end", GH_ParamAccess.item);
            pManager.AddNumberParameter("Location relative start", "Location relative start", "Location relative start", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Location relative end", "Location relative end", "Location relative end", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame load", "Frame load", "Frame load", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_FrameElement gH_FrameElement = null;
            GH_LoadCase loadCase = null;
            int type = 0;
            int direction = 0;
            bool isProjected = false;
            double startLoad = 0.0;
            double endLoad = 0.0;
            double num = 0.0;
            double num2 = 0.0;

            if (DA.GetData(0, ref gH_FrameElement) && DA.GetData(1, ref loadCase) && DA.GetData(2, ref type) &&
                DA.GetData(3, ref direction) && DA.GetData(4, ref isProjected) && DA.GetData(5, ref startLoad) &&
                DA.GetData(6, ref endLoad) && DA.GetData(7, ref num) && DA.GetData(8, ref num2))
            {
                if (num >= num2)
                {
                    AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Location start must be > location end!");
                    return;
                }
                if (num < 0.0 || num2 < 0.0)
                {
                    AddRuntimeMessage((GH_RuntimeMessageLevel)20, "Location start and end must be >= 0!");
                    return;
                }
                FrameLoadModel frameLoad = new FrameLoadModel(loadCase.Value, (FrameLoadModel.FrameLoadTypes)type, (FrameLoadModel.LoadDirections)direction,
                    isProjected, num, startLoad, num2, endLoad);
                FrameElementModel frameElementModel = new FrameElementModel(gH_FrameElement.Value);
                frameElementModel.FrameLoadList.Add(frameLoad);
                DA.SetData(0, new GH_FrameElement(frameElementModel));
            }
        }

        //protected override Bitmap Icon => Resources.load_frame_nonuniform;

        public override Guid ComponentGuid => new Guid("E3474B6C-2432-4DD0-BF32-B3C008480C8A");
    }
}
