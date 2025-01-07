using Grasshopper.Kernel.Types;
using Rhino2Midas.Core.ElementProperties;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_FrameProperty : GH_Goo<FramePropertyModel>
    {
        public GH_FrameProperty(FramePropertyModel model)
        {
            Value = model;
        }

        public GH_FrameProperty()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "FrameProperty";

        public override string TypeDescription => "FrameProperty";

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
