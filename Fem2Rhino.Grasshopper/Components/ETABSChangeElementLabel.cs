using SAP2000v1;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class ETABSChangeElementLabel : GH_Component
	{
		private bool _run = false;

		protected Dictionary<int, string> _elementPresets = new Dictionary<int, string>()
		{
			{ 0, "0 Point" },
			{ 1, "1 Frame" },
			{ 2, "2 Area" },
			{ 3, "3 Tendon" },
			{ 4, "4 Cable" },
			{ 5, "5 Link" },
			{ 6, "6 Solid" },
		};

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public ETABSChangeElementLabel()
			: base("Rename Elements", "R", "Rename elements in ETABS", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVEETABS)
		{
		}

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Rename");
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
			pManager.AddTextParameter("Model Path", "MP", "ETABS model path to open. If empty it will open an empty file", GH_ParamAccess.item, "");
			pManager[0].Optional = true;
			pManager.AddBooleanParameter("Attach to Instance", "AI", "Attach to instance. If true, 'model path' will be ignored", GH_ParamAccess.item, false);
			pManager[1].Optional = true;

			int i = pManager.AddIntegerParameter("Element Type", "T", "", GH_ParamAccess.item);
			Param_Integer mtParam = pManager[i] as Param_Integer;
			foreach (KeyValuePair<int, string> v in _elementPresets)
				mtParam.AddNamedValue(v.Value, v.Key);

			pManager.AddTextParameter("Old ID", "OI", "The list of the IDs to change", GH_ParamAccess.list);
			pManager.AddTextParameter("New ID", "NI", "The list of the new IDs", GH_ParamAccess.list);
		}

		/// <summary>
		/// Registers all the output parameters for this component.
		/// </summary>
		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{

		}

		/// <summary>
		/// This is the method that actually does the work.
		/// </summary>
		/// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
		protected override void SolveInstance(IGH_DataAccess DA)
		{
			string modelPath = "";
			bool attachToInstance = false;
			int elementType = 0;
			List<string> oldID = new List<string>();
			List<string> newID = new List<string>();

			//input
			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);
			if (!DA.GetData(2, ref elementType))
				return;
			if (!DA.GetDataList(3, oldID))
				return;
			if (!DA.GetDataList(4, newID))
				return;

			if (_run)
			{

				if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
				{
					modelPath = string.Empty;
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ETABS model path not valid, opening an empty file");
					return;
				}
				if (elementType < 0 || elementType > 7)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Element type must be >= 0 and <= 7");
					return;
				}
				if (oldID.Count != newID.Count)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Old ID and Nes ID lists must have the same length");
					return;
				}

				try
				{
					cSapModel etabsModel = F2RModelHelper.GetCSIEtabsModel(out _, modelPath, attachToInstance);

					if (etabsModel != null)
					{
						for (int i = 0; i < oldID.Count; i++)
						{
							int ret;
							switch (elementType)
							{
								case 0:
									ret = etabsModel.PointObj.ChangeName(oldID[i], newID[i]);
									break;
								case 1:
									ret = etabsModel.FrameObj.ChangeName(oldID[i], newID[i]);
									break;
								case 2:
									ret = etabsModel.AreaObj.ChangeName(oldID[i], newID[i]);
									break;
								case 3:
									ret = etabsModel.TendonObj.ChangeName(oldID[i], newID[i]);
									break;
								case 4:
									ret = etabsModel.CableObj.ChangeName(oldID[i], newID[i]);
									break;
								case 5:
									ret = etabsModel.LinkObj.ChangeName(oldID[i], newID[i]);
									break;
								case 6:
									ret = etabsModel.SolidObj.ChangeName(oldID[i], newID[i]);
									break;
								default:
									ret = 1;
									break;
							}

							if (ret != 0)
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"OldID n° {i} don't exist or NewID n° {i} already exist");
						}

						if (!attachToInstance)
						{
							if (etabsModel.File.Save() != 0)
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to save the file");

						}
						else
						{
							if (etabsModel.View.RefreshView() != 0)
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Fail to refresh the view");
						}
					}

					_run = false;
				}
				catch (Exception)
				{
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic error");
					_run = false;
				}
			}
		}

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RChangeElementLabelIcon;

		public override Guid ComponentGuid => new Guid("68458bd5-fc78-4e1a-ac6a-c32d1ac7b00d");
	}
}
