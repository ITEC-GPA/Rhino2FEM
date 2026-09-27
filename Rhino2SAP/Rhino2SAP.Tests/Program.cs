using System.Reflection;
using System.Text.Json;
using Rhino2SAP.Core;
using Rhino2SAP.Api;

int passed=0;
void Check(bool value,string name){if(!value)throw new Exception("FAIL "+name);passed++;Console.WriteLine("PASS "+name);}
void Reject(Action action,string name){try{action();}catch{Check(true,name);return;}throw new Exception("FAIL "+name+" (not rejected)");}
var model=StandardDefinitions.AxialExample();
Check(ModelValidation.Errors(model).Count==0,"Axial example validates against installed SDK schema");
Check(model.Fingerprint==new SapModel(model.Definition).Fingerprint,"Stable model fingerprint");
Check(ModelArchive.Deserialize(ModelArchive.Serialize(model)).Fingerprint==model.Fingerprint,"Model definition archive round trip");
var force=Operation.Create("PointObj.SetLoadForce",("Name","TIP"),("LoadPat","AXIAL"),("Value",new[]{2d,0,0,0,0,0}));
var shared=new Fragment([force]);var merged=new SapModel(Fragment.Combine([shared,shared]));
Check(merged.Definition.Operations.Count==1,"Shared load dependency is not applied twice");
var second=Operation.Create("PointObj.SetLoadForce",("Name","TIP"),("LoadPat","AXIAL"),("Value",new[]{2d,0,0,0,0,0}));
Check(new SapModel(Fragment.Combine([shared,new([second])])).Definition.Operations.Count==2,"Independent equal load contributions remain additive");
Check(SapModel.Merge([model,model]).Definition.Elements.Count==3,"Merge deduplicates identical geometry");
Reject(()=>SapModel.Merge([model,new(model.Definition,9)]),"Merge rejects unit mismatch");
Reject(()=>new SapModel(new(elements:[new("F",ElementKind.Frame,[new(0,0,0),new(1,0,0)],"A"),new("F",ElementKind.Frame,[new(0,0,0),new(2,0,0)],"A")])),"Conflicting object names rejected");
Reject(()=>Operation.Create("PropFrame.SetRectangle",("Name","R"),("MatProp","M"),("typo",1d)),"Incorrect SDK parameter rejected");
Reject(()=>Operation.Create("PropMaterial.SetMaterial",("Name","M"),("MatType",1234)),"Invalid enum rejected");
Reject(()=>StandardDefinitions.Material("Bad",1,1,.6,0,0),"Invalid elastic material rejected");
Check(ModelValidation.Errors(new SapModel(new(elements:[new("A",ElementKind.Area,[new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,.1)],"P")]))).Count>0,"Nonplanar area rejected");
Check(ModelValidation.Errors(new SapModel(new([Operation.Create("PointObj.SetRestraint",("Name","N"),("Value",new[]{true,false}))]))).Count>0,"DOF array length checked");
var recorder=new RecordingClient();var nodes=ModelWriter.Write(recorder,model);
Check(nodes.Count==2,"Frame endpoints weld to explicit nodes");
Check(recorder.Calls.FindIndex(c=>c.Key=="PropMaterial.SetMaterial")<recorder.Calls.FindIndex(c=>c.Key=="PropFrame.SetRectangle"),"Materials exported before sections");
Check(recorder.Calls.FindIndex(c=>c.Key=="FrameObj.AddByPoint")<recorder.Calls.FindIndex(c=>c.Key=="PointObj.SetLoadForce"),"Objects exported before assignments");
Check(recorder.Calls.Single(c=>c.Key=="FrameObj.AddByPoint").Arguments!["Point2"].GetString()=="TIP","Frame connectivity uses actual SAP names");
var table=new ResultTable("Results.JointDispl",ApiSchema.Args(("Obj",new[]{"TIP"}),("U1",new[]{3/(210e6*.02)})));
string scratch=Path.Combine(Environment.CurrentDirectory,".local","tests");Directory.CreateDirectory(scratch);string dummy=Path.Combine(scratch,"dummy.sdb");File.WriteAllText(dummy,"test fixture");
var results=new ResultSet(model.Fingerprint,dummy,AnalysisService.Hash(dummy),true,[table],[]);
Check(model.WithResults(results).Results!.IsComplete,"Results attached to identical Model");
Reject(()=>new SapModel(model.Definition,9,1e-6,results),"Stale results rejected after units change");
ResultArchive.Save(dummy+".results.json",results);var read=ResultArchive.Load(dummy+".results.json",model);
Check(read.Tables[0].Columns["U1"][0].GetDouble()==table.Columns["U1"][0].GetDouble(),"Result archive round trip");
var plate=new Element("A1",ElementKind.Area,[new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0)],"Shell");
var mixed=new SapModel(Fragment.Combine([model.Definition,new Fragment(elements:[plate,new("F2",ElementKind.Frame,[new(0,2,0),new(3,2,0)],"R")])]));
var beamTable=new ResultTable("Results.FrameForce",ApiSchema.Args(("NumberResults",3),("Obj",new[]{"F1","F2","F1"}),("LoadCase",new[]{"AXIAL","AXIAL","COMBO"}),("P",new[]{1d,2d,3d}),("ObjSta",new[]{0d,0d,3d})));
var plateTable=new ResultTable("Results.AreaForceShell",ApiSchema.Args(("NumberResults",2),("Obj",new[]{"A1","A2"}),("LoadCase",new[]{"AXIAL","AXIAL"}),("F11",new[]{10d,99d}),("M11",new[]{20d,99d})));
var strainTable=new ResultTable("Results.AreaStrainShell",ApiSchema.Args(("NumberResults",1),("obj",new[]{"A1"}),("e11top",new[]{.001})));
var mixedResults=new ResultSet(mixed.Fingerprint,dummy,AnalysisService.Hash(dummy),true,[beamTable,plateTable,strainTable],[]);
var solvedMixed=mixed.WithResults(mixedResults);
var beam=solvedMixed.Definition.Elements.Single(e=>e.Name=="F1");var solvedPlate=solvedMixed.Definition.Elements.Single(e=>e.Name=="A1");
Check(beam.Results!.Tables.Single().Columns["P"].EnumerateArray().Select(x=>x.GetDouble()).SequenceEqual(new[]{1d,3d}),"Beam embeds only its own stations/cases");
Check(solvedPlate.Results!.Tables.Single(t=>t.Method=="Results.AreaForceShell").Columns["F11"][0].GetDouble()==10,"Plate embeds only its own shell forces");
Check(solvedPlate.Results.Tables.Single(t=>t.Method=="Results.AreaStrainShell").RowCount==1,"Lower-case SDK obj column maps shell strains correctly");
Check(beam.Results.Tables[0].Columns["NumberResults"].GetInt32()==2,"Filtered native row count is preserved");
Check(ModelArchive.Deserialize(ModelArchive.Serialize(solvedMixed)).Definition.Elements.Single(e=>e.Name=="F1").Results!.Tables[0].RowCount==2,"Solved Model round trip reattaches element results");
Check(ModelArchive.DeserializeFragment(ModelArchive.SerializeFragment(new Fragment(elements:[beam]))).Elements[0].Results!.Tables[0].RowCount==2,"Extracted beam round trip preserves embedded results");
Check(solvedMixed.WithoutResults().Definition.Elements.All(e=>e.Results==null),"Clearing Model results also clears every element");
Check(new SapModel(solvedMixed.Definition).Definition.Elements.All(e=>e.Results==null),"Rebuilding Model invalidates embedded results");
Check(new Fragment(elements:[beam]).Append(force).Elements.Single().Results==null,"Element assignment clears stale embedded results");
Check(mixed.Definition.Elements.Count(e=>e.Kind==ElementKind.Node)==7,"Build includes implicit connectivity nodes for decomposition");
File.WriteAllText(dummy,"changed");Reject(()=>ResultArchive.Load(dummy+".results.json",model),"Changed .sdb invalidates archive provenance");
var sdk=Assembly.LoadFrom(Path.Combine(SapClient.FindInstallation(),"SAP2000v1.dll"));
foreach(var method in ApiSchema.Methods.Values)
{
    var type=sdk.GetType("SAP2000v1.cSapModel")!;
    foreach(var part in method.Path.Split('.',StringSplitOptions.RemoveEmptyEntries))type=type.GetProperty(part)!.PropertyType;
    var actual=type.GetMethod(method.Method)??throw new Exception(method.Key);
    var parameters=actual.GetParameters();
    if(parameters.Length!=method.Parameters.Length||parameters.Where((p,i)=>p.Name!=method.Parameters[i].Name).Any())throw new Exception("SDK drift: "+method.Key);
}
Check(true,$"All {ApiSchema.Methods.Count} schema methods match local SDK");
if(args.Contains("--native"))
{
    SapClient.Trace=Console.WriteLine;
    var native=SapThread.Run(()=>new AnalysisService().Execute(model,new RunOptions(Path.Combine(scratch,"axial.sdb"),true,true,["AXIAL"],[],[ResultRequest.All("Results.JointDispl"),ResultRequest.All("Results.JointReact"),ResultRequest.All("Results.FrameForce")])));
    Check(native.Results!.Issues.Count==0,"Native SAP result capture reports no issues");
    var displacement=native.Results.Tables.Single(t=>t.Method=="Results.JointDispl");int tip=Array.FindIndex(displacement.Columns["Obj"].EnumerateArray().Select(x=>x.GetString()).ToArray(),x=>x=="TIP");
    double ux=displacement.Columns["U1"][tip].GetDouble();double expected=3/(210e6*.02);
    Check(Math.Abs(ux-expected)<Math.Abs(expected)*1e-5,$"Native axial displacement {ux:G12} matches PL/EA {expected:G12}");
    var reactions=native.Results.Tables.Single(t=>t.Method=="Results.JointReact");double total=reactions.Columns["F1"].EnumerateArray().Sum(x=>x.GetDouble());
    Check(Math.Abs(total+1)<1e-6,"Native support reaction balances 1 kN load");
}
if(args.Contains("--worker-native"))
{
    var request=new WorkerRequest("Run",ModelArchive.Serialize(model),new RunOptions(Path.Combine(scratch,"worker-axial.sdb"),true,true,["AXIAL"],[],[ResultRequest.All("Results.JointDispl"),ResultRequest.All("Results.JointReact")]));
    var solved=ModelArchive.Deserialize(WorkerClient.Invoke(request,45,120));
    var displ=solved.Results!.Tables.Single(t=>t.Method=="Results.JointDispl");
    int row=Enumerable.Range(0,displ.RowCount).Single(i=>displ.Columns["Obj"][i].GetString()=="TIP");
    Check(Math.Abs(displ.Columns["U1"][row].GetDouble()-3/(210e6*.02))<1e-11,"Isolated worker native analysis matches PL/EA");
}
Console.WriteLine($"{passed} checks passed.");

sealed class RecordingClient:ISapClient
{
    public List<(string Key,IReadOnlyDictionary<string,JsonElement>? Arguments)> Calls{get;}=[];
    public IReadOnlyDictionary<string,JsonElement> Call(string method,IReadOnlyDictionary<string,JsonElement>? arguments=null)
    {
        Calls.Add((method,arguments));
        // Check required by-value inputs and all explicitly provided values, including connectivity.
        ApiSchema.ValidateArguments(method,arguments??ApiSchema.Args(),true);
        if(method.Contains(".AddBy")||method=="PointObj.AddCartesian")return ApiSchema.Args(("Name",arguments!["UserName"].GetString()));
        return ApiSchema.Args();
    }
    public void Dispose(){}
}
