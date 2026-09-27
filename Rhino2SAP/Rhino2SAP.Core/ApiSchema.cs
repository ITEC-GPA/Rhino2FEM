using System.Collections.Frozen;
using System.Text.Json;

namespace Rhino2SAP.Core;

public sealed record EnumMember(string Name, int Value);
public sealed record ApiParameter(string Name, string Type, bool ByRef, bool Optional, JsonElement Default, EnumMember[] Enum)
{
    public bool IsArray => Type.EndsWith("[]", StringComparison.Ordinal);
    public string ScalarType => Type.Replace("[]", "");
    public string Description => Name + (Enum.Length == 0 ? "" : ": " + string.Join(", ", Enum.Select(e => $"{e.Value}={e.Name}")));
}
public sealed record ApiMethod(string Key, string Path, string Method, string ReturnType, ApiParameter[] Parameters);

public static class ApiSchema
{
    public static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    public static FrozenDictionary<string, ApiMethod> Methods { get; } = Load();
    private static FrozenDictionary<string, ApiMethod> Load()
    {
        var assembly = typeof(ApiSchema).Assembly;
        using var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(x => x.EndsWith("sap-api-schema.json")))!;
        return JsonSerializer.Deserialize<ApiMethod[]>(stream)!.Select(m=>m with { Parameters=m.Parameters.Select(p=>p with { Enum=p.Enum??[] }).ToArray() }).ToFrozenDictionary(m => m.Key);
    }
    public static JsonElement Value(object? value) => JsonSerializer.SerializeToElement(value);
    public static IReadOnlyDictionary<string, JsonElement> Args(params (string Name, object? Value)[] pairs) =>
        pairs.ToFrozenDictionary(x => x.Name, x => Value(x.Value));
    public static ApiMethod Get(string key) => Methods.TryGetValue(key, out var method) ? method : throw new ArgumentException($"Unknown SAP2000 API method: {key}");

    public static void ValidateArguments(string key, IReadOnlyDictionary<string, JsonElement> args, bool query = false)
    {
        var method = Get(key);
        foreach (var name in args.Keys)
            if (!method.Parameters.Any(p => p.Name == name)) throw new ArgumentException($"{key}: unknown parameter {name}.");
        foreach (var p in method.Parameters)
        {
            if (!args.TryGetValue(p.Name, out var value))
            {
                if (!p.Optional && !(query && p.ByRef)) throw new ArgumentException($"{key}: missing {p.Name}.");
                continue;
            }
            if (p.IsArray)
            {
                if (value.ValueKind != JsonValueKind.Array) throw new ArgumentException($"{key}.{p.Name} must be a list.");
                foreach (var item in value.EnumerateArray()) ValidateScalar(p, item);
            }
            else ValidateScalar(p, value);
        }
    }
    private static void ValidateScalar(ApiParameter p, JsonElement value)
    {
        bool valid = p.ScalarType switch
        {
            "System.String" => value.ValueKind == JsonValueKind.String,
            "System.Boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "System.Double" => value.TryGetDouble(out var d) && double.IsFinite(d),
            "System.Int32" => value.TryGetInt32(out _),
            _ => p.Enum.Length > 0 && value.TryGetInt32(out var e) && p.Enum.Any(x => x.Value == e)
        };
        if (!valid) throw new ArgumentException($"Invalid value for {p.Name} ({p.Type}).");
    }
}
