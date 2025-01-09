using Grasshopper.Kernel.Types;
using Rhino2Midas.Core.Settings;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_Units : GH_Goo<ModelUnits>
    {
        public GH_Units(ModelUnits model)
        {
            Value = model;
        }

        public GH_Units()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "ModelUnits";

        public override string TypeDescription => "ModelUnits";

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
