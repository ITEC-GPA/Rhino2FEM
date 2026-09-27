using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2MidasGen.Core;
using Rhino2MidasGen.Api;

namespace Rhino2MidasGen.Grasshopper;

public sealed class BuildModelComponent:SafeComponent
{
    public BuildModelComponent():base("Build Midas GEN Model","Model","Collect definitions, weld implicit nodes and preserve explicit connectivity.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definitions","D","Fragments and Models.",GH_ParamAccess.list);p.AddGenericParameter("Units","U","Optional Midas GEN Units. Default KN/M/KJ/C. Coordinates are not converted.",GH_ParamAccess.item);p[1].Optional=true;p.AddNumberParameter("Tolerance","T","Node welding tolerance.",GH_ParamAccess.item,1e-6);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Model","M","Unified Model.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Validation issues.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var raw=new List<object>();da.GetDataList(0,raw);object u=null!;var units=da.GetData(1,ref u)?Unwrap(u) as Units??throw new ArgumentException("Use Midas GEN Units."):new Units();foreach(var m in raw.Select(Unwrap).OfType<MidasModel>())if(m.Units!=units)throw new ArgumentException("Input Model uses different units.");var model=new MidasModel(Fragment.Combine(raw.Select(FragmentOf)),units,Item<double>(da,2));Output(da,0,model);da.SetDataList(1,ModelValidation.Errors(model).Concat(ModelValidation.Warnings(model)));}
}
public sealed class UnitsComponent:SafeComponent
{
    public UnitsComponent():base("Midas GEN Model Units","Units","All dimensional values use this unit system. No implicit conversion.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Force","F","N KN KGF TONF LBF KIPS.",GH_ParamAccess.item,"KN");p.AddTextParameter("Length","L","MM CM M IN FT.",GH_ParamAccess.item,"M");p.AddTextParameter("Temperature","T","C F.",GH_ParamAccess.item,"C");p.AddTextParameter("Heat","H","J KJ CAL KCAL BTU.",GH_ParamAccess.item,"KJ");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Units","U","Model units.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var u=new Units(Item<string>(da,0).ToUpperInvariant(),Item<string>(da,1).ToUpperInvariant(),Item<string>(da,3).ToUpperInvariant(),Item<string>(da,2).ToUpperInvariant());u.Validate();da.SetData(0,u);}
}
public sealed class MergeModelsComponent:SafeComponent
{
    public MergeModelsComponent():base("Merge Midas GEN Models","Merge","Merge compatible Model definitions. Conflicting IDs/units are rejected; results cleared.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Models","M","Models with coordinated IDs and units.",GH_ParamAccess.list);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Merged model.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,MidasModel.Merge(NativeList<MidasModel>(da,0)));
}
public sealed class ValidateModelComponent:SafeComponent
{
    public ValidateModelComponent():base("Validate Midas GEN Model","Validate","Check managed data and dependencies before GEN export.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddBooleanParameter("Valid","V","Managed checks passed; not a structural stability check.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Validation messages.",GH_ParamAccess.list);p.AddTextParameter("Fingerprint","H","Model SHA256.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var m=Native<MidasModel>(da,0);var errors=ModelValidation.Errors(m);da.SetData(0,errors.Count==0);da.SetDataList(1,errors.Concat(ModelValidation.Warnings(m)));if(errors.Count==0)da.SetData(2,m.Fingerprint);}
}
public sealed class DecomposeModelComponent:SafeComponent
{
    public DecomposeModelComponent():base("Decompose Midas GEN Model","Decompose","Access nodes, members, definitions and embedded results.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){foreach(var k in Enum.GetValues<ElementKind>())p.AddGenericParameter(k.ToString(),k.ToString(),k+" elements with results.",GH_ParamAccess.list);p.AddGenericParameter("Definitions","D","Native API entries.",GH_ParamAccess.list);p.AddGenericParameter("Results","R","Native result tables.",GH_ParamAccess.list);p.AddGenericParameter("Units","U","Units.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var m=Native<MidasModel>(da,0);foreach(var kind in Enum.GetValues<ElementKind>())da.SetDataList((int)kind,m.Definition.Elements.Where(e=>e.Kind==kind).Select(e=>new FragmentGoo(m.Definition.ForElement(e))));da.SetDataList(Enum.GetValues<ElementKind>().Length,m.Definition.Entries.Select(e=>new FragmentGoo(new(entries:[e]))));da.SetDataList(Enum.GetValues<ElementKind>().Length+1,m.Results?.Tables??[]);da.SetData(Enum.GetValues<ElementKind>().Length+2,m.Units);}
}
public sealed class ModelSummaryComponent:SafeComponent
{
    public ModelSummaryComponent():base("Midas GEN Model Summary","Summary","Counts and unit/result status.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddTextParameter("Summary","S","Summary.",GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da){var m=Native<MidasModel>(da,0);da.SetDataList(0,new[]{m.ToString(),m.Units.ToString()}.Concat(m.Definition.Elements.GroupBy(e=>e.Kind).Select(g=>$"{g.Key}: {g.Count()}")).Concat(m.Definition.Entries.GroupBy(e=>e.Endpoint).Select(g=>$"{g.Key}: {g.Count()}")));}
}
public sealed class ExampleModelComponent:SafeComponent
{
    public ExampleModelComponent():base("Midas GEN Cantilever Example","Example","3 m fixed cantilever, 200×300 mm rectangle, E=210 GPa, 10 kN downward tip load. KN/m units.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Offline example. Connect to Preview or Analyze.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Definitions.Example());
}
public sealed class ClearResultsComponent:SafeComponent
{
    public ClearResultsComponent():base("Clear Midas GEN Results","Clear Results","Return a copy without stored solver results.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Model with no results.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Native<MidasModel>(da,0).ClearResults());
}
public abstract class ExecuteModelComponent:SafeComponent
{
    protected abstract bool Analyze{get;}
    private bool wasRun;private MidasModel? cached;private string? cacheKey;
    protected ExecuteModelComponent(string title,string category):base(title,title,"Direct GEN NX REST API. Run on false→true. Replaces GEN's active document only with explicit Replace active document=true. Set MIDAS_GEN_API_URL and MIDAS_GEN_API_KEY in the environment.",category){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","One unified Model.",GH_ParamAccess.item);p.AddTextParameter("Path","P","Local .mgb destination.",GH_ParamAccess.item);p.AddBooleanParameter("Run","R","Rising edge executes.",GH_ParamAccess.item,false);p.AddBooleanParameter("Replace active document","A","Save existing GEN work first. Allows /doc/NEW to replace the active document.",GH_ParamAccess.item,false);p.AddBooleanParameter("Overwrite","O","Allow overwriting destination and sidecars.",GH_ParamAccess.item,false);p.AddGenericParameter("Requests","Q","Native Result Requests; empty = nodal, beam, plate, solid, truss and link results for static cases.",GH_ParamAccess.list);p[5].Optional=true;}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Model","M","Model, with embedded results if analyzed.",GH_ParamAccess.item);p.AddGenericParameter("Beams","B","Beams with own results.",GH_ParamAccess.list);p.AddGenericParameter("Plates","P","Plates with own results.",GH_ParamAccess.list);p.AddTextParameter("Path","F","GEN model path.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Missing/failed result quantities.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da)
    {
        if(da.Iteration!=0)throw new ArgumentException("Merge lists into one Model before execution.");var m=Native<MidasModel>(da,0);string path=Path.GetFullPath(Item<string>(da,1));var requests=NativeList<TableRequest>(da,5);bool run=Item<bool>(da,2);string key=m.Fingerprint+path+ModelArchive.Json(requests);
        if(cacheKey!=key)cached=null;bool trigger=run&&!wasRun;wasRun=run;
        if(trigger){cached=null;using var client=ConnectionSettings.Create();cached=GenService.ExecuteAsync(client,m,new(path,Analyze,Item<bool>(da,3),Item<bool>(da,4),requests.Count>0?requests:null)).GetAwaiter().GetResult();cacheKey=key;}
        if(cached==null){Message="Run to execute";return;}Output(da,0,cached);da.SetDataList(1,cached.Definition.Elements.Where(e=>e.Kind==ElementKind.Beam).Select(e=>new FragmentGoo(cached.Definition.ForElement(e))));da.SetDataList(2,cached.Definition.Elements.Where(e=>e.Kind==ElementKind.Plate).Select(e=>new FragmentGoo(cached.Definition.ForElement(e))));da.SetData(3,path);da.SetDataList(4,cached.Results?.Issues??[]);
    }
}
public sealed class ExportModelComponent:ExecuteModelComponent{protected override bool Analyze=>false;public ExportModelComponent():base("Export Midas GEN Model","09-Export"){} }
public sealed class AnalyzeModelComponent:ExecuteModelComponent{protected override bool Analyze=>true;public AnalyzeModelComponent():base("Midas GEN Analyze and Embed Results","11-Analysis"){} }
public sealed class RunCasesComponent:ExecuteModelComponent{protected override bool Analyze=>true;public RunCasesComponent():base("Run Midas GEN Model Cases and Combinations","11-Analysis"){} }
public sealed class ModelArchiveComponent:SafeComponent
{
    public ModelArchiveComponent():base("Midas GEN Model to JSON","Model JSON","Serialize model definitions and results without API credentials.","09-Export"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddTextParameter("JSON","J","Rhino2MidasGen model archive.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>da.SetData(0,ModelArchive.Serialize(Native<MidasModel>(da,0)));
}
public sealed class ReadModelArchiveComponent:SafeComponent
{
    public ReadModelArchiveComponent():base("Midas GEN Model from JSON","Read JSON","Restore a Rhino2MidasGen archive, including model-bound results.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddTextParameter("JSON","J","Archive JSON content, e.g. Read File output.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Restored model.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,ModelArchive.Deserialize(Item<string>(da,0)));
}
public sealed class ReadResultArchiveComponent:SafeComponent
{
    public ReadResultArchiveComponent():base("Read Midas GEN Results into Model","Read Results","Verify model fingerprint, units and GEN source file hash before loading results.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Unchanged model.",GH_ParamAccess.item);p.AddTextParameter("Archive","A",".mgb.results.json path.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Model with verified results.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var m=Native<MidasModel>(da,0);Output(da,0,m.WithResults(ResultSet.Load(Item<string>(da,1),m)));}
}
public sealed class ReadGenModelComponent:SafeComponent
{
    private bool wasRun;private MidasModel? cached;private string? tableKey;
    public ReadGenModelComponent():base("Read Active Midas GEN Model","Read GEN","Read selected database tables from the active GEN document. No modification. Include all required property/load tables for a complete model.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Tables","T","Additional /db table names. UNIT/NODE/ELEM always read. Include MATL SECT THIK and all needed assignments.",GH_ParamAccess.list);p[0].Optional=true;p.AddBooleanParameter("Run","R","Read on rising edge.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Model","M","Imported model subset.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Missing properties/dependencies.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){bool run=Item<bool>(da,1);var tables=new List<string>();da.GetDataList(0,tables);string current=string.Join("|",tables);if(current!=tableKey)cached=null;tableKey=current;bool trigger=run&&!wasRun;wasRun=run;if(trigger){cached=null;using var client=ConnectionSettings.Create();cached=GenService.ReadModelAsync(client,tables.Select(s=>s.ToUpperInvariant()).ToArray()).GetAwaiter().GetResult();}if(cached!=null){Output(da,0,cached);da.SetDataList(1,ModelValidation.Errors(cached).Concat(ModelValidation.Warnings(cached)));}}
}
