using Rhino2Fem.Core.Attributes;
using Rhino2Fem.Core.Cases;
using Rhino2Fem.Core.ElementProperties;
using Rhino2Fem.Core.Elements;
using Rhino2Fem.Core.Loads;
using Rhino2Fem.Core.Models;
using Rhino2Fem.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rhino2Fem.Core.Helper
{
    public class HelperMidasExport
    {
        public ModelModel Model { get; set; }

        public HelperMidasExport(ModelModel model)
        {
            Model = model;
        }

        #region MGT/MCT File Writing Methods

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

                        Model.ModelUnits = modelUnits;
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

                        Model.Materials.Add(materialModel);
                    }
                }
            }

            for (int i = 0; i < textMgt.Length; i++)
            {
                string row = textMgt[i];

                if (row == sectionMatchTest)
                {
                    Dictionary<string, FrameSectionModel.SectionGeometryTypes> kvpType = new Dictionary<string, FrameSectionModel.SectionGeometryTypes>()
                    {
                        {"SB", FrameSectionModel.SectionGeometryTypes.SB},
                        {"SR",  FrameSectionModel.SectionGeometryTypes.SR},
                        {"P",  FrameSectionModel.SectionGeometryTypes.P},
                        {"L",  FrameSectionModel.SectionGeometryTypes.L},
                        {"C",  FrameSectionModel.SectionGeometryTypes.C},
                        {"H",  FrameSectionModel.SectionGeometryTypes.H},
                        {"T",  FrameSectionModel.SectionGeometryTypes.T},
                        {"B",  FrameSectionModel.SectionGeometryTypes.B},
                    };

                    Dictionary<string, FrameSectionModel.OffsetTypes> kvpOffset = new Dictionary<string, FrameSectionModel.OffsetTypes>()
                    {
                        {"LT", FrameSectionModel.OffsetTypes.LT},
                        {"CT", FrameSectionModel.OffsetTypes.CT},
                        {"RT", FrameSectionModel.OffsetTypes.RT},
                        {"LC", FrameSectionModel.OffsetTypes.LC},
                        {"CC", FrameSectionModel.OffsetTypes.CC},
                        {"RC", FrameSectionModel.OffsetTypes.RC},
                        {"LB", FrameSectionModel.OffsetTypes.LB},
                        {"CB", FrameSectionModel.OffsetTypes.CB},
                        {"RB", FrameSectionModel.OffsetTypes.RB},
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

                        FrameSectionModel sectionModel = new FrameSectionModel()
                        {
                            Id = int.Parse(splitlist[0].Trim()),
                            SectionGeometryType = kvpType[splitlist[12].Trim()],
                            Name = splitlist[2].Trim(),
                            Offset = kvpOffset[splitlist[3].Trim()],
                        };

                        Model.FrameSections.Add(sectionModel);
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
                            ThicknessMembrane = double.Parse(splitlist[4].Trim()),
                            Offset = double.Parse(splitlist[8].Trim()),
                        };

                        Model.AreaThicknesses.Add(areaThicknessModel);
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

                        Model.NodeElements.Add(nodeElementModel);
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
                                Material = Model.Materials[int.Parse(splitlist[2].Trim())],
                                FrameSection = Model.FrameSections[int.Parse(splitlist[3].Trim())],
                                NodeStart = Model.NodeElements[int.Parse(splitlist[4].Trim())],
                                NodeEnd = Model.NodeElements[int.Parse(splitlist[5].Trim())],
                                Angle = double.Parse(splitlist[6].Trim()),
                            };
                            Model.FrameElements.Add(frameElementModel);
                        }

                        if (elemType == "PLATE")
                        {
                            List<NodeElementModel> ns = new List<NodeElementModel>
                            {
                                Model.NodeElements[int.Parse(splitlist[4].Trim())],
                                Model.NodeElements[int.Parse(splitlist[5].Trim())],
                                Model.NodeElements[int.Parse(splitlist[6].Trim())]
                            };
                            if (splitlist[7].Trim() != "0")
                                ns.Add(Model.NodeElements[int.Parse(splitlist[7].Trim())]);

                            AreaElementModel areaElementModel = new AreaElementModel(ns, Model.AreaThicknesses[int.Parse(splitlist[3].Trim())], Model.Materials[int.Parse(splitlist[2].Trim())],
                                double.Parse(splitlist[6].Trim()));
                            areaElementModel.Id = int.Parse(splitlist[0].Trim());

                            Model.AreaElements.Add(areaElementModel);
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
                        Model.Groups.Add(grp);

                        // Parse nodes
                        var nodesBuffer = match.Groups["nodes"].Value.Replace("\\", "").Trim();
                        var elementsBuffer = match.Groups["elements"].Value.Replace("\\", "").Trim();

                        var nodeList = ParseIds(NormalizeInput(nodesBuffer));
                        for (int k = 0; k < nodeList.Count; k++)
                            Model.NodeElements[nodeList[k]].Groups.Add(grp);
                        var elemList = ParseIds(NormalizeInput(elementsBuffer));
                        for (int k = 0; k < elemList.Count; k++)
                        {
                            if (Model.FrameElements.ContainsKey(elemList[k]))
                                Model.FrameElements[elemList[k]].Groups.Add(grp);
                            if (Model.AreaElements.ContainsKey(elemList[k]))
                                Model.AreaElements[elemList[k]].Groups.Add(grp);
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
                                NodeStart = Model.NodeElements[int.Parse(splitlist[1].Trim())],
                                NodeEnd = Model.NodeElements[int.Parse(splitlist[2].Trim())],
                                LinkProperty = new LinkPropertyModel(LinkPropertyModel.LinkPropertyTypes.RIGID) { BoundaryGroup = new BoundaryGroupModel(splitlist[8].Trim()) },
                            };

                            Model.LinkElements.Add(frameElementModel);
                        }

                        if (elemType == "GEN")
                        {
                            LinkElementModel frameElementModel = new LinkElementModel()
                            {
                                Id = int.Parse(splitlist[0].Trim()),
                                NodeStart = Model.NodeElements[int.Parse(splitlist[1].Trim())],
                                NodeEnd = Model.NodeElements[int.Parse(splitlist[2].Trim())],
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

                            Model.LinkElements.Add(frameElementModel);
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods for MGT File Writing

        private void WriteMgtHeader(List<string> textMgt)
        {
            textMgt.Add("; This MGT file is generated using Rhino2Midas plug in for grasshopper");
            textMgt.Add("; Created by Gabriele Pacini, contact: gabrielepacini6293@gmail.com");
            textMgt.Add("; Disclamer: please check if MIDAS output is as intended!");
        }

        private void WriteMgtUnit(List<string> textMgt)
        {
            textMgt.Add("*UNIT");
            textMgt.Add(Model.ModelUnits.ForceUnit + ", " + Model.ModelUnits.LengthUnit + ", " + Model.ModelUnits.HeatUnit + ", " + Model.ModelUnits.TemperatureUnit);
        }

        private void WriteMgtMaterial(List<string> textMgt)
        {
            if (Model.Materials.Count > 0)
            {
                textMgt.Add("*MATERIAL");
                textMgt.Add("; iMAT, TYPE, MNAME, SPHEAT, HEATCO, PLAST, TUNIT, bMASS, DAMPRATIO, [DATA1]           ; STEEL, CONC, USER \n\r" +
                    "; iMAT, TYPE, MNAME, SPHEAT, HEATCO, PLAST, TUNIT, bMASS, DAMPRATIO, [DATA2], [DATA2]                      ; SRC\n\r" +
                    "; [DATA1] : 1, STANDARD, CODE/PRODUCT, DB, USEELAST, ELAST\n\r" +
                    "; [DATA1] : 2, ELAST, POISN, THERMAL, DEN, MASS\n\r" +
                    "; [DATA1] : 3, Ex, Ey, Ez, Tx, Ty, Tz, Sxy, Sxz, Syz, Pxy, Pxz, Pyz, DEN, MASS         ; Orthotropic\n\r" +
                    "; [DATA2] : 1, STANDARD, CODE/PRODUCT, DB, USEELAST, ELAST or 2, ELAST, POISN, THERMAL, DEN, MASS");
            }
            foreach (KeyValuePair<int, MaterialModel> kvp in Model.Materials)
            {
                MaterialModel material = kvp.Value;

                string matName = material.Name;
                if (matName.Length > 16)
                    matName = matName.Substring(0, 28);
                if (material.Standard == MaterialModel.Standards.Custom)
                {
                    textMgt.Add($"{material.Id}, {material.Type.GetDescription()}, {matName}, 0, 0, , C, NO, {material.DampingRatio}, 2, {material.ModulusElasticity}," +
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
            if (Model.NodeElements.Count > 0)
            {
                textMgt.Add("*NODE");
                textMgt.Add("; i, (X), (Y), (Z)");
            }
            foreach (var kvp in Model.NodeElements)
            {
                NodeElementModel node = kvp.Value;
                textMgt.Add($"{node.Id}, {node.X}, {node.Y}, {node.Z}");
            }
        }

        private void WriteMgtFrameProperty(List<string> textMgt)
        {
            if (Model.FrameSections.Count > 0)
            {
                textMgt.Add("*SECTION");
                textMgt.Add("; i, (section type), (name), (offset), (iCENT=0), (iREF=0), (iHORZ=0), (huser=0), (iVERT=0), (vuser=0), (consider shear deformation = \"YES\"), (consider warping effect = \"NO\"), (shape type), 2, (dimension 1 to 10)");
            }
            foreach (var kvp in Model.FrameSections)
            {
                FrameSectionModel frameProperty = kvp.Value;
                string framePropertyName = frameProperty.Name;
                if (framePropertyName.Length > 28)
                    framePropertyName = framePropertyName.Substring(0, 28);

                if (frameProperty.Type == FrameSectionModel.Types.DBUSER)
                    textMgt.Add($"{frameProperty.Id}, {frameProperty.Type}, {framePropertyName}, {frameProperty.Offset.ToString()}, 0, 0, 0, 0, 0, 0, YES, NO, {frameProperty.SectionGeometryType.ToString()}, 2, " +
                        $"{frameProperty.Dimension1}, {frameProperty.Dimension2}, {frameProperty.Dimension3}, {frameProperty.Dimension4}, {frameProperty.Dimension5}, " +
                        $"{frameProperty.Dimension6}, {frameProperty.Dimension7}, {frameProperty.Dimension8}, {frameProperty.Dimension9}, {frameProperty.Dimension10}");
                else if (frameProperty.Type == FrameSectionModel.Types.COMPOSITE_I)
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
            if (Model.AreaThicknesses.Count > 0)
            {
                textMgt.Add("*THICKNESS");
                textMgt.Add("; i, (section type=VALUE), (name), (same thickness all around=YES), (thickness in plane), (thickness out of plane=0), (offset=NO), (offtype=0), (value=0)");
            }
            foreach (var kvp in Model.AreaThicknesses)
            {
                AreaThicknessModel areaThickness = kvp.Value;
                string name = areaThickness.Name;
                if (name.Length > 28)
                    name = name.Substring(0, 28);

                string offsett = areaThickness.Offset != 0 ? "YES" : "NO";
                string offsettype = areaThickness.Offset != 0 ? "1" : "0";

                textMgt.Add($"{areaThickness.Id}, VALUE, {name}, YES, {areaThickness.ThicknessMembrane}, 0, {offsett}, {offsettype}, {areaThickness.Offset}");
            }
        }

        private void WriteMgtFrameElement(List<string> textMgt)
        {
            if (Model.FrameElements.Count > 0)
            {
                textMgt.Add("*ELEMENT");
                textMgt.Add("; i, BEAM, (index material), (index property), (index node start), (index node end), (angle=0), (index subtype=0)");
            }
            foreach (var kvp in Model.FrameElements)
            {
                FrameElementModel frameElement = kvp.Value;
                textMgt.Add($"{frameElement.Id}, BEAM, {frameElement.Material.Id}, {frameElement.FrameSection.Id}, {frameElement.NodeStart.Id}, {frameElement.NodeEnd.Id}, {Rhino.RhinoMath.ToDegrees(frameElement.Angle)} , 0");
            }
        }

        private void WriteMgtAreaElement(List<string> textMgt)
        {
            if (Model.AreaElements.Count > 0)
            {
                textMgt.Add("*ELEMENT");
                textMgt.Add("; i, PLATE, (index material), (index property), (index joint 1), (index joint 2), (index joint 3), (index joint 4), (subtype thick=1 thin=2), (local axis)");
            }
            foreach (var kvp in Model.AreaElements)
            {
                AreaElementModel areaElement = kvp.Value;
                int id4 = areaElement.NodeList.Count == 4 ? areaElement.NodeList[3].Id : 0;
                textMgt.Add($"{areaElement.Id}, PLATE, {areaElement.Material.Id}, {areaElement.AreaThickness.Id}, {areaElement.NodeList[0].Id}, {areaElement.NodeList[1].Id}, " +
                    $"{areaElement.NodeList[2].Id}, {id4}, 1, {areaElement.Angle}");
            }
        }

        private void WriteMgtLinkElement(List<string> textMgt)
        {
            if (Model.LinkElements.Count > 0)
            {
                textMgt.Add("*ELASTICLINK; Elastic Link");
                textMgt.Add("; iNO, iNODE1, iNODE2, LINK, ANGLE, R_SDx, R_SDy, R_SDz, R_SRx, R_SRy, R_SRz, SDx, SDy, SDz, SRx, SRy, SRz... ");
                textMgt.Add("; bSHEAR, DRy, DRz, GROUP; GEN");
                textMgt.Add("                ; iNO, iNODE1, iNODE2, LINK, ANGLE, bSHEAR, DRy, DRz, GROUP; RIGID,SADDLE");
                textMgt.Add("               ; iNO, iNODE1, iNODE2, LINK, ANGLE, SDx, bSHEAR, DRy, DRz, GROUP; TENS,COMP");
                textMgt.Add("; iNO, iNODE1, iNODE2, LINK, ANGLE, DIR, FUNCTION, bSHEAR, DRENDI, GROUP; MULTI LINEAR");
            }
            foreach (var kvp in Model.LinkElements)
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
            if (Model.LoadCases.Count > 0)
            {
                textMgt.Add("*STLDCASE");
                textMgt.Add("; (name), (load type), (desc)");
            }
            foreach (var kvp in Model.LoadCases)
            {
                var loadCase = kvp.Value;
                textMgt.Add($"{loadCase.Name}, {loadCase.Type.ToString()}, {loadCase.Description}");
            }
        }

        private void WriteMgtLoadGroup(List<string> textMgt)
        {
            HashSet<string> loadGroups = new HashSet<string>();
            foreach (var node in Model.NodeElements)
            {
                for (int j = 0; j < node.Value.NodalLoadList.Count; j++)
                {
                    NodalLoadModel load = node.Value.NodalLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            foreach (var frame in Model.FrameElements)
            {
                for (int j = 0; j < frame.Value.FrameLoadList.Count; j++)
                {
                    FrameLoadModel load = frame.Value.FrameLoadList[j];
                    if (load.LoadGroup != null && !string.IsNullOrEmpty(load.LoadGroup.Name))
                        loadGroups.Add(load.LoadGroup.Name);
                }
            }
            foreach (var area in Model.AreaElements)
            {
                for (int j = 0; j < area.Value.AreaLoadList.Count; j++)
                {
                    AreaLoadMidasModel load = area.Value.AreaLoadList[j];
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
            foreach (var node in Model.NodeElements)
            {
                if (node.Value.Support != null && node.Value.Support.BoundaryGroup != null)
                    boundaryGroups.Add(node.Value.Support.BoundaryGroup.Name);

            }
            foreach (var link in Model.LinkElements)
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
            foreach (var kvp in Model.FrameElements)
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
            foreach (var node in Model.NodeElements)
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
            foreach (var kvp in Model.AreaElements)
            {
                AreaElementModel areaElement = kvp.Value;
                List<AreaLoadMidasModel> areaLoadList = areaElement.AreaLoadList;
                for (int i = 0; i < areaLoadList.Count; i++)
                {
                    AreaLoadMidasModel item = areaLoadList[i];
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
            if (Model.LoadCombinations.Count > 0)
            {
                textMgt.Add("*LOADCOMB    ; Combinations");
                textMgt.Add("; (NAME=name), (kind=GEN), (active=ACTIVE), (bES=0), (linear add=0, envelope=1), (desc=''), (iSERVE-TYPE=0), (nLCOMTYPE=0), (nSEISTYPE=0)");
                textMgt.Add("; (load type=ST), (LCNAME1), (FACTOR)");
            }
            foreach (var kvp in Model.LoadCombinations)
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
            if (Model.Groups.Count > 0)
            {
                textMgt.Add("*GROUP    ; Group");
                textMgt.Add("; (group name), (node list), (element list), (plane type = 0)");
            }
            foreach (var kvp in Model.Groups)
            {
                var group = kvp.Value;
                string groupName = group.Name;

                string nodeList = "";
                string elementList = "";
                for (int i = 0; i < Model.NodeElements.Count; i++)
                {
                    NodeElementModel node = Model.NodeElements.ElementAt(i).Value;
                    for (int j = 0; j < node.Groups.Count; j++)
                    {
                        if (node.Groups[j].Name == groupName)
                            nodeList += $"{node.Id} ";
                    }
                }
                for (int i = 0; i < Model.FrameElements.Count; i++)
                {
                    FrameElementModel elem = Model.FrameElements.ElementAt(i).Value;
                    for (int j = 0; j < elem.Groups.Count; j++)
                    {
                        if (elem.Groups[j].Name == groupName)
                            elementList += $"{elem.Id} ";
                    }
                }
                for (int i = 0; i < Model.AreaElements.Count; i++)
                {
                    AreaElementModel elem = Model.AreaElements.ElementAt(i).Value;
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
            foreach (var kvp in Model.NodeElements)
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
            if (Model.SelfWeight != null)
            {
                textMgt.Add("*USE-STLD, " + Model.SelfWeight.LoadCase.Name);
                textMgt.Add("*SELFWEIGHT");
                textMgt.Add($"{Model.SelfWeight.FactorX}, {Model.SelfWeight.FactorY}, {Model.SelfWeight.FactorZ}");
            }
        }

        #endregion
    }
}
