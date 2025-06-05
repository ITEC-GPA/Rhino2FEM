using CSiAPIv1;
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
	public class SapInteractiveDatabaseModel : Model
	{
		protected readonly int _units;
		protected string _group;
		protected readonly ApiWrapper.SAPApiWrapper _apiWrapper;
		protected string[] _tableDataJointCoordinates;
		protected string[] _tableDataConnectivityFrame;
		protected string[] _tableDataConnectivityArea;
		protected string[] _tableDataFrameSectionAssignment;
		protected string[] _tableDataAreaSectionAssignment;
		protected string[] _tableDataGroupAssignment;
		protected Dictionary<string, Joint> _idJointAssociation;

		protected cHelper _myHelper;
		protected cOAPI _SapObject;
		protected cSapModel _SapModel;

		public string Group { get => _group; set => _group = value; }

		public SapInteractiveDatabaseModel()
			: base()
		{
			_apiWrapper = new ApiWrapper.SAPApiWrapper();
			_idJointAssociation = new Dictionary<string, Joint>();
		}

		protected override void InizializeAPI()
		{
			_myHelper = new Helper();
			if (_myHelper == null)
				return;
			_SapObject = _myHelper.GetObject("CSI.SAP2000.API.SapObject");
			if (_SapObject == null)
				return;
			_SapModel = _SapObject.SapModel;
		}

		public override void Process()
		{
			InizializeAPI();
			if (_SapModel == null)
				return;
			OpenInteractiveTables();
			CloseModel();
		}

		protected override void ReadModel()
		{
			ReadJoint();
			ReadFrame();
			ReadArea();
		}

		protected override void CloseModel()
		{
			_SapModel = null;
			_SapObject = null;
			_myHelper = null;
		}

		public void OpenInteractiveTables()
		{
			/// Error
			int ret;

			///Input default Interactive Database Editing
			string GroupName;
			if (_group == "" || _group == string.Empty)
				GroupName = "ALL";
			else
				GroupName = _group;

			int TableVersion = 0;
			string[] FieldKeysIncluded = { };
			int NumberRecords = 0;

			///lista delle tabelle da esportare
			string TableKeyJC = "Joint Coordinates";
			string TableKeyCF = "Connectivity - Frame";
			string TableKeyFSA = "Frame Section Assignments";
			string TableKeyG2A = "Groups 2 - Assignments";
			string TableKeyASC = "Connectivity - Area";
			string TableKeyASA = "Area Section Assignments";

			string[] TableDataJointCoordinates = new string[0];
			string[] TableDataConnectivityFrame = new string[0];
			string[] TableDataFrameSectionAssignment = new string[0];
			string[] TableDataAreaSectionAssignment = new string[0];
			string[] TableDataGroupAssignment = new string[0];
			string[] TableDataConnectivityArea = new string[0];


			_SapModel.SetModelIsLocked(false);

			ret = _SapModel.DatabaseTables.GetTableForEditingArray(TableKeyJC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataJointCoordinates);
			if (ret != 0 && TableDataJointCoordinates.Length != 0)
				_log.Add("Fail to get Joint Coordinates table");

			ret = _SapModel.DatabaseTables.GetTableForEditingArray(TableKeyCF, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityFrame);
			if (ret != 0 && TableDataConnectivityFrame.Length != 0)
				_log.Add("Fail to get Connectivity - Frame table");

			ret = _SapModel.DatabaseTables.GetTableForEditingArray(TableKeyFSA, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataFrameSectionAssignment);
			if (ret != 0 && TableDataFrameSectionAssignment.Length != 0)
				_log.Add("Fail to get Frame Section Assignments table");

			ret = _SapModel.DatabaseTables.GetTableForEditingArray(TableKeyG2A, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataGroupAssignment);
			if (ret != 0 && TableDataGroupAssignment.Length != 0)
				_log.Add("Fail to get Groups 2 - Assignments table");

			ret = _SapModel.DatabaseTables.GetTableForEditingArray(TableKeyASC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityArea);
			if (ret != 0 && TableDataConnectivityArea.Length != 0)
				_log.Add("Fail to get Connectivity - Area table");

			ret = _SapModel.DatabaseTables.GetTableForEditingArray(TableKeyASA, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataAreaSectionAssignment);
			if (ret != 0 && TableDataAreaSectionAssignment.Length != 0)
				_log.Add("Fail to get Area Section Assignments table");

			_tableDataJointCoordinates = TableDataJointCoordinates;
			_tableDataConnectivityFrame = TableDataConnectivityFrame;
			_tableDataFrameSectionAssignment = TableDataFrameSectionAssignment;
			_tableDataConnectivityArea = TableDataConnectivityArea;
			_tableDataAreaSectionAssignment = TableDataAreaSectionAssignment;
			_tableDataGroupAssignment = TableDataGroupAssignment;

			ReadJoint();
			ReadFrame();
			ReadArea();
		}

		protected override void ReadJoint()
		{
			int numberOfJoints = _tableDataJointCoordinates.Count() / 9;

			_joints = new Joint[numberOfJoints];

			for (int i = 0; i < numberOfJoints; i++)
			{
				double x = 0;
				double y = 0;
				double z = 0;
				string id = "";

				try
				{
					id = _tableDataJointCoordinates[9 * i];
					x = Convert.ToDouble(_tableDataJointCoordinates[3 + 9 * i]);
					y = Convert.ToDouble(_tableDataJointCoordinates[4 + 9 * i]);
					z = Convert.ToDouble(_tableDataJointCoordinates[6 + 9 * i]);
					string guid = _tableDataJointCoordinates[8 + 9 * i];
					bool special = false;
					if (_tableDataJointCoordinates[7 + 9 * i] == "No")
						special = false;
					else
						special = true;

					Joint joint = new Joint(x, y, z, id) { SpecialJoint = special, Guid = guid != "" ? Guid.Parse(guid) : Guid.NewGuid() };

					_idJointAssociation.Add(id, joint);

					string[] jointgrp = { };
					int numgrp = 0;
					_SapModel.PointObj.GetGroupAssign(joint.Id, ref numgrp, ref jointgrp);

					joint.Groups = Enumerable.Range(0, jointgrp.Length).Select(k => new Common.Group(jointgrp[k])).ToList();

					_joints[i] = joint;
				}
				catch (Exception)
				{
					_log.Add($"Fail to get joint {id} with coordinates {x}, {y}, {z}");
				}
			}
		}

		protected override void ReadFrame()
		{
			int numberOfFrames = _tableDataConnectivityFrame.Count() / 5;

			_frames = new Beam[numberOfFrames];
			for (int i = 0; i < numberOfFrames; i++)
			{
				string frameId = string.Empty;
				string iNode = string.Empty;
				string jNode = string.Empty;

				try
				{
					frameId = _tableDataConnectivityFrame[5 * i];
					iNode = _tableDataConnectivityFrame[1 + 5 * i];
					jNode = _tableDataConnectivityFrame[2 + 5 * i];
					string guid = _tableDataConnectivityFrame[4 + 5 * i];

					Joint startNode = _idJointAssociation[iNode];
					Joint endNode = _idJointAssociation[jNode];

					Frame frame = new Frame(startNode, endNode, frameId) { Guid = guid != "" ? Guid.Parse(guid) : Guid.NewGuid() };

					_frames[i] = frame;
				}
				catch (Exception)
				{
					_log.Add($"Fail to get frame {frameId} with start node {iNode} and end node {jNode}");
				}
			}

			int numberOfFrameAssignment = _tableDataFrameSectionAssignment.Count() / 7;
			for (int i = 0; i < numberOfFrameAssignment; i++)
			{
				Common.Material material = new Common.Material() { Name = _tableDataFrameSectionAssignment[3 + 7 * i], };
				Section section = new Section() { Name = _tableDataFrameSectionAssignment[2 + 7 * i] };

				FrameProperty frameProperty = new FrameProperty()
				{
					Section = section,
					Material = material,
					Name = section.Name,
				};
				((Frame)_frames[i]).Property = frameProperty;

				string[] beamgrp = { };
				int numgrp = 0;
				_SapModel.FrameObj.GetGroupAssign(_tableDataFrameSectionAssignment[7 * i], ref numgrp, ref beamgrp);

				((Frame)_frames[i]).Groups = Enumerable.Range(0, beamgrp.Length).Select(k => new Common.Group(beamgrp[k])).ToList();
			}
		}

		protected override void ReadArea()
		{
			List<Plate> plateBuffer = new List<Plate>();
			string currentArea = string.Empty;
			string currentGuid = string.Empty;
			List<Joint> currentJoints = new List<Joint>();
			Dictionary<int, Area> areaIndexNameAssociation = new Dictionary<int, Area>();

			int numberOfRows = _tableDataConnectivityArea.Count() / 6;
			int plateCount = 0;

			for (int i = 0; i < numberOfRows; i++)
			{
				try
				{
					string areaId = _tableDataConnectivityArea[6 * i];
					string n1 = _tableDataConnectivityArea[1 + 6 * i];
					string n2 = _tableDataConnectivityArea[2 + 6 * i];
					string n3 = _tableDataConnectivityArea[3 + 6 * i];
					string n4 = _tableDataConnectivityArea[4 + 6 * i];
					string guid = _tableDataConnectivityArea[5 + 6 * i];

					if (currentArea != areaId)
					{
						if (currentArea != string.Empty)
						{
							try
							{
								Area area = new Area(currentArea, currentGuid != "" ? new Guid(currentGuid) : Guid.NewGuid(), currentJoints.ToArray());
								plateBuffer.Add(area);
								areaIndexNameAssociation.Add(plateCount, area);
								plateCount++;
							}
							catch (Exception)
							{
								_log.Add($"Fail to get area {currentArea}");
								plateCount++;
							}
						}

						currentGuid = guid;
						currentArea = areaId;
						currentJoints.Clear();
					}

					if (_idJointAssociation.ContainsKey(n1))
						currentJoints.Add(_idJointAssociation[n1]);
					if (_idJointAssociation.ContainsKey(n2))
						currentJoints.Add(_idJointAssociation[n2]);
					if (_idJointAssociation.ContainsKey(n3))
						currentJoints.Add(_idJointAssociation[n3]);
					if (_idJointAssociation.ContainsKey(n4))
						currentJoints.Add(_idJointAssociation[n4]);
				}
				catch (Exception)
				{
					_log.Add($"Fail to get area {currentArea}");
				}
			}

			if (currentArea != string.Empty)
			{
				Area area = new Area(currentArea, currentGuid != "" ? new Guid(currentGuid) : Guid.NewGuid(), currentJoints.ToArray());
				plateBuffer.Add(area);
				areaIndexNameAssociation.Add(plateCount, area);
				plateCount++;
			}

			_areas = plateBuffer.ToArray();

			int numberOfConnAreas = _tableDataAreaSectionAssignment.Count() / 3;

			for (int i = 0; i < numberOfConnAreas; i++)
			{
				string areaId = _tableDataAreaSectionAssignment[3 * i];
				string section = _tableDataAreaSectionAssignment[1 + 3 * i];
				string matProp = _tableDataAreaSectionAssignment[2 + 3 * i];

				if (section == "None")
				{
					if (areaIndexNameAssociation.ContainsKey(i))
						areaIndexNameAssociation[i].Property = new AreaProperty(true) { Name = "None" };
				}
				else
				{
					if (areaIndexNameAssociation.ContainsKey(i))
						areaIndexNameAssociation[i].Property = new AreaProperty { Name = section };
				}

				string[] areagrp = { };
				int numgrp = 0;
				_SapModel.AreaObj.GetGroupAssign(areaId, ref numgrp, ref areagrp);

				if (areaIndexNameAssociation.ContainsKey(i))
					areaIndexNameAssociation[i].Groups = Enumerable.Range(0, areagrp.Length).Select(k => new Common.Group(areagrp[k])).ToList();
			}
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

			Guid jointsLayerGuid = F2RModelHelper.AddLayerToTable("JOINTS", layerTable);
			Guid framesLayerGuid = F2RModelHelper.AddLayerToTable("FRAMES", layerTable);
			Guid areaLayerGuid = F2RModelHelper.AddLayerToTable("AREAS", layerTable);

			Dictionary<string, Guid> frameLayerGuidAss = F2RModelHelper.AddLayerProperties(Frames, layerTable, framesLayerGuid);
			Dictionary<string, Guid> areaLayerGuidAss = F2RModelHelper.AddLayerProperties(Areas, layerTable, areaLayerGuid);

			#endregion

			#region Joints

			Layer jointsLayer = layerTable.FindId(jointsLayerGuid);

			for (int i = 0; i < Joints.Length; i++)
			{
				Joint joint = Joints[i];
				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();

					objAtt.SetUserString("F2R_ID", joint.Id);
					objAtt.SetUserString("F2R_SPECIALJOINT", joint.SpecialJoint.ToString());
					objAtt.SetUserString("F2R_GUID", joint.Guid.ToString());

					if (joint.Groups.Count > 0)
					{
						for (int k = 0; k < joint.Groups.Count; k++)
						{
							objAtt.SetUserString("F2R_GROUP_" + jointGroupsDictionary[joint.Groups[k].Name], joint.Groups[k].Name);
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
				if (Frames[i] != null)
				{
					Frame frame = (Frame)Frames[i];
					try
					{
						ObjectAttributes objAtt = new ObjectAttributes();
						objAtt.SetUserString("F2R_ID", frame.Id);
						objAtt.SetUserString("F2R_JOINT1", frame.StartNode.Id);
						objAtt.SetUserString("F2R_JOINT2", frame.EndNode.Id);
						objAtt.SetUserString("F2R_GUID", frame.Guid.ToString());
						objAtt.SetUserString("F2R_PROPERTY", frame.Property.Name);
						objAtt.SetUserString("F2R_PROPERTY_MATERIAL", frame.Property.Material.Name);

						if (frame.Groups.Count > 0)
						{
							for (int j = 0; j < frame.Groups.Count; j++)
							{
								objAtt.SetUserString("F2R_GROUP_" + frameGroupsDictionary[frame.Groups[j].Name], frame.Groups[j].Name);
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
			}

			#endregion

			#region Areas					

			for (int i = 0; i < Areas.Length; i++)
			{
				if (Areas[i] != null)
				{
					Area area = (Area)Areas[i];

					try
					{
						ObjectAttributes objAtt = new ObjectAttributes();

						objAtt.SetUserString("F2R_ID", area.Id);
						objAtt.SetUserString("F2R_GUID", area.Guid.ToString());
						objAtt.SetUserString("F2R_PROPERTY", area.Property.Name);
						objAtt.SetUserString("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
						for (int n = 0; n < area.GetPoints.Length; n++)
							objAtt.SetUserString($"F2R_JOINT{n + 1}", area.GetPoints[n].Id);

						if (((AreaProperty)area.Property).IsNone)
							objAtt.SetUserString("F2R_ISNONE", true.ToString());

						if (area.Groups.Count > 0)
						{
							for (int j = 0; j < area.Groups.Count; j++)
							{
								objAtt.SetUserString("F2R_GROUP_" + areaGroupsDictionary[area.Groups[j].Name], area.Groups[j].Name);
							}
						}

						Layer sublayerFrame = layerTable.FindId(areaLayerGuidAss[area.Property.Name]);
						objAtt.LayerIndex = sublayerFrame.Index;

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
			}

			#endregion
		}

		public override bool BakeGeometry(RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, out Guid obj_guid)
		{
			doc.Views.RedrawEnabled = false;

			BakeGeometryCustom(doc);

			doc.Views.RedrawEnabled = true;
			doc.Views.Redraw();

			obj_guid = Guid.NewGuid();
			return true;
		}
	}
}
