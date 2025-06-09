using Grasshopper.Kernel;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Core.Elements;
using Rhino2Fem.Core.Models;
using Rhino2Fem.Grasshopper.Datatype;
using System;
using System.Collections.Generic;

namespace Rhino2Fem.Grasshopper.Straus.Components.Models
{
    public class BuildModelComponent : GH_Component
    {
        public BuildModelComponent()
            : base("Build Model", "Build Model", "Build Model", Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Core.Helper.Constants.SUBCATEGORY_MODEL)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Unit", "Unit", "Unit", GH_ParamAccess.item);
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.list);
            pManager.AddGenericParameter("Frame element", "Frame element", "Frame element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Area element", "Area element", "Area element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Link element", "Link element", "Link element", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load combination", "Load combination", "Load combination", GH_ParamAccess.list);
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
            List<GH_LinkElement> gH_Links = new List<GH_LinkElement>();
            List<GH_LoadCombination> gH_LoadCombinations = new List<GH_LoadCombination>();

            int count = 0;
            if (!DA.GetData(count++, ref gH_Units))
                return;
            DA.GetDataList(count++, gh_Nodes);
            DA.GetDataList(count++, gh_Frames);
            DA.GetDataList(count++, gH_Areas);
            DA.GetDataList(count++, gH_Links);
            DA.GetDataList(count++, gH_LoadCombinations);

            ModelModel modelModel = new ModelModel { ModelUnits = gH_Units.Value };

            List<NodeElementModel> nodes = new List<NodeElementModel>(gh_Nodes.Count);
            List<FrameElementModel> frames = new List<FrameElementModel>(gh_Frames.Count);
            List<AreaElementModel> areas = new List<AreaElementModel>(gH_Areas.Count);
            List<LinkElementModel> links = new List<LinkElementModel>(gH_Links.Count);
            List<LoadCombinationModel> combos = new List<LoadCombinationModel>(gH_LoadCombinations.Count);

            for (int i = 0; i < gh_Nodes.Count; i++)
                nodes.Add(gh_Nodes[i].Value);
            for (int i = 0; i < gh_Frames.Count; i++)
                frames.Add(gh_Frames[i].Value);
            for (int i = 0; i < gH_Areas.Count; i++)
                areas.Add(gH_Areas[i].Value);
            for (int i = 0; i < gH_Links.Count; i++)
                links.Add(gH_Links[i].Value);
            for (int i = 0; i < gH_LoadCombinations.Count; i++)
                combos.Add(gH_LoadCombinations[i].Value);

            modelModel.BuildModel(nodes, frames, areas, links, combos, null);

            DA.SetData(0, new GH_Model(modelModel));
        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("e95664b0-8ee6-4db9-a001-517e57bdab93");
    }
}
