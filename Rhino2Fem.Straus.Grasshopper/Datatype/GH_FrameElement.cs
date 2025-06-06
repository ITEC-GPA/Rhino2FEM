using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Fem.Core.Elements;

namespace Rhino2Fem.Grasshopper.Datatype
{
    public class GH_FrameElement : GH_GeometricGoo<FrameElementModel>, IGH_PreviewData
    {
        public Guid Guid { get; set; }

        public override bool IsValid => true;

        public override string TypeName => "Frame";

        public override string TypeDescription => "Frame";

        BoundingBox IGH_PreviewData.ClippingBox => Boundingbox;

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                else
                    return new BoundingBox(Value.NodeStart.Position, Value.NodeEnd.Position);
            }
        }

        public GH_FrameElement(FrameElementModel nodeModel, Guid? guid = null)
        {
            if (nodeModel != null)
                Value = new FrameElementModel(nodeModel);
            else
                Value = new FrameElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_FrameElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return (IGH_Goo)new FrameElementModel(Value);
        }

        public override string ToString()
        {
            return $"Frame element from point \r" +
                $"X: {Value.NodeStart.X}, Y: {Value.NodeStart.Y}, Z: {Value.NodeStart.Z} to point" +
                $"X: {Value.NodeEnd.X}, Y: {Value.NodeEnd.Y}, Z: {Value.NodeEnd.Z}";
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            Value.DrawWireframe(args.Pipeline, args.Viewport, args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Value.DrawSolid(args.Pipeline, args.Viewport, args.Material.Diffuse);
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return (IGH_GeometricGoo)new FrameElementModel(Value);
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
            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotImplementedException();
        }

        #region Casting methods

        public override bool CastTo<Q>(out Q target)
        {
            if (typeof(Q) == typeof(GH_Curve))
            {
                LineCurve pline = new LineCurve(Value.NodeStart.Position, Value.NodeEnd.Position);
                target = (Q)Convert.ChangeType(new GH_Curve(pline), typeof(Q));
                return true;
            }
            else if (typeof(Q) == typeof(GH_Brep))
            {
                if (Value.Breps == null || Value.Breps.Count == 0)
                {
                    target = default(Q);
                    return false;
                }
                if (Value.Breps.Count == 1)
                {
                    Brep brep = Value.Breps[0].DuplicateBrep();
                    target = (Q)Convert.ChangeType(new GH_Brep(brep), typeof(Q));
                    return true;
                }
            }

            target = default(Q);
            return false;
        }

        #endregion
    }
}
