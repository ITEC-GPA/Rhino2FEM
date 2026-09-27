using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace Rhino2MidasGen.Core;

public sealed class ResultTable
{
    public string Name { get; }
    public string Type { get; }
    public IReadOnlyList<string> Columns { get; }
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }
    [JsonConstructor]
    public ResultTable(string name,string type,IReadOnlyList<string> columns,IReadOnlyList<IReadOnlyList<string>> rows)
    {
        Name=name;Type=type;Columns=Array.AsReadOnly(columns.ToArray());Rows=Array.AsReadOnly(rows.Select(r=>(IReadOnlyList<string>)Array.AsReadOnly(r.ToArray())).ToArray());
        if(Rows.Any(r=>r.Count!=Columns.Count))throw new FormatException($"{name}: table row/header length mismatch.");
    }
    public int Column(string name,int occurrence=0)
    {
        if(occurrence<0)throw new ArgumentOutOfRangeException(nameof(occurrence));
        for(int i=0;i<Columns.Count;i++)if(Columns[i].Equals(name,StringComparison.OrdinalIgnoreCase)&&occurrence--==0)return i;
        throw new ArgumentException($"Column '{name}' absent. Available: {string.Join(", ",Columns)}");
    }
    public ResultTable ForElement(Element element)
    {
        bool compatible=element.Kind switch
        {
            ElementKind.Node=>!Type.StartsWith("ELASTICLINK",StringComparison.Ordinal)&&!Columns.Any(c=>c.Equals("Elem",StringComparison.OrdinalIgnoreCase)||c.Equals("Element",StringComparison.OrdinalIgnoreCase)),
            ElementKind.ElasticLink=>Type.StartsWith("ELASTICLINK",StringComparison.Ordinal),
            _=>!Type.StartsWith("ELASTICLINK",StringComparison.Ordinal)
        };
        if(!compatible)return new(Name,Type,Columns,[]);
        // GEN wall resultants belong to the native WALL group, not to a finite element ID.
        bool wallResult=Type=="WALL_FORCE_MOMENT";
        if(wallResult&&element.Kind!=ElementKind.Wall)return new(Name,Type,Columns,[]);
        int target=wallResult?Building.WallId(element):element.Id;
        string column=wallResult?"Wall":element.Kind==ElementKind.Node?"Node":element.Kind==ElementKind.ElasticLink?"No.":"Elem";
        int index=Columns.ToList().FindIndex(c=>c.Equals(column,StringComparison.OrdinalIgnoreCase));
        if(index<0&&element.Kind!=ElementKind.Node)index=Columns.ToList().FindIndex(c=>c.Equals("Element",StringComparison.OrdinalIgnoreCase));
        return new(Name,Type,Columns,index<0?[]:Rows.Where(r=>int.TryParse(r[index],out int id)&&id==target).ToArray());
    }
    public ResultTable Filter(string column,string value)=>new(Name,Type,Columns,Rows.Where(r=>r[Column(column)]==value).ToArray());
    public IReadOnlyList<double> Numbers(string column,int occurrence=0)=>Rows.Select(r=>Native.Real(r[Column(column,occurrence)])).ToArray();
    public override string ToString()=>$"{Type}: {Rows.Count} records";
    public static IReadOnlyList<ResultTable> ParseResponse(JsonElement response,string type)
    {
        var output=new List<ResultTable>();
        foreach(var entry in response.EnumerateObject())
        {
            var table=entry.Value;if(table.ValueKind!=JsonValueKind.Object||!table.TryGetProperty("HEAD",out var head)||!table.TryGetProperty("DATA",out var data))continue;
            var headers=head.EnumerateArray().Select(v=>v.ToString()).ToArray();
            var rows=data.EnumerateArray().Select(row=>(IReadOnlyList<string>)row.EnumerateArray().Select(v=>v.ToString()).ToArray()).ToArray();
            output.Add(new(entry.Name,type,headers,rows));
        }
        if(output.Count==0)throw new FormatException("No HEAD/DATA result table returned by Midas.");return output;
    }
}
public sealed class ResultSet
{
    public string Fingerprint{get;}public Units Units{get;}public string SourcePath{get;}public string SourceHash{get;}public bool AnalysisCompleted{get;}public IReadOnlyList<ResultTable> Tables{get;}public IReadOnlyList<string> Issues{get;}
    [JsonConstructor]public ResultSet(string fingerprint,Units units,string sourcePath,string sourceHash,bool analysisCompleted,IReadOnlyList<ResultTable> tables,IReadOnlyList<string> issues)
    {Fingerprint=fingerprint;Units=units;SourcePath=sourcePath;SourceHash=sourceHash;AnalysisCompleted=analysisCompleted;Tables=Array.AsReadOnly(tables.ToArray());Issues=Array.AsReadOnly(issues.ToArray());}
    public static string FileHash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    public void Save(string path,bool overwrite=false)=>AtomicFile.Write(path,ModelArchive.Json(this),overwrite);
    public static ResultSet Load(string path,MidasModel model)
    {
        var result=JsonSerializer.Deserialize<ResultSet>(File.ReadAllText(path))??throw new FormatException("Empty results archive.");
        if(result.Fingerprint!=model.Fingerprint||result.Units!=model.Units)throw new ArgumentException("Archive model fingerprint/units do not match.");
        if(!File.Exists(result.SourcePath)||result.SourceHash!=FileHash(result.SourcePath))throw new ArgumentException("Source model is missing or changed since analysis.");
        return result;
    }
}
public sealed record TableRequest(string Type,IReadOnlyList<string> Components,IReadOnlyList<int> Ids,IReadOnlyList<string> Cases,IReadOnlyList<string>? Parts=null,IReadOnlyList<string>? Stages=null,JsonElement? Options=null)
{
    public object Body(Units units)
    {
        if(string.IsNullOrWhiteSpace(Type))throw new ArgumentException("A result request needs a native table type.");
        var argument=Options.HasValue?Options.Value.EnumerateObject().Where(p=>p.Name!="EXPORT_PATH").ToDictionary(p=>p.Name,p=>(object)p.Value.Clone()):new Dictionary<string,object>();
        argument["TABLE_NAME"]=Type;argument["TABLE_TYPE"]=Type;argument["UNIT"]=new{FORCE=units.Force,DIST=units.Length};argument["STYLES"]=new{FORMAT="Scientific",PLACE=12};argument["COMPONENTS"]=Components;argument["NODE_ELEMS"]=new{KEYS=Ids};argument["LOAD_CASE_NAMES"]=Cases;
        if(Ids.Count==0)argument.Remove("NODE_ELEMS");if(Cases.Count==0)argument.Remove("LOAD_CASE_NAMES");
        if(Components.Count==0)argument.Remove("COMPONENTS");
        if(Parts?.Count>0)argument["PARTS"]=Parts;if(Stages?.Count>0){argument["OPT_CS"]=true;argument["STAGE_STEP"]=Stages;}
        return new{Argument=argument};
    }
    public static TableRequest Beam(IEnumerable<int> ids,IReadOnlyList<string> cases)=>new("BEAMFORCE",["Elem","Load","Part","Axial","Shear-y","Shear-z","Torsion","Moment-y","Moment-z"],ids.ToArray(),cases,["PartI","PartJ"]);
    public static TableRequest Displacement(IEnumerable<int> ids,IReadOnlyList<string> cases)=>new("DISPLACEMENTG",["Node","Load","DX","DY","DZ","RX","RY","RZ"],ids.ToArray(),cases);
    public static TableRequest Reaction(IEnumerable<int> ids,IReadOnlyList<string> cases)=>new("REACTIONG",["Node","Load","FX","FY","FZ","MX","MY","MZ"],ids.ToArray(),cases);
    public static TableRequest FromTemplate(string type,IEnumerable<int> ids,IReadOnlyList<string> cases)
    {
        var example=ApiCatalogue.Definitions.Values.FirstOrDefault(d=>d.Endpoint=="post/TABLE"&&Native.String(d.Example,"TABLE_TYPE")==type)?.Example??throw new ArgumentException("No documented request template for "+type);
        if(example.TryGetProperty("AVERAGE_NODAL_RESULT",out _))example=Native.With(example,"AVERAGE_NODAL_RESULT",false);
        var options=System.Text.Json.Nodes.JsonNode.Parse(example.GetRawText())!.AsObject();
        options.Remove("STORY_NAMES");
        return new(type,example.TryGetProperty("COMPONENTS",out var components)?components.EnumerateArray().Select(s=>s.GetString()!).ToArray():[],ids.ToArray(),cases,Options:JsonSerializer.SerializeToElement(options));
    }
}
