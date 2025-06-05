using Rhino.Display;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class Plate
	{
		private readonly Node[] _nodes;
		private readonly GeometryBase _shape;
		private readonly BoundingBox _boundingBox;

		public string Id { get; set; }
		public int Number { get; set; }
		public Guid Guid { get; set; }
		public List<Group> Groups { get; set; }
		public PlateProperty Property { get; set; }
		public virtual Node[] Nodes => _nodes;
		public BoundingBox BoundingBox => _boundingBox;
		public GeometryBase Shape => _shape;
		public double Angle { get; set; }


		public Plate()
		{

		}

		public Plate(string id, Guid guid)
		{
			Id = id;
			Guid = guid;
		}

		public Plate(string id, Guid guid, Node[] nodes)
		{
			Id = id;
			Guid = guid;
			_nodes = nodes;
			Groups = new List<Group>();

			GeometryBase surface;
			if (nodes.Length == 3)
			{
                surface = NurbsSurface.CreateFromCorners(nodes[0].Location, nodes[1].Location, nodes[2].Location).ToBrep();
			}
			else if (nodes.Length == 4)
			{
                surface = NurbsSurface.CreateFromCorners(nodes[0].Location, nodes[1].Location, nodes[2].Location, nodes[3].Location).ToBrep();
			}
			else
			{   //sistemare
				List<LineCurve> lineCurves = new List<LineCurve>();
				for (int i = 0; i < nodes.Length; i++)
				{
					if (i != nodes.Length - 1)
						lineCurves.Add(new LineCurve(nodes[i].Location, nodes[i + 1].Location));
					else
						lineCurves.Add(new LineCurve(nodes[i].Location, nodes[0].Location));
				}

				//Polyline ppp = new Polyline(nodes.Select(i => i.Location));
				//ppp.Add(ppp[0]);
				//var PatchBrep = Brep.CreatePatch(lineCurves, 3, 3, 0);
				//surface = PatchBrep.Surfaces[0];
				surface = Brep.CreatePlanarBreps(lineCurves, 1).FirstOrDefault();
			}

			_shape = surface;
			_boundingBox = _shape.GetBoundingBox(false);
		}

		public Plate(string id, Guid guid, Node[] nodes, PlateProperty plateProperty)
		{
			Id = id;
			Guid = guid;
			_nodes = nodes;
			Property = plateProperty;
		}

		public string GetNodeIds()
		{
			string outString = string.Empty;
			for (int i = 0; i < _nodes.Length; i++)
			{
				if (i < _nodes.Length - 1)
					outString += _nodes[i].Id + ";";
				else
					outString += _nodes[i].Id;
			}
			return outString;
		}

		public void Draw(DisplayPipeline displayPipeline, RhinoViewport viewport, Color color)
		{
			if(Shape is Surface)
				displayPipeline.DrawSurface((Surface)Shape, color, 3);
			else if (Shape is Brep)
				displayPipeline.DrawBrepShaded((Brep)Shape, new DisplayMaterial(color));
		}
	}
}
