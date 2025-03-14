using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Grasshopper.Datatype;
using System;

namespace Rhino2Midas.Grasshopper.Components.Attributes
{
    public class LinkPropertyComponent : GH_Component
    {
        public LinkPropertyComponent()
            : base("Link Type", "Link Type", "Link Type", Helper.Constants.Rhino2Midas, Helper.Constants.Attributes)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Type", "Type", "Rigid/Elastic", GH_ParamAccess.item, 5);
            foreach (LinkPropertyModel.LinkPropertyTypes v in Enum.GetValues(typeof(LinkPropertyModel.LinkPropertyTypes)))
                ((Param_Integer)pManager[pManager.ParamCount - 1]).AddNamedValue(v.GetDescription(), (int)v);
            pManager.AddNumberParameter("Kx", "Kx", "Kx", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Ky", "Ky", "Ky", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Kz", "Kz", "Kz", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Rx", "Rx", "Rx", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Ry", "Ry", "Ry", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Rz", "Rz", "Rz", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("Boundary Group", "Boundary Group", "Boundary Group", GH_ParamAccess.item);
            pManager[pManager.ParamCount - 1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Link Type", "Link Type", "Link Type", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int type = 0;
            double dx = 0;
            double dy = 0;
            double dz = 0;
            double mx = 0;
            double my = 0;
            double mz = 0;
            GH_BoundaryGroup boundary = null;

            if (!DA.GetData(0, ref type))
                return;

            if (DA.GetData(1, ref dx) && DA.GetData(2, ref dy) && DA.GetData(3, ref dz) && DA.GetData(4, ref mx) && DA.GetData(5, ref my) && DA.GetData(6, ref mz))
            {
                DA.GetData(7, ref boundary);

                LinkPropertyModel linkProperty = new LinkPropertyModel((LinkPropertyModel.LinkPropertyTypes)type, dx, dy, dz, mx, my, mz);
                if (boundary != null)
                    linkProperty.BoundaryGroup = boundary.Value;
                DA.SetData(0, new GH_LinkProperty(linkProperty));
            }
        }

        //protected override Bitmap Icon => Resources.support;

        public override Guid ComponentGuid => new Guid("8ce029fc-b982-4b8f-877b-06d53b2a813c");

        public override GH_Exposure Exposure => GH_Exposure.secondary;
    }
}