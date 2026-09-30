using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2SAP.Core;
using Rhino2SAP.Api;
using Point = Rhino.Geometry.Point;

namespace Rhino2SAP.Grasshopper;

public sealed class BuildModelComponent:SafeComponent
{
    public BuildModelComponent():base("Build SAP Model","Model","Collect geometry, properties, loads and cases into one SAP Model.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definitions","D","SAP fragments or Models. Shared dependencies are collected once.",GH_ParamAccess.list);p.AddIntegerParameter("Units","U","SAP eUnits (6=kN_m_C, 9=N_mm_C). Coordinates are not converted.",GH_ParamAccess.item,6);p.AddNumberParameter("Tolerance","T","Node welding distance in Model length units.",GH_ParamAccess.item,1e-6);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Model","M","Unified SAP Model.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Validation errors.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var raw=NativeList<object>(da,0);int units=Item<int>(da,1);double tolerance=Item<double>(da,2);foreach(var m in raw.OfType<SapModel>())if(m.Units!=units)throw new ArgumentException("Input Model units differ. Explicitly convert before building.");var model=new SapModel(Fragment.Combine(raw.Select(FragmentOf)),units,tolerance);Output(da,0,model);da.SetDataList(1,ModelValidation.Errors(model));}
}
public sealed class MergeModelsComponent:SafeComponent
{
    public MergeModelsComponent():base("Merge SAP Models","Merge","Merge Models with identical units/tolerance. Conflicting names are rejected; input models remain unchanged.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Models","M","Models to merge.",GH_ParamAccess.list);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Merged Model. Previous results removed.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,SapModel.Merge(NativeList<SapModel>(da,0)));
}
public sealed class ValidateModelComponent:SafeComponent
{
    public ValidateModelComponent():base("Validate SAP Model","Validate","Check geometry, duplicate definitions and SDK arguments before export.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddBooleanParameter("Valid","V","Managed validation passed; does not certify structural stability.",GH_ParamAccess.item);p.AddTextParameter("Errors","E","Errors.",GH_ParamAccess.list);p.AddTextParameter("Fingerprint","H","SHA256 model fingerprint.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);var errors=ModelValidation.Errors(m);da.SetData(0,errors.Count==0);da.SetDataList(1,errors);da.SetData(2,m.Fingerprint);}
}
public sealed class DecomposeModelComponent:SafeComponent
{
    public DecomposeModelComponent():base("Decompose SAP Model","Decompose","Expose geometry, deferred assignments, units and stored results.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){foreach(var k in Enum.GetValues<ElementKind>())p.AddGenericParameter(k.ToString(),k.ToString(),k+" elements.",GH_ParamAccess.list);p.AddGenericParameter("Operations","O","Deferred SDK operations.",GH_ParamAccess.list);p.AddIntegerParameter("Units","U","SAP eUnits.",GH_ParamAccess.item);p.AddGenericParameter("Results","R","Native result tables.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);foreach(var k in Enum.GetValues<ElementKind>())da.SetDataList((int)k,m.Definition.Elements.Where(e=>e.Kind==k).Select(e=>new FragmentGoo(new Fragment(m.Definition.Operations,[e],m.NativeSource))));da.SetDataList(6,m.Definition.Operations.Select(o=>new FragmentGoo(new Fragment([o],nativeSource:m.NativeSource))));da.SetData(7,m.Units);da.SetDataList(8,m.Results?.Tables??[]);}
}
public sealed class PreviewModelComponent:SafeComponent
{
    private readonly List<(PreviewScene Scene,DisplayOptions? Options)> scenes=[];
    public PreviewModelComponent():base("Preview SAP Model","Preview","Preview geometry, sections, loads and attributes; output closed physical Breps for downstream geometry operations and solid bake. Display switches do not change Brep output.","13-Preview"){}
    public override bool IsPreviewCapable=>true;
    public override BoundingBox ClippingBox{get{var bounds=BoundingBox.Empty;foreach(var entry in scenes){entry.Scene.Prepare(Options(entry.Options));bounds.Union(entry.Scene.Bounds);}return bounds;}}
    private static DisplayOptions Options(DisplayOptions? local){var o=local??DisplayPreferences.Current;return o with{Enabled=o.Enabled&&DisplayPreferences.Current.Enabled};}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Model, elements or section definition to preview.",GH_ParamAccess.item);p.AddGenericParameter("Settings","S","Optional SAP Display Settings. Empty = global Rhino2SAP preferences.",GH_ParamAccess.item);p[1].Optional=true;}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Status","S","Preview summary.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Assignments shown symbolically and elements omitted from solid Breps.",GH_ParamAccess.list);p.AddBrepParameter("Breps","B","Valid closed physical Breps: beam profiles, shell thicknesses and solid elements. Nodes and symbols are excluded. Independent of preview visibility.",GH_ParamAccess.list);p.AddTextParameter("Brep Names","N","SAP object names matching the Breps output, in the same order.",GH_ParamAccess.list);}
    protected override void BeforeSolveInstance(){scenes.Clear();base.BeforeSolveInstance();}
    protected override void Solve(IGH_DataAccess da)
    {
        var input=Unwrap(Item<object>(da,0));var fragment=FragmentOf(input);object raw=null!;DisplayOptions? settings=null;
        if(da.GetData(1,ref raw))settings=Unwrap(raw) as DisplayOptions??throw new ArgumentException("Use SAP Display Settings.");
        var scene=input is SapModel m?PreviewScene.For(m):PreviewScene.For(fragment);scene.Prepare(Options(settings));scenes.Add((scene,settings));
        var solids=scene.CopySolids(out var solidIssues);
        da.SetData(0,$"{fragment.Elements.Count} elements / {fragment.Operations.Count} definitions / {solids.Count} solid Breps; preview {(Options(settings).Enabled?"on":"off")}");
        da.SetDataList(1,scene.Issues.Concat(solidIssues).Distinct());da.SetDataList(2,solids.Select(s=>s.Brep));da.SetDataList(3,solids.Select(s=>s.Name));
    }
    public override void DrawViewportWires(IGH_PreviewArgs args){if(Hidden||Locked)return;foreach(var entry in scenes)entry.Scene.DrawWires(args.Display,Options(entry.Options),Attributes.Selected?System.Drawing.Color.LimeGreen:System.Drawing.Color.FromArgb(39,115,213));}
    public override void DrawViewportMeshes(IGH_PreviewArgs args){if(Hidden||Locked)return;foreach(var entry in scenes)entry.Scene.DrawMeshes(args.Display,Options(entry.Options),new Rhino.Display.DisplayMaterial(Attributes.Selected?System.Drawing.Color.LimeGreen:System.Drawing.Color.FromArgb(39,115,213),.25));}
}
public sealed class ModelGeometryComponent:SafeComponent
{
    public ModelGeometryComponent():base("SAP Model Geometry","Geometry","Get native Rhino geometry and corresponding SAP object names.","08-Display"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGeometryParameter("Geometry","G","Points, curves and meshes in element order.",GH_ParamAccess.list);p.AddTextParameter("Names","N","SAP names.",GH_ParamAccess.list);p.AddTextParameter("Kinds","K","SAP kinds.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var e=Native<SapModel>(da,0).Definition.Elements;da.SetDataList(0,e.Select(x=>x.Kind==ElementKind.Node?(GeometryBase)new Point(Preview.Point(x.Points[0])):x.Points.Count==2?new LineCurve(Preview.Point(x.Points[0]),Preview.Point(x.Points[1])):Preview.Mesh(x)));da.SetDataList(1,e.Select(x=>x.Name));da.SetDataList(2,e.Select(x=>x.Kind.ToString()));}
}
public sealed class ModelSummaryComponent:SafeComponent
{
    public ModelSummaryComponent():base("SAP Model Summary","Summary","Count objects, properties, cases and results.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddTextParameter("Summary","S","Summary.",GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);da.SetDataList(0,new[]{m.ToString(),"Fingerprint: "+m.Fingerprint}.Concat(m.Definition.Elements.GroupBy(e=>e.Kind).Select(g=>$"{g.Key}: {g.Count()}")).Concat(m.Definition.Operations.GroupBy(o=>o.Key.Split('.')[0]).Select(g=>$"{g.Key}: {g.Count()}")).Concat(m.Results?.Issues.Select(i=>i.Method+": "+i.Message)??[]));}
}
public sealed class UnitsComponent:SafeComponent
{
    public UnitsComponent():base("SAP Model Units","Units","List the installed SDK unit systems.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddIntegerParameter("Values","V","eUnits values.",GH_ParamAccess.list);p.AddTextParameter("Names","N","Force_length_temperature.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var values=ApiSchema.Get("SetPresentUnits").Parameters[0].Enum;da.SetDataList(0,values.Select(e=>e.Value));da.SetDataList(1,values.Select(e=>e.Name));}
}

public abstract class ExecuteModelComponent:SafeComponent
{
    protected abstract bool Analyze { get; }
    private bool wasRun;private SapModel? cached;private string? cacheKey;
    protected ExecuteModelComponent(string title,string category):base(title,title,"Create an isolated SAP2000 process, export, optionally solve and capture results. Trigger only on Run false → true. Use one Model per component.",category){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model","M","One unified Model.",GH_ParamAccess.item);p.AddTextParameter("Path","P","Destination .sdb file.",GH_ParamAccess.item);p.AddBooleanParameter("Run","R","Rising edge starts operation.",GH_ParamAccess.item,false);p.AddBooleanParameter("Overwrite","O","Replace existing destination and sidecars.",GH_ParamAccess.item,false);
        p.AddTextParameter("Cases","C","Case names to run. Empty = all cases defined in this Model.",GH_ParamAccess.list);p[4].Optional=true;p.AddTextParameter("Combinations","Co","Combination names selected for output. Empty = all combinations defined in this Model.",GH_ParamAccess.list);p[5].Optional=true;
        p.AddTextParameter("Quantities","Q","API result keys, e.g. Results.FrameForce. Empty = common geometry-appropriate quantities.",GH_ParamAccess.list);p[6].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Model","M","Exported Model, with results embedded in Model and elements if analyzed.",GH_ParamAccess.item);p.AddTextParameter("Path","P","Saved .sdb path.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Missing quantities / API errors.",GH_ParamAccess.list);p.AddGenericParameter("Beams","B","Frame elements with their own results.",GH_ParamAccess.list);p.AddGenericParameter("Plates","A","Area elements with their own results.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da)
    {
        if(da.Iteration!=0)throw new ArgumentException("Use a single unified Model. Merge lists before running SAP.");
        var model=Native<SapModel>(da,0);string path=Item<string>(da,1);bool run=Item<bool>(da,2),overwrite=Item<bool>(da,3);var cases=new List<string>();var combos=new List<string>();var quantities=new List<string>();da.GetDataList(4,cases);da.GetDataList(5,combos);da.GetDataList(6,quantities);
        string key=model.Fingerprint+"|"+path+"|"+string.Join("|",cases)+"|"+string.Join("|",combos)+"|"+string.Join("|",quantities);
        if(cacheKey!=key)cached=null;
        bool trigger=run&&!wasRun;wasRun=run;
        if(trigger)
        {
            var requests=quantities.Count==0?null:quantities.Distinct().Select(q=>ResultRequest.All(q.StartsWith("Results.")?q:"Results."+q)).ToArray();
            cached=WorkerClient.Execute(model,new RunOptions(path,Analyze,overwrite,cases,combos,requests));cacheKey=key;
        }
        if(cached!=null){Output(da,0,cached);da.SetData(1,Path.GetFullPath(path));da.SetDataList(2,cached.Results?.Issues.Select(i=>i.Method+": "+i.Message)??[]);da.SetDataList(3,cached.Definition.Elements.Where(e=>e.Kind==ElementKind.Frame).Select(e=>new FragmentGoo(new Fragment(cached.Definition.Operations,[e],cached.NativeSource))));da.SetDataList(4,cached.Definition.Elements.Where(e=>e.Kind==ElementKind.Area).Select(e=>new FragmentGoo(new Fragment(cached.Definition.Operations,[e],cached.NativeSource))));}else Message="Run to execute";
    }
}
public sealed class ExportModelComponent:ExecuteModelComponent{protected override bool Analyze=>false;public ExportModelComponent():base("Export SAP2000 Model","09-Export"){} }
public sealed class RunModelComponent:ExecuteModelComponent{protected override bool Analyze=>true;public RunModelComponent():base("Run SAP Model Cases and Combinations","11-Analysis"){} }
public sealed class AnalyzeAndEmbedComponent:ExecuteModelComponent{protected override bool Analyze=>true;public AnalyzeAndEmbedComponent():base("SAP Analyze and Embed Results","11-Analysis"){} }
public sealed class ReadResultArchiveComponent:SafeComponent
{
    public ReadResultArchiveComponent():base("Read SAP Results Into Model","Read results","Load captured results and verify their model fingerprint and .sdb hash.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Unchanged Model definition.",GH_ParamAccess.item);p.AddTextParameter("Archive","A",".sdb.results.json saved by Run Model.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Model with verified stored results.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);Output(da,0,m.WithResults(ResultArchive.Load(Item<string>(da,1),m)));}
}
public sealed class ResultCatalogueComponent:SafeComponent
{
    public ResultCatalogueComponent():base("SAP Result Quantities","Quantities","List available native result methods and output columns.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Methods","M","Use in Run Model Quantities.",GH_ParamAccess.list);p.AddTextParameter("Columns","C","Native column names.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var methods=ApiSchema.Methods.Values.Where(m=>m.Path=="Results").OrderBy(m=>m.Key).ToArray();da.SetDataList(0,methods.Select(m=>m.Key));da.SetDataList(1,methods.Select(m=>string.Join(", ",m.Parameters.Where(p=>p.ByRef).Select(p=>p.Name))));}
}
