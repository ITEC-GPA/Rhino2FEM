using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Rhino2SAP.Core;

public readonly record struct Position(double X, double Y, double Z)
{
    public double DistanceTo(Position other) => Math.Sqrt(Math.Pow(X-other.X,2)+Math.Pow(Y-other.Y,2)+Math.Pow(Z-other.Z,2));
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
}
public enum ElementKind { Node, Frame, Area, Solid, Link, Cable }

/// <summary>A read-only association with a native SDB, preserving definitions not represented by GH.</summary>
public sealed record NativeModelSource(string FilePath, string FileHash, string DefinitionFingerprint,
    string[] Cases, string[] Combinations, string[] Issues, bool HasModal = false, bool HasBuckling = false);
public sealed record Element
{
    public string Name { get; init; }
    public ElementKind Kind { get; init; }
    public IReadOnlyList<Position> Points { get; init; }
    public string Property { get; init; }
    public ElementResults? Results { get; init; }
    public Element(string name, ElementKind kind, IEnumerable<Position> points, string property = "")
    { Name = name; Kind = kind; Points = Array.AsReadOnly(points.ToArray()); Property = property; }
}

/// <summary>A deferred SAP command. No SAP process or file is touched while composing fragments.</summary>
public sealed record Operation
{
    public Guid Token { get; } = Guid.NewGuid();
    public string Key { get; init; }
    public IReadOnlyDictionary<string, JsonElement> Arguments { get; init; }
    public Operation(string key, IReadOnlyDictionary<string, JsonElement> arguments)
    { Key = key; Arguments = arguments.ToFrozenDictionary(p => p.Key, p => p.Value.Clone()); ApiSchema.ValidateArguments(key, Arguments); }
    public static Operation Create(string key, params (string Name, object? Value)[] values) => new(key, ApiSchema.Args(values));
    public string Identity => Key+":"+string.Join("|",Arguments.OrderBy(p=>p.Key).Select(p=>p.Key+"="+JsonSerializer.Serialize(p.Value)));
    public int Stage => Key.StartsWith("PropMaterial.") ? 10 : Key.StartsWith("Prop") ? 20 : Key.StartsWith("CoordSys.") || Key.StartsWith("ConstraintDef.") || Key.StartsWith("GroupDef.") ? 25 : Key.StartsWith("LoadPatterns.") ? 30 : Key.StartsWith("LoadCases.") ? 60 : Key.StartsWith("RespCombo.") ? 70 : 50;
    public override string ToString() => Key + (Arguments.TryGetValue("Name", out var n) ? " / "+n.GetString() : "");
}

public sealed class Fragment
{
    public NativeModelSource? NativeSource { get; }
    public IReadOnlyList<Operation> Operations { get; }
    public IReadOnlyList<Element> Elements { get; }
    public Fragment(IEnumerable<Operation>? operations = null, IEnumerable<Element>? elements = null, NativeModelSource? nativeSource = null)
    {
        Operations = Array.AsReadOnly((operations ?? []).ToArray());
        Elements = Array.AsReadOnly((elements ?? []).ToArray());
        NativeSource = nativeSource;
    }
    public void RequireEditable()
    {
        if (NativeSource != null)
            throw new InvalidOperationException("This Model is associated with a native SAP file. Edit that file and import it again; export and analysis preserve its complete native definition.");
    }
    public Fragment Append(Operation op)
    {
        RequireEditable();
        return new(Operations.Append(op), Elements.Select(e=>e with { Results=null }));
    }
    public static Fragment Combine(IEnumerable<Fragment> fragments)
    {
        var items = fragments.ToArray();
        var sources = items.Select(f => f.NativeSource).OfType<NativeModelSource>().ToArray();
        if (sources.Any(s => s.FileHash != sources[0].FileHash || s.DefinitionFingerprint != sources[0].DefinitionFingerprint))
            throw new ArgumentException("Cannot merge different native SAP model associations.");
        return new(items.SelectMany(f=>f.Operations), items.SelectMany(f=>f.Elements), sources.FirstOrDefault());
    }
    public override string ToString() => $"SAP fragment: {Elements.Count} elements, {Operations.Count} operations";
}

public sealed class SapModel
{
    public Fragment Definition { get; }
    public int Units { get; }
    public double Tolerance { get; }
    public ResultSet? Results { get; }
    public NativeModelSource? NativeSource => Definition.NativeSource;
    public string Fingerprint { get; }
    public SapModel(Fragment definition, int units = 6, double tolerance = 1e-6, ResultSet? results = null)
    {
        if (units < 1 || units > 16) throw new ArgumentOutOfRangeException(nameof(units), "Use SAP eUnits values 1..16; 6 = kN_m_C.");
        if (!double.IsFinite(tolerance) || tolerance <= 0) throw new ArgumentOutOfRangeException(nameof(tolerance));
        Definition = Normalize(new Fragment(definition.Operations,definition.Elements.Select(e=>e with { Results=null }),definition.NativeSource),tolerance); Units = units; Tolerance = tolerance;
        Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { Units, Tolerance, Elements=Definition.Elements.OrderBy(e=>e.Kind).ThenBy(e=>e.Name).Select(e=>new{e.Name,e.Kind,e.Points,e.Property}), Operations=Definition.Operations.Select(o=>o.Identity) }))));
        if (NativeSource is { } source)
        {
            if (Fingerprint != source.DefinitionFingerprint)
                throw new InvalidOperationException("An imported native Model must retain its complete definition, units and tolerance. Reimport after editing in SAP.");
            Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Fingerprint + "|SAP:" + source.FileHash)));
        }
        if (results != null && results.Fingerprint != Fingerprint) throw new InvalidOperationException("The results belong to a different model definition.");
        Results = results;
        if(results!=null)Definition=new Fragment(Definition.Operations,Definition.Elements.Select(e=>e with { Results=ElementResults.Attach(e,results,tolerance) }),Definition.NativeSource);
    }
    private static Fragment Normalize(Fragment definition,double tolerance)
    {
        var elements = new Dictionary<(ElementKind,string),Element>();
        foreach (var e in definition.Elements)
        {
            if (elements.TryGetValue((e.Kind,e.Name),out var prior) && (prior.Property!=e.Property || !prior.Points.SequenceEqual(e.Points)))
                throw new ArgumentException($"Conflicting {e.Kind} name '{e.Name}'. Use unique names before merging.");
            elements[(e.Kind,e.Name)] = e;
        }
        // Include implicit connectivity nodes in Model and Decompose, not only during export.
        var nodePositions=elements.Values.Where(e=>e.Kind==ElementKind.Node&&e.Points.Count==1).Select(e=>e.Points[0]).ToList();
        foreach(var point in elements.Values.Where(e=>e.Kind!=ElementKind.Node).SelectMany(e=>e.Points).ToArray())
        {
            if(nodePositions.Any(p=>p.DistanceTo(point)<=tolerance))continue;
            string name="R2S_J_"+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(point))))[..12];
            if(elements.ContainsKey((ElementKind.Node,name)))throw new ArgumentException("Reserved automatic node name conflict: "+name);
            elements[(ElementKind.Node,name)]=new Element(name,ElementKind.Node,[point]);nodePositions.Add(point);
        }
        // Idempotent shared definitions are deduplicated. Load contributions are deliberately preserved.
        var seen = new HashSet<string>();
        var tokens = new HashSet<Guid>();
        var ops = definition.Operations.Where(o => tokens.Add(o.Token)).Where(o => !(o.Stage <= 30 || o.Key.EndsWith(".SetCase") || o.Key == "RespCombo.Add") || seen.Add(o.Identity)).ToArray();
        var conflicts = ops.Where(o=>o.Stage<=30 && o.Arguments.ContainsKey("Name"))
            .GroupBy(o=>(o.Key,o.Arguments["Name"].GetString())).FirstOrDefault(g=>g.Select(o=>o.Identity).Distinct().Count()>1);
        if(conflicts!=null) throw new ArgumentException($"Conflicting definition: {conflicts.Key}.");
        return new Fragment(ops,elements.Values,definition.NativeSource);
    }
    public SapModel WithResults(ResultSet results) => new(Definition,Units,Tolerance,results);
    public SapModel WithoutResults() => new(Definition,Units,Tolerance);
    public override string ToString() => $"SAP Model: {Definition.Elements.Count} elements, units {Units}"+(Results==null?"":$", {Results.Tables.Count} result tables");
    public static SapModel Merge(IEnumerable<SapModel> models, IEnumerable<Fragment>? additions = null)
    {
        var list=models.ToArray(); if(list.Length==0) return new SapModel(Fragment.Combine(additions??[]));
        if(list.Any(m=>m.Units!=list[0].Units || m.Tolerance!=list[0].Tolerance)) throw new ArgumentException("Models must use identical units and tolerance before merging.");
        return new SapModel(Fragment.Combine(list.Select(m=>m.Definition).Concat(additions??[])),list[0].Units,list[0].Tolerance);
    }
}

public static class ModelValidation
{
    public static IReadOnlyList<string> Errors(SapModel model)
    {
        var errors=new List<string>();
        foreach(var e in model.Definition.Elements)
        {
            if(string.IsNullOrWhiteSpace(e.Name)) errors.Add("Element names cannot be empty.");
            if(e.Points.Any(p=>!p.IsFinite)) errors.Add($"{e.Name}: non-finite coordinates.");
            // Native topology (including zero-length links and polygonal areas) is kept in the SDB.
            if(model.NativeSource != null) continue;
            int count=e.Points.Count;
            if(e.Kind==ElementKind.Node && count!=1 || e.Kind is ElementKind.Frame or ElementKind.Link or ElementKind.Cable && count!=2 || e.Kind==ElementKind.Area && count is not (3 or 4) || e.Kind==ElementKind.Solid && count!=8)
                errors.Add($"{e.Name}: invalid vertex count for {e.Kind}.");
            if(e.Kind!=ElementKind.Solid && e.Points.SelectMany((p,i)=>e.Points.Skip(i+1).Select(q=>p.DistanceTo(q))).Any(d=>d<=model.Tolerance)) errors.Add($"{e.Name}: coincident vertices.");
            if(e.Kind!=ElementKind.Node && string.IsNullOrWhiteSpace(e.Property)) errors.Add($"{e.Name}: a named property is required.");
            if(e.Kind==ElementKind.Area && count>=3)
            {
                var a=e.Points[0]; var b=e.Points[1]; var c=e.Points[2];
                double nx=(b.Y-a.Y)*(c.Z-a.Z)-(b.Z-a.Z)*(c.Y-a.Y), ny=(b.Z-a.Z)*(c.X-a.X)-(b.X-a.X)*(c.Z-a.Z), nz=(b.X-a.X)*(c.Y-a.Y)-(b.Y-a.Y)*(c.X-a.X);
                double norm=Math.Sqrt(nx*nx+ny*ny+nz*nz);
                if(norm<=model.Tolerance*model.Tolerance) errors.Add($"{e.Name}: degenerate area.");
                else if(count==4 && Math.Abs(nx*(e.Points[3].X-a.X)+ny*(e.Points[3].Y-a.Y)+nz*(e.Points[3].Z-a.Z))/norm>model.Tolerance) errors.Add($"{e.Name}: area vertices are not coplanar.");
            }
        }
        foreach(var op in model.Definition.Operations)
        {
            try{ApiSchema.ValidateArguments(op.Key,op.Arguments);}catch(Exception ex){errors.Add(ex.Message);}
            if(op.Arguments.TryGetValue("ItemType",out var selection)&&selection.ValueKind==JsonValueKind.Number&&selection.GetInt32()==2)errors.Add($"{op.Key}: Selection is unavailable in an isolated model. Use Object=0 or Group=1.");
            foreach(var pair in op.Arguments)
                if(pair.Value.ValueKind==JsonValueKind.Array && pair.Key is "Value" or "DOF" or "Fixed" && op.Key is "PointObj.SetRestraint" or "PointObj.SetMass" or "PointObj.SetSpring" or "PointObj.SetLoadForce" or "PointObj.SetLoadDispl" or "PropLink.SetLinear")
                    if(pair.Value.GetArrayLength()!=6)errors.Add($"{op.Key}.{pair.Key}: expected six values (U1,U2,U3,R1,R2,R3).");
        }
        return errors;
    }
    public static void RequireValid(SapModel model) { var errors=Errors(model); if(errors.Count>0)throw new ArgumentException(string.Join(Environment.NewLine,errors)); }
}
