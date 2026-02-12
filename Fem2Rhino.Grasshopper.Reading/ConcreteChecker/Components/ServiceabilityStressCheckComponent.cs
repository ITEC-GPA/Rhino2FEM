using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Fem.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class ServiceabilityStressCheckComponent : GH_Component
    {
        protected Dictionary<int, string> _serviceabilityLimitState = new Dictionary<int, string>() {
            { 0, "Quasi-Permanent" },
            { 1, "Frequent" },
            { 2, "Characteristic" },
        };

        public ServiceabilityStressCheckComponent()
            : base("Serviceability Stress Check", "SSC", "Serviceability Stress Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Stress Analysis Results", "SA", "Stress Analysis Results", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Serviceability Limit State", "SLS", "Serviceability Limit State", GH_ParamAccess.item);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _serviceabilityLimitState)
                mtParam.AddNamedValue(v.Value, v.Key);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.list);
            pManager.AddPointParameter("Concrete Points", "CP", "Concrete Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Concrete Values", "CV", "Concrete Values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Concrete Working Ratio", "CWR", "Concrete Working Ratio", GH_ParamAccess.list);
            pManager.AddPointParameter("Rebar Points", "RP", "Rebar Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rebar Values", "RV", "Rebar Values", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rebar Working Ratio", "RWR", "Rebar Working Ratio", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_StressAnalysisResult gH_StressAnalysisResult = null;
            int type = 0;

            if (DA.GetData(0, ref gH_StressAnalysisResult) && DA.GetData(1, ref type))
            {
                GPC.Checkers.Concrete.Results.StressAnalysisResult analysisresult = gH_StressAnalysisResult.Value;

                List<string> toExcel = new List<string>();
                List<Point2d> concretePoints = new List<Point2d>();
                List<double> concreteValues = new List<double>();
                List<double> concreteWR = new List<double>();
                List<Point2d> rebarPoints = new List<Point2d>();
                List<double> rebarValues = new List<double>();
                List<double> rebarWR = new List<double>();
                List<Point2d> presPoints = new List<Point2d>();
                List<double> presValues = new List<double>();
                List<double> presWR = new List<double>();

                int cifresignificativeStress = 2;
                int cifresignificativeWr = 2;

                if (type == 0) // QUASI-PERMANENT  
                {
                    (GPC.Geometry.Point2d point, double tension, double workingRatio)[] concreteResult = null;
                    double limit = ((ConcreteMaterialEuropeanCommon)analysisresult.ConcreteSection.ConcreteMaterial).GetConcreteServiceabilityQuasiPermanentStress((StandardModelCode2010)analysisresult.Standard);
                    if (analysisresult.LinearElasticAnalysis)                    
                        concreteResult = analysisresult.ConcreteServiceabilityQuasiPermanentCheck(analysisresult.PsiRebar.Value);
                    else
                        concreteResult = analysisresult.ConcreteServiceabilityQuasiPermanentCheck();


                    for (int i = 0; i < concreteResult.Length; i++)
                    {
                        concretePoints.Add(new Point2d(concreteResult[i].point.X, concreteResult[i].point.Y));
                        concreteValues.Add(Math.Round(concreteResult[i].tension, cifresignificativeStress));
                        concreteWR.Add(Math.Round(concreteResult[i].workingRatio, cifresignificativeWr));
                    }
                    for (int i = 0; i < analysisresult.ConcreteSection.Rebars.Count(); i++)
                    {
                        rebarPoints.Add(new Point2d(analysisresult.ConcreteSection.Rebars.ElementAt(i).Position.X, analysisresult.ConcreteSection.Rebars.ElementAt(i).Position.Y));
                        rebarValues.Add(Math.Round(0.0, cifresignificativeStress));
                        rebarWR.Add(Math.Round(0.0, cifresignificativeStress));
                    }
                    toExcel.Add($"{Math.Abs(Math.Round(concreteValues.Min(), cifresignificativeStress))};{Math.Abs(limit)};{Math.Round(concreteWR.Max(), cifresignificativeWr)};0;0;0;0;0;0;0;0;0");
                }
                else if(type == 1) // FREQUENT
                {
                    var pointsSection = analysisresult.ConcreteSection.SectionShape.Shape.GetPoints();
                    for (int i = 0; i < pointsSection.Length; i++)
                    {
                        concretePoints.Add(new Point2d(pointsSection[i].X, pointsSection[i].Y));
                        concreteValues.Add(Math.Round(0.0, cifresignificativeStress));
                        concreteWR.Add(Math.Round(0.0, cifresignificativeWr));
                    }
                    for (int i = 0; i < analysisresult.ConcreteSection.Rebars.Count(); i++)
                    {
                        rebarPoints.Add(new Point2d(analysisresult.ConcreteSection.Rebars.ElementAt(i).Position.X, analysisresult.ConcreteSection.Rebars.ElementAt(i).Position.Y));
                        rebarValues.Add(Math.Round(0.0, cifresignificativeStress));
                        rebarWR.Add(Math.Round(0.0, cifresignificativeStress));
                    }
                    toExcel.Add($"0;0;0;0;0;0;0;0;0;0;0;0");
                }
                else if (type == 2) // CHARACTERISTIC
                {
                    double limitconcrete = ((ConcreteMaterialEuropeanCommon)analysisresult.ConcreteSection.ConcreteMaterial).GetConcreteServiceabilityCharacteristicStress((StandardModelCode2010)analysisresult.Standard);
                    double limitSteel = ((analysisresult.ConcreteSection.Rebars.Where(k =>k.EpsilonP == 0).FirstOrDefault().RebarMaterial).GetServiceabilityCharacteristicStress((StandardModelCode2010)analysisresult.Standard));
                    double limitSteelPress = 0;
                    if((analysisresult.ConcreteSection.Rebars.Any(k =>k.EpsilonP != 0)))
                        limitSteelPress = analysisresult.ConcreteSection.Rebars.Where(k => k.EpsilonP != 0).FirstOrDefault().RebarMaterial.GetServiceabilityCharacteristicStressPrestress((StandardModelCode2010)analysisresult.Standard);

                    (GPC.Geometry.Point2d point, double tension, double workingRatio)[] concreteResult = null;
                    if (analysisresult.LinearElasticAnalysis)
                        concreteResult = analysisresult.ConcreteServiceabilityCharacteristicCheck(analysisresult.PsiRebar.Value);
                    else
                        concreteResult = analysisresult.ConcreteServiceabilityCharacteristicCheck();

                    (ReinforcedConcreteRebar rebar, double tension, double workingRatio)[] steelResult = null;
                    if (analysisresult.LinearElasticAnalysis)
                        steelResult = analysisresult.SteelServiceabilityCharacteristicCheck(analysisresult.PsiRebar.Value, analysisresult.PsiTendon.Value);
                    else
                        steelResult = analysisresult.SteelServiceabilityCharacteristicCheck();

                    for (int i = 0; i < concreteResult.Length; i++)
                    {
                        concretePoints.Add(new Point2d(concreteResult[i].point.X, concreteResult[i].point.Y));
                        concreteValues.Add(Math.Round(concreteResult[i].tension, cifresignificativeStress));
                        concreteWR.Add(Math.Round(concreteResult[i].workingRatio, cifresignificativeWr));
                    }
                    for (int i = 0; i < steelResult.Length; i++)
                    {
                        if (steelResult[i].rebar.EpsilonP == 0)
                        {
                            rebarPoints.Add(new Point2d(steelResult[i].rebar.Position.X, steelResult[i].rebar.Position.Y));
                            rebarValues.Add(Math.Round(steelResult[i].tension, cifresignificativeStress));
                            rebarWR.Add(Math.Round(steelResult[i].workingRatio, cifresignificativeStress));
                            presPoints.Add(Point2d.Unset);
                            presValues.Add(0.0);
                            presWR.Add(0.0);
                        }
                        else
                        {
                            rebarPoints.Add(Point2d.Unset);
                            rebarValues.Add(0.0);
                            rebarWR.Add(0.0);
                            presPoints.Add(new Point2d(steelResult[i].rebar.Position.X, steelResult[i].rebar.Position.Y));
                            presValues.Add(Math.Round(steelResult[i].tension, cifresignificativeStress));
                            presWR.Add(Math.Round(steelResult[i].workingRatio, cifresignificativeStress));
                        }
                    }
                    toExcel.Add($"0;0;0;{Math.Abs(concreteValues.Min())};{Math.Abs(limitconcrete)};{concreteWR.Max()};{Math.Abs(rebarValues.Max())};{limitSteel};{rebarWR.Max()};0;0;0");
                }

                var count = 0;
                DA.SetDataList(count++, toExcel);
                DA.SetDataList(count++, concretePoints);
                DA.SetDataList(count++, concreteValues);
                DA.SetDataList(count++, concreteWR);
                DA.SetDataList(count++, rebarPoints);
                DA.SetDataList(count++, rebarValues);
                DA.SetDataList(count++, rebarWR);
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("4cdbe859-b9a3-42d1-87d4-42c24647fc68");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
