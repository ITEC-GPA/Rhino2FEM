using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.Loads;

namespace Rhino2Midas.Core.Elements
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
    }
}
