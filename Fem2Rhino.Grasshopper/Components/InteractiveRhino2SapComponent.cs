using CSiAPIv1;
using FeMM.Grasshopper.Helpers;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FeMM.Grasshopper.Components.F2R
{
	public class InteractiveRhino2SapComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public InteractiveRhino2SapComponent()
			: base("Rhino2SAP", "R2S", "Update SAP interactive model of current open istance", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_INTERACTIVESAP)
		{
		}

		public override void CreateAttributes()
		{
			ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Rhino2SAP");
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
			pManager.AddTextParameter("Frame Layer", "F", "Frame Rhino Layer", GH_ParamAccess.item, string.Empty);
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
			pManager.AddTextParameter("Log", "L", "SAP log", GH_ParamAccess.item);
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

					cSapModel SapModel = F2RModelHelper.GetSapModel(out _, modelPath, attachToInstance);

					if (SapModel.SetModelIsLocked(false) != 0)
						return;

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

					#region Joint Coordinates

					if (hashpoints.Count > 0)
					{
						/// Input per i GetTableForEditingArray
						int TableVersion = 0;
						int NumberRecords = 0;

						/// Input per i SetTableForEditingArray
						string TableKeyJC_set = "Joint Coordinates";
						string[] FieldKeysIncludedJC_set = { "Joint", "CoordSys", "CoordType", "XorR", "Y", "T", "Z", "SpecialJt", "GUID" };

						string[] TableDataJC_set = new string[hashpoints.Count * 9];

						for (int b = 0; b < hashpoints.Count; b++)
						{
							RhinoObject obj = hashpoints.ElementAt(b);
							Point point = obj.Geometry as Point;
							if (point != null)
							{
								TableDataJC_set[9 * b] = obj.Attributes.GetUserString("F2R_ID");
								TableDataJC_set[9 * b + 1] = "GLOBAL";
								TableDataJC_set[9 * b + 2] = "Cartesian";
								TableDataJC_set[9 * b + 3] = point.Location.X.ToString();
								TableDataJC_set[9 * b + 4] = point.Location.Y.ToString();
								TableDataJC_set[9 * b + 5] = "0";
								TableDataJC_set[9 * b + 6] = point.Location.Z.ToString();
								TableDataJC_set[9 * b + 7] = obj.Attributes.GetUserString("F2R_SPECIALJOINT") == "True" ? "Yes" : "No";
								TableDataJC_set[9 * b + 8] = obj.Attributes.GetUserString("F2R_GUID") != null ? obj.Attributes.GetUserString("F2R_GUID") : "";
							}
							else
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Fail to Joint n: {b}");
							}
						}

						if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyJC_set, ref TableVersion, ref FieldKeysIncludedJC_set, NumberRecords, ref TableDataJC_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Joint Coordinates table");
							_run = false;
							Message = "Fail";
							return;
						}
					}

					#endregion

					#region Connectivity - Frame and Frame Section Assignments

					if (hashFrames.Count > 0)
					{
						string TableKeyCF_set = "Connectivity - Frame";
						string TableKeyFSA_set = "Frame Section Assignments";
						string[] FieldKeysIncludedCF_set = { "Frame", "JointI", "JointJ", "IsCurved" };
						string[] FieldKeysIncludedFSA_set = { "Frame", "AnalSect" };
						int TableVersion = 0;
						int NumberRecords = 0;

						string[] TableDataCF_set = new string[hashFrames.Count * 4];
						string[] TableDataFSA_set = new string[hashFrames.Count * 2];

						for (int b = 0; b < hashFrames.Count; b++)
						{
							RhinoObject obj = hashFrames.ElementAt(b);

							TableDataCF_set[4 * b] = obj.Attributes.GetUserString("F2R_ID");
							TableDataCF_set[4 * b + 1] = obj.Attributes.GetUserString("F2R_JOINT1");
							TableDataCF_set[4 * b + 2] = obj.Attributes.GetUserString("F2R_JOINT2");
							TableDataCF_set[4 * b + 3] = "No";

							TableDataFSA_set[2 * b] = obj.Attributes.GetUserString("F2R_ID");
							TableDataFSA_set[2 * b + 1] = obj.Attributes.GetUserString("F2R_PROPERTY");
						}
						if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyCF_set, ref TableVersion, ref FieldKeysIncludedCF_set, NumberRecords, ref TableDataCF_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Connectivity - Frame table");
							_run = false;
							Message = "Fail";
							return;
						}
						if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyFSA_set, ref TableVersion, ref FieldKeysIncludedFSA_set, NumberRecords, ref TableDataFSA_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Frame Section Assignments table");
							_run = false;
							Message = "Fail";
							return;
						}
					}

					#endregion

					#region Connectivity - Area and Area Section Assignments

					if (hashAreas.Count > 0)
					{
						string TableKeyCA_set = "Connectivity - Area";
						string TableKeyASA_set = "Area Section Assignments";
						string[] FieldKeysIncludedCA_set = { "Area", "Joint1", "Joint2", "Joint3", "Joint4", "GUID" };
						string[] FieldKeysIncludedASA_set = { "Area", "Section", "MatProp" };
						int TableVersion = 0;
						int NumberRecords = 0;

						List<string> TableDataCA_set_string = new List<string>();
						string[] TableDataCAA_set = new string[hashAreas.Count * 3];

						for (int b = 0; b < hashAreas.Count; b++)
						{
							RhinoObject obj = hashAreas.ElementAt(b);

							if (double.TryParse(obj.Attributes.GetUserString("F2R_VERTICESCOUNT"), out double verticesCount))
							{
								for (int c = 0; c < (int)Math.Ceiling(verticesCount / 4); c++)
								{
									var iiiii = obj.Attributes.GetUserString("F2R_ID");
									TableDataCA_set_string.Add(obj.Attributes.GetUserString("F2R_ID"));
									TableDataCA_set_string.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 1}"));
									TableDataCA_set_string.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 2}"));
									TableDataCA_set_string.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 3}"));
									TableDataCA_set_string.Add(obj.Attributes.GetUserString($"F2R_JOINT{c * 4 + 4}"));

									if (c == 0)
										TableDataCA_set_string.Add(obj.Attributes.GetUserString("F2R_GUID") != null ? obj.Attributes.GetUserString("F2R_GUID") : Guid.NewGuid().ToString());
									else
										TableDataCA_set_string.Add("");
								}
							}

							TableDataCAA_set[3 * b] = obj.Attributes.GetUserString("F2R_ID");
							TableDataCAA_set[3 * b + 1] = obj.Attributes.GetUserString("F2R_PROPERTY");
							TableDataCAA_set[3 * b + 2] = "Default";
						}

						string[] TableDataCA_set = TableDataCA_set_string.ToArray();

						if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyCA_set, ref TableVersion, ref FieldKeysIncludedCA_set, NumberRecords, ref TableDataCA_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Connectivity - Area table");
							_run = false;
							Message = "Fail";
							return;
						}
						if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyASA_set, ref TableVersion, ref FieldKeysIncludedASA_set, NumberRecords, ref TableDataCAA_set) != 0)
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to set Area Section Assignments table");
							_run = false;
							Message = "Fail";
							return;
						}
					}

					#endregion

					#region Groups 2 - Assignments

					if (updateGroups)
					{
						HashSet<string> groups = new HashSet<string>();
						List<string> TableDataG2A_setList = new List<string>();

						/// Input per i GetTableForEditingArray
						int TableVersion = 0;
						int NumberRecords = 0;

						for (int i = 0; i < hashpoints.Count; i++)
						{
							RhinoObject obj = hashpoints.ElementAt(i);
							var keys = obj.Attributes.GetUserStrings();
							for (int j = 0; j < keys.Count; j++)
							{
								string key = keys.AllKeys[j];
								if (key.StartsWith("F2R_GROUP_"))
								{
									var newGroup = obj.Attributes.GetUserString(key);
									groups.Add(newGroup);

									TableDataG2A_setList.Add(newGroup);
									TableDataG2A_setList.Add("Joint");
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
									var newGroup = obj.Attributes.GetUserString(key);
									groups.Add(newGroup);

									TableDataG2A_setList.Add(newGroup);
									TableDataG2A_setList.Add("Frame");
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
									var newGroup = obj.Attributes.GetUserString(key);
									groups.Add(newGroup);

									TableDataG2A_setList.Add(newGroup);
									TableDataG2A_setList.Add("Area");
									TableDataG2A_setList.Add(obj.Attributes.GetUserString("F2R_ID"));
								}
							}
						}

						string[] TableDataG2A_set = TableDataG2A_setList.ToArray();

						/// Input per i SetTableForEditingArray
						string TableKeyG2A_set = "Groups 2 - Assignments";
						string[] FieldKeysIncludedG2A_set = { "GroupName", "ObjectType", "ObjectLabel" };

						if (SapModel.DatabaseTables.SetTableForEditingArray(TableKeyG2A_set, ref TableVersion, ref FieldKeysIncludedG2A_set, NumberRecords, ref TableDataG2A_set) != 0)
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

					if (SapModel.DatabaseTables.ApplyEditedTables(FillImportLog, ref NumFatalErrors, ref NumErrorMsgs, ref NumWarnMsgs, ref NumInfoMsgs, ref ImportLog) != 0)
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

					if (SapModel.View.RefreshView() != 0)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to refresh the view");

					if (!attachToInstance)
					{
						if (SapModel.File.Save(modelPath) != 0)
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

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RRhino2SAP;

		public override Guid ComponentGuid => new Guid("17fc3dcd-5ab8-412f-857c-30c075f63398");
	}
}
