using Rhino.Display;
using Rhino.Geometry;
using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.Loads;
using Rhino2Fem.Core.Models;
using System.Collections.Generic;
using System.Drawing;

namespace Rhino2Fem.Core.Elements
{
    public class NodeElementModel : ElementModel
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
        public List<NodalLoadModel> NodalLoadList { get; set; }

        public NodeSupportModel Support { get; set; }

        public Point3d Position => new Point3d(X, Y, Z);

        public NodeElementModel(Point3d position, List<ElementGroupModel> group = null, NodeSupportModel support = null, List<NodalLoadModel> nodalLoadList = null)
            : this(position.X, position.Y, position.Z, group, support, nodalLoadList)
        {
        }

        public NodeElementModel(double x, double y, double z, List<ElementGroupModel> group = null, NodeSupportModel support = null, List<NodalLoadModel> nodalLoadList = null)
            : base()
        {
            X = x;
            Y = y;
            Z = z;
            if (nodalLoadList != null)
                NodalLoadList = nodalLoadList;
            else
                NodalLoadList = new List<NodalLoadModel>();

            if (group != null)
                Groups = group;
            else
                Groups = new List<ElementGroupModel>();

            if (support != null)
                Support = support;
            else
                Support = new NodeSupportModel();
        }

        public NodeElementModel(NodeElementModel nodeModel)
            : base(nodeModel)
        {
            X = nodeModel.X;
            Y = nodeModel.Y;
            Z = nodeModel.Z;
            NodalLoadList = nodeModel.NodalLoadList;
            Groups = nodeModel.Groups;
            Support = nodeModel.Support;
        }

        public NodeElementModel()
            :base()
        {
            NodalLoadList = new List<NodalLoadModel>();            
            Support = new NodeSupportModel();
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawPoint(Position, color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {

        }

        public static int Comparer(NodeElementModel a, NodeElementModel b)
        {
            var rx = a.Position.X.CompareTo(b.Position.X);
            if (rx == 0)
            {
                var ry = a.Position.Y.CompareTo(b.Position.Y);

                if (ry == 0)
                    return a.Position.Z.CompareTo(b.Position.Z);
                else
                    return ry;
            }
            return rx;
        }
    }
}
