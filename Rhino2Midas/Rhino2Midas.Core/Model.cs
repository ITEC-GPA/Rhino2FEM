using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino2Midas.Core;

public readonly record struct Position(double X, double Y, double Z)
{
    public double DistanceTo(Position p) => Math.Sqrt(Math.Pow(X-p.X,2)+Math.Pow(Y-p.Y,2)+Math.Pow(Z-p.Z,2));
    [JsonIgnore] public bool IsFinite => double.IsFinite(X)&&double.IsFinite(Y)&&double.IsFinite(Z);
}
public sealed record Units(string Force="KN", string Length="M", string Heat="KJ", string Temperature="C")
{
    public void Validate()
    {
        if(!new[]{"N","KN","KGF","TONF","LBF","KIPS"}.Contains(Force) || !new[]{"MM","CM","M","IN","FT"}.Contains(Length) || !new[]{"J","KJ","CAL","KCAL","BTU"}.Contains(Heat) || !new[]{"C","F"}.Contains(Temperature))
            throw new ArgumentException("Unsupported Midas unit. Use uppercase native unit tokens.");
    }
    public override string ToString() => $"{Force}, {Length}, {Heat}, {Temperature}";
}
public enum ElementKind { Node, Beam, Plate, Solid, Truss, Tension, Compression, ElasticLink }
public sealed record Element
{
    public int Id { get; init; }
    public ElementKind Kind { get; init; }
    public IReadOnlyList<Position> Points { get; init; }
    public int Material { get; init; }
    public int Property { get; init; }
    public double Angle { get; init; }
    public IReadOnlyList<int> NodeIds { get; init; }
    public string Extra { get; init; }
    public IReadOnlyList<ResultTable> Results { get; init; }
    [JsonConstructor]
    public Element(int id, ElementKind kind, IReadOnlyList<Position> points, int material=0, int property=0, double angle=0, IReadOnlyList<int>? nodeIds=null, string extra="", IReadOnlyList<ResultTable>? results=null)
    { Id=id;Kind=kind;Points=Array.AsReadOnly(points.ToArray());Material=material;Property=property;Angle=angle;NodeIds=Array.AsReadOnly(nodeIds?.ToArray()??[]);Extra=extra;Results=Array.AsReadOnly(results?.ToArray()??[]); }
    public override string ToString() => $"{Kind} {Id}";
}
public sealed record ApiEntry
{
    public Guid Identity { get; init; }
    public string Endpoint { get; init; }
    public int Id { get; init; }
    public JsonElement Data { get; init; }
    public bool IsItem { get; init; }
    [JsonConstructor]
    public ApiEntry(string endpoint,int id,JsonElement data,bool isItem=false,Guid identity=default)
    {
        Endpoint=endpoint.ToUpperInvariant();Id=id;Data=data.Clone();IsItem=isItem;
        Identity=identity==Guid.Empty?Guid.NewGuid():identity;
        if(!System.Text.RegularExpressions.Regex.IsMatch(Endpoint,@"^[A-Z][A-Z0-9_-]*$"))throw new ArgumentException("Expected a database endpoint name, e.g. MATL.");
        if(id<=0||data.ValueKind!=JsonValueKind.Object)throw new ArgumentException("API record requires a positive ID and a JSON object.");
    }
    public static ApiEntry Create(string endpoint,int id,object data,bool isItem=false)=>new(endpoint,id,JsonSerializer.SerializeToElement(data),isItem);
    public override string ToString()=>Endpoint+" / "+Id;
}
public sealed class Fragment
{
    public IReadOnlyList<Element> Elements { get; }
    public IReadOnlyList<ApiEntry> Entries { get; }
    [JsonConstructor]
    public Fragment(IReadOnlyList<Element>? elements=null,IReadOnlyList<ApiEntry>? entries=null)
    {Elements=Array.AsReadOnly(elements?.ToArray()??[]);Entries=Array.AsReadOnly(entries?.ToArray()??[]);}
    public Fragment Append(ApiEntry entry)=>new(Elements,Entries.Append(entry).ToArray());
    public Fragment ForElement(Element element)
    {
        var nodes=Elements.Where(e=>e.Kind==ElementKind.Node&&element.NodeIds.Contains(e.Id)).ToArray();
        var selected=element.Kind==ElementKind.Node?new[]{element}:nodes.Append(element).ToArray();
        var nodeIds=selected.Where(e=>e.Kind==ElementKind.Node).Select(e=>e.Id).ToHashSet();
        return new(selected,Entries.Where(e=>ModelValidation.TargetTable(e.Endpoint) switch
        {"NODE"=>nodeIds.Contains(e.Id),"ELEM"=>element.Kind is not (ElementKind.Node or ElementKind.ElasticLink)&&e.Id==element.Id,_=>true}).ToArray());
    }
    public static Fragment Combine(IEnumerable<Fragment> fragments)
    {
        var all=fragments.ToArray(); var elements=new Dictionary<(int,int),Element>();
        foreach(var e in all.SelectMany(f=>f.Elements))
        {
            var key=(e.Kind==ElementKind.Node?0:e.Kind==ElementKind.ElasticLink?2:1,e.Id);
            if(elements.TryGetValue(key,out var previous))
            {
                if(ModelArchive.Json(previous with{Results=[]})!=ModelArchive.Json(e with{Results=[]}))throw new ArgumentException($"Conflicting {e.Kind} ID {e.Id}. Assign unique IDs before merging.");
            }
            else elements.Add(key,e with{Results=[]});
        }
        var entries=all.SelectMany(f=>f.Entries).DistinctBy(b=>b.Identity).ToArray();
        return new(elements.Values.ToArray(),entries);
    }
    public override string ToString()=>$"Midas definition: {Elements.Count} elements, {Entries.Count} API records";
}
public sealed class MidasModel
{
    public Fragment Definition { get; }
    public Units Units { get; }
    public double Tolerance { get; }
    public ResultSet? Results { get; }
    [JsonIgnore] public string Fingerprint=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ApiModel.Canonical(this))));
    [JsonConstructor]
    public MidasModel(Fragment definition,Units? units=null,double tolerance=1e-6,ResultSet? results=null)
    {
        Units=units??new();Units.Validate();
        if(!double.IsFinite(tolerance)||tolerance<=0)throw new ArgumentException("Tolerance must be finite and positive.");
        Tolerance=tolerance;Definition=Compose(definition,tolerance);Results=results;
        if(results!=null)
        {
            if(results.Fingerprint!=Fingerprint||results.Units!=Units)throw new ArgumentException("Results belong to another model or unit system.");
            Definition=new(Definition.Elements.Select(e=>e with{Results=Array.AsReadOnly(results.Tables.Select(t=>t.ForElement(e)).Where(t=>t.Rows.Count>0).ToArray())}).ToArray(),Definition.Entries);
        }
    }
    private static Fragment Compose(Fragment source,double tolerance)
    {
        var combined=Fragment.Combine([source]);var explicitNodes=combined.Elements.Where(e=>e.Kind==ElementKind.Node).OrderBy(e=>e.Id).ToList();
        if(explicitNodes.Any(e=>e.Points.Count!=1))throw new ArgumentException("Each node needs one point.");
        // Preserve explicitly numbered coincident nodes (links, discontinuities, external models).
        var nodes=explicitNodes.ToList();var ids=nodes.Select(n=>n.Id).ToHashSet();int next=1;var members=new List<Element>();
        foreach(var e in combined.Elements.Where(e=>e.Kind!=ElementKind.Node).OrderBy(e=>e.Id))
        {
            if(e.NodeIds.Count!=0&&e.NodeIds.Count!=e.Points.Count)throw new ArgumentException($"{e}: connectivity size mismatch.");
            var connectivity=new List<int>();var points=new List<Position>();
            for(int i=0;i<e.Points.Count;i++)
            {
                var p=e.Points[i];Element? node=null;
                if(e.NodeIds.Count>0)
                {
                    node=nodes.FirstOrDefault(n=>n.Id==e.NodeIds[i])??throw new ArgumentException($"{e}: missing node {e.NodeIds[i]}.");
                    if(node.Points[0].DistanceTo(p)>tolerance)throw new ArgumentException($"{e}: node coordinate mismatch.");
                }
                else
                {
                    var matches=nodes.Where(n=>n.Points[0].DistanceTo(p)<=tolerance).ToArray();
                    if(matches.Length>1)throw new ArgumentException($"{e}: ambiguous coincident nodes. Supply explicit connectivity.");
                    node=matches.FirstOrDefault();
                    if(node==null){while(ids.Contains(next))next++;node=new(next,ElementKind.Node,[p]);ids.Add(next++);nodes.Add(node);}
                }
                connectivity.Add(node.Id);points.Add(node.Points[0]);
            }
            members.Add(e with{Points=Array.AsReadOnly(points.ToArray()),NodeIds=Array.AsReadOnly(connectivity.ToArray()),Results=[]});
        }
        return new(nodes.Concat(members).ToArray(),combined.Entries);
    }
    public MidasModel WithResults(ResultSet results)=>new(Definition,Units,Tolerance,results);
    public MidasModel ClearResults()=>new(Definition,Units,Tolerance);
    public static MidasModel Merge(IEnumerable<MidasModel> models)
    {
        var a=models.ToArray();if(a.Length==0)throw new ArgumentException("No models to merge.");
        if(a.Any(m=>m.Units!=a[0].Units||m.Tolerance!=a[0].Tolerance))throw new ArgumentException("Models must use identical units and tolerance.");
        return new(Fragment.Combine(a.Select(m=>m.Definition)),a[0].Units,a[0].Tolerance);
    }
    public override string ToString()=>$"Midas Model: {Definition.Elements.Count(e=>e.Kind==ElementKind.Node)} nodes, {Definition.Elements.Count(e=>e.Kind!=ElementKind.Node)} elements, {Results?.Tables.Count??0} result tables";
}
public static class ModelArchive
{
    private static readonly JsonSerializerOptions Options=new(){WriteIndented=true};
    public static string Json<T>(T value)=>JsonSerializer.Serialize(value,Options);
    public static string Serialize(MidasModel model)=>Json(model);
    public static MidasModel Deserialize(string json)=>JsonSerializer.Deserialize<MidasModel>(json,Options)??throw new FormatException("Empty model archive.");
    public static string SerializeFragment(Fragment fragment)=>Json(fragment);
    public static Fragment DeserializeFragment(string json)=>JsonSerializer.Deserialize<Fragment>(json,Options)??throw new FormatException("Empty definition archive.");
}
