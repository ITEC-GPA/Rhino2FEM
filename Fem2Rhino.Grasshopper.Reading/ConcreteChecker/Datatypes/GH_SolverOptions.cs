using GPC.Checkers.Concrete.Checkers;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_SolverOptions : GH_Goo<SectionChecker.SectionOptions>
    {
        public GH_SolverOptions(SectionChecker.SectionOptions model)
        {
            Value = model;
        }

        public GH_SolverOptions()
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
