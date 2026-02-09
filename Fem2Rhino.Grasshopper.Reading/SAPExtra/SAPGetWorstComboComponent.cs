using Fem2Rhino.CSI;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Helper;
using SAP2000v1;
using System;
using System.Collections.Generic;

namespace Rhino2Fem.Grasshopper.Components.SAPExtra
{
    public class SAPSetDesignComboComponent : GH_Component
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
        public SAPSetDesignComboComponent()
          : base("Set Design Combo", "SDC", "Set design combo for design", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
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
            pManager.AddTextParameter("Design Groups", "G", "List of groups to set", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddTextParameter("Strength Combinations", "C", "List of load case to set", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddTextParameter("Deflection Combinations", "C", "List of load case to set", GH_ParamAccess.list);
            pManager[4].Optional = true;
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
            List<string> groupWanted = new List<string>();
            List<string> strangthComboWanted = new List<string>();
            List<string> deflComboWanted = new List<string>();

            //input
            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            DA.GetDataList(2, groupWanted);
            DA.GetDataList(3, strangthComboWanted);
            DA.GetDataList(4, deflComboWanted);

            if (_run)
            {
                if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sap model path not valid, opening an empty file");
                    return;
                }

                ProxyHelper.CSI.ApiWrapper.SAPApiWrapper wr = new ProxyHelper.CSI.ApiWrapper.SAPApiWrapper();
                if(!wr.InitializeModel(!attachToInstance, "", modelPath, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper))
                {
                    _run = false;
                    Message = "SAP Fail";
                    return;
                }

                int groupsoNumb = 0;
                string[] groups = new string[0];
                if (mySapModel.GroupDef.GetNameList(ref groupsoNumb, ref groups) != 0)
                {
                    _run = false;
                    Message = "Fail";
                    return;
                }

                for (int i = 0; i < groupsoNumb; i++)
                {
                    if (groupWanted.Contains(groups[i]))
                    {
                        if (mySapModel.DesignSteel.SetGroup(groups[i], true) != 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to select {groups[i]}");
                    }
                    else
                    {
                        if (mySapModel.DesignSteel.SetGroup(groups[i], false) != 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to deselect {groups[i]}");
                    }
                }

                int comboNumb = 0;
                string[] combos = new string[0];
                if (mySapModel.RespCombo.GetNameList(ref comboNumb, ref combos) != 0)
                {
                    _run = false;
                    Message = "Fail";
                    return;
                }

                for (int i = 0; i < comboNumb; i++)
                {
                    if (strangthComboWanted.Contains(combos[i]))
                    {
                        if (mySapModel.DesignSteel.SetComboStrength(combos[i], true) != 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to select {combos[i]}");
                    }
                    else
                    {
                        if (mySapModel.DesignSteel.SetComboStrength(combos[i], false) != 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to deselect {combos[i]}");
                    }

                    if (deflComboWanted.Contains(combos[i]))
                    {
                        if (mySapModel.DesignSteel.SetComboDeflection(combos[i], true) != 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to select {combos[i]}");
                    }
                    else
                    {
                        if (mySapModel.DesignSteel.SetComboDeflection(combos[i], false) != 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to deselect {combos[i]}");
                    }
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
        public override Guid ComponentGuid => new Guid("1986ba57-34c1-4385-9a9d-ca75fd39ca3e");
    }
}
