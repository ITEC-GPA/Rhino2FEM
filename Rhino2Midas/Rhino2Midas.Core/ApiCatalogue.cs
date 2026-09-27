using System.Text.Json;

namespace Rhino2Midas.Core;

public sealed record ApiDefinition(string Key,string Endpoint,string Title,string Url,JsonElement Example,JsonElement Schema,bool Item);
public static class ApiCatalogue
{
    private static readonly Lazy<IReadOnlyDictionary<string,ApiDefinition>> Cache=new(()=>
    {
        using var stream=typeof(ApiCatalogue).Assembly.GetManifestResourceStream("ApiDefinitions.json")!;
        return JsonSerializer.Deserialize<ApiDefinition[]>(stream)!.ToDictionary(d=>d.Key);
    });
    public static IReadOnlyDictionary<string,ApiDefinition> Definitions=>Cache.Value;
    public static ApiDefinition Get(string key)=>Definitions[key];
    public static IEnumerable<string> Validate(JsonElement data,JsonElement schema,string path="")
    {
        if(schema.ValueKind!=JsonValueKind.Object)yield break;
        if(schema.TryGetProperty("type",out var type)&&type.ValueKind==JsonValueKind.String)
        {
            bool valid=type.GetString() switch{"object"=>data.ValueKind==JsonValueKind.Object,"array"=>data.ValueKind==JsonValueKind.Array,"string"=>data.ValueKind==JsonValueKind.String,"number"=>data.ValueKind==JsonValueKind.Number,"integer"=>data.ValueKind==JsonValueKind.Number&&data.TryGetInt32(out _),"boolean"=>data.ValueKind is JsonValueKind.True or JsonValueKind.False,_=>true};
            if(!valid){yield return path+": expected "+type.GetString();yield break;}
        }
        if(schema.TryGetProperty("enum",out var values)&&!values.EnumerateArray().Any(v=>Native.Canonical(v)==Native.Canonical(data)))yield return path+": value is outside documented enum.";
        if(data.ValueKind==JsonValueKind.Object)
        {
            if(schema.TryGetProperty("required",out var required))foreach(var key in required.EnumerateArray())if(!data.TryGetProperty(key.GetString()!,out _))yield return path+"."+key.GetString()+": required.";
            if(schema.TryGetProperty("properties",out var properties))foreach(var property in data.EnumerateObject())if(properties.TryGetProperty(property.Name,out var sub))foreach(var error in Validate(property.Value,sub,path+"."+property.Name))yield return error;
        }
        if(data.ValueKind==JsonValueKind.Array&&schema.TryGetProperty("items",out var items)){int i=0;foreach(var item in data.EnumerateArray()){foreach(var error in Validate(item,items,path+"["+i+"]"))yield return error;i++;}}
    }
}
