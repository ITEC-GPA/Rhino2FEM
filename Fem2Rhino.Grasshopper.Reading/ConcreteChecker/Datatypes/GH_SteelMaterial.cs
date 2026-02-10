using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;
using GPC.Model.Materials;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_SteelMaterial : GH_Goo<SteelMaterialEN1992>
    {
        public GH_SteelMaterial(SteelMaterialEN1992 model)
        {
            Value = model;
        }

        public GH_SteelMaterial()
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
            return "Name: " + Value.Name + ", \r\n" +
                "Elastic modulus: " + Value.E + "MPa" + ", \r\n" +
                "Yield strength: " + Value.Fyk + "MPa" + ", \r\n" +
                "Ultimate strength: " + Value.Fu + "MPa" + ", \r\n" +
                "Strain at maximum strength: " + Value.StrainUTension + "‰" + ", \r\n" +
                "Strain at yielding: " + Value.StrainYTension + "‰" + ", \r\n" +
                "Density: " + Value.Density + "kg/m3" + ", \r\n" +
                "Thermal expansion coefficient: " + Value.AlfaThermalExpansion + " 1/°C";
        }
    }
}
