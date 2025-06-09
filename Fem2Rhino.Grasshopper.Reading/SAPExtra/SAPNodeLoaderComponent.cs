//using Fem2Rhino.CSI;
//using FeMM.Common.Helpers;
//using FeMM.Common.Models;
//using FeMM.Grasshopper.DataTypes.FeMM;
//using FeMM.Grasshopper.Helpers;
//using Grasshopper.Kernel;
//using Rhino.Geometry;
//using SAP2000v1;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//// In order to load the result of this wizard, you will also need to
//// add the output bin/ folder of this project to the list of loaded
//// folder in Grasshopper.
//// You can use the _GrasshopperDeveloperSettings Rhino command for that.

//namespace FeMM.Grasshopper.Components.SAPExtra
//{
//    public class SAPNodeLoaderComponent : GH_Component
//	{
//		/// <summary>
//		/// Each implementation of GH_Component must provide a public 
//		/// constructor without any arguments.
//		/// Category represents the Tab in which the component will appear, 
//		/// Subcategory the panel. If you use non-existing tab or panel names, 
//		/// new tabs/panels will automatically be created.
//		/// </summary>
//		public SAPNodeLoaderComponent()
//		  : base("Node Loader", "NLC", "SAP Node Loader", CategoryNameConstants.CATEGORY_CHECKS, CategoryNameConstants.SUBCATEGORY_SAPEXTRA)
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
//			pManager.AddGenericParameter("Nodes", "N", "FEMM Nodes", GH_ParamAccess.list);
//			pManager.AddTextParameter("Path SAP Model", "P", "", GH_ParamAccess.item);
//			int j = pManager.AddTextParameter("Load Pattern", "LP", "Load Patterns to copy. If is empty, copy all load patterns", GH_ParamAccess.list, new List<string>());
//			pManager[j].Optional = true;
//			int i = pManager.AddTextParameter("SapExe", "S", "sap2000 exe fullpath", GH_ParamAccess.item);
//			pManager[i].Optional = true;
//		}

//		/// <summary>
//		/// Registers all the output parameters for this component.
//		/// </summary>
//		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Nodes", "N", "", GH_ParamAccess.list);
//			pManager.AddTextParameter("dev", "dev", "", GH_ParamAccess.item);
//		}

//		/// <summary>
//		/// This is the method that actually does the work.
//		/// </summary>
//		/// <param name="DA">The DA object can be used to retrieve data from input parameters and 
//		/// to store data in output parameters.</param>
//		protected override void SolveInstance(IGH_DataAccess DA)
//		{
//			List<GH_Node> nodes = new List<GH_Node>();
//			string path = "";
//			List<string> load_patterns = new List<string>();
//			string sapExe = "";

//			if (!DA.GetDataList(0, nodes))
//				return;
//			if (!DA.GetData(1, ref path)) 
//				return;
//			DA.GetDataList(2, load_patterns);
//			DA.GetData(3, ref sapExe);

//			if (_run)
//			{
//				List<GH_Node> new_nodes = new List<GH_Node>();
//				for (int i = 0; i < nodes.Count; i++)
//				{
//					new_nodes.Add(new GH_Node(nodes[i]));
//				}
//				string dev = "";
//				LoadNodes(sapExe, path, load_patterns, ref new_nodes, ref dev);

//				DA.SetDataList(0, new_nodes);
//				DA.SetData(1, dev);
//			}

//			_run = false;
//		}

//		/// <summary>
//		/// Provides an Icon for every component that will be visible in the User Interface.
//		/// Icons need to be 24x24 pixels.
//		/// </summary>
//		protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPNodeLoaderIcon;

//		/// <summary>
//		/// Each component must have a unique Guid to identify it. 
//		/// It is vital this Guid doesn't change otherwise old ghx files 
//		/// that use the old ID will partially fail during loading.
//		/// </summary>
//		public override Guid ComponentGuid => new Guid("a8923d60-824f-4036-abf3-3dee3751a9c2");

//		public void LoadNodes(string sapExe, string path_model, List<string> load_patterns, ref List<GH_Node> new_nodes, ref string dev)
//		{
//			bool AttachToInstance = false;
//			int ret = 0;

//            ProxyHelper.CSI.ApiWrapper.SAPApiWrapper wr = new ProxyHelper.CSI.ApiWrapper.SAPApiWrapper();
//            if(!wr.InitializeModel(!AttachToInstance, sapExe, path_model, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper))
//				return;

//			bool exportAll = false;
//			if (load_patterns == null || load_patterns.Count == 0)
//				exportAll = true;

//			//get name list of all nodes
//			int nodes_nr = -1;
//			string[] nodes_name = new string[0];
//			ret = mySapObject.SapModel.PointObj.GetNameList(ref nodes_nr, ref nodes_name);
//			List<string> nodes_name_list = new List<string>(nodes_name);

//			int tot = mySapObject.SapModel.PointObj.Count();
//			if (tot != nodes_nr || tot != nodes_name_list.Distinct().ToList().Count)
//			{
//				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plese add unique ID to all nodes!");
//				return;
//			}

//			for (int i = 0; i < nodes_nr; i++)
//			{
//				//search in selected beam if have same name
//				int j_iter = -1;
//				for (int j = 0; j < new_nodes.Count; j++)
//				{
//					ElementIdAttributeModel attr;
//					try
//					{
//						attr = (ElementIdAttributeModel)new_nodes[j].Value.Attributes.Where(a => a is ElementIdAttributeModel).First();
//					}
//					catch (Exception)
//					{
//						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Add ID to nodes!");
//						return;
//					}

//					if (attr.Value == nodes_name[i])
//					{
//						j_iter = j;
//						j = new_nodes.Count(); //exit
//					}
//				}
//				if (j_iter >= 0) //I have found
//				{
//					//dev = dev + " " + beams_name[i] + "\n";
//					int nr_load = -1;
//					string[] node_name = new string[0];
//					string[] load_pat = new string[0];
//					int[] my_type = new int[0];
//					int[] LC_step = new int[0];
//					string[] c_sys = new string[0];
//					double[] F1 = new double[0];
//					double[] F2 = new double[0];
//					double[] F3 = new double[0];
//					double[] M1 = new double[0];
//					double[] M2 = new double[0];
//					double[] M3 = new double[0];

//					ret = mySapObject.SapModel.PointObj.GetLoadForce(nodes_name[i], ref nr_load, ref node_name, ref load_pat, ref LC_step, ref c_sys, ref F1, ref F2, ref F3, ref M1, ref M2, ref M3);

//					for (int j = 0; j < nr_load; j++)
//					{
//						if (load_patterns.Contains(load_pat[j]) || exportAll)
//						{
//							if (c_sys[j] != "GLOBAL")
//							{
//								AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Forces/Moments on local axis not supported");
//								return;
//							}
//							dev = dev + " name = " + node_name[j] + "\n";
//							dev = dev + " load pattern = " + load_pat[j] + "\n";
//							dev = dev + " LC step = " + LC_step[j] + "\n";
//							dev = dev + " c sys = " + c_sys[j] + "\n";
//							dev = dev + " F1 = " + F1[j] + "\n";
//							dev = dev + " F2 = " + F2[j] + "\n";
//							dev = dev + " F3 = " + F3[j] + "\n";
//							dev = dev + " M1 = " + M1[j] + "\n";
//							dev = dev + " M2 = " + M2[j] + "\n";
//							dev = dev + " M3 = " + M3[j] + "\n \n";

//							//Add load to new points
//							string id = load_pat[j];
//							string name = load_pat[j];
//							LoadCaseModel loadCase = new LoadCaseModel(id, name);


//							if (F1[j] != 0 || F2[j] != 0 || F3[j] != 0)
//							{
//								NodeForceLoadModel load_force = new NodeForceLoadModel(loadCase, new Vector3d(F1[j], F2[j], F3[j]));
//								new_nodes[j_iter].Value.Loads.Add(load_force);
//							}

//							if (M1[j] != 0 || M2[j] != 0 || M3[j] != 0)
//							{
//								NodeMomentLoadModel load_moment = new NodeMomentLoadModel(loadCase, new Vector3d(M1[j], M2[j], M3[j]));
//								new_nodes[j_iter].Value.Loads.Add(load_moment);
//							}
//						}
//					}
//				}
//			}
//			mySapObject.ApplicationExit(false);
//		}
//	}
//}

