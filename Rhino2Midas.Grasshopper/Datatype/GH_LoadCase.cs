using Grasshopper.Kernel.Types;
using Rhino2Midas.Core.Cases;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_LoadCase : GH_Goo<LoadCaseModel>
    {
        public GH_LoadCase(LoadCaseModel model)
        {
            Value = model;
        }

        public GH_LoadCase()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "LoadCaseModel";

        public override string TypeDescription => "LoadCaseModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Load case name: " + Value.Name;
        }
    }
}
