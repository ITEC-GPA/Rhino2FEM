using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Midas.Core.Elements;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_LinkElement : GH_GeometricGoo<LinkElementModel>, IGH_PreviewData
    {
        public Guid Guid { get; set; }

        public override bool IsValid => true;

        public override string TypeName => "Node";

        public override string TypeDescription => "Node";

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

        public GH_LinkElement(LinkElementModel model, Guid? guid = null)
        {
            if (model != null)
                Value = new LinkElementModel(model);
            else
                Value = new LinkElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_LinkElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return (IGH_Goo)new LinkElementModel(Value);
        }

        public override string ToString()
        {
            return $"Link element type {Value.LinkProperty.Type} from point \r" +
                $"X: {Value.NodeStart.X}, Y: {Value.NodeStart.Y}, Z: {Value.NodeStart.Z} to point" +
                $"X: {Value.NodeEnd.X}, Y: {Value.NodeEnd.Y}, Z: {Value.NodeEnd.Z}";
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            Value.DrawWireframe(args.Pipeline, args.Viewport, args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            //throw new NotImplementedException();
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return (IGH_GeometricGoo)new LinkElementModel(Value);
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotImplementedException();
        }
    }
}
