using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fem2Rhino.Common
{
	public abstract class Model
	{
		protected const double _gravityms2 = 9.80665;

		protected Joint[] _joints;
		protected Beam[] _frames;
		protected Plate[] _areas;
		protected List<string> _log = new List<string>();

		protected readonly string _filePath;

		public string FilePath => _filePath;
		public Options Option { get; set; }

		public Joint[] Joints => _joints;
		public Beam[] Frames => _frames;
		public Plate[] Areas => _areas;
		public bool IsValid => Joints.Length != 0 || Frames.Length != 0 || Areas.Length != 0;

		public Model(string filePath = "")
		{
			_filePath = filePath;
			_joints = new Joint[0];
			_frames = new Beam[0];
			_areas = new Plate[0];
			_log = new List<string>();
		}

		protected abstract void InizializeAPI();

		protected abstract void ReadModel();

		protected abstract void ReadJoint();

		protected abstract void ReadFrame();

		protected abstract void ReadArea();

		protected abstract void CloseModel();

		public abstract void Process();

		public abstract void BakeGeometryCustom(RhinoDoc doc);

		public abstract bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid);

		public List<string> GetLog()
		{
			return _log;
		}

		public void DrawPointsAndLines(DisplayPipeline displayPipeline, RhinoViewport viewport, Color color)
		{
			for (int i = 0; i < Joints.Length; i++)
			{
				Joints[i].Draw(displayPipeline, viewport, color);
			}
			for (int i = 0; i < Frames.Length; i++)
			{
				Frames[i].Draw(displayPipeline, viewport, color);
			}
		}

		public void DrawAreas(DisplayPipeline displayPipeline, RhinoViewport viewport, Color color)
		{
			for (int i = 0; i < Areas.Length; i++)
			{
				Areas[i].Draw(displayPipeline, viewport, color);
			}
		}

		public class Options
		{
			public bool ExportLoads { get; set; } = false;
			public bool ExportGeometries { get; set; } = false;
			public bool ExportGeometryDatas { get; set; } = false;
			public bool ExportSectionProperties { get; set; } = false;
		}
	}
}
