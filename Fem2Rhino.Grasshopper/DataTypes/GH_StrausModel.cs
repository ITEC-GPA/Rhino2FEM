using Fem2Rhino.Straus7;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;

namespace FeMM.Grasshopper.DataTypes.F2R
{
    public class GH_StrausModel : GH_GeometricGoo<StrausModel>, IGH_BakeAwareData, IGH_PreviewData
    {
        #region CONSTRUCTOR

        public GH_StrausModel()
        {

        }

        public GH_StrausModel(int units = 10, string filePath = "", bool visible = true)
        {
            Value = new StrausModel(units, filePath, visible);
        }

        public GH_StrausModel(GH_StrausModel modelType) //copy constructor
        {
            Value = modelType.Value;
        }

        public GH_StrausModel(StrausModel modelTypeWrapper)
        {
            Value = modelTypeWrapper;
        }

        #endregion CONSTRUCTOR

        #region FORMATTERS

        public override bool IsValid => Value != null && Value.IsValid;

        public override string IsValidWhyNot
        {
            get
            {
                if (Value == null)
                    return "No internal ModelTypeWrapper instance";
                if (Value.IsValid)
                    return string.Empty;

                return "Invalid ModelTypeWrapper instance";
            }
        }

        public override string TypeName => "Straus7 Model";

        public override string TypeDescription => "a Straus7 Model";

        public override BoundingBox Boundingbox
        {
            get
            {
                if (Value == null)
                    return BoundingBox.Empty;
                return ComputeBoundingBox(new Transform(0));
            }
        }

        public BoundingBox ClippingBox => Boundingbox;

        private BoundingBox ComputeBoundingBox(Transform xform)
        {
            BoundingBox boundingBox = new BoundingBox();

            for (int i = 0; i < Value.Joints.Length; i++)
            {
                boundingBox.Union(Value.Joints[i].Location);
            }
            for (int i = 0; i < Value.Frames.Length; i++)
            {
                boundingBox.Union(Value.Frames[i].BoundingBox);
            }

            for (int i = 0; i < Value.Areas.Length; i++)
            {
                boundingBox.Union(Value.Areas[i].BoundingBox);
            }

            if (xform.IsZero)
                return boundingBox;
            else
            {
                boundingBox.Transform(xform);
                return boundingBox;
            }
        }

        public override string ToString()
        {
            if (Value == null)
                return "Null Beam";
            else
                return Value.ToString();
        }

        #endregion FORMATTERS

        #region SERIALIZATION

        // Serialize this instance to a Grasshopper writer object.
        //public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        //{
        //    writer.SetInt32("tri", this.Value);
        //    return true;
        //}

        //// Deserialize this instance from a Grasshopper reader object.
        //public override bool Read(GH_IO.Serialization.GH_IReader reader)
        //{
        //    this.Value = reader.GetInt32("tri");
        //    return true;
        //}

        #endregion SERIALIZATION

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return ComputeBoundingBox(xform);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            throw new NotImplementedException();
        }

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            throw new NotImplementedException();
        }

        #region PREVIEW DATA

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            Value.DrawPointsAndLines(args.Pipeline, args.Viewport, args.Color);
            Value.DrawAreas(args.Pipeline, args.Viewport, args.Color);
        }

        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {

        }

        #endregion PREVIEW DATA

        #region PREVIEW DATA

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            Value.BakeGeometry(doc, att, out obj_guid);
            return true;
        }

        public void BakeGeometryCustom(RhinoDoc doc)
        {
            Value.BakeGeometryCustom(doc);
        }

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            return new GH_StrausModel(Value == null ? null : new StrausModel());
        }

        #endregion
    }
}
