namespace Rhino2Midas.Core.Settings
{
    public class ModelUnits
    {
        public enum ForceUnitTypes
        {
            KN,
            N,
        }

        public enum LengthUnitTypes
        {
            M,
            CM,
            MM,
        }

        public enum HeatUnitTypes
        {
            KJ,
            J,
            BTU,
            KCAL,
            CAL,
        }

        public enum TemperatureUnitTypes
        {
            C,
            F,
        }

        public ForceUnitTypes ForceUnit { get; set; }

        public LengthUnitTypes LengthUnit { get; set; }

        public HeatUnitTypes HeatUnit { get; set; }

        public TemperatureUnitTypes TemperatureUnit { get; set; }

        public double Tolerance { get; set; }

        public ModelUnits(ForceUnitTypes forceUnit, LengthUnitTypes lengthUnit, HeatUnitTypes heatUnit, TemperatureUnitTypes temperatureUnit, double tolerance)
        {
            ForceUnit = forceUnit;
            LengthUnit = lengthUnit;
            HeatUnit = heatUnit;
            TemperatureUnit = temperatureUnit;
            Tolerance = tolerance;
        }

        public ModelUnits()
        {
        }
    }
}
