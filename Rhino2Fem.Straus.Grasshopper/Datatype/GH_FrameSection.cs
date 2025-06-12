using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_FrameSection : GH_Goo<FrameSectionModel>
    {
        public GH_FrameSection(FrameSectionModel model)
        {
            Value = model;
        }

        public GH_FrameSection()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "FrameSectionModel";

        public override string TypeDescription => "FrameSectionModel";

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Name: " + Value.Name + ", \r" +
                "Type: " + Value.SectionGeometryType;
        }
    }
}
