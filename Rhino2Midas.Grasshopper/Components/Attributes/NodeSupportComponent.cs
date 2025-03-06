using System;
using Grasshopper.Kernel;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Attributes
{
    public class NodeSupportComponent : GH_Component
    {
        public NodeSupportComponent()
            : base("Support", "Support", "Support", Helper.Constants.Rhino2Midas, Helper.Constants.Attributes)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Dx", "Dx", "Dx", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Dy", "Dy", "Dy", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Dz", "Dz", "Dz", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Mx", "Mx", "Mx", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("My", "My", "My", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Mz", "Mz", "Mz", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Boundary Group", "Boundary Group", "Boundary Group", GH_ParamAccess.item);
            pManager[pManager.ParamCount-1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_NodeElement node = null;
            bool dx = false;
            bool dy = false;
            bool dz = false;
            bool mx = false;
            bool my = false;
            bool mz = false;
            GH_BoundaryGroup boundary = null;

            if (!DA.GetData(0, ref node))
                return;

            if (DA.GetData(1, ref dx) && DA.GetData(2, ref dy) && DA.GetData(3, ref dz) && DA.GetData(4, ref mx) && DA.GetData(5, ref my) && DA.GetData(6, ref mz))
            {
                DA.GetData(7, ref boundary);

                NodeSupportModel support = new NodeSupportModel(dx, dy, dz, mx, my, mz);
                NodeElementModel newNode = new NodeElementModel(node.Value);
                if (boundary != null)
                    support.BoundaryGroup = boundary.Value;
                newNode.Support = support;
                DA.SetData(0, new GH_NodeElement(newNode));
            }
        }

        //protected override Bitmap Icon => Resources.support;

        public override Guid ComponentGuid => new Guid("B7A1CFAC-719F-40D0-B93B-2A3D1F9C28B5");
    }
}