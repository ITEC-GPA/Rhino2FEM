using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.Models
{
    public class DecomposeModelComponent : GH_Component
    {
        public DecomposeModelComponent()
            : base("Decompose Model", "Decompose Model", "Decompose Model", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_MODEL)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node elements", "Node elements", "Node elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Frame elements", "Frame elements", "Frame elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Area elements", "Area elements", "Area elements", GH_ParamAccess.list);
            pManager.AddGenericParameter("Link elements", "Link elements", "Link elements", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Model gH_Model = new GH_Model();
            int count = 0;
            if (!DA.GetData(count++, ref gH_Model))
                return;

            List<GH_NodeElement> gh_Nodes = new List<GH_NodeElement>(gH_Model.Value.NodeElements.Count);
            List<GH_FrameElement> gh_Frames = new List<GH_FrameElement>(gH_Model.Value.FrameElements.Count);
            List<GH_AreaElement> gH_Areas = new List<GH_AreaElement>(gH_Model.Value.AreaElements.Count);
            List<GH_LinkElement> gH_Links = new List<GH_LinkElement>(gH_Model.Value.LinkElements.Count);

            for (int i = 0; i < gH_Model.Value.NodeElements.Count; i++)
                gh_Nodes.Add(new GH_NodeElement(gH_Model.Value.NodeElements.ElementAt(i).Value));
            for (int i = 0; i < gH_Model.Value.FrameElements.Count; i++)
                gh_Frames.Add(new GH_FrameElement(gH_Model.Value.FrameElements.ElementAt(i).Value));
            for (int i = 0; i < gH_Model.Value.AreaElements.Count; i++)
                gH_Areas.Add(new GH_AreaElement(gH_Model.Value.AreaElements.ElementAt(i).Value));
            for (int i = 0; i < gH_Model.Value.LinkElements.Count; i++)
                gH_Links.Add(new GH_LinkElement(gH_Model.Value.LinkElements.ElementAt(i).Value));

            DA.SetDataList(0, gh_Nodes);
            DA.SetDataList(1, gh_Frames);
            DA.SetDataList(2, gH_Areas);
            DA.SetDataList(3, gH_Links);
        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("5d5d4ffc-5270-420b-9847-0b55fba78fe2");
    }
}
