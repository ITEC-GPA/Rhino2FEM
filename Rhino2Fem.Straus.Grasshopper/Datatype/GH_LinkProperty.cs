using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_LinkProperty : GH_Goo<LinkPropertyModel>
    {
        public GH_LinkProperty(LinkPropertyModel model)
        {
            Value = model;
        }

        public GH_LinkProperty()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "LinkPropertyModel";

        public override string TypeDescription => "LinkPropertyModel";

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
