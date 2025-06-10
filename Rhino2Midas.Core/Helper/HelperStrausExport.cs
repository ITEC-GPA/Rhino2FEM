using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Elements;
using Rhino2Fem.Core.Loads;
using Rhino2Fem.Core.Models;
using Rhino2Fem.Core.Settings;
using St7API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rhino2Fem.Core.Helper
{
    public class HelperStrausExport
    {
        /// <summary>
        /// False means id is free to be used, index 0 to be ignored since straus use ID from 1 to 32
        /// </summary>
        public static bool[] _openedModelsCount = new bool[32];
        public static string _tempModelFileName;

        public static string[] Solvers = { "Linear Static", "Nonlinear Static", "Staged analysis" };

        public ModelModel Model { get; set; }

        public HelperStrausExport(ModelModel model)
        {
            Model = model;
        }

        public bool CreateModelR3(string outputPath, out int modelId, out List<string> warnings, out List<string> errors)
        {
            string scratchPath = Path.GetTempPath();
            warnings = new List<string>();
            errors = new List<string>();
            modelId = GetAvailableModelId();

            bool st7FileOpened = false;
            //scratchPath = Path.GetDirectoryName(outputPath) + "\\";
            try
            {
                if (HandleError(St7.St7Init()))
                    throw new Exception("Failed to init st7");

                if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                    throw new Exception("Directory does not exist");

                // Create a new model
                if (HandleError(St7.St7NewFile(modelId, outputPath, scratchPath)))
                    throw new Exception("Failed to create new model");
                else
                    st7FileOpened = true;

                // Set the measure units
                int[] st7_units = new int[St7.kLastUnit];

                switch (Model.ModelUnits.HeatUnit)
                {
                    case ModelUnitsModel.HeatUnitTypes.J:
                        st7_units[St7.ipENERGYU] = St7.euJOULE;
                        break;
                    case ModelUnitsModel.HeatUnitTypes.CAL:
                        st7_units[St7.ipENERGYU] = St7.euCALORIE;
                        break;
                    case ModelUnitsModel.HeatUnitTypes.KJ:
                        st7_units[St7.ipENERGYU] = St7.euKILOJOULE;
                        break;
                    case ModelUnitsModel.HeatUnitTypes.BTU:
                        st7_units[St7.ipENERGYU] = St7.euBTU;
                        break;
                    default:
                        return false;
                }
                switch (Model.ModelUnits.LengthUnit)
                {
                    case ModelUnitsModel.LengthUnitTypes.MM:
                        st7_units[St7.ipLENGTHU] = St7.luMILLIMETRE;
                        break;
                    case ModelUnitsModel.LengthUnitTypes.M:
                        st7_units[St7.ipLENGTHU] = St7.luMETRE;
                        break;
                    case ModelUnitsModel.LengthUnitTypes.CM:
                        st7_units[St7.ipLENGTHU] = St7.luMETRE;
                        break;
                    default:
                        return false;
                }
                switch (Model.ModelUnits.ForceUnit)
                {
                    case ModelUnitsModel.ForceUnitTypes.N:
                        st7_units[St7.ipFORCEU] = St7.fuNEWTON;
                        break;
                    case ModelUnitsModel.ForceUnitTypes.KN:
                        st7_units[St7.ipFORCEU] = St7.fuKILONEWTON;
                        break;
                    default:
                        return false;
                }
                switch (Model.ModelUnits.TemperatureUnit)
                {
                    case ModelUnitsModel.TemperatureUnitTypes.F:
                        st7_units[St7.ipFORCEU] = St7.tuFAHRENHEIT;
                        break;
                    case ModelUnitsModel.TemperatureUnitTypes.C:
                        st7_units[St7.ipFORCEU] = St7.tuCELSIUS;
                        break;
                    default:
                        return false;
                }
                st7_units[St7.ipSTRESSU] = St7.suKILOPASCAL;
                st7_units[St7.ipMASSU] = St7.muTONNE;

                if (HandleError(St7.St7SetUnits(modelId, st7_units)))
                    throw new Exception("Failed to set the units");

                // Groups
                Dictionary<ElementGroupModel, int> groupsIds = new Dictionary<ElementGroupModel, int>();
                for (int k = 0; k < Model.Groups.Count; k++)
                {
                    ElementGroupModel group = Model.Groups.ElementAt(k).Value;
                    int parent_id = 1;
                    if (group.HasGroupParent)
                    {
                        /*List<GroupModel> parents = new List<GroupModel>();
						GroupModel curr_parent = group.Parent;
						while (curr_parent != null)
						{
							parents.Insert(0, curr_parent);
							curr_parent = curr_parent.Parent;
						}*/
                        List<ElementGroupModel> parents = group.GetBranch();
                        parents.RemoveAt(parents.Count - 1);
                        for (int j = 0; j < parents.Count; j++)
                        {
                            ElementGroupModel parent = parents[j];
                            // Check in the group already exists
                            bool found = false;
                            int numGroups = 0;
                            St7.St7GetNumGroups(modelId, ref numGroups);
                            for (int i = 1; i <= numGroups; i++)
                            {
                                StringBuilder gname = new StringBuilder(St7.kMaxStrLen);
                                int gid = 0;
                                St7.St7GetGroupByIndex(modelId, i, gname, St7.kMaxStrLen, ref gid);
                                string sname = gname.ToString().Replace("Model\\", "");
                                if (parent.GroupFullName == sname)
                                {
                                    found = true;
                                    parent_id = gid;
                                    break;
                                }
                            }
                            if (!found) // Create it if not existing
                            {
                                if (HandleError(St7.St7NewChildGroup(modelId, parent_id, parent.Name, ref parent_id)))
                                {
                                    warnings.Add(string.Format("Failed to create parent group {0}", parent.Name));
                                    continue;
                                }
                            }
                        }
                    }
                    int group_id = 0;
                    if (HandleError(St7.St7NewChildGroup(modelId, parent_id, group.Name, ref group_id)))
                    {
                        warnings.Add(string.Format("Failed to create group {0}", group.Name));
                        continue;
                    }
                    groupsIds.Add(group, group_id);
                }

                // Beam properties
                int id = 1;
                Dictionary<string, int> beamProperties = new Dictionary<string, int>();
                Dictionary<int, string> bxsFiles = new Dictionary<int, string>();

                for (int k = 0; k < Model.FrameProperties.Count; k++)
                {
                    FramePropertyModel bp = Model.FrameProperties.ElementAt(k).Value;
                    if (beamProperties.ContainsKey(bp.Name))
                    {
                        warnings.Add(string.Format("The beam property {0} already exists", bp.Name));
                        continue;
                    }

                    // Property type
                    int beamType = St7.btBeam;
                    //int beamType = St7.btNull;
                    //if (bp.PropertyType == BeamPropertyType.Cable)
                    //    beamType = St7.btCable;
                    //else if (bp.PropertyType == BeamPropertyType.Truss)
                    //    beamType = St7.btTruss;
                    //else if (bp.PropertyType == BeamPropertyType.CutoffBar)
                    //    beamType = St7.btCutoff;
                    //else if (bp.PropertyType == BeamPropertyType.PointContact)
                    //    beamType = St7.btContact;
                    //else if (bp.PropertyType == BeamPropertyType.Beam)
                    //    beamType = St7.btBeam;
                    //else if (bp.PropertyType == BeamPropertyType.Pipe)
                    //    beamType = St7.btPipe;
                    //else if (bp.PropertyType == BeamPropertyType.Connection)
                    //    beamType = St7.btConnection;

                    if (HandleError(St7.St7NewBeamProperty(modelId, id, beamType, bp.Name)))
                    {
                        warnings.Add(string.Format("Failed to create beam property {0}\r\n", bp.Name));
                        continue;
                    }
                    // CutoffBar specific properties
                    //if (bp.PropertyType == BeamPropertyType.CutoffBar)
                    //{
                    //    int[] co_ints = new int[2];
                    //    co_ints[St7.ipCutoffType] = bp.CutoffType == CutoffType.Brittle ? St7.cbBrittle : St7.cbDuctile;
                    //    co_ints[St7.ipKeepMass] = bp.KeepMass ? St7.btTrue : St7.btFalse;
                    //    double[] co_doubles = new double[2];
                    //    co_doubles[St7.ipCutoffTension] = bp.MaxTension;
                    //    co_doubles[St7.ipCutoffCompression] = bp.MaxCompression;
                    //    St7.St7SetCutoffBarData(modelId, id, co_ints, co_doubles);
                    //}

                    // Section Geometry
                    if (bp.SectionGeometryType != FrameSectionModel.SectionGeometryTypes.Generic)
                    {
                        /*
                        if (bp.SectionType == SectionModel.SectionTypes.GenericShapes)
                        {
                            //using bxs. creating a new model
                            int BXS_MODEL = 0;
                            int nodeIndex = 0;
                            int propNum = 1;
                            int elNumber = 0;
                            string directory = Path.GetDirectoryName(outputPath) + @"\";
                            string bxsFileName = directory + $"bxsFile{id}.st7";
                            //for bxs generation, scratch path cannot be the temp path
                            St7.St7NewFile(BXS_MODEL, bxsFileName, directory);
                            St7.St7SetUnits(BXS_MODEL, st7_units);
                            //creating plates
                            St7.St7NewPlateProperty(BXS_MODEL, propNum, St7.ptPlateShell, St7.mtIsotropic, "BXS_Prop");

                            DelaunayMesh.DelaunayGenerateOptions generateOptions = new DelaunayMesh.DelaunayGenerateOptions { InitialMeshOnly = true };

                            if (DelaunayMesh.Generate(bp.GenericShapes.ToShapes2d(), generateOptions, out List<Mesh> meshes, out DelaunayMesh.DelaunayGenerateMeshStatus meshStatus))
                            {
                                for (int j = 0; j < meshes.Count; j++)
                                {
                                    Mesh mesh = meshes[j];
                                    Dictionary<int, int> nodeMapper = new Dictionary<int, int>();
                                    MeshVertex[] vv = mesh.Vertices.ToArray();
                                    for (int i = 0; i < vv.Length; i++)
                                    {
                                        MeshVertex n = vv[i];
                                        nodeIndex++;
                                        St7.St7SetNodeXYZ(BXS_MODEL, nodeIndex, new double[] { n.Point.X, n.Point.Y, n.Point.Z });
                                        nodeMapper.Add(n.Id, nodeIndex);
                                    }
                                    MeshFace[] ff = mesh.Faces.ToArray();
                                    for (int jj = 0; jj < ff.Length; jj++)
                                    {
                                        MeshFace sh = ff[jj];
                                        MeshVertex[] nodes = mesh.GetFaceVertices(sh);

                                        int[] connections = new int[nodes.Length + 1];
                                        connections[0] = nodes.Length;
                                        for (int i = 0; i < nodes.Length; i++)
                                        {
                                            connections[i + 1] = nodeMapper[nodes[i].Id];
                                        }
                                        elNumber++;
                                        St7.St7SetElementConnection(BXS_MODEL, St7.tyPLATE, elNumber, propNum, connections);
                                    }
                                }
                            }

                            // Saving and closing the BXS model
                            St7.St7SaveFile(BXS_MODEL);
                            St7.St7CloseFile(BXS_MODEL);
                            bxsFiles.Add(id, bxsFileName);
                        }
                        else*/
                        {
                            int sectionType = St7.bsNullSection;
                            double[] st_doubles = new double[6];
                            switch (bp.SectionGeometryType)
                            {
                                case FrameSectionModel.SectionGeometryTypes.SR:
                                    St7.St7SetBeamSectionName(modelId, id, "SolidRound");
                                    sectionType = St7.bsCircularSolid;
                                    st_doubles[0] = bp.Dimension1;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.P:
                                    St7.St7SetBeamSectionName(modelId, id, "HollowRound");
                                    sectionType = St7.bsCircularHollow;
                                    st_doubles[0] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension2;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.SB:
                                    St7.St7SetBeamSectionName(modelId, id, "SolidRect");
                                    sectionType = St7.bsSquareSolid;
                                    st_doubles[0] = bp.Dimension1;
                                    st_doubles[1] = bp.Dimension2;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.B:
                                    St7.St7SetBeamSectionName(modelId, id, "HollowRect");
                                    sectionType = St7.bsSquareHollow;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension3;
                                    st_doubles[4] = bp.Dimension4;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.H:
                                    St7.St7SetBeamSectionName(modelId, id, "IBeam");
                                    sectionType = St7.bsISection;
                                    st_doubles[0] = bp.Dimension5;
                                    st_doubles[1] = bp.Dimension2;
                                    st_doubles[2] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension6;
                                    st_doubles[4] = bp.Dimension4;
                                    st_doubles[5] = bp.Dimension3;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.T:
                                    St7.St7SetBeamSectionName(modelId, id, "TBeam");
                                    sectionType = St7.bsTSection;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[2] = bp.Dimension5;
                                    st_doubles[3] = bp.Dimension4;
                                    st_doubles[4] = bp.Dimension3;
                                    st_doubles[5] = bp.Dimension6;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.C:
                                    St7.St7SetBeamSectionName(modelId, id, "Lipped Channel");
                                    sectionType = St7.bsLipChannel;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[2] = bp.Dimension5;
                                    st_doubles[3] = bp.Dimension4;
                                    st_doubles[4] = bp.Dimension3;
                                    st_doubles[5] = bp.Dimension6;
                                    break;
                                case FrameSectionModel.SectionGeometryTypes.L:
                                    St7.St7SetBeamSectionName(modelId, id, "Angle");
                                    sectionType = St7.bsLSection;
                                    st_doubles[0] = bp.Dimension2;
                                    st_doubles[1] = bp.Dimension1;
                                    st_doubles[3] = bp.Dimension4;
                                    st_doubles[4] = bp.Dimension3;
                                    break;
                                default:
                                    warnings.Add(string.Format("Unable to det the section geometry {0} of the beam property {1}",
                                        bp.SectionGeometryType, bp.Id));
                                    break;
                            }
                            St7.St7SetBeamSectionGeometry(modelId, id, sectionType, st_doubles);
                        }
                    }

                    // Section data
                    if (bp.SectionGeometryType == FrameSectionModel.SectionGeometryTypes.Generic)
                    {
                        int[] ints = new int[1];
                        ints[0] = 0;
                        double[] doubles = new double[11];
                        doubles[St7.ipAREA] = bp.SectionArea;
                        doubles[St7.ipI11] = bp.I11;
                        doubles[St7.ipI22] = bp.I22;
                        doubles[St7.ipJ] = bp.J;
                        doubles[St7.ipSL1] = bp.ShearL1;
                        doubles[St7.ipSL2] = bp.ShearL2;
                        doubles[St7.ipSA1] = bp.ShearA1;
                        doubles[St7.ipSA2] = bp.ShearA2;
                        doubles[St7.ipXBAR] = bp.Centroid.X;
                        doubles[St7.ipYBAR] = bp.Centroid.Y;
                        doubles[St7.ipANGLE] = bp.Angle;
                        St7.St7SetBeamSectionPropertyData(modelId, id, ints, doubles);
                    }

                    //if (bp.Mirror != SectionModel.MirrorTypes.None)
                    //{
                    //    int mirrorType = St7.mtLeft;
                    //    int compatibleTwist = St7.btFalse;
                    //    St7.St7SetBeamMirrorOption(modelId, id, mirrorType, compatibleTwist, new double[] { bp.MirrorGapA, bp.MirrorGapB });
                    //}

                    // Material
                    St7.St7SetMaterialName(modelId, St7.ptBEAMPROP, id, bp.Material.Name);

                    double[] doubles1 = new double[9];
                    doubles1[St7.ipBeamModulus] = bp.Material.ModulusElasticity;
                    doubles1[St7.ipBeamDensity] = bp.Material.Density;
                    doubles1[St7.ipBeamAlpha] = bp.Material.ThermalCoefficient;
                    doubles1[St7.ipBeamViscosity] = bp.Material.ViscousDamping;
                    doubles1[St7.ipBeamDampingRatio] = bp.Material.DampingRatio;
                    doubles1[St7.ipBeamConductivity] = bp.Material.Conductivity;
                    doubles1[St7.ipBeamSpecificHeat] = bp.Material.SpecificHeat;
                    doubles1[St7.ipBeamShear] = bp.Material.ShearModulus;
                    doubles1[St7.ipBeamPoisson] = bp.Material.PoissonRatio;

                    St7.St7SetBeamMaterialData(modelId, id, doubles1);
                    //if (bp.Material.UsePoisson)
                    St7.St7SetBeamShearModulusMode(modelId, id, St7.smUsePoissonsRatio);
                    //else
                    //    St7.St7SetBeamShearModulusMode(modelId, id, St7.smUseShearModulus);

                    St7.St7UpdateElementPropertyData(modelId, St7.ptBEAMPROP, id);

                    if (bp.SectionGeometryType != FrameSectionModel.SectionGeometryTypes.Generic)
                        St7.St7CalculateBeamSectionProperties(modelId, id, St7.btTrue);


                    //if (bp.Material.StressStrainTable != null && (bp.PropertyType == BeamPropertyType.Beam || bp.PropertyType == BeamPropertyType.Truss))
                    //{
                    //    int maxTableNum = 0;
                    //    int tables = 0;

                    //    St7.St7GetNumTables(modelId, St7.ttStressStrain, ref tables, ref maxTableNum);

                    //    var strain = bp.Material.StressStrainTable.Select(i => i.strain).ToArray();
                    //    var stress = bp.Material.StressStrainTable.Select(i => i.stress).ToArray();

                    //    double[] stressStrainTable = new double[strain.Length * 2];
                    //    for (int i = 0; i < strain.Length; i++)
                    //    {
                    //        stressStrainTable[i * 2] = strain[i];
                    //        stressStrainTable[i * 2 + 1] = stress[i];
                    //    }
                    //    maxTableNum++;

                    //    St7.St7NewTableType(modelId, St7.ttStressStrain, maxTableNum, bp.Material.StressStrainTable.Length, bp.Material.Name, stressStrainTable);

                    //    if (bp.PropertyType == BeamPropertyType.Beam)
                    //        St7.St7SetPropertyTable(modelId, St7.ptBeamStressVsStrain, id, maxTableNum);
                    //    else if (bp.PropertyType == BeamPropertyType.Truss)
                    //        St7.St7SetPropertyTable(modelId, St7.ptBeamStressVsStrain, id, maxTableNum);
                    //}

                    beamProperties.Add(bp.Name, id++);
                    //St7.St7SaveFile(modelId);
                }

                // Plate properties
                id = 1;
                Dictionary<string, int> plateProperties = new Dictionary<string, int>();
                for (int j = 0; j < Model.AreaProperties.Count; j++)
                {
                    AreaPropertyModel pp = Model.AreaProperties.ElementAt(j).Value;
                    if (plateProperties.ContainsKey(pp.Name))
                    {
                        warnings.Add(string.Format("The plate property {0} already exists", pp.Name));
                        continue;
                    }

                    int plateType = St7.ptNull;
                    int materialType = St7.mtNull;
                    if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShellThin ||
                        pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShellThick ||
                        pp.PropertyType == AreaPropertyModel.PlatePropertyType.PlateThin ||
                        pp.PropertyType == AreaPropertyModel.PlatePropertyType.PlateThick)
                    {
                        plateType = St7.ptPlateShell;
                        materialType = St7.mtIsotropic;
                    }
                    else if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShearPanel)
                    {
                        plateType = St7.ptPlateShell;
                        materialType = St7.mtOrthotropic;
                    }
                    else if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.Membrane)
                    {
                        plateType = St7.ptMembrane;
                        materialType = St7.mtIsotropic;
                    }
                    else if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.LoadPatch)
                    {
                        plateType = St7.ptLoadPatch;
                        materialType = St7.mtIsotropic;
                    }

                    if (HandleError(St7.St7NewPlateProperty(modelId, id, plateType, materialType, pp.Name)))
                    {
                        warnings.Add(string.Format("Failed to create beam property {0}\r\n", pp.Name));
                        continue;
                    }

                    if (pp.PropertyType != AreaPropertyModel.PlatePropertyType.LoadPatch)
                    {
                        St7.St7SetMaterialName(modelId, St7.ptPLATEPROP, id, pp.Material.Name);

                        if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShellThin ||
                            pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShellThick ||
                            pp.PropertyType == AreaPropertyModel.PlatePropertyType.PlateThin ||
                            pp.PropertyType == AreaPropertyModel.PlatePropertyType.PlateThick ||
                            pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShearPanel)
                        {
                            double[] thickness = new double[2];
                            thickness[0] = pp.ThicknessMembrane;
                            thickness[1] = pp.ThicknessBending;
                            St7.St7SetPlateThickness(modelId, id, thickness);
                        }
                        else if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.Membrane)
                        {
                            double[] thickness = new double[2];
                            thickness[0] = pp.ThicknessMembrane;
                            thickness[1] = 0;
                            St7.St7SetPlateThickness(modelId, id, thickness);
                        }

                        double[] doubles;
                        //switch (pp.Material)
                        //{
                        //    case Model.PlateProperty.MaterialType.Isotropic:
                        // Workarround for the API bug that it doesn't set the shear modulus in the ShearPanels
                        if (pp.PropertyType == AreaPropertyModel.PlatePropertyType.ShearPanel)
                        {
                            // temporarilly set the property as Shell Orthotropic
                            doubles = new double[18];
                            doubles[St7.ipPlateOrthoModulus1] = pp.Material.ModulusElasticity;
                            doubles[St7.ipPlateOrthoModulus1] = doubles[St7.ipPlateOrthoModulus1];
                            doubles[St7.ipPlateOrthoModulus1] = doubles[St7.ipPlateOrthoModulus1];
                            doubles[St7.ipPlateOrthoShear12] = doubles[St7.ipPlateOrthoModulus1];
                            doubles[St7.ipPlateOrthoPoisson12] = pp.Material.PoissonRatio;
                            doubles[St7.ipPlateOrthoPoisson23] = doubles[St7.ipPlateOrthoPoisson12];
                            doubles[St7.ipPlateOrthoPoisson31] = doubles[St7.ipPlateOrthoPoisson12];
                            doubles[St7.ipPlateOrthoDensity] = pp.Material.Density; //* c_f * Math.Pow(c_l, 3);
                            doubles[St7.ipPlateOrthoAlpha1] = pp.Material.ThermalCoefficient;
                            doubles[St7.ipPlateOrthoAlpha2] = doubles[St7.ipPlateOrthoAlpha1];
                            doubles[St7.ipPlateOrthoAlpha3] = doubles[St7.ipPlateOrthoAlpha1];
                            doubles[St7.ipPlateOrthoViscosity] = pp.Material.ViscousDamping;
                            doubles[St7.ipPlateOrthoDampingRatio] = pp.Material.DampingRatio;
                            doubles[St7.ipPlateOrthoConductivity1] = pp.Material.Conductivity;// pp.Material.Conductivity / c_l;
                            doubles[St7.ipPlateOrthoConductivity2] = doubles[St7.ipPlateOrthoConductivity1];
                            doubles[St7.ipPlateOrthoSpecificHeat] = pp.Material.SpecificHeat;// pp.Material.SpecificHeat * Math.Pow(c_l, 3);
                            St7.St7SetPlateOrthotropicMaterial(modelId, id, doubles);
                            // Then change the property in ShearPanel Isotropic
                            St7.St7SetPlatePropertyType(modelId, id, St7.ptShearPanel, St7.mtIsotropic);
                        }

                        doubles = new double[8];
                        doubles[St7.ipPlateIsoModulus] = pp.Material.ModulusElasticity;// * c_f * Math.Pow(c_l, 2);
                        doubles[St7.ipPlateIsoPoisson] = pp.Material.PoissonRatio;
                        doubles[St7.ipPlateIsoDensity] = pp.Material.Density; //* c_f * Math.Pow(c_l, 3);
                        doubles[St7.ipPlateIsoAlpha] = pp.Material.ThermalCoefficient;
                        doubles[St7.ipPlateIsoViscosity] = pp.Material.ViscousDamping;
                        doubles[St7.ipPlateIsoDampingRatio] = pp.Material.DampingRatio;
                        doubles[St7.ipPlateIsoConductivity] = pp.Material.Conductivity;// pp.Material.Conductivity / c_l;
                        doubles[St7.ipPlateIsoSpecificHeat] = pp.Material.SpecificHeat;// pp.Material.SpecificHeat * Math.Pow(c_l, 3);
                        St7.St7SetPlateIsotropicMaterial(modelId, id, doubles);

                        //if (pp.Material.StressStrainTable != null && (pp.PropertyType == PlatePropertyType.ShellThin || pp.PropertyType == PlatePropertyType.ShellThick ||
                        //    pp.PropertyType == PlatePropertyType.PlateThin || pp.PropertyType == PlatePropertyType.PlateThick))
                        //{
                        //    int maxTableNum = 0;
                        //    int tables = 0;

                        //    St7.St7GetNumTables(modelId, St7.ttStressStrain, ref tables, ref maxTableNum);

                        //    double[] strain = pp.Material.StressStrainTable.Select(i => i.strain).ToArray();
                        //    double[] stress = pp.Material.StressStrainTable.Select(i => i.stress).ToArray();
                        //    double[] stressStrainTable = new double[strain.Length * 2];

                        //    for (int i = 0; i < strain.Length; i++)
                        //    {
                        //        stressStrainTable[(i) * 2] = strain[i];
                        //        stressStrainTable[(i) * 2 + 1] = stress[i];
                        //    }
                        //    maxTableNum++;
                        //    St7.St7NewTableType(modelId, St7.ttStressStrain, maxTableNum, pp.Material.StressStrainTable.Length, pp.Material.Name, stressStrainTable);
                        //    St7.St7SetPropertyTable(modelId, St7.ptPlateStressVsStrain, id, maxTableNum);
                        //}

                        /*break;
					case Model.PlateProperty.MaterialType.Orthotropic:
						doubles = new double[18];
						doubles[ro.Const("ipPlateOrthoModulus1")] = pp.Modulus1;
						doubles[ro.Const("ipPlateOrthoModulus2")] = pp.Modulus2;
						doubles[ro.Const("ipPlateOrthoModulus3")] = pp.Modulus3;
						doubles[ro.Const("ipPlateOrthoShear12")] = pp.ShearMod12;
						if (pp.GetType() == typeof(Model.PlateShellProp))
						{
							Model.PlateShellProp psp = pp as Model.PlateShellProp;
							doubles[ro.Const("ipPlateOrthoShear23")] = psp.ShearMod23;
							doubles[ro.Const("ipPlateOrthoShear31")] = psp.ShearMod31;
						}
						doubles[ro.Const("ipPlateOrthoPoisson12")] = pp.Poisson12;
						doubles[ro.Const("ipPlateOrthoPoisson23")] = pp.Poisson23;
						doubles[ro.Const("ipPlateOrthoPoisson31")] = pp.Poisson31;
						doubles[ro.Const("ipPlateOrthoDensity")] = pp.Density;
						doubles[ro.Const("ipPlateOrthoAlpha1")] = pp.Expansion1;
						doubles[ro.Const("ipPlateOrthoAlpha2")] = pp.Expansion2;
						doubles[ro.Const("ipPlateOrthoAlpha3")] = pp.Expansion3;
						doubles[ro.Const("ipPlateOrthoViscosity")] = pp.Damping;
						doubles[ro.Const("ipPlateOrthoDampingRatio")] = pp.DampingRatio;
						doubles[ro.Const("ipPlateOrthoConductivity1")] = pp.ThermalCond1;
						doubles[ro.Const("ipPlateOrthoConductivity2")] = pp.ThermalCond2;
						doubles[ro.Const("ipPlateOrthoSpecificHeat")] = pp.SpecificHeat;
						ro.SetPlateOrthotropicMaterial(modelId, pp.Id, doubles);
						break;
					default:
						_output += string.Format("Unable to set the material of type {0}\r\n", pp.Material.ToString());
						break;
				}*/
                    }

                    plateProperties.Add(pp.Name, id++);
                }

                // Load cases
                Dictionary<string, int> loadCaseNameIdMap = new Dictionary<string, int>();

                id = 1;
                if (Model.SelfWeight != null)
                {
                    var sw = Model.SelfWeight;

                    if (HandleError(St7.St7SetLoadCaseName(modelId, id, sw.LoadCase.Name)))
                        warnings.Add(string.Format("Failed to rename the default load case in {0}", sw.LoadCase.Name));

                    //sw.LoadCase.Id = id;
                    loadCaseNameIdMap[sw.LoadCase.Name] = id;

                    int caseType = St7.lcNoInertia;

                    if (sw.GlobalInertiaLoad == SelfWeightModel.GlobalInertiaLoads.Gravity)
                        caseType = St7.lcGravity;

                    if (HandleError(St7.St7SetLoadCaseType(modelId, id, caseType)))
                        warnings.Add(string.Format("Failed to set load case type of the load case {0}", sw.LoadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseGravityDir(modelId, id, (int)sw.GravityDirection + 1)))
                        warnings.Add(string.Format("Failed to set gravity direction of the load case {0}", sw.LoadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseMassOption(modelId, id, Convert.ToByte(sw.StructuralMassAcceleration), Convert.ToByte(sw.NonStructuralMassAcceleration))))
                        warnings.Add(string.Format("Failed to set the mass options of the {0} load case", sw.LoadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseDefaults(modelId, id, new[] { 0, 0, 0, 0, 0, 0, sw.GravityValue, 0, 0, 0, 0, 0, 0 })))
                        warnings.Add(string.Format("Failed to set the gravity value of the {0} load case", sw.LoadCase.Name));
                    id++;
                }

                for (int i = 0; i < Model.LoadCases.Count; i++)
                {
                    LoadCaseModel loadCase = new LoadCaseModel(Model.LoadCases.ElementAt(i).Value);

                    if (HandleError(St7.St7NewLoadCase(modelId, loadCase.Name)))
                        warnings.Add(string.Format("Failed to define the load case {0}", loadCase.Name));

                    //loadCase.Id = id;
                    loadCaseNameIdMap[loadCase.Name] = id;

                    int caseType = St7.lcNoInertia;

                    if (HandleError(St7.St7SetLoadCaseType(modelId, id, caseType)))
                        warnings.Add(string.Format("Failed to set load case type of the load case {0}", loadCase.Name));

                    if (HandleError(St7.St7SetLoadCaseMassOption(modelId, id, Convert.ToByte(false), Convert.ToByte(loadCase.GravityNoStructuralMass))))
                        warnings.Add(string.Format("Failed to set the mass options of the {0} load case", loadCase.Name));
                    id++;
                }

                // Load Combinations
                int comboNumber = 1;
                for (int i = 0; i < Model.LoadCombinations.Count; i++)
                {
                    LoadCombinationModel combo = Model.LoadCombinations.ElementAt(i).Value;
                    // For linear static analysis
                    if (HandleError(St7.St7AddLSACombination(modelId, combo.Name)))
                    {
                        warnings.Add($"Failed to add the LSA combination: {combo.Name}");
                    }
                    else
                    {
                        foreach (LoadFactorModel item in combo.LoadFactorList)
                        {
                            int lc = Model.LoadCases.Single(l => l.Value.Name == item.LoadCase.Name).Value.Id;
                            if (HandleError(St7.St7SetLSACombinationFactor(modelId, St7.ltLoadCase, comboNumber, lc, 1, item.Factor)))
                                warnings.Add($"Failed to set the LSA combination factor: {combo.Name}");
                        }
                    }

                    /*
                    // For nonlinear static analysis
                    if (HandleError(St7.St7AddNLAIncrement(modelId, 0, combo.Name)))
                    {
                        warnings.Add($"Failed to add the NLA increment: {combo.Name}");
                    }
                    else
                    {
                        foreach (KeyValuePair<LoadCaseModel, double> item in combo.Values)
                        {
                            //int lc = Convert.ToInt32(loadCases.Single(l => l.Name == item.Key.Name).Id);
                            if (HandleError(St7.St7SetNLALoadIncrementFactor(modelId, 0, comboNumber, loadCaseNameIdMap[item.Key.Name], item.Value)))
                                warnings.Add($"Failed to set the NLA increment factor: {combo.Name}");
                            if (displLoadCase != null && displLoadCase.Name == item.Key.Name)
                            {
                                if (HandleError(St7.St7SetNLAFreedomIncrementFactor(modelId, 0, comboNumber, 1, item.Value)))
                                {
                                    warnings.Add($"Failed to set the NLA freedom case factor: {combo.Name}");
                                }
                            }
                        }
                    }*/
                    comboNumber++;
                }


                #region Multithread preprocessing

                // Checking only a single load case for node displacements. This load case will give coefficients to apply in all
                // combinations at the freedom case
                LoadCaseModel displLoadCase = null;
                Dictionary<int, int[]> beamsNodeNumberMap = new Dictionary<int, int[]>();
                Dictionary<int, int[]> plateNodeNumberMap = new Dictionary<int, int[]>();
                List<string> warningPreprocessingList = new List<string>();
                Dictionary<NodeElementModel, int> nodePositionMap = new Dictionary<NodeElementModel, int>();

                try
                {
                    for (int i = 0; i < Model.FrameElements.Count; i++)
                    {
                        var beam = Model.FrameElements.ElementAt(i);

                        List<NodeElementModel> fromNodes = Model.NodeElements.Where(n => n.Value.Position.DistanceTo(beam.Value.NodeStart.Position) < Model.ModelUnits.Tolerance).Select(k => k.Value).ToList();
                        List<NodeElementModel> toNodes = Model.NodeElements.Where(n => n.Value.Position.DistanceTo(beam.Value.NodeEnd.Position) < Model.ModelUnits.Tolerance).Select(k => k.Value).ToList();

                        if (fromNodes.Count() > 1)
                            warningPreprocessingList.Add(string.Format($"Collapsing {fromNodes.Count()} nodes at coordinates {beam.Value.NodeStart.Position}"));
                        if (toNodes.Count() > 1)
                            warningPreprocessingList.Add(string.Format($"Collapsing {toNodes.Count()} nodes at coordinates {beam.Value.NodeEnd.Position}"));

                        fromNodes.Sort(NodeElementModel.Comparer);
                        toNodes.Sort(NodeElementModel.Comparer);
                        NodeElementModel node1 = fromNodes.FirstOrDefault();
                        NodeElementModel node2 = toNodes.FirstOrDefault();

                        if (node1 == null || node2 == null)
                        {
                            warnings.Add($"Unreconized node(s) at coordinates {beam.Value.NodeEnd.Position} and / or {beam.Value.NodeStart.Position}");
                            continue;
                        }

                        int[] connections = new int[St7.kMaxElementNode + 1];
                        connections[0] = 2;
                        connections[1] = nodePositionMap[node1];
                        connections[2] = nodePositionMap[node2];

                        beamsNodeNumberMap[i] = connections;
                    }

                    for (int j = 0; j < Model.AreaElements.Count; j++)
                    {
                        AreaElementModel plate = Model.AreaElements.ElementAt(j).Value;

                        int[] connections = new int[St7.kMaxElementNode + 1];
                        connections[0] = plate.NodeList.Count;

                        for (int i = 0; i < plate.NodeList.Count; i++)
                        {
                            List<NodeElementModel> nodes = Model.NodeElements.Where(n => n.Value.Position.DistanceTo(plate.NodeList[i].Position) < Model.ModelUnits.Tolerance).Select(k => k.Value).ToList();
                            if (nodes.Count() > 1)
                                warningPreprocessingList.Add(string.Format($"Collapsing {nodes.Count()} nodes at coordinates {nodes.FirstOrDefault().Position}"));

                            nodes.Sort(NodeElementModel.Comparer);
                            connections[i + 1] = nodePositionMap[nodes.FirstOrDefault()];
                        }

                        plateNodeNumberMap[j] = connections;
                    }
                }
                catch (Exception ex)
                {
                    warnings.Add(string.Format($"Failed to preprocess the model. {ex.Message}"));
                }

                warnings.AddRange(warningPreprocessingList);

                #endregion

                #region Export nodi ed elementi

                // Nodes
                //List<int> addedNodes = new List<int>();
                HashSet<int> addedNodesNumber = new HashSet<int>();
                for (int i = 0; i < Model.NodeElements.Count; i++)
                {
                    NodeElementModel node = Model.NodeElements.ElementAt(i).Value;
                    try
                    {
                        int nodeNumber = i + 1;

                        if (!addedNodesNumber.Contains(nodeNumber))
                        {
                            St7.St7SetNodeXYZ(modelId, nodeNumber, new double[] { node.Position.X, node.Position.Y, node.Position.Z });
                            addedNodesNumber.Add(nodeNumber);
                        }
                        if (St7.St7SetNodeID(modelId, nodeNumber, node.Id) != 0)
                            warnings.Add(string.Format($"Failed to set Node ID {node.Id} on node {nodeNumber}."));

                        if (node.Support.IsActive)
                        {
                            try
                            {
                                int[] sts = new int[6];
                                sts[0] |= node.Support.Dx ? St7.btTrue : St7.btFalse;
                                sts[1] |= node.Support.Dy ? St7.btTrue : St7.btFalse;
                                sts[2] |= node.Support.Dz ? St7.btTrue : St7.btFalse;
                                sts[3] |= node.Support.Mx ? St7.btTrue : St7.btFalse;
                                sts[4] |= node.Support.My ? St7.btTrue : St7.btFalse;
                                sts[5] |= node.Support.Mz ? St7.btTrue : St7.btFalse;

                                double[] doubles = new double[6];

                                /*
                                //searching for displacements on the node
                                for (int j = 0; j < node.Loads.Count; j++)
                                {
                                    LoadModel l = node.Loads[j];
                                    if (l is NodeDisplacementLoadModel ndl)
                                    {
                                        doubles[0] = ndl.Value.Translation.X;
                                        doubles[1] = ndl.Value.Translation.Y;
                                        doubles[2] = ndl.Value.Translation.Z;
                                        doubles[3] = ndl.Value.Rotation.X;
                                        doubles[4] = ndl.Value.Rotation.Y;
                                        doubles[5] = ndl.Value.Rotation.Z;
                                        break;
                                    }
                                }*/
                                St7.St7SetNodeRestraint6(modelId, nodeNumber, 1, 1, sts, doubles);
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to set Node Restraint on {node.Id}. {ex.Message}"));
                                continue;
                            }
                        }
                        /*
                        if (node.HasAttribute(typeof(NodeStiffnessAttributeModel)))
                        {
                            try
                            {
                                NodeStiffnessAttributeModel.AttributeData stiffness = NodeStiffnessAttributeModel.GetStiffness(node);
                                int[] sts = new int[6];
                                St7.St7SetNodeKTranslation3F(modelId, nodeNumber, 1, 1, new double[] { stiffness.Tx, stiffness.Ty, stiffness.Tz });
                                St7.St7SetNodeKRotation3F(modelId, nodeNumber, 1, 1, new double[] { stiffness.Rx, stiffness.Ry, stiffness.Rz });
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to set node stiffness on node {node.NodeId}. {ex.Message}"));
                                continue;
                            }
                        }
                        */

                        for (int j = 0; j < node.NodalLoadList.Count; j++)
                        {
                            NodalLoadModel load = node.NodalLoadList[j];
                            int lc = loadCaseNameIdMap[load.LoadCase.Name];
                            if (load.GetType() == typeof(NodalLoadModel))
                            {
                                NodalLoadModel f = (NodalLoadModel)load;

                                double fx = 0, fy = 0, fz = 0;
                                St7.St7GetNodeForce3(modelId, nodeNumber, lc, new double[] { fx, fy, fz });
                                St7.St7SetNodeForce3(modelId, nodeNumber, lc, new double[] { f.FX + fx, f.FY + fy, f.FZ + fz });

                                double mx = 0, my = 0, mz = 0;
                                St7.St7SetNodeMoment3(modelId, nodeNumber, lc, new double[] { mx, my, mz });
                                St7.St7SetNodeMoment3(modelId, nodeNumber, lc, new double[] { f.MX + mx, f.MY + my, f.MZ + mz });
                            }
                            /*
                            else if (load.GetType() == typeof(NodeTemperatureLoadModel))
                            {
                                NodeTemperatureLoadModel t = (NodeTemperatureLoadModel)load;
                                St7.St7SetNodeTemperature1(modelId, nodeNumber, lc, new double[] { t.Value });
                            }*/
                        }
                    }
                    catch (Exception ex)
                    {
                        warnings.Add(string.Format($"Failed to create node {node.Id}. {ex.Message}"));
                        continue;
                    }
                }

                // Beams
                HashSet<int> addedBeamsId = new HashSet<int>();
                //var ordered = model.Beams.OrderBy(k => k.BeamId).ToArray();
                for (int k = 0; k < Model.FrameElements.Count; k++)
                {
                    FrameElementModel beam = Model.FrameElements.ElementAt(k).Value;
                    try
                    {
                        int beamNumber = k + 1;
                        int propNumber = beamProperties[beam.FrameProperty.Name];

                        if (!addedBeamsId.Contains(beamNumber))
                        {
                            try
                            {
                                if (St7.St7SetElementConnection(modelId, St7.tyBEAM, beamNumber, propNumber, beamsNodeNumberMap[k]) != 0)
                                {
                                    warnings.Add(string.Format($"Failed to set the connection of the beam {beam.Id} with nodes {beam.NodeStart.Id} and {beam.NodeEnd.Id}"));
                                    continue;
                                }

                                if (beam.Groups.Count > 0)
                                {
                                    if (HandleError(St7.St7SetEntityGroup(modelId, St7.tyBEAM, beamNumber, groupsIds[beam.Groups[0]])))
                                        warnings.Add(string.Format("Failed to set the group {0} to the beam {1}\r\n", beam.Groups[0], id));
                                    if (beam.Groups.Count > 1)
                                        warnings.Add(string.Format("Straus7 allows assigning only one single group to the beam {0}\r\n", id));
                                }

                                if (St7.St7SetBeamID(modelId, beamNumber, beam.Id) != 0)
                                    warnings.Add(string.Format($"Failed to set the ID {beam.Id} of the beam {beamNumber}."));

                                double angle = 90;
                                if (beam.Angle != 0)
                                    angle += beam.Angle;
                                if (RotateOf90Deg(beam.FrameProperty))
                                    angle += 90;
                                else if (RotateOf180Deg(beam.FrameProperty))
                                    angle += 180;

                                if (St7.St7SetBeamReferenceAngle1(modelId, beamNumber, new double[] { Rhino.RhinoMath.ToDegrees(angle) }) != 0)
                                    warnings.Add(string.Format($"Failed to set the reference angle of the beam {beam.Id}."));

                                if (beam.Offset != Rhino.Geometry.Vector2d.Unset && beam.Offset.Length > 0) //Rhino.Geometry.Vector2d or Maffeis.Geometry.Vector2d?
                                    St7.St7SetBeamOffset2(modelId, beamNumber, new double[] { beam.Offset.X, beam.Offset.Y });

                                addedBeamsId.Add(beamNumber);
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to create geometry of beam {beam.Id}. {ex.Message}"));
                                continue;
                            }
                        }
                        /*
                        // Releases
                        if (beam.HasAttribute(typeof(BeamReleasesAttributeModel)))
                        {
                            try
                            {
                                if (beam.Attributes.Where(x => x is BeamReleasesAttributeModel).Count() > 1)
                                {
                                    warnings.Add($"Cannot assign more than 1 set of releases for beam {beamNumber}");
                                }
                                BeamReleasesAttributeModel.AttributeData rel = BeamReleasesAttributeModel.GetRelease(beam);
                                if (rel.TransNode1.Count(b => b == true) > 0)
                                {
                                    int[] rel_status = new[]
                                    {
                                        rel.TransNode1[0] == false ? St7.brFixed :  (rel.TransN1Partial.X == 0 ? St7.brReleased : St7.brPartial),
                                        rel.TransNode1[1] == false ? St7.brFixed :  (rel.TransN1Partial.Y == 0 ? St7.brReleased : St7.brPartial),
                                        rel.TransNode1[2] == false ? St7.brFixed :  (rel.TransN1Partial.Z == 0 ? St7.brReleased : St7.brPartial),
                                    };
                                    double[] rel_values = new[] { rel.TransN1Partial.X, rel.TransN1Partial.Y, rel.TransN1Partial.Z };
                                    St7.St7SetBeamTRelease3(modelId, beamNumber, 1, rel_status, rel_values);
                                }
                                if (rel.RotNode1.Count(b => b == true) > 0)
                                {
                                    int[] rel_status = new[]
                                    {
                                        rel.RotNode1[0] == false ? St7.brFixed : (rel.RotN1Partial.X == 0 ? St7.brReleased : St7.brPartial),
                                        rel.RotNode1[1] == false ? St7.brFixed : (rel.RotN1Partial.Y == 0 ? St7.brReleased : St7.brPartial),
                                        rel.RotNode1[2] == false ? St7.brFixed : (rel.RotN1Partial.Z == 0 ? St7.brReleased : St7.brPartial),
                                    };
                                    double[] rel_values = new[] { rel.RotN1Partial.X, rel.RotN1Partial.Y, rel.RotN1Partial.Z };
                                    St7.St7SetBeamRRelease3(modelId, beamNumber, 1, rel_status, rel_values);
                                }
                                if (rel.TransNode2.Count(b => b == true) > 0)
                                {
                                    int[] rel_status = new[]
                                    {
                                        rel.TransNode2[0] == false ? St7.brFixed :  (rel.TransN2Partial.X == 0 ? St7.brReleased : St7.brPartial),
                                        rel.TransNode2[1] == false ? St7.brFixed :  (rel.TransN2Partial.Y == 0 ? St7.brReleased : St7.brPartial),
                                        rel.TransNode2[2] == false ? St7.brFixed :  (rel.TransN2Partial.Z == 0 ? St7.brReleased : St7.brPartial),
                                    };
                                    double[] rel_values = new[] { rel.TransN2Partial.X, rel.TransN2Partial.Y, rel.TransN2Partial.Z };
                                    St7.St7SetBeamTRelease3(modelId, beamNumber, 2, rel_status, rel_values);
                                }
                                if (rel.RotNode2.Count(b => b == true) > 0)
                                {
                                    int[] rel_status = new[]
                                    {
                                        rel.RotNode2[0] == false ? St7.brFixed : (rel.RotN2Partial.X == 0 ? St7.brReleased : St7.brPartial),
                                        rel.RotNode2[1] == false ? St7.brFixed : (rel.RotN2Partial.Y == 0 ? St7.brReleased : St7.brPartial),
                                        rel.RotNode2[2] == false ? St7.brFixed : (rel.RotN2Partial.Z == 0 ? St7.brReleased : St7.brPartial),
                                    };
                                    double[] rel_values = new[] { rel.RotN2Partial.X, rel.RotN2Partial.Y, rel.RotN2Partial.Z };
                                    St7.St7SetBeamRRelease3(modelId, beamNumber, 2, rel_status, rel_values);
                                }
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to set release to beam {beam.BeamId}. {ex.Message}"));
                                continue;
                            }
                        }

                        // Linear Support
                        if (beam.HasAttribute(typeof(BeamSupportAttributeModel)))
                        {
                            try
                            {
                                bool[] compressionOnly = new bool[3];
                                double[] supportStiffness = new double[3];

                                int directionIndex = 0;
                                bool reverseCompressionOnly = false;
                                for (int j = 0; j < beam.Attributes.Count; j++)
                                {
                                    if (beam.Attributes[j].GetType() == typeof(BeamSupportAttributeModel))
                                    {
                                        BeamSupportAttributeModel support = (BeamSupportAttributeModel)beam.Attributes[j];

                                        // Manuale Straus:
                                        // The direction of compression is in towards the negative beam axis direction.
                                        // Tension is towards the positive beam axis direction

                                        // supportato solo le molle con direzione degli assi locali
                                        if (support.Value.SpringTensionOrientation == BeamSupportAttributeModel.SpringTensionOrientation.BeamLocalAxis)
                                        {

                                            if (support.Value.SpringType == BeamSupportAttributeModel.SpringType.Simple)
                                            {
                                                switch (support.Value.LocalAxisDirection)
                                                {
                                                    case BeamSupportAttributeModel.LocalAxisDirection.negative1Axis:
                                                        directionIndex = 0;
                                                        break;

                                                    case BeamSupportAttributeModel.LocalAxisDirection.negative2Axis:
                                                        directionIndex = 1;
                                                        break;

                                                    case BeamSupportAttributeModel.LocalAxisDirection.negative3Axis:
                                                        directionIndex = 2;
                                                        break;

                                                    case BeamSupportAttributeModel.LocalAxisDirection.positive1Axis:
                                                        directionIndex = 0;
                                                        reverseCompressionOnly = true;
                                                        break;

                                                    case BeamSupportAttributeModel.LocalAxisDirection.positive2Axis:
                                                        directionIndex = 1;
                                                        reverseCompressionOnly = true;
                                                        break;

                                                    case BeamSupportAttributeModel.LocalAxisDirection.positive3Axis:
                                                        directionIndex = 2;
                                                        reverseCompressionOnly = true;
                                                        break;

                                                    default:
                                                        throw new NotSupportedException();
                                                }

                                                if (support.Value.SimpleSpringType == BeamSupportAttributeModel.SimpleSpringTypes.CompressionOnly)
                                                {
                                                    if (!reverseCompressionOnly)
                                                    {
                                                        compressionOnly[directionIndex] = true;
                                                    }
                                                }

                                                supportStiffness[directionIndex] = support.Value.Stiffness;
                                            }
                                            else
                                            {
                                                warnings.Add($"Only simple spring type is supported for beam support attribute. Beam {beamNumber}");
                                            }
                                        }
                                        else
                                        {
                                            warnings.Add($"Only local axis spring direction is supported for beam support attribute. Beam {beamNumber}");
                                        }
                                    }
                                }

                                if (compressionOnly.Distinct().Count() > 1)
                                {
                                    warnings.Add($"Compression only flag must be the equal for all supports attribute directions. Beam {beamNumber}");
                                }
                                else
                                {
                                    //TODO: implementare
                                    for (int i = 0; i < loadCases.Count; i++)
                                        St7.St7SetBeamSupport2(modelId, beamNumber, directionIndex, Convert.ToInt32(loadCases[i].Id),
                                            compressionOnly.First() ? St7.btTrue : St7.btFalse, new double[] { supportStiffness[0], supportStiffness[1] });
                                }
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to set support attribute to beam {beam.BeamId}. {ex.Message}"));
                                continue;
                            }
                        }

                        // Taper
                        if (beam.HasAttribute(typeof(BeamTaperAttributeModel)))
                        {
                            try
                            {
                                BeamTaperAttributeModel.AttributeData tap = BeamTaperAttributeModel.GetTaper(beam);
                                if (tap.NFromX != 1 || tap.NToX != 1)
                                {
                                    int code;
                                    switch (tap.AlignmentX)
                                    {
                                        case BeamTaperAttributeModel.AlignmentCodes.Bottom:
                                            code = St7.btBottom;
                                            break;
                                        case BeamTaperAttributeModel.AlignmentCodes.Center:
                                            code = St7.btSymm;
                                            break;
                                        case BeamTaperAttributeModel.AlignmentCodes.Top:
                                            code = St7.btTop;
                                            break;
                                        default:
                                            throw new NotSupportedException();
                                    }
                                    St7.St7SetBeamTaper2(modelId, beamNumber, St7.axLocalX, code, new double[2] { tap.NFromX, tap.NToX });
                                }
                                if (tap.NFromY != 1 || tap.NToY != 1)
                                {
                                    int code;
                                    switch (tap.AlignmentY)
                                    {
                                        case BeamTaperAttributeModel.AlignmentCodes.Bottom:
                                            code = St7.btBottom;
                                            break;
                                        case BeamTaperAttributeModel.AlignmentCodes.Center:
                                            code = St7.btSymm;
                                            break;
                                        case BeamTaperAttributeModel.AlignmentCodes.Top:
                                            code = St7.btTop;
                                            break;
                                        default:
                                            throw new NotSupportedException();
                                    }
                                    St7.St7SetBeamTaper2(modelId, beamNumber, St7.axLocalY, code, new double[2] { tap.NFromY, tap.NToY });
                                }
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to set taper attribute to beam {beam.BeamId}. {ex.Message}"));
                                continue;
                            }
                        }
                        */

                        // Loads
                        List<int> preLoadLC = new List<int>();
                        Dictionary<int, int> pointFPId = new Dictionary<int, int>();
                        Dictionary<int, int> pointFGId = new Dictionary<int, int>();
                        Dictionary<int, int> pointMPId = new Dictionary<int, int>();
                        Dictionary<int, int> pointMGId = new Dictionary<int, int>();
                        Dictionary<int, int> distrFPId = new Dictionary<int, int>();
                        Dictionary<int, int> distrFGId = new Dictionary<int, int>();

                        for (int j = 0; j < beam.FrameLoadList.Count; j++)
                        {
                            FrameLoadModel load = beam.FrameLoadList[j];
                            try
                            {
                                int lc = loadCaseNameIdMap[load.LoadCase.Name];
                                /*
                                if (load.GetType() == typeof(BeamPreLoadModel))
                                {
                                    BeamPreLoadModel pl = (BeamPreLoadModel)load;
                                    int type = pl.PreLoadType == BeamPreLoadModel.LoadType.Strain ? St7.plBeamPreStrain : St7.plBeamPreTension;
                                    St7.St7SetBeamPreLoad1(modelId, beamNumber, lc, type, new double[] { pl.Value });
                                    if (preLoadLC.Contains(lc))
                                    {
                                        warnings.Add("Beam " + beamNumber + " has multiple prestrain in loadcase " + load.LoadCase.Name);
                                    }
                                    else
                                    {
                                        preLoadLC.Add(lc);
                                    }
                                }
                                else if (load.GetType() == typeof(BeamDistributedLoadModel))
                                {
                                    BeamDistributedLoadModel dl = (BeamDistributedLoadModel)load;
                                    int type = 0;
                                    int loadId;
                                    switch (dl.Value.LoadSchema)
                                    {
                                        case BeamDistributedLoadModel.LoadSchema.Uniform:
                                            type = St7.dlConstant;
                                            break;
                                        case BeamDistributedLoadModel.LoadSchema.Keystone:
                                            type = St7.dlLinear;
                                            break;
                                        case BeamDistributedLoadModel.LoadSchema.Triangular:
                                            type = St7.dlTriangular;
                                            break;
                                        case BeamDistributedLoadModel.LoadSchema.TriangularEnd1_Keystone:
                                            type = St7.dlThreePoint0;
                                            break;
                                        case BeamDistributedLoadModel.LoadSchema.Keystone_TriangularEnd2:
                                            type = St7.dlThreePoint1;
                                            break;
                                        case BeamDistributedLoadModel.LoadSchema.TriangularEnds_Keystone:
                                            type = St7.dlTrapezoidal;
                                            break;
                                    }
                                    if (dl.Value.CoordinateSystem == null)
                                    {
                                        int dir = (int)dl.Value.LoadDirection + 1;
                                        if (distrFPId.TryGetValue(lc, out loadId))
                                        {
                                            loadId++;
                                            distrFPId[lc] = loadId;
                                        }
                                        else
                                        {
                                            loadId = 1;
                                            distrFPId.Add(lc, loadId);
                                        }
                                        if (dl.DistributedLoadType == BeamDistributedLoadModel.LoadType.Force)
                                            St7.St7SetBeamDistributedForcePrincipal6ID(modelId, beamNumber, dir, lc, type, loadId, new[] { dl.Value.PA, dl.Value.PB,
                                        dl.Value.P1, dl.Value.P2, dl.Value.A, dl.Value.B });
                                        else
                                            St7.St7SetBeamDistributedMomentPrincipal6ID(modelId, beamNumber, dir, lc, type, loadId, new[] { dl.Value.PA, dl.Value.PB,
                                        dl.Value.P1, dl.Value.P2, dl.Value.A, dl.Value.B });
                                    }
                                    else
                                    {
                                        int dir = 0;
                                        switch (dl.Value.LoadDirection)
                                        {
                                            case BeamDistributedLoadModel.LoadDirection.X:
                                            case BeamDistributedLoadModel.LoadDirection.X_Projected:
                                                dir = 1;
                                                break;
                                            case BeamDistributedLoadModel.LoadDirection.Y:
                                            case BeamDistributedLoadModel.LoadDirection.Y_Projected:
                                                dir = 2;
                                                break;
                                            case BeamDistributedLoadModel.LoadDirection.Z:
                                            case BeamDistributedLoadModel.LoadDirection.Z_Projected:
                                            case BeamDistributedLoadModel.LoadDirection.Gravity:
                                            case BeamDistributedLoadModel.LoadDirection.Gravity_Projected:
                                                dir = 3;
                                                break;
                                        }
                                        int projectFlag = 0;
                                        switch (dl.Value.LoadDirection)
                                        {
                                            case BeamDistributedLoadModel.LoadDirection.X:
                                            case BeamDistributedLoadModel.LoadDirection.Y:
                                            case BeamDistributedLoadModel.LoadDirection.Z:
                                            case BeamDistributedLoadModel.LoadDirection.Gravity:
                                                projectFlag = St7.btFalse;
                                                break;
                                            case BeamDistributedLoadModel.LoadDirection.X_Projected:
                                            case BeamDistributedLoadModel.LoadDirection.Y_Projected:
                                            case BeamDistributedLoadModel.LoadDirection.Z_Projected:
                                            case BeamDistributedLoadModel.LoadDirection.Gravity_Projected:
                                                projectFlag = St7.btTrue;
                                                break;
                                        }
                                        double[] values = new[] { dl.Value.PA, dl.Value.PB, dl.Value.P1, dl.Value.P2, dl.Value.A, dl.Value.B };
                                        if (dl.Value.LoadDirection == BeamDistributedLoadModel.LoadDirection.Gravity ||
                                            dl.Value.LoadDirection == BeamDistributedLoadModel.LoadDirection.Gravity_Projected)
                                        {
                                            values[0] = -dl.Value.PA != 0 ? -dl.Value.PA : 0;
                                            values[1] = -dl.Value.PB != 0 ? -dl.Value.PB : 0;
                                            values[2] = -dl.Value.P1 != 0 ? -dl.Value.P1 : 0;
                                            values[3] = -dl.Value.P2 != 0 ? -dl.Value.P2 : 0;
                                        }

                                        if (distrFGId.TryGetValue(lc, out loadId))
                                        {
                                            loadId++;
                                            distrFGId[lc] = loadId;
                                        }
                                        else
                                        {
                                            loadId = 1;
                                            distrFGId.Add(lc, loadId);
                                        }

                                        St7.St7SetBeamDistributedForceGlobal6ID(modelId, beamNumber, dir, projectFlag, lc, type, loadId, values);
                                    }
                                }
                                else if (load.GetType() == typeof(BeamPointLoadModel))
                                {
                                    BeamPointLoadModel pl = (BeamPointLoadModel)load;
                                    double[] values = new[] { pl.Value.P.X, pl.Value.P.Y, pl.Value.P.Z, pl.Value.A };
                                    int loadId;
                                    if (pl.PointLoadType == BeamPointLoadModel.LoadType.Force)
                                    {
                                        if (pl.Value.CoordinateSystem == null)
                                        {
                                            if (pointFPId.TryGetValue(lc, out loadId))
                                            {
                                                loadId++;
                                                pointFPId[lc] = loadId;
                                            }
                                            else
                                            {
                                                loadId = 1;
                                                pointFPId.Add(lc, loadId);
                                            }
                                            St7.St7SetBeamPointForcePrincipal4ID(modelId, beamNumber, lc, loadId, values);
                                        }
                                        else
                                        {
                                            if (pointFGId.TryGetValue(lc, out loadId))
                                            {
                                                loadId++;
                                                pointFGId[lc] = loadId;
                                            }
                                            else
                                            {
                                                loadId = 1;
                                                pointFGId.Add(lc, loadId);
                                            }
                                            St7.St7SetBeamPointForceGlobal4ID(modelId, beamNumber, lc, loadId, values);
                                        }
                                    }
                                    else
                                    {
                                        if (pl.Value.CoordinateSystem == null)
                                        {
                                            if (pointMPId.TryGetValue(lc, out loadId))
                                            {
                                                loadId++;
                                                pointMPId[lc] = loadId;
                                            }
                                            else
                                            {
                                                loadId = 1;
                                                pointMPId.Add(lc, loadId);
                                            }
                                            St7.St7SetBeamPointMomentPrincipal4ID(modelId, beamNumber, lc, loadId, values);
                                        }
                                        else
                                        {
                                            if (pointMGId.TryGetValue(lc, out loadId))
                                            {
                                                loadId++;
                                                pointMGId[lc] = loadId;
                                            }
                                            else
                                            {
                                                loadId = 1;
                                                pointMGId.Add(lc, loadId);
                                            }
                                            St7.St7SetBeamPointMomentGlobal4ID(modelId, beamNumber, lc, loadId, values);
                                        }
                                    }
                                }
                                else if (load.GetType() == typeof(BeamGradientLoadModel))
                                {
                                    try
                                    {
                                        BeamGradientLoadModel gl = (BeamGradientLoadModel)load;
                                        St7.St7SetBeamTempGradient2(modelId, beamNumber, lc, new double[] { gl.Value.Axis1, gl.Value.Axis2 });
                                    }
                                    catch (Exception ex)
                                    {
                                        warnings.Add(string.Format($"Failed to set string group to beam {beam.BeamId}. {ex.Message}"));
                                        continue;
                                    }
                                }
                                */
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to set load case {load.LoadCase.Id} to beam {beam.Id}. {ex.Message}"));
                                continue;
                            }
                        }

                        ////StringGroup
                        //if (beam.HasAttribute(typeof(BeamStringGroupAttributeModel)))
                        //{
                        //    BeamStringGroupAttributeModel.AttributeData sg = BeamStringGroupAttributeModel.GetStringGroup(beam);
                        //    int idSG = sg.ID;
                        //    St7.St7SetBeamStringGroup1(modelId, beamNumber, idSG);
                        //}
                    }
                    catch (Exception ex)
                    {
                        warnings.Add(string.Format($"Failed to create beam {beam.Id}. {ex.Message}"));
                        continue;
                    }
                }

                // Plates
                HashSet<int> addedPlatesId = new HashSet<int>();
                for (int k = 0; k < Model.AreaElements.Count; k++)
                {
                    var plate = Model.AreaElements.ElementAt(k).Value;
                    try
                    {
                        int plateNumber = k + 1;

                        if (!addedPlatesId.Contains(plateNumber))
                        {
                            //int[] connections = new int[St7ApiWrapper.St7ApiConst.kMaxElementNode + 1];
                            //connections[0] = plate.Points.Count;
                            //for (int i = 0; i < plate.Points.Count; i++)
                            //    connections[i + 1] = Convert.ToInt32(model.Nodes.Single(n => n.Position.DistanceToSquared(plate.Points[i]) < squaredTol).NodeId);

                            try
                            {
                                int propId = plateProperties[plate.AreaProperty.Name];
                                if (HandleError(St7.St7SetElementConnection(modelId, St7.tyPLATE, plateNumber, propId, plateNodeNumberMap[k])))
                                {
                                    warnings.Add($"Failed to define the plate {id}");
                                    continue;
                                }
                                if (St7.St7SetPlateID(modelId, plateNumber, plate.Id) != 0)                                
                                    warnings.Add($"Failed to set the ID to the plate {plate.Id}");

                                if (plate.Groups.Count > 0) // We consider only the first group as Straus7 allows
                                {
                                    if (HandleError(St7.St7SetEntityGroup(modelId, St7.tyPLATE, plateNumber, groupsIds[plate.Groups[0]])))
                                        warnings.Add($"Failed to set the group {plate.Groups[0]} to the plate {id}");
                                    if (plate.Groups.Count > 1)
                                        warnings.Add($"Straus7 allows assigning only one single group to the beam {id}");
                                }
                                if (plate.Angle != 0)
                                {
                                    St7.St7SetPlateXAngle1(modelId, plateNumber, new[] { Rhino.RhinoMath.ToDegrees(plate.Angle) });
                                }
                                if (plate.Offset != 0)
                                {
                                    if (HandleError(St7.St7SetPlateOffset1(modelId, plateNumber, new double[] { plate.Offset })))
                                        warnings.Add($"Failed to set the offset to the plate {id}");
                                }

                                addedPlatesId.Add(plateNumber);
                            }
                            catch (Exception ex)
                            {
                                warnings.Add(string.Format($"Failed to create geometry of plate {plate.Id}. {ex.Message}"));
                                continue;
                            }
                        }

                        /*
                        // Attributes
                        for (int j = 0; j < plate.Attributes.Count; j++)
                        {
                            AttributeModel attribute = plate.Attributes[j];
                            // Load Patch                       
                            if (attribute is LoadPatchTypeAttributeModel lp)
                            {
                                try
                                {
                                    int patchType = 0;
                                    switch (lp.Value.PatchType)
                                    {
                                        case LoadPatchTypeAttributeModel.PatchType.Auto1:
                                            patchType = St7.ptAuto1;
                                            break;
                                        case LoadPatchTypeAttributeModel.PatchType.Auto2:
                                            patchType = St7.ptAuto2;
                                            break;
                                        case LoadPatchTypeAttributeModel.PatchType.Auto3:
                                            patchType = St7.ptAuto3;
                                            break;
                                        case LoadPatchTypeAttributeModel.PatchType.Auto4:
                                            patchType = St7.ptAuto4;
                                            break;
                                        case LoadPatchTypeAttributeModel.PatchType.AngleSplit:
                                            patchType = St7.ptAngleSplit;
                                            break;
                                        case LoadPatchTypeAttributeModel.PatchType.Manual:
                                            patchType = St7.ptManual;
                                            break;
                                    }
                                    string edgesString = "";
                                    for (int e = 1; e <= 4; e++)
                                    {
                                        if (lp.Value.SelectedEdges.Contains(e))
                                            edgesString = "1" + edgesString;
                                        else
                                            edgesString = "0" + edgesString;
                                    }
                                    int edgesBits = Convert.ToInt32(edgesString, 2);
                                    double[] doubles = lp.Value.Weights.ToArray();
                                    St7.St7SetPlateLoadPatch4(modelId, plateNumber, patchType, edgesBits, doubles);
                                }
                                catch (Exception ex)
                                {
                                    warnings.Add(string.Format($"Failed to set load patch {lp.Value.PatchType} on plate {plate.PlateId}. {ex.Message}"));
                                    continue;
                                }
                            }
                            // Face support
                            else if (attribute is PlateSupportAttributeModel support)
                            {
                                try
                                {
                                    int cOnly;
                                    if (support.Value.CompressionOnly)
                                    {
                                        cOnly = St7.btTrue;
                                    }
                                    else
                                    {
                                        cOnly = St7.btFalse;
                                    }
                                    for (int i = 0; i < loadCases.Count; i++)
                                    {
                                        St7.St7SetPlateFaceSupport4(modelId, plateNumber, St7.psPlateMinusZ, Convert.ToInt32(loadCases[i].Id),
                                            new int[] { cOnly, St7.btFalse }, new double[] { support.Value.Stiffness, 0, 0, 0 });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    warnings.Add(string.Format($"Failed to set support on plate {plate.PlateId}. {ex.Message}"));
                                    continue;
                                }
                            }
                        }
                        */

                        // Loads
                        for (int i = 0; i < plate.AreaLoadList.Count; i++)
                        {
                            AreaLoadMidasModel load = plate.AreaLoadList[i];
                            
                            {
                                int lc = loadCaseNameIdMap[load.LoadCase.Name];
                                
                                if (load is AreaLoadMidasModel pp)
                                {
                                    try
                                    {
                                        if (pp.CoordinateSystem == null)
                                        {
                                            St7.St7SetPlateNormalPressure2(modelId, plateNumber, lc, new double[] { pp.Value.P.Z });
                                        }
                                        else
                                        {
                                            int projectFlag = pp.IsProjected ? St7.btTrue : St7.btFalse;
                                            int face = St7.psPlatePlusZ;
                                            //int face = pp.Value.Face == PlatePressureModel.LoadFace.Top ? St7.psPlatePlusZ : St7.psPlateMinusZ;
                                            St7.St7SetPlateGlobalPressure3S(modelId, plateNumber, face, projectFlag, lc, new double[] { pp.Value.P.X, pp.Value.P.Y, pp.Value.P.Z });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        warnings.Add(string.Format($"Failed to set load case {lc} on plate {plate.Id}. {ex.Message}"));
                                        continue;
                                    }
                                }
                                /*
                                else if (load is PlatePreStressModel pps)
                                {
                                    try
                                    {
                                        St7.St7SetPlatePreLoad3(modelId, plateNumber, lc, St7.plPlatePreStress, new double[] { pps.Value.LocalPressure.X, pps.Value.LocalPressure.Y, pps.Value.LocalPressure.Z });
                                    }
                                    catch (Exception ex)
                                    {
                                        warnings.Add(string.Format($"Failed to set load patch {lc} on plate {plate.PlateId}. {ex.Message}"));
                                        continue;
                                    }
                                }
                                else if (load is PlateNSMassModel ns)
                                {
                                    try
                                    {
                                        St7.St7SetPlateNSMass5ID(modelId, plateNumber, lc, 1, new double[5] { ns.Value.Mass, ns.Value.DynFactor, ns.Value.Offset.X, ns.Value.Offset.Y, ns.Value.Offset.Z });
                                    }
                                    catch (Exception ex)
                                    {
                                        warnings.Add(string.Format($"Failed to set load patch {lc} on plate {plate.PlateId}. {ex.Message}"));
                                        continue;
                                    }
                                }*/
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        warnings.Add(string.Format($"Failed to create plate {plate.Id}. {ex.Message}"));
                        continue;
                    }
                }

                #endregion

                /*
                // StagedLoadCase
                for (int i = 0; i < loadCases.Count; i++)
                {
                    LoadCaseModel lc = loadCases[i];
                    if (lc.Stages != null)
                    {
                        if (lc.Stages.Any(k => k.BridgePhase != LoadCaseModel.Stage.BridgePhases.Unassigned))
                        {
                            warnings.Add("Stages on load cases not supported");
                            break;
                        }
                    }
                }

                // BXS sections
                if (bxsFiles.Count > 0)
                {
                    try
                    {
                        // Save and close the model file
                        St7.St7SaveFile(modelId);
                        St7.St7CloseFile(modelId);

                        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("FeMM.Common")).First();
                        string support_path = Path.GetDirectoryName(assembly.Location);
                        foreach (KeyValuePair<int, string> bxs in bxsFiles)
                        {
                            var enviromentPath = Environment.GetEnvironmentVariable("PATH");
                            ProcessStartInfo pInfo = new ProcessStartInfo
                            {
                                FileName = Path.Combine(support_path, "St7Solver.exe"),
                                Arguments = string.Format("\"{0}\" {1} \"{2}\" {3}", outputPath, -1, bxs.Value, bxs.Key),
                                WorkingDirectory = scratchPath,
                                UseShellExecute = false
                            };
                            // Ensures that the Straus7 binary directory is in the path environment variable
                            string programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                            pInfo.EnvironmentVariables["PATH"] += $";{programFilesPath}\\Straus7 R24\\Bin\\";

                            if (!File.Exists(pInfo.FileName))
                            {
                                errors.Add("Cannot find the ST7 solver wrapper");
                                return false;
                            }
                            else
                            {
                                Process p = Process.Start(pInfo);
                                // Wait for the process to end.
                                p.WaitForExit();

                                if (p.ExitCode != 0)
                                {
                                    errors.Add("Unable to assign bxs section of prop " + id.ToString());
                                    return false;
                                }
                            }
                        }
                        // Re-open current file
                        St7.St7OpenFile(modelId, outputPath, scratchPath);
                    }
                    catch (Exception ex)
                    {
                        warnings.Add(string.Format($"Failed to create bxs. {ex.Message}"));
                    }
                }

                // Defines the Stages for the Staged Analysis
                if (model.StagedConstruction.Count() > 0)
                {
                    int stage = 1;
                    StagedConstructionModel[] stagedConstArr = model.StagedConstruction.ToArray();
                    for (int i = 0; i < stagedConstArr.Length; i++)
                    {
                        try
                        {
                            StagedConstructionModel stagedConstruction = stagedConstArr[i];
                            int[] integers = new[]
                            {
                            stagedConstruction.Marph ? St7.btTrue : St7.btFalse, St7.btFalse,
                            stagedConstruction.Rotate ? St7.btTrue : St7.btFalse
                        };
                            St7.St7AddStage(modelId, stagedConstruction.Name, integers);

                            for (int k = 0; k < groups.Length; k++)
                            {
                                GroupModel group = groups[k];
                                if (stagedConstruction.Groups.Contains(group))
                                    St7.St7EnableStageGroup(modelId, stage, groups_ids[group]);
                                else
                                    St7.St7DisableStageGroup(modelId, stage, groups_ids[group]);
                            }

                            comboNumber = 1;
                            LoadCombinationModel[] stageConArray = stagedConstruction.LoadCombinations.ToArray();
                            for (int kk = 0; kk < stageConArray.Length; kk++)
                            {
                                LoadCombinationModel combo = stageConArray[kk];
                                if (HandleError(St7.St7AddNLAIncrement(modelId, stage, combo.Name)))
                                {
                                    warnings.Add($"Failed to add the NLA increment {combo.Name}");
                                }
                                else
                                {
                                    foreach (KeyValuePair<LoadCaseModel, double> item in combo.Values)
                                    {
                                        //int lc = Convert.ToInt32(loadCases.Single(l => l.Name == item.Key.Name).Id);

                                        if (HandleError(St7.St7SetNLALoadIncrementFactor(modelId, stage, comboNumber, loadCaseNameIdMap[item.Key.Name], item.Value)))
                                            warnings.Add($"Failed to set the NLA increment factor {combo.Name}");

                                        if (displLoadCase != null && displLoadCase.Name == item.Key.Name)
                                        {
                                            if (HandleError(St7.St7SetNLAFreedomIncrementFactor(modelId, stage, comboNumber, 1, item.Value)))
                                            {
                                                warnings.Add($"Failed to set the NLA freedom case factor {combo.Name}");
                                            }
                                        }
                                    }
                                }
                                comboNumber++;
                            }

                            stage++;
                        }
                        catch (Exception ex)
                        {
                            warnings.Add(string.Format($"Failed to create stage {stagedConstArr[i].Name}. {ex.Message}"));
                            continue;
                        }
                    }
                }
                */

                if (st7FileOpened)
                {
                    St7.St7SaveFile(modelId);
                    St7.St7CloseFile(modelId);
                    ReleaseModelId(modelId);
                }

#if !DEBUG
				// Save statistics
				try
				{

				}
				catch (Exception) { }
#endif
                return true;
            }
            catch (Exception e)
            {
                errors.Add(e.Message);

                if (st7FileOpened)
                {
                    St7.St7SaveFile(modelId);
                    St7.St7CloseFile(modelId);
                    ReleaseModelId(modelId);
                }

                return false;
            }
        }

        private static bool HandleError(int errorCode)
        {
            StringBuilder sbErrorString;

            if (errorCode != St7.ERR7_NoError) // Returned an error
            {
                Exception exception = null;

                sbErrorString = new StringBuilder(St7.kMaxStrLen);
                if (St7.ERR7_NoError == St7.St7GetAPIErrorString(errorCode, sbErrorString, sbErrorString.Capacity))
                {
                    exception = new Exception(sbErrorString.ToString());
                }
                else if (St7.ERR7_NoError == St7.St7GetSolverErrorString(errorCode, sbErrorString, sbErrorString.Capacity))
                {
                    exception = new Exception(sbErrorString.ToString());
                }
                if (exception != null)
                {
                    exception.Data.Add("ErrorCode", errorCode);
                    throw exception;
                }
                return true;
            }
            else // No errors
            {
                return false;
            }
        }

        public static bool RotateOf90Deg(FramePropertyModel beam)
        {
            return beam.Section.SectionGeometryType == FrameSectionModel.SectionGeometryTypes.L;
        }

        public static bool RotateOf180Deg(FramePropertyModel beam)
        {
            return beam.Section.Angle < -0.0001;
        }

        private static int GetAvailableModelId()
        {

            for (int i = 1; i < _openedModelsCount.Length; i++)
            {
                if (!_openedModelsCount[i])
                {
                    _openedModelsCount[i] = true;
                    return i;
                }
            }

            return -1;
        }

        public static void ReleaseModelId(int modelId)
        {
            if (modelId > 32 || modelId < 1)
                throw new IndexOutOfRangeException();

            _openedModelsCount[modelId] = false;
        }
    }
}
