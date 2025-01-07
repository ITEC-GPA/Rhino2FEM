using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Midas.Core.Elements;

namespace Rhino2Midas.Grasshopper.Datatype
{
    public class GH_AreaElement : GH_GeometricGoo<AreaElementModel>, IGH_PreviewData
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
                {
                    var bb = BoundingBox.Empty;
                    for (int i = 0; i < Value.NodeList.Count; i++)
                        bb.Union(new BoundingBox(Value.NodeList[i].Position, Value.NodeList[i].Position));
                    return bb;
                }
            }
        }

        public GH_AreaElement(AreaElementModel nodeModel, Guid? guid = null)
        {
            if (nodeModel != null)
                Value = new AreaElementModel(nodeModel);
            else
                Value = new AreaElementModel();

            if (guid != null && guid.HasValue)
                Guid = guid.Value;
        }

        public GH_AreaElement()
        {
        }

        public override IGH_Goo Duplicate()
        {
            return (IGH_Goo)new AreaElementModel(Value);
        }

        public override string ToString()
        {
            return $"Area element";
        }

        void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
        {
            Value.DrawWireframe(args.Pipeline, args.Viewport, args.Color);
        }

        void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return (IGH_GeometricGoo)new AreaElementModel(Value);
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
