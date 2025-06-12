using Rhino.Geometry;
using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Base;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Core.Collections;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Elements;
using Rhino2Fem.Core.Helper;
using Rhino2Fem.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rhino2Fem.Core.Models
{
    public class ModelModel : IEquatable<ModelModel>
    {
        public ModelUnitsModel ModelUnits { get; set; }
        public UniqueIdCollection<MaterialModel> Materials { get; }
        public UniqueNameCollection<LoadCaseModel> LoadCases { get; }
        public UniqueNameCollection<LoadCombinationModel> LoadCombinations { get; }
        public UniqueNameCollection<ElementGroupModel> Groups { get; }
        public UniqueIdCollection<FrameSectionModel> FrameSections { get; }
        public UniqueIdCollection<FramePropertyModel> FrameProperties { get; }
        public UniqueIdCollection<AreaThicknessModel> AreaThicknesses { get; }
        public UniqueIdCollection<AreaPropertyModel> AreaProperties { get; }
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
            FrameSections = new UniqueIdCollection<FrameSectionModel>();
            FrameProperties = new UniqueIdCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueIdCollection<AreaThicknessModel>();
            AreaProperties = new UniqueIdCollection<AreaPropertyModel>();
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
            FrameSections = new UniqueIdCollection<FrameSectionModel>();
            FrameProperties = new UniqueIdCollection<FramePropertyModel>();
            AreaThicknesses = new UniqueIdCollection<AreaThicknessModel>();
            AreaProperties = new UniqueIdCollection<AreaPropertyModel>();
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
            for (int i = 0; i < model.FrameSections.Count; i++)
                FrameSections.Add(new FrameSectionModel(model.FrameSections.Values.ElementAt(i)));
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
            int idFrameSection = 1;
            int idFrameProperty = 1;
            int idAreaThicknes = 1;
            int idAreaProperty = 1;
            List<int> usedMaterialIds = frames.Select(x => x.Material.Id).Union(areas.Select(i => i.Material.Id)).Distinct().ToList();
            List<int> usedFrameSectionIds = frames.Select(x => x.FrameSection.Id).Distinct().ToList();
            List<int> usedFramePropertyIds = frames.Select(x => x.FrameProperty.Id).Distinct().ToList();
            List<int> usedAreaSectionIds = areas.Select(x => x.AreaThickness.Id).Distinct().ToList();
            List<int> usedAreaPropertyIds = areas.Select(x => x.AreaProperty.Id).Distinct().ToList();

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
                var fp = new FrameSectionModel(framesBuffer[i].FrameSection);

                bool exist = false;
                for (int j = 0; j < FrameSections.Count; j++)
                {
                    if (framesBuffer[i].FrameSection == FrameSections.ElementAt(j).Value)
                    {
                        exist = true;
                        framesBuffer[i].FrameSection = FrameSections.ElementAt(j).Value;
                    }
                }
                if (!exist)
                {
                    if (fp.Id == ModelObjectId.UNASSIGNED)
                        fp.Id = NewId(usedFrameSectionIds, idFrameSection);
                    usedFrameSectionIds.Add(fp.Id);
                    FrameSections.Add(fp);
                    framesBuffer[i].FrameSection = FrameSections[fp.Id];
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
                        fp.Id = NewId(usedAreaSectionIds, idAreaThicknes);
                    usedAreaSectionIds.Add(fp.Id);
                    AreaThicknesses.Add(fp);
                    areasBuffer[i].AreaThickness = AreaThicknesses[fp.Id];
                }
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var fp = new AreaPropertyModel(areasBuffer[i].AreaProperty);

                bool exist = false;
                for (int j = 0; j < AreaProperties.Count; j++)
                {
                    if (areasBuffer[i].AreaProperty == AreaProperties.ElementAt(j).Value)
                    {
                        exist = true;
                        areasBuffer[i].AreaProperty = AreaProperties.ElementAt(j).Value;
                    }
                }
                if (!exist)
                {
                    if (fp.Id == ModelObjectId.UNASSIGNED)
                        fp.Id = NewId(usedAreaPropertyIds, idAreaProperty);
                    usedAreaSectionIds.Add(fp.Id);
                    AreaProperties.Add(fp);
                    areasBuffer[i].AreaProperty = AreaProperties[fp.Id];
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
            HelperMidasExport helperMidasExport = new HelperMidasExport(this);
            return helperMidasExport.CreateMgtFile();
        }

        public static ModelModel CreateFromMgtFile(IEnumerable<string> strings)
        {
            HelperMidasExport helperMidasExport = new HelperMidasExport(new ModelModel());
            helperMidasExport.ReadMgtFile(strings.ToArray());
            return helperMidasExport.Model;
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

        public bool CreateModelStraus7R3(string outputPath, out int modelId, out List<string> warnings, out List<string> errors)
        {
            HelperStrausExport helperStrausExport = new HelperStrausExport(this);
            if (helperStrausExport.CreateModelR3(outputPath, out modelId, out warnings, out errors))
                return true;
            return false;
        }

        #region Overrides and Operators

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
                   EqualityComparer<UniqueIdCollection<FrameSectionModel>>.Default.Equals(FrameSections, other.FrameSections) &&
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
            hashCode = hashCode * -17 + EqualityComparer<UniqueIdCollection<FrameSectionModel>>.Default.GetHashCode(FrameSections);
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

        #endregion
    }
}
