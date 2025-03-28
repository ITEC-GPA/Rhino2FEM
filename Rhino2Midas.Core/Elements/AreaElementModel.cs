using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public double Offset { get; set; }

        public Brep Brep { get; set; }

        public AreaElementModel(List<NodeElementModel> nodeList, AreaThicknessModel areaThickness, MaterialModel material, double angle = 0, List<ElementGroupModel> group = null, List<AreaLoadModel> areaLoadList = null)
            : base()
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
            BuildBrep();
        }

        public AreaElementModel()
            : base()
        {
        }


        public AreaElementModel(AreaElementModel areaElementModel)
            : base(areaElementModel)
        {
            NodeList = areaElementModel.NodeList;
            Offset = areaElementModel.Offset;
            AreaThickness = areaElementModel.AreaThickness;
            Material = areaElementModel.Material;
            Angle = areaElementModel.Angle;
            Groups = areaElementModel.Groups;
            AreaLoadList = areaElementModel.AreaLoadList;
            Brep = areaElementModel.Brep;
        }

        public void DrawWireframe(DisplayPipeline display, RhinoViewport viewport, Color color)
        {
            if (Brep == null)
                BuildBrep();
            if (Brep != null)
                display.DrawBrepWires(Brep, color);
        }

        public void DrawSolid(DisplayPipeline display, RhinoViewport viewport, Color color)
        {

        }

        private void BuildBrep()
        {
            if(Brep == null)
            {
                if(NodeList.Count > 2)
                {
                    List<Curve> curves = new List<Curve>();
                    for (int i = 0; i < NodeList.Count; i++)
                    {
                        var start = NodeList[i];
                        var end = NodeList[(i + 1) % NodeList.Count]; // Collega l'ultimo punto al primo
                        curves.Add(new LineCurve(start.Position, end.Position));
                    }

                    Brep = Brep.CreatePlanarBreps(curves, 0.001).FirstOrDefault();
                    var plane = new Plane(NodeList[0].Position, NodeList[1].Position, NodeList[2].Position);
                    Brep.Translate(plane.Normal * Offset);
                }
            }
        }
    }
}
