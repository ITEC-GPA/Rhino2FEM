using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.DocObjects.Tables;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fem2Rhino.CSI;
using Fem2Rhino.CSI.ModelWrapper;
using Fem2Rhino.CSI.ApiWrapper;
using Fem2Rhino.Common;

namespace FeMM.Grasshopper.DataTypes.F2R
{
	public class GH_SAPInteractiveDatabaseModel : GH_GeometricGoo<SapInteractiveDatabaseModel>, IGH_BakeAwareData, IGH_PreviewData
	{
		#region CONSTRUCTOR

		public GH_SAPInteractiveDatabaseModel()
		{
			Value = new SapInteractiveDatabaseModel();
		}

		public GH_SAPInteractiveDatabaseModel(GH_SAPInteractiveDatabaseModel modelType) //copy constructor
		{
			Value = modelType.Value;
		}

		public GH_SAPInteractiveDatabaseModel(SapInteractiveDatabaseModel modelTypeWrapper)
		{
			Value = modelTypeWrapper;
		}

		#endregion CONSTRUCTOR

		#region FORMATTERS

		public override bool IsValid => Value != null;

		public override string IsValidWhyNot
		{
			get
			{
				if (Value == null)
					return "No internal ModelTypeWrapper instance";

				return "Invalid ModelTypeWrapper instance";
			}
		}

		public override string TypeName => "Sap2000 Model";

		public override string TypeDescription => "a Sap2000 Model";

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
			doc.Views.RedrawEnabled = false;

			Value.BakeGeometryCustom(doc);

			doc.Views.RedrawEnabled = true;
			doc.Views.Redraw();
		}

		public void OpenInteractiveTables()
		{
			Value.OpenInteractiveTables();
		}

		public List<string> GetLog()
		{
			return Value.GetLog();
		}

		public override IGH_GeometricGoo DuplicateGeometry()
		{
			return new GH_SAPInteractiveDatabaseModel(Value == null ? null : new SapInteractiveDatabaseModel());
		}

		#endregion
	}
}
