using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Core.Loads
{
    public class AreaPressureLoadModel : AreaLoadBaseModel
    {
        public double Value { get; set; }

        public AreaPressureLoadModel(LoadCaseModel loadCase, double p1 = 0)
            :base(loadCase)
        {
            Value = p1;
        }

        public AreaPressureLoadModel()
        {
        }

        public AreaPressureLoadModel(AreaPressureLoadModel areaLoadModel)
            : base(areaLoadModel)
        {
            Value = areaLoadModel.Value;
        }
    }
}
