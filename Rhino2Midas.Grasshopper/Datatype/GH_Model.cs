using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.Models;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_Model : GH_Goo<ModelModel>
    {
        public GH_Model(ModelModel model)
        {
            Value = model;
        }

        public GH_Model()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "ModelModel";

        public override string TypeDescription => "ModelModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "";
        }
    }
}
