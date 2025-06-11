using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Core.Loads
{
    public class AreaLoadMidasModel : AreaLoadBaseModel
    {
        public enum LoadDirections
        {
            Lx,
            Ly,
            Lz,
            Gx,
            Gy,
            Gz,
        }

        public LoadDirections Direction { get; set; }

        public bool IsProjected { get; set; }

        public double P1 { get; set; }

        public double P2 { get; set; }

        public double P3 { get; set; }

        public double P4 { get; set; }

        public LoadGroupModel LoadGroup { get; set; }

        public AreaLoadMidasModel(LoadCaseModel loadCase, LoadDirections direction, bool isProjected = false, double p1 = 0, double p2 = 0, double p3 = 0, double p4 = 0)
            :base(loadCase)
        {
            Direction = direction;
            IsProjected = isProjected;
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;

            if(direction == LoadDirections.Lx || direction == LoadDirections.Ly || direction == LoadDirections.Lz)
                CoordinateSystem = CoordinateSystemModel.Local;            
            else
                CoordinateSystem = CoordinateSystemModel.Global;            
        }

        public AreaLoadMidasModel()
        {
        }

        public AreaLoadMidasModel(AreaLoadMidasModel areaLoadModel)
            : base(areaLoadModel)
        {
            Direction = areaLoadModel.Direction;
            IsProjected = areaLoadModel.IsProjected;
            P1 = areaLoadModel.P1;
            P2 = areaLoadModel.P2;
            P3 = areaLoadModel.P3;
            P4 = areaLoadModel.P4;
            LoadGroup = areaLoadModel.LoadGroup;
        }
    }
}
