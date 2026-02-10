using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_FailureDomain3DResult : GH_Goo<FailureDomainResult>
    {
        public GH_FailureDomain3DResult(FailureDomainResult model)
        {
            Value = model;
        }

        public GH_FailureDomain3DResult()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "FailureDomainResult";

        public override string TypeDescription => "FailureDomainResult";

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
