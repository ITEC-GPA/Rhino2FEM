using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rhino.Geometry;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Core.Collections;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Elements;
using Rhino2Midas.Core.Helper;
using Rhino2Midas.Core.Loads;
using Rhino2Midas.Core.Settings;

namespace Rhino2Midas.Core.Models
{
    public class ModelModel : IEquatable<ModelModel>
    {
        public ModelUnitsModel ModelUnits { get; set; }
        public UniqueIdCollection<MaterialModel> Materials { get; }
        public UniqueNameCollection<LoadCaseModel> LoadCases { get; }
        public UniqueNameCollection<LoadCombinationModel> LoadCombinations { get; }
        public UniqueNameCollection<ElementGroupModel> Groups { get; }
        public UniqueIdCollection<FramePropertyModel> FrameProperties { get; }
        public UniqueIdCollection<AreaThicknessModel> AreaThicknesses { get; }
        public UniqueIdCollection<NodeElementModel> NodeElements { get; }
        public UniqueIdCollection<FrameElementModel> FrameElements { get; }
        public UniqueIdCollection<AreaElementModel> AreaElements { get; }
        public UniqueIdCollection<LinkElementModel> LinkElements { get; }
        public SelfWeightModel SelfWeight { get; set; }

        public ModelModel()
        {
            Materials = new UniqueIdCollection<MaterialModel>();
            LoadCases = new UniqueNameCollection<LoadCaseModel>();
            LoadCombinations = new UniqueNameCollection<LoadCombinationModel>();
            Groups = new UniqueNameCollection<ElementGroupModel>();
            FrameProperties = new UniqueIdCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueIdCollection<AreaThicknessModel>();
            NodeElements = new UniqueIdCollection<NodeElementModel>();
            FrameElements = new UniqueIdCollection<FrameElementModel>();
            AreaElements = new UniqueIdCollection<AreaElementModel>();
            LinkElements = new UniqueIdCollection<LinkElementModel>();
        }

        public ModelModel(ModelModel model)
        {
            Materials = new UniqueIdCollection<MaterialModel>();
            LoadCases = new UniqueNameCollection<LoadCaseModel>();
            LoadCombinations = new UniqueNameCollection<LoadCombinationModel>();
            Groups = new UniqueNameCollection<ElementGroupModel>();
            FrameProperties = new UniqueIdCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueIdCollection<AreaThicknessModel>();
            NodeElements = new UniqueIdCollection<NodeElementModel>();
            FrameElements = new UniqueIdCollection<FrameElementModel>();
            AreaElements = new UniqueIdCollection<AreaElementModel>();
            LinkElements = new UniqueIdCollection<LinkElementModel>();

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
            for (int i = 0; i < model.LinkElements.Count; i++)
                LinkElements.Add(new LinkElementModel(model.LinkElements.Values.ElementAt(i)));
        }

        public void BuildModel(List<NodeElementModel> inputNodes = null, List<FrameElementModel> inputFrames = null, List<AreaElementModel> inputAreas = null,
            List<LinkElementModel> inputLinks = null, List<LoadCombinationModel> inputcombos = null, SelfWeightModel inputSelfWeight = null)
        {
            #region Check Element IDs

            Dictionary<int, NodeElementModel> nodeDictionary = new Dictionary<int, NodeElementModel>();
            Dictionary<int, FrameElementModel> frameDictionary = new Dictionary<int, FrameElementModel>();
            Dictionary<int, AreaElementModel> areaDictionary = new Dictionary<int, AreaElementModel>();
            Dictionary<int, LinkElementModel> linkDictionary = new Dictionary<int, LinkElementModel>();

            if (inputNodes == null)
                inputNodes = new List<NodeElementModel>();
            if (inputFrames == null)
                inputFrames = new List<FrameElementModel>();
            if (inputAreas == null)
                inputAreas = new List<AreaElementModel>();
            if (inputLinks == null)
                inputLinks = new List<LinkElementModel>();
            if (inputcombos == null)
                inputcombos = new List<LoadCombinationModel>();
            SelfWeightModel selfWeight = null;
            if (inputSelfWeight != null)
                selfWeight = new SelfWeightModel(inputSelfWeight);

            int idNode = 1;
            List<int> usedNodeIds = inputNodes.Select(x => x.Id).ToList();
            List<int> usedElementIds = inputFrames.Select(x => x.Id).ToList();
            usedElementIds.AddRange(inputAreas.Select(x => x.Id).ToList());
            List<int> usedLinkIds = new List<int>();

            List<NodeElementModel> nodes = new List<NodeElementModel>();
            List<FrameElementModel> frames = new List<FrameElementModel>();
            List<AreaElementModel> areas = new List<AreaElementModel>();
            List<LinkElementModel> links = new List<LinkElementModel>();
            List<LoadCombinationModel> combos = new List<LoadCombinationModel>();

            for (int i = 0; i < inputNodes.Count; i++)
                nodes.Add(new NodeElementModel(inputNodes[i]));
            for (int i = 0; i < inputFrames.Count; i++)
                frames.Add(new FrameElementModel(inputFrames[i]));
            for (int i = 0; i < inputAreas.Count; i++)
                areas.Add(new AreaElementModel(inputAreas[i]));
            for (int i = 0; i < inputLinks.Count; i++)
                links.Add(new LinkElementModel(inputLinks[i]));
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

            int idLink = 1;
            for (int i = 0; i < links.Count; i++)
            {
                if (linkDictionary.ContainsKey(links[i].Id) && links[i].Id != ModelObjectId.UNASSIGNED)
                {
                    linkDictionary[links[i].Id].Merge(links[i]);
                }
                else
                {
                    LinkElementModel nn = new LinkElementModel(links[i]);
                    if (nn.Id != ModelObjectId.UNASSIGNED)
                    {
                        linkDictionary.Add(nn.Id, nn);
                    }
                    else
                    {
                        idLink = NewId(usedLinkIds, idLink);
                        nn.Id = idLink;
                        linkDictionary.Add(nn.Id, nn);
                        usedLinkIds.Add(idLink);
                        idLink++;
                    }
                }
            }

            #endregion

            #region Matching add nodes

            double tol = ModelUnits.Tolerance;

            List<NodeElementModel> nodesBuffer = nodeDictionary.Values.ToList();
            List<FrameElementModel> framesBuffer = frameDictionary.Values.ToList();
            List<AreaElementModel> areasBuffer = areaDictionary.Values.ToList();
            List<LinkElementModel> linkBuffer = linkDictionary.Values.ToList();

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

            for (int i = 0; i < linkBuffer.Count; i++)
            {
                {
                    Point3d pt = linkBuffer[i].NodeStart.Position;
                    bool found = false;
                    int nodeId = ModelObjectId.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        linkBuffer[i].NodeStart = nodeDictionary[nodeId];
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
                        linkBuffer[i].NodeStart = newNode;
                    }
                }
                {
                    Point3d pt = linkBuffer[i].NodeEnd.Position;
                    bool found = false;
                    int nodeId = ModelObjectId.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        linkBuffer[i].NodeEnd = nodeDictionary[nodeId];
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
                        linkBuffer[i].NodeEnd = newNode;
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

                bool exist = false;
                for (int j = 0; j < Materials.Count; j++)
                {
                    if (framesBuffer[i].Material == Materials.ElementAt(j).Value)
                    {
                        exist = true;
                        framesBuffer[i].Material = Materials.ElementAt(j).Value;
                    }
                }
                if (!exist)
                {
                    if (mat.Id == ModelObjectId.UNASSIGNED)
                        mat.Id = NewId(usedMaterialIds, idMaterial);
                    usedMaterialIds.Add(mat.Id);
                    Materials.Add(mat);
                    framesBuffer[i].Material = Materials[mat.Id];
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var mat = new MaterialModel(areasBuffer[i].Material);

                bool exist = false;
                for (int j = 0; j < Materials.Count; j++)
                {
                    if (areasBuffer[i].Material == Materials.ElementAt(j).Value)
                    {
                        exist = true;
                        areasBuffer[i].Material = Materials.ElementAt(j).Value;
                    }
                }
                if (!exist)
                {
                    if (mat.Id == ModelObjectId.UNASSIGNED)
                        mat.Id = NewId(usedMaterialIds, idMaterial);
                    usedMaterialIds.Add(mat.Id);
                    Materials.Add(mat);
                    areasBuffer[i].Material = Materials[mat.Id];
                }
            }

            for (int i = 0; i < framesBuffer.Count; i++)
            {
                var fp = new FramePropertyModel(framesBuffer[i].FrameProperty);

                bool exist = false;
                for (int j = 0; j < FrameProperties.Count; j++)
                {
                    if (framesBuffer[i].FrameProperty == FrameProperties.ElementAt(j).Value)
                    {
                        exist = true;
                        framesBuffer[i].FrameProperty = FrameProperties.ElementAt(j).Value;
                    }
                }
                if (!exist)
                {
                    if (fp.Id == ModelObjectId.UNASSIGNED)
                        fp.Id = NewId(usedFramePropertyIds, idFrameProperty);
                    usedFramePropertyIds.Add(fp.Id);
                    FrameProperties.Add(fp);
                    framesBuffer[i].FrameProperty = FrameProperties[fp.Id];
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var fp = new AreaThicknessModel(areasBuffer[i].AreaThickness);

                bool exist = false;
                for (int j = 0; j < AreaThicknesses.Count; j++)
                {
                    if (areasBuffer[i].AreaThickness == AreaThicknesses.ElementAt(j).Value)
                    {
                        exist = true;
                        areasBuffer[i].AreaThickness = AreaThicknesses.ElementAt(j).Value;
                    }
                }
                if (!exist)
                {
                    if (fp.Id == ModelObjectId.UNASSIGNED)
                        fp.Id = NewId(usedAreaPropertyIds, idAreaProperty);
                    usedAreaPropertyIds.Add(fp.Id);
                    AreaThicknesses.Add(fp);
                    areasBuffer[i].AreaThickness = AreaThicknesses[fp.Id];
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
            LinkElements.AddRange(linkBuffer);
        }

        public List<string> CreateMgtFile()
        {
            List<string> outputStrings = new List<string>();
            WriteMgtHeader(outputStrings);
            WriteMgtUnit(outputStrings);
            WriteMgtGroup(outputStrings);
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
            WriteMgtLinkElement(outputStrings);
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
                textMgt.Add("; iMAT, TYPE, MNAME, SPHEAT, HEATCO, PLAST, TUNIT, bMASS, DAMPRATIO, [DATA1]           ; STEEL, CONC, USER \n\r" +
                    "; iMAT, TYPE, MNAME, SPHEAT, HEATCO, PLAST, TUNIT, bMASS, DAMPRATIO, [DATA2], [DATA2]                      ; SRC\n\r" +
                    "; [DATA1] : 1, STANDARD, CODE/PRODUCT, DB, USEELAST, ELAST\n\r" +
                    "; [DATA1] : 2, ELAST, POISN, THERMAL, DEN, MASS\n\r" +
                    "; [DATA1] : 3, Ex, Ey, Ez, Tx, Ty, Tz, Sxy, Sxz, Syz, Pxy, Pxz, Pyz, DEN, MASS         ; Orthotropic\n\r" +
                    "; [DATA2] : 1, STANDARD, CODE/PRODUCT, DB, USEELAST, ELAST or 2, ELAST, POISN, THERMAL, DEN, MASS");
            }
            foreach (KeyValuePair<int, MaterialModel> kvp in Materials)
            {
                MaterialModel material = kvp.Value;

                string matName = material.Name;
                if (matName.Length > 16)
                    matName = matName.Substring(0, 28);
                if (material.Standard == MaterialModel.Standards.Custom)
                {
                    textMgt.Add($"{material.Id}, {material.Type.GetDescription()}, {material.Name}, 0, 0, , C, NO, {material.DampingRatio}, 2, {material.ModulusElasticity}," +
                        $" {material.PoissonRatio}, {material.ThermalCoefficient}, {material.Density}, {material.Mass}");
                }
                else
                {
                    textMgt.Add($"{material.Id}, {material.Type.GetDescription()}, {material.Name}, 0, 0, , C, NO, {material.DampingRatio}, 1, {material.Standard.GetDescription()}, " +
                        $",{material.Name}, NO, {material.ModulusElasticity}");
                }
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

                if (frameProperty.Type == FramePropertyModel.Types.DBUSER)
                    textMgt.Add($"{frameProperty.Id}, {frameProperty.Type}, {framePropertyName}, {frameProperty.Offset.ToString()}, 0, 0, 0, 0, 0, 0, YES, NO, {frameProperty.PropertyType.ToString()}, 2, " +
                        $"{frameProperty.Dimension1}, {frameProperty.Dimension2}, {frameProperty.Dimension3}, {frameProperty.Dimension4}, {frameProperty.Dimension5}, " +
                        $"{frameProperty.Dimension6}, {frameProperty.Dimension7}, {frameProperty.Dimension8}, {frameProperty.Dimension9}, {frameProperty.Dimension10}");
                else if (frameProperty.Type == FramePropertyModel.Types.COMPOSITE_I)
                    textMgt.Add($"{frameProperty.Id}, {frameProperty.Type}, {framePropertyName}, {frameProperty.Offset.ToString()}, 0, 0, 0, 0, 0, 0, YES, NO, NO, I, " +
                        $"{frameProperty.Dimension1}, {frameProperty.Dimension2}, {frameProperty.Dimension3}, {frameProperty.Dimension4}, {frameProperty.Dimension5}, {frameProperty.Dimension6}, " +
                        "0, 0, 0, 0, 0, 0, 0\r\n       " +
                        "0\r\n       " +
                        "0\r\n       " +
                        "0\r\\n      " +
                        $"{frameProperty.Dimension7}, 1, {frameProperty.Dimension7}, {frameProperty.Dimension7}, {frameProperty.Dimension8}, {frameProperty.Dimension9}" +
                        $", {frameProperty.CompositeData1}, {frameProperty.CompositeData2}, {frameProperty.CompositeData3}, {frameProperty.CompositeData4}, {frameProperty.CompositeData5}");
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
                textMgt.Add($"{frameElement.Id}, BEAM, {frameElement.Material.Id}, {frameElement.FrameProperty.Id}, {frameElement.NodeStart.Id}, {frameElement.NodeEnd.Id}, {Rhino.RhinoMath.ToDegrees(frameElement.Angle)} , 0");
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

        private void WriteMgtLinkElement(List<string> textMgt)
        {
            if (LinkElements.Count > 0)
            {
                textMgt.Add("*ELASTICLINK; Elastic Link");
                textMgt.Add("; iNO, iNODE1, iNODE2, LINK, ANGLE, R_SDx, R_SDy, R_SDz, R_SRx, R_SRy, R_SRz, SDx, SDy, SDz, SRx, SRy, SRz... ");
                textMgt.Add("; bSHEAR, DRy, DRz, GROUP; GEN");
                textMgt.Add("                ; iNO, iNODE1, iNODE2, LINK, ANGLE, bSHEAR, DRy, DRz, GROUP; RIGID,SADDLE");
                textMgt.Add("               ; iNO, iNODE1, iNODE2, LINK, ANGLE, SDx, bSHEAR, DRy, DRz, GROUP; TENS,COMP");
                textMgt.Add("; iNO, iNODE1, iNODE2, LINK, ANGLE, DIR, FUNCTION, bSHEAR, DRENDI, GROUP; MULTI LINEAR");
            }
            foreach (var kvp in LinkElements)
            {
                LinkElementModel linkElement = kvp.Value;
                if (linkElement.LinkProperty.Type == LinkPropertyModel.LinkPropertyTypes.GEN)
                    textMgt.Add($"{linkElement.Id}, {linkElement.NodeStart.Id}, {linkElement.NodeEnd.Id}, {linkElement.LinkProperty.Type}, " +
                        $"0, NO, NO, NO, NO, NO, NO," +
                        $"{linkElement.LinkProperty.Kx}, {linkElement.LinkProperty.Ky},{linkElement.LinkProperty.Kz}," +
                        $"{linkElement.LinkProperty.Rx},{linkElement.LinkProperty.Ry},{linkElement.LinkProperty.Rz}," +
                        $"NO, 0.5, 0.5, {linkElement.LinkProperty.BoundaryGroup.Name}");
                else if (linkElement.LinkProperty.Type == LinkPropertyModel.LinkPropertyTypes.RIGID)
                    textMgt.Add($"{linkElement.Id}, {linkElement.NodeStart.Id}, {linkElement.NodeEnd.Id}, {linkElement.LinkProperty.Type}, 0, " +
                        $"NO, 0.5, 0.5, {linkElement.LinkProperty.BoundaryGroup.Name}");
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
                for (int j = 0; j < node.Value.NodalLoadList.Count; j++)
                {
                    NodalLoadModel load = node.Value.NodalLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            foreach (var frame in FrameElements)
            {
                for (int j = 0; j < frame.Value.FrameLoadList.Count; j++)
                {
                    FrameLoadModel load = frame.Value.FrameLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            foreach (var area in AreaElements)
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
                if (node.Value.Support != null && node.Value.Support.BoundaryGroup != null)
                    boundaryGroups.Add(node.Value.Support.BoundaryGroup.Name);

            }
            foreach (var link in LinkElements)
            {
                if (link.Value.LinkProperty != null && link.Value.LinkProperty.BoundaryGroup != null)
                    boundaryGroups.Add(link.Value.LinkProperty.BoundaryGroup.Name);

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

        public void ReadMgtFile(string[] textMgt)
        {
            string unitsMatchTest = "*UNIT    ; Unit System";
            string groupMatchTest = "*GROUP    ; Group";
            string nodeCoordinateMatchTest = "*NODE    ; Nodes";
            string elementMatchTest = "*ELEMENT    ; Elements";
            string materialMatchTest = "*MATERIAL    ; Material";
            string sectionMatchTest = "*SECTION    ; Section";
            string thicknessMatchTest = "*THICKNESS    ; Thickness";
            string elasticLinkMatchTest = "*ELASTICLINK    ; Elastic Link";

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == unitsMatchTest)
                {
                    Dictionary<string, ModelUnitsModel.LengthUnitTypes> kvpL = new Dictionary<string, ModelUnitsModel.LengthUnitTypes>()
                    {
                        { "MM", ModelUnitsModel.LengthUnitTypes.MM },
                        { "CM", ModelUnitsModel.LengthUnitTypes.CM },
                        { "M", ModelUnitsModel.LengthUnitTypes.M },
                    };
                    Dictionary<string, ModelUnitsModel.ForceUnitTypes> kvpF = new Dictionary<string, ModelUnitsModel.ForceUnitTypes>()
                    {
                        { "KN", ModelUnitsModel.ForceUnitTypes.KN },
                        { "N", ModelUnitsModel.ForceUnitTypes.N },
                    };
                    Dictionary<string, ModelUnitsModel.HeatUnitTypes> kvpH = new Dictionary<string, ModelUnitsModel.HeatUnitTypes>()
                    {
                        { "KJ", ModelUnitsModel.HeatUnitTypes.KJ },
                        { "J", ModelUnitsModel.HeatUnitTypes.J },
                        { "BTU", ModelUnitsModel.HeatUnitTypes.BTU },
                        { "KCAL", ModelUnitsModel.HeatUnitTypes.KCAL },
                        { "CAL", ModelUnitsModel.HeatUnitTypes.CAL },
                    };
                    Dictionary<string, ModelUnitsModel.TemperatureUnitTypes> kvpT = new Dictionary<string, ModelUnitsModel.TemperatureUnitTypes>()
                    {
                        { "C", ModelUnitsModel.TemperatureUnitTypes.C },
                        { "F", ModelUnitsModel.TemperatureUnitTypes.F },
                    };

                    for (int j = i + 2; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        ModelUnitsModel modelUnits = new ModelUnitsModel()
                        {
                            ForceUnit = kvpF[splitlist[0].Trim()],
                            LengthUnit = kvpL[splitlist[1].Trim()],
                            HeatUnit = kvpH[splitlist[2].Trim()],
                            TemperatureUnit = kvpT[splitlist[3].Trim()],
                        };

                        ModelUnits = modelUnits;
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == materialMatchTest)
                {
                    for (int j = i + 7; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        MaterialModel materialModel = new MaterialModel()
                        {
                            Id = int.Parse(splitlist[0].Trim()),
                            Name = splitlist[2].Trim(),
                        };
                        if (splitlist[1] == "CONC")
                            materialModel.Type = MaterialModel.MaterialTypes.Concrete;
                        else if (splitlist[1] == "STEEL")
                            materialModel.Type = MaterialModel.MaterialTypes.Steel;

                        Materials.Add(materialModel);
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == sectionMatchTest)
                {
                    Dictionary<string, FramePropertyModel.FramePropertyTypes> kvpType = new Dictionary<string, FramePropertyModel.FramePropertyTypes>()
                    {
                        {"SB", FramePropertyModel.FramePropertyTypes.SB},
                        {"SR",  FramePropertyModel.FramePropertyTypes.SR},
                        {"P",  FramePropertyModel.FramePropertyTypes.P},
                        {"L",  FramePropertyModel.FramePropertyTypes.L},
                        {"C",  FramePropertyModel.FramePropertyTypes.C},
                        {"H",  FramePropertyModel.FramePropertyTypes.H},
                        {"T",  FramePropertyModel.FramePropertyTypes.T},
                        {"B",  FramePropertyModel.FramePropertyTypes.B},
                    };

                    Dictionary<string, FramePropertyModel.OffsetTypes> kvpOffset = new Dictionary<string, FramePropertyModel.OffsetTypes>()
                    {
                        {"LT", FramePropertyModel.OffsetTypes.LT},
                        {"CT", FramePropertyModel.OffsetTypes.CT},
                        {"RT", FramePropertyModel.OffsetTypes.RT},
                        {"LC", FramePropertyModel.OffsetTypes.LC},
                        {"CC", FramePropertyModel.OffsetTypes.CC},
                        {"RC", FramePropertyModel.OffsetTypes.RC},
                        {"LB", FramePropertyModel.OffsetTypes.LB},
                        {"CB", FramePropertyModel.OffsetTypes.CB},
                        {"RB", FramePropertyModel.OffsetTypes.RB},
                    };

                    for (int j = i + 42; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }
                        if (textMgt[j].StartsWith(";"))
                        {
                            continue;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        FramePropertyModel sectionModel = new FramePropertyModel()
                        {
                            Id = int.Parse(splitlist[0].Trim()),
                            PropertyType = kvpType[splitlist[12].Trim()],
                            Name = splitlist[2].Trim(),
                            Offset = kvpOffset[splitlist[3].Trim()],
                        };

                        FrameProperties.Add(sectionModel);
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == thicknessMatchTest)
                {
                    for (int j = i + 11; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        AreaThicknessModel areaThicknessModel = new AreaThicknessModel()
                        {
                            Id = int.Parse(splitlist[0].Trim()),
                            Name = splitlist[2].Trim(),
                            Thickness = double.Parse(splitlist[4].Trim()),
                            Offset = double.Parse(splitlist[8].Trim()),
                        };

                        AreaThicknesses.Add(areaThicknessModel);
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == nodeCoordinateMatchTest)
                {
                    for (int j = i + 2; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        NodeElementModel nodeElementModel = new NodeElementModel()
                        {
                            Id = int.Parse(splitlist[0].Trim()),
                            X = double.Parse(splitlist[1].Trim()),
                            Y = double.Parse(splitlist[2].Trim()),
                            Z = double.Parse(splitlist[3].Trim()),
                        };

                        NodeElements.Add(nodeElementModel);
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == elementMatchTest)
                {
                    for (int j = i + 5; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        var elemType = splitlist[1].Trim();

                        if (elemType == "BEAM")
                        {
                            FrameElementModel frameElementModel = new FrameElementModel()
                            {
                                Id = int.Parse(splitlist[0].Trim()),
                                Material = Materials[int.Parse(splitlist[2].Trim())],
                                FrameProperty = FrameProperties[int.Parse(splitlist[3].Trim())],
                                NodeStart = NodeElements[int.Parse(splitlist[4].Trim())],
                                NodeEnd = NodeElements[int.Parse(splitlist[5].Trim())],
                                Angle = double.Parse(splitlist[6].Trim()),
                            };
                            FrameElements.Add(frameElementModel);
                        }

                        if (elemType == "PLATE")
                        {
                            List<NodeElementModel> ns = new List<NodeElementModel>
                            {
                                NodeElements[int.Parse(splitlist[4].Trim())],
                                NodeElements[int.Parse(splitlist[5].Trim())],
                                NodeElements[int.Parse(splitlist[6].Trim())]
                            };
                            if (splitlist[7].Trim() != "0")
                                ns.Add(NodeElements[int.Parse(splitlist[7].Trim())]);

                            AreaElementModel areaElementModel = new AreaElementModel(ns, AreaThicknesses[int.Parse(splitlist[3].Trim())], Materials[int.Parse(splitlist[2].Trim())],
                                double.Parse(splitlist[6].Trim()));
                            areaElementModel.Id = int.Parse(splitlist[0].Trim());

                            AreaElements.Add(areaElementModel);
                        }
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == groupMatchTest)
                {
                    List<int> ParseIds(string input)
                    {
                        var ids = new List<int>();
                        if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                        {
                            return ids;
                        }

                        var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var part in parts)
                        {
                            if (part.Contains("to"))
                            {
                                string[] rangeParts = part.Split(new[] { "to", "by" }, StringSplitOptions.RemoveEmptyEntries);

                                if (rangeParts.Length >= 2)
                                {
                                    int start = int.Parse(rangeParts[0]);
                                    int end = int.Parse(rangeParts[1]);
                                    int step = rangeParts.Length == 3 ? int.Parse(rangeParts[2]) : 1;

                                    for (int j = start; j <= end; j += step)
                                    {
                                        ids.Add(j);
                                    }
                                }
                            }
                            else
                            {
                                // Handle single numbers
                                int number = -1;
                                if (int.TryParse(part, out number))
                                {
                                    ids.Add(number);
                                }
                            }
                        }

                        return ids;
                    }
                    string NormalizeInput(string input)
                    {
                        // Rimuove i backslash e concatena le righe
                        return input.Replace("\\\n", " ").Replace("\\", "").Replace("\n", " ");
                    }
                    /*
                    List<int> ParseRange(string input)
                    {
                        var result = new List<int>();
                        var rangePattern = new Regex(@"(\d+)(to(\d+)(by(\d+))?)?");
                        var matches = rangePattern.Matches(input);

                        foreach (Match match in matches)
                        {
                            int start = int.Parse(match.Groups[1].Value);
                            int end = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : start;
                            int step = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : 1;

                            for (int j = start; j <= end; j += step)
                            {
                                result.Add(j);
                            }
                        }

                        return result;
                    }
                    void SplitNodesAndElements(string input, out string nodes, out string elements)
                    {
                        // Separate nodes and elements by detecting patterns
                        var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        nodes = string.Join(" ", parts.Where(p => p.Contains("to") || p.Contains("by")));
                        elements = string.Join(" ", parts.Where(p => !p.Contains("to") && !p.Contains("by")));
                    }
                    */
                    string currentString = "";
                    for (int j = i + 2; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }
                        else
                            currentString += textMgt[j];
                    }

                    // Split input into group sections
                    Regex groupPattern = new Regex(@"\s*(?<name>[^,]+),\s*(?<nodes>[^,]+),\s*(?<elements>[^,]+),\s*(?<plane>[^,])");
                    MatchCollection matches = groupPattern.Matches(currentString);

                    foreach (Match match in matches)
                    {
                        // Extract group name
                        var grp = new ElementGroupModel(match.Groups["name"].Value.Trim());
                        Groups.Add(grp);

                        // Parse nodes
                        var nodesBuffer = match.Groups["nodes"].Value.Replace("\\", "").Trim();
                        var elementsBuffer = match.Groups["elements"].Value.Replace("\\", "").Trim();

                        var nodeList = ParseIds(NormalizeInput(nodesBuffer));
                        for (int k = 0; k < nodeList.Count; k++)
                            NodeElements[nodeList[k]].Groups.Add(grp);
                        var elemList = ParseIds(NormalizeInput(elementsBuffer));
                        for (int k = 0; k < elemList.Count; k++)
                        {
                            if (FrameElements.ContainsKey(elemList[k]))
                                FrameElements[elemList[k]].Groups.Add(grp);
                            if (AreaElements.ContainsKey(elemList[k]))
                                AreaElements[elemList[k]].Groups.Add(grp);
                        }
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == elasticLinkMatchTest)
                {
                    for (int j = i + 6; j < textMgt.Length; j++)
                    {
                        if (string.IsNullOrEmpty(textMgt[j]))
                        {
                            i = j;
                            break;
                        }

                        var splitlist = textMgt[j].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        var elemType = splitlist[3].Trim();

                        if (elemType == "RIGID")
                        {
                            LinkElementModel frameElementModel = new LinkElementModel()
                            {
                                Id = int.Parse(splitlist[0].Trim()),
                                NodeStart = NodeElements[int.Parse(splitlist[1].Trim())],
                                NodeEnd = NodeElements[int.Parse(splitlist[2].Trim())],
                                LinkProperty = new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.RIGID) { BoundaryGroup = new BoundaryGroupModel(splitlist[8].Trim()) },
                            };

                            LinkElements.Add(frameElementModel);
                        }

                        if (elemType == "GEN")
                        {
                            LinkElementModel frameElementModel = new LinkElementModel()
                            {
                                Id = int.Parse(splitlist[0].Trim()),
                                NodeStart = NodeElements[int.Parse(splitlist[1].Trim())],
                                NodeEnd = NodeElements[int.Parse(splitlist[2].Trim())],
                                LinkProperty = new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.GEN)
                                {
                                    Kx = double.Parse(splitlist[11].Trim()),
                                    Ky = double.Parse(splitlist[12].Trim()),
                                    Kz = double.Parse(splitlist[13].Trim()),
                                    Rx = double.Parse(splitlist[14].Trim()),
                                    Ry = double.Parse(splitlist[15].Trim()),
                                    Rz = double.Parse(splitlist[16].Trim()),
                                    BoundaryGroup = new BoundaryGroupModel(splitlist[8].Trim())
                                },
                            };

                            LinkElements.Add(frameElementModel);
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ModelModel);
        }

        public bool Equals(ModelModel other)
        {
            return !(other is null) &&
                   EqualityComparer<ModelUnitsModel>.Default.Equals(ModelUnits, other.ModelUnits) &&
                   EqualityComparer<UniqueIdCollection<MaterialModel>>.Default.Equals(Materials, other.Materials) &&
                   EqualityComparer<UniqueNameCollection<LoadCaseModel>>.Default.Equals(LoadCases, other.LoadCases) &&
                   EqualityComparer<UniqueNameCollection<LoadCombinationModel>>.Default.Equals(LoadCombinations, other.LoadCombinations) &&
                   EqualityComparer<UniqueNameCollection<ElementGroupModel>>.Default.Equals(Groups, other.Groups) &&
                   EqualityComparer<UniqueIdCollection<FramePropertyModel>>.Default.Equals(FrameProperties, other.FrameProperties) &&
                   EqualityComparer<UniqueIdCollection<AreaThicknessModel>>.Default.Equals(AreaThicknesses, other.AreaThicknesses) &&
                   EqualityComparer<UniqueIdCollection<NodeElementModel>>.Default.Equals(NodeElements, other.NodeElements) &&
                   EqualityComparer<UniqueIdCollection<FrameElementModel>>.Default.Equals(FrameElements, other.FrameElements) &&
                   EqualityComparer<UniqueIdCollection<AreaElementModel>>.Default.Equals(AreaElements, other.AreaElements);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + EqualityComparer<ModelUnitsModel>.Default.GetHashCode(ModelUnits);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<MaterialModel>>.Default.GetHashCode(Materials);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<LoadCaseModel>>.Default.GetHashCode(LoadCases);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<LoadCombinationModel>>.Default.GetHashCode(LoadCombinations);
            hashCode = hashCode * -17 + EqualityComparer<UniqueNameCollection<ElementGroupModel>>.Default.GetHashCode(Groups);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<FramePropertyModel>>.Default.GetHashCode(FrameProperties);
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<AreaThicknessModel>>.Default.GetHashCode(AreaThicknesses);
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
