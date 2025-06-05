using FeMM.Grasshopper.DataTypes.F2R;
using FeMM.Grasshopper.Helpers;
using Fem2Rhino.Common;
using Fem2Rhino.CSI.ModelWrapper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace FeMM.Grasshopper.Components.F2R
{
    public class InteractiveSap2GrasshopperComponent : GH_Component
    {
        private bool _run = false;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public InteractiveSap2GrasshopperComponent()
            : base("SAP2Grasshopper", "S2G", "Open interactive model of current open istance", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEGRASSHOPPER)
        {
        }

        public override void CreateAttributes()
        {
            ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "SAP2Rhino");
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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Group", "G", "Sap2000 group for selection element. Default value: ALL", GH_ParamAccess.item, "ALL");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Joints", "J", "The Joints", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Joint Attributes", "JA", "The data attached to each joints", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Frames", "F", "The Frames with the data attached", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Areas", "A", "The Areas with the data attached", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string groupName = "";

            DA.GetData(0, ref groupName);

            List<GeometryBase> joints = new List<GeometryBase>();
            List<GeometryBase> jointsAttributes = new List<GeometryBase>();
            List<GeometryBase> frames = new List<GeometryBase>();
            List<GeometryBase> areas = new List<GeometryBase>();

            if (_run)
            {

                GH_SAPInteractiveDatabaseModel modelType = new GH_SAPInteractiveDatabaseModel();
                modelType.Value.Group = groupName;
                try
                {
                    modelType.Value.Process();
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to process the model: {ex.Message}");
                }

                List<string> log = modelType.GetLog();
                if (log.Count > 0)
                {
                    foreach (string logItem in log)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, logItem);
                    }
                }

                try
                {
                    BakeGeometryCustom(modelType, out joints, out jointsAttributes, out frames, out areas);
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to bake the model");
                }

                _run = false;
            }
            DA.SetDataList(0, joints);
            DA.SetDataList(1, jointsAttributes);
            DA.SetDataList(2, frames);
            DA.SetDataList(3, areas);
        }

        public void BakeGeometryCustom(GH_SAPInteractiveDatabaseModel gH_SAPInteractiveDatabaseModel, out List<GeometryBase> joints, out List<GeometryBase> jointsAttribute,
            out List<GeometryBase> frames, out List<GeometryBase> areas)
        {
            Fem2Rhino.CSI.SapInteractiveDatabaseModel model = gH_SAPInteractiveDatabaseModel.Value;

            joints = new List<GeometryBase>();
            jointsAttribute = new List<GeometryBase>();
            frames = new List<GeometryBase>();
            areas = new List<GeometryBase>();

            RhinoDoc doc = RhinoDoc.ActiveDoc;

            #region Dictionary

            Dictionary<string, int> jointGroupsDictionary = new Dictionary<string, int>();
            Dictionary<string, int> frameGroupsDictionary = new Dictionary<string, int>();
            Dictionary<string, int> areaGroupsDictionary = new Dictionary<string, int>();

            Fem2Rhino.Common.F2RModelHelper.GetGroupDictionaryAssociation(model.Joints, ref jointGroupsDictionary);
            Fem2Rhino.Common.F2RModelHelper.GetGroupDictionaryAssociation(model.Frames, ref frameGroupsDictionary);
            Fem2Rhino.Common.F2RModelHelper.GetGroupDictionaryAssociation(model.Areas, ref areaGroupsDictionary);

            #endregion

            #region Joints

            for (int i = 0; i < model.Joints.Length; i++)
            {
                Joint joint = model.Joints[i];
                try
                {
                    var sphere = new Circle(joint.Location, 1);
                    Point point = new Point(joint.Location);
                    GeometryBase geometry = GH_Convert.ToGeometryBase(sphere);
                    GeometryBase new_geometry = geometry.Duplicate();

                    new_geometry.UserDictionary.Set("F2R_ID", joint.Id);
                    new_geometry.UserDictionary.Set("F2R_SPECIALJOINT", joint.SpecialJoint.ToString());
                    new_geometry.UserDictionary.Set("F2R_GUID", joint.Guid.ToString());

                    if (joint.Groups.Count > 0)
                    {
                        for (int k = 0; k < joint.Groups.Count; k++)
                        {
                            new_geometry.UserDictionary.Set("F2R_GROUP_" + jointGroupsDictionary[joint.Groups[k].Name], joint.Groups[k].Name);
                        }
                    }

                    jointsAttribute.Add(new_geometry);
                    joints.Add(point);
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to bake joint {joint.Id}. Exception: {ex}");
                }
            }

            #endregion

            #region Frames

            for (int i = 0; i < model.Frames.Length; i++)
            {
                if (model.Frames[i] != null)
                {
                    Frame frame = (Frame)model.Frames[i];
                    try
                    {
                        GeometryBase geometry = GH_Convert.ToGeometryBase(frame.Line);
                        GeometryBase new_geometry = geometry.Duplicate();

                        new_geometry.UserDictionary.Set("F2R_ID", frame.Id);
                        new_geometry.UserDictionary.Set("F2R_JOINT1", frame.StartNode.Id);
                        new_geometry.UserDictionary.Set("F2R_JOINT2", frame.EndNode.Id);
                        new_geometry.UserDictionary.Set("F2R_GUID", frame.Guid.ToString());
                        new_geometry.UserDictionary.Set("F2R_PROPERTY", frame.Property.Name);
                        new_geometry.UserDictionary.Set("F2R_PROPERTY_MATERIAL", frame.Property.Material.Name);

                        if (frame.Groups.Count > 0)
                        {
                            for (int j = 0; j < frame.Groups.Count; j++)
                            {
                                new_geometry.UserDictionary.Set("F2R_GROUP_" + frameGroupsDictionary[frame.Groups[j].Name], frame.Groups[j].Name);
                            }
                        }
                        frames.Add(new_geometry);
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to bake frame {frame.Id}. Exception: {ex}");
                    }
                }
            }

            #endregion

            #region Areas					

            for (int i = 0; i < model.Areas.Length; i++)
            {
                if (model.Areas[i] != null)
                {
                    Area area = (Area)model.Areas[i];

                    try
                    {
                        GeometryBase geometry = GH_Convert.ToGeometryBase(area.Shape);
                        GeometryBase new_geometry = geometry.Duplicate();

                        new_geometry.UserDictionary.Set("F2R_ID", area.Id);
                        new_geometry.UserDictionary.Set("F2R_GUID", area.Guid.ToString());
                        new_geometry.UserDictionary.Set("F2R_PROPERTY", area.Property.Name);
                        new_geometry.UserDictionary.Set("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
                        for (int n = 0; n < area.GetPoints.Length; n++)
                            new_geometry.UserDictionary.Set($"F2R_JOINT{n + 1}", area.GetPoints[n].Id);

                        if (((AreaProperty)area.Property).IsNone)
                            new_geometry.UserDictionary.Set("F2R_ISNONE", true.ToString());

                        if (area.Groups.Count > 0)
                        {
                            for (int j = 0; j < area.Groups.Count; j++)
                            {
                                new_geometry.UserDictionary.Set("F2R_GROUP_" + areaGroupsDictionary[area.Groups[j].Name], area.Groups[j].Name);
                            }
                        }
                        areas.Add(new_geometry);
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to bake area {area.Id}. Exception: {ex}");
                    }
                }
            }

            #endregion
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RSAP2Grasshopper;

        public override Guid ComponentGuid => new Guid("291ae773-3e91-4731-b464-7045e2b7f57e");
    }
}
