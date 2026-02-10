using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_SectionChecker : GH_Goo<SectionCheckerModelCode2010>
    {
        public GH_SectionChecker(SectionCheckerModelCode2010 model)
        {
            Value = model;
        }

        public GH_SectionChecker()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "SectionCheckerModelCode2010";

        public override string TypeDescription => "SectionCheckerModelCode2010";

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
