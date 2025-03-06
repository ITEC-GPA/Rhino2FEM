using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Core.Collections;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Core.Loads;
using Rhino2Midas.Core.Settings;

namespace Rhino2Midas.Core.Models
{
    public class ModelModel : IEquatable<ModelModel>
    {
        public ModelUnits ModelUnits { get; set; }
        public UniqueNameCollection<MaterialModel> Materials { get; }
        public UniqueNameCollection<LoadCaseModel> LoadCases { get; }
        public UniqueNameCollection<LoadCombinationModel> LoadCombinations { get; }
        public UniqueNameCollection<ElementGroupModel> Groups { get; }
        public UniqueNameCollection<FramePropertyModel> FrameProperties { get; }
        public UniqueNameCollection<AreaThicknessModel> AreaThicknesses { get; }
        public UniqueIdCollection<NodeElementModel> NodeElements { get; }
        public UniqueIdCollection<FrameElementModel> FrameElements { get; }
        public UniqueIdCollection<AreaElementModel> AreaElements { get; }
        public SelfWeightModel SelfWeight { get; set; }

        public ModelModel()
        {
            Materials = new UniqueNameCollection<MaterialModel>();
            LoadCases = new UniqueNameCollection<LoadCaseModel>();
            LoadCombinations = new UniqueNameCollection<LoadCombinationModel>();
            Groups = new UniqueNameCollection<ElementGroupModel>();
            FrameProperties = new UniqueNameCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueNameCollection<AreaThicknessModel>();
            NodeElements = new UniqueIdCollection<NodeElementModel>();
            FrameElements = new UniqueIdCollection<FrameElementModel>();
            AreaElements = new UniqueIdCollection<AreaElementModel>();
        }

        public ModelModel(ModelModel model)
        {
            Materials = new UniqueNameCollection<MaterialModel>();
            LoadCases = new UniqueNameCollection<LoadCaseModel>();
            LoadCombinations = new UniqueNameCollection<LoadCombinationModel>();
            Groups = new UniqueNameCollection<ElementGroupModel>();
            FrameProperties = new UniqueNameCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueNameCollection<AreaThicknessModel>();
            NodeElements = new UniqueIdCollection<NodeElementModel>();
            FrameElements = new UniqueIdCollection<FrameElementModel>();
            AreaElements = new UniqueIdCollection<AreaElementModel>();

            for (int i = 0; i < model.Materials.Count; i++)
                Materials.Add(new MaterialModel(model.Materials.Values.ElementAt(i)));

            for (int i = 0; i < model.LoadCases.Count; i++)
                LoadCases.Add(new LoadCaseModel(model.LoadCases.Values.ElementAt(i)));

            for (int i = 0; i < model.LoadCombinations.Count; i++)
                LoadCombinations.Add(new LoadCombinationModel(model.LoadCombinations.Values.ElementAt(i)));

            for (int i = 0; i < model.Groups.Count; i++)
                Groups.Add(new ElementGroupModel(model.Groups.Values.ElementAt(i)));

            for (int i = 0; i < model.FrameProperties.Count; i++)
                FrameProperties.Add(new FramePropertyModel(model.FrameProperties.Values.ElementAt(i)));

            for (int i = 0; i < model.AreaThicknesses.Count; i++)
                AreaThicknesses.Add(new AreaThicknessModel(model.AreaThicknesses.Values.ElementAt(i)));

            for (int i = 0; i < model.NodeElements.Count; i++)
                NodeElements.Add(new NodeElementModel(model.NodeElements.Values.ElementAt(i)));

            for (int i = 0; i < model.FrameElements.Count; i++)
                FrameElements.Add(new FrameElementModel(model.FrameElements.Values.ElementAt(i)));

            for (int i = 0; i < model.AreaElements.Count; i++)
                AreaElements.Add(new AreaElementModel(model.AreaElements.Values.ElementAt(i)));
        }

        public void BuildModel(List<NodeElementModel> inputNodes = null, List<FrameElementModel> inputFrames = null, List<AreaElementModel> inputAreas = null, List<LoadCombinationModel> inputcombos = null, SelfWeightModel inputSelfWeight = null)
        {
            #region Check Element IDs

            Dictionary<int, NodeElementModel> nodeDictionary = new Dictionary<int, NodeElementModel>();
            Dictionary<int, FrameElementModel> frameDictionary = new Dictionary<int, FrameElementModel>();
            Dictionary<int, AreaElementModel> areaDictionary = new Dictionary<int, AreaElementModel>();

            if (inputNodes == null)
                inputNodes = new List<NodeElementModel>();
            if (inputFrames == null)
                inputFrames = new List<FrameElementModel>();
            if (inputAreas == null)
                inputAreas = new List<AreaElementModel>();
            if (inputcombos == null)
                inputcombos = new List<LoadCombinationModel>();
            SelfWeightModel selfWeight = null;
            if (inputSelfWeight != null)
                selfWeight = new SelfWeightModel(inputSelfWeight);

            int idNode = 1;
            List<int> usedNodeIds = inputNodes.Select(x => x.Id).ToList();
            List<int> usedElementIds = inputFrames.Select(x => x.Id).ToList();
            usedElementIds.AddRange(inputAreas.Select(x => x.Id).ToList());

            List<NodeElementModel> nodes = new List<NodeElementModel>();
            List<FrameElementModel> frames = new List<FrameElementModel>();
            List<AreaElementModel> areas = new List<AreaElementModel>();
            List<LoadCombinationModel> combos = new List<LoadCombinationModel>();

            for (int i = 0; i < inputNodes.Count; i++)
                nodes.Add(new NodeElementModel(inputNodes[i]));
            for (int i = 0; i < inputFrames.Count; i++)
                frames.Add(new FrameElementModel(inputFrames[i]));
            for (int i = 0; i < inputAreas.Count; i++)
                areas.Add(new AreaElementModel(inputAreas[i]));
            for (int i = 0; i < inputcombos.Count; i++)
                combos.Add(new LoadCombinationModel(inputcombos[i]));

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodeDictionary.ContainsKey(nodes[i].Id) && nodes[i].Id != ModelObjectId.UNASSIGNED)
                {
                    nodeDictionary[nodes[i].Id].Merge(nodes[i]);
                }
                else
                {
                    NodeElementModel nn = new NodeElementModel(nodes[i]);
                    if (nn.Id != ModelObjectId.UNASSIGNED)
                    {
                        nodeDictionary.Add(nn.Id, nn);
                    }
                    else
                    {
                        idNode = NewId(usedNodeIds, idNode);

                        nn.Id = idNode;
                        nodeDictionary.Add(nn.Id, nn);
                        usedNodeIds.Add(idNode);
                        idNode++;
                    }
                }
            }

            int idFrame = 1;
            for (int i = 0; i < frames.Count; i++)
            {
                if (frameDictionary.ContainsKey(frames[i].Id) && frames[i].Id != ModelObjectId.UNASSIGNED)
                {
                    frameDictionary[frames[i].Id].Merge(frames[i]);
                }
                else
                {
                    FrameElementModel nn = new FrameElementModel(frames[i]);
                    if (nn.Id != ModelObjectId.UNASSIGNED)
                    {
                        frameDictionary.Add(nn.Id, nn);
                    }
                    else
                    {
                        idFrame = NewId(usedElementIds, idFrame);
                        nn.Id = idFrame;
                        frameDictionary.Add(nn.Id, nn);
                        usedElementIds.Add(idFrame);
                        idFrame++;
                    }
                }
            }

            int idArea = 1;
            for (int i = 0; i < areas.Count; i++)
            {
                if (areaDictionary.ContainsKey(areas[i].Id) && areas[i].Id != ModelObjectId.UNASSIGNED)
                {
                    areaDictionary[areas[i].Id].Merge(areas[i]);
                }
                else
                {
                    AreaElementModel nn = new AreaElementModel(areas[i]);
                    if (nn.Id != ModelObjectId.UNASSIGNED)
                    {
                        areaDictionary.Add(nn.Id, nn);
                    }
                    else
                    {
                        idArea = NewId(usedElementIds, idArea);

                        nn.Id = idArea;
                        areaDictionary.Add(nn.Id, nn);
                        usedElementIds.Add(idArea);
                        idArea++;
                    }
                }
            }

            #endregion

            #region Matching add nodes

            double tol = ModelUnits.Tolerance;

            List<NodeElementModel> nodesBuffer = nodeDictionary.Values.ToList();
            List<FrameElementModel> framesBuffer = frameDictionary.Values.ToList();
            List<AreaElementModel> areasBuffer = areaDictionary.Values.ToList();

            RTree rTree = new RTree();
            for (int i = 0; i < nodesBuffer.Count; i++)
                rTree.Insert(nodesBuffer[i].Position, nodesBuffer[i].Id);

            for (int i = 0; i < framesBuffer.Count; i++)
            {
                {
                    Point3d pt = framesBuffer[i].NodeStart.Position;
                    bool found = false;
                    int nodeId = ModelObjectId.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        framesBuffer[i].NodeStart = nodeDictionary[nodeId];
                    }
                    else
                    {
                        NodeElementModel newNode = new NodeElementModel(pt);
                        idNode = NewId(usedNodeIds, idNode);
                        newNode.Id = idNode;
                        usedNodeIds.Add(idNode);
                        nodeDictionary.Add(idNode, newNode);
                        nodesBuffer.Add(newNode);
                        rTree.Insert(pt, idNode);
                        framesBuffer[i].NodeStart = newNode;
                    }
                }
                {
                    Point3d pt = framesBuffer[i].NodeEnd.Position;
                    bool found = false;
                    int nodeId = ModelObjectId.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        framesBuffer[i].NodeEnd = nodeDictionary[nodeId];
                    }
                    else
                    {
                        NodeElementModel newNode = new NodeElementModel(pt);
                        idNode = NewId(usedNodeIds, idNode);
                        newNode.Id = idNode;
                        usedNodeIds.Add(idNode);
                        nodeDictionary.Add(idNode, newNode);
                        nodesBuffer.Add(newNode);
                        rTree.Insert(pt, idNode);
                        framesBuffer[i].NodeEnd = newNode;
                    }
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var area = areasBuffer[i];
                for (int j = 0; j < area.NodeList.Count; j++)
                {
                    NodeElementModel node = area.NodeList[j];
                    Point3d pt = node.Position;
                    bool found = false;
                    int nodeId = ModelObjectId.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        area.NodeList[j] = nodeDictionary[nodeId];
                    }
                    else
                    {
                        NodeElementModel newNode = new NodeElementModel(pt);
                        idNode = NewId(usedNodeIds, idNode);
                        newNode.Id = idNode;
                        usedNodeIds.Add(idNode);
                        nodeDictionary.Add(idNode, newNode);
                        nodesBuffer.Add(newNode);
                        rTree.Insert(pt, idNode);
                        area.NodeList[j] = newNode;
                    }
                }
            }

            #endregion

            #region Groups

            int idGroup = 1;
            for (int i = 0; i < nodesBuffer.Count; i++)
            {
                NodeElementModel element = nodesBuffer[i];
                if (element != null)
                {
                    if (element.Groups.Count > 0)
                    {
                        for (int j = 0; j < element.Groups.Count; j++)
                        {
                            ElementGroupModel newGroup = element.Groups[j];
                            newGroup.Id = idGroup;
                            idGroup++;
                            Groups.Add(element.Groups[j]);
                        }
                    }
                }
            }
            for (int i = 0; i < framesBuffer.Count; i++)
            {
                var element = framesBuffer[i];
                if (element != null)
                {
                    if (element.Groups.Count > 0)
                    {
                        for (int j = 0; j < element.Groups.Count; j++)
                        {
                            ElementGroupModel newGroup = element.Groups[j];
                            newGroup.Id = idGroup;
                            idGroup++;
                            Groups.Add(element.Groups[j]);
                        }
                    }
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var element = areasBuffer[i];
                if (element != null)
                {
                    if (element.Groups.Count > 0)
                    {
                        for (int j = 0; j < element.Groups.Count; j++)
                        {
                            ElementGroupModel newGroup = element.Groups[j];
                            newGroup.Id = idGroup;
                            idGroup++;
                            Groups.Add(element.Groups[j]);
                        }
                    }
                }
            }

            #endregion

            #region Properties

            int idMaterial = 1;
            int idFrameProperty = 1;
            int idAreaProperty = 1;
            List<int> usedMaterialIds = frames.Select(x => x.Material.Id).Union(areas.Select(i => i.Material.Id)).Distinct().ToList();
            List<int> usedFramePropertyIds = frames.Select(x => x.FrameProperty.Id).Distinct().ToList();
            List<int> usedAreaPropertyIds = areas.Select(x => x.AreaThickness.Id).Distinct().ToList();

            for (int i = 0; i < framesBuffer.Count; i++)
            {
                var mat = new MaterialModel(framesBuffer[i].Material);

                if (Materials.ContainsKey(mat.Name))
                    framesBuffer[i].Material = Materials[mat.Name];
                else
                {
                    if (mat.Id == ModelObjectId.UNASSIGNED)
                        mat.Id = NewId(usedMaterialIds, idMaterial);
                    usedMaterialIds.Add(mat.Id);
                    Materials.Add(mat);
                    framesBuffer[i].Material = Materials[mat.Name];
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var mat = new MaterialModel(areasBuffer[i].Material);

                if (Materials.ContainsKey(mat.Name))
                    areasBuffer[i].Material = Materials[mat.Name];
                else
                {
                    if (mat.Id == ModelObjectId.UNASSIGNED)
                        mat.Id = NewId(usedMaterialIds, idMaterial);
                    usedMaterialIds.Add(mat.Id);
                    Materials.Add(mat);
                    areasBuffer[i].Material = Materials[mat.Name];
                }
            }

            for (int i = 0; i < framesBuffer.Count; i++)
            {
                var fp = new FramePropertyModel(framesBuffer[i].FrameProperty);

                if (FrameProperties.ContainsKey(fp.Name))
                    framesBuffer[i].FrameProperty = FrameProperties[fp.Name];
                else
                {
                    if (fp.Id == ModelObjectId.UNASSIGNED)
                        fp.Id = NewId(usedFramePropertyIds, idFrameProperty);
                    usedFramePropertyIds.Add(fp.Id);
                    FrameProperties.Add(fp);
                    framesBuffer[i].FrameProperty = FrameProperties[fp.Name];
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var fp = new AreaThicknessModel(areasBuffer[i].AreaThickness);

                if (AreaThicknesses.ContainsKey(fp.Name))
                    areasBuffer[i].AreaThickness = AreaThicknesses[fp.Name];
                else
                {
                    if (fp.Id == ModelObjectId.UNASSIGNED)
                        fp.Id = NewId(usedAreaPropertyIds, idAreaProperty);
                    usedAreaPropertyIds.Add(fp.Id);
                    AreaThicknesses.Add(fp);
                    areasBuffer[i].AreaThickness = AreaThicknesses[fp.Name];
                }
            }

            #endregion

            #region Cases

            int idCases = 1;

            for (int i = 0; i < nodesBuffer.Count; i++)
            {
                NodeElementModel element = nodesBuffer[i];
                if (element != null)
                {
                    if (element.NodalLoadList.Count > 0)
                    {
                        for (int j = 0; j < element.NodalLoadList.Count; j++)
                        {
                            if (!LoadCases.ContainsKey(element.NodalLoadList[j].LoadCase.Name))
                            {
                                var newCase = new LoadCaseModel(element.NodalLoadList[j].LoadCase);
                                newCase.Id = idCases;
                                idCases++;
                                LoadCases.Add(newCase);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < framesBuffer.Count; i++)
            {
                var element = framesBuffer[i];
                if (element != null)
                {
                    if (element.FrameLoadList.Count > 0)
                    {
                        for (int j = 0; j < element.FrameLoadList.Count; j++)
                        {
                            if (!LoadCases.ContainsKey(element.FrameLoadList[j].LoadCase.Name))
                            {
                                var newCase = new LoadCaseModel(element.FrameLoadList[j].LoadCase);
                                newCase.Id = idCases;
                                idCases++;
                                LoadCases.Add(newCase);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var element = areasBuffer[i];
                if (element != null)
                {
                    if (element.AreaLoadList.Count > 0)
                    {
                        for (int j = 0; j < element.AreaLoadList.Count; j++)
                        {
                            if (!LoadCases.ContainsKey(element.AreaLoadList[j].LoadCase.Name))
                            {
                                var newCase = new LoadCaseModel(element.AreaLoadList[j].LoadCase);
                                newCase.Id = idCases;
                                idCases++;
                                LoadCases.Add(newCase);
                            }
                        }
                    }
                }
            }

            int idCombo = 1;
            for (int i = 0; i < combos.Count; i++)
            {
                var newCombo = new LoadCombinationModel(combos[i]);
                for (int j = 0; j < combos[i].LoadFactorList.Count; j++)
                {
                    if (!LoadCases.ContainsKey(combos[i].LoadFactorList[j].LoadCase.Name))
                    {
                        var newCase = new LoadCaseModel(combos[i].LoadFactorList[j].LoadCase);
                        newCase.Id = idCases;
                        idCases++;
                        LoadCases.Add(newCase);
                        newCombo.LoadFactorList[j].LoadCase = newCase;
                    }
                    else
                        newCombo.LoadFactorList[j].LoadCase = LoadCases[combos[i].LoadFactorList[j].LoadCase.Name];
                }
                newCombo.Id = idCombo;
                LoadCombinations.Add(newCombo);
            }

            if (selfWeight != null)
            {
                if (!LoadCases.ContainsKey(selfWeight.LoadCase.Name))
                    LoadCases.Add(selfWeight.LoadCase);
                SelfWeight = selfWeight;
            }

            #endregion            

            NodeElements.AddRange(nodesBuffer);
            FrameElements.AddRange(framesBuffer);
            AreaElements.AddRange(areasBuffer);
        }

        public List<string> CreateMgtFile()
        {
            List<string> outputStrings = new List<string>();
            WriteMgtHeader(outputStrings);
            WriteMgtUnit(outputStrings);
            WriteMgtLoadGroup(outputStrings);
            WriteMgtBoundaryGroup(outputStrings);
            WriteMgtMaterial(outputStrings);
            WriteMgtFrameProperty(outputStrings);
            WriteMgtAreaThickness(outputStrings);
            WriteMgtLoadCase(outputStrings);
            WriteMgtSelfWeight(outputStrings);
            WriteMgtLoadCombination(outputStrings);
            WriteMgtNode(outputStrings);
            WriteMgtSupport(outputStrings);
            WriteMgtFrameElement(outputStrings);
            WriteMgtAreaElement(outputStrings);
            WriteMgtGroup(outputStrings);
            WriteMgtNodalLoad(outputStrings);
            WriteMgtFrameLoad(outputStrings);
            WriteMgtAreaLoad(outputStrings);
            return outputStrings;
        }

        private int NewId(List<int> usedNodeIds, int previousId)
        {
            bool exist = true;
            do
            {
                if (usedNodeIds.Contains(previousId))
                    previousId++;
                else
                    exist = false;
            } while (exist);
            return previousId;
        }

        private void WriteMgtHeader(List<string> textMgt)
        {
            textMgt.Add("; This MGT file is generated using Rhino2Midas plug in for grasshopper");
            textMgt.Add("; Created by Gabriele Pacini, contact: gabrielepacini6293@gmail.com");
            textMgt.Add("; Disclamer: please check if MIDAS output is as intended!");
        }

        private void WriteMgtUnit(List<string> textMgt)
        {
            textMgt.Add("*UNIT");
            textMgt.Add(ModelUnits.ForceUnit + ", " + ModelUnits.LengthUnit + ", " + ModelUnits.HeatUnit + ", " + ModelUnits.TemperatureUnit);
        }

        private void WriteMgtMaterial(List<string> textMgt)
        {
            if (Materials.Count > 0)
            {
                textMgt.Add("*MATERIAL");
                textMgt.Add("; i, CONC/STEEL, (name), (spheat=0), (heatco=0), (plast=''), (tunit=C), (bmass=no), (damp ratio), (2), (modulus elasticity), (poisson), (thermal coeff), (density), (mass)");
            }
            foreach (KeyValuePair<string, MaterialModel> kvp in Materials)
            {
                MaterialModel material = kvp.Value;

                string materialType = "";
                if (material.Type == MaterialModel.MaterialTypes.Concrete)
                    materialType = "CONC";
                else if (material.Type == MaterialModel.MaterialTypes.Steel)
                    materialType = "STEEL";

                string matName = material.Name;
                if (matName.Length > 16)
                    matName = matName.Substring(0, 28);

                textMgt.Add($"{material.Id}, {materialType}, {material.Name}, 0, 0, , C, NO, {material.DampingRatio}, 2, {material.ModulusElasticity}," +
                    $" {material.PoissonRatio}, {material.ThermalCoefficient}, {material.Density}, {material.Mass}");
            }
        }

        private void WriteMgtNode(List<string> textMgt)
        {
            if (NodeElements.Count > 0)
            {
                textMgt.Add("*NODE");
                textMgt.Add("; i, (X), (Y), (Z)");
            }
            foreach (var kvp in NodeElements)
            {
                NodeElementModel node = kvp.Value;
                textMgt.Add($"{node.Id}, {node.X}, {node.Y}, {node.Z}");
            }
        }

        private void WriteMgtFrameProperty(List<string> textMgt)
        {
            if (FrameProperties.Count > 0)
            {
                textMgt.Add("*SECTION");
                textMgt.Add("; i, (section type), (name), (offset), (iCENT=0), (iREF=0), (iHORZ=0), (huser=0), (iVERT=0), (vuser=0), (consider shear deformation = \"YES\"), (consider warping effect = \"NO\"), (shape type), 2, (dimension 1 to 10)");
            }
            foreach (var kvp in FrameProperties)
            {
                FramePropertyModel frameProperty = kvp.Value;
                string framePropertyName = frameProperty.Name;
                if (framePropertyName.Length > 28)
                    framePropertyName = framePropertyName.Substring(0, 28);

                textMgt.Add($"{frameProperty.Id}, DBUSER, {framePropertyName}, {frameProperty.Offset.ToString()}, 0, 0, 0, 0, 0, 0, YES, NO, {frameProperty.Type.ToString()}, 2, " +
                    $"{frameProperty.Dimension1}, {frameProperty.Dimension2}, {frameProperty.Dimension3}, {frameProperty.Dimension4}, {frameProperty.Dimension5}, " +
                    $"{frameProperty.Dimension6}, {frameProperty.Dimension7}, {frameProperty.Dimension8}, {frameProperty.Dimension9}, {frameProperty.Dimension10}");
            }
        }

        private void WriteMgtAreaThickness(List<string> textMgt)
        {
            if (AreaThicknesses.Count > 0)
            {
                textMgt.Add("*THICKNESS");
                textMgt.Add("; i, (section type=VALUE), (name), (same thickness all around=YES), (thickness in plane), (thickness out of plane=0), (offset=NO), (offtype=0), (value=0)");
            }
            foreach (var kvp in AreaThicknesses)
            {
                AreaThicknessModel areaThickness = kvp.Value;
                string name = areaThickness.Name;
                if (name.Length > 28)
                    name = name.Substring(0, 28);

                string offsett = areaThickness.Offset != 0 ? "YES" : "NO";
                string offsettype = areaThickness.Offset != 0 ? "1" : "0";

                textMgt.Add($"{areaThickness.Id}, VALUE, {name}, YES, {areaThickness.Thickness}, 0, {offsett}, {offsettype}, {areaThickness.Offset}");
            }
        }

        private void WriteMgtFrameElement(List<string> textMgt)
        {
            if (FrameElements.Count > 0)
            {
                textMgt.Add("*ELEMENT");
                textMgt.Add("; i, BEAM, (index material), (index property), (index node start), (index node end), (angle=0), (index subtype=0)");
            }
            foreach (var kvp in FrameElements)
            {
                FrameElementModel frameElement = kvp.Value;
                textMgt.Add($"{frameElement.Id}, BEAM, {frameElement.Material.Id}, {frameElement.FrameProperty.Id}, {frameElement.NodeStart.Id}, {frameElement.NodeEnd.Id}, {frameElement.Angle} , 0");
            }
        }

        private void WriteMgtAreaElement(List<string> textMgt)
        {
            if (AreaElements.Count > 0)
            {
                textMgt.Add("*ELEMENT");
                textMgt.Add("; i, PLATE, (index material), (index property), (index joint 1), (index joint 2), (index joint 3), (index joint 4), (subtype thick=1 thin=2), (local axis)");
            }
            foreach (var kvp in AreaElements)
            {
                AreaElementModel areaElement = kvp.Value;
                int id4 = areaElement.NodeList.Count == 4 ? areaElement.NodeList[3].Id : 0;
                textMgt.Add($"{areaElement.Id}, PLATE, {areaElement.Material.Id}, {areaElement.AreaThickness.Id}, {areaElement.NodeList[0].Id}, {areaElement.NodeList[1].Id}, " +
                    $"{areaElement.NodeList[2].Id}, {id4}, 1, {areaElement.Angle}");
            }
        }

        private void WriteMgtLoadCase(List<string> textMgt)
        {
            if (LoadCases.Count > 0)
            {
                textMgt.Add("*STLDCASE");
                textMgt.Add("; (name), (load type), (desc)");
            }
            foreach (var kvp in LoadCases)
            {
                var loadCase = kvp.Value;
                textMgt.Add($"{loadCase.Name}, {loadCase.Type.ToString()}, {loadCase.Description}");
            }
        }

        private void WriteMgtLoadGroup(List<string> textMgt)
        {
            HashSet<string> loadGroups = new HashSet<string>();
            foreach (var node in NodeElements)
            {
                for(int j = 0; j < node.Value.NodalLoadList.Count; j++)
                {
                    NodalLoadModel load = node.Value.NodalLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            foreach(var frame in FrameElements)
            {
                for (int j = 0; j < frame.Value.FrameLoadList.Count; j++)
                {
                    FrameLoadModel load = frame.Value.FrameLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            foreach(var area in AreaElements)
            {
                for (int j = 0; j < area.Value.AreaLoadList.Count; j++)
                {
                    AreaLoadModel load = area.Value.AreaLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            if (loadGroups.Count > 0)
            {
                textMgt.Add("*LOAD-GROUP    ; Load Group");
                textMgt.Add("; NAME");

                foreach (var name in loadGroups)
                    textMgt.Add($"{name}");
            }
        }

        private void WriteMgtBoundaryGroup(List<string> textMgt)
        {
            HashSet<string> boundaryGroups = new HashSet<string>();
            foreach (var node in NodeElements)
            {
                if(node.Value.Support != null && node.Value.Support.BoundaryGroup != null)
                    boundaryGroups.Add(node.Value.Support.BoundaryGroup.Name);
                
            }
            if (boundaryGroups.Count > 0)
            {
                textMgt.Add("*BNDR-GROUP    ; Boundary Group");
                textMgt.Add("; NAME, AUTOTYPE");

                foreach (var name in boundaryGroups)
                    textMgt.Add($"{name}, 9272");
            }
        }

        private void WriteMgtFrameLoad(List<string> textMgt)
        {
            bool flag = false;
            foreach (var kvp in FrameElements)
            {
                FrameElementModel frameElement = kvp.Value;
                foreach (var item in frameElement.FrameLoadList)
                {
                    string forceOrMoment = item.LoadType == FrameLoadModel.FrameLoadTypes.Force ? "UNILOAD" : "UNIMOMENT";
                    string proj = (!item.IsProjected) ? "NO" : "YES";

                    textMgt.Add("*USE-STLD, " + item.LoadCase.Name);
                    textMgt.Add("*BEAMLOAD");
                    if (!flag)
                    {
                        textMgt.Add("; (index element), (load classificiation=BEAM), (loadtype), (direction), (projected), (bEccen=NO), (eccenDir=aDir[1]), (i-end=''), (j-end=''), (bj-end=''), " +
                            "(location relative 1), (force1), (location relative 2), (force2), (location relative 3=0), (force3=0), (location relative 4=0), (force4=0)");
                        flag = true;
                    }
                    string loadGroup = item.LoadGroup == null ? "" : item.LoadGroup.Name;
                    textMgt.Add($"{frameElement.Id}, BEAM, {forceOrMoment}, {item.Direction.ToString()}, {proj}, NO, aDir[1], , , , " +
                        $"{item.StartLocationRelative}, {item.StartLoad}, {item.EndLocationRelative}, {item.EndLoad}, 0, 0, 0, 0, {loadGroup}");
                }
            }
        }

        private void WriteMgtNodalLoad(List<string> textMgt)
        {
            bool flag = false;
            foreach (var node in NodeElements)
            {
                foreach (var load in node.Value.NodalLoadList)
                {
                    if (load.FX != 0.0 || load.FY != 0.0 || load.FZ != 0.0 || load.MX != 0.0 || load.MY != 0.0 || load.MZ != 0.0)
                    {
                        textMgt.Add("*USE-STLD, " + load.LoadCase.Name);
                        textMgt.Add("*CONLOAD");
                        if (!flag)
                        {
                            textMgt.Add("; (index node), (FX), (FY), (FZ), (MX), (MY), (MZ), (group='') ");
                            flag = true;
                        }
                        string loadGroup = load.LoadGroup == null ? "" : load.LoadGroup.Name;
                        textMgt.Add($"{node.Value.Id}, {load.FX}, {load.FY}, {load.FZ}, {load.MX}, {load.MY}, {load.MZ},{loadGroup} ");
                    }
                }
            }
        }

        private void WriteMgtAreaLoad(List<string> textMgt)
        {
            bool flag = false;
            foreach (var kvp in AreaElements)
            {
                AreaElementModel areaElement = kvp.Value;
                List<AreaLoadModel> areaLoadList = areaElement.AreaLoadList;
                for (int i = 0; i < areaLoadList.Count; i++)
                {
                    AreaLoadModel item = areaLoadList[i];
                    string proj = ((!item.IsProjected) ? "NO" : "YES");
                    textMgt.Add("*USE-STLD, " + item.LoadCase.Name);
                    textMgt.Add("*PRESSURE   ;Pressure Loads");
                    if (!flag)
                    {
                        textMgt.Add("; (index element), (load classificiation=PRES), (elementtype=PLATE), (loadtype=FACE), (direction), (Vx=0), (Vy=0), (Vz=0),(projected yes/no), (load uniform), (P1=0), (P2=0), (P3=0), (P4=0), (group=''), (psltkey=0)");
                        flag = true;
                    }
                    string loadGroup = item.LoadGroup == null ? "" : item.LoadGroup.Name;
                    textMgt.Add($"{areaElement.Id}, PRES, PLATE, FACE, {item.Direction.ToString()}, 0, 0, 0, {proj}, 0, {item.P1}, {item.P2}, {item.P3}, {item.P4}, {loadGroup},0");
                }
            }
        }

        private void WriteMgtLoadCombination(List<string> textMgt)
        {
            if (LoadCombinations.Count > 0)
            {
                textMgt.Add("*LOADCOMB    ; Combinations");
                textMgt.Add("; (NAME=name), (kind=GEN), (active=ACTIVE), (bES=0), (linear add=0, envelope=1), (desc=''), (iSERVE-TYPE=0), (nLCOMTYPE=0), (nSEISTYPE=0)");
                textMgt.Add("; (load type=ST), (LCNAME1), (FACTOR)");
            }
            foreach (var kvp in LoadCombinations)
            {
                var loadCombination = kvp.Value;
                int type = loadCombination.Type == LoadCombinationModel.LoadCombinationTypes.Linear ? 0 : 1;
                textMgt.Add($"NAME={loadCombination.Name}, GEN, ACTIVE, 0, {type}, {loadCombination.Description}, 0, 0, 0");
                foreach (var item in loadCombination.LoadFactorList)
                    textMgt.Add($"ST, {item.LoadCase.Name}, {item.Factor}");
            }
        }

        private void WriteMgtGroup(List<string> textMgt)
        {
            if (Groups.Count > 0)
            {
                textMgt.Add("*GROUP    ; Group");
                textMgt.Add("; (group name), (node list), (element list), (plane type = 0)");
            }
            foreach (var kvp in Groups)
            {
                var group = kvp.Value;
                string groupName = group.Name;

                string nodeList = "";
                string elementList = "";
                for (int i = 0; i < NodeElements.Count; i++)
                {
                    NodeElementModel node = NodeElements.ElementAt(i).Value;
                    for (int j = 0; j < node.Groups.Count; j++)
                    {
                        if (node.Groups[j].Name == groupName)
                            nodeList += $"{node.Id} ";
                    }
                }
                for (int i = 0; i < FrameElements.Count; i++)
                {
                    FrameElementModel elem = FrameElements.ElementAt(i).Value;
                    for (int j = 0; j < elem.Groups.Count; j++)
                    {
                        if (elem.Groups[j].Name == groupName)
                            elementList += $"{elem.Id} ";
                    }
                }
                for (int i = 0; i < AreaElements.Count; i++)
                {
                    AreaElementModel elem = AreaElements.ElementAt(i).Value;
                    for (int j = 0; j < elem.Groups.Count; j++)
                    {
                        if (elem.Groups[j].Name == groupName)
                            elementList += $"{elem.Id} ";
                    }
                }
                textMgt.Add(groupName + ", " + nodeList + ", " + elementList + ", 0");
            }
        }

        private void WriteMgtSupport(List<string> textMgt)
        {
            bool flag = false;
            int num = 0;
            foreach (var kvp in NodeElements)
            {
                var node = kvp.Value;
                if (node.Support.Dx || node.Support.Dy || node.Support.Dz || node.Support.Mx || node.Support.My || node.Support.Mz)
                {
                    if (!flag)
                    {
                        textMgt.Add("*CONSTRAINT ; Restraints");
                        textMgt.Add("; (index node), (resrtaint condition-i 000000 Dx,Dy,Dz,Rx,Ry,Rz), (groups='')");
                        flag = true;
                    }
                    int dx = node.Support.Dx ? 1 : 0;
                    int dy = node.Support.Dy ? 1 : 0;
                    int dz = node.Support.Dz ? 1 : 0;
                    int mx = node.Support.Mx ? 1 : 0;
                    int my = node.Support.My ? 1 : 0;
                    int mz = node.Support.Mz ? 1 : 0;

                    string arg = dx.ToString() + dy.ToString() + dz.ToString() + mx.ToString() + my.ToString() + mz.ToString();
                    string boundaryGroup = node.Support.BoundaryGroup == null ? "" : node.Support.BoundaryGroup.Name;
                    textMgt.Add($"{node.Id}, {arg}, {boundaryGroup}");
                }
                num++;
            }
        }

        private void WriteMgtSelfWeight(List<string> textMgt)
        {
            if (SelfWeight != null)
            {
                textMgt.Add("*USE-STLD, " + SelfWeight.LoadCase.Name);
                textMgt.Add("*SELFWEIGHT");
                textMgt.Add($"{SelfWeight.FactorX}, {SelfWeight.FactorY}, {SelfWeight.FactorZ}");
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ModelModel);
        }

        public bool Equals(ModelModel other)
        {
            return !(other is null) &&
                   EqualityComparer<ModelUnits>.Default.Equals(ModelUnits, other.ModelUnits) &&
                   EqualityComparer<UniqueNameCollection<MaterialModel>>.Default.Equals(Materials, other.Materials) &&
                   EqualityComparer<UniqueNameCollection<LoadCaseModel>>.Default.Equals(LoadCases, other.LoadCases) &&
                   EqualityComparer<UniqueNameCollection<LoadCombinationModel>>.Default.Equals(LoadCombinations, other.LoadCombinations) &&
                   EqualityComparer<UniqueNameCollection<ElementGroupModel>>.Default.Equals(Groups, other.Groups) &&
                   EqualityComparer<UniqueNameCollection<FramePropertyModel>>.Default.Equals(FrameProperties, other.FrameProperties) &&
                   EqualityComparer<UniqueNameCollection<AreaThicknessModel>>.Default.Equals(AreaThicknesses, other.AreaThicknesses) &&
                   EqualityComparer<UniqueIdCollection<NodeElementModel>>.Default.Equals(NodeElements, other.NodeElements) &&
                   EqualityComparer<UniqueIdCollection<FrameElementModel>>.Default.Equals(FrameElements, other.FrameElements) &&
                   EqualityComparer<UniqueIdCollection<AreaElementModel>>.Default.Equals(AreaElements, other.AreaElements);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + EqualityComparer<ModelUnits>.Default.GetHashCode(ModelUnits);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<MaterialModel>>.Default.GetHashCode(Materials);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<LoadCaseModel>>.Default.GetHashCode(LoadCases);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<LoadCombinationModel>>.Default.GetHashCode(LoadCombinations);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<ElementGroupModel>>.Default.GetHashCode(Groups);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<FramePropertyModel>>.Default.GetHashCode(FrameProperties);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<AreaThicknessModel>>.Default.GetHashCode(AreaThicknesses);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<NodeElementModel>>.Default.GetHashCode(NodeElements);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<FrameElementModel>>.Default.GetHashCode(FrameElements);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<AreaElementModel>>.Default.GetHashCode(AreaElements);
            return hashCode;
        }

        public static bool operator ==(ModelModel left, ModelModel right)
        {
            return EqualityComparer<ModelModel>.Default.Equals(left, right);
        }

        public static bool operator !=(ModelModel left, ModelModel right)
        {
            return !(left == right);
        }
    }
}
