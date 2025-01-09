using System;
using Grasshopper.Kernel;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Core.Loads;
using Rhino2Midas.Grasshopper.Datatype;

namespace Rhino2Midas.Grasshopper.Components.Loads
{
    public class LoadNodalComponent : GH_Component
    {
        public LoadNodalComponent()
            : base("Load nodal", "Load nodal", "Load nodal", Helper.Constants.Tabname, Helper.Constants.Loads)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
            pManager.AddGenericParameter("Load case", "Load case", "Load case", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fx", "Fx", "Fx", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fy", "Fy", "Fy", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Fz", "Fz", "Fz", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Mx", "Mx", "Mx", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("My", "My", "My", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Mz", "Mz", "Mz", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "Node", "Node", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_NodeElement gH_Node = null;
            GH_LoadCase gH_LoadCase = null;
            double fX = 0.0;
            double fY = 0.0;
            double fZ = 0.0;
            double mX = 0.0;
            double mY = 0.0;
            double mZ = 0.0;

            if (DA.GetData(0, ref gH_Node) && DA.GetData(1, ref gH_LoadCase) && DA.GetData(2, ref fX) && DA.GetData(3, ref fY) && DA.GetData(4, ref fZ) &&
                DA.GetData(5, ref mX) && DA.GetData(6, ref mY) && DA.GetData(7, ref mZ))
            {
                NodalLoadModel nodalLoad = new NodalLoadModel(gH_LoadCase.Value, fX, fY, fZ, mX, mY, mZ);
                NodeElementModel newnode = new NodeElementModel(gH_Node.Value);
                newnode.NodalLoadList.Add(nodalLoad);
                DA.SetData(0, new GH_NodeElement(newnode));
            }
        }

        //protected override Bitmap Icon => Resources.load_node;

        public override Guid ComponentGuid => new Guid("964C547B-3D16-4F94-8B28-5C497278104C");
    }
}
