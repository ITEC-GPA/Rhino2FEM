using System.Text.Json;
using Rhino2SAP.Api;
using Rhino2SAP.Core;

internal static class ImportChecks
{
    public static void Run(Action<bool,string> check,Action<Action,string> reject,string scratch)
    {
        string folder=Path.Combine(scratch,"import-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        string source=Path.Combine(folder,"source.sdb");File.WriteAllText(source,"Native SDB stand-in for recorded API tests, not a solver file.");
        var recorder=new ImportRecordingClient();
        var imported=new ModelImportService(()=>recorder).Read(source);
        check(imported.NativeSource!=null&&imported.Units==9,"Import preserves native units and a source association");
        check(imported.Definition.Elements.Count==3&&imported.Definition.Elements.Single(e=>e.Kind==ElementKind.Frame).Property=="RECT","Import reconstructs exact node connectivity and frame property");
        check(imported.Definition.Operations.Any(o=>o.Key=="PropFrame.SetRectangle"&&o.Arguments["T3"].GetDouble()==200),"Import maps native property dimensions without unit conversion");
        check(imported.NativeSource!.Cases.SequenceEqual(new[]{"Q"})&&imported.NativeSource.Combinations.SequenceEqual(new[]{"COMBO"}),"Import lists native cases and combinations independently of GH definitions");
        string opened=recorder.Calls.Single(c=>c.Key=="File.OpenFile").Arguments!["FileName"].GetString()!;
        check(opened!=source&&!File.Exists(opened)&&recorder.Calls.All(c=>!c.Key.StartsWith("Set")&&!c.Key.Contains(".Set")&&c.Key!="File.Save"),"Import opens a disposable copy and never saves or mutates the source");
        check(imported.Definition.Operations.All(o=>o.Key!="FrameObj.SetEndLengthOffset"),"Zero native end offsets do not disable solid preview");
        var restored=ModelArchive.Deserialize(ModelArchive.Serialize(imported));
        check(restored.Fingerprint==imported.Fingerprint&&restored.NativeSource!.FileHash==AnalysisService.Hash(source),"Associated Model archive round trip retains source hash and fingerprint");
        var fragment=ModelArchive.DeserializeFragment(ModelArchive.SerializeFragment(imported.Definition));
        check(new SapModel(fragment,9).Fingerprint==imported.Fingerprint,"Associated fragment serialization retains provenance");
        reject(()=>imported.Definition.Append(Operation.Create("FrameObj.SetLocalAxes",("Name","F1"),("Ang",45d))),"Deferred mutations cannot silently detach an imported native Model");
        reject(()=>new SapModel(imported.Definition,6),"Associated units cannot silently change");
        reject(()=>new SapModel(new Fragment(imported.Definition.Operations,imported.Definition.Elements.Where(e=>e.Kind==ElementKind.Node),imported.NativeSource),9),"A partial native view cannot replace the full native Model");
        reject(()=>ModelWriter.Write(new ImportRecordingClient(),imported),"Native import cannot accidentally use blank-model reconstruction");
        reject(()=>new AnalysisService(()=>new ImportRecordingClient()).Execute(imported,new RunOptions(source,false,true)),"Overwrite never permits writing to the imported source path");
        string export=Path.Combine(folder,"copy.sdb");var exportClient=new ImportRecordingClient();
        var exported=new AnalysisService(()=>exportClient).Execute(imported,new RunOptions(export));
        check(File.ReadAllText(export)==File.ReadAllText(source)&&exported.NativeSource?.FileHash==imported.NativeSource.FileHash,"Export retains the complete native file and its association");
        check(exportClient.Calls.All(c=>c.Key is "File.OpenFile" or "SetPresentUnits" or "File.Save"),"Associated export reopens the native source without rebuilding assignments");
        var runClient=new ImportRecordingClient();
        var solved=new AnalysisService(()=>runClient).Execute(imported,new RunOptions(Path.Combine(folder,"analysis.sdb"),true,Requests:[]));
        check(runClient.Calls.Any(c=>c.Key=="Analyze.SetRunCaseFlag"&&c.Arguments!["Name"].GetString()=="Q"),"Associated analysis selects original native cases by default");
        check(runClient.Calls.Any(c=>c.Key=="Results.Setup.SetComboSelectedForOutput"&&c.Arguments!["Name"].GetString()=="COMBO"),"Associated analysis selects original combinations by default");
        check(solved.Results!=null&&solved.NativeSource?.FileHash==imported.NativeSource.FileHash,"Associated analysis retains source metadata when results are embedded");
        File.AppendAllText(source,"changed");
        reject(()=>new AnalysisService(()=>new ImportRecordingClient()).Execute(imported,new RunOptions(Path.Combine(folder,"stale.sdb"))),"Changed native source invalidates export/analysis");
        var changed=new ModelImportService(()=>new ImportRecordingClient()).Read(source);
        check(changed.Fingerprint!=imported.Fingerprint,"Reimporting a changed native source changes the Model fingerprint");
        reject(()=>changed.WithResults(solved.Results!),"Results from an older native source cannot be attached after reimport");
        reject(()=>SapModel.Merge([imported,changed]),"Different native source revisions cannot be merged");
    }
}

internal sealed class ImportRecordingClient:ISapClient
{
    public List<(string Key,IReadOnlyDictionary<string,JsonElement>? Arguments)> Calls{get;}=[];
    public IReadOnlyDictionary<string,JsonElement> Call(string key,IReadOnlyDictionary<string,JsonElement>? args=null)
    {
        Calls.Add((key,args));ApiSchema.ValidateArguments(key,args??ApiSchema.Args(),true);
        string name=args?.TryGetValue("Name",out var n)==true?n.GetString()!:"";
        if(key.EndsWith(".GetNameList"))
        {
            string[] names=key switch
            {
                "PointObj.GetNameList"=>["BASE","TIP"],"FrameObj.GetNameList"=>["F1"],"PropMaterial.GetNameList"=>["STEEL"],
                "LoadPatterns.GetNameList"=>["Q"],"LoadCases.GetNameList"=>["Q"],"RespCombo.GetNameList"=>["COMBO"],_=>[]
            };
            return ApiSchema.Args(("NumberNames",names.Length),("MyName",names.Length==0?null:names));
        }
        return key switch
        {
            "GetPresentUnits"=>ApiSchema.Args(("ReturnValue",9)),
            "PointObj.GetCoordCartesian"=>ApiSchema.Args(("X",name=="BASE"?0d:3000d),("Y",0d),("Z",0d)),
            "FrameObj.GetPoints"=>ApiSchema.Args(("Point1","BASE"),("Point2","TIP")),
            "FrameObj.GetSection"=>ApiSchema.Args(("PropName","RECT"),("SAuto","")),
            "PropMaterial.GetMaterial"=>ApiSchema.Args(("MatType",1)),
            "PropMaterial.GetMPIsotropic"=>ApiSchema.Args(("E",210000d),("U",.3),("A",1.2e-5)),
            "PropMaterial.GetWeightAndMass"=>ApiSchema.Args(("W",7.85e-5),("M",0d)),
            "PropFrame.GetTypeOAPI"=>ApiSchema.Args(("PropType",8)),
            "PropFrame.GetRectangle"=>ApiSchema.Args(("MatProp","STEEL"),("T3",200d),("T2",100d)),
            "PointObj.GetRestraint"=>ApiSchema.Args(("Value",Enumerable.Repeat(name=="BASE",6).ToArray())),
            "PointObj.GetSpring"=>ApiSchema.Args(("K",new double[6])),
            "PointObj.GetLocalAxes"=>ApiSchema.Args(("A",0d),("B",0d),("C",0d)),
            "FrameObj.GetLocalAxes"=>ApiSchema.Args(("Ang",0d)),
            "FrameObj.GetLocalAxesAdvanced"=>throw new NotSupportedException("Recorded fixture omits advanced axes."),
            "FrameObj.GetEndLengthOffset"=>ApiSchema.Args(("AutoOffset",false),("Length1",0d),("Length2",0d),("RZ",0d)),
            "FrameObj.GetModifiers"=>ApiSchema.Args(("Value",Enumerable.Repeat(1d,8).ToArray())),
            "FrameObj.GetReleases"=>ApiSchema.Args(("II",new bool[6]),("JJ",new bool[6]),("StartValue",new double[6]),("EndValue",new double[6])),
            "PointObj.GetLoadForce" or "FrameObj.GetLoadDistributed" or "FrameObj.GetLoadPoint"=>ApiSchema.Args(("NumberItems",0)),
            "LoadPatterns.GetLoadType"=>ApiSchema.Args(("MyType",8)),
            "LoadPatterns.GetSelfWTMultiplier"=>ApiSchema.Args(("SelfWTMultiplier",0d)),
            "LoadCases.GetTypeOAPI"=>ApiSchema.Args(("CaseType",1),("SubType",0)),
            "Analyze.GetCaseStatus"=>ApiSchema.Args(("CaseName",new[]{"Q"}),("Status",new[]{4})),
            "PointObj.GetTransformationMatrix"=>ApiSchema.Args(("Value",new double[]{1,0,0,0,1,0,0,0,1})),
            _=>ApiSchema.Args()
        };
    }
    public void Dispose(){}
}
