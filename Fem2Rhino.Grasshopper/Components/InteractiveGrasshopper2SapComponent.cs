using SAP2000v1;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
    public class InteractiveGrasshopper2SapComponent : GH_Component
    {
        private bool _run = false;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public InteractiveGrasshopper2SapComponent()
            : base("Grasshopper2SAP", "G2S", "Update SAP interactive model of current open istance", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVEGRASSHOPPER)
        {
        }

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Rhino2SAP");
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
            pManager.AddTextParameter("Model Path", "P", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("Attach to Instance", "A", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Joints", "J", "Joints element", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Joint Attributes", "JA", "Joint Attributes", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddGenericParameter("Frames", "F", "Frames", GH_ParamAccess.list);
            pManager[4].Optional = true;
            pManager.AddGenericParameter("Areas", "A", "Areas", GH_ParamAccess.list);
            pManager[5].Optional = true;
            pManager.AddBooleanParameter("Update groups", "UG", "If true, update the group definitions and group assignment in SAP", GH_ParamAccess.item, false);
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "L", "SAP log", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string modelPath = "";
            bool attachToInstance = false;
            List<object> joints = new List<object>();
            List<object> jointAttributes = new List<object>();
            List<object> frames = new List<object>();
            List<object> areas = new List<object>();
            bool updateGroups = false;

            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            DA.GetDataList(2, joints);
            DA.GetDataList(3, jointAttributes);
            DA.GetDataList(4, frames);
            DA.GetDataList(5, areas);
            DA.GetData(6, ref updateGroups);

            if (_run)
            {
                try
                {
                    if (!File.Exists(modelPath) && !attachToInstance)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Model path don't exist and Attach to Instance is false");
                        _run = false;
                        return;
                    }
                    if (joints.Count != jointAttributes.Count)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"The number of joints and joint attributes must be equal");
                        _run = false;
                        return;
                    }

                    cSapModel SapModel = F2RModelHelper.GetSapModel(out _, modelPath, attachToInstance);

                    if (SapModel.SetModelIsLocked(false) != 0)
                        return;

                    global::Rhino.DocObjects.Tables.LayerTable layers = RhinoDoc.ActiveDoc.Layers;

                    #region Joint Coordinates

                    if (joints.Count > 0)
                    {
                        /// Input per i GetTableForEditingArray
                        int TableVersion = 0;
                        int NumberRecords = 0;

                        /// Input per i SetTableForEditingArray
                        string TableKeyJC_set = "Joint Coordinates";
                        string[] FieldKeysIncludedJC_set = { "Joint", "CoordSys", "CoordType", "XorR", "Y", "T", "Z", "SpecialJt", "GUID" };

                        string[] TableDataJC_set = new string[joints.Count * 9];

                        for (int b = 0; b < joints.Count; b++)
                        {
                            var joint = joints[b];
                            GeometryBase geometry = GH_Convert.ToGeometryBase(joint);
                            GeometryBase geometryAttributes = GH_Convert.ToGeometryBase(jointAttributes[b]);
                            Point point = geometry as Point;

                            if (point != null)
                            {
                                TableDataJC_set[9 * b] = geometryAttributes.UserDictionary.GetString("F2R_ID");
                                TableDataJC_set[9 * b + 1] = "GLOBAL";
                                TableDataJC_set[9 * b + 2] = "Cartesian";
                                TableDataJC_set[9 * b + 3] = point.Location.X.ToString();
                                TableDataJC_set[9 * b + 4] = point.Location.Y.ToString();
                                TableDataJC_set[9 * b + 5] = "0";
                                TableDataJC_set[9 * b + 6] = point.Location.Z.ToString();
                                TableDataJC_set[9 * b + 7] = geometryAttributes.UserDictionary.GetString("F2R_SPECIALJOINT") == "True" ? "Yes" : "No";
                                TableDataJC_set[9 * b + 8] = geometryAttributes.UserDictionary.GetString("F2R_GUID") != null ? geometryAttributes.UserDictionary.GetString("F2R_GUID") : "";
                            }
                            else
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to Joint n: {b}");
                            }
                        }

                        if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyJC_set, ref TableVersion, ref FieldKeysIncludedJC_set, NumberRecords, ref TableDataJC_set) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Joint Coordinates table");
                            _run = false;
                            Message = "Fail";
                            return;
                        }
                    }

                    #endregion

                    #region Connectivity - Frame and Frame Section Assignments

                    if (frames.Count > 0)
                    {
                        string TableKeyCF_set = "Connectivity - Frame";
                        string TableKeyFSA_set = "Frame Section Assignments";
                        string[] FieldKeysIncludedCF_set = { "Frame", "JointI", "JointJ", "IsCurved" };
                        string[] FieldKeysIncludedFSA_set = { "Frame", "AnalSect" };
                        int TableVersion = 0;
                        int NumberRecords = 0;

                        string[] TableDataCF_set = new string[frames.Count * 4];
                        string[] TableDataFSA_set = new string[frames.Count * 2];

                        for (int b = 0; b < frames.Count; b++)
                        {
                            var frame = frames[b];
                            GeometryBase geometry = GH_Convert.ToGeometryBase(frame);

                            TableDataCF_set[4 * b] = geometry.UserDictionary.GetString("F2R_ID");
                            TableDataCF_set[4 * b + 1] = geometry.UserDictionary.GetString("F2R_JOINT1");
                            TableDataCF_set[4 * b + 2] = geometry.UserDictionary.GetString("F2R_JOINT2");
                            TableDataCF_set[4 * b + 3] = "No";

                            TableDataFSA_set[2 * b] = geometry.UserDictionary.GetString("F2R_ID");
                            TableDataFSA_set[2 * b + 1] = geometry.UserDictionary.GetString("F2R_PROPERTY");
                        }
                        if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyCF_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Connectivity - Frame table");
                            _run = false;
                            Message = "Fail";
                            return;
                        }
                        if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyFSA_set, ref TableVersion, ref FieldKeysIncludedFSA_set, NumberRecords, ref TableDataFSA_set) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Frame Section Assignments table");
                            _run = false;
                            Message = "Fail";
                            return;
                        }
                    }

                    #endregion

                    #region Connectivity - Area and Area Section Assignments

                    if (areas.Count > 0)
                    {
                        string TableKeyCA_set = "Connectivity - Area";
                        string TableKeyASA_set = "Area Section Assignments";
                        string[] FieldKeysIncludedCA_set = { "Area", "Joint1", "Joint2", "Joint3", "Joint4", "GUID" };
                        string[] FieldKeysIncludedASA_set = { "Area", "Section", "MatProp" };
                        int TableVersion = 0;
                        int NumberRecords = 0;

                        List<string> TableDataCA_set_string = new List<string>();
                        string[] TableDataCAA_set = new string[areas.Count * 3];

                        for (int b = 0; b < areas.Count; b++)
                        {
                            var area = areas[b];
                            GeometryBase geometry = GH_Convert.ToGeometryBase(area);

                            if (double.TryParse(geometry.UserDictionary.GetString("F2R_VERTICESCOUNT"), out double verticesCount))
                            {
                                for (int c = 0; c < (int)Math.Ceiling(verticesCount / 4); c++)
                                {
                                    TableDataCA_set_string.Add(geometry.UserDictionary.GetString("F2R_ID"));
                                    if (geometry.UserDictionary.ContainsKey($"F2R_JOINT{c * 4 + 1}"))
                                        TableDataCA_set_string.Add(geometry.UserDictionary.GetString($"F2R_JOINT{c * 4 + 1}"));
                                    else
                                        TableDataCA_set_string.Add(null);

                                    if (geometry.UserDictionary.ContainsKey($"F2R_JOINT{c * 4 + 2}"))
                                        TableDataCA_set_string.Add(geometry.UserDictionary.GetString($"F2R_JOINT{c * 4 + 2}"));
                                    else
                                        TableDataCA_set_string.Add(null);

                                    if (geometry.UserDictionary.ContainsKey($"F2R_JOINT{c * 4 + 3}"))
                                        TableDataCA_set_string.Add(geometry.UserDictionary.GetString($"F2R_JOINT{c * 4 + 3}"));
                                    else
                                        TableDataCA_set_string.Add(null);

                                    if (geometry.UserDictionary.ContainsKey($"F2R_JOINT{c * 4 + 4}"))
                                        TableDataCA_set_string.Add(geometry.UserDictionary.GetString($"F2R_JOINT{c * 4 + 4}"));
                                    else
                                        TableDataCA_set_string.Add(null);

                                    if (c == 0)
                                        TableDataCA_set_string.Add(geometry.UserDictionary.ContainsKey("F2R_GUID") ? geometry.UserDictionary.GetString("F2R_GUID") : "");
                                    else
                                        TableDataCA_set_string.Add("");
                                }
                            }

                            TableDataCAA_set[3 * b] = geometry.UserDictionary.GetString("F2R_ID");
                            TableDataCAA_set[3 * b + 1] = geometry.UserDictionary.GetString("F2R_PROPERTY");
                            TableDataCAA_set[3 * b + 2] = "Default";
                        }

                        string[] TableDataCA_set = TableDataCA_set_string.ToArray();

                        if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyCA_set, ref TableVersion, ref FieldKeysIncludedCA_set, NumberRecords, ref TableDataCA_set) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Connectivity - Area table");
                            _run = false;
                            Message = "Fail";
                            return;
                        }
                        if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyASA_set, ref TableVersion, ref FieldKeysIncludedASA_set, NumberRecords, ref TableDataCAA_set) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Area Section Assignments table");
                            _run = false;
                            Message = "Fail";
                            return;
                        }
                    }

                    #endregion

                    #region Groups 2 - Assignments

                    if (updateGroups)
                    {
                        HashSet<string> groups = new HashSet<string>();
                        List<string> TableDataG2A_setList = new List<string>();

                        /// Input per i GetTableForEditingArray
                        int TableVersion = 0;
                        int NumberRecords = 0;

                        for (int i = 0; i < jointAttributes.Count; i++)
                        {
                            var joint = jointAttributes[i];
                            GeometryBase geometry = GH_Convert.ToGeometryBase(joint);

                            string[] keys = geometry.UserDictionary.Keys;

                            for (int j = 0; j < keys.Length; j++)
                            {
                                string key = keys[j];
                                if (key.StartsWith("F2R_GROUP_"))
                                {
                                    var newGroup = geometry.GetUserString(key);
                                    groups.Add(newGroup);

                                    TableDataG2A_setList.Add(newGroup);
                                    TableDataG2A_setList.Add("Joint");
                                    TableDataG2A_setList.Add(geometry.GetUserString("F2R_ID"));
                                }
                            }
                        }
                        for (int i = 0; i < frames.Count; i++)
                        {
                            var frame = frames[i];
                            GeometryBase geometry = GH_Convert.ToGeometryBase(frame);

                            string[] keys = geometry.UserDictionary.Keys;

                            for (int j = 0; j < keys.Length; j++)
                            {
                                string key = keys[j];
                                if (key.StartsWith("F2R_GROUP_"))
                                {
                                    var newGroup = geometry.GetUserString(key);
                                    groups.Add(newGroup);

                                    TableDataG2A_setList.Add(newGroup);
                                    TableDataG2A_setList.Add("Frame");
                                    TableDataG2A_setList.Add(geometry.GetUserString("F2R_ID"));
                                }
                            }
                        }
                        for (int i = 0; i < areas.Count; i++)
                        {
                            var area = areas[i];
                            GeometryBase geometry = GH_Convert.ToGeometryBase(area);

                            string[] keys = geometry.UserDictionary.Keys;

                            for (int j = 0; j < keys.Length; j++)
                            {
                                string key = keys[j];
                                if (key.StartsWith("F2R_GROUP_"))
                                {
                                    var newGroup = geometry.GetUserString(key);
                                    groups.Add(newGroup);

                                    TableDataG2A_setList.Add(newGroup);
                                    TableDataG2A_setList.Add("Area");
                                    TableDataG2A_setList.Add(geometry.GetUserString("F2R_ID"));
                                }
                            }
                        }

                        string[] TableDataG2A_set = TableDataG2A_setList.ToArray();

                        /// Input per i SetTableForEditingArray
                        string TableKeyG2A_set = "Groups 2 - Assignments";
                        string[] FieldKeysIncludedG2A_set = { "GroupName", "ObjectType", "ObjectLabel" };

                        if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyG2A_set, ref TableVersion, ref FieldKeysIncludedG2A_set, NumberRecords, ref TableDataG2A_set) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Groups 1 - Definitions table");
                            _run = false;
                            Message = "Fail";
                            return;
                        }
                    }

                    #endregion

                    #region Apply Edited Tables

                    /// Input per la funzione "ApplyEditedTables"
                    bool FillImportLog = true;
                    int NumFatalErrors = 0;
                    int NumErrorMsgs = 0;
                    int NumWarnMsgs = 0;
                    int NumInfoMsgs = 0;
                    string ImportLog = "";

                    if (SapModel.DatabaseTables.ApplyEditedTables(FillImportLog, ref NumFatalErrors, ref NumErrorMsgs, ref NumWarnMsgs, ref NumInfoMsgs, ref ImportLog) != 0)
                    {
                        DA.SetData(0, ImportLog);
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to apply edited tables");
                        _run = false;
                        Message = "Fail";
                        return;
                    }

                    DA.SetData(0, ImportLog);

                    #endregion

                    #region Refresh Save and Close

                    if (SapModel.View.RefreshView() != 0)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to refresh the view");

                    if (!attachToInstance)
                    {
                        if (SapModel.File.Save(modelPath) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to save the model");
                            Message = "Fail";
                            _run = false;
                        }
                        else
                        {
                            Message = "Done";
                        }
                    }
                    else
                    {
                        Message = "Done";
                    }

                    #endregion

                    _run = false;
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
                    _run = false;
                }
            }
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RGrasshopper2SAP;

        public override Guid ComponentGuid => new Guid("166736e2-a595-4125-b5b5-414b2c4c8941");
    }
}
