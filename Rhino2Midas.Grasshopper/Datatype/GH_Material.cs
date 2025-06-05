using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_Material : GH_Goo<MaterialModel>
    {
        public GH_Material(MaterialModel model)
        {
            Value = model;
        }

        public GH_Material()
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
            return "Name: " + Value.Name + ", \r" +
                "Type: " + Value.Type;
        }
    }
}
