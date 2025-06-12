using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Core.Loads
{
    public abstract class FrameLoadBaseModel
    {
        public LoadCaseModel LoadCase { get; set; }

        public CoordinateSystemModel CoordinateSystem { get; set; }

        public FrameLoadBaseModel(LoadCaseModel loadCase, CoordinateSystemModel coordinateSystem)
        {
            LoadCase = loadCase;
            CoordinateSystem = coordinateSystem;
        }

        public FrameLoadBaseModel()
        {
        }

        public FrameLoadBaseModel(FrameLoadBaseModel frameLoadModel)
        {
            LoadCase = frameLoadModel.LoadCase;
            CoordinateSystem = frameLoadModel.CoordinateSystem;
        }
    }
}
