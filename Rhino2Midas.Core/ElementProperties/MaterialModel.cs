using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.ElementProperties
{
    public class MaterialModel : ModelObjectId
    {
        public enum MaterialTypes
        {
            Steel,
            Concrete,
        }

        public MaterialTypes Type { get; set; }

        public double DampingRatio { get; set; }

        public double ModulusElasticity { get; set; }

        public double PoissonRatio { get; set; }

        public double ThermalCoefficient { get; set; }

        public double Density { get; set; }

        public double Mass { get; set; }

        public MaterialModel(string name, MaterialTypes type, double dampingRatio, double modulusElasticity, double poissonRatio, double thermalCoefficient, double density, double mass)
            : base(name)
        {
            Type = type;
            DampingRatio = dampingRatio;
            ModulusElasticity = modulusElasticity;
            PoissonRatio = poissonRatio;
            ThermalCoefficient = thermalCoefficient;
            Density = density;
            Mass = mass;
        }

        public MaterialModel()
            : base()
        {
        }

        public MaterialModel(MaterialModel materialModel)
            : base(materialModel.Name)
        {
            Type = materialModel.Type;
            DampingRatio = materialModel.DampingRatio;
            ModulusElasticity = materialModel.ModulusElasticity;
            PoissonRatio = materialModel.PoissonRatio;
            ThermalCoefficient = materialModel.ThermalCoefficient;
            Density = materialModel.Density;
            Mass = materialModel.Mass;
        }
    }
}
