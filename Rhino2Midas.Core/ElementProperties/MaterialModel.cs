using System;
using System.Collections.Generic;
using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.ElementProperties
{
    public class MaterialModel : ModelObjectId, IEquatable<MaterialModel>
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
            : base(materialModel.Id, materialModel.Name)
        {
            Type = materialModel.Type;
            DampingRatio = materialModel.DampingRatio;
            ModulusElasticity = materialModel.ModulusElasticity;
            PoissonRatio = materialModel.PoissonRatio;
            ThermalCoefficient = materialModel.ThermalCoefficient;
            Density = materialModel.Density;
            Mass = materialModel.Mass;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MaterialModel);
        }

        public bool Equals(MaterialModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   Type == other.Type &&
                   DampingRatio == other.DampingRatio &&
                   ModulusElasticity == other.ModulusElasticity &&
                   PoissonRatio == other.PoissonRatio &&
                   ThermalCoefficient == other.ThermalCoefficient &&
                   Density == other.Density &&
                   Mass == other.Mass;
        }

        public override bool EqualsWithId(object obj)
        {
            return EqualsWithId(obj as MaterialModel);
        }

        public bool EqualsWithId(MaterialModel other)
        {
            return !(other is null) &&
                   base.EqualsWithId(other) &&
                   Name == other.Name &&
                   Type == other.Type &&
                   DampingRatio == other.DampingRatio &&
                   ModulusElasticity == other.ModulusElasticity &&
                   PoissonRatio == other.PoissonRatio &&
                   ThermalCoefficient == other.ThermalCoefficient &&
                   Density == other.Density &&
                   Mass == other.Mass;
        }

        public override int GetHashCode()
        {
            int hashCode = 1413619688;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + DampingRatio.GetHashCode();
            hashCode = hashCode * -1521134295 + ModulusElasticity.GetHashCode();
            hashCode = hashCode * -1521134295 + PoissonRatio.GetHashCode();
            hashCode = hashCode * -1521134295 + ThermalCoefficient.GetHashCode();
            hashCode = hashCode * -1521134295 + Density.GetHashCode();
            hashCode = hashCode * -1521134295 + Mass.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MaterialModel left, MaterialModel right)
        {
            return EqualityComparer<MaterialModel>.Default.Equals(left, right);
        }

        public static bool operator !=(MaterialModel left, MaterialModel right)
        {
            return !(left == right);
        }
    }
}
