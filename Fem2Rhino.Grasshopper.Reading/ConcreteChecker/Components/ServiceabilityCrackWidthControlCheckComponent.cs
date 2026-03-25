using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ServiceabilityCrackWidthControlCheckComponent : GH_Component
    {
        protected Dictionary<int, string> _serviceabilityLimitState = new Dictionary<int, string>() {
            { 0, "Quasi-Permanent" },
            { 1, "Frequent" },
            { 2, "Characteristic" },
        };

        protected Dictionary<int, string> _classeEsposizione = new Dictionary<int, string>() {
            { 0, "X0" },
            { 1, "XC1" },
            { 2, "XC2" },
            { 3, "XC3" },
            { 4, "XF1" },
            { 5, "XC4" },
            { 6, "XD1" },
            { 7, "XS1" },
            { 8, "XA1" },
            { 9, "XA2" },
            { 10, "XF2" },
            { 11, "XF3" },
            { 12, "XD2" },
            { 13, "XD3" },
            { 14, "XS2" },
            { 15, "XS3" },
            { 16, "XA3" },
            { 17, "XF4" },
        };

        protected Dictionary<int, string> _tipoArmatura = new Dictionary<int, string>() {
            { 0, "Sensibile" },
            { 1, "Poco Sensibile" },
        };

        protected Dictionary<int, string> _condizioniAmbientali = new Dictionary<int, string>() {
            { 0, "Ordinarie" },
            { 1, "Aggressive" },
            { 2, "Molto Aggressive" },
        };

        protected Dictionary<int, (string name, double coefficient)> _limitiApertura = new Dictionary<int, (string, double)>() {
            { 0, ("w1", 0.2) },
            { 1, ("w2", 0.3) },
            { 2, ("w3", 0.4) },
            { 3, ("decompressione", 0.0) },
        };

        protected Dictionary<int, (string name, double coefficient)> _tipoDiSollecitazione = new Dictionary<int, (string, double)>() {
            { 0, ("Flessione", 0.5) },
            { 1, ("Trazione", 1.0) },
        };

        protected Dictionary<int, (string name, double coefficient)> _tipoDiCarico = new Dictionary<int, (string, double)>() {
            { 0, ("Breve durata", 0.4) },
            { 1, ("Lunga durata", 0.6) },
        };

        protected Dictionary<int, (string name, double coefficient)> _tipoBarra = new Dictionary<int, (string, double)>() {
            { 0, ("Aderenza migliorata", 0.8) },
            { 1, ("Lisce", 1.6) },
        };

        public ServiceabilityCrackWidthControlCheckComponent()
            : base("Serviceability Crack Width Check", "SCWC", "Serviceability Crack Width Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
            pManager.AddNumberParameter("Ricoprimento dell'armatura", "C", "Ricoprimento dell'armatura", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Serviceability Limit State", "SLS", "Serviceability Limit State", GH_ParamAccess.item);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _serviceabilityLimitState)
                mtParam.AddNamedValue(v.Value, v.Key);
            i = pManager.AddIntegerParameter("Classe di Esposizione", "CE", "Classe di Esposizione", GH_ParamAccess.item);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _classeEsposizione)
                mtParam.AddNamedValue(v.Value, v.Key);
            i = pManager.AddIntegerParameter("Tipo di Armatura", "TA", "Tipo di Armatura", GH_ParamAccess.item, 1);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _tipoArmatura)
                mtParam.AddNamedValue(v.Value, v.Key);
            i = pManager.AddIntegerParameter("Tipo di barra", "TB", "Tipo di Barra", GH_ParamAccess.item, 0);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, (string, double)> v in _tipoBarra)
                mtParam.AddNamedValue(v.Value.Item1, v.Key);
            i = pManager.AddIntegerParameter("Tipo di Carico", "TA", "Tipo di Carico", GH_ParamAccess.item, 1);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, (string, double)> v in _tipoDiCarico)
                mtParam.AddNamedValue(v.Value.Item1, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Crack width", "CW", "Crack width", GH_ParamAccess.item);
            pManager.AddNumberParameter("Crack width limit", "CWL", "Crack width limit", GH_ParamAccess.item);
            pManager.AddTextParameter("Crack width limit", "CWL", "Crack width limit", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio", "WR", "Working Ratio", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult gH_StressAnalysisResult = null;
            double ricoprimentoBarra = 0;
            int limitState = 0;
            int classeEsposizione = 0;
            int tipoArmatura = 0;
            int tipoBarra = 0;
            int tipoCarico = 0;

            int inputCount = 0;
            if (DA.GetData(inputCount++, ref gH_StressAnalysisResult) && DA.GetData(inputCount++, ref ricoprimentoBarra) && DA.GetData(inputCount++, ref limitState)
                && DA.GetData(inputCount++, ref classeEsposizione) && DA.GetData(inputCount++, ref tipoArmatura) && DA.GetData(inputCount++, ref tipoBarra)
                && DA.GetData(inputCount++, ref tipoCarico))
            {
                double wLim = 0;
                string wLimName = string.Empty;
                string condizioniAmbientali = string.Empty;
                double w = 0;
                double wr = 0;
                string toExcel = string.Empty;
                string toExcelHeader = "WR [-];h[mm];b[mm];d[mm];d' [mm];c'[mm];nf.1[-];ff.1[mm];Asf.1[mm2];nf.2[-];ff.2[mm];Asf.2[mm2];ff.3[mm];" +
                    "Classe di calcestruzzo di progetto;fck[MPa];Rck[MPa];fctm[MPa];Ecm[MPa];fyk[MPa];Es[MPa];Tipo di armatura;Classe di esposizione;" +
                    "Condizioni ambientali;Tipo di barra;Tipo di sollecitazione;Tipo di combinazione;σs[MPa];x[mm];Tipo e durata dei carichi applicati;" +
                    "ae[-];As[mm2];Ac,eff.1[mm2];Ac,eff.2[mm2];Ac,eff.3[mm2];Ac,eff.min[mm2];rp,eff[-];fct,eff[MPa];kt[-];[esm-ecm]min[-];[esm-ecm]calc. [-];" +
                    "[esm-ecm] [-];s[mm];feq[mm];smax,rif[mm];k1[-];k2[-];k3[-];k4[-];sr,max.1[mm];sr,max.2[mm];sr,max[mm];wk.lim[mm];wk[mm];wk / wk.lim[-]";


                if (limitState == 2)
                {
                    toExcel = $"1;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0";
                }
                else
                {
                    if (classeEsposizione >= 0 && classeEsposizione <= 4) // ORDINARIE
                    {
                        condizioniAmbientali = _condizioniAmbientali[0];

                        if (tipoArmatura == 0) // SENSIBILE
                        {
                            if (limitState == 0) // QUASI PERMANENTE
                            {
                                wLimName = _limitiApertura[0].name;
                                wLim = _limitiApertura[0].coefficient;
                            }
                            else if (limitState == 1) // FREQUENTE
                            {
                                wLimName = _limitiApertura[1].name;
                                wLim = _limitiApertura[1].coefficient;
                            }
                        }
                        else // POCO SENSIBILE
                        {
                            if (limitState == 0) // QUASI PERMANENTE
                            {
                                wLimName = _limitiApertura[1].name;
                                wLim = _limitiApertura[1].coefficient;
                            }
                            else if (limitState == 1) // FREQUENTE
                            {
                                wLimName = _limitiApertura[2].name;
                                wLim = _limitiApertura[2].coefficient;
                            }
                        }
                    }
                    else if (classeEsposizione >= 5 && classeEsposizione <= 11) // AGGRESSIVE
                    {
                        condizioniAmbientali = _condizioniAmbientali[1];

                        if (tipoArmatura == 0) // SENSIBILE
                        {
                            if (limitState == 0) // QUASI PERMANENTE
                            {
                                wLimName = _limitiApertura[3].name;
                                wLim = _limitiApertura[3].coefficient;
                            }
                            else if (limitState == 1) // FREQUENTE
                            {
                                wLimName = _limitiApertura[0].name;
                                wLim = _limitiApertura[0].coefficient;
                            }
                        }
                        else // POCO SENSIBILE
                        {
                            if (limitState == 0) // QUASI PERMANENTE
                            {
                                wLimName = _limitiApertura[0].name;
                                wLim = _limitiApertura[0].coefficient;
                            }
                            else if (limitState == 1) // FREQUENTE
                            {
                                wLimName = _limitiApertura[1].name;
                                wLim = _limitiApertura[1].coefficient;
                            }
                        }
                    }
                    else if (classeEsposizione >= 12 && classeEsposizione <= 17) // MOLTO AGGRESSIVE
                    {
                        condizioniAmbientali = _condizioniAmbientali[2];

                        if (tipoArmatura == 0) // SENSIBILE
                        {
                            wLimName = _limitiApertura[3].name;
                            wLim = _limitiApertura[3].coefficient;
                        }
                        else // POCO SENSIBILE
                        {
                            wLimName = _limitiApertura[0].name;
                            wLim = _limitiApertura[0].coefficient;
                        }
                    }

                    GPC.Checkers.Concrete.Results.StressAnalysisResult analysisresult = gH_StressAnalysisResult.Value;
                    IConcreteSection concreteSection = analysisresult.ConcreteSection;
                    ConcreteMaterialEuropeanCommon concreteMaterial = ((ConcreteMaterialEuropeanCommon)concreteSection.ConcreteMaterial);
                    GPC.Checker.Results.ResultType.StrainPlaneResult strainPlaneResult = analysisresult.CalculateStrainPlaneResult(analysisresult.LinearElasticAnalysis, analysisresult.PsiRebar.Value, analysisresult.PsiTendon.Value);
                    (ReinforcedConcreteRebar rebar, double strain)[] rebarstrains = analysisresult.LinearElasticAnalysis ? analysisresult.GetRebarsStrain(analysisresult.PsiRebar.Value) : analysisresult.GetRebarsStrain();

                    double k1 = _tipoBarra[tipoBarra].coefficient;
                    double k2 = rebarstrains.Any(k => k.strain < 0) ? _tipoDiSollecitazione[0].coefficient : _tipoDiSollecitazione[1].coefficient;
                    string tipoSollecitazione = rebarstrains.Any(k => k.strain < 0) ? _tipoDiSollecitazione[0].name : _tipoDiSollecitazione[1].name;
                    double k3 = 3.4;
                    double k4 = 0.425;

                    double phiEqNumeratore = 0;
                    double phiEqDenominatore = 0;
                    double areaRebarsTens = 0;
                    double numbRebarsTens = 0;
                    double diamRebarsTens = 0;
                    for (int i = 0; i < analysisresult.ConcreteSection.RebarsCount; i++)
                    {
                        phiEqNumeratore += concreteSection.Rebars.ElementAt(i).RebarSection.Diameter * concreteSection.Rebars.ElementAt(i).RebarSection.Diameter;
                        phiEqDenominatore += concreteSection.Rebars.ElementAt(i).RebarSection.Diameter;
                    }

                    double phiEq = phiEqNumeratore / phiEqDenominatore;
                    double sigmaS = strainPlaneResult.SigmaSMax;

                    double d = strainPlaneResult.NetHeight;
                    double x = strainPlaneResult.NeutralAxisDistance;
                    double h = GetConcreteHeight(strainPlaneResult, analysisresult);
                    double lim1 = 2.5 * (h - d);
                    double lim2 = (h - x) / 3.0;
                    double lim3 = h / 2.0;

                    double hLim = Math.Min(lim1, Math.Min(lim2, lim3));
                    double distanceFromNeutralAxis = h - hLim - x;
                    double deltax = distanceFromNeutralAxis * Math.Sin(strainPlaneResult.StrainPlane.Teta);
                    double deltay = -distanceFromNeutralAxis * Math.Cos(strainPlaneResult.StrainPlane.Teta);

                    GPC.Geometry.Line2d neutralAxis = new GPC.Geometry.Line2d(analysisresult.StrainPlane.GetNeutralAxisRespectReferencePoint());
                    neutralAxis.Move(analysisresult.StrainPlane.ReferencePoint.X, analysisresult.StrainPlane.ReferencePoint.Y);
                    Rhino.Geometry.Line neutralAxisRhino = new Rhino.Geometry.Line(new Rhino.Geometry.Point3d(neutralAxis.Start.X, neutralAxis.Start.Y, 0), new Rhino.Geometry.Point3d(neutralAxis.End.X, neutralAxis.End.Y, 0));

                    GPC.Geometry.Line2d cutLine = new GPC.Geometry.Line2d(neutralAxis);
                    cutLine.Move(deltax, deltay);
                    Rhino.Geometry.LineCurve lineCurve = new Rhino.Geometry.LineCurve(new Rhino.Geometry.Point2d(cutLine.Start.X, cutLine.Start.Y), new Rhino.Geometry.Point2d(cutLine.End.X, cutLine.End.Y));
                    Rhino.Geometry.Curve cutLineRhino = lineCurve.Extend(Rhino.Geometry.CurveEnd.Both, 10000, Rhino.Geometry.CurveExtensionStyle.Line);
                    cutLine = new GPC.Geometry.Line2d(new GPC.Geometry.Point2d(cutLineRhino.PointAtStart.X, cutLineRhino.PointAtStart.Y), new GPC.Geometry.Point2d(cutLineRhino.PointAtEnd.X, cutLineRhino.PointAtEnd.Y));

                    Mesh meshClone = (Mesh)concreteSection.Mesh.Clone();
                    meshClone.Cut(cutLine);

                    double areaCBuffer = 0;
                    var rebars = analysisresult.ConcreteSection.GetRebars();
                    for (int i = 0; i < meshClone.Faces.Count; i++)
                    {
                        var centroid = meshClone.GetFaceCentroid(meshClone.Faces.ElementAt(i));
                        if (strainPlaneResult.StrainPlane.GetStrain(centroid) > 0)
                        {
                            if (neutralAxisRhino.DistanceTo(new Rhino.Geometry.Point3d(centroid.X, centroid.Y, centroid.Z), false) > distanceFromNeutralAxis)
                            {
                                areaCBuffer += meshClone.FaceArea(meshClone.Faces.ElementAt(i));
                            }
                        }
                    }
                    for (int i = 0; i < rebars.Length; i++)
                    {
                        if (strainPlaneResult.StrainPlane.GetStrain(rebars[i].Position) > 0)
                        {
                            if (neutralAxisRhino.DistanceTo(new Rhino.Geometry.Point3d(rebars[i].Position.X, rebars[i].Position.Y, 0), false) > distanceFromNeutralAxis)
                            {
                                areaRebarsTens += rebars[i].Area;
                                numbRebarsTens++;
                                diamRebarsTens += rebars[i].RebarSection.Diameter;
                            }
                        }
                    }
                    if (numbRebarsTens == 0)
                    {
                        numbRebarsTens = 1;
                        diamRebarsTens = phiEq;
                    }
                    diamRebarsTens /= numbRebarsTens;

                    double As = areaRebarsTens;
                    double Aceff = areaCBuffer;
                    double rhoEff = As / Aceff;
                    ReinforcedConcreteRebar rebar = concreteSection.Rebars.Where(i => i.EpsilonP == 0).First();
                    double Es = rebar.RebarMaterial.ElasticModulusTension;
                    double fyk = rebar.RebarMaterial.Fyk;
                    double kt = _tipoDiCarico[tipoCarico].coefficient;
                    double alphaE = Es / concreteMaterial.Ecm;
                    double sMaxRif = 5 * (ricoprimentoBarra + diamRebarsTens / 2.0);
                    double deltaSM1 = (k3 * ricoprimentoBarra + k1 * k2 * k4 * phiEq / rhoEff) / 1.7;
                    double deltaSM2 = 0.75 * (h - x);
                    double deltaSM = Math.Min(deltaSM1, deltaSM2);
                    double epsiolnSM1 = (sigmaS - kt * concreteMaterial.Fctm / rhoEff * (1 + alphaE * rhoEff)) / Es;
                    double epsiolnSM2 = 0.6 * sigmaS / Es;
                    double epsiolnSM = Math.Max(epsiolnSM1, epsiolnSM2);

                    w = Math.Max(1.70 * deltaSM * epsiolnSM, 0);
                    if (strainPlaneResult.EpsilonCMax < 0)
                        w = 0;

                    // for (int i = 0; i < meshClone.VerticesCount; i++)
                    //     outlist.Add(new Rhino.Geometry.Point2d(meshClone.Vertices.EslementAt(i).Point.X, meshClone.Vertices.ElementAt(i).Point.Y));

                    wr = Math.Round(w / wLim, 3);

                    toExcel = $"1;{h};-;{d};{d};{ricoprimentoBarra};{concreteSection.RebarsCount};" +
                        $"{concreteSection.Rebars.Select(i => i.RebarSection.Diameter).Average()};{concreteSection.AreaRebars};" +
                        $"{numbRebarsTens};{diamRebarsTens};{areaRebarsTens};-;" +
                        $"{concreteMaterial.Name};{concreteMaterial.Fck};{concreteMaterial.Rck};{concreteMaterial.Fctm};{concreteMaterial.Ecm};{fyk};{Es};" +
                        $"{_tipoArmatura[tipoArmatura]};{_classeEsposizione[classeEsposizione]};{condizioniAmbientali};{_tipoBarra[tipoBarra]};{tipoSollecitazione};{_serviceabilityLimitState[limitState]};" +
                        $"{sigmaS};{x};{_tipoDiCarico[tipoCarico].name};{alphaE};{As};{lim1};{lim2};{lim3};{areaCBuffer};" +
                        $"{rhoEff};{concreteMaterial.Fctm};{kt};{epsiolnSM1};{epsiolnSM2};{epsiolnSM};" +
                        $"{ricoprimentoBarra};{diamRebarsTens};{sMaxRif};{k1};{k2};{k3};{k4};{deltaSM1};{deltaSM2};{deltaSM};" +
                        $"{wLim};{w};{wr}";
                }

                int count = 0;
                DA.SetData(count++, toExcelHeader);
                DA.SetData(count++, toExcel);
                DA.SetData(count++, Math.Round(w, 3));
                DA.SetData(count++, wLim);
                DA.SetData(count++, wLimName);
                DA.SetData(count++, Math.Round(wr, 3));
            }
        }

        private double GetConcreteHeight(GPC.Checker.Results.ResultType.StrainPlaneResult strainPlaneResult, GPC.Checkers.Concrete.Results.StressAnalysisResult analysisresult)
        {
            double cosTeta = Math.Cos(strainPlaneResult.StrainPlane.Teta);
            double sinTeta = Math.Sin(strainPlaneResult.StrainPlane.Teta);

            double dmaxConcrete = double.MinValue;
            double dminConcrete = double.MaxValue;

            for (int c = 0; c < analysisresult.ConcreteSection.Shape.Fill.Count; c++)
            {
                double w1 = (analysisresult.ConcreteSection.Shape.Fill[c].Y - analysisresult.SectionSolver.IntegrationReferencePoint.Y) * cosTeta -
                    (analysisresult.ConcreteSection.Shape.Fill[c].X - analysisresult.SectionSolver.IntegrationReferencePoint.X) * sinTeta;

                if (w1 >= dmaxConcrete)
                    dmaxConcrete = w1;

                if (w1 <= dminConcrete)
                    dminConcrete = w1;
            }

            return Math.Abs(dmaxConcrete) + Math.Abs(dminConcrete);
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("22bb7ea8-8721-42bf-b016-67c4379f9ca3");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
