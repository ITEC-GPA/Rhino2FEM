using System.Text.Json;
using Rhino.Geometry;
using Rhino2MidasGen.Core;
using Data=Rhino2MidasGen.Core.Native;

namespace Rhino2MidasGen.Grasshopper;

public static class PhysicalGeometry
{
    public sealed record Solid(string Name,Brep Brep);
    public static IEnumerable<JsonElement> Items(Fragment fragment,string endpoint,int id)=>fragment.Entries.Where(e=>e.Endpoint==endpoint&&e.Id==id).SelectMany(e=>e.IsItem?new[]{e.Data}:e.Data.TryGetProperty("ITEMS",out var a)?a.EnumerateArray().ToArray():new[]{e.Data});
    public static (Vector3d X,Vector3d Y,Vector3d Z) Axes(Element e)
    {
        var x=Preview.Point(e.Points[1])-Preview.Point(e.Points[0]);if(!x.Unitize())throw new ArgumentException("Zero-length element.");
        var y=Vector3d.CrossProduct(Vector3d.ZAxis,x);if(!y.Unitize())y=Vector3d.CrossProduct(Vector3d.XAxis,x);
        y.Rotate(e.Angle*Math.PI/180,x);return(x,y,Vector3d.CrossProduct(x,y));
    }
    public static GeometryBase Analytical(Element e)=>e.Kind==ElementKind.Node?new Rhino.Geometry.Point(Preview.Point(e.Points[0])):e.Points.Count==2?new LineCurve(Preview.Point(e.Points[0]),Preview.Point(e.Points[1])):Preview.Mesh(e);
    public static IReadOnlyList<Solid> CreateSolids(MidasModel model,out List<string> issues)
    {
        var output=new List<Solid>();issues=[];
        foreach(var e in model.Definition.Elements.Where(e=>e.Kind is not (ElementKind.Node or ElementKind.ElasticLink)))
        {
            try
            {
                var solid=e.Kind is ElementKind.Plate or ElementKind.Wall?Plate(e,model):e.Kind==ElementKind.Solid?Brep.CreateFromMesh(Preview.Mesh(e),true):Beam(e,model);
                if(solid==null||!solid.IsValid||!solid.IsSolid)throw new ArgumentException("No valid closed solid.");
                if(solid.SolidOrientation==BrepSolidOrientation.Inward)solid.Flip();output.Add(new(e.ToString(),solid));
            }
            catch(Exception ex){issues.Add(e+": "+ex.Message+" Omitted from Breps.");}
        }
        return output;
    }
    private static Brep Plate(Element e,MidasModel model)
    {
        var property=model.Definition.Entries.FirstOrDefault(p=>p.Endpoint=="THIK"&&p.Id==e.Property)??throw new ArgumentException("No thickness definition.");
        if(Data.String(property.Data,"TYPE")!="VALUE")throw new ArgumentException("Stiffened plate preview unavailable.");
        double t=Data.Number(property.Data,"T_IN"),offset=Data.Number(property.Data,"O_VALUE");int type=(int)Data.Number(property.Data,"OFFSET");if(type==0)offset=0;else if(type==1)offset*=t;
        if(t<=0)throw new ArgumentException("Nonpositive thickness.");var points=e.Points.Select(Preview.Point).ToArray();var normal=Vector3d.CrossProduct(points[1]-points[0],points[2]-points[0]);if(!normal.Unitize())throw new ArgumentException("Degenerate plate.");
        if(points.Any(p=>Math.Abs((p-points[0])*normal)>model.Tolerance))throw new ArgumentException("Nonplanar plate cannot be extruded as a constant thickness solid.");
        var bottom=points.Select(p=>p+normal*(offset-t/2)).ToArray();using var curve=new Polyline(bottom.Append(bottom[0])).ToNurbsCurve();
        using var surface=Surface.CreateExtrusion(curve,normal*t);using var open=surface.ToBrep();return open.CapPlanarHoles(model.Tolerance)??throw new ArgumentException("Plate caps failed.");
    }
    public static IReadOnlyList<Curve> Profile(JsonElement data)
    {
        var before=Data.Object(data,"SECT_BEFORE");var section=Data.Object(before,"SECT_I");
        if(Data.String(data,"SECTTYPE")!="DBUSER"||Data.Number(before,"DATATYPE")!=2)throw new ArgumentException("Physical profile requires a user-dimension DBUSER section; native database/PSC/composite sections remain analytical.");
        string shape=Data.String(before,"SHAPE");var d=Data.Numbers(section,"vSIZE");Array.Resize(ref d,10);
        double h=d[0],b=d[1],tw=d[2],tf=d[3],b2=d[4]>0?d[4]:b,tf2=d[5]>0?d[5]:tf;
        if(d[6]!=0||d[7]!=0)throw new ArgumentException("Filleted profile requires native section geometry; it is not approximated as an exact solid.");
        Curve Polygon(params (double Y,double Z)[] p)=>new Polyline(p.Select(v=>new Point3d(v.Y,v.Z,0)).Append(new Point3d(p[0].Y,p[0].Z,0))).ToNurbsCurve();
        if(shape=="SR")return[new Circle(Plane.WorldXY,h/2).ToNurbsCurve()];
        if(shape=="P")return[new Circle(Plane.WorldXY,h/2).ToNurbsCurve(),new Circle(Plane.WorldXY,h/2-b).ToNurbsCurve()];
        if(shape=="SB")return[Polygon((-b/2,-h/2),(b/2,-h/2),(b/2,h/2),(-b/2,h/2))];
        if(shape=="B")
        {
            if(Math.Abs(b2-b)>1e-10)throw new ArgumentException("Tapered box physical preview not implemented.");
            return[Polygon((-b/2,-h/2),(b/2,-h/2),(b/2,h/2),(-b/2,h/2)),Polygon((-b/2+tw,-h/2+tf2),(-b/2+tw,h/2-tf),(b/2-tw,h/2-tf),(b/2-tw,-h/2+tf2))];
        }
        if(shape=="H")return[Polygon((-b2/2,0),(b2/2,0),(b2/2,tf2),(tw/2,tf2),(tw/2,h-tf),(b/2,h-tf),(b/2,h),(-b/2,h),(-b/2,h-tf),(-tw/2,h-tf),(-tw/2,tf2),(-b2/2,tf2))];
        if(shape=="T")return[Polygon((-tw/2,0),(tw/2,0),(tw/2,h-tf),(b/2,h-tf),(b/2,h),(-b/2,h),(-b/2,h-tf),(-tw/2,h-tf))];
        if(shape=="C")return[Polygon((0,0),(b2,0),(b2,tf2),(tw,tf2),(tw,h-tf),(b,h-tf),(b,h),(0,h))];
        if(shape=="L")return[Polygon((0,0),(b,0),(b,tf),(tw,tf),(tw,h),(0,h))];
        throw new ArgumentException("Physical preview unsupported for native shape "+shape);
    }
    private static Brep Beam(Element e,MidasModel model)
    {
        var definition=model.Definition.Entries.FirstOrDefault(p=>p.Endpoint=="SECT"&&p.Id==e.Property)??throw new ArgumentException("Missing section.");
        var curves=Profile(definition.Data).Select(c=>c.DuplicateCurve()).ToArray();
        var profiles=Brep.CreatePlanarBreps(curves,model.Tolerance);if(profiles==null||profiles.Length!=1)throw new ArgumentException("Section profile is not one planar region.");
        using var profile=profiles[0];using var properties=AreaMassProperties.Compute(profile);var centroid=properties.Centroid;var bounds=profile.GetBoundingBox(true);var before=Data.Object(definition.Data,"SECT_BEFORE");string insertion=Data.String(before,"OFFSET_PT","CC");
        double y=centroid.X,z=centroid.Y;
        if(insertion.Length==2){if(insertion[0]=='L')y=bounds.Min.X;else if(insertion[0]=='R')y=bounds.Max.X;if(insertion[1]=='T')z=bounds.Max.Y;else if(insertion[1]=='B')z=bounds.Min.Y;}
        profile.Transform(Transform.Translation(-y,-z,0));
        var axes=Axes(e);var start=Preview.Point(e.Points[0]);var end=Preview.Point(e.Points[1]);var offsets=Items(model.Definition,"OFFS",e.Id).ToArray();
        if(offsets.Length>1)throw new ArgumentException("Multiple staged offsets require selecting an active GEN stage.");
        if(offsets.Length==1)
        {
            var o=offsets[0];Vector3d Offset(string suffix){var v=new Vector3d(Data.Number(o,"RGDX"+suffix),Data.Number(o,"RGDY"+suffix),Data.Number(o,"RGDZ"+suffix));return Data.String(o,"TYPE")=="LOCAL"?axes.X*v.X+axes.Y*v.Y+axes.Z*v.Z:v;}
            start+=Offset("i");end+=Offset("j");
        }
        var extrusionDirection=end-start;if(extrusionDirection.Length<model.Tolerance)throw new ArgumentException("Offsets collapse beam length.");
        profile.Transform(Transform.PlaneToPlane(Plane.WorldXY,new Plane(start,axes.Y,axes.Z)));
        return profile.Faces[0].CreateExtrusion(new LineCurve(start,end),true)??throw new ArgumentException("Beam extrusion failed.");
    }
}
