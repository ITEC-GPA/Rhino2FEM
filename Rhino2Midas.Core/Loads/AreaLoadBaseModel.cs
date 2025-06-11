using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Core.Loads
{
    public abstract class AreaLoadBaseModel
    {
        public LoadCaseModel LoadCase { get; set; }
        public CoordinateSystemModel CoordinateSystem { get; set; }

        public AreaLoadBaseModel(LoadCaseModel loadCase, CoordinateSystemModel coordinateSystem = null)
        {
            LoadCase = loadCase;
            if(coordinateSystem != null)
                CoordinateSystem = coordinateSystem;
        }

        public AreaLoadBaseModel()
        {
        }

        public AreaLoadBaseModel(AreaLoadBaseModel areaLoadModel)
        {
            LoadCase = areaLoadModel.LoadCase;
            CoordinateSystem = areaLoadModel.CoordinateSystem;
        }
    }
}
