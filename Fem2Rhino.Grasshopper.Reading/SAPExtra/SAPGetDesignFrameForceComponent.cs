using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino2Fem.Core.Helper;

namespace Rhino2Fem.Grasshopper.Components.SAPExtra
{
    public class SAPGetDesignFrameForceComponent : GH_Component
    {
        bool _run;

        /// <summary>
        /// Initializes a new instance of the SAPAreaLoaderComponent class.
        /// </summary>
        public SAPGetDesignFrameForceComponent()
          : base("SAP2000 Design Frame Force", "FFR", "Read the frame design forces for selected combinations", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
        {
            _run = false;
        }

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes attr = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Run");
            attr.ButtonPressed += () =>
            {
                _run = true;
                ExpireSolution(true);
            };
            m_attributes = attr;
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
            pManager.AddTextParameter("SapExe", "S", "sap2000 exe fullpath", GH_ParamAccess.item, "");
            pManager[2].Optional = true;
            pManager.AddTextParameter("Groups", "G", "Groups selected for output. If empty, all beams selected in design group are exported", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddTextParameter("Load cases", "LC", "Load cases selected for output. If empty, cases selected in SAP design combo are exported", GH_ParamAccess.list);
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Beam IDs", "BI", "The beam element IDs", GH_ParamAccess.list);
            pManager.AddNumberParameter("Beam stations", "BS", "The beam element stations", GH_ParamAccess.tree);
            pManager.AddTextParameter("Combos", "LC", "The load cases or the load combinations", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Axial Force", "N", "The beam element results", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Shear 2", "V2", "The beam element results", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Shear 3", "V3", "The beam element results", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Torsion", "T", "The beam element results", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Bending 2", "M2", "The beam element results", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Bending 3", "M3", "The beam element results", GH_ParamAccess.tree);
            pManager.AddTextParameter("Step Type", "ST", "The beam element results", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string sapModel = "";
            bool attach = false;
            string sapExe = "";
            List<string> groups = new List<string>();
            List<string> loadCases = new List<string>();

            DA.GetData(0, ref sapModel);
            DA.GetData(1, ref attach);
            DA.GetData(2, ref sapExe);
            DA.GetDataList(3, groups);
            DA.GetDataList(4, loadCases);

            if (_run)
            {
                if (!System.IO.File.Exists(sapModel) && !attach) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sap model path not valid, opening an empty file");
                    _run = false;
                    return;
                }

                ProxyHelper.CSI.ApiWrapper.SAPApiWrapper wr = new ProxyHelper.CSI.ApiWrapper.SAPApiWrapper();
                wr.InitializeModel(!attach, sapExe, sapModel, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper);

                if (mySapObject != null && mySapModel != null)
                {
                    if (!mySapModel.GetModelIsLocked())
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please run the analysis");
                        _run = false;
                        return;
                    }
                }

                eUnits units = mySapModel.GetPresentUnits();
                Message = units.ToString();

                // Lista degli ID delle travi di cui richiedere i risultati
                HashSet<string> eulerBeams = new HashSet<string>();

                if (groups.Count > 0)
                {
                    int groupNumber = 0;
                    string[] groupNames = new string[0];
                    mySapModel.GroupDef.GetNameList(ref groupNumber, ref groupNames);

                    for (int i = 0; i < groupNumber; i++)
                        mySapModel.DesignSteel.SetGroup(groupNames[i], false);

                    for (int i = 0; i < groups.Count; i++)
                        mySapModel.DesignSteel.SetGroup(groups[i], true);
                }

                int groupNumberOutput = 0;
                string[] groupNamesOutput = new string[0];
                mySapModel.DesignSteel.GetGroup(ref groupNumberOutput, ref groupNamesOutput);

                if (groupNumberOutput == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No groups selected");
                    _run = false;
                    return;
                }

                for (int i = 0; i < groupNumberOutput; i++)
                {
                    int numberItems = 0;
                    int[] objectType = new int[0];
                    string[] objectName = new string[0];

                    mySapModel.GroupDef.GetAssignments(groupNamesOutput[i], ref numberItems, ref objectType, ref objectName);

                    for (int j = 0; j < numberItems; j++)
                    {
                        // objectType == Frame object
                        if (objectType[j] == 2)
                        {
                            eulerBeams.Add(objectName[j]);
                        }
                    }
                }

                if (eulerBeams.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No beams in selected groups");
                    _run = false;
                    return;
                }

                if (loadCases.Count > 0)
                {
                    int comboNumber = 0;
                    string[] comboNames = new string[0];
                    mySapModel.RespCombo.GetNameList(ref comboNumber, ref comboNames);

                    for (int i = 0; i < comboNumber; i++)
                        mySapModel.DesignSteel.SetComboStrength(comboNames[i], false);

                    for (int i = 0; i < loadCases.Count; i++)
                        mySapModel.DesignSteel.SetComboStrength(loadCases[i], true);
                }


                int beamCount = eulerBeams.Count;

                string[][] loadCase = new string[beamCount][];
                string[][] frameName = new string[beamCount][];
                double[][] objStation = new double[beamCount][];
                double[][] elementStation = new double[beamCount][];
                double[][] N = new double[beamCount][];
                double[][] V2 = new double[beamCount][];
                double[][] V3 = new double[beamCount][];
                double[][] T = new double[beamCount][];
                double[][] M2 = new double[beamCount][];
                double[][] M3 = new double[beamCount][];
                string[][] stepTypes = new string[beamCount][];

                for (int b = 0; b < eulerBeams.Count; b++)
                {
                    string beamName = eulerBeams.ElementAt(b);

                    frameName[b] = new string[0];
                    objStation[b] = new double[0];
                    elementStation[b] = new double[0];
                    loadCase[b] = new string[0];
                    int number_result = 0;
                    stepTypes[b] = new string[0];

                    N[b] = new double[0];
                    V2[b] = new double[0];
                    V3[b] = new double[0];
                    T[b] = new double[0];
                    M2[b] = new double[0];
                    M3[b] = new double[0];

                    try
                    {
                        if (mySapModel.DesignResults.DesignForces.BeamDesignForces(beamName, ref number_result, ref frameName[b], ref loadCase[b],
                            ref objStation[b], ref N[b], ref V2[b], ref V3[b], ref T[b], ref M2[b], ref M3[b], eItemType.Objects) != 0)
                        {
                            if (mySapModel.DesignResults.DesignForces.ColumnDesignForces(beamName, ref number_result, ref frameName[b], ref loadCase[b],
                                ref objStation[b], ref N[b], ref V2[b], ref V3[b], ref T[b], ref M2[b], ref M3[b], eItemType.Objects) != 0)
                            {
                                if (mySapModel.DesignResults.DesignForces.BraceDesignForces(beamName, ref number_result, ref frameName[b], ref loadCase[b],
                                    ref objStation[b], ref N[b], ref V2[b], ref V3[b], ref T[b], ref M2[b], ref M3[b], eItemType.Objects) != 0)
                                {
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to read results for beam {beamName}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to read results for beam {beamName}: {ex.Message}");
                    }
                }


                DataTree<double> stationPositionsDataTree = new DataTree<double>();
                for (int i = 0; i < objStation.GetLength(0); i++)
                    stationPositionsDataTree.AddRange(objStation[i], new GH_Path(i));

                DataTree<string> loadCaseDataTree = new DataTree<string>();
                for (int i = 0; i < loadCase.GetLength(0); i++)
                    loadCaseDataTree.AddRange(loadCase[i], new GH_Path(i));

                DataTree<double> NDataTree = new DataTree<double>();
                for (int i = 0; i < N.GetLength(0); i++)
                    NDataTree.AddRange(N[i], new GH_Path(i));

                DataTree<double> V2DataTree = new DataTree<double>();
                for (int i = 0; i < V2.GetLength(0); i++)
                    V2DataTree.AddRange(V2[i], new GH_Path(i));

                DataTree<double> V3DataTree = new DataTree<double>();
                for (int i = 0; i < V3.GetLength(0); i++)
                    V3DataTree.AddRange(V3[i], new GH_Path(i));

                DataTree<double> TDataTree = new DataTree<double>();
                for (int i = 0; i < T.GetLength(0); i++)
                    TDataTree.AddRange(T[i], new GH_Path(i));

                DataTree<double> M2DataTree = new DataTree<double>();
                for (int i = 0; i < M2.GetLength(0); i++)
                    M2DataTree.AddRange(M2[i], new GH_Path(i));

                DataTree<double> M3DataTree = new DataTree<double>();
                for (int i = 0; i < M3.GetLength(0); i++)
                    M3DataTree.AddRange(M3[i], new GH_Path(i));

                DataTree<string> stepTypeDataTree = new DataTree<string>();
                for (int i = 0; i < stepTypes.GetLength(0); i++)
                    stepTypeDataTree.AddRange(stepTypes[i], new GH_Path(i));

                DA.SetDataList(0, eulerBeams.ToList());
                DA.SetDataTree(1, stationPositionsDataTree);
                DA.SetDataTree(2, loadCaseDataTree);
                DA.SetDataTree(3, NDataTree);
                DA.SetDataTree(4, V2DataTree);
                DA.SetDataTree(5, V3DataTree);
                DA.SetDataTree(6, TDataTree);
                DA.SetDataTree(7, M2DataTree);
                DA.SetDataTree(8, M3DataTree);
                DA.SetDataTree(9, stepTypeDataTree);
            }

            _run = false;
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPBeamLoaderIcon;

        public override Guid ComponentGuid => new Guid("33a2008c-f64c-4c7b-8bfa-d2a30d3a19f5");
    }
}
