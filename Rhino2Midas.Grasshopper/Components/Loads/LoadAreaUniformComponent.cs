using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Core.Loads;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Loads
{
    public class LoadAreaUniformComponent : GH_Component
    {
        public LoadAreaUniformComponent()
            : base("Area load uniform", "Area load uniform", "Area load uniform", Helper.Constants.Tabname, Helper.Constants.Loads)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Area", "Area", "Area", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Direction", "Direction", "Lx/Ly/Lz/Gx/Gy/Gz", GH_ParamAccess.item, 5);
            foreach (AreaLoadModel.LoadDirections v in Enum.GetValues(typeof(AreaLoadModel.LoadDirections)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddBooleanParameter("Projected?", "Projected?", "Bool", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Load", "Load", "Load", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Area", "Area", "Area", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_AreaElement gH_AreaElement = null;
            GH_LoadCase loadCase = null;
            int direction = 0;
            bool isProjected = false;
            double num = 0.0;

            if (DA.GetData(0, ref gH_AreaElement) && DA.GetData(1, ref loadCase) && DA.GetData(2, ref direction) && DA.GetData(3, ref isProjected) && DA.GetData(4, ref num))
            {
                AreaLoadModel areaLoad = new AreaLoadModel(loadCase.Value, (AreaLoadModel.LoadDirections)direction, isProjected, num, num, num, num);
                AreaElementModel areaElementModel = new AreaElementModel(gH_AreaElement.Value);
                areaElementModel.AreaLoadList.Add(areaLoad);
                DA.SetData(0, new GH_AreaElement(areaElementModel));
            }
        }

        //protected override Bitmap Icon => Resources.load_area_uniform;

        public override Guid ComponentGuid => new Guid("2451D2F2-0466-4A3A-BBC1-96EAD7A9D64D");
    }
}
