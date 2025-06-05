using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.Cases;

namespace Rhino2Fem.Core.Loads
{
    public class SelfWeightModel
    {
        public LoadCaseModel LoadCase { get; set; }

        public double FactorX { get; set; }

        public double FactorY { get; set; }

        public double FactorZ { get; set; }

        public SelfWeightModel(LoadCaseModel loadCase, double factorX, double factorY, double factorZ)
        {
            LoadCase = loadCase;
            FactorX = factorX;
            FactorY = factorY;
            FactorZ = factorZ;
        }

        public SelfWeightModel()
        {
        }

        public SelfWeightModel(SelfWeightModel model)
        {
            FactorX = model.FactorX;
            FactorY = model.FactorY;    
            FactorZ = model.FactorZ;
            LoadCase = new LoadCaseModel(model.LoadCase);
        }
    }
}
