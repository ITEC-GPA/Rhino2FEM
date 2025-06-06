using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.Attributes;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_BoundaryGroup : GH_Goo<BoundaryGroupModel>
    {
        public GH_BoundaryGroup(BoundaryGroupModel model)
        {
            Value = model;
        }

        public GH_BoundaryGroup()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "BoundaryGroupModel";

        public override string TypeDescription => "BoundaryGroupModel";

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
