using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Helper;
using System;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class RectangularSectionShearCheckComponent : GH_Component
    {
        public RectangularSectionShearCheckComponent()
            : base("Rectangular Section Shear Check", "SC", "Shear Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force", "F", "Force", GH_ParamAccess.item);
            pManager.AddNumberParameter("Height", "H", "Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Base", "B", "Base", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup Diameter", "SD", "Stirrup Diameter", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Legs X", "NLX", "Number of Legs of a stirrup in X direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Legs Y", "NLY", "Number of Legs of a stirrup in Y direction", GH_ParamAccess.item);
            pManager.AddNumberParameter("Spacing", "S", "Spacing", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup slope X", "SS X", "Stirrup slope X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup slope Y", "SS Y", "Stirrup slope Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter X", "LRD X", "Longitudinal rebar diameter X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter Y", "LRD Y", "Longitudinal rebar diameter Y", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of longit rebarsX", "Number of longit rebarsX", "Number of longit rebarsX", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of longit rebarsY", "Number of longit rebarsY", "Number of longit rebarsY", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Cover", "CC", "Concrete Cover", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Area", "CA", "Concrete area", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd X no stirrup", "Vrsd X no stirrup", "Vrsd X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd Y no stirrup", "Vrsd Y no stirrup", "Vrsd Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd X no stirrup", "Vrcd X no stirrup", "Vrcd X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd Y no stirrup", "Vrcd Y no stirrup", "Vrcd Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd X no stirrup", "Vrd X no stirrup", "Vrd X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd Y no stirrup", "Vrd Y no stirrup", "Vrd Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd X", "Vrsd X", "Vrsd X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd Y", "Vrsd Y", "Vrsd Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd X", "Vrcd X", "Vrcd X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd Y", "Vrcd Y", "Vrcd Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd X", "Vrd X", "Vrd X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd Y", "Vrd Y", "Vrd Y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio X no stirrup", "WR X NS", "Working Ratio X no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio Y no stirrup", "WR Y NS", "Working Ratio Y no stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio X", "WR X", "Working Ratio X", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio Y", "WR Y", "Working Ratio Y", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_SectionChecker gH_SectionChecker = null;
            GH_ResultBeamForces gH_ResultBeamForces = null;
            double height = 0;
            double baseSection = 0;
            double stirrupDiameter = 0;
            double spacing = 0;
            int numberOfLegsY = 0;
            int numberOfLegsX = 0;
            double stirrupSlopeY = 0;
            double stirrupSlopeX = 0;
            double longitudinalRebarDiameterX = 0;
            double longitudinalRebarDiameterY = 0;
            double concreteCover = 0;
            double concreteArea = 0;
            int numberOfRebarX = 0;
            int numberOfRebarY = 0;

            int count = 0;
            if (DA.GetData(count++, ref gH_SectionChecker) && DA.GetData(count++, ref gH_ResultBeamForces) 
                && DA.GetData(count++, ref height) && DA.GetData(count++, ref baseSection) && DA.GetData(count++, ref stirrupDiameter)
                && DA.GetData(count++, ref numberOfLegsX) && DA.GetData(count++, ref numberOfLegsY) && DA.GetData(count++, ref spacing)
                && DA.GetData(count++, ref stirrupSlopeX) && DA.GetData(count++, ref stirrupSlopeY)
                && DA.GetData(count++, ref longitudinalRebarDiameterX) && DA.GetData(count++, ref longitudinalRebarDiameterY)
                && DA.GetData(count++, ref numberOfRebarX) && DA.GetData(count++, ref numberOfRebarY)
                && DA.GetData(count++, ref concreteCover) && DA.GetData(count++, ref concreteArea))
            {
                int cc = 0;
                string toExcelHeader = "ss_Classe di calcestruzzo di progetto ss_Rck[MPa];ss_fck[MPa];ss_fcd[MPa];ss_fyk[MPa];ss_fyd[MPa];" +
                    "ss_ϕlongx[mm];ss_ρlx[-];ss_Aswx[mm2];ss_b[mm];ss_hw[mm];ss_cx[mm];ss_dx[mm];ss_Acx[mm2];ss_σcpx[MPa];ss_αcx[-];ss_VRsdx[kN];ss_Vrcdx[kN];ss_VRdx[kN];ss_WRvx;ss_WRvLimx;" +
                    "ss_ϕlongy[mm];ss_ρly[-];ss_Aswy[mm2];ss_bw[mm];ss_h[mm];ss_cy[mm];ss_dy[mm];ss_Acy[mm2];ss_σcpy[MPa];ss_αcy[-];ss_VRsdy[kN];ss_Vrcdy[kN];ss_VRdy[kN];ss_WRvy;ss_WRvLimy; " +
                    "Classe di calcestruzzo di progetto;Rck [MPa];fck [MPa];fcd [MPa];fyk [MPa];fyd [MPa];ϕlong [mm];ϕst [mm];s [mm];" +
                    "n°x;αx [°];Aswx [mm2];b [mm];hw [mm];cx [mm];dx [mm];Acx [mm2];σcpx [MPa];αcx [°];νx;Cotg ϑx;ϑx [°];VRsdx [kN];Vrcdx [kN];VRdx [kN];WRvx;WRvxLim;" +
                    "n°y;αy [°];Aswy [mm2];h [mm];bw [mm];cy [mm];dy [mm];Acy [mm2];σcpy [MPa];αcy [°];νy;Cotg ϑy;ϑy [°];VRsdy [kN];Vrcdy [kN];VRdy [kN];WRvy;WRvyLim";

                GPC.Model.Standards.StandardModelCode2010 standard = gH_SectionChecker.Value.StandardModelCode2010;
                ConcreteMaterialEuropeanCommon concreteMaterial = (ConcreteMaterialEuropeanCommon)gH_SectionChecker.Value.SectionCheckerAttribute.Section.ConcreteMaterial;
                var fcd = Math.Abs(concreteMaterial.CalculateFcd(standard));
                var rb = gH_SectionChecker.Value.SectionCheckerAttribute.Section.Rebars.Where(i => i.EpsilonP == 0).First();
                var fyd = rb.RebarMaterial.CalculateFyd(standard);
                var fyk = rb.RebarMaterial.Fyk;

                double hUtile = height - concreteCover - longitudinalRebarDiameterY / 2.0;
                double bUtile = baseSection - concreteCover - longitudinalRebarDiameterX / 2.0;
                double areaLongitudinalRebarX = Math.PI * Math.Pow(longitudinalRebarDiameterX, 2) / 4.0;
                double areaLongitudinalRebarY = Math.PI * Math.Pow(longitudinalRebarDiameterY, 2) / 4.0;
                double areaLongitudinalRebarsX = areaLongitudinalRebarX * numberOfRebarX;
                double areaLongitudinalRebarsY = areaLongitudinalRebarY * numberOfRebarY;
                double rholX = Math.Min(0.02, areaLongitudinalRebarsX / (bUtile * height));
                double rholY = Math.Min(0.02, areaLongitudinalRebarsY / (baseSection * hUtile));

                double sigmaCP = Math.Abs(gH_ResultBeamForces.Value.N / (baseSection * height));
                sigmaCP = Math.Min(sigmaCP, Math.Abs(0.2 * fcd));

                double kX = Math.Min(1 + Math.Sqrt(200.0 / bUtile), 2.0);
                double vminX = 0.035 * Math.Pow(kX, 3.0 / 2.0) * Math.Pow(Math.Abs(concreteMaterial.Fck), 0.5);
                double ns_vrdsX = (0.18 * kX * Math.Pow(100 * rholX * Math.Abs(concreteMaterial.Fck), 1.0 / 3.0) / standard.GammaC + 0.15 * sigmaCP) * bUtile * height;
                double ns_vrdcX = (vminX + 0.15 * sigmaCP) * bUtile * height;

                double kY = Math.Min(1 + Math.Sqrt(200.0 / hUtile), 2.0);
                double vminY = 0.035 * Math.Pow(kY, 3.0 / 2.0) * Math.Pow(Math.Abs(concreteMaterial.Fck), 0.5);
                double ns_vrdsY = (0.18 * kY * Math.Pow(100 * rholY * Math.Abs(concreteMaterial.Fck), 1.0 / 3.0) / standard.GammaC + 0.15 * sigmaCP) * baseSection * hUtile;
                double ns_vrdcY = (vminY + 0.15 * sigmaCP) * baseSection * hUtile;

                double ns_vrdX = Math.Max(ns_vrdsX, ns_vrdcX);
                double ns_vrdY = Math.Max(ns_vrdsY, ns_vrdcY);
                double ns_wrX = Math.Abs(gH_ResultBeamForces.Value.V1) / ns_vrdX;
                double ns_wrY = Math.Abs(gH_ResultBeamForces.Value.V2) / ns_vrdY;

                string toExcel = $"{concreteMaterial.Name};{concreteMaterial.Rck};{concreteMaterial.Fck};{fcd};{fyk};{fyd};" +
                    $"{longitudinalRebarDiameterX};{rholX};{numberOfRebarX};{areaLongitudinalRebarsX};{baseSection};{height};{concreteCover};" +
                    $"{bUtile};{concreteArea};{sigmaCP};1;{ns_vrdsX / 1000.0};{ns_vrdcX / 1000.0};{ns_vrdX / 1000.0};{ns_wrX};{1};" +
                    $"{longitudinalRebarDiameterY};{rholY};{numberOfRebarY};{areaLongitudinalRebarsY};{baseSection};{height};{concreteCover};" +
                    $"{hUtile};{concreteArea};{sigmaCP};1;{ns_vrdsY / 1000.0};{ns_vrdcY / 1000.0};{ns_vrdY / 1000.0};{ns_wrY};{1}";

                if (stirrupDiameter > 0)
                {
                    double stirrupAreaY = numberOfLegsY * Math.Pow(stirrupDiameter / 2.0, 2) * Math.PI;
                    double stirrupAreaX = numberOfLegsX * Math.Pow(stirrupDiameter / 2.0, 2) * Math.PI;
                    hUtile = height - concreteCover - longitudinalRebarDiameterX / 2.0 - stirrupDiameter;
                    bUtile = baseSection - concreteCover - longitudinalRebarDiameterY / 2.0 - stirrupDiameter;

                    sigmaCP = Math.Abs(gH_ResultBeamForces.Value.N / (baseSection * height));
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

                    double ni = 0.5;
                    double cotTeta1 = 1.0;
                    double cotTeta2X = Math.Sqrt((ni * fcd * height * alphaC) / (stirrupAreaX / spacing * fyd * Math.Sin(double.DegreesToRadians(stirrupSlopeY))) - 1);
                    double cotTeta2Y = Math.Sqrt((ni * fcd * baseSection * alphaC) / (stirrupAreaY / spacing * fyd * Math.Sin(double.DegreesToRadians(stirrupSlopeX))) - 1);
                    double cotTeta3 = 2.5;

                    double cotTetaY = cotTeta1;
                    if (cotTeta2Y > cotTetaY)
                        cotTetaY = cotTeta2Y;
                    if (cotTeta3 < cotTetaY)
                        cotTetaY = cotTeta3;

                    double cotTetaX = cotTeta1;
                    if (cotTeta2X > cotTetaX)
                        cotTetaX = cotTeta2X;
                    if (cotTeta3 < cotTetaX)
                        cotTetaX = cotTeta3;

                    double tetaX = double.RadiansToDegrees(Math.PI / 2.0 - Math.Atan(cotTetaX));
                    double tetaY = double.RadiansToDegrees(Math.PI / 2.0 - Math.Atan(cotTetaY));

                    double vrsdX = 0.9 * bUtile * stirrupAreaX / spacing * fyd * (1.0 / Math.Tan(double.DegreesToRadians(stirrupSlopeX)) + cotTetaX) * Math.Sin(double.DegreesToRadians(stirrupSlopeX));
                    double vrsdY = 0.9 * hUtile * stirrupAreaY / spacing * fyd * (1.0 / Math.Tan(double.DegreesToRadians(stirrupSlopeY)) + cotTetaY) * Math.Sin(double.DegreesToRadians(stirrupSlopeY));

                    double vrcdX = 0.9 * bUtile * height * alphaC * ni * fcd * (1.0 / Math.Tan(double.DegreesToRadians(stirrupSlopeX)) + cotTetaX) / (1 + Math.Pow(cotTetaX, 2));
                    double vrcdY = 0.9 * hUtile * baseSection * alphaC * ni * fcd * (1.0 / Math.Tan(double.DegreesToRadians(stirrupSlopeY)) + cotTetaY) / (1 + Math.Pow(cotTetaY, 2));

                    double vrdX = Math.Min(vrsdX, vrcdX);
                    double vrdY = Math.Min(vrsdY, vrcdY);

                    double wrX = Math.Abs(gH_ResultBeamForces.Value.V1) / vrdX;
                    double wrY = Math.Abs(gH_ResultBeamForces.Value.V2) / vrdY;

                    toExcel += ($"{concreteMaterial.Name};{concreteMaterial.Rck};{concreteMaterial.Fck};{fcd};{fyk};{fyd};{longitudinalRebarDiameterX};{stirrupDiameter};{spacing};" +
                        $"{numberOfLegsX};{stirrupSlopeY};{stirrupAreaX};{baseSection};{height};{concreteCover};{bUtile};{concreteArea};{sigmaCP};{alphaC};{ni};{cotTetaX};{tetaX};{vrsdX / 1000.0};{vrcdX / 1000.0};{vrdX / 1000.0};{wrX};{1};" +
                        $"{numberOfLegsY};{stirrupSlopeX};{stirrupAreaY};{height};{baseSection};{concreteCover};{hUtile};{concreteArea};{sigmaCP};{alphaC};{ni};{cotTetaY};{tetaY};{vrsdY / 1000.0};{vrcdY / 1000.0};{vrdY / 1000.0};{wrY};{1}");

                    DA.SetData(cc++, toExcelHeader);
                    DA.SetData(cc++, toExcel);

                    DA.SetData(cc++, ns_vrdsX / 1000.0);
                    DA.SetData(cc++, ns_vrdsY / 1000.0);
                    DA.SetData(cc++, ns_vrdcX / 1000.0);
                    DA.SetData(cc++, ns_vrdcY / 1000.0);
                    DA.SetData(cc++, ns_vrdX / 1000.0);
                    DA.SetData(cc++, ns_vrdY / 1000.0);

                    DA.SetData(cc++, vrsdX / 1000.0);
                    DA.SetData(cc++, vrsdY / 1000.0);
                    DA.SetData(cc++, vrcdX / 1000.0);
                    DA.SetData(cc++, vrcdY / 1000.0);
                    DA.SetData(cc++, vrdX / 1000.0);
                    DA.SetData(cc++, vrdY / 1000.0);

                    DA.SetData(cc++, Math.Round(ns_wrX, 3));
                    DA.SetData(cc++, Math.Round(ns_wrY, 3));
                    DA.SetData(cc++, Math.Round(wrX, 3));
                    DA.SetData(cc++, Math.Round(wrY, 3));
                }
                else
                {
                    toExcel += ($"0;0;0;0;0;0;0;0;0;" +
                        $"0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;{1};" +
                        $"0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;0;{1}");

                    DA.SetData(cc++, toExcelHeader);
                    DA.SetData(cc++, toExcel);

                    DA.SetData(cc++, ns_vrdsX / 1000.0);
                    DA.SetData(cc++, ns_vrdsY / 1000.0);
                    DA.SetData(cc++, ns_vrdcX / 1000.0);
                    DA.SetData(cc++, ns_vrdcY / 1000.0);
                    DA.SetData(cc++, ns_vrdX / 1000.0);
                    DA.SetData(cc++, ns_vrdY / 1000.0);

                    DA.SetData(cc++, 0);
                    DA.SetData(cc++, 0);
                    DA.SetData(cc++, 0);
                    DA.SetData(cc++, 0);
                    DA.SetData(cc++, 0);
                    DA.SetData(cc++, 0);

                    DA.SetData(cc++, Math.Round(ns_wrX, 3));
                    DA.SetData(cc++, Math.Round(ns_wrY, 3));
                    DA.SetData(cc++, 0);
                    DA.SetData(cc++, 0);
                }
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("73aec799-492f-4978-9e12-48faf9853d10");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
