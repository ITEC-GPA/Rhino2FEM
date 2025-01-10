using System;
using Grasshopper.Kernel;
using Rhino2Midas.Grasshopper.Datatype;
using System.Collections.Generic;
using Rhino2Midas.Core.Models;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Core.Loads;

namespace Rhino2Midas.Grasshopper.Components.Models
{
    public class BuildModelComponent : GH_Component
    {
        public BuildModelComponent()
            : base("Build Model", "Build Model", "Build Model", Helper.Constants.Tabname, Helper.Constants.Model)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Unit", "Unit", "Unit", GH_ParamAccess.item);
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.list);
            pManager.AddGenericParameter("Frame element", "Frame element", "Frame element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Area element", "Area element", "Area element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load combination", "Load combination", "Load combination", GH_ParamAccess.list);
            pManager.AddGenericParameter("Selfweight", "Selfweight", "Selfweight", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Units gH_Units = new GH_Units();
            List<GH_NodeElement> gh_Nodes = new List<GH_NodeElement>();
            List<GH_FrameElement> gh_Frames = new List<GH_FrameElement>();
            List<GH_AreaElement> gH_Areas = new List<GH_AreaElement>();
            List<GH_LoadCombination> gH_LoadCombinations = new List<GH_LoadCombination>();
            GH_SelfWeight gH_SelfWeight = new GH_SelfWeight();

            int count = 0;
            //if (!DA.GetData(count++, ref gH_Units))
            //    return;
            DA.GetData(count++, ref gH_Units);
            //    return;

            DA.GetDataList(count++, gh_Nodes);
            DA.GetDataList(count++, gh_Frames);
            DA.GetDataList(count++, gH_Areas);
            DA.GetDataList(count++, gH_LoadCombinations);
            DA.GetData(count++, ref gH_SelfWeight);

            ModelModel modelModel = new ModelModel();
            modelModel.ModelUnits = gH_Units.Value;

            List<NodeElementModel> nodes = new List<NodeElementModel>(gh_Nodes.Count);
            List<FrameElementModel> frames = new List<FrameElementModel>(gh_Frames.Count);
            List<AreaElementModel> areas = new List<AreaElementModel>(gH_Areas.Count);
            List<LoadCombinationModel> combos = new List<LoadCombinationModel>(gH_LoadCombinations.Count);

            for (int i = 0; i < gh_Nodes.Count; i++)
                nodes.Add( gh_Nodes[i].Value);
            for (int i = 0; i < gh_Frames.Count; i++)
                frames.Add(gh_Frames[i].Value);
            for (int i = 0; i < gH_Areas.Count; i++)
                areas.Add(gH_Areas[i].Value);
            for (int i = 0; i < gH_LoadCombinations.Count; i++)
                combos.Add(gH_LoadCombinations[i].Value);

            modelModel.BuildModel(nodes, frames, areas, combos, gH_SelfWeight.Value);

            DA.SetData(0, new GH_Model(modelModel));    
        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("ea8dc877-11be-4934-822c-e58058ab81d5");
    }
}
