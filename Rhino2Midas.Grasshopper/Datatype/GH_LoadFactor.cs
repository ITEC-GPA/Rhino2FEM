using Grasshopper.Kernel.Types;
using Rhino2Midas.Core.Cases;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_LoadFactor : GH_Goo<LoadFactorModel>
    {
        public GH_LoadFactor(LoadFactorModel model)
        {
            Value = model;
        }

        public GH_LoadFactor()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "LoadFactorModel";

        public override string TypeDescription => "LoadFactorModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Load Factor: " + Value.LoadCase.Name + " - " + Value.Factor;
        }
    }
}
