using System.Text.Json;
using Rhino2SAP.Core;

namespace Rhino2SAP.Api;

/// <summary>Extracts a GH view while retaining the original SDB as the authoritative solver model.</summary>
public sealed class ModelImportService(Func<ISapClient>? factory = null)
{
    private readonly Func<ISapClient> factory = factory ?? (() => new SapClient());

    public SapModel Read(string file)
    {
        string source = Path.GetFullPath(file);
        if (!source.EndsWith(".sdb", StringComparison.OrdinalIgnoreCase) || !File.Exists(source))
            throw new FileNotFoundException("Choose an existing SAP2000 .sdb file.", source);
        string hash = AnalysisService.Hash(source);
        string folder = Path.Combine(Path.GetTempPath(), "Rhino2SAP-import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string copy = Path.Combine(folder, Path.GetFileName(source));
            File.Copy(source, copy);
            if (AnalysisService.Hash(copy) != hash) throw new IOException("Source changed during import. Try again.");
            using var client = factory();
            client.Call("File.OpenFile", ApiSchema.Args(("FileName", copy)));
            var model = ReadOpened(client, source, hash);
            if (AnalysisService.Hash(source) != hash) throw new IOException("Source changed during import. Try again.");
            return model;
        }
        finally { Directory.Delete(folder, true); }
    }

    // Public for deterministic verification against recorded/native API responses, without starting SAP.
    public static SapModel ReadOpened(ISapClient client, string source, string hash)
    {
        int units = client.Call("GetPresentUnits")["ReturnValue"].GetInt32();
        var elements = new List<Element>();
        var operations = new List<Operation>();
        var issues = new List<string>();
        var points = new Dictionary<string, Position>(StringComparer.Ordinal);
        string[] Names(string path)
        {
            var data=client.Call(path + ".GetNameList");
            if(data.TryGetValue("NumberNames",out var count)&&count.GetInt32()==0)return [];
            return data["MyName"].EnumerateArray().Select(v => v.GetString()!).ToArray();
        }
        void Inspect(string description, Action read)
        {
            try { read(); }
            catch (Exception ex) { issues.Add(description + ": " + ex.GetBaseException().Message); }
        }
        IReadOnlyDictionary<string, JsonElement> Read(string key, string name) => client.Call(key, ApiSchema.Args(("Name", name)));
        void Mirror(string getter, string setter, string name)
        {
            var data = Read(getter, name);
            if(setter=="FrameObj.SetEndLengthOffset"&&data["Length1"].GetDouble()==0&&data["Length2"].GetDouble()==0)return;
            var args = new Dictionary<string, JsonElement> { ["Name"] = ApiSchema.Value(name) };
            foreach (var parameter in ApiSchema.Get(setter).Parameters.Where(p => p.Name != "Name"))
                if (data.TryGetValue(parameter.Name, out var value)) args[parameter.Name] = value;
            operations.Add(new Operation(setter, args));
        }

        foreach (string name in Names("PointObj"))
        {
            var data = Read("PointObj.GetCoordCartesian", name);
            points[name] = new(data["X"].GetDouble(), data["Y"].GetDouble(), data["Z"].GetDouble());
            elements.Add(new(name, ElementKind.Node, [points[name]]));
        }
        foreach (var kind in Enum.GetValues<ElementKind>().Where(k => k != ElementKind.Node))
        {
            string path = kind + "Obj";
            foreach (string name in Names(path))
            {
                var data = Read(path + ".GetPoints", name);
                string[] vertices = data.TryGetValue("Point", out var array)
                    ? array.EnumerateArray().Select(v => v.GetString()!).ToArray()
                    : [data["Point1"].GetString()!, data["Point2"].GetString()!];
                if (vertices.Any(n => !points.ContainsKey(n)))
                    throw new InvalidDataException($"{path} {name}: missing connectivity node.");
                string property = Read(path + (kind == ElementKind.Frame ? ".GetSection" : ".GetProperty"), name)["PropName"].GetString()!;
                elements.Add(new(name, kind, vertices.Select(n => points[n]), property));
            }
        }
        Inspect("Tendons", () =>
        {
            var names = Names("TendonObj");
            if (names.Length != 0) issues.Add($"{names.Length} tendons retained in the source SDB; no GH element representation.");
        });

        foreach (string name in Names("PropMaterial"))
        {
            Inspect("Material " + name, () => Mirror("PropMaterial.GetMaterial", "PropMaterial.SetMaterial", name));
            Inspect("Elasticity " + name, () => Mirror("PropMaterial.GetMPIsotropic", "PropMaterial.SetMPIsotropic", name));
            Inspect("Density " + name, () => operations.Add(Operation.Create("PropMaterial.SetWeightAndMass",
                ("Name", name), ("MyOption", 1), ("Value", Read("PropMaterial.GetWeightAndMass", name)["W"]))));
        }

        var frameShapes = new Dictionary<int, string>
        {
            [1] = "ISection", [2] = "Channel", [3] = "Tee", [4] = "Angle", [5] = "DblAngle",
            [6] = "Tube", [7] = "Pipe", [8] = "Rectangle", [9] = "Circle", [10] = "General",
            [11] = "DblChannel", [46] = "Trapezoidal"
        };
        foreach (var property in elements.Where(e => e.Kind != ElementKind.Node).Select(e => (e.Kind, e.Property)).Distinct())
        {
            string name = property.Property;
            Inspect(property.Kind + " property " + name, () =>
            {
                string family = property.Kind switch
                {
                    ElementKind.Frame => "PropFrame", ElementKind.Area => "PropArea",
                    ElementKind.Solid => "PropSolid", ElementKind.Link => "PropLink", _ => "PropCable"
                };
                string shape = "Prop";
                if (property.Kind == ElementKind.Frame)
                {
                    int type = Read(family + ".GetTypeOAPI", name)["PropType"].GetInt32();
                    if (!frameShapes.TryGetValue(type, out shape!))
                        throw new NotSupportedException("Section shape is retained natively; physical Brep reconstruction is unavailable.");
                }
                if (property.Kind == ElementKind.Area) shape = "Shell_1";
                if (property.Kind == ElementKind.Link) shape = "Linear";
                Mirror(family + ".Get" + shape, family + ".Set" + shape, name);
            });
        }
        foreach (string name in Names("LoadPatterns"))
            Inspect("Pattern " + name, () => operations.Add(Operation.Create("LoadPatterns.Add", ("Name", name),
                ("MyType", Read("LoadPatterns.GetLoadType", name)["MyType"]),
                ("SelfWTMultiplier", Read("LoadPatterns.GetSelfWTMultiplier", name)["SelfWTMultiplier"]), ("AddAnalysisCase", false))));

        foreach (var element in elements)
        {
            string path = element.Kind == ElementKind.Node ? "PointObj" : element.Kind + "Obj";
            string[] attributes = element.Kind switch
            {
                ElementKind.Node => ["Restraint", "Spring", "LocalAxes"],
                ElementKind.Frame => ["LocalAxes", "LocalAxesAdvanced", "Releases", "EndLengthOffset", "Modifiers"],
                ElementKind.Area => ["LocalAxes", "LocalAxesAdvanced", "Modifiers"], _ => []
            };
            foreach (string attribute in attributes)
                Inspect(element.Name + " " + attribute, () => Mirror(path + ".Get" + attribute, path + ".Set" + attribute, element.Name));
            string[] loads = element.Kind switch
            {
                ElementKind.Node => ["Force"], ElementKind.Frame => ["Distributed", "Point"],
                ElementKind.Area => ["Uniform"], _ => []
            };
            foreach (string load in loads)
                Inspect(element.Name + " load " + load, () =>
                {
                    string getter = path + ".GetLoad" + load, setter = path + ".SetLoad" + load;
                    var data = Read(getter, element.Name);
                    int count = data["NumberItems"].GetInt32();
                    for (int row = 0; row < count; row++)
                    {
                        var args = new Dictionary<string, JsonElement> { ["Name"] = ApiSchema.Value(element.Name) };
                        foreach (var p in ApiSchema.Get(setter).Parameters.Where(p => p.Name != "Name"))
                        {
                            if (p.Name == "Replace") args[p.Name] = ApiSchema.Value(false);
                            else if (p.Name == "RelDist") args[p.Name] = ApiSchema.Value(false);
                            else if (load == "Force" && p.Name == "Value")
                                args[p.Name] = ApiSchema.Value(new[] { "F1", "F2", "F3", "M1", "M2", "M3" }.Select(c => data[c][row]));
                            else if (data.TryGetValue(p.Name, out var column) && column.ValueKind == JsonValueKind.Array)
                                args[p.Name] = column[row];
                        }
                        operations.Add(new(setter, args));
                    }
                });
        }

        string[] cases = Names("LoadCases"), combinations = Names("RespCombo");
        bool modal = false, buckling = false;
        foreach (string name in cases)
            Inspect("Case " + name, () =>
            {
                int type = Read("LoadCases.GetTypeOAPI", name)["CaseType"].GetInt32();
                modal |= type == 3; buckling |= type == 10;
            });
        issues.Insert(0, "GH contains geometry, supported properties and common assignments for inspection/preview. All native loads, cases, combinations and advanced definitions remain in the associated SDB; export/analysis open a verified private copy. Edit in SAP and reimport to change this Model.");
        var view = new SapModel(new Fragment(operations, elements), units);
        var association = new NativeModelSource(Path.GetFullPath(source), hash, view.Fingerprint, cases, combinations,
            issues.Distinct().ToArray(), modal, buckling);
        return new SapModel(new Fragment(view.Definition.Operations, view.Definition.Elements, association), units);
    }
}
