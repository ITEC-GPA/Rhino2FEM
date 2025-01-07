using System.Collections.Generic;
using System.Drawing;
using Rhino.Display;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Loads;

namespace Rhino2Midas.Core.Elements
{
    public class FrameElementModel
    {
        public NodeElementModel NodeStart { get; set; }

        public NodeElementModel NodeEnd { get; set; }

        public FramePropertyModel FrameProperty { get; set; }

        public MaterialModel Material { get; set; }

        public List<FrameLoadModel> FrameLoadList { get; set; }

        public double Angle { get; set; }

        public List<ElementGroupModel> Groups { get; set; }

        public int IndexFrameElement { get; set; }

        public int IndexFrameProperty { get; set; }

        public int IndexMaterial { get; set; }

        public int IndexNodeStart { get; set; }

        public int IndexNodeEnd { get; set; }

        public FrameElementModel(NodeElementModel startNode, NodeElementModel endNode, FramePropertyModel frameProperty, MaterialModel material, double angle = 0, List<ElementGroupModel> group = null, List<FrameLoadModel> frameLoadList = null)
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
        }

        public FrameElementModel()
        {
        }

        public FrameElementModel(FrameElementModel frameElementModel)
        {
            NodeStart = frameElementModel.NodeStart;
            NodeEnd = frameElementModel.NodeEnd;
            FrameProperty = frameElementModel.FrameProperty;
            Material = frameElementModel.Material;
            Angle = frameElementModel.Angle;
            Groups = frameElementModel.Groups;
            FrameLoadList = frameElementModel.FrameLoadList;
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
