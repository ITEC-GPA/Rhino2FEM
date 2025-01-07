using Grasshopper.Kernel.Types;
using Rhino2Midas.Core.Cases;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_LoadCombination : GH_Goo<LoadCombinationModel>
    {
        public GH_LoadCombination(LoadCombinationModel model)
        {
            Value = model;
        }

        public GH_LoadCombination()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "LoadCombinationModel";

        public override string TypeDescription => "LoadCombinationModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Load combination name: " + Value.Name;
        }
    }
}
