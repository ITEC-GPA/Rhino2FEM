using FeMM.Grasshopper.ComponentAttributes;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Fem2Rhino.CSI.ApiWrapper;
using FeMM.Grasshopper.DataTypes.F2R;
using FeMM.Grasshopper.Helpers;
using Rhino;
using Grasshopper.Kernel.Parameters;
using Fem2Rhino.Common;

namespace FeMM.Grasshopper.Components.F2R
{
	public class BakeSap2000ModelComponent : GH_Component
	{
		protected Dictionary<int, string> _outputPresets = new Dictionary<int, string>()
		{
			{ 0, "All" },
			{ 1, "Geometry assignements" },
			{ 2, "Section properties" },
		};

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public BakeSap2000ModelComponent() 
			: base("Bake SAP2000 Model", "BSM", "Bake SAP2000 model", CategoryNameConstants.CATEGORY_F2R, CategoryNameConstants.SUBCATEGORY_F2R_BAKE)
		{
		}

		private bool _run = false;

		public override void CreateAttributes()
		{
            ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new ComponentAttributes.ComponentOneButtonAttributes(this, "Bake");
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
		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
		{
			pManager.AddTextParameter("Model Path", "MP", "Sap2000 model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
			pManager[0].Optional = true;
			pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
			pManager[1].Optional = true;

			int i = pManager.AddIntegerParameter("Output preferences", "O", "", GH_ParamAccess.item, 1);
			Param_Integer mtParam = pManager[i] as Param_Integer;
			foreach (KeyValuePair<int, string> v in _outputPresets)
				mtParam.AddNamedValue(v.Value, v.Key);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
		{
		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string modelPath = "";
			string sapExePath = "";
			bool attachToInstance = false;
			bool visible = false;
			int type = 0;

			//input
			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);
			DA.GetData(2, ref type);

			if (_run)
			{
				if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sap model path not valid, opening an empty file");
					return;
				}

				int units;

				if (RhinoDoc.ActiveDoc != null)
				{
					string rhinoUnits = RhinoDoc.ActiveDoc.GetUnitSystemName(true, true, true, true);
					if (rhinoUnits.ToLower() == "millimeter" || rhinoUnits.ToLower() == "mm")
					{
						units = Units.N_mm_C;
					}
					else if (rhinoUnits.ToLower() == "meter" || rhinoUnits.ToLower() == "m")
					{
						units = Units.N_m_C;
					}
					else if (rhinoUnits.ToLower() == "centimeter" || rhinoUnits.ToLower() == "cm")
					{
						units = Units.N_cm_C;
					}
					else
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rhino lenght units not recognized. The model will be open using kN, mm, C");
						units = Units.N_mm_C;
					}
				}
				else
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rhino active doc is null");
					units = Units.N_mm_C;
				}

				Model.Options options = new Model.Options();
				if(type == 0)
				{
					options.ExportGeometries = true;
					options.ExportSectionProperties = true;
					options.ExportGeometryDatas = true;
					options.ExportLoads = true;
				}
				if (type == 1)
				{
					options.ExportGeometries = true;
					options.ExportSectionProperties = true;
					options.ExportGeometryDatas = true;
					options.ExportLoads = false;
				}
				if (type == 2)
				{
					options.ExportGeometries = true;
					options.ExportSectionProperties = true;
					options.ExportGeometryDatas = false;
					options.ExportLoads = false;
				}
				if (type == 3)
				{
					options.ExportGeometries = true;
					options.ExportSectionProperties = false;
					options.ExportGeometryDatas = false;
					options.ExportLoads = false;
				}

				GH_CSIModel modelType = new GH_CSIModel(CSIApiWrapper.CsiSoftware.Sap2000, attachToInstance, sapExePath, units, modelPath, visible);
				modelType.Value.Option = options;

				List<string> log = new List<string>();

				try
				{
					modelType.Value.Process();
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to read the model");

					log = modelType.Value.GetLog();
					for(int i = 0; i < log.Count; i++)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);
					return;
				}

				try
				{
					modelType.BakeGeometryCustom(RhinoDoc.ActiveDoc);
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Fail to bake geometries");

					log = modelType.Value.GetLog();
					for (int i = 0; i < log.Count; i++)
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

					return;
				}

				log = modelType.Value.GetLog();
				for (int i = 0; i < log.Count; i++)
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, log[i]);

				_run = false;
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.Fem2RhinoBakeSAPIcon;

		public override Guid ComponentGuid => new Guid("e42e7bb2-3d70-4d90-8f53-581f1e4174f1");
	}
}
