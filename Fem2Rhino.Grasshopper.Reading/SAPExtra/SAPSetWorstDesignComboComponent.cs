using SAP2000v1;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Rhino2Fem.Core.Helper;

namespace Rhino2Fem.Grasshopper.Components.SAPExtra
{
    public class SAPGetWorstComboComponent : GH_Component
    {
        protected Dictionary<int, string> _stations = new Dictionary<int, string>()
        {
            { 0, "I"},
            { 1, "J"},
            { 2, "Ends"},
            { 3, "EndsAndMiddle"},
            { 4, "AllStations" }
        };

        private bool _run = false;

        /// <summary>
        /// Initializes a new instance of the BeamResultFilterComponent class.
        /// </summary>
        public SAPGetWorstComboComponent()
          : base("12+2 Worst combo", "12+2C", "Return the 12+2 worst combinations", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
        {
        }

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
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Model Path", "MP", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Groups", "G", "Groups for beams", GH_ParamAccess.list);
            pManager.AddGenericParameter("Combinations", "C", "Combinations", GH_ParamAccess.list);
            int i = pManager.AddIntegerParameter("Stations", "S", "The stations where calculate the worst combos", GH_ParamAccess.item, 4);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _stations)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Worst Combos", "C", "The combinations filtered", GH_ParamAccess.list);
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
            List<string> combosWanted = new List<string>();
            int stat = 0;

            DA.GetData(0, ref modelPath);
            DA.GetData(1, ref attachToInstance);
            if (!DA.GetDataList(2, groupWanted))
                return;
            if (!DA.GetDataList(3, combosWanted))
                return;
            DA.GetData(4, ref stat);


            if (_run)
            {
                try
                {
                    if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sap model path not valid, opening an empty file");
                        return;
                    }

                    ProxyHelper.CSI.ApiWrapper.SAPApiWrapper wr = new ProxyHelper.CSI.ApiWrapper.SAPApiWrapper();
                    if (!wr.InitializeModel(!attachToInstance, "", modelPath, out cOAPI _mySapObject, out cSapModel _mySapModel, out cHelper myHelper))
                    {
                        _run = false;
                        Message = "SAP Fail";
                        return;
                    }

                    if (!_mySapObject.SapModel.GetModelIsLocked())
                    {
                        _run = false;
                        Message = "SAP Fail";
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plase run the analysis");
                        return;
                    }

                    if (_mySapModel.SelectObj.ClearSelection() != 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error deselect previous groups for output");
                        _run = false;
                        return;
                    }

                    int nGroups = 0;
                    string[] allGroups = new string[0];
                    if (_mySapModel.GroupDef.GetNameList(ref nGroups, ref allGroups) != 0)
                        return;
                    Array.Sort(allGroups);

                    int groupRead = 0;
                    for (int i = 0; i < groupWanted.Count; i++)
                    {
                        if (allGroups.Contains(groupWanted[i]))
                        {
                            if (groupWanted[i] != "ALL")
                            {
                                _mySapModel.SelectObj.Group(groupWanted[i]);
                                groupRead++;
                            }
                            else
                            {
                                if (_mySapModel.SelectObj.All() != 0)
                                {
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error during selection of {groupWanted[i]}");
                                    _run = false;
                                    return;
                                }
                            }
                        }
                    }


                    int nCombos = 0;
                    string[] allCombos = new string[0];
                    if (_mySapModel.RespCombo.GetNameList(ref nCombos, ref allCombos) != 0)
                        return;
                    Array.Sort(allCombos);

                    if (_mySapObject.SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput() != 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error deselect previous combo for output");
                        _run = false;
                        return;
                    }

                    int comboRead = 0;
                    for (int i = 0; i < allCombos.Count(); i++)
                    {
                        if (combosWanted.Contains(allCombos[i]))
                        {
                            if (_mySapObject.SapModel.Results.Setup.SetCaseSelectedForOutput(allCombos[i]) == 0)
                            {
                                comboRead++;
                            }
                            else
                            {
                                if (_mySapObject.SapModel.Results.Setup.SetComboSelectedForOutput(allCombos[i]) == 0)
                                {
                                    comboRead++;
                                }
                                else
                                {
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error set combo/load case of output");
                                    _run = false;
                                    return;
                                }
                            }
                        }
                    }

                    int nrBeams = 0;
                    string[] nameBeams = new string[0];
                    _mySapModel.FrameObj.GetNameList(ref nrBeams, ref nameBeams);


                    List<FrameForcesResultsStructSigma> resultsStructSigmas = new List<FrameForcesResultsStructSigma>();

                    for (int i = 0; i < nrBeams; i++)
                    {
                        string name_beam = nameBeams[i];

                        //get groups of beam
                        int nr_groups_beam = 0;
                        string[] groups_beam = new string[0];
                        if (_mySapModel.FrameObj.GetGroupAssign(name_beam, ref nr_groups_beam, ref groups_beam) != 0)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No group assign for beam {name_beam}");
                            _run = false;
                            return;
                        }

                        bool flag = false;

                        for (int j = 0; j < groups_beam.Length; j++)
                        {
                            if (groupWanted.Contains(groups_beam[j]))
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (flag)
                        {
                            int number_result = 0;
                            string[] obj = new string[0];
                            double[] obj_station = new double[0];
                            string[] element = new string[0];
                            double[] element_station = new double[0];
                            string[] load_case = new string[0];
                            string[] step_type = new string[0];
                            double[] step_num = new double[0];
                            double[] P = new double[0];
                            double[] V2 = new double[0];
                            double[] V3 = new double[0];
                            double[] T = new double[0];
                            double[] M2 = new double[0];
                            double[] M3 = new double[0];

                            //get results
                            if (_mySapObject.SapModel.Results.FrameForce(name_beam, eItemTypeElm.ObjectElm, ref number_result, ref obj,
                                ref obj_station, ref element, ref element_station, ref load_case, ref step_type, ref step_num,
                                ref P, ref V2, ref V3, ref T, ref M2, ref M3) == 0)
                            {
                                double length = obj_station[number_result - 1];
                                //get section data for calculation of stress
                                string property_name = "";
                                string SAuto = "";
                                if (_mySapModel.FrameObj.GetSection(name_beam, ref property_name, ref SAuto) != 0)
                                    return;

                                //get frame section properties
                                double Area = 0, As2 = 0, As3 = 0, Torsion = 0, I22 = 0, I33 = 0, S22 = 0, S33 = 0, Z22 = 0, Z33 = 0, R22 = 0, R33 = 0;
                                if (_mySapModel.PropFrame.GetSectProps(property_name, ref Area, ref As2, ref As3, ref Torsion, ref I22,
                                    ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33) != 0)
                                    return;

                                //check if round section
                                eFramePropType type_section = 0;
                                if (_mySapModel.PropFrame.GetTypeOAPI(property_name, ref type_section) != 0)
                                    return;

                                bool flagCircleSection = false;
                                if (type_section == eFramePropType.Pipe || type_section == eFramePropType.Circle)
                                {
                                    flagCircleSection = true;
                                }

                                List<FrameForcesResultsStructSigma> resultsStructSigmasBuffer = new List<FrameForcesResultsStructSigma>(number_result);

                                for (int j = 0; j < number_result; j++)
                                {
                                    try
                                    {
                                        double multiplier = 1.0;

                                        P[j] = multiplier * P[j];
                                        V2[j] = multiplier * V2[j];
                                        V3[j] = multiplier * V3[j];
                                        M2[j] = multiplier * M2[j];
                                        M3[j] = multiplier * M3[j];
                                        T[j] = multiplier * T[j];

                                        double[] sigma = new double[4];
                                        if (flagCircleSection == false)
                                        {
                                            sigma[0] = P[j] / Area + M2[j] / S22 + M3[j] / S33;
                                            sigma[1] = P[j] / Area + M2[j] / S22 - M3[j] / S33;
                                            sigma[2] = P[j] / Area - M2[j] / S22 + M3[j] / S33;
                                            sigma[3] = P[j] / Area - M2[j] / S22 - M3[j] / S33;
                                        }
                                        else
                                        {
                                            double M = Math.Pow(Math.Pow(M2[j], 2.0) + Math.Pow(M3[j], 2.0), 0.5);
                                            sigma[0] = P[j] / Area + M / S22;
                                            sigma[1] = P[j] / Area - M / S22;
                                            sigma[2] = sigma[0];
                                            sigma[3] = sigma[1];
                                        }

                                        double sigmaMax = sigma.Max();
                                        double sigmaMin = sigma.Min();

                                        FrameForcesResultsStructSigma r = new FrameForcesResultsStructSigma
                                        {
                                            SectionName = property_name,
                                            NameBeam = obj[j],
                                            Station = obj_station[j],
                                            LoadCase = load_case[j],
                                            N = P[j],
                                            V2 = V2[j],
                                            V3 = V3[j],
                                            M2 = M2[j],
                                            M3 = M3[j],
                                            T = T[j],
                                            SigmaMax = sigmaMax,
                                            SigmaMin = sigmaMin
                                        };

                                        resultsStructSigmasBuffer.Add(r);
                                    }
                                    catch (Exception ex)
                                    {
                                        {
                                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message + " " + ex.StackTrace);
                                            _run = false;
                                            return;
                                        }
                                    }
                                }

                                #region Max/Min

                                int stations;
                                switch (stat)
                                {
                                    case 0:
                                    case 1:
                                        stations = 1;
                                        break;
                                    case 2:
                                        stations = 2;
                                        break;
                                    case 3:
                                        stations = 3;
                                        break;
                                    case 4:
                                        {
                                            stations = 0;
                                            for (int k = 0; k < obj_station.Length; k++)
                                            {
                                                if (k != 0)
                                                {
                                                    if (obj_station[k] == obj_station[0])
                                                        break;
                                                    else
                                                        stations++;
                                                }
                                                else
                                                    stations++;
                                            }

                                            break;
                                        }
                                    default:
                                        return;
                                }

                                for (int s = 0; s < stations; s++)
                                {
                                    double stationsLenght = -1;

                                    switch (s)
                                    {
                                        case 0 when stat == 0:
                                            stationsLenght = obj_station[0];
                                            break;
                                        case 0 when stat == 1:
                                            stationsLenght = length;
                                            break;
                                        case 0 when stat == 2:
                                            stationsLenght = obj_station[0];
                                            break;
                                        case 1 when stat == 2:
                                            stationsLenght = length;
                                            break;
                                        default:
                                            if (stat == 3)
                                            {
                                                if (s == 1)
                                                    stationsLenght = obj_station[0];
                                                else if (s == 1)
                                                    stationsLenght = length / 2.0;
                                                else if (s == 2)
                                                    stationsLenght = length;
                                            }
                                            else if (stat == 4)
                                                stationsLenght = obj_station[s];
                                            break;
                                    }

                                    if (stationsLenght == -1)
                                        return;

                                    List<FrameForcesResultsStructSigma> buffer = resultsStructSigmasBuffer.Where(k => Math.Abs(k.Station - stationsLenght) < 0.01).ToList();

                                    resultsStructSigmas.Add(buffer.Where(bb => bb.N == buffer.Select(aa => aa.N).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.N == buffer.Select(aa => aa.N).Min()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.V2 == buffer.Select(aa => aa.V2).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.V2 == buffer.Select(aa => aa.V2).Min()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.V3 == buffer.Select(aa => aa.V3).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.V3 == buffer.Select(aa => aa.V3).Min()).FirstOrDefault());

                                    resultsStructSigmas.Add(buffer.Where(bb => bb.M2 == buffer.Select(aa => aa.M2).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.M2 == buffer.Select(aa => aa.M2).Min()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.M3 == buffer.Select(aa => aa.M3).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.M3 == buffer.Select(aa => aa.M3).Min()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.T == buffer.Select(aa => aa.T).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.T == buffer.Select(aa => aa.T).Min()).FirstOrDefault());

                                    resultsStructSigmas.Add(buffer.Where(bb => bb.SigmaMax == buffer.Select(aa => aa.SigmaMax).Max()).FirstOrDefault());
                                    resultsStructSigmas.Add(buffer.Where(bb => bb.SigmaMin == buffer.Select(aa => aa.SigmaMin).Min()).FirstOrDefault());

                                    #endregion

                                }
                            }
                        }
                    }

                    HashSet<string> outCombos = new HashSet<string>();

                    for (int i = 0; i < resultsStructSigmas.Count; i++)
                        outCombos.Add(resultsStructSigmas[i].LoadCase);

                    DA.SetDataList(0, outCombos.ToList());
                    Message = "Done";
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                    _run = false;
                    return;
                }
            }
        }

        public struct FrameForcesResultsStructSigma
        {
            public string SectionName;
            public string NameBeam;
            public string LoadCase;
            public double Station;
            public double N;
            public double V2;
            public double V3;
            public double M2;
            public double M3;
            public double T;
            public double SigmaMax;
            public double SigmaMin;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        //protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPBeamResultFilterIcon;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("dfb7ddb5-663e-45ad-8107-150d977c8ff1");
    }
}