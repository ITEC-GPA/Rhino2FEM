namespace Rhino2SAP.Grasshopper;

/// <summary>One topic map for the Grasshopper ribbon, generated API components and the catalogue.</summary>
public static class ComponentTopics
{
    public const string Model="01 Modello";
    public const string Materials="02 Materiali";
    public const string BeamSections="03 Sezioni beam";
    public const string SectionDesigner="04 Section Designer";
    public const string ShellSolidProperties="05 Piastre e solidi";
    public const string LinkCableProperties="06 Link e cavi";
    public const string Elements="07 Elementi";
    public const string Supports="08 Vincoli e molle";
    public const string Axes="09 Assi e offset";
    public const string Assignments="10 Mesh e assegnazioni";
    public const string Mass="11 Masse";
    public const string Patterns="12 Load pattern";
    public const string NodeLoads="13 Carichi nodali";
    public const string BeamLoads="14 Carichi beam";
    public const string PlateLoads="15 Carichi plate";
    public const string OtherLoads="16 Altri carichi";
    public const string Wind="17 Vento";
    public const string Seismic="18 Sisma";
    public const string StaticCases="19 Casi statici";
    public const string NonlinearCases="20 Casi non lineari";
    public const string Stages="21 Fasi costruttive";
    public const string ModalCases="22 Modale e spettri";
    public const string TimeHistory="23 Time history";
    public const string DynamicCases="24 Altri casi dinamici";
    public const string Combinations="25 Combinazioni";
    public const string AnalysisFiles="26 Analisi e file";
    public const string NodeResults="27 Risultati nodali";
    public const string BeamResults="28 Risultati beam";
    public const string PlateResults="29 Risultati plate";
    public const string SolidLinkResults="30 Risultati solidi-link";
    public const string ModalResults="31 Risultati modali";
    public const string ResultTools="32 Risultati e query";
    public const string PreviewBake="33 Preview e bake";
    public const string Utilities="34 Utility";

    public static IReadOnlyList<string> All { get; }=Array.AsReadOnly(new[]
    {
        Model,Materials,BeamSections,SectionDesigner,ShellSolidProperties,LinkCableProperties,Elements,
        Supports,Axes,Assignments,Mass,Patterns,NodeLoads,BeamLoads,PlateLoads,OtherLoads,Wind,Seismic,
        StaticCases,NonlinearCases,Stages,ModalCases,TimeHistory,DynamicCases,Combinations,AnalysisFiles,
        NodeResults,BeamResults,PlateResults,SolidLinkResults,ModalResults,ResultTools,PreviewBake,Utilities
    });

    public static string ForApi(string key)
    {
        int separator=key.LastIndexOf('.');
        string path=key[..separator],method=key[(separator+1)..];
        if(path=="Results")
        {
            if(method.StartsWith("Area"))return PlateResults;
            if(method.StartsWith("Frame"))return BeamResults;
            if(method.StartsWith("Solid")||method.StartsWith("Link")||method.StartsWith("PanelZone"))return SolidLinkResults;
            if(method.StartsWith("Modal")||method is "ModeShape" or "BucklingFactor")return ModalResults;
            if(method.StartsWith("Joint")||method.StartsWith("AssembledJointMass"))return NodeResults;
            return ResultTools;
        }
        if(path=="SourceMass"||key=="PropMaterial.SetMassSource")return Mass;
        if(path.StartsWith("PropMaterial"))return Materials;
        if(path=="PropFrame.SDShape"||key=="PropFrame.SetSDSection")return SectionDesigner;
        if(path=="PropFrame")return BeamSections;
        if(path is "PropArea" or "PropSolid")return ShellSolidProperties;
        if(path is "PropLink" or "PropCable" or "PropTendon")return LinkCableProperties;
        if(path.StartsWith("LoadPatterns.AutoWind"))return Wind;
        if(path.StartsWith("LoadPatterns.AutoSeismic"))return Seismic;
        if(path.StartsWith("LoadPatterns.AutoSeastate"))return OtherLoads;
        if(path=="LoadPatterns")return Patterns;
        if(path=="RespCombo")return Combinations;
        if(path.StartsWith("LoadCases."))
        {
            string family=path["LoadCases.".Length..];
            if(family=="StaticNonlinearStaged")return Stages;
            if(family.StartsWith("StaticNonlinear"))return NonlinearCases;
            if(family.StartsWith("StaticLinear")||family is "Buckling" or "HyperStatic")return StaticCases;
            if(family.StartsWith("Modal")||family=="ResponseSpectrum")return ModalCases;
            if(family.StartsWith("DirHist")||family.StartsWith("ModHist"))return TimeHistory;
            return DynamicCases;
        }
        if(method.StartsWith("SetLoad")&&method!="SetLoadedGroup")return path switch
        {
            "PointObj"=>NodeLoads,"FrameObj"=>BeamLoads,"AreaObj"=>PlateLoads,_=>OtherLoads
        };
        if(method.StartsWith("SetMass"))return Mass;
        if(path=="CoordSys"||method.Contains("LocalAxes")||method.Contains("Offset")||method.StartsWith("SetInsertionPoint")||method=="SetEndSkew")return Axes;
        if(path=="ConstraintDef"||method.StartsWith("SetSpring")||method is "SetConstraint" or "SetRestraint" or "SetReleases" or "SetPanelZone" or "SetTCLimits")return Supports;
        if(method.StartsWith("SetPatternBy"))return Patterns;
        if(path is "GroupDef" or "PointObj" or "FrameObj" or "AreaObj" or "SolidObj" or "LinkObj" or "CableObj" or "TendonObj")return Assignments;
        throw new ArgumentException("No Grasshopper topic defined for SAP method "+key);
    }

    public static string ForComponent(string name,string category)
    {
        if(All.Contains(category))return category;
        // Handwritten convenience components retain their GUIDs and constructor names.
        if(name.StartsWith("Align SAP "))return Axes;
        if(name is "Decompose SAP Beam" or "SAP Beam Results N V T M" or "SAP Model Frame Force Diagram")return BeamResults;
        if(name=="Decompose SAP Plate"||name.StartsWith("SAP Plate Results "))return PlateResults;
        return name switch
        {
            "SAP Load Pattern"=>Patterns,
            "SAP Linear Static Case" or "SAP Buckling Case"=>StaticCases,
            "SAP Nonlinear Static Case"=>NonlinearCases,
            "SAP Load Combination"=>Combinations,
            _=>category switch
            {
                "01-Materials"=>Materials,"03-Elements"=>Elements,"07-Model"=>Model,
                "08-Display" or "13-Preview"=>PreviewBake,"08-Tools"=>Utilities,
                "09-Export" or "10-Import" or "11-Analysis"=>AnalysisFiles,"12-Results"=>ResultTools,
                _=>throw new ArgumentException("No Grasshopper topic defined for component "+name)
            }
        };
    }
}
