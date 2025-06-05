using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fem2Rhino.Common
{
	public class Beam : LineCurve
	{
		private readonly BoundingBox _boundingBox;

		public BoundingBox BoundingBox => _boundingBox;

		public string Id { get; set; }
		public int Number { get; set; }
		public Node EndNode { get; set; }
		public Node StartNode { get; set; }
		public Guid Guid { get; set; }
		public double Angle { get; set; }
		public List<Group> Groups { get; set; }

		public Beam(Node nodeA, Node nodeB, string id)
			: this(nodeA, nodeB, id, Guid.NewGuid())
		{
			Id = id;
		}

		public Beam(Node nodeA, Node nodeB, string id, Guid guid)
			: base(nodeA.Location, nodeB.Location)
		{
			Id = id;
			Guid = guid;
			StartNode = nodeA;
			EndNode= nodeB;

			_boundingBox = Line.BoundingBox;
		}

		public void Draw(DisplayPipeline displayPipeline, RhinoViewport viewport, System.Drawing.Color color)
		{
			displayPipeline.DrawLine(Line, color);
		}
	}
}
