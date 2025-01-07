namespace Rhino2Midas.Core.ElementProperties
{
    public class AreaThicknessModel
    {
        public string Name { get; set; }

        public double Thickness { get; set; }

        public double Offset { get; set; }

        public AreaThicknessModel(string name, double thickness, double offset = 0)
        {
            Name = name;
            Thickness = thickness;
            Offset = offset;
        }

        public AreaThicknessModel()
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
