using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Components.F2R
{
	public static class F2RModelHelper
	{
		private static void AddObjToHashSet(Layer layer, HashSet<RhinoObject> hashset)
		{
			RhinoObject[] childRhobjs = RhinoDoc.ActiveDoc.Objects.FindByLayer(layer);
			if (childRhobjs != null)
			{
				for (int k = 0; k < childRhobjs.Length; k++)
					hashset.Add(childRhobjs[k]);
			}
		}

		public static void AddLayerToHashSet(Layer layer, string layerToMatch, HashSet<RhinoObject> hashset)
		{
			if (layer.FullPath == layerToMatch)
			{
				RecursiveAdd(layer, hashset);
			}
		}

		public static void RecursiveAdd(Layer layer, HashSet<RhinoObject> hashset)
		{
			AddObjToHashSet(layer, hashset);

			Layer[] children = layer.GetChildren();
			if (children != null && children.Length > 0)
			{
				for (int j = 0; j < children.Length; j++)
				{
					RecursiveAdd(children[j], hashset);
				}
			}
		}

		public static void GetAllLayerChildren(Layer layer, ref List<Layer> layers)
		{
			layers.Add(layer);

			Layer[] children = layer.GetChildren();
			if (children != null && children.Length > 0)
			{
				for (int j = 0; j < children.Length; j++)
				{
					GetAllLayerChildren(children[j], ref layers);
				}
			}
		}

		public static Layer[] GetAllLayerChildren(Layer layer)
		{
			List<Layer> layers = new List<Layer>();

			GetAllLayerChildren(layer, ref layers);

			return layers.ToArray();
		}

		public static CSIEditingTable GetAllFields(SAP2000v1.cSapModel model, string tableName)
		{
			int NumberFields = 0;
			int TableVersion = 0;
			string[] FieldKey = new string[0];
			string[] FieldName = new string[0];
			string[] Description = new string[0];
			string[] UnitsString = new string[0];
			bool[] IsImportable = new bool[0];

			if (model.DatabaseTables.GetAllFieldsInTable(tableName, ref TableVersion, ref NumberFields, ref FieldKey, ref FieldName, ref Description, ref UnitsString, ref IsImportable) == 0)
			{
				CSIEditingTable table = new CSIEditingTable(tableName)
				{
					NumberFields = NumberFields,
					TableVersion = TableVersion,
					FieldKey = FieldKey,
					Description = Description,
					FieldName = FieldName,
					UnitsString = UnitsString,
					IsImportable = IsImportable,
				};

				return table;
			}
			else
				return null;
		}

		public static CSIEditingTable GetAllFields(ETABSv1.cSapModel model, string tableName)
		{
			int NumberFields = 0;
			int TableVersion = 0;
			string[] FieldKey = new string[0];
			string[] FieldName = new string[0];
			string[] Description = new string[0];
			string[] UnitsString = new string[0];
			bool[] IsImportable = new bool[0];

			if (model.DatabaseTables.GetAllFieldsInTable(tableName, ref TableVersion, ref NumberFields, ref FieldKey, ref FieldName, ref Description, ref UnitsString, ref IsImportable) == 0)
			{
				CSIEditingTable table = new CSIEditingTable(tableName)
				{
					NumberFields = NumberFields,
					TableVersion = TableVersion,
					FieldKey = FieldKey,
					Description = Description,
					FieldName = FieldName,
					UnitsString = UnitsString,
					IsImportable = IsImportable,
				};

				return table;
			}
			else
				return null;
		}

		public static SAP2000v1.cSapModel GetSapModel(out SAP2000v1.cOAPI sapObject, string modelPath = "", bool attachToInstance = false)
		{
            SAP2000v1.cHelper myHelper; //create API helper object
            try
            {
                myHelper = new SAP2000v1.Helper();
            }
            catch
            {
                throw;
            }
            if (attachToInstance)
			{
				try
				{
					// Get the active SapObject
					sapObject = myHelper.GetObject("CSI.SAP2000.API.SapObject");
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				try
				{
					sapObject = myHelper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
				}
				catch
				{
					throw;
				}

				if (sapObject != null)
				{
					if (string.IsNullOrEmpty(modelPath))
						sapObject.ApplicationStart(SAP2000v1.eUnits.N_mm_C, true);
					else
						sapObject.ApplicationStart(SAP2000v1.eUnits.N_mm_C, true, modelPath);
				}
			}

			return sapObject.SapModel != null ? sapObject.SapModel : null;
		}

		public static ETABSv1.cSapModel GetEtabsModel(out ETABSv1.cOAPI sapObject, string modelPath = "", bool attachToInstance = false)
		{
            ETABSv1.cHelper myHelper; //create API helper object
            try
            {
                myHelper = new ETABSv1.Helper();
            }
            catch
            {
                throw;
            }
            if (attachToInstance)
			{
				try
				{
					// Get the active SapObject
					sapObject = (ETABSv1.cOAPI)myHelper.GetObject("CSI.ETABS.API.ETABSObject");
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				try
				{
					sapObject = myHelper.CreateObjectProgID("CSI.ETABS.API.ETABSObject");
				}
				catch
				{
					throw;
				}

				if (sapObject != null)
				{
					sapObject.ApplicationStart();
					sapObject.Unhide();
					if (!string.IsNullOrEmpty(modelPath))
						sapObject.SapModel.File.OpenFile(modelPath);
				}
			}

			return sapObject.SapModel;
		}

		public static SAP2000v1.cSapModel GetCSIEtabsModel(out SAP2000v1.cOAPI sapObject, string modelPath = "", bool attachToInstance = false)
		{
            SAP2000v1.cHelper myHelper; //create API helper object
            try
            {
                myHelper = new SAP2000v1.Helper();
            }
            catch
            {
                throw;
            }
            if (attachToInstance)
			{
				try
				{
					// Get the active SapObject
					sapObject = (SAP2000v1.cOAPI)myHelper.GetObject("CSI.ETABS.API.ETABSObject");
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				try
				{
					sapObject = myHelper.CreateObjectProgID("CSI.ETABS.API.ETABSObject");
				}
				catch
				{
					throw;
				}

				if (sapObject != null)
				{
					if (string.IsNullOrEmpty(modelPath))
						sapObject.ApplicationStart(SAP2000v1.eUnits.N_m_C, true);
					else
						sapObject.ApplicationStart(SAP2000v1.eUnits.N_m_C, true, modelPath);
				}
			}

			return sapObject.SapModel;
		}
	}

	public class CSIEditingTable
	{
		public string Name { get; set; }
		public int NumberFields { get; set; }
		public int TableVersion { get; set; }
		public string[] FieldKey { get; set; }
		public string[] FieldName { get; set; }
		public string[] Description { get; set; }
		public string[] UnitsString { get; set; }
		public bool[] IsImportable { get; set; }

		public CSIEditingTable(string name)
		{
			Name = name;
			NumberFields = 0;
			TableVersion = 0;
			FieldKey = new string[0];
			FieldName = new string[0];
			Description = new string[0];
			UnitsString = new string[0];
			IsImportable = new bool[0];
		}
	}
}
