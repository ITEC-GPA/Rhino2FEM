using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class Node : Point
	{
		public string Id { get; set; }
		public int Number { get; set; }
		public Guid Guid { get; set; }
		public virtual Group Group { get; set; }

		public Node(Point p)
			: base(new Point3d(p.Location.X, p.Location.Y, p.Location.Z))
		{
		}

		public Node(Node p)
			: base(new Point3d(p.Location.X, p.Location.Y, p.Location.Z))
		{
			if (p.Id != null)
				Id = p.Id;
			if (p.Guid != null)
				Guid = p.Guid;
		}

		public Node(double x, double y, double z)
			: base(new Point3d(x, y, z))
		{
		}

		public Node(double x, double y, double z, string id)
			: base(new Point3d(x, y, z))
		{
			Id = id;
		}

		public Node(double x, double y, double z, string id, Guid guid)
			: base(new Point3d(x, y, z))
		{
			Id = id;
			Guid = guid;
		}

		public void Draw(DisplayPipeline displayPipeline, RhinoViewport viewport, System.Drawing.Color color)
		{
			displayPipeline.DrawPoint(Location, color);
		}
	}
}
