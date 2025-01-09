using Rhino.Geometry;
using Rhino2Midas.Core.Base;

namespace Rhino2Midas.Core.ElementProperties
{
    public class FramePropertyModel : ModelObjectId
    {
        public enum FramePropertyTypes
        {
            SB,
            SR,
            P,
            L,
            C,
            H,
            T,
            B,

        }

        public enum OffsetTypes
        {
            LT,
            CT,
            RT,
            LC,
            CC,
            RC,
            LB,
            CB,
            RB,
        }

        public int Number { get; set; }

        public FramePropertyTypes Type { get; set; }

        public OffsetTypes Offset { get; set; }

        public double Dimension1 { get; set; }

        public double Dimension2 { get; set; }

        public double Dimension3 { get; set; }

        public double Dimension4 { get; set; }

        public double Dimension5 { get; set; }

        public double Dimension6 { get; set; }

        public double Dimension7 { get; set; }

        public double Dimension8 { get; set; }

        public double Dimension9 { get; set; }

        public double Dimension10 { get; set; }

        public Brep[] Breps { get; set; }

        public FramePropertyModel(string name, FramePropertyTypes type, OffsetTypes offset, double dimension1 = 0.0, double dimension2 = 0.0,
            double dimension3 = 0.0, double dimension4 = 0.0, double dimension5 = 0.0, double dimension6 = 0.0, double dimension7 = 0.0,
            double dimension8 = 0.0, double dimension9 = 0.0, double dimension10 = 0.0)
            :base(name)
        {
            Type = type;
            Offset = offset;
            Dimension1 = dimension1;
            Dimension2 = dimension2;
            Dimension3 = dimension3;
            Dimension4 = dimension4;
            Dimension5 = dimension5;
            Dimension6 = dimension6;
            Dimension7 = dimension7;
            Dimension8 = dimension8;
            Dimension9 = dimension9;
            Dimension10 = dimension10;
        }

        public FramePropertyModel()
            :base()
        {
        }

        public FramePropertyModel(FramePropertyModel framePropertyModel)
            : base(framePropertyModel.Name)
        {
            Type = framePropertyModel.Type;
            Offset = framePropertyModel.Offset;
            Dimension1 = framePropertyModel.Dimension1;
            Dimension2 = framePropertyModel.Dimension2;
            Dimension3 = framePropertyModel.Dimension3;
            Dimension4 = framePropertyModel.Dimension4;
            Dimension5 = framePropertyModel.Dimension5; 
            Dimension6 = framePropertyModel.Dimension6;
            Dimension7 = framePropertyModel.Dimension7;
            Dimension8 = framePropertyModel.Dimension8;
            Dimension9 = framePropertyModel.Dimension9;
            Dimension10 = framePropertyModel.Dimension10;
        }
    }
}
