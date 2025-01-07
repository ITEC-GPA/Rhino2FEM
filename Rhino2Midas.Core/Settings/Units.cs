namespace Rhino2Midas.Core.Settings
{
    public class ModelUnits
    {
        public enum ForceUnitTypes
        {
            kN,
            N,
        }

        public enum LengthUnitTypes
        {
            m,
            cm,
            mm,
        }

        public enum HeatUnitTypes
        {
            kJ,
        }

        public enum TemperatureUnitTypes
        {
            C,
        }

        public ForceUnitTypes ForceUnit { get; set; }

        public LengthUnitTypes LengthUnit { get; set; }

        public HeatUnitTypes HeatUnit { get; set; }

        public TemperatureUnitTypes TemperatureUnit { get; set; }

        public ModelUnits(ForceUnitTypes forceUnit, LengthUnitTypes lengthUnit, HeatUnitTypes heatUnit, TemperatureUnitTypes temperatureUnit)
        {
            ForceUnit = forceUnit;
            LengthUnit = lengthUnit;
            HeatUnit = heatUnit;
            TemperatureUnit = temperatureUnit;
        }

        public ModelUnits()
        {
        }
    }
}
