using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Rhino2MidasGen.Core;

namespace Rhino2MidasGen.Api;

public sealed class GenClient:IDisposable
{
    private readonly HttpClient http;
    private readonly string key;
    public GenClient(string baseUrl,string apiKey,HttpMessageHandler? handler=null,TimeSpan? timeout=null)
    {
        if(!Uri.TryCreate(baseUrl.TrimEnd('/')+"/",UriKind.Absolute,out var uri)||uri.Scheme is not ("http" or "https")||uri.UserInfo.Length>0||uri.Query.Length>0||uri.Fragment.Length>0)throw new ArgumentException("Use the Base URL shown in GEN NX API Settings.");
        if(uri.Scheme=="http"&&!uri.IsLoopback)throw new ArgumentException("Use HTTPS for remote MIDAS API connections.");
        if(uri.AbsolutePath.Split('/').Contains("civil",StringComparer.OrdinalIgnoreCase))throw new ArgumentException("This is a CIVIL connection. Copy the Base URL from GEN NX API Settings.");
        if(string.IsNullOrWhiteSpace(apiKey))throw new ArgumentException("Set MIDAS_GEN_API_KEY to the MAPI-Key shown in GEN NX API Settings.");
        key=apiKey;http=new(handler??new HttpClientHandler{AllowAutoRedirect=false}){BaseAddress=uri,Timeout=timeout??TimeSpan.FromMinutes(30)};
        http.DefaultRequestHeaders.Add("MAPI-Key",apiKey);http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    public static GenClient FromEnvironment(string? baseUrl=null)=>new(baseUrl??Environment.GetEnvironmentVariable("MIDAS_GEN_API_URL")??throw new ArgumentException("Set MIDAS_GEN_API_URL to GEN NX Base URL."),Environment.GetEnvironmentVariable("MIDAS_GEN_API_KEY")??"");
    public async Task<JsonElement> SendAsync(string method,string path,object? body=null,CancellationToken cancel=default)
    {
        if(!System.Text.RegularExpressions.Regex.IsMatch(path,@"^(db/[A-Za-z0-9_-]+|doc/(NEW|SAVE|SAVEAS|ANAL)|ope/PROJECTSTATUS|post/TABLE)$"))throw new ArgumentException("Unsupported GEN API path.");
        if(path.StartsWith("db/",StringComparison.Ordinal))path=ApiCatalogue.Definitions.Values.FirstOrDefault(d=>d.Endpoint.Equals(path,StringComparison.OrdinalIgnoreCase))?.Endpoint??path;
        using var request=new HttpRequestMessage(new HttpMethod(method),path);
        if(body!=null)request.Content=new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json");
        HttpResponseMessage response;
        try{response=await http.SendAsync(request,cancel).ConfigureAwait(false);}catch(TaskCanceledException){throw new TimeoutException($"Midas GEN {path} timed out. Check GEN NX before retrying; the native operation may still be running.");}
        using(response)
        {
            string text=await response.Content.ReadAsStringAsync(cancel).ConfigureAwait(false);
            string Clean(string s)=>s.Replace(key,"[redacted]");
            if(!response.IsSuccessStatusCode)throw new HttpRequestException(Clean($"Midas GEN {path}: HTTP {(int)response.StatusCode}. {text[..Math.Min(text.Length,500)]}"),null,response.StatusCode);
            JsonElement json;try{using var doc=JsonDocument.Parse(text);json=doc.RootElement.Clone();}catch(JsonException){throw new InvalidOperationException("Midas GEN returned non-JSON data for "+path);}
            if(json.ValueKind!=JsonValueKind.Object)throw new InvalidOperationException("Expected a GEN JSON object for "+path);
            if(json.EnumerateObject().Any(p=>p.Name.Equals("error",StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException(Clean($"Midas GEN {path}: {text[..Math.Min(text.Length,800)]}"));
            return json;
        }
    }
    public async Task<JsonElement> ReadDatabaseAsync(string endpoint,CancellationToken cancel=default)
    {
        var json=await SendAsync("GET","db/"+endpoint,null,cancel).ConfigureAwait(false);
        var data=json.EnumerateObject().FirstOrDefault(p=>p.Name.Equals(endpoint,StringComparison.OrdinalIgnoreCase)).Value;
        if(data.ValueKind!=JsonValueKind.Object)throw new InvalidOperationException("Missing database table "+endpoint);
        return data.Clone();
    }
    public void Dispose()=>http.Dispose();
}
public sealed record RunOptions(string Path,bool Analyze=false,bool ReplaceActiveDocument=false,bool Overwrite=false,IReadOnlyList<TableRequest>? Requests=null);
public static class GenService
{
    public static async Task<MidasModel> ExecuteAsync(GenClient client,MidasModel model,RunOptions options,CancellationToken cancel=default)
    {
        var errors=ModelValidation.Errors(model);if(errors.Count>0)throw new ArgumentException(string.Join("\n",errors));
        if(!options.ReplaceActiveDocument)throw new ArgumentException("GEN API controls the active document. Save your work in GEN and enable Replace active document to create this Model.");
        string path=System.IO.Path.GetFullPath(options.Path);
        if(!new[]{".mgb",".mgbx"}.Contains(System.IO.Path.GetExtension(path).ToLowerInvariant()))throw new ArgumentException("Use a GEN NX .mgb or .mgbx destination.");
        if(!options.Overwrite&&(File.Exists(path)||File.Exists(path+".model.json")||File.Exists(path+".results.json")))throw new IOException("Destination or sidecar already exists. Enable Overwrite explicitly.");
        var writes=ApiModel.Writes(model);var requests=options.Requests?.ToArray()??DefaultRequests(model);
        if(options.Analyze){if(requests.Length==0)throw new ArgumentException("Define static cases or supply result requests for the native analysis cases.");foreach(var r in requests)_=r.Body(model.Units);}
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        await client.SendAsync("POST","doc/NEW",new{},cancel).ConfigureAwait(false);
        foreach(var write in writes)await client.SendAsync(write.Method,"db/"+write.Endpoint,write.Body(),cancel).ConfigureAwait(false);
        await client.SendAsync("POST","doc/SAVEAS",new{Argument=path},cancel).ConfigureAwait(false);
        if(!File.Exists(path))throw new IOException("GEN did not create the requested file locally. This workflow requires GEN NX on the same computer.");
        var clean=model.ClearResults();
        if(options.Analyze)
        {
            await client.SendAsync("POST","doc/ANAL",new{},cancel).ConfigureAwait(false);
            var tables=new List<ResultTable>();var issues=new List<string>();
            foreach(var request in requests)
            {
                // Errors are recorded per quantity; successful tables remain available with explicit issues.
                try{var response=await client.SendAsync("POST","post/TABLE",request.Body(model.Units),cancel).ConfigureAwait(false);var parsed=ResultTable.ParseResponse(response,request.Type);tables.AddRange(parsed);if(parsed.All(t=>t.Rows.Count==0))issues.Add(request.Type+": no result rows returned.");}
                catch(Exception ex) when(ex is InvalidOperationException or FormatException or HttpRequestException){issues.Add(request.Type+": "+ex.Message);}
            }
            if(tables.All(t=>t.Rows.Count==0))throw new InvalidOperationException("Analysis returned no usable result data. "+string.Join("; ",issues));
            await client.SendAsync("POST","doc/SAVE",new{},cancel).ConfigureAwait(false);
            var results=new ResultSet(model.Fingerprint,model.Units,path,ResultSet.FileHash(path),true,tables.AsReadOnly(),issues.AsReadOnly());
            clean=model.WithResults(results);results.Save(path+".results.json",options.Overwrite);
        }
        else if(options.Overwrite&&File.Exists(path+".results.json"))File.Delete(path+".results.json");
        AtomicFile.Write(path+".model.json",ModelArchive.Serialize(clean),options.Overwrite);return clean;
    }
    public static TableRequest[] DefaultRequests(MidasModel model)
    {
        var cases=model.Definition.Entries.Where(e=>e.Endpoint=="STLD").Select(e=>Native.String(e.Data,"NAME")+"(ST)")
            .Concat(model.Definition.Entries.Where(e=>e.Endpoint=="LCOM-GEN"&&Native.Number(e.Data,"iTYPE")==0&&Native.String(e.Data,"ACTIVE")=="ACTIVE").Select(e=>Native.String(e.Data,"NAME")+"(CB)")).Distinct().ToArray();if(cases.Length==0)return [];
        var nodes=model.Definition.Elements.Where(e=>e.Kind==ElementKind.Node).Select(e=>e.Id).ToArray();var beams=model.Definition.Elements.Where(e=>e.Kind==ElementKind.Beam).Select(e=>e.Id).ToArray();
        var requests=new List<TableRequest>{TableRequest.Displacement(nodes,cases),TableRequest.Reaction(nodes,cases)};
        if(beams.Length>0)requests.Add(TableRequest.Beam(beams,cases));
        foreach(var group in model.Definition.Elements.GroupBy(e=>e.Kind))
        {
            string[] types=group.Key switch{ElementKind.Plate=>["PLATEFORCEUL","PLATESTRESSL"],ElementKind.Solid=>["SOLIDSL"],ElementKind.Truss or ElementKind.Tension or ElementKind.Compression=>["TRUSSFORCE","TRUSSSTRESS"],ElementKind.ElasticLink=>["ELASTICLINK"],_=>[]};
            foreach(string type in types)requests.Add(TableRequest.FromTemplate(type,group.Select(e=>e.Id),cases));
            if(group.Key==ElementKind.Wall)requests.Add(TableRequest.FromTemplate("WALL_FORCE_MOMENT",[],cases));
        }
        return requests.ToArray();
    }
    /// <summary>Read a useful model snapshot using GET only; report optional tables that are unavailable.</summary>
    public static async Task<ModelReadResult> ReadExistingModelAsync(GenClient client,IReadOnlyList<string> additional,CancellationToken cancel=default)
    {
        var tables=new Dictionary<string,JsonElement>(StringComparer.OrdinalIgnoreCase);
        var issues=new List<string>();
        foreach(string name in new[]{"UNIT","NODE","ELEM"})
            tables[name]=await client.ReadDatabaseAsync(name,cancel).ConfigureAwait(false);
        string[] standard=["STYP","MATL","SECT","THIK","STLD","CONS","NSPR","NMAS","CNLD","BMLD","PRES","SELF","LCOM-GEN","FRLS","OFFS","ELNK","GRUP","BNGR","LDGR","STOR"];
        foreach(string name in standard.Concat(additional.Select(s=>s.Trim().ToUpperInvariant())).Where(s=>s.Length>0).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if(tables.ContainsKey(name))continue;
            try { tables[name]=await client.ReadDatabaseAsync(name,cancel).ConfigureAwait(false); }
            catch(HttpRequestException ex) when(ex.StatusCode is not (System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden) && ex.StatusCode.HasValue)
            { issues.Add(name+": table not imported: "+ex.Message); }
            catch(InvalidOperationException ex) { issues.Add(name+": table not imported: "+ex.Message); }
        }
        var model=ApiModel.FromDatabase(tables);
        issues.AddRange(ModelValidation.Errors(model).Concat(ModelValidation.Warnings(model)));
        issues.Add("Snapshot of geometry and the listed database tables; add product-specific tables through Tables. Native result tables are read separately. Review Issues before re-exporting.");
        return new(model,issues.ToArray(),tables.Keys.ToArray());
    }
    public static async Task<MidasModel> ReadModelAsync(GenClient client,IReadOnlyList<string> endpoints,CancellationToken cancel=default)
    {
        var data=new Dictionary<string,JsonElement>();foreach(var endpoint in new[]{"UNIT","NODE","ELEM"}.Concat(endpoints).Distinct())data[endpoint]=await client.ReadDatabaseAsync(endpoint,cancel).ConfigureAwait(false);
        return ApiModel.FromDatabase(data);
    }
}

public sealed record ModelReadResult(MidasModel Model,string[] Issues,string[] Tables);
