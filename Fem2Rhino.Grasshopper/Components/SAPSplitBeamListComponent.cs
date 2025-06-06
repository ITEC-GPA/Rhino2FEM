using SAP2000v1;

using Grasshopper.Kernel;
using Rhino;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
    public class SAPSplitBeamListComponent : GH_Component
    {
        private bool _run = false;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public SAPSplitBeamListComponent()
            : base("Split F2R Beam", "SB", "Split the selected beams in SAP2000", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVESAP)
        {
        }

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Split");
            buttonAttributes.ButtonPressed += () =>
            {
                _run = true;
                ExpireSolution(true);
            };

            m_attributes = buttonAttributes;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Model Path", "MP", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Beams", "B", "F2R curve to split", GH_ParamAccess.list);
            pManager.AddTextParameter("Point Splitters", "S", "F2R element used for split", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddTextParameter("Frame Splitters", "S", "F2R element used for split", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddTextParameter("Area Splitters", "S", "F2R element used for split", GH_ParamAccess.list);
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
            List<string> elements = new List<string>();
            List<string> ptSpl = new List<string>();
            List<string> frSpl = new List<string>();
            List<string> plSpl = new List<string>();

            //input
            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            if (!DA.GetDataList(2, elements))
                return;
            DA.GetDataList(3, ptSpl);
            DA.GetDataList(4, frSpl);
            DA.GetDataList(5, plSpl);

            if (_run)
            {

                if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                {
                    modelPath = string.Empty;
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Sap model path not valid, opening an empty file");
                    _run = false;
                }
                if (elements.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No elements selected");
                    _run = false;
                }
                if (ptSpl.Count == 0 && frSpl.Count == 0 && plSpl.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No splitters selected");
                    _run = false;
                }
                try
                {
                    cSapModel sapModel = F2RModelHelper.GetSapModel(out cOAPI sapObject, modelPath, attachToInstance);

                    sapModel.SelectObj.ClearSelection();

                    string nameCustom = (DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond + DateTime.Now.Ticks).ToString();
                    sapModel.GroupDef.SetGroup(nameCustom);

                    RhinoDoc doc = RhinoDoc.ActiveDoc;

                    for (int i = 0; i < ptSpl.Count; i++)
                        sapModel.PointObj.SetGroupAssign(ptSpl[i], nameCustom, false);
                    for (int i = 0; i < frSpl.Count; i++)
                        sapModel.FrameObj.SetGroupAssign(frSpl[i], nameCustom, false);
                    for (int i = 0; i < plSpl.Count; i++)
                        sapModel.AreaObj.SetGroupAssign(plSpl[i], nameCustom, false);

                    for (int i = 0; i < elements.Count; i++)
                    {
                        sapModel.SelectObj.Group(nameCustom, false);

                        int numb = 0;
                        string[] newName = new string[0];
                        if (sapModel.EditFrame.DivideAtIntersections(elements[i], ref numb, ref newName) == 0)
                        {
                            if (numb > 0)
                            {
                                for (int j = 0; j < newName.Length; j++)
                                {
                                    sapModel.FrameObj.ChangeName(newName[j], elements[i] + "-" + (j + 1));
                                }
                            }
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Impossible to divide element n° {i} with ID: {elements[i]}");
                        }
                    }

                    sapModel.GroupDef.Clear(nameCustom);
                    sapModel.GroupDef.Delete(nameCustom);

                    if (attachToInstance)
                        sapModel.View.RefreshView();

                    if (!attachToInstance)
                        sapObject.ApplicationExit(true);

                    _run = false;
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
                    _run = false;
                }
            }
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RSplitBeamIcon;

        public override Guid ComponentGuid => new Guid("b447eae3-8fd3-425d-aebd-da53a89c62c3");
    }
}
