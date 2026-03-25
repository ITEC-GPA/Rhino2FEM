using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Model.Materials;
using Grasshopper.Kernel;
using Rhino2Fem.Core.Helper;
using System;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class CircularSectionShearCheckComponent : GH_Component
    {
        public CircularSectionShearCheckComponent()
            : base("Circular Section Shear Check", "SC", "Shear Check", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER)
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force", "F", "Force", GH_ParamAccess.item);
            pManager.AddNumberParameter("Diameter", "D", "Diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup Diameter", "SD", "Stirrup Diameter", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Legs", "NL", "Number of Legs of a stirrup", GH_ParamAccess.item);
            pManager.AddNumberParameter("Spacing", "S", "Spacing", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stirrup slope", "SS", "Stirrup slope", GH_ParamAccess.item);
            pManager.AddNumberParameter("Longitudinal rebar diameter", "LRD", "Longitudinal rebar diameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Concrete Cover", "CC", "Concrete Cover", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("To Excel Header", "TEH", "To Excel Header", GH_ParamAccess.item);
            pManager.AddTextParameter("To Excel", "TE", "To Excel", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrsd", "Vrsd", "Vrsd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrcd", "Vrcd", "Vrcd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Vrd", "Vrd", "Vrd", GH_ParamAccess.item);
            pManager.AddNumberParameter("Working Ratio", "WR", "Working Ratio", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_SectionChecker gH_SectionChecker = null;
            GH_ResultBeamForces gH_ResultBeamForces = null;
            double diameter = 0;
            double stirrupDiameter = 0;
            double spacing = 0;
            int numberOfLegs = 0;
            double stirrupSlope = 0;
            double longitudinalRebarDiameter = 0;
            double concreteCover = 0;

            int count = 0;
            if (DA.GetData(count++, ref gH_SectionChecker) && DA.GetData(count++, ref gH_ResultBeamForces) && DA.GetData(count++, ref diameter)
                && DA.GetData(count++, ref stirrupDiameter) && DA.GetData(count++, ref numberOfLegs) && DA.GetData(count++, ref spacing)
                && DA.GetData(count++, ref stirrupSlope) && DA.GetData(count++, ref longitudinalRebarDiameter) && DA.GetData(count++, ref concreteCover))
            {
                GPC.Model.Standards.StandardModelCode2010 standard = gH_SectionChecker.Value.StandardModelCode2010;
                ConcreteMaterialEuropeanCommon concreteMaterial = (ConcreteMaterialEuropeanCommon)gH_SectionChecker.Value.SectionCheckerAttribute.Section.ConcreteMaterial;
                var fcd = Math.Abs(concreteMaterial.CalculateFcd(standard));
                var rb = gH_SectionChecker.Value.SectionCheckerAttribute.Section.Rebars.Where(i => i.EpsilonP == 0).First();
                var fyd = rb.RebarMaterial.CalculateFyd(standard);
                var fyk = rb.RebarMaterial.Fyk;
                double stirrupArea = numberOfLegs * Math.Pow(stirrupDiameter / 2.0, 2) * Math.PI;
                double hUtile = diameter - concreteCover - longitudinalRebarDiameter / 2.0 - stirrupDiameter;
                double area = Math.Pow(diameter, 2) * Math.PI / 4.0;
                double sigmaCP = Math.Abs(gH_ResultBeamForces.Value.N / area);
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
                double cotTeta2 = Math.Sqrt((ni * fcd * diameter * alphaC) / (stirrupArea / spacing * fyd * Math.Sin(double.DegreesToRadians(stirrupSlope))) - 1);
                double cotTeta3 = 2.5;

                double cotTeta = cotTeta1;
                if (cotTeta2 > cotTeta)
                    cotTeta = cotTeta2;
                if (cotTeta3 < cotTeta)
                    cotTeta = cotTeta3;

                double teta = double.RadiansToDegrees(Math.PI / 2.0 - Math.Atan(cotTeta));

                double vrsd = 0.75 * hUtile * stirrupArea / spacing * fyd * (1.0 / Math.Tan(double.DegreesToRadians(stirrupSlope)) + cotTeta) * Math.Sin(double.DegreesToRadians(stirrupSlope));
                double vrcd = 0.75 * hUtile * diameter * alphaC * ni * fcd * (1.0 / Math.Tan(double.DegreesToRadians(stirrupSlope)) + cotTeta) / (1 + Math.Pow(cotTeta, 2));

                double vrd = Math.Min(vrsd, vrcd);

                double wr = Math.Abs(Math.Sqrt(Math.Pow(gH_ResultBeamForces.Value.V1, 2) + Math.Pow(gH_ResultBeamForces.Value.V2, 2)) / vrd);

                string toExcelHeader = "Classe di calcestruzzo di progetto;Rck;fck;fcd;fyk;fyd;ϕlong;ϕst;s;n°;α;Asw;" +
                    "D;A;c;d;σcp;αc;ν;Cotg ϑx;ϑx;VRsdx;Vrcdx;VRdx;WRx;WRlim;" +
                    "h;bw;c;d;Ac;σcp;αc;ν;Cotg ϑy;ϑy;VRsdy;Vrcdy;VRdy;WRy;WRlim";
                string toExcel = $"{concreteMaterial.Name};{concreteMaterial.Rck};{concreteMaterial.Fck};{fcd};{fyk};{fyd};{longitudinalRebarDiameter};{stirrupDiameter};{spacing};" +
                    $"{numberOfLegs};{stirrupSlope};{stirrupArea};{diameter};{area};{concreteCover};{hUtile};{sigmaCP};{alphaC};{ni};{cotTeta};{teta};{vrsd / 1000.0};{vrcd / 1000.0};{vrd / 1000.0};{wr};{1}";

                int cc = 0;
                DA.SetData(cc++, toExcelHeader);
                DA.SetData(cc++, toExcel);
                DA.SetData(cc++, vrsd / 1000.0);
                DA.SetData(cc++, vrcd / 1000.0);
                DA.SetData(cc++, vrd / 1000.0);
                DA.SetData(cc++, wr);
            }
        }


        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("fdb8a104-8595-4b8f-9d1a-1ac36a664488");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
