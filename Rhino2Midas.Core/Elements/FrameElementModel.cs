using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Loads;

namespace Rhino2Midas.Core.Elements
{
    public class FrameElementModel : ElementModel
    {
        public NodeElementModel NodeStart { get; set; }

        public NodeElementModel NodeEnd { get; set; }

        public FramePropertyModel FrameProperty { get; set; }

        public MaterialModel Material { get; set; }

        public List<FrameLoadModel> FrameLoadList { get; set; }

        public double Angle { get; set; }

        public List<Brep> Breps { get; set; }

        public bool IsVertical
        {
            get
            {
                Vector3d v = NodeEnd.Position - NodeStart.Position;
                v.Unitize();
                return Vector3d.CrossProduct(Vector3d.ZAxis, v).Length < 1.0E-12;
            }
        }

        public FrameElementModel(NodeElementModel startNode, NodeElementModel endNode, FramePropertyModel frameProperty, MaterialModel material, double angle = 0, List<ElementGroupModel> group = null, List<FrameLoadModel> frameLoadList = null)
            : base()
        {
            NodeStart = startNode;
            NodeEnd = endNode;
            FrameProperty = frameProperty;
            Material = material;
            if (frameLoadList != null)
                FrameLoadList = frameLoadList;
            else
                FrameLoadList = new List<FrameLoadModel>();
            Angle = angle;
            if (group != null)
                Groups = group;
            else
                Groups = new List<ElementGroupModel>();
            Breps = new List<Brep>();
            BuildBreps();
        }

        public FrameElementModel()
            : base()
        {
            FrameLoadList = new List<FrameLoadModel>();
            Groups = new List<ElementGroupModel>();
            Breps = new List<Brep>();
        }

        public FrameElementModel(FrameElementModel frameElementModel)
            : base(frameElementModel)
        {
            NodeStart = frameElementModel.NodeStart;
            NodeEnd = frameElementModel.NodeEnd;
            FrameProperty = frameElementModel.FrameProperty;
            Material = frameElementModel.Material;
            Angle = frameElementModel.Angle;
            Groups = frameElementModel.Groups;
            FrameLoadList = frameElementModel.FrameLoadList;
            Breps = frameElementModel.Breps;
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawLine(new Line(NodeStart.Position, NodeEnd.Position), color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            if (Breps != null)
            {
                foreach (Brep shape in Breps)
                    display.DrawBrepWires(shape, color);
            }
        }

        private void BuildBreps()
        {
            if (Breps == null)
                Breps = new List<Brep>();

            if (FrameProperty != null && FrameProperty.Breps != null && FrameProperty.Breps.Count > 0)
            {
                GetLocalAxes(false, out Vector3d x, out Vector3d y, out Vector3d z, out Transform t);
                for (int j = 0; j < FrameProperty.Breps.Count; j++)
                {
                    Brep surface = FrameProperty.Breps[j].DuplicateBrep();
                    surface.Transform(t);

                    LineCurve line = new LineCurve(NodeStart.Position, NodeEnd.Position);
                    Brep shape = surface.Faces[0].CreateExtrusion(line, true);
                    Breps.Add(shape);
                }
            }
        }

        public void GetLocalAxes(bool getPrincipal, out Vector3d axes3, out Vector3d axes2, out Vector3d axes1, out Transform toGlobal)
        {
            double r = Angle;
            if (!getPrincipal)
            {
                r -= FrameProperty.Angle;
            }
            axes3 = NodeEnd.Position - NodeStart.Position;
            axes3.Unitize();
            if (!IsVertical) // Not vertical
            {
                Vector3d v_xy = new Vector3d(axes3.X, axes3.Y, 0);
                v_xy.Unitize();

                axes1 = Vector3d.CrossProduct(Vector3d.ZAxis, axes3);
                axes1.Unitize();
                axes2 = Vector3d.CrossProduct(axes3, axes1);
                axes2.Unitize();
            }
            else // Vertical
            {
                axes2 = Vector3d.CrossProduct(axes3, Vector3d.YAxis);
                axes2.Unitize();
                axes1 = Vector3d.CrossProduct(axes2, axes3);
                axes1.Unitize();
            }
            Plane pl = new Plane(NodeStart.Position, axes1, axes2);
            pl.Rotate(r, pl.Normal);
            axes1 = pl.XAxis;
            axes2 = pl.YAxis;
            toGlobal = Transform.PlaneToPlane(Plane.WorldXY, pl);
        }
    }
}
