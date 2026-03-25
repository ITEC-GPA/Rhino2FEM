using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Helper;
using System;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class RectangularSectionShearCheckNoStirrupComponent : GH_Component
    {
        public RectangularSectionShearCheckNoStirrupComponent()
            : base("Rectangular Section Shear Check No stirrup", "SC", "Shear Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force", "F", "Force", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Base", "B", "Base", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter", "LRD", "Longitudinal rebar diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number of Rebar", "NR", "Number of Rebar", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Cover", "CC", "Concrete Cover", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Area", "CA", "Concrete area", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrds", "Vrdc", "Vrdc", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrdc", "Vrds", "Vrds", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd", "Vrd", "Vrd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio", "WR", "Working Ratio", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_SectionChecker gH_SectionChecker = null;
            GH_ResultBeamForces gH_ResultBeamForces = null;
            double height = 0;
            double baseSection = 0;
            double longitudinalRebarDiameter = 0;
            double numberOfRebar = 0;
            double concreteCover = 0;
            double concreteArea = 0;

            int count = 0;
            if (DA.GetData(count++, ref gH_SectionChecker) && DA.GetData(count++, ref gH_ResultBeamForces) && DA.GetData(count++, ref height)
                && DA.GetData(count++, ref baseSection) && DA.GetData(count++, ref longitudinalRebarDiameter) && DA.GetData(count++, ref numberOfRebar)
                && DA.GetData(count++, ref concreteCover) && DA.GetData(count++, ref concreteArea))
            {

                GPC.Model.Standards.StandardModelCode2010 standard = gH_SectionChecker.Value.StandardModelCode2010;
                ConcreteMaterialEuropeanCommon concreteMaterial = (ConcreteMaterialEuropeanCommon)gH_SectionChecker.Value.SectionCheckerAttribute.Section.ConcreteMaterial;
                var fcd = Math.Abs(concreteMaterial.CalculateFcd(standard));
                var rb = gH_SectionChecker.Value.SectionCheckerAttribute.Section.Rebars.Where(i => i.EpsilonP == 0).First();
                var fyd = rb.RebarMaterial.CalculateFyd(standard);
                var fyk = rb.RebarMaterial.Fyk;
                double hUtile = height - concreteCover - longitudinalRebarDiameter / 2.0;
                double areaLongitudinalRebar = Math.PI * Math.Pow(longitudinalRebarDiameter, 2) / 4.0;
                double areaLongitudinalRebars = areaLongitudinalRebar * numberOfRebar;
                double rhol = Math.Min(0.02, areaLongitudinalRebars / (baseSection * hUtile));

                double sigmaCP = Math.Abs(gH_ResultBeamForces.Value.N / (baseSection * height));
                double alphaC = 1.0;
                if (gH_ResultBeamForces.Value.N < 0.0)
                {
                    if (sigmaCP > 0 && sigmaCP <= 0.25 * fcd)
                        alphaC = 1.0 + sigmaCP / fcd;
                    else if (sigmaCP >= 0.25 * fcd && sigmaCP <= 0.5 * fcd)
                        alphaC = 1.25;
                    else if (sigmaCP >= 0.5 * fcd && sigmaCP <= 1.0 * fcd)
                        alphaC = 2.5 * (1.0 - sigmaCP / fcd);
                    else if (sigmaCP > fcd)
                        alphaC = 0;
                }

                double k = Math.Min(1 + Math.Sqrt(200.0 / hUtile), 2.0);
                double vmin = 0.035 * Math.Pow(k, 3.0 / 2.0) * Math.Pow(Math.Abs(concreteMaterial.Fck), 0.5);
                double vrds = (0.18 * k * Math.Pow(100 * rhol * Math.Abs( concreteMaterial.Fck), 1.0 / 3.0)/standard.GammaC + 0.15 * sigmaCP) * baseSection * hUtile;
                double vrdc = (vmin +0.15*sigmaCP) * baseSection * hUtile;
                
                double vrd = Math.Max(vrds, vrdc);
                double wr = Math.Abs(gH_ResultBeamForces.Value.V2) / vrd;

                string toExcelHeader = "ss_Classe di calcestruzzo di progetto;ss_Rck [MPa];ss_fck [MPa];ss_fcd [MPa];ss_fyk [MPa];ss_fyd [MPa];ss_ϕlong [mm];ρl [-]" +
                    "ss_n°;ss_Asw [mm2];ss_bw [mm];ss_h [mm];ss_c [mm];ss_d [mm];ss_Ac [mm2];ss_σcp [MPa];" +
                    "ss_αc [°];ss_VRsd [kN];ss_Vrcd [kN];ss_VRd [kN];ss_WRv;ss_WRvLim";
                string toExcel = $"{concreteMaterial.Name};{concreteMaterial.Rck};{concreteMaterial.Fck};{fcd};{fyk};{fyd};{longitudinalRebarDiameter};{rhol}" +
                    $"{numberOfRebar};{areaLongitudinalRebars};{baseSection};{height};{concreteCover};{hUtile};{concreteArea};{sigmaCP};" +
                    $"{alphaC};{vrds / 1000.0};{vrdc / 1000.0};{vrd / 1000.0};{wr};{1}";

                int cc = 0;
                DA.SetData(cc++, toExcelHeader);
                DA.SetData(cc++, toExcel);
                DA.SetData(cc++, vrds / 1000.0);
                DA.SetData(cc++, vrdc / 1000.0);
                DA.SetData(cc++, vrd / 1000.0);
                DA.SetData(cc++, Math.Round(wr, 3));
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Stirrup diameter must be greater than 0.");
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("08625aa4-a62b-4d06-91fb-9c9ffcf076fc");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
