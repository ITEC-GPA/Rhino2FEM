using System.Security.Cryptography;
using System.Text.Json;
using Rhino2SAP.Core;

namespace Rhino2SAP.Api;

public sealed record RunOptions(string Path,bool Analyze=false,bool Overwrite=false,IReadOnlyList<string>? Cases=null,IReadOnlyList<string>? Combinations=null,IReadOnlyList<ResultRequest>? Requests=null);
public sealed record Provenance(string Fingerprint,int Units,string SdbHash,DateTimeOffset CreatedUtc,IReadOnlyList<string> Cases,IReadOnlyList<string> Combinations);
public sealed class AnalysisService
{
    private readonly Func<ISapClient> factory;
    public AnalysisService(Func<ISapClient>? factory=null){this.factory=factory??(()=>new SapClient());}
    public SapModel Execute(SapModel model,RunOptions options)
    {
        ModelValidation.RequireValid(model);
        string path=System.IO.Path.GetFullPath(options.Path);
        if(!path.EndsWith(".sdb",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Choose a .sdb path.");
        if(File.Exists(path)&&!options.Overwrite)throw new IOException("The destination exists. Enable Overwrite or choose a new path.");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        string stage=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path)!,".rhino2sap-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);string stagedPath=System.IO.Path.Combine(stage,System.IO.Path.GetFileName(path));
        var tables=new List<ResultTable>();var issues=new List<ResultIssue>();
        var cases=(options.Cases??[]).ToArray();var combos=(options.Combinations??[]).ToArray();
        if(combos.Length==0)combos=model.Definition.Operations.Where(o=>o.Key=="RespCombo.Add").Select(o=>o.Arguments["Name"].GetString()!).Distinct().ToArray();
        try
        {
            using(var client=factory())
            {
                var nodes=ModelWriter.Write(client,model);
                tables.Add(new ResultTable("Rhino2SAP.NodeCoordinates",ApiSchema.Args(("Name",nodes.Keys.ToArray()),("X",nodes.Values.Select(p=>p.X).ToArray()),("Y",nodes.Values.Select(p=>p.Y).ToArray()),("Z",nodes.Values.Select(p=>p.Z).ToArray()))));
                client.Call("File.Save",ApiSchema.Args(("FileName",stagedPath)));
                if(options.Analyze)
                {
                    if(cases.Length==0)
                    {
                        cases=model.Definition.Operations.Where(o=>o.Key.StartsWith("LoadCases.")&&o.Key.EndsWith(".SetCase")||o.Key=="LoadPatterns.Add"&&(!o.Arguments.TryGetValue("AddAnalysisCase",out var add)||add.GetBoolean())).Select(o=>o.Arguments["Name"].GetString()!).Distinct().ToArray();
                        if(cases.Length==0)throw new ArgumentException("Define at least one analysis case before running SAP.");
                    }
                    client.Call("Analyze.SetRunCaseFlag",ApiSchema.Args(("Name",""),("Run",false),("All",true)));
                    foreach(string name in cases)client.Call("Analyze.SetRunCaseFlag",ApiSchema.Args(("Name",name),("Run",true)));
                    client.Call("Analyze.RunAnalysis");
                    VerifyCaseStatus(client,cases);
                    SelectOutput(client,cases,combos);
                    var matrices=nodes.Keys.Select(n=>client.Call("PointObj.GetTransformationMatrix",ApiSchema.Args(("Name",n)))["Value"].EnumerateArray().Select(v=>v.GetDouble()).ToArray()).ToArray();
                    tables.Add(new ResultTable("Rhino2SAP.NodeAxes",ApiSchema.Args(("Name",nodes.Keys.ToArray()),("Matrix",matrices))));
                    foreach(var request in options.Requests??DefaultRequests(model))
                    {
                        try {ApiSchema.ValidateArguments(request.Method,request.Arguments,true);var table=new ResultTable(request.Method,client.Call(request.Method,request.Arguments));tables.Add(table);if(table.RowCount==0)issues.Add(new(request.Method,"The API returned no result rows for the selected cases/combinations."));}
                        catch(Exception ex){issues.Add(new(request.Method,ex.Message));}
                    }
                    client.Call("File.Save",ApiSchema.Args(("FileName",stagedPath)));
                }
            }
            // Promote the model and its solver sidecars only after export/analysis completes.
            foreach(string file in Directory.GetFiles(stage).OrderBy(f=>f.EndsWith(".sdb",StringComparison.OrdinalIgnoreCase)?1:0))
            {string target=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path)!,System.IO.Path.GetFileName(file));if(File.Exists(target)&&!options.Overwrite)throw new IOException($"Destination sidecar already exists: {target}");}
            foreach(string file in Directory.GetFiles(stage).OrderBy(f=>f.EndsWith(".sdb",StringComparison.OrdinalIgnoreCase)?1:0))File.Move(file,System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path)!,System.IO.Path.GetFileName(file)),options.Overwrite);
            string hash=Hash(path);
            File.WriteAllText(path+".rhino2sap.json",JsonSerializer.Serialize(new Provenance(model.Fingerprint,model.Units,hash,DateTimeOffset.UtcNow,cases,combos),ApiSchema.JsonOptions));
            if(!options.Analyze)return model.WithoutResults();
            var result=new ResultSet(model.Fingerprint,path,hash,true,tables,issues);
            ResultArchive.Save(path+".results.json",result);
            return model.WithResults(result);
        }
        finally{if(Directory.Exists(stage))Directory.Delete(stage,true);}
    }
    private static void VerifyCaseStatus(ISapClient client,string[] cases)
    {
        var status=client.Call("Analyze.GetCaseStatus");var names=status["CaseName"].EnumerateArray().Select(x=>x.GetString()).ToArray();var states=status["Status"].EnumerateArray().Select(x=>x.GetInt32()).ToArray();
        foreach(var name in cases){int i=Array.IndexOf(names,name);if(i<0||states[i]!=4)throw new InvalidOperationException($"Case '{name}' did not finish (SAP status {(i<0?-1:states[i])}).");}
    }
    private static void SelectOutput(ISapClient client,string[] cases,string[] combos)
    {
        client.Call("Results.Setup.DeselectAllCasesAndCombosForOutput");
        foreach(string name in cases)client.Call("Results.Setup.SetCaseSelectedForOutput",ApiSchema.Args(("Name",name),("Selected",true)));
        foreach(string name in combos)client.Call("Results.Setup.SetComboSelectedForOutput",ApiSchema.Args(("Name",name),("Selected",true)));
    }
    public static IReadOnlyList<ResultRequest> DefaultRequests(SapModel model)
    {
        var methods=new List<string>{"Results.JointDispl","Results.JointReact"};var kinds=model.Definition.Elements.Select(e=>e.Kind).ToHashSet();
        if(kinds.Contains(ElementKind.Frame)){methods.Add("Results.FrameForce");methods.Add("Results.FrameJointForce");}
        if(kinds.Contains(ElementKind.Area)){methods.Add("Results.AreaForceShell");methods.Add("Results.AreaStressShell");methods.Add("Results.AreaStrainShell");methods.Add("Results.AreaJointForceShell");}
        if(kinds.Contains(ElementKind.Solid))methods.Add("Results.SolidStress");
        if(kinds.Contains(ElementKind.Link))methods.Add("Results.LinkForce");
        if(model.Definition.Operations.Any(o=>o.Key.StartsWith("LoadCases.Modal"))){methods.Add("Results.ModalPeriod");methods.Add("Results.ModalParticipatingMassRatios");methods.Add("Results.ModeShape");}
        if(model.Definition.Operations.Any(o=>o.Key.StartsWith("LoadCases.Buckling")))methods.Add("Results.BucklingFactor");
        return methods.Select(ResultRequest.All).ToArray();
    }
    public static string Hash(string path){using var file=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(file));}
}

public static class ResultArchive
{
    public static void Save(string path,ResultSet result)=>File.WriteAllText(path,JsonSerializer.Serialize(result,ApiSchema.JsonOptions));
    public static ResultSet Load(string path,SapModel model)
    {
        var result=JsonSerializer.Deserialize<ResultSet>(File.ReadAllText(path))??throw new InvalidDataException("Empty result archive.");
        result.RequireValidFor(model);
        if(!File.Exists(result.FilePath)||AnalysisService.Hash(result.FilePath)!=result.FileHash)throw new InvalidDataException("SAP model file changed since these results were captured.");
        return result;
    }
}
