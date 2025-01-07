using Rhino2Midas.Core.Cases;

namespace Rhino2Midas.Core.Loads
{
    public class NodalLoadModel
    {
        public LoadCaseModel LoadCase { get; set; }

        public double FX { get; set; }

        public double FY { get; set; }

        public double FZ { get; set; }

        public double MX { get; set; }

        public double MY { get; set; }

        public double MZ { get; set; }

        public double IndexLoadCase { get; set; }

        public NodalLoadModel(LoadCaseModel loadCase, double fX, double fY, double fZ, double mX, double mY, double mZ)
        {
            LoadCase = loadCase;
            FX = fX;
            FY = fY;
            FZ = fZ;
            MX = mX;
            MY = mY;
            MZ = mZ;
        }

        public NodalLoadModel()
        {
        }
    }
}
