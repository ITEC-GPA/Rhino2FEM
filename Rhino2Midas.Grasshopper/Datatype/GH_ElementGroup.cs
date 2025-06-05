using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.Attributes;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_ElementGroup : GH_Goo<ElementGroupModel>
    {
        public GH_ElementGroup(ElementGroupModel model)
        {
            Value = model;
        }

        public GH_ElementGroup()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "ElementGroupModel";

        public override string TypeDescription => "ElementGroupModel";

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
