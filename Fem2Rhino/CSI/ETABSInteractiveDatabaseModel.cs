using ETABSv1;
using Fem2Rhino.Common;
using Fem2Rhino.CSI.ModelWrapper;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace Fem2Rhino.CSI
{
	public class ETABSInteractiveDatabaseModel : Model
	{
		protected readonly int _units;
		protected cHelper _myHelper;
		protected cOAPI _EtabsObject;
		protected cSapModel _EtabsModel;

		protected Beam[] _columns;
		protected Beam[] _braces;
		protected Beam[] _nullLines;
		protected Plate[] _floorAreas;
		protected Plate[] _wallAreas;
		protected Plate[] _nullAreas;

		protected string[] _tableDataJointCoordinates;

		protected string[] _tableDataConnectivityBeams;
		protected string[] _tableDataConnectivityColumns;
		protected string[] _tableDataConnectivityBraces;
		protected string[] _tableDataConnectivityNullLines;
		protected string[] _tableDataFrameSectionAssignment;

		protected string[] _tableDataConnectivityFloors;
		protected string[] _tableDataConnectivityNullAreas;
		protected string[] _tableDataConnectivityWalls;
		protected string[] _tableDataAreaSectionAssignment;

		protected string[] _tableDataGroupAssignment;

		protected Dictionary<string, Joint> _idJointAssociation;

		public Beam[] Columns => _columns;
		public Beam[] Braces => _braces;
		public Beam[] NullLines => _nullLines;
		public Plate[] FloorAreas => _floorAreas;
		public Plate[] WallAreas => _wallAreas;
		public Plate[] NullAreas => _nullAreas;

		public ETABSInteractiveDatabaseModel()
			: base()
		{
			_idJointAssociation = new Dictionary<string, Joint>();
			_columns = new Beam[0];
			_braces = new Beam[0];
			_nullLines = new Beam[0];
			_floorAreas = new Plate[0];
			_wallAreas = new Plate[0];
			_nullAreas = new Plate[0];
		}

		protected override void InizializeAPI()
		{
			_myHelper = new Helper();
			if (_myHelper == null)
				return;
			_EtabsObject = _myHelper.GetObject("CSI.ETABS.API.ETABSObject");
			if (_EtabsObject == null)
				return;
			_EtabsModel = _EtabsObject.SapModel;
		}

		public override void Process()
		{
			InizializeAPI();
			if (_EtabsModel == null)
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
			_EtabsModel = null;
			_EtabsObject = null;
			_myHelper = null;
		}

		public void OpenInteractiveTables()
		{
			/// Error
			int ret;

			///Input default Interactive Database Editing
			string GroupName = "ALL";
			int TableVersion = 0;
			string[] FieldKeysIncluded = { };
			int NumberRecords = 0;

			///lista delle tabelle da esportare
			string TableKeyPOC = "Point Object Connectivity";

			string TableKeyBOC = "Beam Object Connectivity";
			string TableKeyCOC = "Column Object Connectivity";
			string TableKeyBrOC = "Brace Object Connectivity";
			string TableKeyNLOC = "Null Line Object Connectivity";
			string TableKeyFASP = "Frame Assignments - Section Properties";

			string TableKeyFOC = "Floor Object Connectivity";
			string TableKeyWOC = "Wall Object Connectivity";
			string TableKeyNAOC = "Null Area Object Connectivity";
			string TableKeyAASP = "Area Assignments - Section Properties";

			string[] TableDataJointCoordinates = new string[0];
			string[] TableDataConnectivityBeam = new string[0];
			string[] TableDataConnectivityColums = new string[0];
			string[] TableDataConnectivityBraces = new string[0];
			string[] TableDataConnectivityNullLine = new string[0];
			string[] TableDataFrameSectionAssignment = new string[0];
			string[] TableDataConnectivityFloor = new string[0];
			string[] TableDataConnectivityWall = new string[0];
			string[] TableDataConnectivityNullArea = new string[0];
			string[] TableDataAreaSectionAssignment = new string[0];
			string[] TableDataGroupAssignment = new string[0];


			_EtabsModel.SetModelIsLocked(false);

			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyPOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataJointCoordinates);
			if (ret != 0 && TableDataJointCoordinates.Length != 0)
				_log.Add($"Fail to get {TableKeyPOC}");

			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyBOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityBeam);
			if (ret != 0 && TableDataConnectivityBeam.Length != 0)
				_log.Add($"Fail to get {TableKeyBOC}");
			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyCOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityColums);
			if (ret != 0 && TableDataConnectivityColums.Length != 0)
				_log.Add($"Fail to get {TableKeyCOC}");
			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyNLOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityNullLine);
			if (ret != 0 && TableDataConnectivityNullLine.Length != 0)
				_log.Add($"Fail to get {TableKeyNLOC}");
			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyBrOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityBraces);
			if (ret != 0 && TableDataConnectivityBraces.Length != 0)
				_log.Add($"Fail to get {TableKeyBrOC}");

			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyFASP, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataFrameSectionAssignment);
			if (ret != 0 && TableDataFrameSectionAssignment.Length != 0)
				_log.Add($"Fail to get {TableKeyFASP}");

			//ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyG2A, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataGroupAssignment);
			//if (ret != 0 && TableDataGroupAssignment.Length != 0)
			//	_log.Add("Fail to get Groups 2 - Assignments table");

			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyFOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityFloor);
			if (ret != 0 && TableDataConnectivityFloor.Length != 0)
				_log.Add($"Fail to get {TableKeyFOC}");

			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyAASP, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataAreaSectionAssignment);
			if (ret != 0 && TableDataAreaSectionAssignment.Length != 0)
				_log.Add($"Fail to get {TableKeyAASP}");
			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyNAOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityNullArea);
			if (ret != 0 && TableDataConnectivityNullArea.Length != 0)
				_log.Add($"Fail to get {TableKeyNAOC}");
			ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyWOC, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataConnectivityWall);
			if (ret != 0 && TableDataConnectivityWall.Length != 0)
				_log.Add($"Fail to get {TableKeyWOC}");

			//ret = _EtabsModel.DatabaseTables.GetTableForEditingArray(TableKeyASA, GroupName, ref TableVersion, ref FieldKeysIncluded, ref NumberRecords, ref TableDataAreaSectionAssignment);
			//if (ret != 0 && TableDataAreaSectionAssignment.Length != 0)
			//	_log.Add("Fail to get Area Section Assignments table");

			_tableDataJointCoordinates = TableDataJointCoordinates;

			_tableDataConnectivityBeams = TableDataConnectivityBeam;
			_tableDataConnectivityColumns = TableDataConnectivityColums;
			_tableDataConnectivityBraces = TableDataConnectivityBraces;
			_tableDataConnectivityNullLines = TableDataConnectivityNullLine;
			_tableDataFrameSectionAssignment = TableDataFrameSectionAssignment;

			_tableDataConnectivityFloors = TableDataConnectivityFloor;
			_tableDataConnectivityNullAreas = TableDataConnectivityNullArea;
			_tableDataConnectivityWalls = TableDataConnectivityWall;
			_tableDataAreaSectionAssignment = TableDataAreaSectionAssignment;

			_tableDataGroupAssignment = TableDataGroupAssignment;

			ReadJoint();
			ReadFrame();
			ReadArea();
		}

		protected override void ReadJoint()
		{
			int numberOfJoints = _tableDataJointCoordinates.Count() / 7;

			_joints = new Joint[numberOfJoints];

			for (int i = 0; i < numberOfJoints; i++)
			{
				double x = 0;
				double y = 0;
				double z = 0;
				bool special = false;
				string id = "";

				try
				{
					id = _tableDataJointCoordinates[7 * i];
					x = Convert.ToDouble(_tableDataJointCoordinates[3 + 7 * i]);
					y = Convert.ToDouble(_tableDataJointCoordinates[4 + 7 * i]);
					z = Convert.ToDouble(_tableDataJointCoordinates[5 + 7 * i]);
					string guid = _tableDataJointCoordinates[6 + 7 * i];
					if (_tableDataJointCoordinates[2 + 7 * i] == "No")
						special = false;
					else
						special = true;

					Joint joint = new Joint(x, y, z, id) { SpecialJoint = special, Guid = Guid.Parse(guid) };

					_idJointAssociation.Add(id, joint);

					string[] jointgrp = { };
					int numgrp = 0;
					_EtabsModel.PointObj.GetGroupAssign(joint.Id, ref numgrp, ref jointgrp);

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
			int numberOfBeams = _tableDataConnectivityBeams.Count() / 4;
			int numberOfColumns = _tableDataConnectivityColumns.Count() / 4;
			int numberOfBraces = _tableDataConnectivityBraces.Count() / 4;
			int numberOfNullLines = _tableDataConnectivityNullLines.Count() / 4;

			_frames = new Beam[numberOfBeams];
			for (int i = 0; i < numberOfBeams; i++)
			{
				string frameId = string.Empty;
				string iNode = string.Empty;
				string jNode = string.Empty;

				try
				{
					frameId = _tableDataConnectivityBeams[4 * i];
					iNode = _tableDataConnectivityBeams[1 + 4 * i];
					jNode = _tableDataConnectivityBeams[2 + 4 * i];

					Joint startNode = _idJointAssociation[iNode];
					Joint endNode = _idJointAssociation[jNode];

					Frame frame = new Frame(startNode, endNode, frameId);

					_frames[i] = frame;
				}
				catch (Exception)
				{
					_log.Add($"Fail to get frame {frameId} with start node {iNode} and end node {jNode}");
				}
			}

			_columns = new Beam[numberOfColumns];
			for (int i = 0; i < numberOfColumns; i++)
			{
				string frameId = string.Empty;
				string iNode = string.Empty;
				string jNode = string.Empty;

				try
				{
					frameId = _tableDataConnectivityColumns[4 * i];
					iNode = _tableDataConnectivityColumns[1 + 4 * i];
					jNode = _tableDataConnectivityColumns[2 + 4 * i];

					Joint startNode = _idJointAssociation[iNode];
					Joint endNode = _idJointAssociation[jNode];

					Frame frame = new Frame(startNode, endNode, frameId);

					_columns[i] = frame;
				}
				catch (Exception)
				{
					_log.Add($"Fail to get column {frameId} with start node {iNode} and end node {jNode}");
				}
			}

			_braces = new Beam[numberOfBraces];
			for (int i = 0; i < numberOfBraces; i++)
			{
				string frameId = string.Empty;
				string iNode = string.Empty;
				string jNode = string.Empty;

				try
				{
					frameId = _tableDataConnectivityBraces[4 * i];
					iNode = _tableDataConnectivityBraces[1 + 4 * i];
					jNode = _tableDataConnectivityBraces[2 + 4 * i];

					Joint startNode = _idJointAssociation[iNode];
					Joint endNode = _idJointAssociation[jNode];

					Frame frame = new Frame(startNode, endNode, frameId);

					_braces[i] = frame;
				}
				catch (Exception)
				{
					_log.Add($"Fail to get column {frameId} with start node {iNode} and end node {jNode}");
				}
			}

			_nullLines = new Beam[numberOfNullLines];
			for (int i = 0; i < numberOfNullLines; i++)
			{
				string frameId = string.Empty;
				string iNode = string.Empty;
				string jNode = string.Empty;

				try
				{
					frameId = _tableDataConnectivityNullLines[4 * i];
					iNode = _tableDataConnectivityNullLines[1 + 4 * i];
					jNode = _tableDataConnectivityNullLines[2 + 4 * i];

					Joint startNode = _idJointAssociation[iNode];
					Joint endNode = _idJointAssociation[jNode];

					Frame frame = new Frame(startNode, endNode, frameId);

					_nullLines[i] = frame;
				}
				catch (Exception)
				{
					_log.Add($"Fail to get null line {frameId} with start node {iNode} and end node {jNode}");
				}
			}

			int numberOfFrameAssignment = _tableDataFrameSectionAssignment.Count() / 3;
			for (int i = 0; i < numberOfFrameAssignment; i++)
			{
				Common.Material material = new Common.Material() { Name = "" };
				Section section = new Section() { Name = _tableDataFrameSectionAssignment[2 + 3 * i] };

				FrameProperty frameProperty = new FrameProperty()
				{
					Section = section,
					Material = material,
					Name = section.Name,
				};

				string id = _tableDataFrameSectionAssignment[3 * i];
				Frame fframe = (((Frame)_frames.Where(j => j.Id == id).FirstOrDefault() ?? (Frame)_columns.Where(j => j.Id == id).FirstOrDefault()) 
					?? (Frame)_nullLines.Where(j => j.Id == id).FirstOrDefault()) ?? (Frame)_braces.Where(j => j.Id == id).FirstOrDefault();

				fframe.Property = frameProperty;

				string[] beamgrp = { };
				int numgrp = 0;
				_EtabsModel.FrameObj.GetGroupAssign(id, ref numgrp, ref beamgrp);

				fframe.Groups = Enumerable.Range(0, beamgrp.Length).Select(k => new Common.Group(beamgrp[k])).ToList();
			}
		}

		protected override void ReadArea()
		{
			List<Plate> plateBuffer = new List<Plate>();

			string currentArea = string.Empty;
			string currentGuid = string.Empty;
			List<Joint> currentJoints = new List<Joint>();

			#region Floors

			int numberOfRows = _tableDataConnectivityFloors.Count() / 6;

			for (int i = 0; i < numberOfRows; i++)
			{
				try
				{
					string areaId = _tableDataConnectivityFloors[6 * i];
					string n1 = _tableDataConnectivityFloors[1 + 6 * i];
					string n2 = _tableDataConnectivityFloors[2 + 6 * i];
					string n3 = _tableDataConnectivityFloors[3 + 6 * i];
					string n4 = _tableDataConnectivityFloors[4 + 6 * i];
					string guid = _tableDataConnectivityFloors[5 + 6 * i];

					if (currentArea != areaId)
					{
						if (currentArea != string.Empty)
						{
							plateBuffer.Add(new Area(currentArea, new Guid(currentGuid), currentJoints.ToArray()));
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
				plateBuffer.Add(new Area(currentArea, new Guid(currentGuid), currentJoints.ToArray()));
			}

			_floorAreas = plateBuffer.ToArray();

			#endregion

			#region Walls

			numberOfRows = _tableDataConnectivityWalls.Count() / 6;
			currentJoints.Clear();
			plateBuffer.Clear();
			currentArea = string.Empty;
			currentGuid = string.Empty;

			for (int i = 0; i < numberOfRows; i++)
			{
				try
				{
					string areaId = _tableDataConnectivityWalls[6 * i];
					string n1 = _tableDataConnectivityWalls[1 + 6 * i];
					string n2 = _tableDataConnectivityWalls[2 + 6 * i];
					string n3 = _tableDataConnectivityWalls[3 + 6 * i];
					string n4 = _tableDataConnectivityWalls[4 + 6 * i];
					string guid = _tableDataConnectivityWalls[5 + 6 * i];

					if (currentArea != areaId)
					{
						if (currentArea != string.Empty)
						{
							plateBuffer.Add(new Area(currentArea, new Guid(currentGuid), currentJoints.ToArray()));
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
				plateBuffer.Add(new Area(currentArea, new Guid(currentGuid), currentJoints.ToArray()));
			}

			_wallAreas = plateBuffer.ToArray();

			#endregion

			#region Null Areas

			numberOfRows = _tableDataConnectivityNullAreas.Count() / 6;
			currentJoints.Clear();
			plateBuffer.Clear();
			currentArea = string.Empty;
			currentGuid = string.Empty;

			for (int i = 0; i < numberOfRows; i++)
			{
				try
				{
					string areaId = _tableDataConnectivityNullAreas[6 * i];
					string n1 = _tableDataConnectivityNullAreas[1 + 6 * i];
					string n2 = _tableDataConnectivityNullAreas[2 + 6 * i];
					string n3 = _tableDataConnectivityNullAreas[3 + 6 * i];
					string n4 = _tableDataConnectivityNullAreas[4 + 6 * i];
					string guid = _tableDataConnectivityNullAreas[5 + 6 * i];

					if (currentArea != areaId)
					{
						if (currentArea != string.Empty)
						{
							plateBuffer.Add(new Area(currentArea, new Guid(currentGuid), currentJoints.ToArray()));
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
				plateBuffer.Add(new Area(currentArea, new Guid(currentGuid), currentJoints.ToArray()));
			}

			_nullAreas = plateBuffer.ToArray();

			#endregion


			int numberOfConnAreas = _tableDataAreaSectionAssignment.Count() / 3;

			for (int i = 0; i < numberOfConnAreas; i++)
			{
				string areaId = _tableDataAreaSectionAssignment[3 * i];
				string sectionProp = _tableDataAreaSectionAssignment[1 + 3 * i];
				string propertyTypeString = _tableDataAreaSectionAssignment[2 + 3 * i];

				PlateProperty.PropertyTypes propertyType;
				if (propertyTypeString == "Wall")
					propertyType = PlateProperty.PropertyTypes.Wall;
				else if (propertyTypeString == "Slab")
					propertyType = PlateProperty.PropertyTypes.Floor;
				else if (propertyTypeString == "Opening")
					propertyType = PlateProperty.PropertyTypes.Null;
				else
					propertyType = PlateProperty.PropertyTypes.Other;

				Plate pplate = (_floorAreas.Where(j => j.Id == areaId).FirstOrDefault() ?? _wallAreas.Where(j => j.Id == areaId).FirstOrDefault()) ?? _nullAreas.Where(j => j.Id == areaId).FirstOrDefault();

				if (sectionProp == "None")
					pplate.Property = new AreaProperty(true) { Name = "None", PropertyType = propertyType };
				else
					pplate.Property = new AreaProperty { Name = sectionProp, PropertyType = propertyType };

				if (propertyTypeString == "Opening")
					pplate.Property.IsOpening = true;

				string[] areagrp = { };
				int numgrp = 0;
				_EtabsModel.AreaObj.GetGroupAssign(areaId, ref numgrp, ref areagrp);

				pplate.Groups = Enumerable.Range(0, areagrp.Length).Select(k => new Common.Group(areagrp[k])).ToList();
			}
		}

		public override void BakeGeometryCustom(RhinoDoc doc)
		{
			LayerTable layerTable = doc.Layers;

			#region Dictionary

			Dictionary<string, int> jointGroupsDictionary = new Dictionary<string, int>();
			Dictionary<string, int> frameGroupsDictionary = new Dictionary<string, int>();
			Dictionary<string, int> areaGroupsDictionary = new Dictionary<string, int>();

			List<Beam> frameList = new List<Beam>(Frames);
			frameList.AddRange(Columns);
			frameList.AddRange(Braces);
			frameList.AddRange(NullLines);
			Beam[] frames = frameList.ToArray();

			List<Plate> areaList = new List<Plate>(FloorAreas);
			areaList.AddRange(WallAreas);
			areaList.AddRange(NullAreas);
			Plate[] areas = areaList.ToArray();

			F2RModelHelper.GetGroupDictionaryAssociation(Joints, ref jointGroupsDictionary);
			F2RModelHelper.GetGroupDictionaryAssociation(frames, ref frameGroupsDictionary);
			F2RModelHelper.GetGroupDictionaryAssociation(areas, ref areaGroupsDictionary);

			#endregion

			Guid jointsLayerGuid = F2RModelHelper.AddLayerToTable("JOINTS", layerTable);

			Guid framesLayerGuid = F2RModelHelper.AddLayerToTable("FRAMES", layerTable);
			Guid beamsLayerGuid = F2RModelHelper.AddLayerToTable("BEAMS", framesLayerGuid, layerTable);
			Guid columnsLayerGuid = F2RModelHelper.AddLayerToTable("COLUMNS", framesLayerGuid, layerTable);
			Guid bracesLayerGuid = F2RModelHelper.AddLayerToTable("BRACES", framesLayerGuid, layerTable);
			Guid nullLinesLayerGuid = F2RModelHelper.AddLayerToTable("NULL LINES", framesLayerGuid, layerTable);

			Guid areaLayerGuid = F2RModelHelper.AddLayerToTable("AREAS", layerTable);
			Guid floorLayerGuid = F2RModelHelper.AddLayerToTable("FLOORS", areaLayerGuid, layerTable);
			Guid wallsLayerGuid = F2RModelHelper.AddLayerToTable("WALLS", areaLayerGuid, layerTable);
			Guid nullAreasLayerGuid = F2RModelHelper.AddLayerToTable("NULL AREAS", areaLayerGuid, layerTable);

			Dictionary<string, Guid> beamLayerGuidAss = F2RModelHelper.AddLayerProperties(Frames, layerTable, beamsLayerGuid);
			Dictionary<string, Guid> columnLayerGuidAss = F2RModelHelper.AddLayerProperties(Columns, layerTable, columnsLayerGuid);
			Dictionary<string, Guid> bracesLayerGuidAss = F2RModelHelper.AddLayerProperties(Braces, layerTable, bracesLayerGuid);
			Dictionary<string, Guid> nullLineLayerGuidAss = F2RModelHelper.AddLayerProperties(NullLines, layerTable, nullLinesLayerGuid);

			Dictionary<string, Guid> floorLayerGuidAss = F2RModelHelper.AddLayerProperties(FloorAreas, layerTable, floorLayerGuid);
			Dictionary<string, Guid> wallsLayerGuidAss = F2RModelHelper.AddLayerProperties(WallAreas, layerTable, wallsLayerGuid);
			Dictionary<string, Guid> nullAreaLayerGuidAss = F2RModelHelper.AddLayerProperties(NullAreas, layerTable, nullAreasLayerGuid);

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

			#region Beams

			for (int i = 0; i < Frames.Length; i++)
			{
				Frame frame = (Frame)Frames[i];
				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					objAtt.SetUserString("F2R_ID", frame.Id);
					objAtt.SetUserString("F2R_FRAMETYPE", "Beam");
					objAtt.SetUserString("F2R_GUID", frame.Guid.ToString());
					objAtt.SetUserString("F2R_JOINT1", frame.StartNode.Id);
					objAtt.SetUserString("F2R_JOINT2", frame.EndNode.Id);
					objAtt.SetUserString("F2R_PROPERTY", frame.Property.Name);
					objAtt.SetUserString("F2R_PROPERTY_MATERIAL", frame.Property.Material.Name);

					if (frame.Groups.Count > 0)
					{
						for (int j = 0; j < frame.Groups.Count; j++)
						{
							objAtt.SetUserString("F2R_GROUP_" + frameGroupsDictionary[frame.Groups[j].Name], frame.Groups[j].Name);
						}
					}

					objAtt.LayerIndex = layerTable.FindId(beamLayerGuidAss[frame.Property.Name]).Index;

					doc.Objects.AddLine(frame.Line, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake beams {frame.Id}. Exception: {ex}");
				}
			}

			#endregion

			#region Columns

			for (int i = 0; i < Columns.Length; i++)
			{
				Frame column = (Frame)Columns[i];
				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					objAtt.SetUserString("F2R_ID", column.Id);
					objAtt.SetUserString("F2R_FRAMETYPE", "Column");
					objAtt.SetUserString("F2R_GUID", column.Guid.ToString());
					objAtt.SetUserString("F2R_JOINT1", column.StartNode.Id);
					objAtt.SetUserString("F2R_JOINT2", column.EndNode.Id);
					objAtt.SetUserString("F2R_PROPERTY", column.Property.Name);
					objAtt.SetUserString("F2R_PROPERTY_MATERIAL", column.Property.Material.Name);

					if (column.Groups.Count > 0)
					{
						for (int j = 0; j < column.Groups.Count; j++)
						{
							objAtt.SetUserString("F2R_GROUP_" + frameGroupsDictionary[column.Groups[j].Name], column.Groups[j].Name);
						}
					}

					objAtt.LayerIndex = layerTable.FindId(columnLayerGuidAss[column.Property.Name]).Index;

					doc.Objects.AddLine(column.Line, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake frame {column.Id}. Exception: {ex}");
				}
			}

			#endregion

			#region Braces

			for (int i = 0; i < Braces.Length; i++)
			{
				Frame brace = (Frame)Braces[i];
				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					objAtt.SetUserString("F2R_ID", brace.Id);
					objAtt.SetUserString("F2R_FRAMETYPE", "Brace");
					objAtt.SetUserString("F2R_GUID", brace.Guid.ToString());
					objAtt.SetUserString("F2R_JOINT1", brace.StartNode.Id);
					objAtt.SetUserString("F2R_JOINT2", brace.EndNode.Id);
					objAtt.SetUserString("F2R_PROPERTY", brace.Property.Name);
					objAtt.SetUserString("F2R_PROPERTY_MATERIAL", brace.Property.Material.Name);

					if (brace.Groups.Count > 0)
					{
						for (int j = 0; j < brace.Groups.Count; j++)
						{
							objAtt.SetUserString("F2R_GROUP_" + frameGroupsDictionary[brace.Groups[j].Name], brace.Groups[j].Name);
						}
					}

					objAtt.LayerIndex = layerTable.FindId(bracesLayerGuidAss[brace.Property.Name]).Index;

					doc.Objects.AddLine(brace.Line, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake frame {brace.Id}. Exception: {ex}");
				}
			}

			#endregion

			#region Null Lines

			for (int i = 0; i < NullLines.Length; i++)
			{
				Frame nullLine = (Frame)NullLines[i];
				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();
					objAtt.SetUserString("F2R_ID", nullLine.Id);
					objAtt.SetUserString("F2R_FRAMETYPE", "Null");
					objAtt.SetUserString("F2R_GUID", nullLine.Guid.ToString());
					objAtt.SetUserString("F2R_JOINT1", nullLine.StartNode.Id);
					objAtt.SetUserString("F2R_JOINT2", nullLine.EndNode.Id);
					objAtt.SetUserString("F2R_PROPERTY", nullLine.Property.Name);
					objAtt.SetUserString("F2R_PROPERTY_MATERIAL", nullLine.Property.Material.Name);

					if (nullLine.Groups.Count > 0)
					{
						for (int j = 0; j < nullLine.Groups.Count; j++)
						{
							objAtt.SetUserString("F2R_GROUP_" + frameGroupsDictionary[nullLine.Groups[j].Name], nullLine.Groups[j].Name);
						}
					}

					objAtt.LayerIndex = layerTable.FindId(nullLineLayerGuidAss[nullLine.Property.Name]).Index;

					doc.Objects.AddLine(nullLine.Line, objAtt);
				}
				catch (Exception ex)
				{
					_log.Add($"Fail to bake frame {nullLine.Id}. Exception: {ex}");
				}
			}

			#endregion

			#region Floor

			for (int i = 0; i < FloorAreas.Length; i++)
			{
				Area area = (Area)FloorAreas[i];

				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();

					objAtt.SetUserString("F2R_ID", area.Id);
					objAtt.SetUserString("F2R_GUID", area.Guid.ToString());
					objAtt.SetUserString("F2R_AREATYPE", PlateProperty.PropertyTypes.Floor.ToString());
					objAtt.SetUserString("F2R_PROPERTY", area.Property.Name);
					objAtt.SetUserString("F2R_PROPERTYTYPE", area.Property.PropertyType.ToString());
					objAtt.SetUserString("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
					objAtt.SetUserString("F2R_ISOPENING", area.Property.IsOpening.ToString());

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

					objAtt.LayerIndex = layerTable.FindId(floorLayerGuidAss[area.Property.Name]).Index;

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

			#region Wall

			for (int i = 0; i < WallAreas.Length; i++)
			{
				Area area = (Area)WallAreas[i];

				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();

					objAtt.SetUserString("F2R_ID", area.Id);
					objAtt.SetUserString("F2R_GUID", area.Guid.ToString());
					objAtt.SetUserString("F2R_AREATYPE", PlateProperty.PropertyTypes.Wall.ToString());
					objAtt.SetUserString("F2R_PROPERTY", area.Property.Name);
					objAtt.SetUserString("F2R_PROPERTYTYPE", area.Property.PropertyType.ToString());
					objAtt.SetUserString("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
					objAtt.SetUserString("F2R_ISOPENING", area.Property.IsOpening.ToString());

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

					objAtt.LayerIndex = layerTable.FindId(wallsLayerGuidAss[area.Property.Name]).Index;

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

			#region Null Areas

			for (int i = 0; i < NullAreas.Length; i++)
			{
				Area area = (Area)NullAreas[i];

				try
				{
					ObjectAttributes objAtt = new ObjectAttributes();

					objAtt.SetUserString("F2R_ID", area.Id);
					objAtt.SetUserString("F2R_GUID", area.Guid.ToString());
					objAtt.SetUserString("F2R_AREATYPE", PlateProperty.PropertyTypes.Null.ToString());
					objAtt.SetUserString("F2R_PROPERTY", area.Property.Name);
					objAtt.SetUserString("F2R_PROPERTYTYPE", area.Property.PropertyType.ToString());
					objAtt.SetUserString("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
					objAtt.SetUserString("F2R_ISOPENING", area.Property.IsOpening.ToString());

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

					objAtt.LayerIndex = layerTable.FindId(nullAreaLayerGuidAss[area.Property.Name]).Index;

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
