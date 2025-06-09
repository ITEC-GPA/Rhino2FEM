//using SAP2000v1;
//using Grasshopper.Kernel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Rhino2Fem.Core.Helper;

//namespace FeMM.Grasshopper.Components.SAPExtra
//{
//    public class SAPAreaLoaderComponent : GH_Component
//	{
//		/// <summary>
//		/// Initializes a new instance of the SAPAreaLoaderComponent class.
//		/// </summary>
//		public SAPAreaLoaderComponent()
//		  : base("Area Loader", "ALC", "SAP Area Loader", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_SAPCHECKS)
//		{
//			_run = false;
//		}

//		bool _run;

//		public override void CreateAttributes()
//		{
//			GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes attr = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Run");
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
//			pManager.AddGenericParameter("Plate", "P", "Plates", GH_ParamAccess.list);
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
//			pManager.AddGenericParameter("Plates", "P", "Loaded plates", GH_ParamAccess.list);
//			pManager.AddTextParameter("dev", "dev", "", GH_ParamAccess.item);
//		}

//		protected override void SolveInstance(IGH_DataAccess DA)
//		{
//			List<GH_Plate> plates = new List<GH_Plate>();
//			string path = "";
//			List<string> loadPatterns = new List<string>();
//			string sapExe = "";

//			if (!DA.GetDataList(0, plates))
//				return;
//			if (!DA.GetData(1, ref path))
//				return;
//			DA.GetDataList(2, loadPatterns);
//			DA.GetData(3, ref sapExe);

//			if (_run)
//			{
//				List<GH_Plate> newPlates = new List<GH_Plate>();
//				for (int i = 0; i < plates.Count; i++)
//					newPlates.Add(new GH_Plate(plates[i]));

//				string dev = "";
//				LoadPlates(sapExe, path, loadPatterns, ref newPlates, ref dev);

//				DA.SetDataList(0, newPlates);
//				DA.SetData(1, dev);
//			}

//			_run = false;
//		}

//		protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPAreaLoaderComponent;

//		public override Guid ComponentGuid => new Guid("bbcedfb7-e777-421a-a7da-afcc1ef7d9ee");

//		protected void LoadPlates(string sapExe, string path_model, List<string> load_patterns, ref List<GH_Plate> newPlates, ref string dev)
//		{
//			bool AttachToInstance = false;
//			ExtraSapHelper.InitializeModel(!AttachToInstance, sapExe, path_model, out cOAPI mySapObject, out cSapModel mySapModel, out cHelper myHelper);

//			bool exportAll = false;
//			if (load_patterns == null || load_patterns.Count == 0)
//				exportAll = true;

//			int AreaNr = -1;
//			string[] AreaName = new string[0];
//			mySapObject.SapModel.AreaObj.GetNameList(ref AreaNr, ref AreaName);
//			List<string> AreaNameList = new List<string>(AreaName);

//			int tot = mySapObject.SapModel.AreaObj.Count();
//			if (tot != AreaNr || tot != AreaNameList.Distinct().ToList().Count)
//			{
//				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Plese add unique ID to all beams!");
//				return;
//			}

//			for (int i = 0; i < AreaNr; i++)
//			{
//				//search in selected beam if have same name
//				//ElementIdAttributeModel attr = (ElementIdAttributeModel)el.Attributes.FirstOrDefault(a => a is ElementIdAttributeModel);
//				int j_iter = -1;
//				for (int j = 0; j < newPlates.Count; j++)
//				{
//					ElementIdAttributeModel attr;
//					try
//					{
//						attr = (ElementIdAttributeModel)newPlates[j].Value.Attributes.Where(a => a is ElementIdAttributeModel).First();
//					}
//					catch (Exception)
//					{
//						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please add ID to beam");
//						return;
//					}
//					if (attr.Value == AreaName[i])
//					{
//						j_iter = j;
//						j = newPlates.Count(); //exit
//					}
//				}
//				if (j_iter >= 0) //I have found
//				{
//					#region Uniform to frame

//					//dev = dev + " " + beams_name[i] + "\n";
//					int nr_load = -1;
//					string[] area_name = new string[0];
//					string[] load_pat = new string[0];
//					string[] c_sys = new string[0];
//					int[] dir = new int[0];
//					double[] value = new double[0];
//					int[] dist_type = new int[0];

//					if (mySapObject.SapModel.AreaObj.GetLoadUniformToFrame(AreaName[i], ref nr_load, ref area_name, ref load_pat, ref c_sys, ref dir, ref value, ref dist_type) != 0)
//						return;

//					for (int j = 0; j < nr_load; j++)
//					{
//						if (load_patterns.Contains(load_pat[j]) || exportAll)
//						{
//							dev = dev + " name = " + area_name[j] + "\n";
//							dev = dev + " load pattern = " + load_pat[j] + "\n";
//							dev = dev + " c sys = " + c_sys[j] + "\n";
//							dev = dev + " dir = " + dir[j] + "\n";
//							dev = dev + " value = " + value[j] + "\n";
//							dev = dev + " dist type = " + dist_type[j] + "\n";

//							//Add load to new beam
//							string id = load_pat[j];
//							string name = load_pat[j];
//							LoadCaseModel loadCase = new LoadCaseModel(id, name);

//							CoordinateSystemModel cs;
//							if (c_sys[j] == "Local")
//								cs = null;
//							else
//								cs = new CoordinateSystemModel(c_sys[j]);


//							PlateLoadUniformToFrameModel.LoadData loadData = new PlateLoadUniformToFrameModel.LoadData()
//							{
//								CoordinateSystem = cs,
//								Direction = ((PlateLoadUniformToFrameModel.LoadData.Directions)dir[j]),
//								DistributionType = (PlateLoadUniformToFrameModel.LoadData.DistributionTypes)dist_type[j],
//								Value = value[j],
//							};

//							PlateLoadUniformToFrameModel load = new PlateLoadUniformToFrameModel(loadCase, loadData);
//							newPlates[j_iter].Value.Loads.Add(load);
//						}
//					}

//					#endregion

//					#region Distribuited

//					/*
//					if (mySapObject.SapModel.AreaObj.GetLoadUniform(AreaName[i], ref nr_load, ref area_name, ref load_pat, ref c_sys, ref dir, ref value) != 0)
//						return;

//					for (int j = 0; j < nr_load; j++)
//					{
//						if (load_patterns.Contains(load_pat[j]) || exportAll)
//						{
//							dev = dev + " name = " + area_name[j] + "\n";
//							dev = dev + " load pattern = " + load_pat[j] + "\n";
//							dev = dev + " c sys = " + c_sys[j] + "\n";
//							dev = dev + " dir = " + dir[j] + "\n";
//							dev = dev + " value = " + value[j] + "\n";
//							dev = dev + " dist type = " + dist_type[j] + "\n";

//							//Add load to new beam
//							string id = load_pat[j];
//							string name = load_pat[j];
//							LoadCaseModel loadCase = new LoadCaseModel(id, name);

//							CoordinateSystemModel cs;
//							if (c_sys[j] == "Local")
//								cs = null;
//							else
//								cs = new CoordinateSystemModel(c_sys[j]);

//							PlatePressureModel.LoadData loadData = new PlatePressureModel.LoadData()
//							{
//								CoordinateSystem = cs,
//								Face = PlatePressureModel.LoadFace.Top,
//								P = value[j],
//								Projected = false,
//							};

//							PlatePressureModel load = new PlatePressureModel(loadCase, loadData);
//							newPlates[j_iter].Value.Loads.Add(load);
//						}
//					}
//					*/

//					#endregion
//				}
//			}

//			mySapObject.ApplicationExit(false);
//		}
//	}
//}
