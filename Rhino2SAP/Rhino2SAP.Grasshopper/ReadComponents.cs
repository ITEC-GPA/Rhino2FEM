using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino2SAP.Api;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public sealed class ReadGeometryComponent:SafeComponent
{
    private bool wasRun;private Fragment? cached;private string? cachedPath;
    public ReadGeometryComponent():base("Read SAP Geometry","Read SAP","Read native geometry from .sdb without saving it. Output excludes materials, assignments and loads; assign properties before rebuilding a model.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Path","P","Existing .sdb.",GH_ParamAccess.item);p.AddBooleanParameter("Run","R","Rising edge reads geometry.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Geometry","G","Geometry-only fragment, in the file's present units. Reassign properties before export.",GH_ParamAccess.item);p.AddTextParameter("Names","N","Native names.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){if(da.Iteration!=0)throw new ArgumentException("Use one file per reader.");string path=Item<string>(da,0);bool run=Item<bool>(da,1);bool trigger=run&&!wasRun;wasRun=run;if(path!=cachedPath)cached=null;if(trigger){cached=WorkerClient.Geometry(path);cachedPath=path;}if(cached!=null){Output(da,0,cached);da.SetDataList(1,cached.Elements.Select(e=>e.Name));}}
}
public sealed class ReadApiTableComponent:SafeComponent
{
    private bool wasRun;private ResultTable? cached;private string? cacheKey;
    public ReadApiTableComponent():base("Read SAP API Table","API Read","Read any documented Get method or Results quantity from a saved SAP file. No model mutations are allowed.","10-Import"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("File","F","Existing .sdb.",GH_ParamAccess.item);p.AddTextParameter("Method","M","Example PropFrame.GetGeneral, FrameObj.GetSection, Results.FrameForce.",GH_ParamAccess.item);p.AddTextParameter("Arguments JSON","A","Named non-output parameters, e.g. {\"Name\":\"F1\"}.",GH_ParamAccess.item,"{}");p.AddTextParameter("Cases","C","Cases for Results queries.",GH_ParamAccess.list);p[3].Optional=true;p.AddTextParameter("Combinations","Co","Combinations for Results queries.",GH_ParamAccess.list);p[4].Optional=true;p.AddBooleanParameter("Run","R","Rising edge triggers read.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Table","T","Named native data.",GH_ParamAccess.item);p.AddTextParameter("Columns","C","Column names.",GH_ParamAccess.list);p.AddGenericParameter("Values","V","Each column on its own branch {column}.",GH_ParamAccess.tree);}
    protected override void Solve(IGH_DataAccess da)
    {
        if(da.Iteration!=0)throw new ArgumentException("Use one query per reader.");string file=Item<string>(da,0),method=Item<string>(da,1),json=Item<string>(da,2);var cases=new List<string>();var combos=new List<string>();da.GetDataList(3,cases);da.GetDataList(4,combos);bool run=Item<bool>(da,5);bool trigger=run&&!wasRun;wasRun=run;string key=file+method+json+string.Join("|",cases)+string.Join("|",combos);if(key!=cacheKey)cached=null;
        if(trigger){var args=JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(json)??[];cached=WorkerClient.Query(file,method,args,cases.ToArray(),combos.ToArray());cacheKey=key;}
        if(cached!=null){da.SetData(0,cached);da.SetDataList(1,cached.Columns.Keys);var tree=new GH_Structure<GH_ObjectWrapper>();int i=0;foreach(var column in cached.Columns.Values){var values=column.ValueKind==JsonValueKind.Array?column.EnumerateArray().ToArray():new[]{column};foreach(var v in values)tree.Append(new GH_ObjectWrapper(ApiResultComponent.Scalar(v)),new GH_Path(i));i++;}da.SetDataTree(2,tree);}
    }
}
public sealed class MaterialLibraryComponent:SafeComponent
{
    public MaterialLibraryComponent():base("SAP Material Library","Material DB","Read material names, standards and grades from the installed CSI XML library.","01-Materials"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("XML","X","Empty = installed Europe material library.",GH_ParamAccess.item,"");p.AddTextParameter("Type","T","Optional Steel, Concrete, Rebar, etc.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Regions","R","Region.",GH_ParamAccess.list);p.AddTextParameter("Standards","S","Standard.",GH_ParamAccess.list);p.AddTextParameter("Grades","G","Grade.",GH_ParamAccess.list);p.AddTextParameter("Types","T","Material type.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){string filter=Item<string>(da,1);var entries=ReadService.MaterialLibrary(Item<string>(da,0)).Where(e=>filter.Length==0||e.Type.Equals(filter,StringComparison.OrdinalIgnoreCase)).ToArray();da.SetDataList(0,entries.Select(e=>e.Region));da.SetDataList(1,entries.Select(e=>e.Standard));da.SetDataList(2,entries.Select(e=>e.Grade));da.SetDataList(3,entries.Select(e=>e.Type));}
}
public sealed class DatabaseMaterialComponent:SafeComponent
{
    public DatabaseMaterialComponent():base("SAP Database Material","DB Material","Add a native SAP material by region, standard and grade. SAP supplies mechanical and design properties from its library.","01-Materials"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddTextParameter("Name","N","Requested material name.",GH_ParamAccess.item);p.AddIntegerParameter("Type","T","SAP eMatType: 1 Steel, 2 Concrete, etc.",GH_ParamAccess.item,1);p.AddTextParameter("Region","R","Region.",GH_ParamAccess.item,"Europe");p.AddTextParameter("Standard","S","Standard exactly as in Material Library.",GH_ParamAccess.item,"EN 1993-1-1 per EN 10025-2");p.AddTextParameter("Grade","G","Grade.",GH_ParamAccess.item,"S355");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Material","M","Native library material definition.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){string n=Item<string>(da,0);Output(da,0,new Fragment([Operation.Create("PropMaterial.AddMaterial",("Name",n),("UserName",n),("MatType",Item<int>(da,1)),("Region",Item<string>(da,2)),("Standard",Item<string>(da,3)),("Grade",Item<string>(da,4)))]));}
}
public sealed class ApiHelpComponent:SafeComponent
{
    public ApiHelpComponent():base("SAP API Signature","API Help","Inspect exact installed SDK argument names, types, optional defaults and enums.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddTextParameter("Method","M","API key such as FrameObj.SetLoadDistributed.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Parameters","P","Parameter contract.",GH_ParamAccess.list);p.AddTextParameter("Help file","H","Installed CSI_OAPI_Documentation.chm.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var m=ApiSchema.Get(Item<string>(da,0));da.SetDataList(0,m.Parameters.Select(p=>$"{p.Name}: {p.Type}; ref={p.ByRef}; optional={p.Optional}; default={p.Default}; {p.Description}"));da.SetData(1,Path.Combine(SapClient.FindInstallation(),"CSI_OAPI_Documentation.chm"));}
}
