using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_AreaProperty : GH_Goo<AreaPropertyModel>
    {
        public GH_AreaProperty(AreaPropertyModel model)
        {
            Value = model;
        }

        public GH_AreaProperty()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "AreaPropertyModel";

        public override string TypeDescription => "AreaPropertyModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + "\n" +
                "Thickness: " + Value.ThicknessMembrane;
        }
    }
}

