using Fem2Rhino.Common;
using Fem2Rhino.CSI.ModelWrapper;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using St7API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fem2Rhino.Straus7
{
    public class StrausModel : Model
    {
        protected readonly int _units;
        protected readonly bool _visibile;
        protected (int, int, int) _version;

        protected readonly int _modelId = 1;


        public StrausModel(int units = 10, string filePath = "", bool visible = true)
            : base(filePath)
        {
            _units = units;
            _visibile = visible;
        }

        public override void Process()
        {
            try
            {
                InizializeAPI();
            }
            catch (Exception)
            {
                St7.St7CloseFile(_modelId);
                return;
            }

            try
            {
                ReadModel();
            }
            catch (Exception)
            {
                St7.St7CloseFile(_modelId);
                return;
            }

            try
            {
                CloseModel();
            }
            catch (Exception)
            {
                St7.St7CloseFile(_modelId);
                return;
            }
        }

        protected override void InizializeAPI()
        {
            if (HandleError(St7.St7Init()))
                return;

            if (HandleError(St7.St7OpenFileReadOnly(_modelId, _filePath, Path.GetTempPath())))
                return;

            if (HandleError(St7.St7SetUnits(_modelId, new int[] { 2, 1, 2, 0, 0, 1 })))
                return;

            int versionMajor = 0;
            int versionMinor = 0;
            int versionPoint = 0;
            St7.St7Version(ref versionMajor, ref versionMinor, ref versionPoint);

            _version = (versionMajor, versionMinor, versionPoint);
        }

        protected override void ReadModel()
        {
            ReadJoint();
            ReadFrame();
            ReadArea();
        }

        protected override void ReadJoint()
        {
            int iErr = 1;

            int nodesTotal = 0;

            iErr = St7.St7GetTotal(_modelId, St7.tyNODE, ref nodesTotal);

            if (nodesTotal > 0)
            {
                _joints = new Joint[nodesTotal];
                for (int i = 1; i <= nodesTotal; i++)
                {
                    int nodeId = -1;
                    if (HandleError(St7.St7GetNodeID(_modelId, i, ref nodeId)))
                        return;

                    double[] coordinates = new double[3];
                    if (HandleError(St7.St7GetNodeXYZ(_modelId, i, coordinates)))
                        return;

                    Joint joint = new Joint(coordinates[0], coordinates[1], coordinates[2], nodeId.ToString()) { Number = i };

                    if (Option.ExportGeometryDatas)
                    {
                        /*
						string[] groups = new string[0];
						int numberGroups = 0;
						ret = _apiWrapper.GetPointGroupAssign(jointName, ref numberGroups, ref groups);
						joint.Groups = Enumerable.Range(0, groups.Length).Select(i => new Common.Group(groups[i])).ToList();

						bool[] restraint = new bool[0];
						ret = _apiWrapper.GetPointRestraint(jointName, ref restraint);
						joint.Restraints = restraint;*/

                    }

                    #region Point Load

                    if (Option.ExportLoads)
                    {
                        /*
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
						}*/
                    }

                    _joints[i - 1] = joint;

                    #endregion
                }
            }
        }

        protected override void ReadFrame()
        {
            List<string> errorBeams = new List<string>();
            int beamsTotal = 0;
            if (HandleError(St7.St7GetTotal(_modelId, St7.tyBEAM, ref beamsTotal)))
                return;

            if (beamsTotal > 0)
            {
                _frames = new Beam[beamsTotal];

                int numGroups = 0;
                if (HandleError(St7.St7GetNumGroups(_modelId, ref numGroups)))
                    return;

                StringBuilder groupStringBuilder = new StringBuilder(St7.kMaxStrLen);
                StringBuilder propStringBuilder = new StringBuilder(St7.kMaxStrLen);
                StringBuilder matStringBuilder = new StringBuilder(St7.kMaxStrLen);

                for (int i = 1; i <= beamsTotal; i++)
                {
                    int beamId = -1;
                    if (HandleError(St7.St7GetBeamID(_modelId, i, ref beamId)))
                        return;

                    int[] connection = new int[3];
                    if (HandleError(St7.St7GetElementConnection(_modelId, St7.tyBEAM, i, connection)))
                        return;

                    Joint joint1 = _joints.Where(j => j.Number == connection[1]).FirstOrDefault();
                    Joint joint2 = _joints.Where(j => j.Number == connection[2]).FirstOrDefault();

                    Frame f = new Frame(joint1, joint2, beamId.ToString()) { Number = i };

                    #region Section

                    if (Option.ExportSectionProperties)
                    {
                        int groupId = 0;
                        if (HandleError(St7.St7GetEntityGroup(_modelId, St7.tyBEAM, i, ref groupId)))
                            return;

                        if (HandleError(St7.St7GetGroupIDName(_modelId, groupId, groupStringBuilder, St7.kMaxStrLen)))
                            return;

                        try
                        {
                            int propNum = 0;
                            if (HandleError(St7.St7GetElementProperty(_modelId, St7.tyBEAM, i, ref propNum)))
                                return;

                            if (HandleError(St7.St7GetPropertyName(_modelId, St7.tyBEAM, propNum, propStringBuilder, St7.kMaxStrLen)))
                                return;

                            if (HandleError(St7.St7GetMaterialName(_modelId, St7.tyBEAM, propNum, matStringBuilder, St7.kMaxStrLen)))
                                return;

                            f.Property = new FrameProperty(propStringBuilder.ToString(), new Common.Material() { Name = matStringBuilder.ToString() });
                        }
                        catch (Exception)
                        {
                            f.Property = new FrameProperty("Error", new Common.Material() { Name = "Error" });
                        }

                        /*
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
						}*/
                    }

                    #endregion

                    if (Option.ExportGeometryDatas)
                    {/*
						#region Local Axis

						double angle = 0;
						bool advancedLocalAxis = false;
						ret = _apiWrapper.GetFrameLocalAxis(frameName, ref angle, ref advancedLocalAxis);
						frame.Angle = angle;
						frame.AdvancedLocalAxis = advancedLocalAxis;
						if (advancedLocalAxis)
						{
							bool active = false; int plane2 = 0; int piVectOut = 0; string piCoordSys = string.Empty;
							int[] piDirect = new int[0]; string[] piPt = new string[0]; double[] piVect = new double[0];

							ret = _apiWrapper.GetFrameLocalAxesAdvanced(frameName, ref active, ref plane2, ref piVectOut, ref piCoordSys, ref piDirect, ref piPt, ref piVect);

							if (active)
							{
								if (piVectOut == 2) //Two joints
								{
									string node1 = piPt[0];
									string node2 = piPt[1];

									Joint j1 = _joints.Where(j => j.Id == node1).FirstOrDefault();
									Joint j2 = _joints.Where(j => j.Id == node2).FirstOrDefault();

									if (j1 != null && j2 != null)
									{
										double deltax = j2.Location.X - j1.Location.X;
										double deltay = j2.Location.Y - j1.Location.Y;

										double angleRad = Math.Atan2(deltay, deltax);
										frame.AdvancedLocalAngle = angleRad * 180 / Math.PI;

										if (plane2 == 13)
											frame.AdvancedLocalAngle -= 90;
									}
								}
							}
						}
						else
						{
							frame.AdvancedLocalAngle = 0;
						}

						#endregion

						#region Groups

						int numberGroup = -1;
						string[] groups = new string[1];

						ret = _apiWrapper.GetFrameGroupAssign(frameName, ref numberGroup, ref groups);
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
						ret = _apiWrapper.GetFrameInsertionPoint(frameName, ref CardinalPoint, ref Mirror2, ref Mirror3, ref StiffTransform, ref Offset1, ref Offset2, ref CSys);

						InsertionPoint insertionPoint = new InsertionPoint()
						{
							CardinalPoint = (InsertionPoint.CardinalPoints)CardinalPoint,
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

						Releases release = new Releases()
						{
							IEndReleases = ii,
							JEndReleases = jj,
							IEndPartialFixity = StartValue,
							JEndPartialFixity = EndValue,
						};

						frame.Releases = release;

						#endregion

						#region Overwrite

						DesignSteelOverwrite designSteelOverwrite = new DesignSteelOverwrite();

						double value = 0.0;
						bool ProgramDetermined = false;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.UnbracedLengthRatioMajor, ref value, ref ProgramDetermined);
						designSteelOverwrite.UnbracedLengthRatioMajor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.UnbracedLengthRatioMinor, ref value, ref ProgramDetermined);
						designSteelOverwrite.UnbracedLengthRatioMinor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.UnbracedLengthRatioLateralTorsionalBuckling, ref value, ref ProgramDetermined);

						designSteelOverwrite.UnbracedLengthRatioLateralTorsionalBuckling = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.EffectiveLengthFactorK1Major, ref value, ref ProgramDetermined);
						designSteelOverwrite.EffectiveLengthFactorK1Major = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.EffectiveLengthFactorK1Minor, ref value, ref ProgramDetermined);
						designSteelOverwrite.EffectiveLengthFactorK1Minor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.EffectiveLengthFactorK2Major, ref value, ref ProgramDetermined);
						designSteelOverwrite.EffectiveLengthFactorK2Major = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.EffectiveLengthFactorK2Minor, ref value, ref ProgramDetermined);
						designSteelOverwrite.EffectiveLengthFactorK2Minor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.EffectiveLengthFactorKLateralTorsionalBuckling, ref value, ref ProgramDetermined);

						designSteelOverwrite.EffectiveLengthFactorKLateralTorsionalBuckling = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.MomentCoefficientCmMajor, ref value, ref ProgramDetermined);
						designSteelOverwrite.MomentCoefficientCmMajor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.MomentCoefficientCmMinor, ref value, ref ProgramDetermined);
						designSteelOverwrite.MomentCoefficientCmMinor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.BendingCoefficientCb, ref value, ref ProgramDetermined);

						designSteelOverwrite.BendingCoefficientCb = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.NonswayMomentFactorB1Major, ref value, ref ProgramDetermined);
						designSteelOverwrite.NonswayMomentFactorB1Major = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.NonswayMomentFactorB1Minor, ref value, ref ProgramDetermined);
						designSteelOverwrite.NonswayMomentFactorB1Minor = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.SwayMomentFactorB2Major, ref value, ref ProgramDetermined);
						designSteelOverwrite.SwayMomentFactorB2Major = (!ProgramDetermined) ? value : 0;
						ret = _apiWrapper.GetDesignSteelOverwrite(frameName, ApiWrapper.ApiWrapper.DesignSteelOverwrite.SwayMomentFactorB2Minor, ref value, ref ProgramDetermined);
						designSteelOverwrite.SwayMomentFactorB2Minor = (!ProgramDetermined) ? value : 0;

						frame.DesignSteelOverwrite = designSteelOverwrite;



						#endregion

						#region Transfer

						bool transfer = false;
						ret = _apiWrapper.GetFrameGetLoadTransfer(frameName, ref transfer);

						frame.LoadTransfer = transfer;

						#endregion*/
                    }

                    if (Option.ExportLoads)
                    {/*
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

						#endregion*/
                    }

                    _frames[i - 1] = f;
                }
            }
        }

        protected override void ReadArea()
        {
            int plateTotal = 0;
            if (HandleError(St7.St7GetTotal(_modelId, St7.tyPLATE, ref plateTotal)))
                return;

            if (plateTotal > 0)
            {
                _areas = new Plate[plateTotal];

                StringBuilder propStringBuilder = new StringBuilder(St7.kMaxStrLen);
                StringBuilder matStringBuilder = new StringBuilder(St7.kMaxStrLen);

                for (int i = 1; i <= plateTotal; i++)
                {
                    int plateId = -1;
                    if (HandleError(St7.St7GetPlateID(_modelId, i, ref plateId)))
                        return;

                    int[] connection = new int[5];
                    if (HandleError(St7.St7GetElementConnection(_modelId, St7.tyPLATE, i, connection)))
                        return;

                    Joint[] joints = new Joint[connection[0]];
                    for (int j = 0; j < connection[0]; j++)
                        joints[j] = _joints.Where(k => k.Number == connection[j + 1]).FirstOrDefault();

                    Area areaToAdd = new Area(plateId.ToString(), Guid.NewGuid(), joints) { Number = i };

                    if (Option.ExportSectionProperties)
                    {
                        int propNum = 0;
                        if (HandleError(St7.St7GetElementProperty(_modelId, St7.tyPLATE, i, ref propNum)))
                            return;

                        if (HandleError(St7.St7GetPropertyName(_modelId, St7.tyPLATE, propNum, propStringBuilder, St7.kMaxStrLen)))
                            return;

                        if (HandleError(St7.St7GetMaterialName(_modelId, St7.tyPLATE, propNum, matStringBuilder, St7.kMaxStrLen)))
                            return;

                        areaToAdd.Property = new AreaProperty(propStringBuilder.ToString(), new Common.Material() { Name = matStringBuilder.ToString() });

                        //string property = string.Empty;
                        //ret = _apiWrapper.GetAreaProperty(areaName, ref property);

                        //if (property == "None")
                        //{
                        //	area.Property = new AreaProperty(true) { Name = "None" };
                        //}
                        //else
                        //{
                        //	area.Property = new AreaProperty { Name = property };
                        //}
                    }

                    /*
					if (Option.ExportGeometryDatas)
					{
						int numberGroup = -1;
						string[] stringGroup = new string[1];
						ret = _apiWrapper.GetAreaGroupAssign(areaName, ref numberGroup, ref stringGroup);

						List<Common.Group> _group = Enumerable.Range(0, stringGroup.Length).Select(i => new Common.Group(stringGroup[i])).ToList();
						area.Groups = _group;
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
					}*/

                    _areas[i - 1] = areaToAdd;
                }
            }
        }

        protected override void CloseModel()
        {
            if (HandleError(St7.St7CloseFile(_modelId)))
                return;
        }

        #region PREVIEW DATA

        public override bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            BakeGeometryCustom(doc);

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
                ObjectAttributes objAtt = new ObjectAttributes();
                objAtt.SetUserString("F2R_NUMBER", joint.Number.ToString());
                objAtt.SetUserString("F2R_ID", joint.Id);
                objAtt.SetUserString("F2R_GUID", joint.Guid.ToString());

                if (Option.ExportSectionProperties)
                {
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

            #endregion

            #region Frames

            for (int i = 0; i < Frames.Length; i++)
            {
                Frame frame = (Frame)Frames[i];
                ObjectAttributes objAtt = new ObjectAttributes();
                objAtt.SetUserString("F2R_ID", frame.Id);
                objAtt.SetUserString("F2R_NUMBER", frame.Number.ToString());
                objAtt.SetUserString("F2R_GUID", frame.Guid.ToString());
                objAtt.SetUserString("F2R_JOINT1", frame.StartNode.Number.ToString());
                objAtt.SetUserString("F2R_JOINT2", frame.EndNode.Number.ToString());

                if (Option.ExportSectionProperties)
                {
                    objAtt.SetUserString("F2R_PROPERTY", frame.Property.Name);

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

                        objAtt.SetUserString("F2R_OVERWRITE_UNBRACEDLENGTH", frame.DesignSteelOverwrite.UnbracedLengthRatioMajor.ToString() + ";" +
                            frame.DesignSteelOverwrite.UnbracedLengthRatioMinor.ToString() + ";" + frame.DesignSteelOverwrite.UnbracedLengthRatioLateralTorsionalBuckling.ToString());

                        objAtt.SetUserString("F2R_OVERWRITE_EFFECTIVELENGTHFACTOR", frame.DesignSteelOverwrite.EffectiveLengthFactorK1Major.ToString() + ";" +
                            frame.DesignSteelOverwrite.EffectiveLengthFactorK1Minor.ToString() + ";" + frame.DesignSteelOverwrite.EffectiveLengthFactorK2Major.ToString() + ";" +
                            frame.DesignSteelOverwrite.EffectiveLengthFactorK1Minor.ToString() + ";" + frame.DesignSteelOverwrite.EffectiveLengthFactorKLateralTorsionalBuckling.ToString());

                        objAtt.SetUserString("F2R_OVERWRITE_MOMENTCOEFFICIENT", frame.DesignSteelOverwrite.MomentCoefficientCmMajor.ToString() + ";" +
                            frame.DesignSteelOverwrite.MomentCoefficientCmMinor.ToString() + ";" + frame.DesignSteelOverwrite.BendingCoefficientCb.ToString());

                        objAtt.SetUserString("F2R_OVERWRITE_NONSWAYMOMENTFACTOR", frame.DesignSteelOverwrite.NonswayMomentFactorB1Major.ToString() + ";" +
                            frame.DesignSteelOverwrite.NonswayMomentFactorB1Minor.ToString());
                        objAtt.SetUserString("F2R_OVERWRITE_SWAYMOMENTFACTOR", frame.DesignSteelOverwrite.SwayMomentFactorB2Major.ToString() + ";" +
                            frame.DesignSteelOverwrite.SwayMomentFactorB2Minor.ToString());

                        objAtt.SetUserString("F2R_LOADTRANSFER", frame.LoadTransfer.ToString());

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

            #endregion

            #region Areas

            for (int i = 0; i < Areas.Length; i++)
            {
                Area area = (Area)Areas[i];
                ObjectAttributes objAtt = new ObjectAttributes();
                objAtt.SetUserString("F2R_ID", area.Id);
                objAtt.SetUserString("F2R_GUID", area.Guid.ToString());
                objAtt.SetUserString("F2R_NUMBER", area.Number.ToString());
                objAtt.SetUserString("F2R_VERTICESCOUNT", area.GetPoints.Length.ToString());
                for (int n = 0; n < area.GetPoints.Length; n++)
                    objAtt.SetUserString($"F2R_JOINT{n + 1}", area.GetPoints[n].Number.ToString());

                if (Option.ExportSectionProperties)
                {
                    objAtt.SetUserString("F2R_PROPERTY", area.Property.Name);
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

            #endregion
        }

        #endregion

        protected bool HandleError(int errorCode)
        {
            StringBuilder sbErrorString;

            if (errorCode != St7.ERR7_NoError) // Returned an error
            {
                Exception exception = null;

                sbErrorString = new StringBuilder(St7.kMaxStrLen);
                if (St7.ERR7_NoError == St7.St7GetAPIErrorString(errorCode, sbErrorString, sbErrorString.Capacity))
                {
                    exception = new Exception(sbErrorString.ToString());
                }
                else if (St7.ERR7_NoError == St7.St7GetSolverErrorString(errorCode, sbErrorString, sbErrorString.Capacity))
                {
                    exception = new Exception(sbErrorString.ToString());
                }
                if (exception != null)
                {
                    exception.Data.Add("ErrorCode", errorCode);
                    throw exception;
                }
                return true;
            }
            else // No errors
            {
                return false;
            }
        }
    }
}
