using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Elements;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Core.Loads;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.Loads
{
    public class LoadFrameUniformComponent : GH_Component
    {
        public LoadFrameUniformComponent()
            : base("Load frame uniform", "Load frame uniform", "Load frame uniform", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_LOADS)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame", "Frame", "Frame", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "Type (Force or Moment)", "Type (Force or Moment)", GH_ParamAccess.item, 0);
            foreach (FrameLoadMidasModel.FrameLoadTypes v in Enum.GetValues(typeof(FrameLoadMidasModel.FrameLoadTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddIntegerParameter("Direction", "Direction", "Lx/Ly/Lz/Gx/Gy/Gz", GH_ParamAccess.item, 5);
            foreach (FrameLoadMidasModel.LoadDirections v in Enum.GetValues(typeof(FrameLoadMidasModel.LoadDirections)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddBooleanParameter("Projected?", "Projected?", "Bool", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Load", "Load", "Load", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load Group", "Load Group", "Load Group", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Frame", "Frame", "Frame", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_FrameElement gH_FrameElement = null;
            GH_LoadCase loadCase = null;
            int type = 0;
            int direction = 0;
            bool isProjected = false;
            double num = 0.0;
            GH_LoadGroup loadGroup = null;

            if (DA.GetData(0, ref gH_FrameElement) && DA.GetData(1, ref loadCase) && DA.GetData(2, ref type) &&
                DA.GetData(3, ref direction) && DA.GetData(4, ref isProjected) && DA.GetData(5, ref num))
            {
                DA.GetData(6, ref loadGroup);

                FrameLoadMidasModel frameLoad = new FrameLoadMidasModel(loadCase.Value, (FrameLoadMidasModel.FrameLoadTypes)type, (FrameLoadMidasModel.LoadDirections)direction,
                    isProjected, 0.0, num, 1.0, num);
                FrameElementModel frameElementModel = new FrameElementModel(gH_FrameElement.Value);
                if (loadGroup != null)
                    frameLoad.LoadGroup = loadGroup.Value;
                frameElementModel.FrameLoadList.Add(frameLoad);
                DA.SetData(0, new GH_FrameElement(frameElementModel));
            }
        }

        //protected override Bitmap Icon => Resources.load_frame_uniform;

        public override Guid ComponentGuid => new Guid("0A103B67-6611-4173-AB3D-9A7B843CEA9B");
    }
}
