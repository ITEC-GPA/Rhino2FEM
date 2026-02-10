using GPC.Model.Sections.Concrete;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes
{
    public class GH_Rebar : GH_GeometricGoo<ReinforcedConcreteRebar>, IGH_PreviewData
    {
        public GH_Rebar(ReinforcedConcreteRebar model)
        {
            Value = model;
        }

        public GH_Rebar()
        {

        }

        public override bool IsValid => true;

        public override string TypeName => "ReinforcedConcreteRebar";

        public override string TypeDescription => "ReinforcedConcreteRebar";


        BoundingBox IGH_PreviewData.ClippingBox => Boundingbox;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                else
                    return new BoundingBox(new Point3d(Value.Position.X, Value.Position.Y, 0), new Point3d(Value.Position.X, Value.Position.Y, 0));
            }
        }

        public override IGH_Goo Duplicate()
        {
            return null;
        }

        public override string ToString()
        {
            return "Position: " + Value.Position.ToString() + ", \r\n" +
                "Diameter: " + Value.RebarSection.Diameter + "mm" + ", \r\n" +
                "Area: " + Value.Area + "mm2" + ", \r\n" +
                "Material: " + Value.RebarMaterial.Name + ", \r\n" +
                "Pretension: " + Value.EpsilonP * Value.RebarMaterial.E + "MPa";
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            throw new System.NotImplementedException();
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            if (Value == null)
                return BoundingBox.Empty;
            if (!IsValid)
                return BoundingBox.Empty;
            return BoundingBox.Empty;
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            throw new System.NotImplementedException();
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new System.NotImplementedException();
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawCircle(new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0), args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            args.Pipeline.DrawCircle(new Circle(new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0), args.Material.Diffuse);
        }

        #region Casting methods

        public override bool CastTo<Q>(out Q target)
        {
            if (typeof(Q) == typeof(GH_Curve))
            {
                Circle circle = new Circle(Plane.WorldXY, new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0);
                target = (Q)Convert.ChangeType(new GH_Curve(circle.ToNurbsCurve()), typeof(Q));
                return true;
            }
            else if (typeof(Q) == typeof(GH_Circle))
            {
                Circle circle = new Circle(Plane.WorldXY, new Point3d(Value.Position.X, Value.Position.Y, 0), Value.RebarSection.Diameter / 2.0);
                target = (Q)Convert.ChangeType(new GH_Circle(circle), typeof(Q));
                return true;
            }

            target = default(Q);
            return false;
        }

        #endregion
    }
}
