using System;
using Grasshopper.Kernel;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Grasshopper.Components.Export
{
    public class MGTExportComponent : GH_Component
    {
        public MGTExportComponent()
            : base("MGT Export", "MGT Export", "MGT Export", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_EXPORT)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("MGT", "MGT", "MGT", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Model gH_Model = new GH_Model();

            if (!DA.GetData(0, ref gH_Model))
                return;

            if (gH_Model.Value != null)
            {
                var model = gH_Model.Value;
                var lines = model.CreateMgtFile();
                DA.SetDataList(0, lines);
            }
        }

        //protected override Bitmap Icon => Resources.frame_element;

        public override Guid ComponentGuid => new Guid("55aba149-7646-456a-877c-4b7f647a1208");
    }
}