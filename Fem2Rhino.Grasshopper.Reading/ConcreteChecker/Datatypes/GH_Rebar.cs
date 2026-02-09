using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_Rebar : GH_Goo<ReinforcedConcreteRebar>
    {
        public GH_Rebar(ReinforcedConcreteRebar model)
        {
            Value = model;
        }

        public GH_Rebar()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "v";

        public override string TypeDescription => "MaterialModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Position: " + Value.Position.ToString()+ ", \r\n" +
                "Diameter: " + Value.RebarSection.Diameter + "mm" + ", \r\n" +
                "Area: " + Value.Area + "mm2" + ", \r\n" +
                "Material: " + Value.RebarMaterial.Name + ", \r\n" +
                "Pretension: " + Value.EpsilonP * Value.RebarMaterial.E + "MPa";
        }
    }
}
