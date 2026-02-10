using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_CheckerOptions : GH_Goo<SectionCheckerModelCode2010.SectionOptionsModelCode2010>
    {
        public GH_CheckerOptions(SectionCheckerModelCode2010.SectionOptionsModelCode2010 model)
        {
            Value = model;
        }

        public GH_CheckerOptions()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "v";

        public override string TypeDescription => "SectionOptions";

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
