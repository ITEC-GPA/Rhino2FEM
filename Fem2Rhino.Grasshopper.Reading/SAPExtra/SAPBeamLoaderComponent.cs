//using SAP2000v1;
//using FeMM.Common.Models;
//using FeMM.Grasshopper.DataTypes.FeMM;
//using FeMM.Grasshopper.Helpers;
//using Grasshopper.Kernel;
//using Rhino.Geometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//// In order to load the result of this wizard, you will also need to
//// add the output bin/ folder of this project to the list of loaded
//// folder in Grasshopper.
//// You can use the _GrasshopperDeveloperSettings Rhino command for that.

//namespace FeMM.Grasshopper.Components.SAPExtra
//{
//    public class SAPBeamLoaderComponent : GH_Component
//	{
//		/// <summary>
//		/// Each implementation of GH_Component must provide a public 
//		/// constructor without any arguments.
//		/// Category represents the Tab in which the component will appear, 
//		/// Subcategory the panel. If you use non-existing tab or panel names, 
//		/// new tabs/panels will automatically be created.
//		/// </summary>
//		public SAPBeamLoaderComponent()
//		  : base("Beam Loader", "BLC", "SAP Beam Loader", CategoryNameConstants.CATEGORY_CHECKS, CategoryNameConstants.SUBCATEGORY_SAPEXTRA)
//		{
//			_run = false;
//		}

//		bool _run;

//		public override void CreateAttributes()
//		{
//            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes attr = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Run");
//			attr.ButtonPressed += () =>
//			{
//				_run = true;
//				ExpireSolution(true);
//			};
//			m_attributes = attr;
//		}

//		/// <summary>
//		/// Registers all the input parameters for this component.
//		/// </summary>
//		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Beams", "B", "Beams", GH_ParamAccess.list);
//			pManager.AddTextParameter("Path SAP Model", "P", "", GH_ParamAccess.item);
//			int j = pManager.AddTextParameter("Load Pattern", "LP", "Load Patterns to copy. If is empty, copy all load patterns", GH_ParamAccess.list);
//			pManager[j].Optional = true;
//			int i = pManager.AddTextParameter("SapExe", "S", "sap2000 exe fullpath", GH_ParamAccess.item);
//			pManager[i].Optional = true;
//		}

//		/// <summary>
//		/// Registers all the output parameters for this component.
//		/// </summary>
//		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Beams", "Loaded Beams", "", GH_ParamAccess.list);
//			pManager.AddTextParameter("dev", "dev", "", GH_ParamAccess.item);
//		}

//		/// <summary>
//		/// This is the method that actually does the work.
//		/// </summary>
//		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
//		/// to store data in output parameters.</param>
//		protected override void SolveInstance(IGH_DataAccess DA)
//		{
//			List<GH_Beam> beams = new List<GH_Beam>();
//			string path = "";
//			List<string> load_patterns = new List<string>();
//			string sapExe = "";

//			if (!DA.GetDataList(0, beams))
//				return;
//			if (!DA.GetData(1, ref path))
//				return;
//			DA.GetDataList(2, load_patterns);
//			DA.GetData(3, ref sapExe);

//			if (_run)
//			{
//				List<GH_Beam> new_beams = new List<GH_Beam>();
//				for (int i = 0; i < beams.Count; i++)
//				{
//					new_beams.Add(new GH_Beam(beams[i]));
//				}
//				string dev = "";
//				LoadBeams(sapExe, path, load_patterns, ref new_beams, ref dev);

//				DA.SetDataList(0, new_beams);
//				DA.SetData(1, dev);
//			}

//			_run = false;
//		}

//		/// <summary>
//		/// Provides an Icon for every component that will be visible in the User Interface.
//		/// Icons need to be 24x24 pixels.
//		/// </summary>
//		protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPBeamLoaderIcon;

//		/// <summary>
//		/// Each component must have a unique Guid to identify it. 
//		/// It is vital this Guid doesn't change otherwise old ghx files 
//		/// that use the old ID will partially fail during loading.
//		/// </summary>
//		public override Guid ComponentGuid => new Guid("1ab542c9-585b-4186-9055-e8a4bfe01f60");

//		protected void LoadBeams(string sapExe, string path_model, List<string> load_patterns, ref List<GH_Beam> new_beams, ref string dev)
//		{
//			bool AttachToInstance = false;
//			ExtraSapHelper.InitializeModel(!AttachToInstance, sapExe, path_model, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper);

//			bool exportAll = false;
//			if (load_patterns == null || load_patterns.Count == 0)
//				exportAll = true;

//			int beams_nr = -1;
//			string[] beams_name = new string[0];
//			mySapObject.SapModel.FrameObj.GetNameList(ref beams_nr, ref beams_name);
//			List<string> beams_name_list = new List<string>(beams_name);

//			int tot = mySapObject.SapModel.FrameObj.Count();
//			if (tot != beams_nr || tot != beams_name_list.Distinct().ToList().Count)
//			{
//				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plese add unique ID to all beams!");
//				return;
//			}

//			for (int i = 0; i < beams_nr; i++)
//			{
//				//search in selected beam if have same name
//				//ElementIdAttributeModel attr = (ElementIdAttributeModel)el.Attributes.FirstOrDefault(a => a is ElementIdAttributeModel);
//				int j_iter = -1;
//				for (int j = 0; j < new_beams.Count; j++)
//				{
//					ElementIdAttributeModel attr;
//					try
//					{
//						attr = (ElementIdAttributeModel)new_beams[j].Value.Attributes.Where(a => a is ElementIdAttributeModel).First();
//					}
//					catch (Exception)
//					{
//						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please add ID to beam");
//						return;
//					}
//					if (attr.Value == beams_name[i])
//					{
//						j_iter = j;
//						j = new_beams.Count(); //exit
//					}
//				}
//				if (j_iter >= 0) //I have found
//				{
//					#region distribuited_load

//					//dev = dev + " " + beams_name[i] + "\n";
//					int nr_load = -1;
//					string[] frame_name = new string[0];
//					string[] load_pat = new string[0];
//					int[] my_type = new int[0];
//					string[] c_sys = new string[0];
//					int[] dir = new int[0];
//					double[] rd1 = new double[0];
//					double[] rd2 = new double[0];
//					double[] dist1 = new double[0];
//					double[] dist2 = new double[0];
//					double[] val1 = new double[0];
//					double[] val2 = new double[0];

//					if (mySapObject.SapModel.FrameObj.GetLoadDistributed(beams_name[i], ref nr_load, ref frame_name, ref load_pat, ref my_type, ref c_sys, ref dir,
//						ref rd1, ref rd2, ref dist1, ref dist2, ref val1, ref val2) != 0)
//						return;

//					for (int j = 0; j < nr_load; j++)
//					{
//						if (load_patterns.Contains(load_pat[j]) || exportAll)
//						{
//							dev = dev + " name = " + frame_name[j] + "\n";
//							dev = dev + " load pattern = " + load_pat[j] + "\n";
//							dev = dev + " type = " + my_type[j] + "\n";
//							dev = dev + " c sys = " + c_sys[j] + "\n";
//							dev = dev + " dir = " + dir[j] + "\n";
//							dev = dev + " rd1 = " + rd1[j] + "\n";
//							dev = dev + " rd2 = " + rd2[j] + "\n";
//							dev = dev + " dist1 = " + dist1[j] + "\n";
//							dev = dev + " dist2 = " + dist2[j] + "\n";
//							dev = dev + " val1 = " + val1[j] + "\n";
//							dev = dev + " val2 = " + val2[j] + "\n \n";

//							//Add load to new beam
//							string id = load_pat[j];
//							string name = load_pat[j];
//							LoadCaseModel loadCase = new LoadCaseModel(id, name);
//							BeamDistributedLoadModel.LoadData loadData = new BeamDistributedLoadModel.LoadData();
//							loadData.A = rd1[j];
//							loadData.B = rd2[j] - 1.0;
//							if (c_sys[j] == "Local")
//							{
//								loadData.CoordinateSystem = null;
//							}
//							else
//							{
//								loadData.CoordinateSystem = new CoordinateSystemModel(c_sys[j]);
//							}

//							if (dir[j] == 1) //direcion 1 for sap
//							{
//								loadData.LoadDirection = (BeamDistributedLoadModel.LoadDirection)(3 - 1); //Local axis 3 for Straus == local axis 1 for Sap
//							}
//							else if (dir[j] == 2) //direction 2 for sap
//							{
//								loadData.LoadDirection = (BeamDistributedLoadModel.LoadDirection)(2 - 1); //Local axis 2 Straus == Local axis 2 for sap
//							}
//							else if (dir[j] == 3) //direction 3 for sap
//							{
//								val1[j] = -val1[j]; //opposite sign of local axis between SAP and Straus
//								val2[j] = -val2[j]; //opposite sign of local axis between SAP and Straus
//								loadData.LoadDirection = (BeamDistributedLoadModel.LoadDirection)(1 - 1); //Local 1 axis for Sap == local axis 3 for sap but opposite signe!!!!
//							}
//							else if (dir[j] == 4)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.X;
//							}
//							else if (dir[j] == 5)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.Y;
//							}
//							else if (dir[j] == 6)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.Z;
//							}
//							else if (dir[j] == 7)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.X_Projected;
//							}
//							else if (dir[j] == 8)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.Y_Projected;
//							}
//							else if (dir[j] == 9)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.Z_Projected;
//							}
//							else if (dir[j] == 10)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.Gravity;
//							}
//							else if (dir[j] == 11)
//							{
//								loadData.LoadDirection = BeamDistributedLoadModel.LoadDirection.Z_Projected;
//							}

//							loadData.LoadSchema = BeamDistributedLoadModel.LoadSchema.Uniform;
//							loadData.P1 = val1[j];
//							loadData.P2 = val2[j];
//							loadData.PA = val1[j];
//							loadData.PB = val2[j];

//							BeamDistributedLoadModel.LoadType loadType;
//							if (my_type[j] == 1)
//							{
//								loadType = BeamDistributedLoadModel.LoadType.Force;
//							}
//							else
//							{
//								loadType = BeamDistributedLoadModel.LoadType.Moment;
//							}
//							BeamDistributedLoadModel load = new BeamDistributedLoadModel(loadCase, loadData, loadType);
//							//new_beams[i].Value.Loads.Add(load);
//							new_beams[j_iter].Value.Loads.Add(load);

//						}
//					}

//					#endregion

//					#region point_load

//					nr_load = -1;
//					frame_name = new string[0];
//					load_pat = new string[0];
//					my_type = new int[0];
//					c_sys = new string[0];
//					dir = new int[0];
//					/**
//					dir:
//					1 = Local 1 axis (only applies when CSys is Local
//					2 = Local 2 axis (only applies when CSys is Local)
//					3 = Local 3 axis (only applies when CSys is Local)
//					4 = X direction (does not apply when CSys is Local)
//					5 = Y direction (does not apply when CSys is Local)
//					6 = Z direction (does not apply when CSys is Local)
//					7 = Projected X direction (does not apply when CSys is Local)
//					8 = Projected Y direction (does not apply when CSys is Local)
//					9 = Projected Z direction (does not apply when CSys is Local)
//					10 = Gravity direction (only applies when CSys is Global)
//					11 = Projected Gravity direction (only applies when CSys is Global)
//					**/
//					double[] rel_dist = new double[0];
//					double[] dist = new double[0];
//					double[] val = new double[0];

//					if (mySapObject.SapModel.FrameObj.GetLoadPoint(beams_name[i], ref nr_load, ref frame_name, ref load_pat, ref my_type, ref c_sys, ref dir, ref rel_dist, ref dist, ref val) != 0)
//						return;

//					for (int j = 0; j < nr_load; j++)
//					{
//						if (load_patterns.Contains(load_pat[j]) || exportAll)
//						{
//							dev = dev + " name = " + frame_name[j] + "\n"; //array?
//							dev = dev + " load pattern = " + load_pat[j] + "\n"; //array?
//							dev = dev + " type = " + my_type[j] + "\n"; //array?
//							dev = dev + " c sys = " + c_sys[j] + "\n"; //array?
//							dev = dev + " dir = " + dir[j] + "\n"; //array?
//							dev = dev + " rel_dist = " + rel_dist[j] + "\n"; //array?
//							dev = dev + " dist = " + dist[j] + "\n"; //array?
//							dev = dev + " val = " + val[j] + "\n \n"; //array?

//							//Add load to new beam
//							string id = load_pat[j];
//							string name = load_pat[j];
//							LoadCaseModel loadCase = new LoadCaseModel(id, name);

//							if (my_type[j] == 1) //Force
//							{
//								BeamPointLoadModel load_model = new BeamPointLoadModel();
//								BeamPointLoadModel.LoadData load_value = new BeamPointLoadModel.LoadData { A = rel_dist[j] };

//								if (c_sys[j] == "Local")
//								{
//									load_value.CoordinateSystem = null;
//								}
//								else
//								{
//									load_value.CoordinateSystem = new CoordinateSystemModel(c_sys[j]);
//								}

//								if (dir[j] == 1) //1 = Local 1 SAP axis
//								{
//									load_value.P = new Vector3d(0, 0, val[j]);
//								}
//								else if (dir[j] == 2) //2 = Local 2 axis SAP
//								{
//									load_value.P = new Vector3d(0, val[j], 0);
//								}
//								else if (dir[j] == 3) //3 = Local 3 axis SAP
//								{
//									load_value.P = new Vector3d(-1.0 * val[j], 0, 0);
//								}
//								else if (dir[j] == 4) //Global X direction
//								{
//									load_value.P = new Vector3d(val[j], 0, 0);
//								}
//								else if (dir[j] == 5) //Global Y direction
//								{
//									load_value.P = new Vector3d(0, val[j], 0);
//								}
//								else if (dir[j] == 6) //Global Z direction
//								{
//									load_value.P = new Vector3d(0, 0, val[j]);
//								}
//								else
//								{
//									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Force Point load on frame {beams_name[i]} case {load_pat[j]} - GetPointLoad type " + dir[j] + " - Direction not supported");
//								}

//								BeamPointLoadModel load = new BeamPointLoadModel(loadCase, load_value, BeamPointLoadModel.LoadType.Force);
//								new_beams[j_iter].Value.Loads.Add(load);
//							}
//							else //Moments
//							{

//								BeamPointLoadModel load_model = new BeamPointLoadModel();
//								BeamPointLoadModel.LoadData load_value = new BeamPointLoadModel.LoadData { A = rel_dist[j] };

//								if (c_sys[j] == "Local")
//								{
//									load_value.CoordinateSystem = null;
//								}
//								else
//								{
//									load_value.CoordinateSystem = new CoordinateSystemModel(c_sys[j]);
//								}

//								if (dir[j] == 1 || dir[j] == 4)
//								{
//									load_value.P = new Vector3d(val[j], 0, 0);
//								}
//								else if (dir[j] == 2 || dir[j] == 5)
//								{
//									load_value.P = new Vector3d(0, val[j], 0);
//								}
//								else if (dir[j] == 3 || dir[j] == 6)
//								{
//									load_value.P = new Vector3d(0, 0, val[j]);
//								}
//								else
//								{
//									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Moment Point load on frame {beams_name[i]} case {load_pat[j]} - GetPointLoad type " + dir[j] + " - Direction not supported");
//								}
//								BeamPointLoadModel load = new BeamPointLoadModel(loadCase, load_value, BeamPointLoadModel.LoadType.Moment);
//								new_beams[j_iter].Value.Loads.Add(load);
//							}
//						}
//					}

//					#endregion
//				}
//			}

//			mySapObject.ApplicationExit(false);
//		}
//	}
//}
