using SAP2000v1;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino2Fem.Core.Helper;

namespace FeMM.Grasshopper.Components.SAPExtra
{
    public class SAPStartDesignComponent : GH_Component
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
        public SAPStartDesignComponent()
          : base("Start Design", "SD", "Start frame design", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
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
            pManager.AddTextParameter("Groups", "G", "List of groups to check", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Hide", "H", "If true, hide the SAP istance", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Checked Beams", "NPBI", "The beam element ID that have been checked", GH_ParamAccess.list);
            pManager.AddTextParameter("Not Passed Beams", "NPBI", "The beam element ID that did not pass the design check", GH_ParamAccess.list);
            pManager.AddTextParameter("Not Checked Beams", "NCBI", "The beam element ID that have not yet been checked", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string modelPath = "";
            bool attachToInstance = false;
            List<string> groupWanted = new List<string>();
            bool hide = false;

            //input
            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            DA.GetDataList(2, groupWanted);
            DA.GetData(3, ref hide);


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

                if (!mySapObject.SapModel.GetModelIsLocked())
                {
                    _run = false;
                    Message = "SAP Fail";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plase run the analysis");
                    return;
                }

                if (hide)
                    mySapObject.Hide();

                for (int i = 0; i < groupWanted.Count; i++)
                {
                    if (mySapModel.SelectObj.Group(groupWanted[i], false) != 0)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to run group {groupWanted[i]}");
                }

                int n = 0;
                int[] objtype = new int[0];
                string[] objName = new string[0];
                if (mySapModel.SelectObj.GetSelected(ref n, ref objtype, ref objName) != 0)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to select checked beams");

                mySapModel.DesignSteel.StartDesign();

                int numb = 0;
                int notPassed = 0;
                int notChecked = 0;
                string[] names = new string[0];
                if (mySapModel.DesignSteel.VerifyPassed(ref numb, ref notPassed, ref notChecked, ref names) != 0)
                {
                    _run = false;
                    Message = "Fail";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to read the design results");
                }

                if (hide)
                    mySapObject.Unhide();

                List<string> checkedBeam = new List<string>();

                var notPassedId = names.Take(notPassed).ToList();
                var notCheckedId = names.Skip(notPassed).Take(notChecked).ToList();


                for (int i = 0; i < n; i++)
                {
                    if (n != 0)
                        if (objtype[i] == 2)                     // Se l'oggetto i è un punto allora aggiungilo
                            checkedBeam.Add(objName[i]);
                }

                DA.SetDataList(0, checkedBeam);
                DA.SetDataList(1, notPassedId);
                DA.SetDataList(2, notCheckedId);

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
        public override Guid ComponentGuid => new Guid("f6bacd07-e0d8-4696-89ef-d5ee623ead90");
    }
}
