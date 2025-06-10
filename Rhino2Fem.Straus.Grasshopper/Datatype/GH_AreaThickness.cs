using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_AreaThickness : GH_Goo<AreaThicknessModel>
    {
        public GH_AreaThickness(AreaThicknessModel model)
        {
            Value = model;
        }

        public GH_AreaThickness()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "AreaThicknessModel";

        public override string TypeDescription => "AreaThicknessModel";

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

