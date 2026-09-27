using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Rhino2Midas.Core;
using Rhino2Midas.Api;

int count=0;
void Check(bool value,string name){if(!value)throw new Exception(name);Console.WriteLine("PASS "+name);count++;}
void Reject(Action action,string name){try{action();}catch(Exception){Check(true,name);return;}throw new Exception("Expected rejection: "+name);}
var example=Definitions.Example();
Check(ModelValidation.Errors(example).Count==0,"Cantilever model validation");
var writes=ApiModel.Writes(example);
Check(writes.First().Endpoint=="UNIT"&&writes.First().Method=="PUT","UNIT uses PUT before dependent records");
Check(writes.ToList().FindIndex(w=>w.Endpoint=="SECT")<writes.ToList().FindIndex(w=>w.Endpoint=="ELEM"),"Sections precede elements");
var elem=writes.Single(w=>w.Endpoint=="ELEM").Records[1];
Check(elem.GetProperty("NODE").EnumerateArray().Select(n=>n.GetInt32()).SequenceEqual(new[]{1,2}),"Explicit native node connectivity");
var original=example.Fingerprint;CultureInfo.CurrentCulture=new("it-IT");Check(example.Fingerprint==original,"Fingerprint independent of UI culture");
var archive=ModelArchive.Deserialize(ModelArchive.Serialize(example));Check(archive.Fingerprint==original,"Model archive roundtrip");
var material=Definitions.Material(1,"Steel",210000000,.3,78.5);var section=Definitions.Section(1,"Rect","SB",[.3,.2]);
var first=new Fragment([new(1,ElementKind.Beam,[new(0,0,0),new(1,0,0)],1,1)]);var second=new Fragment([new(2,ElementKind.Beam,[new(1+1e-7,0,0),new(2,0,0)],1,1)]);
var welded=new MidasModel(Fragment.Combine([material,section,first,second]));Check(welded.Definition.Elements.Count(e=>e.Kind==ElementKind.Node)==3,"Implicit node welding across member fragments");
Check(first.Elements[0].NodeIds.Count==0,"Input fragment unchanged by Build Model");
Reject(()=>new MidasModel(new([new(1,ElementKind.Node,[new(0,0,0)]),new(2,ElementKind.Node,[new(0,0,0)]),new(1,ElementKind.Beam,[new(0,0,0),new(1,0,0)],1,1)])),"Ambiguous coincident nodes require explicit connectivity");
var coincident=new MidasModel(new([new(1,ElementKind.Node,[new(0,0,0)]),new(2,ElementKind.Node,[new(0,0,0)]),new(4,ElementKind.ElasticLink,[new(0,0,0),new(0,0,0)],nodeIds:[1,2],extra:"{\"LINK\":\"RIGID\"}")]));Check(coincident.Definition.Elements.Count==3,"Explicit coincident link nodes preserved");
var load=ApiEntry.Create("CNLD",2,new{LCNAME="LC1",FZ=-10},true);
var shared=new MidasModel(Fragment.Combine([example.Definition,new(entries:[load]),new(entries:[load])]));
Check(ApiModel.Writes(shared).Single(w=>w.Endpoint=="CNLD").Records[2].GetProperty("ITEMS").GetArrayLength()==2,"Shared load dependency collected once");
var independent=ApiEntry.Create("CNLD",2,new{LCNAME="LC1",FZ=-10},true);
var added=new MidasModel(Fragment.Combine([example.Definition,new(entries:[load,independent])]));
Check(ApiModel.Writes(added).Single(w=>w.Endpoint=="CNLD").Records[2].GetProperty("ITEMS").GetArrayLength()==3,"Independent identical contributions remain additive");
var conflict=new MidasModel(Fragment.Combine([example.Definition,Definitions.Section(1,"Other","SB",[.4,.2])]));Check(ModelValidation.Errors(conflict).Any(e=>e.Contains("Conflicting")),"Conflicting property IDs rejected");
Reject(()=>Definitions.Section(1,"Bad","P",[.2,.11]),"Impossible hollow section rejected");
Reject(()=>Definitions.Material(1,"Bad",0,.3,78),"Nonpositive elastic modulus rejected");
Reject(()=>new Units("BAD").Validate(),"Invalid units rejected");
Reject(()=>MidasModel.Merge([example,new(example.Definition,new Units("N","MM"))]),"Merge cannot mix units");
var db=writes.ToDictionary(w=>w.Endpoint,w=>JsonSerializer.SerializeToElement(w.Records.ToDictionary(p=>p.Key.ToString(),p=>p.Value)));
var imported=ApiModel.FromDatabase(db);Check(imported.Fingerprint==original,"Direct API database import/export equivalence");
using var response=JsonDocument.Parse("{\"BeamForce\":{\"HEAD\":[\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Moment-y\"],\"DATA\":[[\"1\",\"1\",\"LC1\",\"I[1]\",\"10\",\"-30\"],[\"2\",\"1\",\"LC1\",\"J[2]\",\"10\",\"0\"]]}}");
var tables=ResultTable.ParseResponse(response.RootElement,"BEAMFORCE");var solved=example.WithResults(new(original,example.Units,"fixture.mcb","fixture",true,tables,[]));
Check(solved.Definition.Elements.Single(e=>e.Kind==ElementKind.Beam).Results[0].Numbers("Axial").SequenceEqual(new[]{10d,10d}),"Results embedded in each element by native ID");
Check(example.Results==null&&example.Definition.Elements.All(e=>e.Results.Count==0),"Attaching results leaves input model unchanged");
Check(ModelArchive.Deserialize(ModelArchive.Serialize(solved)).Definition.Elements.Single(e=>e.Kind==ElementKind.Beam).Results.Count==1,"Embedded results survive archive serialization");
Reject(()=>welded.WithResults(solved.Results!),"Stale results fingerprint rejected");
Reject(()=>new ResultTable("bad","BEAMFORCE",["Elem"],[new[]{"1","2"}]),"Malformed result row rejected");
var sources=new List<ResultTable>(tables);var immutable=new ResultSet(original,example.Units,"p","h",true,sources,[]);sources.Clear();Check(immutable.Tables.Count==1,"Result set snapshots input lists");
var request=TableRequest.Beam([1],["LC1(ST)"]);var requestJson=JsonSerializer.SerializeToElement(request.Body(example.Units));Check(requestJson.GetProperty("Argument").GetProperty("TABLE_TYPE").GetString()=="BEAMFORCE","Native result request contract");
Check(!requestJson.GetProperty("Argument").TryGetProperty("EXPORT_PATH",out _),"Results return through API without intermediate files");
Reject(()=>new CivilClient("http://example.com/civil","key"),"Remote HTTP cannot expose MAPI-Key");
Reject(()=>new CivilClient("https://example.com/civil",""),"Missing credentials fail before network");
var work=Path.Combine(Path.GetTempPath(),"Rhino2Midas-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(work);
try
{
    string path=Path.Combine(work,"test.mcb");var handler=new FakeCivilHandler();using var client=new CivilClient("https://mock.invalid/civil/","test-secret",handler);
    try{await CivilService.ExecuteAsync(client,example,new(path));throw new Exception("Missing active document guard");}catch(ArgumentException){Check(handler.Calls.Count==0,"Replace-active guard precedes all mutations");}
    var run=await CivilService.ExecuteAsync(client,example,new(path,true,true,false,[request]));
    Check(handler.Calls[0]=="POST /civil/doc/NEW"&&handler.Calls.Contains("POST /civil/db/ELEM"),"Model created directly using Civil database endpoints");
    Check(handler.Calls.All(c=>!c.Contains("MXT")&&!c.Contains("IMPORT")),"Execution never uses MCT/MGT import");
    Check(handler.Calls.FindIndex(c=>c.EndsWith("doc/ANAL"))>handler.Calls.FindIndex(c=>c.EndsWith("doc/SAVEAS")),"Analyze follows file creation");
    Check(run.Results?.AnalysisCompleted==true&&File.Exists(path+".results.json"),"Results captured and archived after analysis");
    Check(ResultSet.Load(path+".results.json",example).Tables.Count==1,"Result archive verifies source file hash");
    int callCount=handler.Calls.Count;
    try{await CivilService.ExecuteAsync(client,example,new(path,true,true));throw new Exception("Overwrite guard missing");}catch(IOException){Check(handler.Calls.Count==callCount,"Overwrite guard checked before Civil document replacement");}
    File.AppendAllText(path,"changed");Reject(()=>ResultSet.Load(path+".results.json",example),"Changed Civil file invalidates archived results");
    var failing=new FakeCivilHandler{FailAt="db/MATL"};using var failClient=new CivilClient("https://mock.invalid/civil/","test-secret",failing);
    try{await CivilService.ExecuteAsync(failClient,example,new(Path.Combine(work,"failed.mcb"),false,true));throw new Exception("Expected API failure");}catch(InvalidOperationException ex){Check(!ex.Message.Contains("test-secret"),"API errors redact MAPI-Key");}
    Check(!failing.Calls.Any(c=>c.EndsWith("doc/ANAL")||c.EndsWith("doc/SAVEAS")),"Failed material upload prevents save/analysis");
}
finally{Directory.Delete(work,true);}
Check(ApiCatalogue.Definitions.Count>=300,"Official native API catalogue embedded");
var millimetres=new MidasModel(example.Definition,new Units("N","MM"));Check(Math.Abs(ApiModel.Writes(millimetres).Single(w=>w.Endpoint=="STYP").Records[1].GetProperty("GRAV").GetDouble()-9806)<1e-6,"Default gravity follows model length units");
var beam=example.Definition.Elements.Single(e=>e.Kind==ElementKind.Beam);Check(ModelValidation.Errors(new MidasModel(example.Definition.ForElement(beam))).Count==0,"Extracted member includes its explicit connectivity and assignments");
var importedLoaded=new MidasModel(imported.Definition.Append(ApiEntry.Create("CNLD",2,new{LCNAME="LC1",FZ=-5},true)));
Check(ApiModel.Writes(importedLoaded).Single(w=>w.Endpoint=="CNLD").Records[2].GetProperty("ITEMS").GetArrayLength()==2,"Imported ITEMS can receive new independent loads");
var link=new Element(1,ElementKind.ElasticLink,[new(0,0,0),new(3,0,0)],nodeIds:[1,2],extra:"{\"LINK\":\"RIGID\"}");
var withLink=new MidasModel(new(example.Definition.Elements.Append(link).ToArray(),example.Definition.Entries));Check(ApiModel.Writes(withLink).Any(w=>w.Endpoint=="ELNK")&&withLink.Definition.Elements.Count==4,"Elastic links and beam IDs use separate namespaces");
var linkResult=new ResultTable("ElasticLink","ELASTICLINK",["No.","Load","Axial"],[["1","LC1","42"]]);
Check(linkResult.ForElement(link).Rows.Count==1&&linkResult.ForElement(beam).Rows.Count==0,"Elastic link result IDs never attach to a beam with the same ID");
var plate=new Element(2,ElementKind.Plate,[new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0)],1,1);
var plateModel=new MidasModel(Fragment.Combine([example.Definition,Definitions.Thickness(1,.2),new Fragment([plate])]));
Check(CivilService.DefaultRequests(plateModel).Any(q=>q.Type=="PLATEFORCEUL"),"Default plate requests match Plate Actions unit-length quantities");
var plateRequest=CivilService.DefaultRequests(plateModel).Single(q=>q.Type=="PLATEFORCEUL");Check(!JsonSerializer.SerializeToElement(plateRequest.Body(plateModel.Units)).GetProperty("Argument").GetProperty("AVERAGE_NODAL_RESULT").GetBoolean(),"Default plate results retain unsmoothed element values");
var warped=new MidasModel(Fragment.Combine([material,Definitions.Thickness(1,.2),new Fragment([plate with{Points=new Position[]{new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,.1)}}])]));Check(ModelValidation.Errors(warped).Any(e=>e.Contains("nonplanar")),"Nonplanar plate fails before API calls");
var inverted=new MidasModel(Fragment.Combine([material,new Fragment([new(1,ElementKind.Solid,[new(0,0,0),new(0,1,0),new(1,0,0),new(0,0,1)],1)])]));Check(ModelValidation.Errors(inverted).Any(e=>e.Contains("inverted")),"Inverted tetrahedron fails before API calls");
var combined=new MidasModel(Fragment.Combine([example.Definition,Definitions.Combination(1,"ULS",["LC1"],[1.5])]));Check(CivilService.DefaultRequests(combined)[0].Cases.Contains("ULS(CB)"),"Default requests include active additive general combinations");
Console.WriteLine($"{count} managed checks passed.");
if(args.Contains("--native"))
{
    if(!args.Contains("--replace-active-document"))throw new ArgumentException("Native smoke replaces Civil's active document. Save Civil work and pass --replace-active-document explicitly.");
    string path=Path.Combine(Path.GetTempPath(),"Rhino2Midas-native-"+Guid.NewGuid().ToString("N"),"Cantilever.mcb");
    using var client=CivilClient.FromEnvironment();var model=await CivilService.ExecuteAsync(client,example,new(path,true,true));
    Check(model.Results!.Issues.Count==0,"Native default result requests return without issues");
    var reaction=model.Results.Tables.Single(t=>t.Type=="REACTIONG").Filter("Node","1");
    Check(reaction.Numbers("FZ").Any(v=>Math.Abs(v-10)<1e-5),"Native cantilever vertical equilibrium: 10 kN");
    Check(reaction.Numbers("MY").Any(v=>Math.Abs(Math.Abs(v)-30)<1e-5),"Native cantilever moment equilibrium: 30 kNm");
    var displacement=model.Results.Tables.Single(t=>t.Type=="DISPLACEMENTG").Filter("Node","2");
    Check(displacement.Numbers("DZ").Any(v=>v<-.00090&&v>-.00105),"Native cantilever displacement, including shear flexibility");
    Console.WriteLine("Native Civil smoke passed. Files: "+path);
}

sealed class FakeCivilHandler:HttpMessageHandler
{
    public List<string> Calls{get;}=[];public string? FailAt{get;init;}
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
    {
        string path=request.RequestUri!.AbsolutePath;Calls.Add(request.Method+" "+path);string body=request.Content==null?"{}":await request.Content.ReadAsStringAsync(token);
        if(path.EndsWith(FailAt??"never"))return Json("{\"error\":{\"message\":\"test-secret failed\"}}");
        if(path.EndsWith("doc/SAVEAS")){using var doc=JsonDocument.Parse(body);File.WriteAllText(doc.RootElement.GetProperty("Argument").GetString()!,"fake civil binary");}
        if(path.EndsWith("post/TABLE"))return Json("{\"BeamForce\":{\"HEAD\":[\"Elem\",\"Load\",\"Part\",\"Axial\"],\"DATA\":[[\"1\",\"LC1\",\"I[1]\",\"10\"],[\"1\",\"LC1\",\"J[2]\",\"10\"]]}}");
        return Json("{\"message\":\"OK\"}");
    }
    private static HttpResponseMessage Json(string value)=>new(HttpStatusCode.OK){Content=new StringContent(value,Encoding.UTF8,"application/json")};
}
