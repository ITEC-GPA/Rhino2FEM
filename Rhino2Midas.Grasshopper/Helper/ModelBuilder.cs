//using System;
//using System.Collections.Generic;
//using Grasshopper.Kernel.Data;
//using Grasshopper.Kernel.Types;
//using Rhino2Midas.Core.Attributes;
//using Rhino2Midas.Core.Cases;
//using Rhino2Midas.Core.ElementProperties;
//using Rhino2Midas.Core.Elements;
//using Rhino2Midas.Core.Loads;
//using Rhino2Midas.Core.Settings;

//namespace Rhino2Midas.Grasshopper.Helper
//{
//    public class ModelBuilder
//    {
//        public static List<NodeElementModel> ReadNode(GH_Structure<IGH_Goo> nodesInIghGoo, List<NodeElementModel> nodeList, List<LoadCaseModel> loadCaseList)
//        {
//            List<NodeElementModel> list = new List<NodeElementModel>();
//            for (int i = 0; i < nodesInIghGoo.DataCount; i++)
//            {
//                NodeElementModel node = new NodeElementModel();
//                IGH_Goo val = nodesInIghGoo[nodesInIghGoo[0], i];
//                ((GH_Goo<object>)node).CastFrom(val);
//                var (flag, num) = IsDuplicateNode(nodeList, node);
//                if (!flag)
//                {
//                    nodeList.Add(node);
//                }
//                else
//                {
//                    list.Add(node);
//                }
//            }
//            foreach (NodeElementModel node2 in nodeList)
//            {
//                foreach (NodalLoadModel nodalLoad in node2.NodalLoadList)
//                {
//                    var (flag2, num2) = IsDuplicateLoadCase(loadCaseList, nodalLoad.LoadCase);
//                    if (!flag2)
//                    {
//                        loadCaseList.Add(nodalLoad.LoadCase);
//                        nodalLoad.IndexLoadCase = loadCaseList.Count - 1 + 1;
//                    }
//                    else
//                    {
//                        nodalLoad.IndexLoadCase = num2 + 1;
//                    }
//                }
//            }
//            return list;
//        }

//        public static (List<FramePropertyModel>, List<FrameElementModel>, List<FrameElementModel>) ReadFrameElement(GH_Structure<IGH_Goo> frameElementsInIghGoo, List<NodeElementModel> nodeList, List<MaterialModel> materialList, List<LoadCaseModel> loadCaseList)
//        {
//            List<FramePropertyModel> list = new List<FramePropertyModel>();
//            List<FrameElementModel> list2 = new List<FrameElementModel>();
//            List<FrameElementModel> list3 = new List<FrameElementModel>();
//            for (int i = 0; i < frameElementsInIghGoo.DataCount; i++)
//            {
//                FrameElementModel frameElement = new FrameElementModel();
//                IGH_Goo val = frameElementsInIghGoo[frameElementsInIghGoo[0], i];
//                ((GH_Goo<object>)frameElement).CastFrom((object)val);
//                if (!IsDuplicateFrameElement(list2, frameElement))
//                {
//                    list2.Add(frameElement);
//                }
//                else
//                {
//                    list3.Add(frameElement);
//                }
//            }
//            foreach (FrameElementModel item in list2)
//            {
//                var (flag, num) = IsDuplicateMaterial(materialList, item.Material);
//                if (!flag)
//                {
//                    materialList.Add(item.Material);
//                    item.IndexMaterial = materialList.Count - 1 + 1;
//                }
//                else
//                {
//                    item.IndexMaterial = num + 1;
//                }
//                var (flag2, num2) = IsDuplicateFrameProperty(list, item.FrameProperty);
//                if (!flag2)
//                {
//                    list.Add(item.FrameProperty);
//                    item.IndexFrameProperty = list.Count - 1 + 1;
//                }
//                else
//                {
//                    item.IndexFrameProperty = num2 + 1;
//                }
//                var (flag3, num3) = IsDuplicateNode(nodeList, item.NodeStart);
//                if (!flag3)
//                {
//                    nodeList.Add(item.NodeStart);
//                    item.IndexNodeStart = nodeList.Count - 1 + 1;
//                }
//                else
//                {
//                    item.IndexNodeStart = num3 + 1;
//                }
//                var (flag4, num4) = IsDuplicateNode(nodeList, item.NodeEnd);
//                if (!flag4)
//                {
//                    nodeList.Add(item.NodeEnd);
//                    item.IndexNodeEnd = nodeList.Count - 1 + 1;
//                }
//                else
//                {
//                    item.IndexNodeEnd = num4 + 1;
//                }
//                foreach (FrameLoadModel frameLoad in item.FrameLoadList)
//                {
//                    var (flag5, num5) = IsDuplicateLoadCase(loadCaseList, frameLoad.LoadCase);
//                    if (!flag5)
//                    {
//                        loadCaseList.Add(frameLoad.LoadCase);
//                        frameLoad.IndexLoadCase = loadCaseList.Count - 1 + 1;
//                    }
//                    else
//                    {
//                        frameLoad.IndexLoadCase = num5 + 1;
//                    }
//                }
//            }
//            return (list, list2, list3);
//        }

//        public static (List<AreaThicknessModel>, List<AreaElementModel>, List<AreaElementModel>) ReadAreaElement(GH_Structure<IGH_Goo> areaElementsInIghGoo, List<NodeElementModel> nodeList, List<MaterialModel> materialList, List<LoadCaseModel> loadCaseList)
//        {
//            List<AreaThicknessModel> list = new List<AreaThicknessModel>();
//            List<AreaElementModel> list2 = new List<AreaElementModel>();
//            List<AreaElementModel> list3 = new List<AreaElementModel>();
//            for (int i = 0; i < areaElementsInIghGoo.DataCount; i++)
//            {
//                AreaElementModel areaElement = new AreaElementModel();
//                IGH_Goo val = areaElementsInIghGoo[areaElementsInIghGoo[0], i];
//                ((GH_Goo<object>)areaElement).CastFrom((object)val);
//                if (!IsDuplicateAreaElement(list2, areaElement))
//                {
//                    list2.Add(areaElement);
//                }
//                else
//                {
//                    list3.Add(areaElement);
//                }
//            }
//            foreach (AreaElementModel item in list2)
//            {
//                var (flag, num) = IsDuplicateMaterial(materialList, item.Material);
//                if (!flag)
//                {
//                    materialList.Add(item.Material);
//                    item.IndexMaterial = materialList.Count - 1 + 1;
//                }
//                else
//                {
//                    item.IndexMaterial = num + 1;
//                }
//                var (flag2, num2) = IsDuplicateAreaThickness(list, item.AreaThickness);
//                if (!flag2)
//                {
//                    list.Add(item.AreaThickness);
//                    item.IndexAreaThickness = list.Count - 1 + 1;
//                }
//                else
//                {
//                    item.IndexAreaThickness = num2 + 1;
//                }
//                item.IndexNodeList = new List<int>();
//                foreach (NodeElementModel node in item.NodeList)
//                {
//                    var (flag3, num3) = IsDuplicateNode(nodeList, node);
//                    if (!flag3)
//                    {
//                        nodeList.Add(node);
//                        item.IndexNodeList.Add(nodeList.Count - 1 + 1);
//                    }
//                    else
//                    {
//                        item.IndexNodeList.Add(num3 + 1);
//                    }
//                }
//                foreach (AreaLoadModel areaLoad in item.AreaLoadList)
//                {
//                    var (flag4, num4) = IsDuplicateLoadCase(loadCaseList, areaLoad.LoadCase);
//                    if (!flag4)
//                    {
//                        loadCaseList.Add(areaLoad.LoadCase);
//                        areaLoad.IndexLoadCase = loadCaseList.Count - 1 + 1;
//                    }
//                    else
//                    {
//                        areaLoad.IndexLoadCase = num4 + 1;
//                    }
//                }
//            }
//            return (list, list2, list3);
//        }

//        public static (List<LoadCombinationModel>, List<LoadCombinationModel>) ReadLoadCombination(GH_Structure<IGH_Goo> loadCombinationsInIghGoo, List<LoadCaseModel> loadCaseList)
//        {
//            List<LoadCombinationModel> list = new List<LoadCombinationModel>();
//            List<LoadCombinationModel> list2 = new List<LoadCombinationModel>();
//            for (int i = 0; i < loadCombinationsInIghGoo.DataCount; i++)
//            {
//                LoadCombinationModel loadCombination = new LoadCombinationModel();
//                IGH_Goo val = loadCombinationsInIghGoo[loadCombinationsInIghGoo[0], i];
//                ((GH_Goo<object>)loadCombination).CastFrom((object)val);
//                if (!IsDuplicateLoadCombination(list, loadCombination))
//                {
//                    foreach (LoadFactorModel loadFactor in loadCombination.LoadFactorList)
//                    {
//                        LoadCaseModel loadCase = loadFactor.LoadCase;
//                        var (flag, num) = IsDuplicateLoadCase(loadCaseList, loadCase);
//                        if (!flag)
//                        {
//                            loadCaseList.Add(loadCase);
//                        }
//                    }
//                    list.Add(loadCombination);
//                }
//                else
//                {
//                    list2.Add(loadCombination);
//                }
//            }
//            return (list, list2);
//        }

//        public static (List<SelfWeightModel>, List<SelfWeightModel>) ReadSelfWeight(GH_Structure<IGH_Goo> selfWeightsInIghGoo, List<LoadCaseModel> loadCaseList)
//        {
//            List<SelfWeightModel> list = new List<SelfWeightModel>();
//            List<SelfWeightModel> list2 = new List<SelfWeightModel>();
//            for (int i = 0; i < selfWeightsInIghGoo.DataCount; i++)
//            {
//                SelfWeightModel selfWeight = new SelfWeightModel();
//                IGH_Goo val = selfWeightsInIghGoo[selfWeightsInIghGoo[0], i];
//                ((GH_Goo<object>)selfWeight).CastFrom((object)val);
//                if (!IsDuplicateSelfWeight(list, selfWeight))
//                {
//                    list.Add(selfWeight);
//                }
//                else
//                {
//                    list2.Add(selfWeight);
//                }
//            }
//            foreach (SelfWeightModel item in list)
//            {
//                var (flag, num) = IsDuplicateLoadCase(loadCaseList, item.LoadCase);
//                if (!flag)
//                {
//                    loadCaseList.Add(item.LoadCase);
//                }
//            }
//            return (list, list2);
//        }

//        private static bool IsIdenticalMaterial(MaterialModel material1, MaterialModel material2)
//        {
//            List<string> list = new List<string> { "Name", "Type", "DampingRatio", "ModulusElasticity", "PoissonRatio", "ThermalCoefficient", "Density", "Mass" };
//            foreach (string item in list)
//            {
//                object value = material1.GetType().GetProperty(item).GetValue(material1, null);
//                object value2 = material2.GetType().GetProperty(item).GetValue(material2, null);
//                if (Convert.ToString(value) != Convert.ToString(value2))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static (bool, int) IsDuplicateMaterial(List<MaterialModel> materialList, MaterialModel material)
//        {
//            int num = 0;
//            foreach (MaterialModel material2 in materialList)
//            {
//                if (IsIdenticalMaterial(material2, material))
//                {
//                    return (true, num);
//                }
//                num++;
//            }
//            return (false, -1);
//        }

//        private static bool IsIdenticalFrameProperty(FramePropertyModel frameProperty1, FramePropertyModel frameProperty2)
//        {
//            List<string> list = new List<string> { "Name", "Type" };
//            foreach (string item in list)
//            {
//                object value = frameProperty1.GetType().GetProperty(item).GetValue(frameProperty1, null);
//                object value2 = frameProperty2.GetType().GetProperty(item).GetValue(frameProperty2, null);
//                if (Convert.ToString(value) != Convert.ToString(value2))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static (bool, int) IsDuplicateFrameProperty(List<FramePropertyModel> framePropertyList, FramePropertyModel frameProperty)
//        {
//            int num = 0;
//            foreach (FramePropertyModel frameProperty2 in framePropertyList)
//            {
//                if (IsIdenticalFrameProperty(frameProperty2, frameProperty))
//                {
//                    return (true, num);
//                }
//                num++;
//            }
//            return (false, num);
//        }

//        private static bool IsIdenticalNode(NodeElementModel node1, NodeElementModel node2)
//        {
//            List<string> list = new List<string> { "X", "Y", "Z" };
//            foreach (string item in list)
//            {
//                object value = node1.GetType().GetProperty(item).GetValue(node1, null);
//                object value2 = node2.GetType().GetProperty(item).GetValue(node2, null);
//                if (Convert.ToString(value) != Convert.ToString(value2))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static (bool, int) IsDuplicateNode(List<NodeElementModel> nodeList, NodeElementModel node)
//        {
//            int num = 0;
//            foreach (NodeElementModel node2 in nodeList)
//            {
//                if (IsIdenticalNode(node2, node))
//                {
//                    return (true, num);
//                }
//                num++;
//            }
//            return (false, num);
//        }

//        private static bool IsDuplicateFrameElement(List<FrameElementModel> frameElementList, FrameElementModel frameElement)
//        {
//            foreach (FrameElementModel frameElement2 in frameElementList)
//            {
//                if (IsIdenticalNode(frameElement2.NodeStart, frameElement.NodeStart) && IsIdenticalNode(frameElement2.NodeEnd, frameElement.NodeEnd))
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private static bool IsIdenticalLoadCase(LoadCaseModel loadCase1, LoadCaseModel loadCase2)
//        {
//            List<string> list = new List<string> { "Name" };
//            foreach (string item in list)
//            {
//                object value = loadCase1.GetType().GetProperty(item).GetValue(loadCase1, null);
//                object value2 = loadCase2.GetType().GetProperty(item).GetValue(loadCase2, null);
//                if (Convert.ToString(value) != Convert.ToString(value2))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static (bool, int) IsDuplicateLoadCase(List<LoadCaseModel> loadCaseList, LoadCaseModel loadCase)
//        {
//            int num = 0;
//            foreach (LoadCaseModel loadCase2 in loadCaseList)
//            {
//                if (IsIdenticalLoadCase(loadCase2, loadCase))
//                {
//                    return (true, num);
//                }
//            }
//            num++;
//            return (false, -1);
//        }

//        private static bool IsIdenticalAreaElement(AreaElementModel area1, AreaElementModel area2)
//        {
//            if (area1.NodeList.Count != area2.NodeList.Count)
//            {
//                return false;
//            }
//            for (int i = 0; i < area1.NodeList.Count; i++)
//            {
//                NodeElementModel node = area1.NodeList[i];
//                for (int j = 0; j < area2.NodeList.Count; j++)
//                {
//                    NodeElementModel node2 = area2.NodeList[j];
//                    if (IsIdenticalNode(node, node2))
//                    {
//                        break;
//                    }
//                    if (j == area2.NodeList.Count - 1)
//                    {
//                        return false;
//                    }
//                }
//            }
//            return true;
//        }

//        private static bool IsDuplicateAreaElement(List<AreaElementModel> areaElementList, AreaElementModel areaElement)
//        {
//            int num = 0;
//            foreach (AreaElementModel areaElement2 in areaElementList)
//            {
//                if (IsIdenticalAreaElement(areaElement2, areaElement))
//                {
//                    return true;
//                }
//            }
//            num++;
//            return false;
//        }

//        private static bool IsIdenticalAreaThickness(AreaThicknessModel areaThickness1, AreaThicknessModel areaThickness2)
//        {
//            List<string> list = new List<string> { "Name", "Thickness" };
//            foreach (string item in list)
//            {
//                object value = areaThickness1.GetType().GetProperty(item).GetValue(areaThickness1, null);
//                object value2 = areaThickness2.GetType().GetProperty(item).GetValue(areaThickness2, null);
//                if (Convert.ToString(value) != Convert.ToString(value2))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static (bool, int) IsDuplicateAreaThickness(List<AreaThicknessModel> areaThicknessList, AreaThicknessModel areaThickness)
//        {
//            int num = 0;
//            foreach (AreaThicknessModel areaThickness2 in areaThicknessList)
//            {
//                if (IsIdenticalAreaThickness(areaThickness2, areaThickness))
//                {
//                    return (true, num);
//                }
//                num++;
//            }
//            return (false, -1);
//        }

//        private static bool IsIdenticalLoadCombination(LoadCombinationModel loadCombination1, LoadCombinationModel loadCombination2)
//        {
//            List<string> list = new List<string> { "Name" };
//            foreach (string item in list)
//            {
//                object value = loadCombination1.GetType().GetProperty(item).GetValue(loadCombination1, null);
//                object value2 = loadCombination2.GetType().GetProperty(item).GetValue(loadCombination2, null);
//                if (Convert.ToString(value) != Convert.ToString(value2))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private static bool IsDuplicateLoadCombination(List<LoadCombinationModel> loadCombinationList, LoadCombinationModel loadCombination)
//        {
//            foreach (LoadCombinationModel loadCombination2 in loadCombinationList)
//            {
//                if (IsIdenticalLoadCombination(loadCombination2, loadCombination))
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private static bool IsIdenticalSelfWeight(SelfWeightModel selfWeight1, SelfWeightModel selfWeight2)
//        {
//            if (IsIdenticalLoadCase(selfWeight1.LoadCase, selfWeight2.LoadCase))
//            {
//                return true;
//            }
//            return false;
//        }

//        private static bool IsDuplicateSelfWeight(List<SelfWeightModel> selfWeightList, SelfWeightModel selfWeight)
//        {
//            foreach (SelfWeightModel selfWeight2 in selfWeightList)
//            {
//                if (IsIdenticalSelfWeight(selfWeight2, selfWeight))
//                {
//                    return true;
//                }
//            }
//            return false;
//        }
//    }
//}
