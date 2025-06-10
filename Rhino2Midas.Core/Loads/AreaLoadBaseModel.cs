using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Core.Loads
{
    public abstract class AreaLoadBaseModel
    {
        public LoadCaseModel LoadCase { get; set; }

        public AreaLoadBaseModel(LoadCaseModel loadCase)
        {
            LoadCase = loadCase;
        }

        public AreaLoadBaseModel()
        {
        }

        public AreaLoadBaseModel(AreaLoadBaseModel areaLoadModel)
        {
            LoadCase = areaLoadModel.LoadCase;
        }
    }
}
