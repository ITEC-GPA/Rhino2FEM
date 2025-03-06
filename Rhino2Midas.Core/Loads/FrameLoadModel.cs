using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Cases;

namespace Rhino2Midas.Core.Loads
{
    public class FrameLoadModel
    {
        public enum FrameLoadTypes
        {
            Force,
            Moment,
        }

        public enum LoadDirections
        {
            Lx,
            Ly,
            Lz,
            Gx,
            Gy,
            Gz,
        }

        public LoadCaseModel LoadCase { get; set; }

        public FrameLoadTypes LoadType { get; set; }

        public LoadDirections Direction { get; set; }

        public bool IsProjected { get; set; }

        public double StartLocationRelative { get; set; }

        public double StartLoad { get; set; }

        public double EndLocationRelative { get; set; }

        public double EndLoad { get; set; }

        public LoadGroupModel LoadGroup { get; set; }

        public FrameLoadModel(LoadCaseModel loadCase, FrameLoadTypes forceOrMoment, LoadDirections direction, bool isProjected, double startLocationRelative, double startLoad, double endLocationRelative, double endLoad)
        {
            LoadCase = loadCase;
            LoadType = forceOrMoment;
            Direction = direction;
            IsProjected = isProjected;
            StartLocationRelative = startLocationRelative;
            StartLoad = startLoad;
            EndLocationRelative = endLocationRelative;
            EndLoad = endLoad;
        }

        public FrameLoadModel()
        {
        }

        public FrameLoadModel(FrameLoadModel frameLoadModel)
        {
            LoadCase = frameLoadModel.LoadCase;
            LoadType = frameLoadModel.LoadType;
            Direction = frameLoadModel.Direction;
            IsProjected = frameLoadModel.IsProjected;
            StartLocationRelative = frameLoadModel.StartLocationRelative;
            StartLoad = frameLoadModel.StartLoad;
            EndLocationRelative= frameLoadModel.EndLocationRelative;
            EndLoad = frameLoadModel.EndLoad;
            LoadGroup = frameLoadModel.LoadGroup;
        }
    }
}
