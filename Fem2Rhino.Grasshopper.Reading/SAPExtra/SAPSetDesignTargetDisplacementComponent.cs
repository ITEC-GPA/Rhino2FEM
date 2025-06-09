using SAP2000v1;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using Rhino2Fem.Core.Helper;

namespace FeMM.Grasshopper.Components.SAPExtra
{
    public class SAPSetDesignTargetDisplacementComponent : GH_Component
    {
        private bool _run = false;

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Run");
            buttonAttributes.ButtonPressed += () =>
            {
                _run = true;
                ExpireSolution(true);
            };

            m_attributes = buttonAttributes;
        }

        /// <summary>
        /// Initializes a new instance of the SAPAddLoadCase class.
        /// </summary>
        public SAPSetDesignTargetDisplacementComponent()
          : base("Set Design Target Displacement", "SDTD", "Set design lateral displacement targets for steel design", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Model Path", "MP", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager.AddTextParameter("LoadCases", "C", "This is an array that includes the name of the static linear load case associated with each lateral displacement target", GH_ParamAccess.list);
            pManager.AddTextParameter("Points", "P", "This is an array that includes the name of the point object associated to which the lateral displacement target applies.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacements", "D", "This is an array that includes the lateral displacement target", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string modelPath = "";
            bool attachToInstance = false;
            List<string> loadCases = new List<string>();
            List<string> Ids = new List<string>();
            List<double> displs = new List<double>();

            //input
            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            DA.GetDataList(2, loadCases);
            DA.GetDataList(3, Ids);
            DA.GetDataList(4, displs);

            if (_run)
            {
                if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sap model path not valid, opening an empty file");
                    return;
                }

                ProxyHelper.CSI.ApiWrapper.SAPApiWrapper wr = new ProxyHelper.CSI.ApiWrapper.SAPApiWrapper();
                if (!wr.InitializeModel(!attachToInstance, "", modelPath, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper))
                {
                    _run = false;
                    Message = "SAP Fail";
                    return;
                }

                int caseNumb = 0;
                string[] cases = new string[0];
                if (mySapModel.LoadCases.GetNameList(ref caseNumb, ref cases) != 0)
                {
                    _run = false;
                    Message = "Fail";
                    return;
                }

                for (int i = 0; i < caseNumb; i++)
                    if (loadCases.Contains(cases[i]))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Load case {cases[i]} don't exist in SAP model");
                        _run = false;
                        return;
                    }

                double[] dsp = displs.ToArray();
                string[] ss = Ids.ToArray();
                string[] lcs = loadCases.ToArray();

                if (mySapModel.DesignSteel.SetTargetDispl(loadCases.Count, ref lcs, ref ss, ref dsp) != 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set target displacements");
                    _run = false;
                    return;
                }

                _run = false;
                Message = "Done";
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        //protected override System.Drawing.Bitmap Icon => Properties.Resources.SapAddLoadCaseIcon;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("26bed54c-eac0-4c86-80ff-962e8889b2c4");
    }
}
