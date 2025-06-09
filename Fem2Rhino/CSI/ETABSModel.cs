using ETABSv1;
using Fem2Rhino.Common;
using Fem2Rhino.CSI.ModelWrapper;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.CSI
{
	public class ETABSModel : Model
	{
		protected readonly ProxyHelper.CSI.ApiWrapper.ETABSApiWrapper _apiWrapper;
		protected Dictionary<string, Joint> _idJointAssociation;

		protected readonly bool _attachToInstance;
		protected readonly string _sapExePath;
		protected readonly bool _visibile;
		protected readonly int _units;

		protected string _sapVersion;
		protected double _sapSubVersion;

		public string SapExePath => _sapExePath;
		public bool AttachToInstance => _attachToInstance;

		public ETABSModel(bool AttachToInstance = false, string SapExePath = "", int units = ProxyHelper.CSI.ApiWrapper.Units.N_mm_C, string filePath = "", bool visible = true)
			: base(filePath)
		{
			_units = units;
			_visibile = visible;
			_attachToInstance = AttachToInstance;
			_sapExePath = SapExePath;
			_idJointAssociation = new Dictionary<string, Joint>();
			_apiWrapper = new ProxyHelper.CSI.ApiWrapper.ETABSApiWrapper();
		}

		public override void Process()
		{
			InizializeAPI();
			ReadModel();
			CloseModel();
		}

		protected override void InizializeAPI()
		{
			_apiWrapper.InizializeAPI(_attachToInstance, _sapExePath, _units, _visibile, _filePath);
			_apiWrapper.SetPresentUnits(_units);
			(_sapVersion, _sapSubVersion) = _apiWrapper.GetVersion();
		}

		protected override void ReadModel()
		{
			ReadJoint();
			ReadFrame();
			ReadArea();
		}

		protected override void ReadJoint()
		{
			int rv = 1;
			int jointNumber = -1;
			string[] jointNames = new string[1];

			rv = _apiWrapper.GetPointNameList(ref jointNumber, ref jointNames);

			if (jointNumber > 0)
			{
				_joints = new Joint[jointNumber];
				for (int j = 0; j < jointNames.Length; j++)
				{
					string jointName = jointNames[j];
					int ret = 1;

					string sapGuid = "";
					ret = _apiWrapper.GetPointGuid(jointName, ref sapGuid);
					Guid.TryParse(sapGuid, out Guid guid);

					double x = new double();
					double y = new double();
					double z = new double();
					ret = _apiWrapper.GetCoordCartesian(jointName, ref x, ref y, ref z);
					Joint joint = new Joint(x, y, z, jointName, guid);

					bool specialPoint = false;
					_apiWrapper.GetPointSpecialJoint(jointName, ref specialPoint);
					_idJointAssociation.Add(jointName, joint);

					joint.SpecialJoint = specialPoint;

					if (Option.ExportGeometryDatas)
					{
						string[] groups = new string[0];
						int numberGroups = 0;
						ret = _apiWrapper.GetPointGroupAssign(jointName, ref numberGroups, ref groups);
						joint.Groups = Enumerable.Range(0, groups.Length).Select(i => new Common.Group(groups[i])).ToList();

						bool[] restraint = new bool[0];
						ret = _apiWrapper.GetPointRestraint(jointName, ref restraint);
						joint.Restraints = restraint;

					}

					#region Point Load

					if (Option.ExportLoads)
					{
						int NumberItems = 0;
						string[] PointName = new string[0];
						string[] LoadPat = new string[0];
						int[] LCStep = new int[0]; string[] CSys = new string[0];
						double[] F1 = new double[0];
						double[] F2 = new double[0];
						double[] F3 = new double[0];
						double[] M1 = new double[0];
						double[] M2 = new double[0];
						double[] M3 = new double[0];
						ret = _apiWrapper.GetPointLoadForce(jointName, ref NumberItems, ref PointName, ref LoadPat, ref LCStep, ref CSys, ref F1, ref F2, ref F3, ref M1, ref M2, ref M3);

						if (NumberItems > 0)
						{
							NodePointLoad[] pointLoads = new NodePointLoad[NumberItems];
							for (int k = 0; k < NumberItems; k++)
							{
								pointLoads[k] = new NodePointLoad(LoadPat[k])
								{
									LoadPattern = LoadPat[k],
									CoordinateSystem = CSys[k],
									F1 = F1[k],
									F2 = F2[k],
									F3 = F3[k],
									M1 = M1[k],
									M2 = M2[k],
									M3 = M3[k],
								};
							}

							joint.PointLoads = pointLoads.ToList();
						}
					}

					#endregion

					_joints[j] = joint;
				}
			}
		}

		protected override void ReadFrame()
		{
			int rv = -1;
			int frameNumber = -1;
			string[] frameNames = new string[1];
			List<string> errorBeams = new List<string>();
			rv = _apiWrapper.GetFrameNameList(ref frameNumber, ref frameNames);

			if (frameNumber > 0)
			{
				_frames = new Beam[frameNumber];
				for (int i = 0; i < frameNames.Length; i++)
				{
					string frameName = frameNames[i];
					int ret = 1;
					string sapGuid = "";
					string point1 = "";
					string point2 = "";

					ret = _apiWrapper.GetFrameGuid(frameName, ref sapGuid);
					ret = _apiWrapper.GetFrameEndPoints(frameName, ref point1, ref point2);

					Guid.TryParse(sapGuid, out Guid guid);
					if (ret == 0)
					{
						Joint joint1 = _idJointAssociation[point1];
						Joint joint2 = _idJointAssociation[point2];

						Frame frame = new Frame(joint1, joint2, frameName, guid);
						_frames[i] = frame;

						#region Section

						if (Option.ExportSectionProperties)
						{
							string section = string.Empty;
							string sectionAuto = string.Empty;
							ret = _apiWrapper.GetFrameSection(frameName, ref section, ref sectionAuto);

							if (section == "None")
							{
								frame.Property = new FrameProperty(true) { Name = "None" };
							}
							else
							{
								string sectionName = string.Empty;
								double sVarTotalLength = 0;
								double sVarRelStartLoc = 0;
								ret = _apiWrapper.GetFrameSectionNonPrismatic(frameName, ref sectionName, ref sVarTotalLength, ref sVarRelStartLoc);

								if (ret == 0)
								{
									frame.Property = new FrameProperty(true, sVarTotalLength, sVarRelStartLoc, false) { Name = section };
								}
								else
								{
									frame.Property = new FrameProperty { Name = section };
								}
							}

							eFrameDesignOrientation orientation = eFrameDesignOrientation.Other;
							ret = _apiWrapper.GetFrameDesignOrientation(frameName, ref orientation);

							if (orientation == eFrameDesignOrientation.Column)
								frame.Property.PropertyType = FrameProperty.PropertyTypes.Column;
							else if (orientation == eFrameDesignOrientation.Beam)
								frame.Property.PropertyType = FrameProperty.PropertyTypes.Beam;
							else if (orientation == eFrameDesignOrientation.Brace)
								frame.Property.PropertyType = FrameProperty.PropertyTypes.Brace;
							else if (orientation == eFrameDesignOrientation.Null)
								frame.Property.PropertyType = FrameProperty.PropertyTypes.Null;
							else
								frame.Property.PropertyType = FrameProperty.PropertyTypes.Other;
						}

						#endregion

						if (Option.ExportGeometryDatas)
						{
							#region Local Axis

							double angle = 0;
							bool advancedLocalAxis = false;
							ret = _apiWrapper.GetFrameLocalAxis(frameName, ref angle, ref advancedLocalAxis);
							frame.Angle = angle;
							frame.AdvancedLocalAxis = advancedLocalAxis;
							frame.AdvancedLocalAngle = 0;

							#endregion

							#region Groups

							int numberGroup = -1;
							string[] groups = new string[1];

							ret = _apiWrapper.GetFrameGroupAssign(frameName, ref numberGroup, ref groups);
							if (ret != 0)
								_log.Add($"Warning: Groups on frame {frameName} are not set correctly");

							frame.Groups = Enumerable.Range(0, groups.Length).Select(j => new Common.Group(groups[j])).ToList();

							#endregion

							#region Offset

							int CardinalPoint = -1;
							bool Mirror2 = false;
							bool Mirror3 = false;
							bool StiffTransform = false;
							double[] Offset1 = new double[0];
							double[] Offset2 = new double[0];
							string CSys = string.Empty;
							ret = _apiWrapper.GetFrameInsertionPoint(frameName, ref CardinalPoint, ref Mirror2, ref StiffTransform, ref Offset1, ref Offset2, ref CSys);

							if (ret != 0)
								_log.Add($"Warning: Insertion point on frame {frameName} are not set correctly");

							Frame.InsertionPoints insertionPoint = new Frame.InsertionPoints()
							{
								CardinalPoint = (Frame.InsertionPoints.CardinalPoints)CardinalPoint,
								CoordinateSystem = CSys,
								StiffnessTransform = StiffTransform,
								Mirror2 = Mirror2,
								Mirror3 = Mirror3,
								Offset1 = Offset1,
								Offset2 = Offset2,
							};
							frame.InsertionPoint = insertionPoint;

							#endregion

							#region Releases

							bool[] ii = new bool[0];
							bool[] jj = new bool[0];
							double[] StartValue = new double[0];
							double[] EndValue = new double[0];
							ret = _apiWrapper.GetFrameReleases(frameName, ref ii, ref jj, ref StartValue, ref EndValue);

							if (ret != 0)
								_log.Add($"Warning: Release on frame {frameName} are not set correctly");


							Frame.Releases release = new Frame.Releases()
							{
								IEndReleases = ii,
								JEndReleases = jj,
								IEndPartialFixity = StartValue,
								JEndPartialFixity = EndValue,
							};

							frame.Release = release;

							#endregion

							#region Transfer

							bool transfer = false;
							frame.LoadTransfer = transfer;

							#endregion

							#region AutoMesh

							bool activeAM = false;
							bool AutoMeshAtPoints = false;
							bool AutoMeshAtLines = false;
							int MinimumNumberOfSegments = 0;
							double AutoMeshMaxLength = 0;

							Frame.AutoMeshAttribute autoMeshAttribute = new Frame.AutoMeshAttribute()
							{
								Active = activeAM,
								AutoMeshAtPoints = AutoMeshAtPoints,
								AutoMeshAtLines = AutoMeshAtLines,
								AutoMeshMaxLength = AutoMeshMaxLength,
								MinimumNumberOfSegments = MinimumNumberOfSegments,
							};

							frame.AutoMesh = autoMeshAttribute;

							#endregion

							#region Modifiers

							double[] frameMods = new double[] { };
							ret = _apiWrapper.GetFrameModifiers(frameName, ref frameMods);
							if (ret != 0)
								_log.Add($"Warning: Frame modifiers on frame {frameName} are not set correctly");

							Frame.BeamModifiers beamPropMod = new Frame.BeamModifiers()
							{
								CrossSectionalArea = frameMods[0],
								ShearAreaInLocal2Direction = frameMods[1],
								ShearAreaInLocal3Direction = frameMods[2],
								TorsionalConstant = frameMods[3],
								MomentOfInertiaAboutLocal2Axis = frameMods[4],
								MomentOfInertiaAboutLocal3Axis = frameMods[5],
								Mass = frameMods[6],
								Weight = frameMods[7],
							};

							frame.BeamModifier = beamPropMod;

							string section = string.Empty;
							string sectionAuto = string.Empty;
							ret = _apiWrapper.GetFrameSection(frameName, ref section, ref sectionAuto);

							double[] framePropMods = new double[] { };
							ret = _apiWrapper.GetFramePropertyModifiers(section, ref framePropMods);
							if (ret != 0)
								_log.Add($"Warning: Frames section modifiers on frame {frameName} are not set correctly");

							Frame.BeamPropertyModifiers beamModifiers = new Frame.BeamPropertyModifiers()
							{
								CrossSectionalArea = framePropMods[0],
								ShearAreaInLocal2Direction = framePropMods[1],
								ShearAreaInLocal3Direction = framePropMods[2],
								TorsionalConstant = framePropMods[3],
								MomentOfInertiaAboutLocal2Axis = framePropMods[4],
								MomentOfInertiaAboutLocal3Axis = framePropMods[5],
								Mass = framePropMods[6],
								Weight = framePropMods[7],
							};

							frame.BeamPropertyModifier = beamModifiers;

							#endregion
						}

						if (Option.ExportLoads)
						{
							#region Distributed Load

							int numberLoadDistributedItems = 0;
							string[] FrameNameLoadDistributed = new string[0];
							string[] LoadPatLoadDistributed = new string[0];
							int[] MyTypeLoadDistributed = new int[0];
							string[] CSysLoadDistributed = new string[0];
							int[] DirLoadDistributed = new int[0];
							double[] RD1LoadDistributed = new double[0];
							double[] RD2LoadDistributed = new double[0];
							double[] Dist1LoadDistributed = new double[0];
							double[] Dist2LoadDistributed = new double[0];
							double[] Val1LoadDistributed = new double[0];
							double[] Val2LoadDistributed = new double[0];
							ret = _apiWrapper.GetFrameLoadDistributed(frameName, ref numberLoadDistributedItems, ref FrameNameLoadDistributed, ref LoadPatLoadDistributed,
								ref MyTypeLoadDistributed, ref CSysLoadDistributed, ref DirLoadDistributed, ref RD1LoadDistributed, ref RD2LoadDistributed,
								ref Dist1LoadDistributed, ref Dist2LoadDistributed, ref Val1LoadDistributed, ref Val2LoadDistributed);

							if (numberLoadDistributedItems > 0)
							{
								FrameDistributedLoad[] loadDistributeds = new FrameDistributedLoad[numberLoadDistributedItems];
								for (int k = 0; k < numberLoadDistributedItems; k++)
								{
									loadDistributeds[k] = new FrameDistributedLoad(LoadPatLoadDistributed[k])
									{
										Direction = (FrameLoad.Directions)DirLoadDistributed[k],
										LoadPattern = LoadPatLoadDistributed[k],
										Type = (FrameLoad.Types)MyTypeLoadDistributed[k],
										DistanceIEnd = Dist1LoadDistributed[k],
										DistanceJEnd = Dist2LoadDistributed[k],
										RelativeDistanceIEnd = RD1LoadDistributed[k],
										RelativeDistanceJEnd = RD2LoadDistributed[k],
										ValueStart = Val1LoadDistributed[k],
										ValueEnd = Val2LoadDistributed[k],
										CoordinateSystem = CSysLoadDistributed[k],
									};
								}

								frame.LoadDistributeds = loadDistributeds.ToList();
							}
							#endregion

							#region Point Load

							int numberPointLoadItems = 0;
							string[] FrameNamePointLoad = new string[0];
							string[] LoadPatPointLoad = new string[0];
							int[] MyTypePointLoad = new int[0];
							string[] CSysPointLoad = new string[0];
							int[] DirPointLoad = new int[0];
							double[] RelDistPointLoad = new double[0];
							double[] DistPointLoad = new double[0];
							double[] ValPointLoad = new double[0];
							ret = _apiWrapper.GetFrameLoadPoint(frameName, ref numberPointLoadItems, ref FrameNamePointLoad, ref LoadPatPointLoad, ref MyTypePointLoad, ref CSysPointLoad,
								ref DirPointLoad, ref RelDistPointLoad, ref DistPointLoad, ref ValPointLoad);

							if (numberPointLoadItems > 0)
							{
								FramePointLoad[] pointLoads = new FramePointLoad[numberPointLoadItems];
								for (int k = 0; k < numberPointLoadItems; k++)
								{
									pointLoads[k] = new FramePointLoad(LoadPatPointLoad[k])
									{
										Direction = (FrameLoad.Directions)DirPointLoad[k],
										LoadPattern = LoadPatPointLoad[k],
										Type = (FrameLoad.Types)MyTypePointLoad[k],
										DistanceIEnd = DistPointLoad[k],
										RelativeDistanceIEnd = RelDistPointLoad[k],
										Value = ValPointLoad[k],
										CoordinateSystem = CSysPointLoad[k],
									};
								}

								frame.PointLoads = pointLoads.ToList();
							}

							#endregion
						}
					}
				}
			}
		}

		protected override void ReadArea()
		{
			int rv = -1;
			int areaNumber = -1;
			string[] areaNames = new string[1];
			rv = _apiWrapper.GetAreaNameList(ref areaNumber, ref areaNames);

			if (areaNumber > 0)
			{
				_areas = new Plate[areaNumber];

				for (int k = 0; k < areaNames.Length; k++)
				{
					string areaName = areaNames[k];
					int ret = 1;
					string sapGuid = "";
					int pointNumber = -1;
					string[] pointNames = new string[1];

					ret = _apiWrapper.GetAreaGuid(areaName, ref sapGuid);
					ret = _apiWrapper.GetAreaPoints(areaName, ref pointNumber, ref pointNames);

					Guid.TryParse(sapGuid, out Guid guid);
					if (ret == 0)
					{
						Joint[] joints = new Joint[pointNumber];
						for (int i = 0; i < pointNumber; i++)
							joints[i] = _idJointAssociation[pointNames[i]];

						Area area = new Area(areaName, guid, joints);
						_areas[k] = area;

						if (Option.ExportSectionProperties)
						{
							string property = string.Empty;
							ret = _apiWrapper.GetAreaProperty(areaName, ref property);

							if (property == "None")
							{
								area.Property = new AreaProperty(true) { Name = "None" };
							}
							else
							{
								area.Property = new AreaProperty { Name = property };
							}

							bool isOpening = false;
							ret = _apiWrapper.GetAreaOpening(areaName, ref isOpening);
							area.Property.IsOpening = isOpening;

							eAreaDesignOrientation orientation = eAreaDesignOrientation.Other;
							ret = _apiWrapper.GetAreaDesignOrientation(areaName, ref orientation);

							if (orientation == eAreaDesignOrientation.Wall)
								area.Property.PropertyType = PlateProperty.PropertyTypes.Wall;
							else if (orientation == eAreaDesignOrientation.Floor)
								area.Property.PropertyType = PlateProperty.PropertyTypes.Floor;
							else if (orientation == eAreaDesignOrientation.Null)
								area.Property.PropertyType = PlateProperty.PropertyTypes.Null;
							else 
								area.Property.PropertyType = PlateProperty.PropertyTypes.Other;
						}

						if (Option.ExportGeometryDatas)
						{
							#region	Groups

							int numberGroup = -1;
							string[] stringGroup = new string[1];
							ret = _apiWrapper.GetAreaGroupAssign(areaName, ref numberGroup, ref stringGroup);
							if (ret != 0)
								_log.Add($"Warning: Groups on area {areaName} are not set correctly");

							#endregion

							#region AreaModifiers

							List<Common.Group> _group = Enumerable.Range(0, stringGroup.Length).Select(i => new Common.Group(stringGroup[i])).ToList();
							area.Groups = _group;

							double[] plateMods = new double[] { };
							ret = _apiWrapper.GetAreaModifiers(areaName, ref plateMods);
							if (ret != 0)
								_log.Add($"Warning: Area modifiers on area {areaName} are not set correctly");

							Area.PlateModifiers plateModifier = new Area.PlateModifiers()
							{
								MembraneF11 = plateMods[0],
								MembraneF22 = plateMods[1],
								MembraneF12 = plateMods[2],
								BendingM11 = plateMods[3],
								BendingM22 = plateMods[4],
								BendingM12 = plateMods[5],
								ShearV13 = plateMods[6],
								ShearV23 = plateMods[7],
								Mass = plateMods[8],
								Weight = plateMods[9],
							};

							area.PlateModifier = plateModifier;

							#endregion

							#region Area Property Modifiers

							string property = string.Empty;
							ret = _apiWrapper.GetAreaProperty(areaName, ref property);

							double[] platePropMods = new double[] { };
							ret = _apiWrapper.GetAreaPropertyModifiers(property, ref platePropMods);
							if (ret != 0)
								_log.Add($"Warning: Area property modifiers on area {areaName} are not set correctly");

							Area.PlatePropertyModifiers platePropModifier = new Area.PlatePropertyModifiers()
							{
								MembraneF11 = platePropMods[0],
								MembraneF22 = platePropMods[1],
								MembraneF12 = platePropMods[2],
								BendingM11 = platePropMods[3],
								BendingM22 = platePropMods[4],
								BendingM12 = platePropMods[5],
								ShearV13 = platePropMods[6],
								ShearV23 = platePropMods[7],
								Mass = platePropMods[8],
								Weight = platePropMods[9],
							};

							area.PlatePropertyModifier = platePropModifier;

							#endregion

							#region Area Local Axes

							double angle = 0;
							bool advancedLocalAxis = false;
							ret = _apiWrapper.GetAreaLocalAxes(areaName, ref angle, ref advancedLocalAxis);

							area.Angle = angle;
							area.AdvancedLocalAxis = advancedLocalAxis;
							if (advancedLocalAxis)
							{
								area.AdvancedLocalAngle = 0;
								Area.AdvanceLocalAxes advanceLocalAxes = new Area.AdvanceLocalAxes()
								{
									IsActive = false,
									Plane = 31,
									Joint1 = "None",
									Joint2 = "None",
								};
								area.AdvanceLocalCoordinateSystem = advanceLocalAxes;
							}
							else
							{
								area.AdvancedLocalAngle = 0;
								Area.AdvanceLocalAxes advanceLocalAxes = new Area.AdvanceLocalAxes()
								{
									IsActive = false,
									Plane = 31,
									Joint1 = "None",
									Joint2 = "None",
								};
								area.AdvanceLocalCoordinateSystem = advanceLocalAxes;
							}

							#endregion
						}

						if (Option.ExportLoads)
						{
							#region Load Uniform 

							int NumberItemsUniform = 0;
							string[] AreaNameUniform = new string[0];
							string[] LoadPatUniform = new string[0];
							string[] CSysUniform = new string[0];
							int[] DirUniform = new int[0];
							double[] ValueUniform = new double[0];
							ret = _apiWrapper.GetAreaLoadUniform(areaName, ref NumberItemsUniform, ref AreaNameUniform, ref LoadPatUniform, ref CSysUniform, ref DirUniform, ref ValueUniform);

							if (NumberItemsUniform > 0)
							{
								AreaLoadUniform[] areaLoadUniforms = new AreaLoadUniform[NumberItemsUniform];

								for (int j = 0; j < NumberItemsUniform; j++)
								{
									AreaLoadUniform areaLoadUniform = new AreaLoadUniform(LoadPatUniform[j])
									{
										Name = LoadPatUniform[j],
										CoordinateSystem = CSysUniform[j],
										Direction = (AreaLoad.Directions)DirUniform[j],
										Value = ValueUniform[j],
										LoadPattern = LoadPatUniform[j],
									};

									areaLoadUniforms[j] = areaLoadUniform;
								}

								area.LoadUniform = areaLoadUniforms.ToList();
							}

							#endregion

							#region Load Uniform to Frame

							int NumberItemsUniformToFrame = 0;
							string[] AreaNameUniformToFrame = new string[0];
							string[] LoadPatUniformToFrame = new string[0];
							string[] CSysUniformToFrame = new string[0];
							int[] DirUniformToFrame = new int[0];
							double[] ValueUniformToFrame = new double[0];
							int[] DistTypeUniformToFrame = new int[0];
							ret = _apiWrapper.GetAreaLoadUniformToFrame(areaName, ref NumberItemsUniformToFrame, ref AreaNameUniformToFrame, ref LoadPatUniformToFrame,
								ref CSysUniformToFrame, ref DirUniformToFrame, ref ValueUniformToFrame, ref DistTypeUniformToFrame);

							if (NumberItemsUniformToFrame > 0)
							{
								AreaLoadUniformToFrame[] areaLoadUniformsToFrame = new AreaLoadUniformToFrame[NumberItemsUniformToFrame];

								for (int j = 0; j < NumberItemsUniformToFrame; j++)
								{
									AreaLoadUniformToFrame areaLoadUniformToFrame = new AreaLoadUniformToFrame(LoadPatUniformToFrame[j])
									{
										Name = LoadPatUniformToFrame[j],
										CoordinateSystem = CSysUniformToFrame[j],
										Direction = (AreaLoad.Directions)DirUniformToFrame[j],
										Value = ValueUniformToFrame[j],
										LoadPattern = LoadPatUniformToFrame[j],
										DistributionType = (AreaLoadUniformToFrame.DistributionTypes)DistTypeUniformToFrame[j],
									};

									areaLoadUniformsToFrame[j] = areaLoadUniformToFrame;
								}

								area.AreaLoadUniformToFrames = areaLoadUniformsToFrame.ToList();
							}

							#endregion
						}
					}
				}
			}
		}

		protected override void CloseModel()
		{
			if (!_attachToInstance)
				_apiWrapper.ApplicationExit();
		}

		#region Preview data

		public override bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
		{
			doc.Views.RedrawEnabled = false;

			BakeGeometryCustom(doc);

			doc.Views.RedrawEnabled = true;
			doc.Views.Redraw();

			obj_guid = Guid.NewGuid();
			return true;
		}

		public override void BakeGeometryCustom(RhinoDoc doc)
		{
			LayerTable layerTable = doc.Layers;

			#region Dictionary

			Dictionary<string, int> jointGroupsDictionary = new Dictionary<string, int>();
			Dictionary<string, int> frameGroupsDictionary = new Dictionary<string, int>();
			Dictionary<string, int> areaGroupsDictionary = new Dictionary<string, int>();

			F2RModelHelper.GetGroupDictionaryAssociation(Joints, ref jointGroupsDictionary);
			F2RModelHelper.GetGroupDictionaryAssociation(Frames, ref frameGroupsDictionary);
			F2RModelHelper.GetGroupDictionaryAssociation(Areas, ref areaGroupsDictionary);

			#endregion

			Guid jointsLayerGuid = F2RModelHelper.AddLayerToTable("JOINTS", layerTable);
			Guid framesLayerGuid = F2RModelHelper.AddLayerToTable("FRAMES", layerTable);
			Guid areaLayerGuid = F2RModelHelper.AddLayerToTable("AREAS", layerTable);

			Dictionary<string, Guid> frameLayerGuidAss = F2RModelHelper.AddLayerProperties(Frames, layerTable, framesLayerGuid);
			Dictionary<string, Guid> areaLayerGuidAss = F2RModelHelper.AddLayerProperties(Areas, layerTable, areaLayerGuid);

			#region Joints

			Layer jointsLayer = layerTable.FindId(jointsLayerGuid);

			for (int i = 0; i < Joints.Length; i++)
			{
				Joint joint = Joints[i];
				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					if (Option.ExportSectionProperties)
					{
						objAtt.SetUserString("F2R_ID", joint.Id);
						objAtt.SetUserString("F2R_GUID", joint.Guid.ToString());
						objAtt.SetUserString("F2R_SPECIALJOINT", joint.SpecialJoint.ToString());

						if (Option.ExportGeometryDatas)
						{
							objAtt.SetUserString("F2R_RESTRAINT_TRASLATIONS", joint.GetRestraintsTraslation());
							objAtt.SetUserString("F2R_RESTRAINT_ROTATIONS", joint.GetRestraintsRotation());

							if (joint.Groups.Count > 0)
							{
								for (int k = 0; k < joint.Groups.Count; k++)
								{
									objAtt.SetUserString("F2R_GROUP_" + jointGroupsDictionary[joint.Groups[k].Name], joint.Groups[k].Name);
								}
							}
						}

						if (Option.ExportLoads)
						{
							if (joint.PointLoads.Count > 0)
							{
								int loadCount = 1;
								for (int k = 0; k < joint.PointLoads.Count; k++)
								{
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_LOADPATTERN", joint.PointLoads[k].LoadPattern);
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_COORDINATESYSTEM", joint.PointLoads[k].CoordinateSystem.ToString());
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_F", joint.PointLoads[k].GetF());
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_M", joint.PointLoads[k].GetM());
									loadCount++;
								}
							}
						}
					}
					objAtt.LayerIndex = layerTable.FindByFullPath(jointsLayer.FullPath, -1);

					doc.Objects.AddPoint(joint.Location, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake joint {joint.Id}. Exception: {ex}");
				}
			}

			#endregion

			#region Frames

			for (int i = 0; i < Frames.Length; i++)
			{
				Frame frame = (Frame)Frames[i];

				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					if (Option.ExportSectionProperties)
					{
						objAtt.SetUserString("F2R_ID", frame.Id);
						objAtt.SetUserString("F2R_GUID", frame.Guid.ToString());
						objAtt.SetUserString("F2R_JOINT1", frame.StartNode.Id);
						objAtt.SetUserString("F2R_JOINT2", frame.EndNode.Id);
						objAtt.SetUserString("F2R_PROPERTY", frame.Property.Name);
						objAtt.SetUserString("F2R_FRAMETYPE", frame.Property.PropertyType.ToString());
						if (frame.Property.Material != null)
							objAtt.SetUserString("F2R_PROPERTY_MATERIAL", frame.Property.Material.Name);

						if (Option.ExportGeometryDatas)
						{
							objAtt.SetUserString("F2R_ANGLE", frame.Angle.ToString());
							objAtt.SetUserString("F2R_ADVANCEDLOCALAXIS_ACTIVE", frame.AdvancedLocalAxis.ToString());
							objAtt.SetUserString("F2R_ADVANCEDLOCALAXIS_ANGLE", frame.AdvancedLocalAngle.ToString());

							objAtt.SetUserString("F2R_INSERTIONPOINT_CP_CS_ST_M2_M3", ((int)frame.InsertionPoint.CardinalPoint - 1).ToString() + ";" + frame.InsertionPoint.CoordinateSystem + ";" +
								frame.InsertionPoint.StiffnessTransform + ";" + frame.InsertionPoint.Mirror2 + ";" + frame.InsertionPoint.Mirror3);
							objAtt.SetUserString("F2R_INSERTIONPOINT_OFFSET1", frame.InsertionPoint.GetOffset1());
							objAtt.SetUserString("F2R_INSERTIONPOINT_OFFSET2", frame.InsertionPoint.GetOffset2());

							objAtt.SetUserString("F2R_RELEASES_IEND_RELEASES", frame.Release.GetReleasesIEndReleases());
							objAtt.SetUserString("F2R_RELEASES_JEND_RELEASES", frame.Release.GetReleasesJEndReleases());
							objAtt.SetUserString("F2R_RELEASES_IEND_PARTIALFIXITYSPRINGS", frame.Release.GetReleasesIEndPartialFixity());
							objAtt.SetUserString("F2R_RELEASES_JEND_PARTIALFIXITYSPRINGS", frame.Release.GetReleasesJEndPartialFixity());
														
							objAtt.SetUserString("F2R_LOADTRANSFER", frame.LoadTransfer.ToString());
							objAtt.SetUserString("F2R_AUTOMESH", frame.AutoMesh.ToString());

							objAtt.SetUserString("F2R_FRAME_MODIFIERS", frame.BeamModifier.ToString());
							objAtt.SetUserString("F2R_FRAMEPROP_MODIFIERS", frame.BeamPropertyModifier.ToString());

							if (frame.Groups.Count > 0)
							{
								for (int j = 0; j < frame.Groups.Count; j++)
								{
									objAtt.SetUserString("F2R_GROUP_" + frameGroupsDictionary[frame.Groups[j].Name], frame.Groups[j].Name);
								}
							}
						}

						if (frame.Property.IsNone)
						{
							objAtt.SetUserString("F2R_ISNONE", true.ToString());
						}

						if (frame.Property.IsNonPrismatic)
						{
							objAtt.SetUserString("F2R_ISNONPRISMATIC", true.ToString());
							objAtt.SetUserString("F2R_NONPRISMATIC_TOTALLENGHT", frame.Property.TotalLenght.ToString());
							objAtt.SetUserString("F2R_NONPRISMATIC_RELATIVEDISTANCE", frame.Property.RelativeDistance.ToString());
						}

						if (Option.ExportLoads)
						{
							if (frame.PointLoads.Count > 0)
							{
								int loadCount = 1;
								for (int k = 0; k < frame.PointLoads.Count; k++)
								{
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_LOADPATTERN", frame.PointLoads[k].LoadPattern);
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_DIRECTION", frame.PointLoads[k].Direction.ToString());
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_COORDINATESYSTEM", frame.PointLoads[k].CoordinateSystem.ToString());
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_TYPE", frame.PointLoads[k].Type.ToString());
									objAtt.SetUserString("F2R_POINTLOAD_" + loadCount + "_VALUE", frame.PointLoads[k].Value.ToString());
									loadCount++;
								}
							}

							if (frame.LoadDistributeds.Count > 0)
							{
								int loadCount = 1;
								for (int k = 0; k < frame.LoadDistributeds.Count; k++)
								{
									objAtt.SetUserString("F2R_DISTRIBUTEDLOAD_" + loadCount + "_LOADPATTERN", frame.LoadDistributeds[k].LoadPattern);
									objAtt.SetUserString("F2R_DISTRIBUTEDLOAD_" + loadCount + "_DIRECTION", frame.LoadDistributeds[k].Direction.ToString());
									objAtt.SetUserString("F2R_DISTRIBUTEDLOAD_" + loadCount + "_COORDINATESYSTEM", frame.LoadDistributeds[k].CoordinateSystem.ToString());
									objAtt.SetUserString("F2R_DISTRIBUTEDLOAD_" + loadCount + "_TYPE", frame.LoadDistributeds[k].Type.ToString());
									objAtt.SetUserString("F2R_DISTRIBUTEDLOAD_" + loadCount + "_STARTVALUE", frame.LoadDistributeds[k].ValueStart.ToString());
									objAtt.SetUserString("F2R_DISTRIBUTEDLOAD_" + loadCount + "_ENDVALUE", frame.LoadDistributeds[k].ValueEnd.ToString());
									loadCount++;
								}
							}
						}
					}

					Layer sublayerFrame = layerTable.FindId(frameLayerGuidAss[frame.Property.Name]);
					objAtt.LayerIndex = sublayerFrame.Index;

					doc.Objects.AddLine(frame.Line, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake frame {frame.Id}. Exception: {ex}");
				}
			}

			#endregion

			#region Areas

			for (int i = 0; i < Areas.Length; i++)
			{
				Area area = (Area)Areas[i];

				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					if (Option.ExportSectionProperties)
					{
						objAtt.SetUserString("F2R_ID", area.Id);
						objAtt.SetUserString("F2R_GUID", area.Guid.ToString());
						objAtt.SetUserString("F2R_PROPERTY", area.Property.Name);
						objAtt.SetUserString("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
						objAtt.SetUserString("F2R_PROPERTYTYPE", area.Property.PropertyType.ToString());
						objAtt.SetUserString("F2R_ISOPENING", area.Property.IsOpening.ToString());

						for (int n = 0; n < area.GetPoints.Length; n++)
							objAtt.SetUserString($"F2R_JOINT{n + 1}", area.GetPoints[n].Id);

						if (((AreaProperty)area.Property).IsNone)
							objAtt.SetUserString("F2R_ISNONE", true.ToString());

						if (Option.ExportGeometryDatas)
						{
							if (area.Groups.Count > 0)
							{
								for (int j = 0; j < area.Groups.Count; j++)
								{
									objAtt.SetUserString("F2R_GROUP_" + areaGroupsDictionary[area.Groups[j].Name], area.Groups[j].Name);
								}
							}

							objAtt.SetUserString("F2R_PLATE_MODIFIERS", area.PlateModifier.ToString());
							objAtt.SetUserString("F2R_PLATEPROP_MODIFIERS", area.PlatePropertyModifier.ToString());

							objAtt.SetUserString("F2R_ANGLE", area.Angle.ToString());
							objAtt.SetUserString("F2R_ADVANCEDLOCALAXIS_ACTIVE", area.AdvancedLocalAxis.ToString());
							objAtt.SetUserString("F2R_ADVANCEDLOCALAXIS_ANGLE", area.AdvancedLocalAngle.ToString());
						}

						if (Option.ExportLoads)
						{
							if (area.LoadUniform.Count > 0)
							{
								int loadCount = 1;
								for (int k = 0; k < area.LoadUniform.Count; k++)
								{
									objAtt.SetUserString("F2R_UNIFORMLOAD_" + loadCount + "_LOADPATTERN", area.LoadUniform[k].LoadPattern);
									objAtt.SetUserString("F2R_UNIFORMLOAD_" + loadCount + "_DIRECTION", area.LoadUniform[k].Direction.ToString());
									objAtt.SetUserString("F2R_UNIFORMLOAD_" + loadCount + "_COORDINATESYSTEM", area.LoadUniform[k].CoordinateSystem.ToString());
									objAtt.SetUserString("F2R_UNIFORMLOAD_" + loadCount + "_VALUE", area.LoadUniform[k].Value.ToString());
									loadCount++;
								}
							}

							if (area.AreaLoadUniformToFrames.Count > 0)
							{
								int loadCount = 1;
								for (int k = 0; k < area.AreaLoadUniformToFrames.Count; k++)
								{
									objAtt.SetUserString("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_LOADPATTERN", area.AreaLoadUniformToFrames[k].LoadPattern);
									objAtt.SetUserString("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_DIRECTION", area.AreaLoadUniformToFrames[k].Direction.ToString());
									objAtt.SetUserString("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_COORDINATESYSTEM", area.AreaLoadUniformToFrames[k].CoordinateSystem.ToString());
									objAtt.SetUserString("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_VALUE", area.AreaLoadUniformToFrames[k].Value.ToString());
									objAtt.SetUserString("F2R_UNIFORMTOFRAMELOAD_" + loadCount + "_DISTRIBUTIONTYPE", area.AreaLoadUniformToFrames[k].DistributionType.ToString());
									loadCount++;
								}
							}
						}

						Layer sublayerFrame = layerTable.FindId(areaLayerGuidAss[area.Property.Name]);
						objAtt.LayerIndex = sublayerFrame.Index;
					}

					if (area.Shape is Surface)
						doc.Objects.AddSurface((Surface)area.Shape, objAtt);
					else if (area.Shape is Brep)
						doc.Objects.AddBrep((Brep)area.Shape, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake area {area.Id}. Exception: {ex}");
				}
			}

			#endregion
		}

		#endregion
	}
}
