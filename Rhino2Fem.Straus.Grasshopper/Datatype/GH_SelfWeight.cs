using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_SelfWeight : GH_Goo<SelfWeightModel>
    {
        public GH_SelfWeight(SelfWeightModel model)
        {
            Value = model;
        }

        public GH_SelfWeight()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "SelfWeightModel";

        public override string TypeDescription => "SelfWeightModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Load case name: " + Value.LoadCase.Name + ", \r" +
                "FactorX: " + Value.FactorX + "FactorY: " + Value.FactorY + "FactorZ: " + Value.FactorZ;
        }
    }
}
