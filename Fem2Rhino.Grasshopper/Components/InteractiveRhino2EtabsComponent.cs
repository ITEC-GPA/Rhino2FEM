using ETABSv1;

using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class InteractiveRhino2EtabsComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveRhino2EtabsComponent()
			: base("Rhino2ETABS", "R2S", "Update ETABS interactive model of current open istance", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVEETABS)
		{
		}

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Rhino2ETABS");
			buttonAttributes.ButtonPressed += () =>
			{
				_run = true;
				ExpireSolution(true);
			};

			m_attributes = buttonAttributes;
		}

		/// <summary>
		/// Registers all the input parameters for this component.
		/// </summary>
		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Model Path", "P", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item);
			pManager[0].Optional = true;
			pManager.AddBooleanParameter("Attach to Instance", "A", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
			pManager[1].Optional = true;
			pManager.AddTextParameter("Joint Layer", "J", "Joint Rhino Layer", GH_ParamAccess.item, string.Empty);
			pManager[2].Optional = true;
			pManager.AddTextParameter("Frame Layer", "F", "Beam frame Rhino Layer", GH_ParamAccess.item, string.Empty);
			pManager[3].Optional = true;
			pManager.AddTextParameter("Area Layer", "A", "Area Rhino Layer", GH_ParamAccess.item, string.Empty);
			pManager[4].Optional = true;
			pManager.AddBooleanParameter("Update groups", "UG", "If true, update the group definitions and group assignment in SAP", GH_ParamAccess.item, false);
			pManager[5].Optional = true;
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddTextParameter("Log", "L", "Etabs log", GH_ParamAccess.item);
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string modelPath = "";
			bool attachToInstance = false;
			string jointLayer = "";
			string frameLayer = "";
			string areaLayer = "";
			bool updateGroups = false;

			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);
			DA.GetData(2, ref jointLayer);
			DA.GetData(3, ref frameLayer);
			DA.GetData(4, ref areaLayer);
			DA.GetData(5, ref updateGroups);

			if (_run)
			{
				try
				{
					if (!File.Exists(modelPath) && !attachToInstance)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Model path don't exist and Attach to Instance is false");
						_run = false;
						return;
					}

					cSapModel etabsModel = F2RModelHelper.GetEtabsModel(out cOAPI sapObject, modelPath, attachToInstance);

					//if (etabsModel.SetModelIsLocked(false) != 0)
					//		return;

					global::Rhino.DocObjects.Tables.LayerTable layers = RhinoDoc.ActiveDoc.Layers;

					HashSet<RhinoObject> hashpoints = new HashSet<RhinoObject>();
					HashSet<RhinoObject> hashFrames = new HashSet<RhinoObject>();
					HashSet<RhinoObject> hashAreas = new HashSet<RhinoObject>();

					for (int i = 0; i < layers.Count; i++)
					{
						if (jointLayer != string.Empty)
							F2RModelHelper.AddLayerToHashSet(layers[i], jointLayer, hashpoints);
						if (frameLayer != string.Empty)
							F2RModelHelper.AddLayerToHashSet(layers[i], frameLayer, hashFrames);
						if (areaLayer != string.Empty)
							F2RModelHelper.AddLayerToHashSet(layers[i], areaLayer, hashAreas);
					}

					int TableVersion = 0;
					int NumberRecords = 0;

					#region Joint Coordinates

					if (hashpoints.Count > 0)
					{
						/// Input per i SetTableForEditingArray
						string TableKeyJC_set = "Point Object Connectivity";
						string[] FieldKeysIncludedJC_set = { "UniqueName", "IsSpecial", "X", "Y", "Z" };

						string[] TableDataJC_set = new string[hashpoints.Count * 5];

						for (int b = 0; b < hashpoints.Count; b++)
						{
							RhinoObject obj = hashpoints.ElementAt(b);
							Point point = (Point)obj.Geometry;

							TableDataJC_set[5 * b] = obj.Attributes.GetUserString("F2R_ID");
							TableDataJC_set[5 * b + 1] = obj.Attributes.GetUserString("F2R_SPECIALJOINT") == "True" ? "Yes" : "No";
							TableDataJC_set[5 * b + 2] = point.Location.X.ToString();
							TableDataJC_set[5 * b + 3] = point.Location.Y.ToString();
							TableDataJC_set[5 * b + 4] = point.Location.Z.ToString();
						}

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyJC_set, ref TableVersion, ref FieldKeysIncludedJC_set, NumberRecords, ref TableDataJC_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyJC_set}");
							_run = false;
							Message = "Fail";
							return;
						}
					}

					#endregion

					#region Connectivity - Beams, Braces, Columns, Null Lines and Frame Section Assignments

					if (hashFrames.Count > 0)
					{
						RhinoObject[] beams = hashFrames.Where(j => j.Attributes.GetUserString("F2R_FRAMETYPE") == "Beam").ToArray();
						RhinoObject[] columns = hashFrames.Where(j => j.Attributes.GetUserString("F2R_FRAMETYPE") == "Column").ToArray();
						RhinoObject[] braces = hashFrames.Where(j => j.Attributes.GetUserString("F2R_FRAMETYPE") == "Brace").ToArray();
						RhinoObject[] others = hashFrames.Where(j => j.Attributes.GetUserString("F2R_FRAMETYPE") == "Other").ToArray();
						RhinoObject[] nullLines = hashFrames.Where(j => j.Attributes.GetUserString("F2R_FRAMETYPE") == "Null").ToArray();

						#region Beams

						if (beams.Length > 0)
						{
							string TableKeyBOC_set = "Beam Object Connectivity";
							string[] FieldKeysIncludedCF_set = { "UniqueName", "UniquePtI", "UniquePtJ" };

							string[] TableDataCF_set = new string[beams.Length * 3];

							Array.Reverse(beams);

							for (int b = 0; b < beams.Length; b++)
							{
								RhinoObject obj = beams[b];

								TableDataCF_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
								TableDataCF_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_JOINT1");
								TableDataCF_set[3 * b + 2] = obj.Attributes.GetUserString("F2R_JOINT2");
							}

							if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyBOC_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyBOC_set}");
								_run = false;
								Message = "Fail";
								return;
							}
						}

						#endregion

						#region Columns

						if (columns.Length > 0)
						{
							string TableKeyCOC_set = "Column Object Connectivity";
							string[] FieldKeysIncludedCF_set = { "UniqueName", "UniquePtI", "UniquePtJ" };

							string[] TableDataCF_set = new string[columns.Length * 3];

							for (int b = 0; b < columns.Length; b++)
							{
								RhinoObject obj = columns[b];

								TableDataCF_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
								TableDataCF_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_JOINT1");
								TableDataCF_set[3 * b + 2] = obj.Attributes.GetUserString("F2R_JOINT2");
							}
							if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyCOC_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyCOC_set}");
								_run = false;
								Message = "Fail";
								return;
							}
						}

						#endregion

						#region Braces

						if (braces.Length > 0)
						{
							string TableKeyBrOC_set = "Brace Object Connectivity";
							string[] FieldKeysIncludedCF_set = { "UniqueName", "UniquePtI", "UniquePtJ" };

							string[] TableDataCF_set = new string[braces.Length * 3];

							for (int b = 0; b < braces.Length; b++)
							{
								RhinoObject obj = braces[b];

								TableDataCF_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
								TableDataCF_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_JOINT1");
								TableDataCF_set[3 * b + 2] = obj.Attributes.GetUserString("F2R_JOINT2");
							}
							if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyBrOC_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyBrOC_set}");
								_run = false;
								Message = "Fail";
								return;
							}
						}

						#endregion

						#region Other

						if (others.Length > 0)
						{
							string TableKeyOtOC_set = "Other Object Connectivity";
							string[] FieldKeysIncludedCF_set = { "UniqueName", "UniquePtI", "UniquePtJ" };

							string[] TableDataCF_set = new string[others.Length * 3];

							for (int b = 0; b < others.Length; b++)
							{
								RhinoObject obj = others[b];

								TableDataCF_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
								TableDataCF_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_JOINT1");
								TableDataCF_set[3 * b + 2] = obj.Attributes.GetUserString("F2R_JOINT2");
							}
							if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyOtOC_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyOtOC_set}");
								_run = false;
								Message = "Fail";
								return;
							}
						}

						#endregion

						#region Null lines

						if (nullLines.Length > 0)
						{
							string TableKeyNLOC_set = "Null Line Object Connectivity";
							string[] FieldKeysIncludedCF_set = { "UniqueName", "UniquePtI", "UniquePtJ" };

							string[] TableDataCF_set = new string[nullLines.Length * 3];

							for (int b = 0; b < nullLines.Length; b++)
							{
								RhinoObject obj = nullLines[b];

								TableDataCF_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
								TableDataCF_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_JOINT1");
								TableDataCF_set[3 * b + 2] = obj.Attributes.GetUserString("F2R_JOINT2");
							}
							if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyNLOC_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyNLOC_set}");
								_run = false;
								Message = "Fail";
								return;
							}
						}

						#endregion

						#region Section Properties

						string[] TableDataFASP_set = new string[hashFrames.Count * 2];

						for (int b = 0; b < hashFrames.Count; b++)
						{
							RhinoObject obj = hashFrames.ElementAt(b);

							TableDataFASP_set[2 * b] = obj.Attributes.GetUserString("F2R_ID");
							TableDataFASP_set[2 * b + 1] = obj.Attributes.GetUserString("F2R_PROPERTY");
						}

						string TableKeyFSA_set = "Frame Assignments - Section Properties";
						string[] FieldKeysIncludedFSA_set = { "UniqueName", "SectProp" };

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyFSA_set, ref TableVersion, ref FieldKeysIncludedFSA_set, NumberRecords, ref TableDataFASP_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Frame Section Assignments table");
							_run = false;
							Message = "Fail";
							return;
						}

						#endregion
					}


					#endregion

					#region Connectivity - Area and Area Section Assignments

					if (hashAreas.Count > 0)
					{
						RhinoObject[] floors = hashAreas.Where(j => j.Attributes.GetUserString("F2R_AREATYPE") == "Floor").ToArray();
						RhinoObject[] walls = hashAreas.Where(j => j.Attributes.GetUserString("F2R_AREATYPE") == "Wall").ToArray();
						RhinoObject[] nullAreas = hashAreas.Where(j => j.Attributes.GetUserString("F2R_AREATYPE") == "Null").ToArray();

						string TableKeyFOC_set = "Floor Object Connectivity";
						string TableKeyWOC_set = "Wall Object Connectivity";
						string TableKeyNAOC_set = "Null Area Object Connectivity";
						string TableKeyAASP_set = "Area Assignments - Section Properties";

						string[] FieldKeysIncludedAC_set = { "UniqueName", "UniquePt1", "UniquePt2", "UniquePt3", "UniquePt4" };
						string[] FieldKeysIncludedASP_set = { "UniqueName", "SectProp", "PropType" };

						List<string> buffer = new List<string>();

						#region Floors

						for (int b = 0; b < floors.Length; b++)
						{
							RhinoObject obj = floors[b];

							if (double.TryParse(obj.Attributes.GetUserString("F2R_VERTICESCOUNT"), out double verticesCount))
							{
								for (int c = 0; c < (int)Math.Ceiling(verticesCount / 4); c++)
								{
									buffer.Add(obj.Attributes.GetUserString("F2R_ID"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 1}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 2}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 3}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 4}"));
								}
							}
						}

						string[] TableDataFOC_set = buffer.ToArray();
						buffer.Clear();

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyFOC_set, ref TableVersion, ref FieldKeysIncludedAC_set, NumberRecords, ref TableDataFOC_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyFOC_set}");
							_run = false;
							Message = "Fail";
							return;
						}

						#endregion

						#region Walls

						for (int b = 0; b < walls.Length; b++)
						{
							RhinoObject obj = walls[b];

							if (double.TryParse(obj.Attributes.GetUserString("F2R_VERTICESCOUNT"), out double verticesCount))
							{
								for (int c = 0; c < (int)Math.Ceiling(verticesCount / 4); c++)
								{
									buffer.Add(obj.Attributes.GetUserString("F2R_ID"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 1}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 2}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 3}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 4}"));
								}
							}
						}

						string[] TableDataWOC_set = buffer.ToArray();
						buffer.Clear();

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyWOC_set, ref TableVersion, ref FieldKeysIncludedAC_set, NumberRecords, ref TableDataWOC_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyFOC_set}");
							_run = false;
							Message = "Fail";
							return;
						}

						#endregion

						#region Null Areas

						for (int b = 0; b < nullAreas.Length; b++)
						{
							RhinoObject obj = nullAreas[b];

							if (double.TryParse(obj.Attributes.GetUserString("F2R_VERTICESCOUNT"), out double verticesCount))
							{
								for (int c = 0; c < (int)Math.Ceiling(verticesCount / 4); c++)
								{
									buffer.Add(obj.Attributes.GetUserString("F2R_ID"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 1}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 2}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 3}"));
									buffer.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 4}"));
								}
							}
						}

						string[] TableDataNAOC_set = buffer.ToArray();
						buffer.Clear();

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyNAOC_set, ref TableVersion, ref FieldKeysIncludedAC_set, NumberRecords, ref TableDataNAOC_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Fail to set {TableKeyFOC_set}");
							_run = false;
							Message = "Fail";
							return;
						}

						buffer.Clear();

						#endregion

						#region Area Assignments - Section Properties

						string[] TableDataCAA_set = new string[hashAreas.Count * 3];

						for (int b = 0; b < hashAreas.Count; b++)
						{
							RhinoObject obj = hashAreas.ElementAt(b);

							TableDataCAA_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
							TableDataCAA_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_PROPERTY");
							TableDataCAA_set[3 * b + 2] = obj.Attributes.GetUserString("F2R_AREATYPE");
						}

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyAASP_set, ref TableVersion, ref FieldKeysIncludedASP_set, NumberRecords, ref TableDataCAA_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Area Section Assignments table");
							_run = false;
							Message = "Fail";
							return;
						}

						#endregion
					}

					#endregion

					#region Groups 2 - Assignments

					if (updateGroups)
					{
						HashSet<string> groups = new HashSet<string>();
						List<string> TableDataG2A_setList = new List<string>();

						/// Input per i GetTableForEditingArray

						for (int i = 0; i < hashpoints.Count; i++)
						{
							RhinoObject obj = hashpoints.ElementAt(i);
							var keys = obj.Attributes.GetUserStrings();
							for (int j = 0; j < keys.Count; j++)
							{
								string key = keys.AllKeys[j];
								if (key.StartsWith("F2R_GROUP_"))
								{
									string newGroup = obj.Attributes.GetUserString(key);
									groups.Add(newGroup);

									TableDataG2A_setList.Add(newGroup);
									TableDataG2A_setList.Add("Point");
									TableDataG2A_setList.Add(obj.Attributes.GetUserString("F2R_ID"));
								}
							}
						}
						for (int i = 0; i < hashFrames.Count; i++)
						{
							RhinoObject obj = hashFrames.ElementAt(i);
							var keys = obj.Attributes.GetUserStrings();
							for (int j = 0; j < keys.Count; j++)
							{
								string key = keys.AllKeys[j];
								if (key.StartsWith("F2R_GROUP_"))
								{
									string newGroup = obj.Attributes.GetUserString(key);
									groups.Add(newGroup);

									TableDataG2A_setList.Add(newGroup);
									TableDataG2A_setList.Add("Line");
									TableDataG2A_setList.Add(obj.Attributes.GetUserString("F2R_ID"));
								}
							}
						}
						for (int i = 0; i < hashAreas.Count; i++)
						{
							RhinoObject obj = hashAreas.ElementAt(i);
							var keys = obj.Attributes.GetUserStrings();
							for (int j = 0; j < keys.Count; j++)
							{
								string key = keys.AllKeys[j];
								if (key.StartsWith("F2R_GROUP_"))
								{
									string newGroup = obj.Attributes.GetUserString(key);
									groups.Add(newGroup);

									TableDataG2A_setList.Add(newGroup);
									TableDataG2A_setList.Add("Area");
									TableDataG2A_setList.Add(obj.Attributes.GetUserString("F2R_ID"));
								}
							}
						}

						string[] TableDataG2A_set = TableDataG2A_setList.ToArray();

						/// Input per i SetTableForEditingArray
						string TableKeyG2A_set = "Group Assignments";
						string[] FieldKeysIncludedG2A_set = { "GroupName", "ObjectType", "UniqueName" };

						if (etabsModel.DatabaseTables.SetTableForEditingArray(TableKeyG2A_set, ref TableVersion, ref FieldKeysIncludedG2A_set, NumberRecords, ref TableDataG2A_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Groups 1 - Definitions table");
							_run = false;
							Message = "Fail";
							return;
						}
					}

					#endregion

					#region Apply Edited Tables

					/// Input per la funzione "ApplyEditedTables"
					bool FillImportLog = true;
					int NumFatalErrors = 0;
					int NumErrorMsgs = 0;
					int NumWarnMsgs = 0;
					int NumInfoMsgs = 0;
					string ImportLog = "";

					if (etabsModel.DatabaseTables.ApplyEditedTables(FillImportLog, ref NumFatalErrors, ref NumErrorMsgs, ref NumWarnMsgs, ref NumInfoMsgs, ref ImportLog) != 0)
					{
						DA.SetData(0, ImportLog);
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to apply edited tables");
						_run = false;
						Message = "Fail";
						return;
					}

					DA.SetData(0, ImportLog);

					#endregion

					#region Refresh Save and Close

					if (etabsModel.View.RefreshView() != 0)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to refresh the view");

					if (!attachToInstance)
					{
						if (etabsModel.File.Save(modelPath) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to save the model");
							Message = "Fail";
							_run = false;
						}
						else
						{
							Message = "Done";
						}
					}
					else
					{
						Message = "Done";
					}

					#endregion

					_run = false;
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
					_run = false;
				}
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RRhino2ETABS;

		public override Guid ComponentGuid => new Guid("dacf52e1-fa2a-4c77-90a3-a1741595aa9c");
	}
}
