using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.Results;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel.Types;
using Rhino2Fem.Core.ElementProperties;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_StressAnalysisResult : GH_Goo<StressAnalysisResult>
    {
        public GH_StressAnalysisResult(StressAnalysisResult model)
        {
            Value = model;
        }

        public GH_StressAnalysisResult()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "StressAnalysisResult";

        public override string TypeDescription => "StressAnalysisResult";

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
