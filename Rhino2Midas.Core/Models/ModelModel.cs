using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Rhino.Geometry;
using Rhino2Midas.Core.Attributes;
using Rhino2Midas.Core.Base;
using Rhino2Midas.Core.Cases;
using Rhino2Midas.Core.Collections;
using Rhino2Midas.Core.ElementProperties;
using Rhino2Midas.Core.Elements;
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

        public void BuildModel(List<NodeElementModel> nodes, List<FrameElementModel> frames, List<AreaElementModel> areas, List<LoadCombinationModel> combos)
        {
            #region Check Element IDs

            Dictionary<int, NodeElementModel> nodeDictionary = new Dictionary<int, NodeElementModel>();
            Dictionary<int, FrameElementModel> frameDictionary = new Dictionary<int, FrameElementModel>();
            Dictionary<int, AreaElementModel> areaDictionary = new Dictionary<int, AreaElementModel>();

            int idNode = 1;
            List<int> usedNodeIds = nodes.Select(x => x.Id).ToList();
            List<int> usedFrameIds = frames.Select(x => x.Id).ToList();
            List<int> usedAreaIds = areas.Select(x => x.Id).ToList();
            
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodeDictionary.ContainsKey(nodes[i].Id) && nodes[i].Id != ElementModel.UNASSIGNED)
                {
                    nodeDictionary[nodes[i].Id].Merge(nodes[i]);
                }
                else
                {
                    NodeElementModel nn = new NodeElementModel(nodes[i]);
                    idNode = NewId(usedNodeIds, idNode);

                    nn.Id = idNode;
                    nodeDictionary.Add(nn.Id, nn);
                    usedNodeIds.Add(idNode);
                    idNode++;
                }
            }

            int idFrame = 1;
            for (int i = 0; i < frames.Count; i++)
            {
                if (frameDictionary.ContainsKey(frames[i].Id) && frames[i].Id != ElementModel.UNASSIGNED)
                {
                    frameDictionary[frames[i].Id].Merge(frames[i]);
                }
                else
                {
                    FrameElementModel nn = new FrameElementModel(frames[i]);
                    idFrame = NewId(usedFrameIds, idFrame);

                    nn.Id = idFrame;
                    frameDictionary.Add(nn.Id, nn);
                    usedFrameIds.Add(idFrame);
                    idFrame++;
                }
            }

            int idArea = 1;
            for (int i = 0; i < areas.Count; i++)
            {
                if (areaDictionary.ContainsKey(areas[i].Id) && areas[i].Id != ElementModel.UNASSIGNED)
                {
                    areaDictionary[areas[i].Id].Merge(areas[i]);
                }
                else
                {
                    AreaElementModel nn = new AreaElementModel(areas[i]);
                    idArea = NewId(usedAreaIds, idArea);

                    nn.Id = idArea;
                    areaDictionary.Add(nn.Id, nn);
                    usedAreaIds.Add(idArea);
                    idArea++;
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

            for (int i = 0; i < frames.Count; i++)
            {
                {
                    Point3d pt = frames[i].NodeStart.Position;
                    bool found = false;
                    int nodeId = ElementModel.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        frames[i].NodeStart = nodeDictionary[nodeId];
                    }
                    else
                    {
                        NodeElementModel newNode = new NodeElementModel(pt);
                        idNode = NewId(usedNodeIds, idNode);

                        nodeDictionary.Add(nodeId, newNode);
                        nodesBuffer.Add(newNode);
                        rTree.Insert(pt, rTree.Count);
                    }
                }
                {
                    Point3d pt = frames[i].NodeEnd.Position;
                    bool found = false;
                    int nodeId = ElementModel.UNASSIGNED;
                    rTree.Search(new Sphere(pt, tol), new EventHandler<RTreeEventArgs>((sender, e) => { found = true; nodeId = e.Id; }));

                    if (found)
                    {
                        frames[i].NodeEnd = nodeDictionary[nodeId];
                    }
                    else
                    {
                        NodeElementModel newNode = new NodeElementModel(pt);
                        idNode = NewId(usedNodeIds, idNode);

                        nodeDictionary.Add(nodeId, newNode);
                        nodesBuffer.Add(newNode);
                        rTree.Insert(pt, rTree.Count);
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
                    if(element.Groups.Count > 0)
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
                if (mat.Id == ModelObjectId.UNASSIGNED)
                    mat.Id = NewId(usedMaterialIds, idMaterial);
                Materials.Add(mat);
            }
            
            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var mat = new MaterialModel(areasBuffer[i].Material);
                if (mat.Id == ModelObjectId.UNASSIGNED)
                    mat.Id = NewId(usedMaterialIds, idMaterial);
                Materials.Add(mat);
            }

            for (int i = 0; i < framesBuffer.Count; i++)
            {
                var fp = new FramePropertyModel(framesBuffer[i].FrameProperty);
                if (fp.Id == ModelObjectId.UNASSIGNED)
                    fp.Id = NewId(usedFramePropertyIds, idFrameProperty);
                FrameProperties.Add(fp);            
            }

            for (int i = 0; i < areasBuffer.Count; i++)
            {
                var fp = new AreaThicknessModel(areasBuffer[i].AreaThickness);
                if (fp.Id == ModelObjectId.UNASSIGNED)
                    fp.Id = NewId(usedAreaPropertyIds, idAreaProperty);
                AreaThicknesses.Add(fp);
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
                            var newCase = new LoadCaseModel(element.NodalLoadList[j].LoadCase);
                            newCase.Id = idCases;
                            idCases++;
                            LoadCases.Add(newCase);
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
                            var newCase = new LoadCaseModel(element.FrameLoadList[j].LoadCase);
                            newCase.Id = idCases;
                            idCases++;
                            LoadCases.Add(newCase);
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
                            var newCase = new LoadCaseModel(element.AreaLoadList[j].LoadCase);
                            newCase.Id = idCases;
                            idCases++;
                            LoadCases.Add(newCase);
                        }
                    }
                }
            }

            int idCombo = 1;
            for(int i = 0; i< combos.Count; i++)
            {
                var newCombo = new LoadCombinationModel(combos[i]);
                for(int j = 0; j < combos[i].LoadFactorList.Count; j++)
                {
                    var newCase = new LoadCaseModel(combos[i].LoadFactorList[j].LoadCase);
                    newCase.Id = idCases;
                    idCases++;
                    LoadCases.Add(newCase);
                    newCombo.LoadFactorList[j].LoadCase = newCase;
                }
                newCombo.Id = idCombo;
                LoadCombinations.Add(newCombo);
            }

            #endregion            
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
