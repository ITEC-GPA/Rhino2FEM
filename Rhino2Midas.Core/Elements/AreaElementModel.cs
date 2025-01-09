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
    public class AreaElementModel : ElementModel
    {
        public List<NodeElementModel> NodeList { get; set; }

        public AreaThicknessModel AreaThickness { get; set; }

        public MaterialModel Material { get; set; }

        public List<AreaLoadModel> AreaLoadList { get; set; }

        public double Angle { get; set; }

        public Brep Brep { get; set; }

        public AreaElementModel(List<NodeElementModel> nodeList, AreaThicknessModel areaThickness, MaterialModel material, double angle = 0, List<ElementGroupModel> group = null, List<AreaLoadModel> areaLoadList = null)
        {
            NodeList = nodeList;
            AreaThickness = areaThickness;
            Material = material;
            Angle = angle;
            if (areaLoadList != null)
                AreaLoadList = areaLoadList;
            else
                AreaLoadList = new List<AreaLoadModel>();
            if (group != null)
                Groups = group;
            else
                Groups = new List<ElementGroupModel>();
        }

        public AreaElementModel()
        {
        }

        public AreaElementModel(AreaElementModel areaElementModel)
        {
            NodeList = areaElementModel.NodeList;
            AreaThickness = areaElementModel.AreaThickness;
            Material = areaElementModel.Material;
            Angle = areaElementModel.Angle;
            Groups = areaElementModel.Groups;
            AreaLoadList = areaElementModel.AreaLoadList;
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            display.DrawBrepWires(Brep, color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {

        }
    }
}
