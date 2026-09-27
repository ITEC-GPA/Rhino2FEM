using System.Text.Json;
using System.Xml.Linq;
using Rhino2SAP.Core;

namespace Rhino2SAP.Api;

public static class ReadService
{
    public static ResultTable Query(string file,string key,IReadOnlyDictionary<string,JsonElement> args,IEnumerable<string>? cases=null,IEnumerable<string>? combinations=null)
    {
        var method=ApiSchema.Get(key);
        if(!method.Method.StartsWith("Get")&&method.Path!="Results")throw new ArgumentException("Only Get methods and Results queries are allowed.");
        ApiSchema.ValidateArguments(key,args,true);
        using var client=new SapClient();client.Call("File.OpenFile",ApiSchema.Args(("FileName",Path.GetFullPath(file))));
        if(method.Path=="Results")
        {
            client.Call("Results.Setup.DeselectAllCasesAndCombosForOutput");
            foreach(string name in cases??[])client.Call("Results.Setup.SetCaseSelectedForOutput",ApiSchema.Args(("Name",name),("Selected",true)));
            foreach(string name in combinations??[])client.Call("Results.Setup.SetComboSelectedForOutput",ApiSchema.Args(("Name",name),("Selected",true)));
        }
        return new(key,client.Call(key,args));
    }
    public static Fragment Geometry(string file)
    {
        using var client=new SapClient();client.Call("File.OpenFile",ApiSchema.Args(("FileName",Path.GetFullPath(file))));
        var elements=new List<Element>();var points=new Dictionary<string,Position>();
        foreach(string name in Names(client,"PointObj"))
        {var c=client.Call("PointObj.GetCoordCartesian",ApiSchema.Args(("Name",name)));var p=new Position(c["X"].GetDouble(),c["Y"].GetDouble(),c["Z"].GetDouble());points[name]=p;elements.Add(new(name,ElementKind.Node,[p]));}
        foreach(var kind in Enum.GetValues<ElementKind>().Where(k=>k!=ElementKind.Node))
        {
            string path=kind+"Obj";
            foreach(string name in Names(client,path))
            {
                var c=client.Call(path+".GetPoints",ApiSchema.Args(("Name",name)));
                var vertexNames=c.TryGetValue("Point",out var list)?list.EnumerateArray().Select(p=>p.GetString()!).ToArray():new[]{c["Point1"].GetString()!,c["Point2"].GetString()!};
                // Geometry import deliberately excludes solver/property reconstruction.
                elements.Add(new(name,kind,vertexNames.Where(points.ContainsKey).Select(n=>points[n])));
            }
        }
        return new(elements:elements);
    }
    private static IEnumerable<string> Names(ISapClient client,string path)=>client.Call(path+".GetNameList")["MyName"].EnumerateArray().Select(p=>p.GetString()!).ToArray();

    public static IReadOnlyList<MaterialLibraryEntry> MaterialLibrary(string? file=null)
    {
        file=string.IsNullOrEmpty(file)?Path.Combine(SapClient.FindInstallation(),"Property Libraries","CSiMaterialLibraryEurope.xml"):file;
        var xml=XDocument.Load(file);return xml.Descendants().Where(x=>x.Name.LocalName=="material").Select(m=>new MaterialLibraryEntry(m.Ancestors().First(x=>x.Name.LocalName=="region").Attribute("name")!.Value,m.Ancestors().First(x=>x.Name.LocalName=="standard").Attribute("name")!.Value,m.Attribute("grade")!.Value,m.Attribute("type")!.Value)).ToArray();
    }
}
public sealed record MaterialLibraryEntry(string Region,string Standard,string Grade,string Type);
