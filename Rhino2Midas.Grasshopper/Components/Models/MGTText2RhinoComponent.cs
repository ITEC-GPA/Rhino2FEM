using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Models;
using Rhino2Fem.Grasshopper.Datatype;

namespace Rhino2Fem.Midas.Straus.Components.Models
{
    public class MGTText2RhinoComponent : GH_Component
    {
        public MGTText2RhinoComponent()
            : base("MGT2Rhino", "MGT2Rhino", "MGT2Rhino", Core.Helper.Constants.CATEGORY_RHINO2MIDAS, Core.Helper.Constants.SUBCATEGORY_MODEL)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "Model", "Model", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> strings = new List<string>();

            if (!DA.GetDataList(0, strings))
                return;

            ModelModel modelModel = ModelModel.CreateFromMgtFile(strings);

            DA.SetData(0, new GH_Model(modelModel));
        }

        //protected override Bitmap Icon => Resources.mgt_model_builder;

        public override Guid ComponentGuid => new Guid("c66ffde2-3a95-4926-8feb-3f43212f95bd");
    }
}
