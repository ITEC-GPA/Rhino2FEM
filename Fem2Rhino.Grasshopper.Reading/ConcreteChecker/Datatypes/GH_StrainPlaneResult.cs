using GPC.Checker.Results.ResultType;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_StrainPlaneResult : GH_Goo<StrainPlaneResult>
    {
        public GH_StrainPlaneResult(StrainPlaneResult model)
        {
            Value = model;
        }

        public GH_StrainPlaneResult()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "StrainPlaneResult";

        public override string TypeDescription => "StrainPlaneResult";

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
