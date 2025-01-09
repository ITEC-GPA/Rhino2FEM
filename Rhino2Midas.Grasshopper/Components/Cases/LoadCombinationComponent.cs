using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Cases
{
    public class LoadCombinationComponent : GH_Component
    {
        public LoadCombinationComponent()
            : base("Load combination", "Load combination", "Load combination", Helper.Constants.Tabname, Helper.Constants.Cases)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load factor", "Load factor", "Load factor", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Type", "Type", "Type", GH_ParamAccess.item, 0);
            foreach (LoadCombinationModel.LoadCombinationTypes v in Enum.GetValues(typeof(LoadCombinationModel.LoadCombinationTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddTextParameter("Description", "Description", "Description", GH_ParamAccess.item, "");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load combination", "Load combination", "Load combination", (GH_ParamAccess)0);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string comboName = "";
            List<GH_LoadFactor> GH_loadFactors = new List<GH_LoadFactor>();
            int type = 0;
            string description = "";
            if (!DA.GetData(0, ref comboName) || !DA.GetDataList(1, GH_loadFactors) || !DA.GetData(2, ref type) || !DA.GetData(3, ref description))
                return;

            if (comboName.Length > 20)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Load combination name must be < 20 chars");
                return;
            }

            var loadFactors = new List<LoadFactorModel>();
            for (int i = 0; i < GH_loadFactors.Count; i++)
                loadFactors.Add(new LoadFactorModel(GH_loadFactors[i].Value));

            LoadCombinationModel loadCombination = new LoadCombinationModel(comboName, loadFactors, (LoadCombinationModel.LoadCombinationTypes)type, description);
            DA.SetData(0, new GH_LoadCombination(loadCombination));
        }
        //protected override Bitmap Icon => Resources.load_combination;

        public override Guid ComponentGuid => new Guid("A79F6C26-F9D0-4587-8C7F-49C22FC6DC73");
    }
}
