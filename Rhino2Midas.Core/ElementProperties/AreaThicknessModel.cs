using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.ElementProperties
{
    public class AreaThicknessModel : ModelObjectId
    {
        public int Number { get; set; }

        public double Thickness { get; set; }

        public double Offset { get; set; }

        public AreaThicknessModel(string name, double thickness, double offset = 0)
            : base(name)
        {
            Thickness = thickness;
            Offset = offset;
        }

        public AreaThicknessModel()
            :base()
        {
        }

        public AreaThicknessModel(AreaThicknessModel areaThicknessModel)
        {
            Name = areaThicknessModel.Name;
            Thickness = areaThicknessModel.Thickness;
            Offset = areaThicknessModel.Offset;
        }
    }
}
