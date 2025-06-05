using Fem2Rhino.CSI.ModelWrapper;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fem2Rhino.Common
{
	public static class F2RModelHelper
	{
		private static readonly Random _random = new Random();

		public static Color GetRandomColor()
		{
			return Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
		}

		private static void GetGroupDictionaryAssociation(List<string> groups, ref Dictionary<string, int> groupsDictionary, ref int count)
		{
			for (int j = 0; j < groups.Count; j++)
			{
				if (!groupsDictionary.ContainsKey(groups[j]))
				{
					groupsDictionary.Add(groups[j], count);
					count++;
				}
			}
		}

		public static void GetGroupDictionaryAssociation(Joint[] joints, ref Dictionary<string, int> groupsDictionary)
		{
			int jointGroupCount = 1;

			for (int i = 0; i < joints.Length; i++)
			{
				Joint joint = joints[i];
				List<string> groups = joint.Groups.Select(g => g.Name).ToList();
				GetGroupDictionaryAssociation(groups, ref groupsDictionary, ref jointGroupCount);
			}
		}

		public static void GetGroupDictionaryAssociation(Beam[] beams, ref Dictionary<string, int> groupsDictionary)
		{
			int jointGroupCount = 1;

			for (int i = 0; i < beams.Length; i++)
			{
				Beam joint = beams[i];
				if (joint != null)
				{
					List<string> groups = joint.Groups.Select(g => g.Name).ToList();
					GetGroupDictionaryAssociation(groups, ref groupsDictionary, ref jointGroupCount);
				}
			}
		}

		public static void GetGroupDictionaryAssociation(Plate[] area, ref Dictionary<string, int> groupsDictionary)
		{
			int jointGroupCount = 1;

			for (int i = 0; i < area.Length; i++)
			{
				Plate joint = area[i];
				if (joint != null)
				{
					List<string> groups = joint.Groups.Select(g => g.Name).ToList();
					GetGroupDictionaryAssociation(groups, ref groupsDictionary, ref jointGroupCount);
				}
			}
		}

		public static List<Layer> GetLayersProperty(Beam[] beams, LayerTable layerTable, Layer layer)
		{
			List<string> properties = beams.Select(f => ((Frame)f).Property.Name).Distinct().ToList();

			int frameLayerId = -1;
			if (layerTable.FindByFullPath(layer.Name, -1) != -1)
				frameLayerId = layerTable.FindByFullPath(layer.Name, -1);
			else
				frameLayerId = layerTable.Add(layer);

			return GetLayersProperty(properties, layerTable, frameLayerId);
		}

		public static Dictionary<string, Guid> AddLayerProperties(Beam[] beams, LayerTable layerTable, Guid parentId)
		{
			List<string> properties = beams.Select(f => ((Frame)f).Property.Name).Distinct().ToList();
			return AddLayerProperties(properties, layerTable, parentId);
		}

		public static Dictionary<string, Guid> AddLayerProperties(Plate[] plates, LayerTable layerTable, Guid parentId)
		{
			List<string> properties = plates.Where(i => i != null).Select(f => f.Property.Name).Distinct().ToList();
			return AddLayerProperties(properties, layerTable, parentId);
		}

		public static Dictionary<string, Guid> AddLayerProperties(List<string> properties, LayerTable layerTable, Guid parentId)
		{
			Dictionary<string, Guid> nameGuidAss = new Dictionary<string, Guid>();

			for (int i = 0; i < properties.Count; i++)
			{
				Layer layer = new Layer
				{
					Name = properties[i],
					ParentLayerId = parentId,
					Color = F2RModelHelper.GetRandomColor(),
					Id = Guid.NewGuid()
				};

				layerTable.Add(layer);
				nameGuidAss.Add(layer.Name, layer.Id);
			}

			return nameGuidAss;
		}

		public static List<Layer> GetLayersProperty(Plate[] areas, LayerTable layerTable, Layer layer)
		{
			List<string> properties = areas.Select(f => f.Property.Name).Distinct().ToList();

			int areaLayerId = -1;
			if (layerTable.FindByFullPath(layer.Name, -1) != -1)
				areaLayerId = layerTable.FindByFullPath(layer.Name, -1);
			else
				areaLayerId = layerTable.Add(layer);

			return GetLayersProperty(properties, layerTable, areaLayerId);
		}

		public static List<Layer> GetLayersProperty(List<string> properties, LayerTable layerTable, int layerId)
		{
			List<Layer> layersPropertiesFrame = new List<Layer>();

			for (int i = 0; i < properties.Count; i++)
			{
				Layer subLayer = new Layer
				{
					Name = properties[i],
					Color = F2RModelHelper.GetRandomColor(),
					Id = Guid.NewGuid(),
					ParentLayerId = layerTable.FindIndex(layerId).Id
				};
				if (!layersPropertiesFrame.Contains(subLayer))
					layersPropertiesFrame.Add(subLayer);
			}

			return layersPropertiesFrame;
		}

		public static void SetLayerToObject(Layer sublayer, LayerTable layerTable, ref ObjectAttributes objAtt)
		{
			if (sublayer != null)
			{
				Layer sublayerMatch;
				if (layerTable.FindName(sublayer.Name) != null)
					sublayerMatch = layerTable.FindName(sublayer.Name, -1);
				else
					sublayerMatch = layerTable.FindIndex(layerTable.Add(sublayer));

				objAtt.LayerIndex = sublayerMatch.Index;
			}
		}

		public static Guid AddLayerToTable(string name, Guid parentId, LayerTable layerTable)
		{
			Layer layer = new Layer
			{
				Name = name,
				ParentLayerId = parentId,
				Color = F2RModelHelper.GetRandomColor(),
				Id = Guid.NewGuid()
			};

			layerTable.Add(layer);

			return layer.Id;
		}

		public static Guid AddLayerToTable(string name, LayerTable layerTable)
		{
			Layer layer = new Layer
			{
				Name = name,
				Color = F2RModelHelper.GetRandomColor(),
				Id = Guid.NewGuid()
			};

			layerTable.Add(layer);

			return layer.Id;
		}

		public static Dictionary<Point3d, PointObject> SetPointDictionary(RhinoObject[] rhinoObjPoints, int jointLayerIndex, out List<string> f2rJointId, out List<string> log)
		{
			log = new List<string>();
			f2rJointId = new List<string>();
			Dictionary<Point3d, PointObject> pointDictionary = new Dictionary<Point3d, PointObject>();

			for (int i = 0; i < rhinoObjPoints.Length; i++)
			{
				if (rhinoObjPoints[i].GetType() == typeof(PointObject))
				{
					PointObject currentPointObj = (PointObject)rhinoObjPoints[i];
					if (!pointDictionary.ContainsKey(((Rhino.Geometry.Point)currentPointObj.Geometry).Location))
					{
						pointDictionary.Add(((Rhino.Geometry.Point)currentPointObj.Geometry).Location, currentPointObj);
						if (rhinoObjPoints[i].Attributes.LayerIndex == jointLayerIndex)
						{
							if (currentPointObj.Attributes.GetUserString("F2R_ID") != null)
								f2rJointId.Add(currentPointObj.Attributes.GetUserString("F2R_ID"));
						}
					}
				}
				else
				{
					log.Add($"Input point {i} is not a point");
				}
			}
			return pointDictionary;	
		}

		public static int GetNewElementId(List<string> f2rId, string jointPrefix)
		{
			int jointCount = 1;

			List<string> f2rJointIdBuffer = new List<string>();

			for (int i = 0; i < f2rId.Count; i++)
			{
				if (f2rId[i] != null && f2rId[i].StartsWith(jointPrefix))
				{
					f2rJointIdBuffer.Add(f2rId[i].Remove(0, jointPrefix.Length));
				}
			}

			if (f2rJointIdBuffer.Count > 0)
			{
				List<int> listBuffer = new List<int>();
				for (int i = 0; i < f2rJointIdBuffer.Count; i++)
				{
					try
					{
						int b = Convert.ToInt16(f2rJointIdBuffer[i]);
						listBuffer.Add(b);
					}
					catch (Exception) { }
				}

				jointCount = listBuffer.Max();
				jointCount++;
			}

			return jointCount;
		}

		public static List<string> GetObjectIds(RhinoObject[] rhinoObj, int[] layersIndices)
		{
			List<string> f2rAreaId = new List<string>();

			for (int i = 0; i < rhinoObj.Length; i++)
			{
				if (layersIndices.Contains(rhinoObj[i].Attributes.LayerIndex))
				{
					if (rhinoObj[i].Attributes.GetUserString("F2R_ID") != null)
						f2rAreaId.Add(rhinoObj[i].Attributes.GetUserString("F2R_ID"));
				}
			}
			return f2rAreaId;
		}

		public static string AddPoint(Dictionary<Point3d, PointObject> pointDictionary, Point3d point, RhinoDoc doc, string jointPrefix, int jointLayerIndex,  ref int jointCount)
		{
			ObjectAttributes objAtt = new ObjectAttributes { LayerIndex = jointLayerIndex };
			string id = jointPrefix + jointCount;
			objAtt.SetUserString("F2R_ID", id);
			var gg = doc.Objects.AddPoint(point, objAtt);
			pointDictionary.Add(point, (PointObject)doc.Objects.FindId(gg));
			jointCount++;
			return id;
		}

		public static bool CheckExistingPoint(Point3d point, Point3d pointToMatch, Dictionary<Point3d, PointObject> pointDictionary, double tolerance, ref string nameJoint)
		{
			if (point.DistanceTo(pointToMatch) < tolerance)
			{
				string nameJointBuffer = pointDictionary[point].Attributes.GetUserString("F2R_ID");
				if (nameJointBuffer != null)
				{
					nameJoint = nameJointBuffer;
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool CheckExistingPoint(Point3d point, Point3d pointToMatch, Dictionary<Point3d, PointObject> pointDictionary, RhinoDoc doc, double tolerance, string jointPrefix, int jointLayerIndex, ref int jointCount)
		{
			if (point.DistanceTo(pointToMatch) < tolerance)
			{
				if (!pointDictionary[point].Attributes.UserDictionary.ContainsKey("F2R_ID"))
				{
					var newAttr = pointDictionary[point].Attributes;
					newAttr.LayerIndex = jointLayerIndex;
					newAttr.SetUserString("F2R_ID", jointPrefix + jointCount);
					doc.Objects.ModifyAttributes(pointDictionary[point], newAttr, true);

					jointCount++;

					return true;
				}
				return false;
			}
			return false;
		}

		public static bool CheckExistingPoint(PointObject pointObj, Point3d pointToMatch, RhinoDoc doc, double tolerance, string jointPrefix, int jointLayerIndex, ref int jointCount)
		{
			if (((Rhino.Geometry.Point)pointObj.Geometry).Location.DistanceTo(pointToMatch) < tolerance)
			{
				if (!pointObj.Attributes.UserDictionary.ContainsKey("F2R_ID"))
				{
					var newAttr = pointObj.Attributes;
					newAttr.LayerIndex = jointLayerIndex;
					newAttr.SetUserString("F2R_ID", jointPrefix + jointCount);
					doc.Objects.ModifyAttributes(pointObj, newAttr, true);

					jointCount++;

					return true;
				}
				return false;
			}
			return false;
		}
	}
}
