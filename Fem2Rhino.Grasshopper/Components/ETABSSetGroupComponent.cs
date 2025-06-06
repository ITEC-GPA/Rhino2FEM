using SAP2000v1;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public class ETABSSetGroupComponent : GH_Component
	{
		private bool _run = false;

		/// <summary>
		/// Initializes a new instance of the MyComponent1 class.
		/// </summary>
		public ETABSSetGroupComponent()
			: base("Assign to group", "AG", "Assign group at selected elements in ETABS", Rhino2Fem.Core.Helper.Constants.CATEGORY_FEM2RHINO, Rhino2Fem.Core.Helper.Constants.SUBCATEGORY_F2R_INTERACTIVEETABS)
		{
		}

		public override void CreateAttributes()
		{
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes buttonAttributes = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Assign");
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
			pManager.AddGeometryParameter("Elements", "E", "F2R element to select", GH_ParamAccess.list);
			pManager.AddTextParameter("Group Name", "G", "The name of the group", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Keep model visible", "V", "If true and if is not attch to istance, keep the model open and visible", GH_ParamAccess.item, true);
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
			List<IGH_GeometricGoo> elements = new List<IGH_GeometricGoo>();
			string inputGroupName = string.Empty;
			bool visible = true;

			//input
			DA.GetData(0, ref modelPath);
			DA.GetData(1, ref attachToInstance);
			if (!DA.GetDataList(2, elements))
				return;
			DA.GetData(3, ref inputGroupName);
			DA.GetData(4, ref visible);

			if (_run)
			{

				if (!System.IO.File.Exists(modelPath) && !attachToInstance) // model path non valido, Se path modello non valido fa partire con stringa vuota (file vuoto)
				{
					modelPath = string.Empty;
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Sap model path not valid, opening an empty file");
				}

				try
				{
					cSapModel sapModel = F2RModelHelper.GetCSIEtabsModel(out SAP2000v1.cOAPI sapObject, modelPath, attachToInstance);

					sapModel.SelectObj.ClearSelection();

					int groupNumber = 0;
					string[] groupNames = new string[0];
					sapModel.GroupDef.GetNameList(ref groupNumber, ref groupNames);

					if (!groupNames.Contains(inputGroupName))
						sapModel.GroupDef.SetGroup(inputGroupName);

					RhinoDoc doc = RhinoDoc.ActiveDoc;
					for (int i = 0; i < elements.Count; i++)
					{
						Guid guid = elements[i].ReferenceID;
						if (guid != Guid.Empty)
						{
							RhinoObject rhinoObj = doc.Objects.FindId(guid);

							if (rhinoObj != null)
							{
								string id = rhinoObj.Attributes.GetUserString("F2R_ID");
								if (rhinoObj.GetType() == typeof(PointObject))
								{
									sapModel.PointObj.SetGroupAssign(id, inputGroupName, false);
								}
								else if (rhinoObj.GetType() == typeof(CurveObject))
								{
									sapModel.FrameObj.SetGroupAssign(id, inputGroupName, false);
								}
								else if (rhinoObj.GetType() == typeof(SurfaceObject) || rhinoObj.GetType() == typeof(BrepObject))
								{
									sapModel.AreaObj.SetGroupAssign(id, inputGroupName, false);
								}
								else
								{
									AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element n° {i} is not a point or a curve or a surface");
								}
							}
							else
							{
								AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element n° {i} don't exist in Rhino");
							}
						}
						else
						{
							AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element {i} don't have a valid GUID");
						}
					}

					if (!attachToInstance)
					{
						sapModel.File.Save();
						if (!visible)
							sapObject.ApplicationExit(false);
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

		//protected override System.Drawing.Bitmap Icon => Properties.Resources.F2RSetGroupIcon;

		public override Guid ComponentGuid => new Guid("4e87c502-a163-48d6-a6f6-585da0f2e5f5");
	}
}
