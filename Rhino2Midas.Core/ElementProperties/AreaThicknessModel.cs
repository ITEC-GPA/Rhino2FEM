using System;
using System.Collections.Generic;
using Rhino2Fem.Core.Base;

namespace Rhino2Fem.Core.ElementProperties
{
    public class AreaThicknessModel : ModelObjectId, IEquatable<AreaThicknessModel>
    {
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
            :base(areaThicknessModel.Id, areaThicknessModel.Name)
        {
            Thickness = areaThicknessModel.Thickness;
            Offset = areaThicknessModel.Offset;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AreaThicknessModel);
        }

        public bool Equals(AreaThicknessModel other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Name == other.Name &&
                   Thickness == other.Thickness &&
                   Offset == other.Offset;
        }

        public override int GetHashCode()
        {
            int hashCode = 475748675;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Thickness.GetHashCode();
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(AreaThicknessModel left, AreaThicknessModel right)
        {
            return EqualityComparer<AreaThicknessModel>.Default.Equals(left, right);
        }

        public static bool operator !=(AreaThicknessModel left, AreaThicknessModel right)
        {
            return !(left == right);
        }
    }
}
