using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_Standard : GH_Goo<StandardModelCode2010>
    {
        public GH_Standard(StandardModelCode2010 model)
        {
            Value = model;
        }

        public GH_Standard()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "v";

        public override string TypeDescription => "StandardModelCode2010";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Standard Name: " + Value.Name + ", \r\n" +
                "Standard Description: " + Value.Remarks + ", \r\n" +
                "Gamma C: " + Value.GammaC + ", \r\n" +
                "Gamma C Acc: " + Value.GammaCAccidental + ", \r\n" +
                "Gamma CE: " + Value.GammaCE + ", \r\n" +
                "Gamma S: " + Value.GammaS + ", \r\n" +
                "Gamma S Acc: " + Value.GammaSAccidental + ", \r\n" +
                "Gamma S Prestress: " + Value.GammaSPrestress + ", \r\n" +
                "Gamma S Prestress Acc: " + Value.GammaSPrestressAccidental + ", \r\n" +
                "Alpha CC: " + Value.AlphaCC + ", \r\n" +
                "Alpha CT: " + Value.AlphaCT + ", \r\n" +
                "Steel Coeff. ultimate strain: " + Value.SteelCoefficientStrainTension + ", \r\n";
        }
    }
}
