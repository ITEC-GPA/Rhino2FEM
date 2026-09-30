using System.Text.Json;
using Rhino2SAP.Core;

namespace Rhino2SAP.Api;

public static class ModelWriter
{
    public static IReadOnlyDictionary<string,Position> Write(ISapClient client,SapModel model)
    {
        ModelValidation.RequireValid(model);
        if(model.NativeSource != null)throw new InvalidOperationException("Use AnalysisService to export an imported native Model without reconstructing or losing its source definitions.");
        client.Call("InitializeNewModel",ApiSchema.Args(("Units",model.Units)));
        client.Call("File.NewBlank");
        client.Call("SetPresentUnits",ApiSchema.Args(("Units",model.Units)));
        foreach(var op in model.Definition.Operations.Where(o=>o.Stage<40).OrderBy(o=>o.Stage))
        {
            var output=client.Call(op.Key,op.Arguments);
            if(op.Key=="PropMaterial.AddMaterial"&&output["Name"].GetString()!=op.Arguments["UserName"].GetString())throw new InvalidOperationException("SAP changed the requested library material name.");
        }
        var points=new List<(Position Point,string Name)>();var aliases=new Dictionary<string,string>(StringComparer.Ordinal);
        string AddPoint(Position point,string? name=null)
        {
            var existing=points.FirstOrDefault(p=>p.Point.DistanceTo(point)<=model.Tolerance);
            if(existing.Name!=null){if(name!=null)aliases[name]=existing.Name;return existing.Name;}
            string requested=name??"R2S_J"+(points.Count+1);
            while(points.Any(p=>p.Name==requested)||model.Definition.Elements.Any(e=>e.Kind==ElementKind.Node&&e.Name==requested)&&name==null)requested+="_";
            var output=client.Call("PointObj.AddCartesian",ApiSchema.Args(("X",point.X),("Y",point.Y),("Z",point.Z),("Name",""),("UserName",requested),("MergeOff",true)));
            string actual=output["Name"].GetString()!;
            if(actual!=requested)throw new InvalidOperationException($"SAP renamed point '{requested}' to '{actual}'. Export aborted to preserve identifiers.");
            points.Add((point,actual));if(name!=null)aliases[name]=actual;return actual;
        }
        foreach(var e in model.Definition.Elements.Where(e=>e.Kind==ElementKind.Node))AddPoint(e.Points[0],e.Name);
        foreach(var e in model.Definition.Elements.Where(e=>e.Kind!=ElementKind.Node))
        {
            string[] nodes=e.Points.Select(p=>AddPoint(p)).ToArray();string objectPath=e.Kind+"Obj";
            var args=new Dictionary<string,JsonElement>{{"Name",ApiSchema.Value("")},{"UserName",ApiSchema.Value(e.Name)},{"PropName",ApiSchema.Value(e.Property)}};
            if(e.Kind is ElementKind.Area or ElementKind.Solid)
            {args["Point"]=ApiSchema.Value(nodes);if(e.Kind==ElementKind.Area)args["NumberPoints"]=ApiSchema.Value(nodes.Length);}
            else{args["Point1"]=ApiSchema.Value(nodes[0]);args["Point2"]=ApiSchema.Value(nodes[1]);}
            var output=client.Call(objectPath+".AddByPoint",args);
            if(output["Name"].GetString()!=e.Name)throw new InvalidOperationException($"SAP did not preserve {e.Kind} name '{e.Name}'.");
        }
        foreach(var op in model.Definition.Operations.Where(o=>o.Stage>=40).OrderBy(o=>o.Stage))
        {
            var args=op.Arguments.ToDictionary();
            if(op.Key.StartsWith("PointObj.")&&args.TryGetValue("Name",out var name)&&aliases.TryGetValue(name.GetString()!,out var actual))args["Name"]=ApiSchema.Value(actual);
            client.Call(op.Key,args);
        }
        return points.ToDictionary(p=>p.Name,p=>p.Point);
    }
}
