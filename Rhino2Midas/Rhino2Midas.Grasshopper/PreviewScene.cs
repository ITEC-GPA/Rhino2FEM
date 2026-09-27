using System.Drawing;
using System.Runtime.CompilerServices;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2Midas.Core;
using Data=Rhino2Midas.Core.Native;

namespace Rhino2Midas.Grasshopper;

public sealed class PreviewScene
{
    private static readonly ConditionalWeakTable<MidasModel,PreviewScene> Models=new();
    private static readonly ConditionalWeakTable<Fragment,PreviewScene> Fragments=new();
    private readonly MidasModel model;
    private readonly Lazy<(IReadOnlyList<PhysicalGeometry.Solid> Solids,List<string> Issues)> physical;
    public BoundingBox Bounds {get;private set;}
    public IReadOnlyList<string> Issues=>physical.Value.Issues.Concat(model.Definition.Entries.Where(e=>e.Endpoint=="SKEW").Select(e=>$"Node {e.Id}: local-axis loads are omitted from arrows; use native Civil preview to inspect their directions.")).ToArray();
    private PreviewScene(MidasModel model){this.model=model;physical=new(()=>{var solids=PhysicalGeometry.CreateSolids(model,out var issues);return(solids,issues);});Bounds=Preview.Bounds(model.Definition.Elements);}
    public static PreviewScene For(MidasModel model)=>Models.GetValue(model,m=>new(m));
    public static PreviewScene For(Fragment fragment)=>Fragments.GetValue(fragment,f=>
    {
        var elements=f.Elements.ToList();var known=elements.Where(e=>e.Kind==ElementKind.Node).Select(e=>e.Id).ToHashSet();
        foreach(var e in f.Elements)for(int i=0;i<e.NodeIds.Count;i++)if(known.Add(e.NodeIds[i]))elements.Add(new(e.NodeIds[i],ElementKind.Node,[e.Points[i]]));
        if(elements.Count==0&&f.Entries.FirstOrDefault(e=>e.Endpoint=="SECT") is {} section)elements.Add(new(1,ElementKind.Beam,[new(0,0,0),new(1,0,0)],property:section.Id));
        return new(new(new(elements,f.Entries)));
    });
    public void Prepare(DisplayOptions options)
    {
        options.Validate();Bounds=Preview.Bounds(model.Definition.Elements);foreach(var s in physical.Value.Solids)Bounds=BoundingBox.Union(Bounds,s.Brep.GetBoundingBox(true));
        foreach(var arrow in Arrows(options)){Bounds=BoundingBox.Union(Bounds,new BoundingBox(arrow.Line.From,arrow.Line.To));}
        if(Bounds.IsValid)Bounds.Inflate(options.SymbolScale*2);
    }
    public IReadOnlyList<PhysicalGeometry.Solid> CopySolids(out List<string> issues){issues=Issues.ToList();return physical.Value.Solids.Select(s=>new PhysicalGeometry.Solid(s.Name,s.Brep.DuplicateBrep())).ToArray();}
    public Brep? CopySingleBrep()=>physical.Value.Solids.Count==1?physical.Value.Solids[0].Brep.DuplicateBrep():null;
    private IEnumerable<(Line Line,string Text)> Arrows(DisplayOptions o)
    {
        if(!o.Enabled||!o.Loads)yield break;
        foreach(var e in model.Definition.Elements)
        {
            foreach(string endpoint in new[]{"CNLD","BMLD","PRES"})foreach(var data in PhysicalGeometry.Items(model.Definition,endpoint,e.Id))
            {
                if((endpoint=="CNLD")!=(e.Kind==ElementKind.Node))continue;
                if(o.LoadPattern.Length>0&&Data.String(data,"LCNAME")!=o.LoadPattern)continue;
                if(endpoint=="CNLD")
                {
                    if(model.Definition.Entries.Any(a=>a.Endpoint=="SKEW"&&a.Id==e.Id))continue;
                    for(int i=0;i<6;i++){double value=Data.Number(data,new[]{"FX","FY","FZ","MX","MY","MZ"}[i]);if(value==0)continue;var v=new[]{Vector3d.XAxis,Vector3d.YAxis,Vector3d.ZAxis}[i%3]*value*o.LoadScale;var p=Preview.Point(e.Points[0]);yield return(new(p-v,p),(i>=3?"M ":"F ")+value.ToString("G4"));}
                }
                else if(endpoint=="BMLD"&&e.Points.Count==2)
                {
                    var axes=PhysicalGeometry.Axes(e);string dir=Data.String(data,"DIRECTION");var direction=dir switch{"GX"=>Vector3d.XAxis,"GY"=>Vector3d.YAxis,"GZ"=>Vector3d.ZAxis,"LX"=>axes.X,"LY"=>axes.Y,_=>axes.Z};
                    var values=Data.Numbers(data,"P");var distances=Data.Numbers(data,"D");if(values.Length<2||distances.Length<2)continue;
                    bool point=Data.String(data,"TYPE").StartsWith("CON");int count=point?1:5;
                    for(int i=0;i<count;i++){double t=point?0:i/4d,d=distances[0]+t*(distances[1]-distances[0]),value=values[0]+t*(values[1]-values[0]);var p=Preview.Point(e.Points[0])+(Preview.Point(e.Points[1])-Preview.Point(e.Points[0]))*d;yield return(new(p-direction*value*o.LoadScale,p),Data.String(data,"TYPE")+" "+value.ToString("G4"));}
                }
                else if(endpoint=="PRES"&&e.Kind==ElementKind.Plate)
                {
                    var p=new Point3d(e.Points.Average(v=>v.X),e.Points.Average(v=>v.Y),e.Points.Average(v=>v.Z));var normal=Vector3d.CrossProduct(Preview.Point(e.Points[1])-Preview.Point(e.Points[0]),Preview.Point(e.Points[2])-Preview.Point(e.Points[0]));normal.Unitize();string dir=Data.String(data,"DIRECTION");if(dir=="GX")normal=Vector3d.XAxis;else if(dir=="GY")normal=Vector3d.YAxis;else if(dir=="GZ")normal=Vector3d.ZAxis;var f=Data.Numbers(data,"FORCES");if(f.Length>0)yield return(new(p-normal*f[0]*o.LoadScale,p),f[0].ToString("G4"));
                }
            }
        }
    }
    public void DrawWires(DisplayPipeline display,DisplayOptions o,Color color)
    {
        if(!o.Enabled)return;
        foreach(var e in model.Definition.Elements)
        {
            var p=Preview.Point(e.Points[0]);
            if(o.Geometry){if(e.Kind==ElementKind.Node)display.DrawPoint(p,PointStyle.RoundSimple,3,color);else if(e.Points.Count==2)display.DrawLine(p,Preview.Point(e.Points[1]),color,2);else display.DrawMeshWires(Preview.Mesh(e),color);}
            if(o.Labels)display.Draw2dText(e.ToString(),color,p,false,12);
            if(o.Axes&&e.Points.Count==2&&e.Kind!=ElementKind.ElasticLink){var a=PhysicalGeometry.Axes(e);display.DrawArrow(new(p,p+a.X*o.SymbolScale),Color.Red);display.DrawArrow(new(p,p+a.Y*o.SymbolScale),Color.Green);display.DrawArrow(new(p,p+a.Z*o.SymbolScale),Color.Blue);}
            if(e.Kind==ElementKind.Node)
            {
                if(o.Supports&&PhysicalGeometry.Items(model.Definition,"CONS",e.Id).Any()){double s=o.SymbolScale;display.DrawPolyline(new[]{p,p+new Vector3d(-s,0,-s),p+new Vector3d(s,0,-s),p},Color.ForestGreen,2);if(o.Values)display.Draw2dText("Fix "+Data.String(PhysicalGeometry.Items(model.Definition,"CONS",e.Id).First(),"CONSTRAINT"),Color.ForestGreen,p,false,11);}
                if(o.Springs&&PhysicalGeometry.Items(model.Definition,"NSPR",e.Id).Any())display.DrawCircle(new Circle(new Plane(p,Vector3d.ZAxis),o.SymbolScale*.4),Color.DarkOrange,2);
                if(o.Masses&&PhysicalGeometry.Items(model.Definition,"NMAS",e.Id).Any())display.DrawPoint(p,PointStyle.Square,8,Color.Purple);
                if(o.Supports&&PhysicalGeometry.Items(model.Definition,"SDSP",e.Id).Any())display.Draw2dText("δ",Color.ForestGreen,p,false,15);
            }
            if(o.Releases&&e.Kind==ElementKind.Beam)foreach(var release in PhysicalGeometry.Items(model.Definition,"FRLS",e.Id)){if(Data.String(release,"FLAG_I").Contains('1'))display.DrawPoint(p,PointStyle.Circle,6,Color.DarkOrange);if(Data.String(release,"FLAG_J").Contains('1'))display.DrawPoint(Preview.Point(e.Points[1]),PointStyle.Circle,6,Color.DarkOrange);}
            if(o.Offsets&&e.Kind==ElementKind.Beam)foreach(var offset in PhysicalGeometry.Items(model.Definition,"OFFS",e.Id))
            {var axes=PhysicalGeometry.Axes(e);for(int i=0;i<2;i++){string suffix=i==0?"i":"j";var v=new Vector3d(Data.Number(offset,"RGDX"+suffix),Data.Number(offset,"RGDY"+suffix),Data.Number(offset,"RGDZ"+suffix));if(Data.String(offset,"TYPE")=="LOCAL")v=axes.X*v.X+axes.Y*v.Y+axes.Z*v.Z;if(v.Length>0){var at=Preview.Point(e.Points[i]);display.DrawLine(at,at+v,Color.DarkViolet,3);}}}
            if(o.OtherAttributes&&e.Kind!=ElementKind.Node){var names=model.Definition.Entries.Where(a=>a.Id==e.Id&&ModelValidation.TargetTable(a.Endpoint)=="ELEM"&&a.Endpoint is not ("FRLS" or "OFFS" or "BMLD" or "PRES")).Select(a=>a.Endpoint).Distinct().ToArray();if(names.Length>0)display.Draw2dText(string.Join(" / ",names),Color.Gray,p,false,10);}
        }
        if(o.Sections)foreach(var s in physical.Value.Solids)display.DrawBrepWires(s.Brep,color,1);
        foreach(var arrow in Arrows(o)){if(arrow.Line.Length>1e-12)display.DrawArrow(arrow.Line,Color.OrangeRed);if(o.Values)display.Draw2dText(arrow.Text,Color.OrangeRed,arrow.Line.From,false,10);}
    }
    public void DrawMeshes(DisplayPipeline display,DisplayOptions o,DisplayMaterial material){if(!o.Enabled)return;if(o.Sections){foreach(var s in physical.Value.Solids)display.DrawBrepShaded(s.Brep,material);}else if(o.Geometry){foreach(var e in model.Definition.Elements.Where(e=>e.Kind is ElementKind.Plate or ElementKind.Solid))display.DrawMeshShaded(Preview.Mesh(e),material);}}
    public (int Elements,int Arrows,int Solids) Diagnostics(DisplayOptions o)=>!o.Enabled?(0,0,0):(o.Geometry?model.Definition.Elements.Count:0,Arrows(o).Count(),o.Sections?physical.Value.Solids.Count:0);
}
