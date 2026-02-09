using SAP2000v1;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino2Fem.Core.Helper;

namespace Rhino2Fem.Grasshopper.Components.SAPExtra
{
    public class SAPGetJointDisplacementComponent : GH_Component
    {
        bool _run;

        /// <summary>
        /// Initializes a new instance of the SAPAreaLoaderComponent class.
        /// </summary>
        public SAPGetJointDisplacementComponent()
          : base("SAP2000 Joint Displacements", "JD", "Read the result for selected load case", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
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
            pManager.AddTextParameter("Groups", "G", "Groups selected for output. If empty, all beams are exported", GH_ParamAccess.list, "ALL");
            pManager[3].Optional = true;
            pManager.AddTextParameter("Load cases", "LC", "Load cases selected for output. If empty, cases selected in SAP are exported", GH_ParamAccess.list);
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Joint IDs", "BI", "The joint element IDs", GH_ParamAccess.list);
            pManager.AddTextParameter("Combos", "LC", "The load cases or the load combinations", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U1 Global", "U1 G", "The joint element results in global coordinate system", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U2 Global", "U2 G", "The joint element results in global coordinate system", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U3 Global", "U3 G", "The joint element results in global coordinate system", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U1 Local", "U1 L", "The joint element results in local coordinate system", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U2 Local", "U2 L", "The joint element results in local coordinate system", GH_ParamAccess.tree);
            pManager.AddNumberParameter("U3 Local", "U3 L", "The joint element results in local coordinate system", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Step Type", "ST", "The joint element results", GH_ParamAccess.tree);
            pManager.AddVectorParameter("X Local vector", "X", "The joint coordinate system", GH_ParamAccess.list);
            pManager.AddVectorParameter("Y Local vector", "Y", "The joint coordinate system", GH_ParamAccess.list);
            pManager.AddVectorParameter("Z Local vector", "Z", "The joint coordinate system", GH_ParamAccess.list);
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
                if(!wr.InitializeModel(!attach, sapExe, sapModel, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to initialize SAP");
                    _run = false;
                    return;
                }

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
                HashSet<string> joints = new HashSet<string>();

                if (groups.Count > 0)
                {
                    int groupNumber = 0;
                    string[] groupNames = new string[0];
                    mySapModel.GroupDef.GetNameList(ref groupNumber, ref groupNames);

                    for (int i = 0; i < groups.Count; i++)
                    {
                        int numberItems = 0;
                        int[] objectType = new int[0];
                        string[] objectName = new string[0];

                        mySapModel.GroupDef.GetAssignments(groups[i], ref numberItems, ref objectType, ref objectName);

                        for (int j = 0; j < numberItems; j++)
                        {
                            // objectType == Frame object
                            if (objectType[j] == 1)
                            {
                                joints.Add(objectName[j]);
                            }
                        }
                    }
                }
                else
                {
                    int numberFrames = 0;
                    string[] frameNames = new string[0];
                    mySapModel.PointObj.GetNameList(ref numberFrames, ref frameNames);

                    for (int i = 0; i < numberFrames; i++)
                    {
                        joints.Add(frameNames[i]);
                    }
                }

                if (joints.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No joints in selected groups");
                    _run = false;
                    return;
                }

                int totalLoadPatterns = 0;
                string[] loadPatternNames = new string[0];
                if (mySapModel.LoadPatterns.GetNameList(ref totalLoadPatterns, ref loadPatternNames) == 0)
                {
                    if (mySapModel.DatabaseTables.SetLoadPatternsSelectedForDisplay(ref loadPatternNames) != 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set load patterns for output");
                        _run = false;
                        return;
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set load patterns for output");
                    _run = false;
                    return;
                }

                if (loadCases.Count != 0)
                {
                    //deselect all cases and combos
                    mySapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();

                    //set cases selected for output
                    for (int i = 0; i < loadCases.Count; i++)
                    {
                        if (mySapModel.Results.Setup.SetComboSelectedForOutput($"{loadCases[i]}") != 0)
                            if (mySapModel.Results.Setup.SetCaseSelectedForOutput($"{loadCases[i]}") != 0)
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to set case: {loadCases[i]}");
                    }
                }

                int jointCount = joints.Count;

                int[] number_result = new int[jointCount];
                string[][] loadCase = new string[jointCount][];
                double[][] objStation = new double[jointCount][];
                double[][] elementStation = new double[jointCount][];
                double[][] U1local = new double[jointCount][];
                double[][] U2local = new double[jointCount][];
                double[][] U3local = new double[jointCount][];
                double[][] U1global = new double[jointCount][];
                double[][] U2global = new double[jointCount][];
                double[][] U3global = new double[jointCount][];
                double[][] R1 = new double[jointCount][];
                double[][] R2 = new double[jointCount][];
                double[][] R3 = new double[jointCount][];
                string[][] stepTypes = new string[jointCount][];
                Vector3d[] xLocalVectors = new Vector3d[jointCount];
                Vector3d[] yLocalVectors = new Vector3d[jointCount];
                Vector3d[] zLocalVectors = new Vector3d[jointCount];

                for (int b = 0; b < joints.Count; b++)
                {
                    string jointName = joints.ElementAt(b);

                    string[] obj = new string[0];
                    objStation[b] = new double[0];
                    string[] element = new string[0];
                    elementStation[b] = new double[0];
                    loadCase[b] = new string[0];
                    stepTypes[b] = new string[0];
                    double[] step_num = new double[0];

                    U1local[b] = new double[0];
                    U2local[b] = new double[0];
                    U3local[b] = new double[0];
                    R1[b] = new double[0];
                    R2[b] = new double[0];
                    R3[b] = new double[0];

                    double[] trsf = new double[0];
                    bool isGlobal = true;
                    mySapModel.PointObj.GetTransformationMatrix(jointName, ref trsf, isGlobal);

                    try
                    {
                        if (mySapModel.Results.JointDisplAbs(jointName, eItemTypeElm.ObjectElm, ref number_result[b], ref obj, ref element,
                            ref loadCase[b], ref stepTypes[b], ref step_num, ref U1local[b], ref U2local[b], ref U3local[b], ref R1[b], ref R2[b], ref R3[b]) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to read results for beam {jointName}");
                        }

                        U1global[b] = new double[U1local[b].Length];
                        U2global[b] = new double[U2local[b].Length];
                        U3global[b] = new double[U3local[b].Length];

                        Matrix matrix = new Matrix(3, 3);
                        matrix[0, 0] = trsf[0];
                        matrix[0, 1] = trsf[1];
                        matrix[0, 2] = trsf[2];
                        matrix[1, 0] = trsf[3];
                        matrix[1, 1] = trsf[4];
                        matrix[1, 2] = trsf[5];
                        matrix[2, 0] = trsf[6];
                        matrix[2, 1] = trsf[7];
                        matrix[2, 2] = trsf[8];

                        for (int j = 0; j < number_result[b]; j++)
                        {
                            Matrix U = new Matrix(3, 1);
                            U[0, 0] = U1local[b][j];
                            U[1, 0] = U2local[b][j];
                            U[2, 0] = U3local[b][j];

                            Matrix mtr = matrix * U;

                            U1global[b][j] = mtr[0, 0];
                            U2global[b][j] = mtr[1, 0];
                            U3global[b][j] = mtr[2, 0];
                        }

                        Matrix globalX = new Matrix(3, 1);
                        globalX[0, 0] = 1;
                        globalX[1, 0] = 0;
                        globalX[2, 0] = 0;

                        Matrix globalY = new Matrix(3, 1);
                        globalY[0, 0] = 0;
                        globalY[1, 0] = 1;
                        globalY[2, 0] = 0;

                        Matrix globalZ = new Matrix(3, 1);
                        globalZ[0, 0] = 0;
                        globalZ[1, 0] = 0;
                        globalZ[2, 0] = 1;

                        //matrix.Transpose();

                        Matrix xLocalMatrix = matrix * globalX;
                        Matrix yLocalMatrix = matrix * globalY;
                        Matrix zLocalMatrix = matrix * globalZ;

                        xLocalVectors[b] = new Vector3d(xLocalMatrix[0, 0], xLocalMatrix[1, 0], xLocalMatrix[2, 0]);
                        yLocalVectors[b] = new Vector3d(yLocalMatrix[0, 0], yLocalMatrix[1, 0], yLocalMatrix[2, 0]);
                        zLocalVectors[b] = new Vector3d(zLocalMatrix[0, 0], zLocalMatrix[1, 0], zLocalMatrix[2, 0]);
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to read results for beam {jointName}: {ex.Message}");
                    }
                }

                DataTree<string> loadCaseDataTree = new DataTree<string>();
                for (int i = 0; i < loadCase.GetLength(0); i++)
                    loadCaseDataTree.AddRange(loadCase[i], new GH_Path(i));

                DataTree<double> U1GDataTree = new DataTree<double>();
                for (int i = 0; i < U1global.GetLength(0); i++)
                    U1GDataTree.AddRange(U1global[i], new GH_Path(i));

                DataTree<double> U2GDataTree = new DataTree<double>();
                for (int i = 0; i < U2global.GetLength(0); i++)
                    U2GDataTree.AddRange(U2global[i], new GH_Path(i));

                DataTree<double> U3GDataTree = new DataTree<double>();
                for (int i = 0; i < U3global.GetLength(0); i++)
                    U3GDataTree.AddRange(U3global[i], new GH_Path(i));

                DataTree<double> U1LDataTree = new DataTree<double>();
                for (int i = 0; i < U1local.GetLength(0); i++)
                    U1LDataTree.AddRange(U1local[i], new GH_Path(i));

                DataTree<double> U2LDataTree = new DataTree<double>();
                for (int i = 0; i < U2local.GetLength(0); i++)
                    U2LDataTree.AddRange(U2local[i], new GH_Path(i));

                DataTree<double> U3LDataTree = new DataTree<double>();
                for (int i = 0; i < U3local.GetLength(0); i++)
                    U3LDataTree.AddRange(U3local[i], new GH_Path(i));

                DataTree<string> stepTypeDataTree = new DataTree<string>();
                for (int i = 0; i < stepTypes.GetLength(0); i++)
                    stepTypeDataTree.AddRange(stepTypes[i], new GH_Path(i));

                DA.SetDataList(0, joints.ToList());
                DA.SetDataTree(1, loadCaseDataTree);
                DA.SetDataTree(2, U1GDataTree);
                DA.SetDataTree(3, U2GDataTree);
                DA.SetDataTree(4, U3GDataTree);
                DA.SetDataTree(5, U1LDataTree);
                DA.SetDataTree(6, U2LDataTree);
                DA.SetDataTree(7, U3LDataTree);
                DA.SetDataTree(8, stepTypeDataTree);
                DA.SetDataList(9, xLocalVectors);
                DA.SetDataList(10, yLocalVectors);
                DA.SetDataList(11, zLocalVectors);
            }

            _run = false;
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.NodeDisplacementLoadIcon;

        public override Guid ComponentGuid => new Guid("d61ade2b-9967-4e22-83ec-a6ffcc894aee");
    }
}
