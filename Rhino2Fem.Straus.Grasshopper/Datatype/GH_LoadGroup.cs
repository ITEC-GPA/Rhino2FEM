using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.Attributes;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_LoadGroup : GH_Goo<LoadGroupModel>
    {
        public GH_LoadGroup(LoadGroupModel model)
        {
            Value = model;
        }

        public GH_LoadGroup()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "LoadGroupModel";

        public override string TypeDescription => "LoadGroupModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Name: " + Value.Name;
        }
    }
}
