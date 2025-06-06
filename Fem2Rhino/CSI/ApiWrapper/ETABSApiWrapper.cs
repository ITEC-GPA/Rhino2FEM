using ETABSv1;
using System;

namespace Fem2Rhino.CSI.ApiWrapper
{
	public class ETABSApiWrapper : CSIApiWrapper
	{
		private cOAPI _etabsObject;

		public ETABSApiWrapper()
		{

		}

		public int InizializeAPI(bool attachToInstance = false, string sapExePath = null, int units = Units.N_m_C, bool visible = true, string filePath = "")
		{
			_attachToInstance = attachToInstance;

			cHelper myHelper; //create API helper object
			try
			{
				myHelper = new Helper();
			}
			catch
			{
				throw;
			}

			if (_attachToInstance)
			{
				try
				{
					// Get the active SapObject
					_etabsObject = myHelper.GetObject("CSI.ETABS.API.ETABSObject");
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{

				if (!string.IsNullOrEmpty(sapExePath)) //create an instance of the SapObject from the specified path
				{
					try
					{
						_etabsObject = myHelper.CreateObject(sapExePath);                        //create SapObject
					}
					catch
					{
						throw;
					}
				}
				else   //create an instance of the SapObject from the latest installed SAP2000
				{
					try
					{
						// Get the active SapObject
						_etabsObject = myHelper.CreateObjectProgID("CSI.ETABS.API.ETABSObject");
					}
					catch
					{
						throw;
					}
				}
				if (_etabsObject != null)
					_apiVersion = _etabsObject.GetOAPIVersionNumber();

				if (_etabsObject != null)
				{
					return ApplicationStart(units, filePath);
				}
				else
					return 1;
			}

			return 0;
		}

		#region GENERAL FUNCTIONS

		/// <summary>
		/// 
		/// </summary>
		/// <param name="units">The database units used when a new model is created. Data is internally stored in the program in the database units. The database units may be one of the following items in the eUnits enumeration:</param>
		/// <param name="visible">If this item is True then the application is visible when started.  If it is False then the application is hidden when started.</param>
		/// <param name="FilePath">The full path of a model file to be opened when the Sap2000 application is started. If no file name is specified, the application starts without loading an existing model.</param>
		private int ApplicationStart(int units = Units.N_m_C, bool visible = true, string FilePath = "")
		{
			try
			{
				eUnits _units = (eUnits)units;

				if (_etabsObject.ApplicationStart() != 0)
					return 1;
				if (_etabsObject.SapModel.InitializeNewModel(_units) != 0)
					return 1;

				if (visible)
				{
					if (_etabsObject.Unhide() != 0)
						return 1;
				}
				else
				{
					if (_etabsObject.Hide() != 0)
						return 1;
				}

				if (!string.IsNullOrEmpty(FilePath))
					_etabsObject.SapModel.File.OpenFile(FilePath);

				return 0;
			}
			catch
			{
				return -1;
			}
		}

		private int ApplicationStart(int units = Units.N_m_C, string FilePath = "")
		{
			int ret;
			try
			{
				eUnits _units = (eUnits)units;

				ret = _etabsObject.ApplicationStart();
				if (ret == 0)
					ret = _etabsObject.SapModel.File.OpenFile(FilePath);
				if (ret == 0)
					ret = _etabsObject.SapModel.SetPresentUnits(_units);

				return ret;
			}
			catch
			{
				return -1;
			}
		}

		public int ApplicationExit()
		{
			int ret = 1;
			if (_etabsObject != null)
				ret = _etabsObject.ApplicationExit(false);
			return ret;
		}

		public Tuple<string, double> GetVersion()
		{
			string version = "";
			double myVersionNumber = -1;
			_etabsObject.SapModel.GetVersion(ref version, ref myVersionNumber);

			return new Tuple<string, double>(version, myVersionNumber);
		}

		public int SetPresentUnits(int units)
		{
			eUnits _units = (eUnits)units;
			int ret = _etabsObject.SapModel.SetPresentUnits(_units);

			return ret;
		}

		#endregion

		#region OBJECT MODEL

		#region POINT

		public int GetCoordCartesian(string name, ref double x, ref double y, ref double z)
		{
			return _etabsObject.SapModel.PointObj.GetCoordCartesian(name, ref x, ref y, ref z);
		}
		public int GetPointGuid(string pointName, ref string guid)
		{
			return _etabsObject.SapModel.PointObj.GetGUID(pointName, ref guid);
		}
		public int GetPointNameList(ref int numberNames, ref string[] names)
		{
			return _etabsObject.SapModel.PointObj.GetNameList(ref numberNames, ref names);
		}
		public int GetPointGroupAssign(string pointName, ref int numberGroups, ref string[] groups)
		{
			return _etabsObject.SapModel.PointObj.GetGroupAssign(pointName, ref numberGroups, ref groups);
		}
		public int GetPointRestraint(string pointName, ref bool[] value)
		{
			return _etabsObject.SapModel.PointObj.GetRestraint(pointName, ref value);
		}
		public int GetPointSpecialJoint(string pointName, ref bool specialJoint)
		{
			return _etabsObject.SapModel.PointObj.GetSpecialPoint(pointName, ref specialJoint);
		}
		public int GetPointLoadForce(string pointName, ref int NumberItems, ref string[] PointName, ref string[] LoadPat, ref int[] LCStep, ref string[] CSys, ref double[] F1,
			 ref double[] F2, ref double[] F3, ref double[] M1, ref double[] M2, ref double[] M3, eItemType ItemType = eItemType.Objects)
		{
			return _etabsObject.SapModel.PointObj.GetLoadForce(pointName, ref NumberItems, ref PointName, ref LoadPat, ref LCStep, ref CSys, ref F1,
				ref F2, ref F3, ref M1, ref M2, ref M3, ItemType);
		}

		#endregion

		#region FRAME

		public int GetFrameNameList(ref int numberNames, ref string[] names)
		{
			return _etabsObject.SapModel.FrameObj.GetNameList(ref numberNames, ref names);
		}
		public int GetFrameGuid(string framename, ref string guid)
		{
			return _etabsObject.SapModel.FrameObj.GetGUID(framename, ref guid);
		}
		public int GetFrameEndPoints(string framename, ref string point1, ref string point2)
		{
			return _etabsObject.SapModel.FrameObj.GetPoints(framename, ref point1, ref point2);
		}
		public int GetFrameSection(string frameName, ref string sectionName, ref string autoSelectList)
		{
			return _etabsObject.SapModel.FrameObj.GetSection(frameName, ref sectionName, ref autoSelectList);
		}
		public int GetFrameSectionNonPrismatic(string frameName, ref string sectionName, ref double sVarTotalLength, ref double sVarRelStartLoc)
		{
			return _etabsObject.SapModel.FrameObj.GetSectionNonPrismatic(frameName, ref sectionName, ref sVarTotalLength, ref sVarRelStartLoc);
		}
		public int GetFrameGroupAssign(string frameName, ref int numberGroups, ref string[] groups)
		{
			return _etabsObject.SapModel.FrameObj.GetGroupAssign(frameName, ref numberGroups, ref groups);
		}
		public int GetFrameLocalAxis(string frameName, ref double angle, ref bool advanced)
		{
			return _etabsObject.SapModel.FrameObj.GetLocalAxes(frameName, ref angle, ref advanced);
		}
		//public int GetFrameLocalAxesAdvanced(string frameName, ref bool active, ref int plane2, ref int plVectOpt, ref string plCSys, ref int[] plDir, ref string[] plPt, ref double[] plVect)
		//{
		//	return _etabsObject.SapModel.FrameObj.GetLocalAxesAdvanced(frameName, ref active, ref plane2, ref plVectOpt, ref plCSys, ref plDir, ref plPt, ref plVect);
		//}
		public int GetFrameInsertionPoint(string frameName, ref int CardinalPoint, ref bool Mirror2, ref bool StiffTransform,
			ref double[] Offset1, ref double[] Offset2, ref string CSys)
		{
			return _etabsObject.SapModel.FrameObj.GetInsertionPoint(frameName, ref CardinalPoint, ref Mirror2, ref StiffTransform, ref Offset1, ref Offset2, ref CSys);
		}
		public int GetFrameLoadDistributed(string frameName, ref int NumberItems, ref string[] FrameName, ref string[] LoadPat, ref int[] MyType, ref string[] CSys, ref int[] Dir,
			ref double[] RD1, ref double[] RD2, ref double[] Dist1, ref double[] Dist2, ref double[] Val1, ref double[] Val2, eItemType ItemType = eItemType.Objects)
		{
			return _etabsObject.SapModel.FrameObj.GetLoadDistributed(frameName, ref NumberItems, ref FrameName, ref LoadPat, ref MyType, ref CSys, ref Dir,
				ref RD1, ref RD2, ref Dist1, ref Dist2, ref Val1, ref Val2, ItemType);
		}
		public int GetFrameLoadPoint(string frameName, ref int NumberItems, ref string[] FrameName, ref string[] LoadPat, ref int[] MyType, ref string[] CSys, ref int[] Dir,
			ref double[] RelDist, ref double[] Dist, ref double[] Val, eItemType ItemType = eItemType.Objects)
		{
			return _etabsObject.SapModel.FrameObj.GetLoadPoint(frameName, ref NumberItems, ref FrameName, ref LoadPat, ref MyType, ref CSys, ref Dir, ref RelDist, ref Dist, ref Val, ItemType);
		}
		public int GetFrameLoadTemperature(string name, ref int NumberItems, ref string[] FrameName, ref string[] LoadPat, ref int[] MyType, ref double[] Val,
			ref string[] PatternName, eItemType ItemType = eItemType.Objects)
		{
			return _etabsObject.SapModel.FrameObj.GetLoadTemperature(name, ref NumberItems, ref FrameName, ref LoadPat, ref MyType, ref Val, ref PatternName, ItemType);
		}
		public int GetFrameReleases(string frameName, ref bool[] ii, ref bool[] jj, ref double[] StartValue, ref double[] EndValue)
		{
			return _etabsObject.SapModel.FrameObj.GetReleases(frameName, ref ii, ref jj, ref StartValue, ref EndValue);
		}
		//public int GetFrameGetLoadTransfer(string frameName, ref bool val)
		//{
		//	return _etabsObject.SapModel.FrameObj.GetLoadTransfer(frameName, ref val);
		//}
		public int GetFramePropertyModifiers(string frameName, ref double[] val)
		{
			int ret = _etabsObject.SapModel.PropFrame.GetModifiers(frameName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		public int GetFrameDesignOrientation(string areaName, ref eFrameDesignOrientation eAreaDesignOrientation)
		{
			return _etabsObject.SapModel.FrameObj.GetDesignOrientation(areaName, ref eAreaDesignOrientation);
		}
		public int GetFrameModifiers(string frameName, ref double[] val)
		{
			int ret = _etabsObject.SapModel.FrameObj.GetModifiers(frameName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		//public int GetFrameAutomesh(string frameName, ref bool autoMesh, ref bool AutoMeshAtPoints, ref bool AutoMeshAtLines, ref int NumSegs, ref double AutoMeshMaxLength)
		//{
		//	return _etabsObject.SapModel.FrameObj.GetAutoMesh(frameName, ref autoMesh, ref AutoMeshAtPoints, ref AutoMeshAtLines, ref NumSegs, ref AutoMeshMaxLength);
		//}

		#endregion

		#region AREA

		public int GetAreaNameList(ref int numberNames, ref string[] names)
		{
			return _etabsObject.SapModel.AreaObj.GetNameList(ref numberNames, ref names);
		}
		public int GetAreaGuid(string framename, ref string guid)
		{
			return _etabsObject.SapModel.AreaObj.GetGUID(framename, ref guid);
		}
		public int GetAreaPoints(string areaName, ref int numberPoints, ref string[] pointNames)
		{
			return _etabsObject.SapModel.AreaObj.GetPoints(areaName, ref numberPoints, ref pointNames);
		}
		public int GetAreaProperty(string areaName, ref string propertyName)
		{
			return _etabsObject.SapModel.AreaObj.GetProperty(areaName, ref propertyName);
		}
		public int GetAreaDesignOrientation(string areaName, ref eAreaDesignOrientation eAreaDesignOrientation)
		{
			return _etabsObject.SapModel.AreaObj.GetDesignOrientation(areaName, ref eAreaDesignOrientation);
		}
		public int GetAreaGroupAssign(string areaName, ref int numberGroups, ref string[] groups)
		{
			return _etabsObject.SapModel.AreaObj.GetGroupAssign(areaName, ref numberGroups, ref groups);
		}
		public int GetAreaLoadUniform(string name, ref int NumberItems, ref string[] AreaName, ref string[] LoadPat, ref string[] CSys, ref int[] Dir, ref double[] Value,
			eItemType ItemType = eItemType.Objects)
		{
			return _etabsObject.SapModel.AreaObj.GetLoadUniform(name, ref NumberItems, ref AreaName, ref LoadPat, ref CSys, ref Dir, ref Value, ItemType);
		}
		public int GetAreaLoadUniformToFrame(string name, ref int NumberItems, ref string[] AreaName, ref string[] LoadPat, ref string[] CSys, ref int[] Dir, ref double[] Value,
			ref int[] DistType, eItemType ItemType = eItemType.Objects)
		{
			return _etabsObject.SapModel.AreaObj.GetLoadUniformToFrame(name, ref NumberItems, ref AreaName, ref LoadPat, ref CSys, ref Dir, ref Value, ref DistType, ItemType);
		}
		public int GetAreaPropertyModifiers(string areaPropName, ref double[] val)
		{
			int ret = _etabsObject.SapModel.PropArea.GetModifiers(areaPropName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		public int GetAreaModifiers(string areaName, ref double[] val)
		{
			int ret = _etabsObject.SapModel.AreaObj.GetModifiers(areaName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		public int GetAreaLocalAxes(string name, ref double angle, ref bool advanced)
		{
			return _etabsObject.SapModel.AreaObj.GetLocalAxes(name, ref angle, ref advanced);
		}
		public int GetAreaOpening(string name, ref bool IsOpening)
		{
			return _etabsObject.SapModel.AreaObj.GetOpening(name, ref IsOpening);
		}
		//public int GetAreaLocalAxesAdvanced(string frameName, ref bool active, ref int plane2, ref int plVectOpt, ref string plCSys, ref int[] plDir, ref string[] plPt, ref double[] plVect)
		//{
		//	return _etabsObject.SapModel.AreaObj.GetLocalAxesAdvanced(frameName, ref active, ref plane2, ref plVectOpt, ref plCSys, ref plDir, ref plPt, ref plVect);
		//}

		#endregion

		#endregion

		~ETABSApiWrapper()
		{
			try
			{
				if (_etabsObject != null)
				{
					try
					{
						if (!_attachToInstance)
							_etabsObject.ApplicationExit(false); // se falso non salva prima di chiudere i.e. non sovrascrive il file 
					}
					catch (Exception)
					{

					}
				}
			}
			finally
			{
				_etabsObject = null;
			}
		}
	}
}
