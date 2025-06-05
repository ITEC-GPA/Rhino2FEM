using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.Common
{
	public class Group
	{
		public string Name { get; set; }
		public Color Color { get; set; }

		public Group()
		{
		}

		public Group(string name)
		{
			Name = name;
		}
		public Group(string name, Color color)
		{
			Name = name;
			Color = color;
		}
	}
}
