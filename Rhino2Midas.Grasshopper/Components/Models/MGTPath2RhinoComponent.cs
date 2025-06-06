using System;
using System.IO;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Models;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Straus.Components.Models
{
    public class MGTPath2RhinoComponent : GH_Component
    {
        public MGTPath2RhinoComponent()
            : base("MGT2Rhino", "MGT2Rhino", "MGT2Rhino", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_MODEL)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "Path", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";

            if (!DA.GetData(0, ref path))
                return;

            var strings = File.ReadAllLines(path, System.Text.Encoding.UTF8);

            ModelModel modelModel = new ModelModel();

            modelModel.ReadMgtFile(strings);

            DA.SetData(0, new GH_Model(modelModel));
        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("14fd86a1-b6ec-41a3-853a-7e1db55b456d");
    }
}
