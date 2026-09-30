using System.Text.Json;

namespace Rhino2SAP.Core;

public static class ModelArchive
{
    public sealed record OperationData(string Key,Dictionary<string,JsonElement> Arguments);
    public sealed record ElementData(string Name,ElementKind Kind,Position[] Points,string Property,ElementResults? Results=null);
    public sealed record FragmentData(OperationData[] Operations,ElementData[] Elements,NativeModelSource? NativeSource=null);
    public sealed record Data(int Version,int Units,double Tolerance,OperationData[] Operations,ElementData[] Elements,ResultSet? Results,NativeModelSource? NativeSource=null);
    public static string Serialize(SapModel model)=>JsonSerializer.Serialize(new Data(1,model.Units,model.Tolerance,model.Definition.Operations.Select(o=>new OperationData(o.Key,o.Arguments.ToDictionary())).ToArray(),model.Definition.Elements.Select(e=>new ElementData(e.Name,e.Kind,e.Points.ToArray(),e.Property)).ToArray(),model.Results,model.NativeSource),ApiSchema.JsonOptions);
    public static SapModel Deserialize(string json)
    {
        var data=JsonSerializer.Deserialize<Data>(json)??throw new InvalidDataException("Empty SAP Model JSON.");if(data.Version!=1)throw new InvalidDataException("Unsupported model archive version.");
        return new SapModel(new Fragment(data.Operations.Select(o=>new Operation(o.Key,o.Arguments)),data.Elements.Select(e=>new Element(e.Name,e.Kind,e.Points,e.Property)),data.NativeSource),data.Units,data.Tolerance,data.Results);
    }
    public static string SerializeFragment(Fragment fragment)=>JsonSerializer.Serialize(new FragmentData(fragment.Operations.Select(o=>new OperationData(o.Key,o.Arguments.ToDictionary())).ToArray(),fragment.Elements.Select(e=>new ElementData(e.Name,e.Kind,e.Points.ToArray(),e.Property,e.Results)).ToArray(),fragment.NativeSource),ApiSchema.JsonOptions);
    public static Fragment DeserializeFragment(string json)
    {
        var data=JsonSerializer.Deserialize<FragmentData>(json)??throw new InvalidDataException("Empty SAP fragment JSON.");
        return new Fragment(data.Operations.Select(o=>new Operation(o.Key,o.Arguments)),data.Elements.Select(e=>new Element(e.Name,e.Kind,e.Points,e.Property){Results=e.Results}),data.NativeSource);
    }
}
