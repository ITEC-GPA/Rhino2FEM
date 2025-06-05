using FeMM.Grasshopper.ComponentAttributes;
using FeMM.Grasshopper.DataTypes.F2R;
using FeMM.Grasshopper.Helpers;
using Fem2Rhino.Common;
using Fem2Rhino.CSI.ApiWrapper;
using Fem2Rhino.CSI.ModelWrapper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace FeMM.Grasshopper.Components.F2R
{
    public class BakeSap2000ModelToGrasshopperComponent : GH_Component
    {
        protected Dictionary<int, string> _outputPresets = new Dictionary<int, string>()
        {
            { 0, "All" },
            { 1, "Geometry assignements" },
            { 2, "Section properties" },
        };

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BakeSap2000ModelToGrasshopperComponent()
            : base("Bake SAP2000 Model", "BSM", "Bake SAP2000 model", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVEGRASSHOPPER)
        {
        }

        private bool _run = false;

        public override void CreateAttributes()
        {
            ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Bake");
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
            pManager.AddTextParameter("Model Path", "MP", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
            pManager[1].Optional = true;

            int i = pManager.AddIntegerParameter("Output preferences", "O", "", GH_ParamAccess.item, 1);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _outputPresets)
                mtParam.AddNamedValue(v.Value, v.Key);
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
            string modelPath = "";
            string sapExePath = "";
            bool attachToInstance = false;
            bool visible = false;
            int type = 0;

            //input
            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            DA.GetData(2, ref type);

            List<GeometryBase> joints = new List<GeometryBase>();
            List<GeometryBase> jointsAttributes = new List<GeometryBase>();
            List<GeometryBase> frames = new List<GeometryBase>();
            List<GeometryBase> areas = new List<GeometryBase>();

            if (_run)
            {
                if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sap model path not valid, opening an empty file");
                    return;
                }

                int units;

                if (RhinoDoc.ActiveDoc != null)
                {
                    string rhinoUnits = RhinoDoc.ActiveDoc.GetUnitSystemName(true, true, true, true);
                    if (rhinoUnits.ToLower() == "millimeter" || rhinoUnits.ToLower() == "mm")
                    {
                        units = Units.N_mm_C;
                    }
                    else if (rhinoUnits.ToLower() == "meter" || rhinoUnits.ToLower() == "m")
                    {
                        units = Units.N_m_C;
                    }
                    else if (rhinoUnits.ToLower() == "centimeter" || rhinoUnits.ToLower() == "cm")
                    {
                        units = Units.N_cm_C;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rhino lenght units not recognized. The model will be open using N, mm, C");
                        units = Units.N_mm_C;
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rhino active doc is null");
                    units = Units.N_mm_C;
                }

                Model.Options options = new Model.Options();
                if (type == 0)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = true;
                    options.ExportGeometryDatas = true;
                    options.ExportLoads = true;
                }
                if (type == 1)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = true;
                    options.ExportGeometryDatas = true;
                    options.ExportLoads = false;
                }
                if (type == 2)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = true;
                    options.ExportGeometryDatas = false;
                    options.ExportLoads = false;
                }
                if (type == 3)
                {
                    options.ExportGeometries = true;
                    options.ExportSectionProperties = false;
                    options.ExportGeometryDatas = false;
                    options.ExportLoads = false;
                }

                GH_CSIModel modelType = new GH_CSIModel(CSIApiWrapper.CsiSoftware.Sap2000, attachToInstance, sapExePath, units, modelPath, visible);
                modelType.Value.Option = options;

                List<string> log = new List<string>();

                try
                {
                    modelType.Value.Process();
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to read the model");

                    log = modelType.Value.GetLog();
                    for (int i = 0; i < log.Count; i++)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);
                    return;
                }

                try
                {
                    BakeGeometryCustom(modelType, out joints, out jointsAttributes, out frames, out areas);
                }
                catch (Exception)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to bake geometries");

                    log = modelType.Value.GetLog();
                    for (int i = 0; i < log.Count; i++)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

                    return;
                }

                log = modelType.Value.GetLog();
                for (int i = 0; i < log.Count; i++)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

                _run = false;
            }

            DA.SetDataList(0, joints);
            DA.SetDataList(1, jointsAttributes);
            DA.SetDataList(2, frames);
            DA.SetDataList(3, areas);
        }

        public void BakeGeometryCustom(GH_CSIModel gH_CSIModel, out List<GeometryBase> joints, out List<GeometryBase> jointsAttribute,
            out List<GeometryBase> frames, out List<GeometryBase> areas)
        {
            Model model = gH_CSIModel.Value;

            joints = new List<GeometryBase>();
            jointsAttribute = new List<GeometryBase>();
            frames = new List<GeometryBase>();
            areas = new List<GeometryBase>();

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

                    if (model.Option.ExportSectionProperties)
                    {
                        new_geometry.UserDictionary.Set("F2R_ID", joint.Id);
                        new_geometry.UserDictionary.Set("F2R_GUID", joint.Guid.ToString());
                        new_geometry.UserDictionary.Set("F2R_SPECIALJOINT", joint.SpecialJoint.ToString());

                        if (model.Option.ExportGeometryDatas)
                        {
                            new_geometry.UserDictionary.Set("F2R_RESTRAINT_TRASLATIONS", joint.GetRestraintsTraslation());
                            new_geometry.UserDictionary.Set("F2R_RESTRAINT_ROTATIONS", joint.GetRestraintsRotation());

                            if (joint.Groups.Count > 0)
                            {
                                for (int k = 0; k < joint.Groups.Count; k++)
                                {
                                    new_geometry.UserDictionary.Set("F2R_GROUP_" + jointGroupsDictionary[joint.Groups[k].Name], joint.Groups[k].Name);
                                }
                            }
                        }

                        if (model.Option.ExportLoads)
                        {
                            if (joint.PointLoads.Count > 0)
                            {
                                int loadCount = 1;
                                for (int k = 0; k < joint.PointLoads.Count; k++)
                                {
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_LOADPATTERN", joint.PointLoads[k].LoadPattern);
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_COORDINATESYSTEM", joint.PointLoads[k].CoordinateSystem.ToString());
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_F", joint.PointLoads[k].GetF());
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_M", joint.PointLoads[k].GetM());
                                    loadCount++;
                                }
                            }
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
                Frame frame = (Frame)model.Frames[i];

                try
                {
                    GeometryBase geometry = GH_Convert.ToGeometryBase(frame.Line);
                    GeometryBase new_geometry = geometry.Duplicate();

                    if (model.Option.ExportSectionProperties)
                    {
                        new_geometry.UserDictionary.Set("F2R_ID", frame.Id);
                        new_geometry.UserDictionary.Set("F2R_GUID", frame.Guid.ToString());
                        new_geometry.UserDictionary.Set("F2R_JOINT1", frame.StartNode.Id);
                        new_geometry.UserDictionary.Set("F2R_JOINT2", frame.EndNode.Id);
                        new_geometry.UserDictionary.Set("F2R_PROPERTY", frame.Property.Name);
                        if (frame.Property.Material != null)
                            new_geometry.UserDictionary.Set("F2R_PROPERTY_MATERIAL", frame.Property.Material.Name);

                        new_geometry.UserDictionary.Set("F2R_BEAMVOLUME", frame.BeamVolume.ToString());
                        new_geometry.UserDictionary.Set("F2R_BEAMWEIGHT", frame.BeamWeight.ToString());
                        new_geometry.UserDictionary.Set("F2R_SECTIONAREA", frame.Property.Area.ToString());
                        new_geometry.UserDictionary.Set("F2R_SECTIONTYPE", frame.Property.SectionType);

                        if (model.Option.ExportGeometryDatas)
                        {
                            new_geometry.UserDictionary.Set("F2R_ANGLE", frame.Angle.ToString());
                            new_geometry.UserDictionary.Set("F2R_ADVANCEDLOCALAXIS_ACTIVE", frame.AdvancedLocalAxis.ToString());
                            new_geometry.UserDictionary.Set("F2R_ADVANCEDLOCALAXIS_ANGLE", frame.AdvancedLocalAngle.ToString());

                            new_geometry.UserDictionary.Set("F2R_INSERTIONPOINT_CP_CS_ST_M2_M3", ((int)frame.InsertionPoint.CardinalPoint - 1).ToString() + ";" + frame.InsertionPoint.CoordinateSystem + ";" +
                                frame.InsertionPoint.StiffnessTransform + ";" + frame.InsertionPoint.Mirror2 + ";" + frame.InsertionPoint.Mirror3);
                            new_geometry.UserDictionary.Set("F2R_INSERTIONPOINT_OFFSET1", frame.InsertionPoint.GetOffset1());
                            new_geometry.UserDictionary.Set("F2R_INSERTIONPOINT_OFFSET2", frame.InsertionPoint.GetOffset2());

                            new_geometry.UserDictionary.Set("F2R_RELEASES_IEND_RELEASES", frame.Release.GetReleasesIEndReleases());
                            new_geometry.UserDictionary.Set("F2R_RELEASES_JEND_RELEASES", frame.Release.GetReleasesJEndReleases());
                            new_geometry.UserDictionary.Set("F2R_RELEASES_IEND_PARTIALFIXITYSPRINGS", frame.Release.GetReleasesIEndPartialFixity());
                            new_geometry.UserDictionary.Set("F2R_RELEASES_JEND_PARTIALFIXITYSPRINGS", frame.Release.GetReleasesJEndPartialFixity());

                            new_geometry.UserDictionary.Set("F2R_OVERWRITE_UNBRACEDLENGTH", frame.DesignSteelOverwrite.UnbracedLengthRatioMajor.ToString() + ";" +
                                frame.DesignSteelOverwrite.UnbracedLengthRatioMinor.ToString() + ";" + frame.DesignSteelOverwrite.UnbracedLengthRatioLateralTorsionalBuckling.ToString());

                            new_geometry.UserDictionary.Set("F2R_OVERWRITE_EFFECTIVELENGTHFACTOR", frame.DesignSteelOverwrite.EffectiveLengthFactorK1Major.ToString() + ";" +
                                frame.DesignSteelOverwrite.EffectiveLengthFactorK1Minor.ToString() + ";" + frame.DesignSteelOverwrite.EffectiveLengthFactorK2Major.ToString() + ";" +
                                frame.DesignSteelOverwrite.EffectiveLengthFactorK1Minor.ToString() + ";" + frame.DesignSteelOverwrite.EffectiveLengthFactorKLateralTorsionalBuckling.ToString());

                            new_geometry.UserDictionary.Set("F2R_OVERWRITE_MOMENTCOEFFICIENT", frame.DesignSteelOverwrite.MomentCoefficientCmMajor.ToString() + ";" +
                                frame.DesignSteelOverwrite.MomentCoefficientCmMinor.ToString() + ";" + frame.DesignSteelOverwrite.BendingCoefficientCb.ToString());

                            new_geometry.UserDictionary.Set("F2R_OVERWRITE_NONSWAYMOMENTFACTOR", frame.DesignSteelOverwrite.NonswayMomentFactorB1Major.ToString() + ";" +
                                frame.DesignSteelOverwrite.NonswayMomentFactorB1Minor.ToString());
                            new_geometry.UserDictionary.Set("F2R_OVERWRITE_SWAYMOMENTFACTOR", frame.DesignSteelOverwrite.SwayMomentFactorB2Major.ToString() + ";" +
                                frame.DesignSteelOverwrite.SwayMomentFactorB2Minor.ToString());

                            new_geometry.UserDictionary.Set("F2R_LOADTRANSFER", frame.LoadTransfer.ToString());
                            new_geometry.UserDictionary.Set("F2R_AUTOMESH", frame.AutoMesh.ToString());

                            new_geometry.UserDictionary.Set("F2R_FRAME_MODIFIERS", frame.BeamModifier.ToString());
                            new_geometry.UserDictionary.Set("F2R_FRAMEPROP_MODIFIERS", frame.BeamPropertyModifier.ToString());

                            if (frame.Groups.Count > 0)
                            {
                                for (int j = 0; j < frame.Groups.Count; j++)
                                {
                                    new_geometry.UserDictionary.Set("F2R_GROUP_" + frameGroupsDictionary[frame.Groups[j].Name], frame.Groups[j].Name);
                                }
                            }
                        }

                        if (frame.Property.IsNone)
                        {
                            new_geometry.UserDictionary.Set("F2R_ISNONE", true.ToString());
                        }

                        if (frame.Property.IsNonPrismatic)
                        {
                            new_geometry.UserDictionary.Set("F2R_ISNONPRISMATIC", true.ToString());
                            new_geometry.UserDictionary.Set("F2R_NONPRISMATIC_TOTALLENGHT", frame.Property.TotalLenght.ToString());
                            new_geometry.UserDictionary.Set("F2R_NONPRISMATIC_RELATIVEDISTANCE", frame.Property.RelativeDistance.ToString());
                        }

                        if (model.Option.ExportLoads)
                        {
                            if (frame.PointLoads.Count > 0)
                            {
                                int loadCount = 1;
                                for (int k = 0; k < frame.PointLoads.Count; k++)
                                {
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_LOADPATTERN", frame.PointLoads[k].LoadPattern);
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_DIRECTION", frame.PointLoads[k].Direction.ToString());
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_COORDINATESYSTEM", frame.PointLoads[k].CoordinateSystem.ToString());
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_TYPE", frame.PointLoads[k].Type.ToString());
                                    new_geometry.UserDictionary.Set("F2R_POINTLOAD_" + loadCount + "_VALUE", frame.PointLoads[k].Value.ToString());
                                    loadCount++;
                                }
                            }

                            if (frame.LoadDistributeds.Count > 0)
                            {
                                int loadCount = 1;
                                for (int k = 0; k < frame.LoadDistributeds.Count; k++)
                                {
                                    new_geometry.UserDictionary.Set("F2R_DISTRIBUTEDLOAD_" + loadCount + "_LOADPATTERN", frame.LoadDistributeds[k].LoadPattern);
                                    new_geometry.UserDictionary.Set("F2R_DISTRIBUTEDLOAD_" + loadCount + "_DIRECTION", frame.LoadDistributeds[k].Direction.ToString());
                                    new_geometry.UserDictionary.Set("F2R_DISTRIBUTEDLOAD_" + loadCount + "_COORDINATESYSTEM", frame.LoadDistributeds[k].CoordinateSystem.ToString());
                                    new_geometry.UserDictionary.Set("F2R_DISTRIBUTEDLOAD_" + loadCount + "_TYPE", frame.LoadDistributeds[k].Type.ToString());
                                    new_geometry.UserDictionary.Set("F2R_DISTRIBUTEDLOAD_" + loadCount + "_STARTVALUE", frame.LoadDistributeds[k].ValueStart.ToString());
                                    new_geometry.UserDictionary.Set("F2R_DISTRIBUTEDLOAD_" + loadCount + "_ENDVALUE", frame.LoadDistributeds[k].ValueEnd.ToString());
                                    loadCount++;
                                }
                            }
                        }
                    }

                    frames.Add(new_geometry);
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to bake frame {frame.Id}. Exception: {ex}");
                }
            }

            #endregion

            #region Areas

            for (int i = 0; i < model.Areas.Length; i++)
            {
                Plate a = model.Areas[i];
                if (a != null)
                {
                    Area area = (Area)a;

                    try
                    {
                        GeometryBase geometry = GH_Convert.ToGeometryBase(area.Shape);
                        GeometryBase new_geometry = geometry.Duplicate();
                        if (model.Option.ExportSectionProperties)
                        {
                            new_geometry.UserDictionary.Set("F2R_ID", area.Id);
                            new_geometry.UserDictionary.Set("F2R_GUID", area.Guid.ToString());
                            new_geometry.UserDictionary.Set("F2R_PROPERTY", area.Property.Name);
                            new_geometry.UserDictionary.Set("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
                            for (int n = 0; n < area.GetPoints.Length; n++)
                                new_geometry.UserDictionary.Set($"F2R_JOINT{n + 1}", area.GetPoints[n].Id);

                            if (((AreaProperty)area.Property).IsNone)
                                new_geometry.UserDictionary.Set("F2R_ISNONE", true.ToString());

                            if (model.Option.ExportGeometryDatas)
                            {
                                if (area.Groups.Count > 0)
                                {
                                    for (int j = 0; j < area.Groups.Count; j++)
                                    {
                                        new_geometry.UserDictionary.Set("F2R_GROUP_" + areaGroupsDictionary[area.Groups[j].Name], area.Groups[j].Name);
                                    }
                                }

                                new_geometry.UserDictionary.Set("F2R_PLATE_MODIFIERS", area.PlateModifier.ToString());
                                new_geometry.UserDictionary.Set("F2R_PLATEPROP_MODIFIERS", area.PlatePropertyModifier.ToString());

                                new_geometry.UserDictionary.Set("F2R_ANGLE", area.Angle.ToString());
                                new_geometry.UserDictionary.Set("F2R_ADVANCEDLOCALAXIS_ACTIVE", area.AdvancedLocalAxis.ToString());
                                new_geometry.UserDictionary.Set("F2R_ADVANCEDLOCALAXIS_ANGLE", area.AdvancedLocalAngle.ToString());
                                new_geometry.UserDictionary.Set("F2R_ADVANCEDLOCALAXIS", area.AdvanceLocalCoordinateSystem.ToString());
                            }

                            if (model.Option.ExportLoads)
                            {
                                if (area.LoadUniform.Count > 0)
                                {
                                    int loadCount = 1;
                                    for (int k = 0; k < area.LoadUniform.Count; k++)
                                    {
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMLOAD_" + loadCount + "_LOADPATTERN", area.LoadUniform[k].LoadPattern);
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMLOAD_" + loadCount + "_DIRECTION", area.LoadUniform[k].Direction.ToString());
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMLOAD_" + loadCount + "_COORDINATESYSTEM", area.LoadUniform[k].CoordinateSystem.ToString());
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMLOAD_" + loadCount + "_VALUE", area.LoadUniform[k].Value.ToString());
                                        loadCount++;
                                    }
                                }

                                if (area.AreaLoadUniformToFrames.Count > 0)
                                {
                                    int loadCount = 1;
                                    for (int k = 0; k < area.AreaLoadUniformToFrames.Count; k++)
                                    {
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_LOADPATTERN", area.AreaLoadUniformToFrames[k].LoadPattern);
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_DIRECTION", area.AreaLoadUniformToFrames[k].Direction.ToString());
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_COORDINATESYSTEM", area.AreaLoadUniformToFrames[k].CoordinateSystem.ToString());
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_VALUE", area.AreaLoadUniformToFrames[k].Value.ToString());
                                        new_geometry.UserDictionary.Set("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_DISTRIBUTIONTYPE", area.AreaLoadUniformToFrames[k].DistributionType.ToString());
                                        loadCount++;
                                    }
                                }
                            }
                        }

                        areas.Add(new_geometry);
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to bake area {area.Id}. Exception: {ex}");
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to bake area n� {i}");
                }
            }

            #endregion
        }

        //protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoBakeSAPIcon;

        public override Guid ComponentGuid => new Guid("35fa961d-0ee9-4040-9cc5-3b07a249aca4");
    }
}
