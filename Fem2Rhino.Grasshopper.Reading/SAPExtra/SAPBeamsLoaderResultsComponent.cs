//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;
//using Grasshopper.Kernel;
//using Rhino.Geometry;
//using SAP2000v1;
//using System.Linq;
//using System.ComponentModel;
//using System.Collections;
//using System.Threading.Tasks;
//using FeMM.Common.Helpers;
//using FeMM.Grasshopper.Helpers;
//using FeMM.Grasshopper.DataTypes.FeMM;

//namespace FeMM.Grasshopper.Components.SAPExtra
//{
//    public class SAPBeamsLoaderResultsComponent : GH_Component
//	{
//		bool _run;
//        ComponentAttributes.ComponentOneButtonAttributes attr;
//		List<Common.Models.BeamModel> output_beam;
//		string dev;

//		/// <summary>
//		/// Initializes a new instance of the FeMMBeamsLoaderResults class.
//		/// </summary>
//		public SAPBeamsLoaderResultsComponent()
//		  : base("Beams Loader Results", "SAP Beams Loader Results", "Get results after analysis in beams", CategoryNameConstants.CATEGORY_CHECKS, CategoryNameConstants.SUBCATEGORY_SAPEXTRA)
//		{
//			_run = false;
//			output_beam = new List<Common.Models.BeamModel>();
//			dev = "";
//		}

//		public override void CreateAttributes()
//		{
//			attr = new ComponentAttributes.ComponentOneButtonAttributes(this, "Run");

//			attr.ButtonPressed += () =>
//			{
//				attr.Text = "Running...";
//				output_beam = new List<FeMM.Common.Models.BeamModel>();
//				_run = true;
//				dev = "";

//				ExpireSolution(true);
//			};
//			m_attributes = attr;
//		}

//		/// <summary>
//		/// Registers all the input parameters for this component.
//		/// </summary>
//		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Beams", "B", "FeMM beam to read results", GH_ParamAccess.list);
//			pManager.AddGenericParameter("Load Cases", "LC", "Load case or Combo to be loaded", GH_ParamAccess.list);
//			pManager.AddGenericParameter("SapFile", "SapFile", "Sdb file runned", GH_ParamAccess.item);
//		}

//		/// <summary>
//		/// Registers all the output parameters for this component.
//		/// </summary>
//		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//		{
//			pManager.AddGenericParameter("Beam", "B", "Loaded FeMM beam", GH_ParamAccess.item);
//			pManager.AddGenericParameter("dev", "General Output", "General Output", GH_ParamAccess.item);
//		}

//		/// <summary>
//		/// This is the method that actually does the work.
//		/// </summary>
//		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//		/// 

//		//ManualResetEvent mre = new ManualResetEvent(false);

//		protected override void SolveInstance(IGH_DataAccess DA)
//		{
//			if (_run)
//			{
//				if (output_beam.Count() == 0) //result not yet processed
//				{
//					string sapfile = "";
//					if (!DA.GetData("SapFile", ref sapfile)) { return; }

//					List<string> combo = new List<string>();
//					if (!DA.GetDataList<string>("LC", combo)) { return; }


///* Unmerged change from project 'FeMM.Grasshopper (net7.0-windows)'
//Before:
//					List<FeMM.Grasshopper.DataTypes.GH_Beam> input_beam = new List<FeMM.Grasshopper.DataTypes.GH_Beam>();
//					if (!DA.GetDataList<FeMM.Grasshopper.DataTypes.GH_Beam>("Beam", input_beam)) { return; }
//After:
//					List<GH_Beam> input_beam = new List<GH_Beam>();
//					if (!DA.GetDataList<GH_Beam>("Beam", input_beam)) { return; }
//*/
//					List<DataTypes.FeMM.GH_Beam> input_beam = new List<DataTypes.FeMM.GH_Beam>();
//					if (!DA.GetDataList<DataTypes.FeMM.GH_Beam>("Beam", input_beam)) { return; }

//					BackgroundWorker worker = new BackgroundWorker();
//					Hashtable parameters = new Hashtable();

//					parameters.Add("sapfile", sapfile);
//					parameters.Add("combo", combo);
//					parameters.Add("input_beam", input_beam);

//					worker.DoWork += Execute;
//					worker.RunWorkerAsync(parameters);
//					worker.RunWorkerCompleted += finish;

//					void finish(object sender, RunWorkerCompletedEventArgs e)
//					{
//						ExpireSolution(true);
//						return;
//					}
//				}
//				else
//				{

///* Unmerged change from project 'FeMM.Grasshopper (net7.0-windows)'
//Before:
//					List<FeMM.Grasshopper.DataTypes.GH_Beam> out_b = new List<FeMM.Grasshopper.DataTypes.GH_Beam>();
//After:
//					List<GH_Beam> out_b = new List<GH_Beam>();
//*/
//					List<DataTypes.FeMM.GH_Beam> out_b = new List<DataTypes.FeMM.GH_Beam>();
//					for (int i = 0; i < output_beam.Count; i++)
//					{
//						out_b.Add(new GH_Beam(output_beam[i]));
//					}
//					DA.SetDataList(0, out_b);
//					DA.SetData(1, dev);
//					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, dev);
//					_run = false;
//				}
//			}
//		}

//		public void Execute(object sender, DoWorkEventArgs e)
//		{
//			Hashtable parameters = e.Argument as Hashtable;
//			string sapfile = parameters["sapfile"].ToString();
//			List<string> combo = (List<string>)parameters["combo"];

///* Unmerged change from project 'FeMM.Grasshopper (net7.0-windows)'
//Before:
//			List<FeMM.Grasshopper.DataTypes.GH_Beam> input_beam = (List<FeMM.Grasshopper.DataTypes.GH_Beam>)parameters["input_beam"];
//After:
//			List<GH_Beam> input_beam = (List<GH_Beam>)parameters["input_beam"];
//*/
//			List<DataTypes.FeMM.GH_Beam> input_beam = (List<DataTypes.FeMM.GH_Beam>)parameters["input_beam"];
//			//List<FeMM.Grasshopper.DataTypes.GH_Beam> output_beam = new List<FeMM.Grasshopper.DataTypes.GH_Beam>();

//			//set the following flag to true to attach to an existing instance of the program
//			//otherwise a new instance of the program will be started
//			bool AttachToInstance;
//			AttachToInstance = false;

//			//set the following flag to true to manually specify the path to SAP2000.exe
//			//this allows for a connection to a version of SAP2000 other than the latest installation
//			//otherwise the latest installed version of SAP2000 will be launched
//			bool SpecifyPath;
//			SpecifyPath = false;


//			//if the above flag is set to true, specify the path to SAP2000 below
//			string ProgramPath;
//			ProgramPath = @"C:\Program Files\Computers and Structures\SAP2000 24\SAP2000.exe";

//			//dimension the SapObject as cOAPI type
//			cOAPI mySapObject = null;

//			//Use ret to check if functions return successfully (ret = 0) or fail (ret = nonzero)
//			int ret = 0;

//			//create API helper object
//			cHelper myHelper;
//			try
//			{
//				myHelper = new Helper();
//			}
//			catch (Exception)
//			{
//				Console.WriteLine("Cannot create an instance of the Helper object");
//				_run = false;
//				return;
//			}


//			if (AttachToInstance)
//			{
//				//attach to a running instance of SAP2000
//				try
//				{
//					//get the active SapObject
//					mySapObject = myHelper.GetObject("CSI.SAP2000.API.SapObject");
//				}
//				catch (Exception)
//				{
//					Console.WriteLine("No running instance of the program found or failed to attach.");
//					_run = false;
//					return;
//				}
//			}
//			else
//			{
//				if (SpecifyPath)
//				{
//					//'create an instance of the SapObject from the specified path
//					try
//					{
//						//create SapObject
//						mySapObject = myHelper.CreateObject(ProgramPath);
//					}
//					catch (Exception)
//					{
//						Console.WriteLine("Cannot start a new instance of the program from " + ProgramPath);
//						_run = false;
//						return;
//					}
//				}
//				else
//				{
//					//'create an instance of the SapObject from the latest installed SAP2000
//					try
//					{
//						//create SapObject
//						mySapObject = myHelper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
//					}
//					catch (Exception)
//					{
//						Console.WriteLine("Cannot start a new instance of the program.");
//						_run = false;
//						return;
//					}
//				}
//				//start SAP2000 application
//				ret = mySapObject.ApplicationStart();
//				if (ret != 0) { MessageBox.Show("Error application start"); _run = false; return; }
//			}

//			//initialize model
//			ret = mySapObject.SapModel.InitializeNewModel();
//			if (ret != 0) { MessageBox.Show("Error initialize model"); _run = false; return; }

//			//open file
//			ret = mySapObject.SapModel.File.OpenFile(sapfile);
//			if (ret != 0) { MessageBox.Show("Error open file"); _run = false; return; }

//			//check if analysis has been runned
//			bool runned = mySapObject.SapModel.GetModelIsLocked();
//			if (runned == false)
//			{
//				MessageBox.Show("Please run the analysis!");
//				_run = false;
//				return;
//			}

//			//clear all case and combo output selections
//			ret = mySapObject.SapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
//			if (ret != 0) { MessageBox.Show("Error deselect previous combo for output"); _run = false; return; }

//			//set case and combo output selections
//			for (int i = 0; i < combo.Count; i++)
//			{
//				ret = mySapObject.SapModel.Results.Setup.SetCaseSelectedForOutput(combo[i]);
//				if (ret != 0)
//				{
//					ret = mySapObject.SapModel.Results.Setup.SetComboSelectedForOutput(combo[i]);
//					if (ret != 0) { MessageBox.Show("Error set combo/load case of output"); _run = false; return; }
//				}
//			}

//			//delete all old results of beam
//			for (int i = 0; i < input_beam.Count; i++)
//			{
//				input_beam[i].Value.Results.Clear();
//			}

//			//get all beams
//			int nr_frame = 0;
//			string[] name_frame = new string[0];
//			ret = mySapObject.SapModel.FrameObj.GetNameList(ref nr_frame, ref name_frame);
//			if (ret != 0) { MessageBox.Show("Error get beams"); _run = false; return; }

//			//check if double beams in input or if beams without ID
//			HashSet<string> unique_label = new HashSet<string>();
//			//Parallel.For(0, input_beam.Count, 
//			for (int k = 0; k < input_beam.Count; k++)
//			{
//				FeMM.Common.Models.ElementIdAttributeModel attr;
//				try
//				{
//					attr = (FeMM.Common.Models.ElementIdAttributeModel)input_beam[k].Value.Attributes.Where(a => a is FeMM.Common.Models.ElementIdAttributeModel).First();
//					unique_label.Add(attr.Value);
//					if (k >= unique_label.Count)
//					{
//						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Duplicate beam! " + attr.Value);
//						_run = false; return;
//					}
//				}
//				catch (Exception)
//				{
//					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please add ID to beam");
//					_run = false; return;
//				}
//			}

//			string[] labels = unique_label.ToArray();

//			//get frame forces foreach beam
//			object _obj = new object();
//			int iter = 0;
//			Parallel.For(0, input_beam.Count, i =>
//			//for (int i = 0; i < input_beam.Count; i++)
//			{
//				bool found = false;
//				string label = labels[i];

//				for (int k = 0; k < nr_frame; k++) //search for the right beam
//				{
//					if (label == name_frame[k]) //right beam found!
//					{
//						found = true;
//						FeMM.Common.Models.BeamModel new_beam = new FeMM.Common.Models.BeamModel(input_beam[i].Value);
//						output_beam.Add(new_beam); //Add to output beams
//						int index = output_beam.Count - 1;
//						int number_result = 0;
//						string[] obj = new string[0];
//						double[] obj_station = new double[0];
//						string[] element = new string[0];
//						double[] element_station = new double[0];
//						string[] load_case = new string[0];
//						string[] step_type = new string[0];
//						double[] step_num = new double[0];
//						double[] P = new double[0];
//						double[] V2 = new double[0];
//						double[] V3 = new double[0];
//						double[] T = new double[0];
//						double[] M2 = new double[0];
//						double[] M3 = new double[0];

//						ret = mySapObject.SapModel.Results.FrameForce(label, eItemTypeElm.ObjectElm, ref number_result, ref obj, ref obj_station, ref element, ref element_station, ref load_case, ref step_type, ref step_num, ref P, ref V2, ref V3, ref T, ref M2, ref M3);
//						if (ret != 0) { MessageBox.Show("Error get forces for " + name_frame[k]); _run = false; return; }

//						for (int j = 0; j < number_result; j++)
//						{
//							//dev = dev + "obj = " + obj[j] + "\n";
//							//dev = dev + "element = " + element[j] + "\n";
//							//dev = dev + "obj station = " + obj_station[j] + "\n";
//							//dev = dev + "el station = " + element_station[j] + "\n";
//							//dev = dev + "load case = " +load_case[j] + "\n";

//							FeMM.Common.Models.CaseModel comb = new FeMM.Common.Models.CaseModel(load_case[j]);
//							ExtraSapHelper.ConvertStressesFromSAP2000(output_beam[index], ref M2[j], ref M3[j], ref V2[j], ref V3[j]);
//							FeMM.Common.Models.BeamForceResultModel res = new FeMM.Common.Models.BeamForceResultModel(comb, obj_station[j], step_num[j], step_type[j]);
//							res.AxialForce = P[j];
//							res.BendingMoment = new Vector2d(M2[j], M3[j]);
//							res.Torque = T[j];
//							res.ShearForce = new Vector2d(V2[j], V3[j]);

//							output_beam[index].Results.Add(res);
//						}
//						k = nr_frame; //beam found -> exit
//					}
//					if (k == (nr_frame - 1) && found == false)
//					{
//						//BEAM NOT FOUND!!!
//						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Beam in input not found in model! ID = " + label);
//						dev = dev + "Beam in input not found in model! ID = " + label + "\n";
//					}
//					//Rhino.UI.StatusBar.UpdateProgressMeter(i, true);
//				}
//				lock (_obj)
//				{
//					iter++;
//					attr.Text = "Processing element " + iter + " of " + input_beam.Count();
//				}
//			});


//			//'close Sap2000
//			ret = mySapObject.ApplicationExit(false);
//			if (ret != 0) { MessageBox.Show("Error exit SAP"); _run = false; return; }
//			attr.Text = "Finish";
//		}

//		/// <summary>
//		/// Provides an Icon for the component.
//		/// </summary>
//		protected override System.Drawing.Bitmap Icon => Properties.Resources.SAPBeamResultLoaderIcon;

//		/// <summary>
//		/// Gets the unique ID for this component. Do not change this ID after release.
//		/// </summary>
//		public override Guid ComponentGuid
//		{
//			get { return new Guid("9e1645eb-0748-472d-8157-4b864b4061a0"); }
//		}
//	}
//}