using CSiAPIv1;
using System;

namespace Fem2Rhino.CSI.ApiWrapper
{
	public class SAPApiWrapper : CSIApiWrapper
	{
		public enum DesignSteelOverwrite
		{
			UnbracedLengthRatioMajor = 18,
			UnbracedLengthRatioMinor = 19,
			UnbracedLengthRatioLateralTorsionalBuckling = 20,
			EffectiveLengthFactorK1Major = 21,
			EffectiveLengthFactorK1Minor = 22,
			EffectiveLengthFactorK2Major = 23,
			EffectiveLengthFactorK2Minor = 24,
			EffectiveLengthFactorKLateralTorsionalBuckling = 25,
			MomentCoefficientCmMajor = 26,
			MomentCoefficientCmMinor = 27,
			BendingCoefficientCb = 28,
			NonswayMomentFactorB1Major = 29,
			NonswayMomentFactorB1Minor = 30,
			SwayMomentFactorB2Major = 31,
			SwayMomentFactorB2Minor = 32,
		}

		private cOAPI _sapObject;

		public SAPApiWrapper()
		{

		}

		public int InizializeAPI(bool attachToInstance = false, string sapExePath = null, int units = Units.N_m_C, bool visible = true, string filePath = "")
		{
			_attachToInstance = attachToInstance;
			if (_attachToInstance)
			{
				try
				{
					// Get the active SapObject
					_sapObject = (cOAPI)StrausProxy.CSIActiveObject.GetActiveObject("CSI.SAP2000.API.SapObject");
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				cHelper myHelper; //create API helper object
				try
				{
					myHelper = new CSiAPIv1.Helper();
				}
				catch
				{
					throw;
				}

				if (!string.IsNullOrEmpty(sapExePath)) //create an instance of the SapObject from the specified path
				{
					try
					{
						_sapObject = myHelper.CreateObject(sapExePath);                        //create SapObject
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
						_sapObject = myHelper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
					}
					catch
					{
						throw;
					}
				}
				if (_sapObject != null)
					_apiVersion = _sapObject.GetOAPIVersionNumber();


				if (_sapObject != null)
				{
					return ApplicationStart(units, visible, filePath);
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
			int ret;
			try
			{
				eUnits _units = (eUnits)units;

				if (string.IsNullOrEmpty(FilePath))
					ret = _sapObject.ApplicationStart(_units, visible);
				else
					ret = _sapObject.ApplicationStart(_units, visible, FilePath);

				return ret;
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

				ret = _sapObject.ApplicationStart();
				if (ret == 0)
					ret = _sapObject.SapModel.File.OpenFile(FilePath);
				if (ret == 0)
					ret = _sapObject.SapModel.SetPresentUnits(_units);

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
			if (_sapObject != null)
				ret = _sapObject.ApplicationExit(false);
			return ret;
		}

		public Tuple<string, double> GetVersion()
		{
			string version = "";
			double myVersionNumber = -1;
			_sapObject.SapModel.GetVersion(ref version, ref myVersionNumber);

			return new Tuple<string, double>(version, myVersionNumber);
		}

		public int SetPresentUnits(int units)
		{
			eUnits _units = (eUnits)units;
			int ret = _sapObject.SapModel.SetPresentUnits(_units);

			return ret;
		}

		#endregion

		#region OBJECT MODEL

		#region POINT

		public int GetCoordCartesian(string name, ref double x, ref double y, ref double z)
		{
			return _sapObject.SapModel.PointObj.GetCoordCartesian(name, ref x, ref y, ref z);
		}
		public int GetPointGuid(string pointName, ref string guid)
		{
			return _sapObject.SapModel.PointObj.GetGUID(pointName, ref guid);
		}
		public int GetPointNameList(ref int numberNames, ref string[] names)
		{
			return _sapObject.SapModel.PointObj.GetNameList(ref numberNames, ref names);
		}
		public int GetPointGroupAssign(string pointName, ref int numberGroups, ref string[] groups)
		{
			return _sapObject.SapModel.PointObj.GetGroupAssign(pointName, ref numberGroups, ref groups);
		}
		public int GetPointRestraint(string pointName, ref bool[] value)
		{
			return _sapObject.SapModel.PointObj.GetRestraint(pointName, ref value);
		}
		public int GetPointSpecialJoint(string pointName, ref bool specialJoint)
		{
			return _sapObject.SapModel.PointObj.GetSpecialPoint(pointName, ref specialJoint);
		}
		public int GetPointLoadForce(string pointName, ref int NumberItems, ref string[] PointName, ref string[] LoadPat, ref int[] LCStep, ref string[] CSys, ref double[] F1,
			 ref double[] F2, ref double[] F3, ref double[] M1, ref double[] M2, ref double[] M3, eItemType ItemType = eItemType.Objects)
		{
			return _sapObject.SapModel.PointObj.GetLoadForce(pointName, ref NumberItems, ref PointName, ref LoadPat, ref LCStep, ref CSys, ref F1,
				ref F2, ref F3, ref M1, ref M2, ref M3, ItemType);
		}

		#endregion

		#region FRAME

		public int GetFrameNameList(ref int numberNames, ref string[] names)
		{
			return _sapObject.SapModel.FrameObj.GetNameList(ref numberNames, ref names);
		}
		public int GetFrameGuid(string framename, ref string guid)
		{
			return _sapObject.SapModel.FrameObj.GetGUID(framename, ref guid);
		}
		public int GetFrameEndPoints(string framename, ref string point1, ref string point2)
		{
			return _sapObject.SapModel.FrameObj.GetPoints(framename, ref point1, ref point2);
		}
		public int GetFrameSection(string frameName, ref string sectionName, ref string autoSelectList)
		{
			return _sapObject.SapModel.FrameObj.GetSection(frameName, ref sectionName, ref autoSelectList);
		}
		public int GetFrameSectionNonPrismatic(string frameName, ref string sectionName, ref double sVarTotalLength, ref double sVarRelStartLoc)
		{
			return _sapObject.SapModel.FrameObj.GetSectionNonPrismatic(frameName, ref sectionName, ref sVarTotalLength, ref sVarRelStartLoc);
		}
		public int GetFrameMass(string frameName, ref double MassOverL)
		{
			return _sapObject.SapModel.FrameObj.GetMass(frameName, ref MassOverL);
		}
		public int GetFrameSectionProps(string frameName, ref double Area, ref double As2, ref double As3, ref double Torsion, ref double I22, ref double I33, ref double S22,
			ref double S33, ref double Z22, ref double Z33, ref double R22, ref double R33)
		{
			return _sapObject.SapModel.PropFrame.GetSectProps(frameName, ref Area, ref As2, ref As3, ref Torsion, ref I22, ref I33, ref S22, ref S33, ref Z22, ref Z33, ref R22, ref R33); ;
		}
		public int GetFramePropNameInPropFile(string name, ref string nameInFile, ref string fileName, ref string matProp, ref eFramePropType propType)
		{
			return _sapObject.SapModel.PropFrame.GetNameInPropFile(name, ref nameInFile, ref fileName, ref matProp, ref propType);
		}
		public int GetFrameWeightAndMass(string materialName, ref double weight, ref double mass)
		{
			return _sapObject.SapModel.PropMaterial.GetWeightAndMass(materialName, ref weight, ref mass);
		}
		public int GetFrameGroupAssign(string frameName, ref int numberGroups, ref string[] groups)
		{
			return _sapObject.SapModel.FrameObj.GetGroupAssign(frameName, ref numberGroups, ref groups);
		}
		public int GetFrameLocalAxis(string frameName, ref double angle, ref bool advanced)
		{
			return _sapObject.SapModel.FrameObj.GetLocalAxes(frameName, ref angle, ref advanced);
		}
		public int GetFrameLocalAxesAdvanced(string frameName, ref bool active, ref int plane2, ref int plVectOpt, ref string plCSys, ref int[] plDir, ref string[] plPt, ref double[] plVect)
		{
			return _sapObject.SapModel.FrameObj.GetLocalAxesAdvanced(frameName, ref active, ref plane2, ref plVectOpt, ref plCSys, ref plDir, ref plPt, ref plVect);
		}
		public int GetFrameInsertionPoint(string frameName, ref int CardinalPoint, ref bool Mirror2, ref bool Mirror3, ref bool StiffTransform,
			ref double[] Offset1, ref double[] Offset2, ref string CSys)
		{
			return _sapObject.SapModel.FrameObj.GetInsertionPoint_1(frameName, ref CardinalPoint, ref Mirror2, ref Mirror3, ref StiffTransform, ref Offset1, ref Offset2, ref CSys);
		}
		public int GetFrameLoadDistributed(string frameName, ref int NumberItems, ref string[] FrameName, ref string[] LoadPat, ref int[] MyType, ref string[] CSys, ref int[] Dir,
			ref double[] RD1, ref double[] RD2, ref double[] Dist1, ref double[] Dist2, ref double[] Val1, ref double[] Val2, eItemType ItemType = eItemType.Objects)
		{
			return _sapObject.SapModel.FrameObj.GetLoadDistributed(frameName, ref NumberItems, ref FrameName, ref LoadPat, ref MyType, ref CSys, ref Dir,
				ref RD1, ref RD2, ref Dist1, ref Dist2, ref Val1, ref Val2, ItemType);
		}
		public int GetFrameLoadPoint(string frameName, ref int NumberItems, ref string[] FrameName, ref string[] LoadPat, ref int[] MyType, ref string[] CSys, ref int[] Dir,
			ref double[] RelDist, ref double[] Dist, ref double[] Val, eItemType ItemType = eItemType.Objects)
		{
			return _sapObject.SapModel.FrameObj.GetLoadPoint(frameName, ref NumberItems, ref FrameName, ref LoadPat, ref MyType, ref CSys, ref Dir, ref RelDist, ref Dist, ref Val, ItemType);
		}
		public int GetFrameLoadTemperature(string name, ref int NumberItems, ref string[] FrameName, ref string[] LoadPat, ref int[] MyType, ref double[] Val,
			ref string[] PatternName, eItemType ItemType = eItemType.Objects)
		{
			return _sapObject.SapModel.FrameObj.GetLoadTemperature(name, ref NumberItems, ref FrameName, ref LoadPat, ref MyType, ref Val, ref PatternName, ItemType);
		}
		public int GetFrameReleases(string frameName, ref bool[] ii, ref bool[] jj, ref double[] StartValue, ref double[] EndValue)
		{
			return _sapObject.SapModel.FrameObj.GetReleases(frameName, ref ii, ref jj, ref StartValue, ref EndValue);
		}
		public int GetFrameGetLoadTransfer(string frameName, ref bool val)
		{
			return _sapObject.SapModel.FrameObj.GetLoadTransfer(frameName, ref val);
		}
		public int GetFramePropertyModifiers(string frameName, ref double[] val)
		{
			int ret = _sapObject.SapModel.PropFrame.GetModifiers(frameName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		public int GetFrameModifiers(string frameName, ref double[] val)
		{
			int ret = _sapObject.SapModel.FrameObj.GetModifiers(frameName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		public int GetFrameAutomesh(string frameName, ref bool autoMesh, ref bool AutoMeshAtPoints, ref bool AutoMeshAtLines, ref int NumSegs, ref double AutoMeshMaxLength)
		{
			return _sapObject.SapModel.FrameObj.GetAutoMesh(frameName, ref autoMesh, ref AutoMeshAtPoints, ref AutoMeshAtLines, ref NumSegs, ref AutoMeshMaxLength);
		}

		#endregion

		#region AREA

		public int GetAreaNameList(ref int numberNames, ref string[] names)
		{
			return _sapObject.SapModel.AreaObj.GetNameList(ref numberNames, ref names);
		}
		public int GetAreaGuid(string framename, ref string guid)
		{
			return _sapObject.SapModel.AreaObj.GetGUID(framename, ref guid);
		}
		public int GetAreaPoints(string areaName, ref int numberPoints, ref string[] pointNames)
		{
			return _sapObject.SapModel.AreaObj.GetPoints(areaName, ref numberPoints, ref pointNames);
		}
		public int GetAreaProperty(string areaName, ref string propertyName)
		{
			return _sapObject.SapModel.AreaObj.GetProperty(areaName, ref propertyName);
		}
		public int GetAreaGroupAssign(string areaName, ref int numberGroups, ref string[] groups)
		{
			return _sapObject.SapModel.AreaObj.GetGroupAssign(areaName, ref numberGroups, ref groups);
		}
		public int GetAreaLoadUniform(string name, ref int NumberItems, ref string[] AreaName, ref string[] LoadPat, ref string[] CSys, ref int[] Dir, ref double[] Value,
			eItemType ItemType = eItemType.Objects)
		{
			return _sapObject.SapModel.AreaObj.GetLoadUniform(name, ref NumberItems, ref AreaName, ref LoadPat, ref CSys, ref Dir, ref Value, ItemType);
		}
		public int GetAreaLoadUniformToFrame(string name, ref int NumberItems, ref string[] AreaName, ref string[] LoadPat, ref string[] CSys, ref int[] Dir, ref double[] Value,
			ref int[] DistType, eItemType ItemType = eItemType.Objects)
		{
			return _sapObject.SapModel.AreaObj.GetLoadUniformToFrame(name, ref NumberItems, ref AreaName, ref LoadPat, ref CSys, ref Dir, ref Value, ref DistType, ItemType);
		}
		public int GetAreaPropertyModifiers(string areaPropName, ref double[] val)
		{
			if (areaPropName == "None")
			{
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
				return 0;
			}
			else
			{
				int ret = _sapObject.SapModel.PropArea.GetModifiers(areaPropName, ref val);
				if (ret == 1)
					val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
				return ret;
			}
		}
		public int GetAreaModifiers(string areaName, ref double[] val)
		{
			int ret = _sapObject.SapModel.AreaObj.GetModifiers(areaName, ref val);
			if (ret == 1)
				val = new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			return ret;
		}
		public int GetAreaLocalAxes(string name, ref double angle, ref bool advanced)
		{
			return _sapObject.SapModel.AreaObj.GetLocalAxes(name, ref angle, ref advanced);
		}
		public int GetAreaLocalAxesAdvanced(string frameName, ref bool active, ref int plane2, ref int plVectOpt, ref string plCSys, ref int[] plDir, ref string[] plPt, ref double[] plVect)
		{
			return _sapObject.SapModel.AreaObj.GetLocalAxesAdvanced(frameName, ref active, ref plane2, ref plVectOpt, ref plCSys, ref plDir, ref plPt, ref plVect);
		}

		#endregion

		#region DESIGN

		public int GetDesignSteelOverwrite(string frameName, int item, ref double value, ref bool programDetermined)
		{
			return _sapObject.SapModel.DesignSteel.AISC360_16.GetOverwrite(frameName, item, ref value, ref programDetermined);
		}

		public int GetDesignSteelOverwrite(string frameName, DesignSteelOverwrite item, ref double value, ref bool programDetermined)
		{
			return _sapObject.SapModel.DesignSteel.AISC360_16.GetOverwrite(frameName, (int)item, ref value, ref programDetermined);
		}

		#endregion

		#endregion

		~SAPApiWrapper()
		{
			try
			{
				if (_sapObject != null)
				{
					try
					{
						if (!_attachToInstance)
							_sapObject.ApplicationExit(false); // se falso non salva prima di chiudere i.e. non sovrascrive il file 
					}
					catch (Exception)
					{

					}
				}
			}
			finally
			{
				_sapObject = null;
			}
		}
	}
}
