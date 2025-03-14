using Rhino.Display;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.ElementProperties;
using System.Drawing;

namespace Rhino2Midas.Core.Elements
{
    public class LinkElementModel : ElementModel
    {
        public NodeElementModel NodeStart { get; set; }

        public NodeElementModel NodeEnd { get; set; }

        public LinkPropertyModel LinkProperty { get; set; }

        public LinkElementModel(NodeElementModel nodeStart, NodeElementModel nodeEnd, LinkPropertyModel linkProperty)
        {
            NodeStart = nodeStart;
            NodeEnd = nodeEnd;
            LinkProperty = linkProperty;
        }

        public LinkElementModel()
            : base()
        {
        }

        public LinkElementModel(LinkElementModel linkElementModel)
            : base()
        {
            NodeStart = linkElementModel.NodeStart;
            NodeEnd = linkElementModel.NodeEnd;
            LinkProperty = linkElementModel.LinkProperty;
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawLine(new Rhino.Geometry.Line(NodeStart.Position, NodeEnd.Position), color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {

        }
    }
}
