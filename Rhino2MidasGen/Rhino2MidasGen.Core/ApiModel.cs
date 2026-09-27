using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rhino2MidasGen.Core;

public static class Native
{
    public static double Real(string text)=>double.Parse(text.Replace('D','E').Replace('d','E'),NumberStyles.Float,CultureInfo.InvariantCulture);
    public static string Name(string name)=>string.IsNullOrWhiteSpace(name)||name.Any(char.IsControl)?throw new ArgumentException("Name cannot be empty or contain control characters."):name.Trim();
    public static string String(JsonElement json,string key,string fallback="")=>json.TryGetProperty(key,out var p)?p.ToString():fallback;
    public static double Number(JsonElement json,string key,double fallback=0)=>json.TryGetProperty(key,out var p)?p.GetDouble():fallback;
    public static bool Boolean(JsonElement json,string key,bool fallback=false)=>json.TryGetProperty(key,out var p)?p.GetBoolean():fallback;
    public static double[] Numbers(JsonElement json,string key)=>json.TryGetProperty(key,out var p)?p.EnumerateArray().Select(n=>n.GetDouble()).ToArray():[];
    public static JsonElement Object(JsonElement json,string key)=>json.TryGetProperty(key,out var p)?p:JsonSerializer.SerializeToElement(new{});
    public static JsonElement With(JsonElement json,string key,object value){var node=JsonNode.Parse(json.GetRawText())!.AsObject();node[key]=JsonSerializer.SerializeToNode(value);return JsonSerializer.SerializeToElement(node);}
    public static string Canonical(JsonElement element)=>element.ValueKind switch
    {
        JsonValueKind.Object=>"{"+string.Join(",",element.EnumerateObject().OrderBy(p=>p.Name,StringComparer.Ordinal).Select(p=>JsonSerializer.Serialize(p.Name)+":"+Canonical(p.Value)))+"}",
        JsonValueKind.Array=>"["+string.Join(",",element.EnumerateArray().Select(Canonical))+"]",
        _=>element.GetRawText()
    };
}
public sealed record DatabaseWrite(string Endpoint,string Method,IReadOnlyDictionary<int,JsonElement> Records)
{
    public object Body()=>new{Assign=Records.ToDictionary(p=>p.Key.ToString(CultureInfo.InvariantCulture),p=>p.Value)};
}
public static class ApiModel
{
    public static IReadOnlyList<DatabaseWrite> Writes(MidasModel model)
    {
        var entries=model.Definition.Entries.ToList();
        entries.Insert(0,ApiEntry.Create("UNIT",1,new{FORCE=model.Units.Force,DIST=model.Units.Length,HEAT=model.Units.Heat,TEMPER=model.Units.Temperature}));
        if(!entries.Any(e=>e.Endpoint=="STYP"))
        {
            double metres=model.Units.Length switch{"MM"=>.001,"CM"=>.01,"IN"=>.0254,"FT"=>.3048,_=>1};
            entries.Add(ApiEntry.Create("STYP",1,new{STYP=0,MASS=1,bMASSOFFSET=false,bSELFWEIGHT=false,GRAV=9.806/metres,TEMP=model.Units.Temperature=="F"?32:0,bALIGNBEAM=false,bALIGNSLAB=false,bROTRIGID=false}));
        }
        foreach(var e in model.Definition.Elements)
        {
            if(e.Kind==ElementKind.Node){entries.Add(ApiEntry.Create("NODE",e.Id,new{X=e.Points[0].X,Y=e.Points[0].Y,Z=e.Points[0].Z}));continue;}
            if(e.Kind==ElementKind.ElasticLink)
            {
                var link=e.Extra.Length>0?JsonSerializer.Deserialize<JsonElement>(e.Extra):throw new ArgumentException($"{e}: missing native elastic link data.");
                link=Native.With(link,"NODE",e.NodeIds);link=Native.With(link,"ANGLE",e.Angle);entries.Add(new("ELNK",e.Id,link));continue;
            }
            string kind=e.Kind switch{ElementKind.Beam=>"BEAM",ElementKind.Plate=>"PLATE",ElementKind.Wall=>"WALL",ElementKind.Solid=>"SOLID",ElementKind.Truss=>"TRUSS",ElementKind.Tension=>"TENSTR",ElementKind.Compression=>"COMPTR",_=>throw new ArgumentException("Unknown element kind.")};
            var data=e.Extra.Length>0?JsonNode.Parse(e.Extra)!.AsObject():new JsonObject();
            data["TYPE"]=kind;data["MATL"]=e.Material;data["SECT"]=e.Property;data["NODE"]=JsonSerializer.SerializeToNode(e.NodeIds);data["ANGLE"]=e.Angle;
            if(e.Kind==ElementKind.Plate&&!data.ContainsKey("STYPE"))data["STYPE"]=1;
            if(e.Kind is ElementKind.Tension or ElementKind.Compression&&!data.ContainsKey("STYPE"))data["STYPE"]=1;
            entries.Add(new("ELEM",e.Id,JsonSerializer.SerializeToElement(data)));
        }
        var writes=new List<DatabaseWrite>();
        foreach(var endpoint in entries.GroupBy(e=>e.Endpoint))
        {
            var records=new SortedDictionary<int,JsonElement>();
            foreach(var target in endpoint.GroupBy(e=>e.Id))
            {
                var distinct=target.DistinctBy(e=>e.Identity).ToArray();
                if(distinct.Any(e=>e.IsItem))
                {
                    var complete=distinct.Where(e=>!e.IsItem).ToArray();
                    if(complete.Select(e=>Native.Canonical(e.Data)).Distinct().Count()>1)throw new ArgumentException($"Conflicting {endpoint.Key} ID {target.Key}.");
                    if(complete.Any(e=>e.Data.EnumerateObject().Any(p=>p.Name!="ITEMS")))throw new ArgumentException($"{endpoint.Key}/{target.Key}: cannot mix item contributions with a record containing other fields.");
                    var items=complete.Take(1).SelectMany(e=>e.Data.GetProperty("ITEMS").EnumerateArray()).ToList();
                    int next=items.Select(e=>(int)Native.Number(e,"ID")).DefaultIfEmpty(0).Max()+1;
                    items.AddRange(distinct.Where(e=>e.IsItem).Select(e=>Native.With(e.Data,"ID",next++)));
                    records[target.Key]=JsonSerializer.SerializeToElement(new{ITEMS=items});
                }
                else
                {
                    var values=distinct.Select(e=>Native.Canonical(e.Data)).Distinct().ToArray();
                    if(values.Length!=1)throw new ArgumentException($"Conflicting {endpoint.Key} ID {target.Key}.");
                    records[target.Key]=distinct[0].Data;
                }
            }
            writes.Add(new(endpoint.Key,endpoint.Key is "UNIT" or "STYP"?"PUT":"POST",records));
        }
        // Material/section/case definitions before element assignment, boundaries and loads.
        string[] first=["UNIT","STYP","PJCF","BNGR","LDGR","TDGR","MATL","IMFM","TDMT","TDME","TDMF","TMAT","SECT","THIK","GSTP","MLFC","NLLP","STLD","NODE","ELEM","STOR","GRUP","ELNK"];
        int Order(string e){int i=Array.IndexOf(first,e);return i<0?first.Length:i;}
        return writes.OrderBy(w=>Order(w.Endpoint)).ToArray();
    }
    public static string Canonical(MidasModel model)=>Native.Canonical(JsonSerializer.SerializeToElement(new{Product="MIDAS GEN NX",Version=1,model.Tolerance,Writes=Writes(model).OrderBy(w=>w.Endpoint).Select(w=>new{w.Endpoint,w.Records})}));
    public static MidasModel FromDatabase(IReadOnlyDictionary<string,JsonElement> tables,double tolerance=1e-6)
    {
        if(!tables.TryGetValue("UNIT",out var unit)||!tables.TryGetValue("NODE",out var nodeTable))throw new ArgumentException("Import needs UNIT and NODE tables.");
        var u=unit.GetProperty("1");var units=new Units(Native.String(u,"FORCE").ToUpperInvariant(),Native.String(u,"DIST").ToUpperInvariant(),Native.String(u,"HEAT","KJ").ToUpperInvariant(),Native.String(u,"TEMPER","C").ToUpperInvariant());
        var nodes=nodeTable.EnumerateObject().Select(n=>new Element(int.Parse(n.Name),ElementKind.Node,[new(Native.Number(n.Value,"X"),Native.Number(n.Value,"Y"),Native.Number(n.Value,"Z"))])).ToDictionary(n=>n.Id);
        var elements=new List<Element>(nodes.Values);var entries=new List<ApiEntry>();
        foreach(var table in tables)
        {
            if(table.Key is "UNIT" or "NODE")continue;
            foreach(var record in table.Value.EnumerateObject())
            {
                int id=int.Parse(record.Name);var data=record.Value;
                if(table.Key is "ELEM" or "ELNK")
                {
                    var kind=table.Key=="ELNK"?ElementKind.ElasticLink:Native.String(data,"TYPE") switch{"BEAM"=>ElementKind.Beam,"TRUSS"=>ElementKind.Truss,"TENSTR"=>ElementKind.Tension,"COMPTR"=>ElementKind.Compression,"PLATE"=>ElementKind.Plate,"WALL"=>ElementKind.Wall,"SOLID"=>ElementKind.Solid,_=>(ElementKind?)null};
                    if(kind.HasValue)
                    {
                        var ids=data.GetProperty("NODE").EnumerateArray().Select(n=>n.GetInt32()).Where(n=>n>0).ToArray();
                        if(ids.Any(n=>!nodes.ContainsKey(n)))throw new ArgumentException($"Element {id} references a missing node.");
                        elements.Add(new(id,kind.Value,ids.Select(n=>nodes[n].Points[0]).ToArray(),(int)Native.Number(data,"MATL"),(int)Native.Number(data,"SECT"),Native.Number(data,"ANGLE"),ids,data.GetRawText()));continue;
                    }
                }
                entries.Add(new(table.Key,id,data));
            }
        }
        return new(new(elements,entries),units,tolerance);
    }
}
public static class AtomicFile
{
    public static void Write(string path,string text,bool overwrite=false)
    {
        path=Path.GetFullPath(path);if(!overwrite&&File.Exists(path))throw new IOException("Destination exists: "+path);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{File.WriteAllText(temp,text,new UTF8Encoding(false));File.Move(temp,path,overwrite);}finally{if(File.Exists(temp))File.Delete(temp);}
    }
}
public static class ModelValidation
{
    public static string? TargetTable(string endpoint)=>endpoint switch
    {"CONS" or "NSPR" or "NSTM" or "NMAS" or "CNLD" or "SDSP" or "NTMP" or "NLSN" or "SKEW"=>"NODE","BMLD" or "PRES" or "ETMP" or "ELTM" or "FRLS" or "OFFS" or "BTMP"=>"ELEM",_=>null};
    public static IReadOnlyList<string> Errors(MidasModel model)
    {
        var errors=new List<string>();IReadOnlyList<DatabaseWrite> writes;
        try{writes=ApiModel.Writes(model);}catch(Exception ex){return [ex.Message];}
        var definitions=writes.ToDictionary(w=>w.Endpoint,w=>w.Records);
        bool Exists(string table,int id)=>definitions.TryGetValue(table,out var records)&&records.ContainsKey(id);
        foreach(var e in model.Definition.Elements)
        {
            if(e.Id<=0)errors.Add($"{e}: ID must be positive.");
            if(e.Points.Any(p=>!p.IsFinite)||!double.IsFinite(e.Angle))errors.Add($"{e}: nonfinite geometry.");
            int n=e.Points.Count;bool count=e.Kind switch{ElementKind.Node=>n==1,ElementKind.Plate=>n is 3 or 4,ElementKind.Wall=>n==4,ElementKind.Solid=>n is 4 or 6 or 8,_=>n==2};
            if(!count)errors.Add($"{e}: unsupported node count {n}.");
            if(e.Kind!=ElementKind.Node&&e.Kind!=ElementKind.ElasticLink&&e.NodeIds.Distinct().Count()!=n)errors.Add($"{e}: collapsed connectivity after welding.");
            if(n==2&&e.Kind!=ElementKind.ElasticLink&&e.Points[0].DistanceTo(e.Points[1])<=model.Tolerance)errors.Add($"{e}: zero-length member.");
            if(e.Kind is ElementKind.Plate or ElementKind.Wall&&n is 3 or 4)
            {
                var normal=Cross(Sub(e.Points[1],e.Points[0]),Sub(e.Points[2],e.Points[0]));double area2=Math.Sqrt(Dot(normal,normal));
                if(area2<=model.Tolerance*model.Tolerance)errors.Add($"{e}: degenerate plate.");
                else if(e.Points.Any(p=>Math.Abs(Dot(Sub(p,e.Points[0]),normal))>model.Tolerance*area2))errors.Add($"{e}: nonplanar plate. Split into planar elements.");
            }
            if(e.Kind==ElementKind.Solid&&n is 4 or 6 or 8)
            {
                int[][] tetra=n==4?[[0,1,2,3]]:n==6?[[0,1,2,3],[1,4,2,3],[2,4,5,3]]:[[0,1,3,4],[1,2,3,6],[1,3,4,6],[1,4,5,6],[3,4,6,7]];
                if(tetra.Any(t=>Dot(Sub(e.Points[t[1]],e.Points[t[0]]),Cross(Sub(e.Points[t[2]],e.Points[t[0]]),Sub(e.Points[t[3]],e.Points[t[0]])))<=Math.Pow(model.Tolerance,3)))errors.Add($"{e}: inverted or degenerate solid connectivity.");
            }
            if(e.Kind is ElementKind.Node or ElementKind.ElasticLink)continue;
            if(e.Kind==ElementKind.Wall)
            {
                try{var data=JsonSerializer.Deserialize<JsonElement>(e.Extra);if(Building.WallId(e)<=0||Native.Number(data,"STYPE") is not (1 or 2)||Native.Number(data,"W_CON",-1) is <0 or >2)errors.Add($"{e}: invalid WALL/STYPE/W_CON metadata.");}
                catch(Exception){errors.Add($"{e}: missing native wall metadata.");}
            }
            if(!Exists("MATL",e.Material))errors.Add($"{e}: missing material {e.Material}.");
            if(e.Kind!=ElementKind.Solid&&!Exists(e.Kind is ElementKind.Plate or ElementKind.Wall?"THIK":"SECT",e.Property))errors.Add($"{e}: missing property {e.Property}.");
        }
        var cases=definitions.TryGetValue("STLD",out var caseRecords)?caseRecords.Values.Select(c=>Native.String(c,"NAME")).ToHashSet():[];
        foreach(var write in writes)
        {
            string? target=TargetTable(write.Endpoint);
            foreach(var record in write.Records)
            {
                if(target!=null&&!Exists(target,record.Key))errors.Add($"{write.Endpoint}: missing target {target} {record.Key}.");
                IEnumerable<JsonElement> items=record.Value.TryGetProperty("ITEMS",out var itemArray)?itemArray.EnumerateArray().ToArray():[record.Value];
                foreach(var item in items)if(item.TryGetProperty("LCNAME",out var lc)&&!cases.Contains(lc.GetString()!))errors.Add("Undefined static load case: "+lc.GetString());
            }
        }
        return errors.Distinct().ToArray();
    }
    private static Position Sub(Position a,Position b)=>new(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
    private static Position Cross(Position a,Position b)=>new(a.Y*b.Z-a.Z*b.Y,a.Z*b.X-a.X*b.Z,a.X*b.Y-a.Y*b.X);
    private static double Dot(Position a,Position b)=>a.X*b.X+a.Y*b.Y+a.Z*b.Z;
    public static IReadOnlyList<string> Warnings(MidasModel model)=>model.Definition.Entries.Where(e=>e.Endpoint=="ELEM").Select(e=>"Native element "+e.Id+" preserved without geometric preview.").ToArray();
}
