using System.Collections.Frozen;
using System.Text.Json;

namespace Rhino2SAP.Core;

public sealed record ResultRequest(string Method, IReadOnlyDictionary<string,JsonElement> Arguments)
{
    public static ResultRequest All(string method) => new(method,ApiSchema.Get(method).Parameters.Where(p=>!p.ByRef&&!p.Optional).ToFrozenDictionary(p=>p.Name,p=>p.Name=="Name"?ApiSchema.Value("ALL"):p.Name=="ItemTypeElm"?ApiSchema.Value(2):throw new ArgumentException($"{method} needs a custom {p.Name}.")));
}
public sealed class ResultTable
{
    public string Method { get; }
    public IReadOnlyDictionary<string,JsonElement> Columns { get; }
    public ResultTable(string method, IReadOnlyDictionary<string,JsonElement> columns)
    { Method=method; Columns=columns.ToFrozenDictionary(p=>p.Key,p=>p.Value.Clone(),StringComparer.OrdinalIgnoreCase); }
    public int RowCount => Columns.Values.Where(v=>v.ValueKind==JsonValueKind.Array).Select(v=>v.GetArrayLength()).DefaultIfEmpty(0).Max();
    public override string ToString() => $"{Method}: {RowCount} rows";
    public ResultTable SelectRows(IEnumerable<int> rows)
    {
        var indices=rows.ToArray();
        return new ResultTable(Method,Columns.ToDictionary(p=>p.Key,p=>p.Value.ValueKind==JsonValueKind.Array?ApiSchema.Value(indices.Where(i=>i<p.Value.GetArrayLength()).Select(i=>p.Value[i]).ToArray()):p.Key=="NumberResults"?ApiSchema.Value(indices.Length):p.Value));
    }
}
public sealed record ResultIssue(string Method,string Message);
public sealed class ResultSet
{
    public string Fingerprint { get; }
    public string FilePath { get; }
    public string FileHash { get; }
    public bool Verified { get; }
    public IReadOnlyList<ResultTable> Tables { get; }
    public IReadOnlyList<ResultIssue> Issues { get; }
    public bool IsComplete => Issues.Count==0;
    public ResultSet(string fingerprint,string filePath,string fileHash,bool verified,IReadOnlyList<ResultTable> tables,IReadOnlyList<ResultIssue> issues)
    { Fingerprint=fingerprint;FilePath=filePath;FileHash=fileHash;Verified=verified;Tables=Array.AsReadOnly(tables.ToArray());Issues=Array.AsReadOnly(issues.ToArray()); }
    public void RequireValidFor(SapModel model,bool verified=true)
    { if(Fingerprint!=model.Fingerprint)throw new InvalidOperationException("Stale results: model changed.");if(verified&&!Verified)throw new InvalidOperationException("Results have unverified provenance."); }
}

/// <summary>Immutable results embedded in one element. Includes own element quantities and its joint kinematics.</summary>
public sealed class ElementResults
{
    public string ModelFingerprint { get; }
    public string ElementName { get; }
    public ElementKind Kind { get; }
    public IReadOnlyList<ResultTable> Tables { get; }
    public IReadOnlyList<ResultIssue> Issues { get; }
    public bool Verified { get; }
    public ElementResults(string modelFingerprint,string elementName,ElementKind kind,IReadOnlyList<ResultTable> tables,IReadOnlyList<ResultIssue> issues,bool verified)
    {ModelFingerprint=modelFingerprint;ElementName=elementName;Kind=kind;Tables=Array.AsReadOnly(tables.ToArray());Issues=Array.AsReadOnly(issues.ToArray());Verified=verified;}
    public static ElementResults Attach(Element element,ResultSet results,double tolerance)
    {
        var nodeNames=new HashSet<string>();
        var coords=results.Tables.FirstOrDefault(t=>t.Method=="Rhino2SAP.NodeCoordinates");
        if(coords!=null)
            for(int i=0;i<coords.RowCount;i++)
            {var point=new Position(coords.Columns["X"][i].GetDouble(),coords.Columns["Y"][i].GetDouble(),coords.Columns["Z"][i].GetDouble());if(element.Points.Any(p=>p.DistanceTo(point)<=tolerance))nodeNames.Add(coords.Columns["Name"][i].GetString()!);}
        if(element.Kind==ElementKind.Node)nodeNames.Add(element.Name);
        bool Own(string key)=>element.Kind switch{ElementKind.Frame=>key.StartsWith("Results.Frame"),ElementKind.Area=>key.StartsWith("Results.Area"),ElementKind.Solid=>key.StartsWith("Results.Solid"),ElementKind.Link=>key.StartsWith("Results.Link"),_=>false};
        var tables=new List<ResultTable>();
        foreach(var table in results.Tables)
        {
            if(!table.Columns.TryGetValue("Obj",out var objects)||objects.ValueKind!=JsonValueKind.Array)continue;
            bool joint=table.Method.StartsWith("Results.Joint")||table.Method=="Results.ModeShape";
            if(!joint&&!Own(table.Method))continue;
            var rows=Enumerable.Range(0,objects.GetArrayLength()).Where(i=>joint?nodeNames.Contains(objects[i].GetString()!):objects[i].GetString()==element.Name).ToArray();
            if(rows.Length>0)tables.Add(table.SelectRows(rows));
        }
        return new(results.Fingerprint,element.Name,element.Kind,tables,results.Issues.Where(i=>Own(i.Method)||i.Method.StartsWith("Results.Joint")).ToArray(),results.Verified);
    }
    public override string ToString()=>$"{Kind} {ElementName}: {Tables.Count} embedded result tables";
}
