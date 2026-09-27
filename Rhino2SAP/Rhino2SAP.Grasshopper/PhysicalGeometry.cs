using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public static class PhysicalGeometry
{
    public sealed record Solid(string Name,Brep Brep);

    /// <summary>Only actual closed volumes; analytical fallback geometry never enters this output.</summary>
    public static IReadOnlyList<Solid> CreateSolids(SapModel model,out List<string> issues)
    {
        var solids=new List<Solid>();issues=[];
        foreach(var e in model.Definition.Elements.Where(e=>e.Kind!=ElementKind.Node))
        {
            try{solids.Add(new(e.Name,CreateSolid(e,model)));}
            catch(Exception ex){issues.Add(e.Name+": "+ex.Message+" Omitted from solid Breps.");}
        }
        return solids;
    }

    private static Brep CreateSolid(Element e,SapModel model)
    {
        Brep brep=e.Kind switch
        {
            ElementKind.Frame=>Frame(e,model),
            ElementKind.Area=>Area(e,model),
            ElementKind.Solid=>SolidElement(e),
            _=>throw new ArgumentException("No physical volume defined for "+e.Kind+".")
        };
        if(!brep.IsValid||!brep.IsSolid){brep.Dispose();throw new ArgumentException("Physical geometry is not a valid closed Brep.");}
        if(brep.SolidOrientation==BrepSolidOrientation.Inward)brep.Flip();
        if(brep.SolidOrientation!=BrepSolidOrientation.Outward){brep.Dispose();throw new ArgumentException("Cannot determine solid orientation.");}
        return brep;
    }

    private static Brep SolidElement(Element e)
    {
        if(e.Points.Count!=8)throw new ArgumentException("Solid geometry requires eight ordered vertices.");
        using var mesh=Preview.Mesh(e);
        return Brep.CreateFromMesh(mesh,true)??throw new ArgumentException("Solid mesh to Brep conversion failed.");
    }

    private static Brep Area(Element e,SapModel model)
    {
        var property=model.Definition.Operations.LastOrDefault(o=>o.Key is "PropArea.SetShell" or "PropArea.SetShell_1"&&o.Arguments["Name"].GetString()==e.Property);
        if(property==null||model.Definition.Operations.Any(o=>o.Key=="AreaObj.SetOffsets"&&o.Arguments["Name"].GetString()==e.Name))throw new ArgumentException("Area thickness/offset preview unavailable for this property.");
        double thickness=property.Arguments["Thickness"].GetDouble();
        if(!double.IsFinite(thickness)||thickness<=0)throw new ArgumentException("Nonpositive or invalid shell thickness.");
        if(e.Points.Count is not (3 or 4))throw new ArgumentException("Shell geometry requires three or four planar vertices.");
        var points=e.Points.Select(Preview.Point).ToArray();var normal=Vector3d.CrossProduct(points[1]-points[0],points[2]-points[0]);
        if(!normal.Unitize())throw new ArgumentException("Degenerate shell geometry.");
        if(points.Any(p=>Math.Abs((p-points[0])*normal)>model.Tolerance))throw new ArgumentException("Shell vertices are not coplanar.");
        using var curve=new Polyline(points.Select(p=>p-normal*thickness/2).Append(points[0]-normal*thickness/2)).ToNurbsCurve();
        using var surface=Surface.CreateExtrusion(curve,normal*thickness)??throw new ArgumentException("Shell extrusion failed.");
        using var open=surface.ToBrep();
        return open.CapPlanarHoles(model.Tolerance)??throw new ArgumentException("Shell caps could not be created.");
    }

    public static (Vector3d One,Vector3d Two,Vector3d Three) FrameAxes(Element e,Fragment definition)
    {
        var one=Preview.Point(e.Points[1])-Preview.Point(e.Points[0]);if(!one.Unitize())throw new ArgumentException("Zero-length frame.");
        var two=Vector3d.ZAxis-one*one.Z;if(two.Length<1e-8)two=Vector3d.XAxis;two.Unitize();
        var advanced=definition.Operations.LastOrDefault(o=>o.Key=="FrameObj.SetLocalAxesAdvanced"&&o.Arguments["Name"].GetString()==e.Name);
        if(advanced!=null&&advanced.Arguments["Active"].GetBoolean())
        {
            if(advanced.Arguments["PlVectOpt"].GetInt32()!=3||advanced.Arguments["PlCSys"].GetString()!="Global")throw new ArgumentException("Physical preview requires a global user-vector advanced axis; query SAP for other axis definitions.");
            var a=advanced.Arguments["PlVect"];var reference=new Vector3d(a[0].GetDouble(),a[1].GetDouble(),a[2].GetDouble());reference-=one*(one*reference);if(!reference.Unitize())throw new ArgumentException("Invalid advanced axes.");two=advanced.Arguments["Plane2"].GetInt32()==12?reference:Vector3d.CrossProduct(reference,one);
        }
        var angle=definition.Operations.LastOrDefault(o=>o.Key=="FrameObj.SetLocalAxes"&&o.Arguments["Name"].GetString()==e.Name);
        if(angle!=null)two.Rotate(angle.Arguments["Ang"].GetDouble()*Math.PI/180,one);
        return(one,two,Vector3d.CrossProduct(one,two));
    }
    public static IReadOnlyList<GeometryBase> Create(SapModel model,out List<string> issues)
    {
        var geometry=new List<GeometryBase>();issues=[];
        foreach(var e in model.Definition.Elements)
        {
            try
            {
                if(e.Kind is ElementKind.Frame or ElementKind.Area or ElementKind.Solid){geometry.Add(CreateSolid(e,model));continue;}
                geometry.Add(AnalysisGeometry(e));
            }
            catch(Exception ex){issues.Add(e.Name+": "+ex.Message+" Showing analysis geometry.");geometry.Add(AnalysisGeometry(e));}
        }
        return geometry;
    }
    public static GeometryBase AnalysisGeometry(Element e)=>e.Kind==ElementKind.Node?new Rhino.Geometry.Point(Preview.Point(e.Points[0])):e.Points.Count==2?new LineCurve(Preview.Point(e.Points[0]),Preview.Point(e.Points[1])):Preview.Mesh(e);
    private static Brep Frame(Element e,SapModel model)
    {
        if(model.Definition.Operations.Any(o=>o.Key is "FrameObj.SetInsertionPoint" or "FrameObj.SetEndLengthOffset"&&o.Arguments["Name"].GetString()==e.Name))throw new ArgumentException("Physical preview of insertion/end offsets requires native geometry.");
        var section=model.Definition.Operations.LastOrDefault(o=>o.Key.StartsWith("PropFrame.Set")&&o.Arguments.TryGetValue("Name",out var n)&&n.GetString()==e.Property&&o.Arguments.ContainsKey("MatProp"))??throw new ArgumentException("Section definition not found.");
        double Get(string name)=>section.Arguments[name].GetDouble();
        var axes=FrameAxes(e,model.Definition);var plane=new Plane(Preview.Point(e.Points[0]),axes.Two,axes.Three);var curves=new List<Curve>();
        Curve Polygon(params (double X,double Y)[] vertices)=>new Polyline(vertices.Select(p=>plane.PointAt(p.X,p.Y)).Append(plane.PointAt(vertices[0].X,vertices[0].Y))).ToNurbsCurve();
        string shape=section.Key["PropFrame.Set".Length..];double d=Get("T3");
        if(d<=0)throw new ArgumentException("Section depth must be positive.");
        if(shape is "Circle" or "Pipe")
        {
            curves.Add(new Circle(plane,d/2).ToNurbsCurve());
            if(shape=="Pipe"){double t=Get("TW");if(t<=0||2*t>=d)throw new ArgumentException("Invalid pipe thickness.");curves.Add(new Circle(plane,d/2-t).ToNurbsCurve());}
        }
        else
        {
            double b=Get("T2");if(b<=0)throw new ArgumentException("Section width must be positive.");
            if(shape is "Rectangle" or "Tube")
            {
                curves.Add(Polygon((-d/2,-b/2),(d/2,-b/2),(d/2,b/2),(-d/2,b/2)));
                if(shape=="Tube"){double tf=Get("Tf"),tw=Get("Tw");if(tf<=0||tw<=0||tf*2>=d||tw*2>=b)throw new ArgumentException("Invalid tube thickness.");curves.Add(Polygon((-d/2+tf,-b/2+tw),(-d/2+tf,b/2-tw),(d/2-tf,b/2-tw),(d/2-tf,-b/2+tw)));}
            }
            else
            {
                double tf=Get("Tf"),tw=Get("Tw");if(tf<=0||tw<=0||tf>=d||tw>=b)throw new ArgumentException("Invalid flange/web thickness.");
                switch(shape)
                {
                    case "ISection":double bb=Get("T2b"),tbf=Get("Tfb");curves.Add(Polygon((-d/2,-bb/2),(-d/2+tbf,-bb/2),(-d/2+tbf,-tw/2),(d/2-tf,-tw/2),(d/2-tf,-b/2),(d/2,-b/2),(d/2,b/2),(d/2-tf,b/2),(d/2-tf,tw/2),(-d/2+tbf,tw/2),(-d/2+tbf,bb/2),(-d/2,bb/2)));break;
                    case "Tee":curves.Add(Polygon((-d/2,-tw/2),(d/2-tf,-tw/2),(d/2-tf,-b/2),(d/2,-b/2),(d/2,b/2),(d/2-tf,b/2),(d/2-tf,tw/2),(-d/2,tw/2)));break;
                    case "Channel":curves.Add(Polygon((-d/2,-b/2),(d/2,-b/2),(d/2,b/2),(d/2-tf,b/2),(d/2-tf,-b/2+tw),(-d/2+tf,-b/2+tw),(-d/2+tf,b/2),(-d/2,b/2)));break;
                    case "Angle":curves.Add(Polygon((-d/2,-b/2),(d/2,-b/2),(d/2,-b/2+tw),(-d/2+tf,-b/2+tw),(-d/2+tf,b/2),(-d/2,b/2)));break;
                    default:throw new ArgumentException("No exact physical profile implementation for "+shape);
                }
            }
        }
        var profile=Brep.CreatePlanarBreps(curves,model.Tolerance)?.SingleOrDefault()??throw new ArgumentException("Invalid section profile.");
        var center=AreaMassProperties.Compute(profile).Centroid;profile.Transform(Transform.Translation(plane.Origin-center));
        var solid=profile.Faces[0].CreateExtrusion(new LineCurve(plane.Origin,Preview.Point(e.Points[1])),true)??throw new ArgumentException("Section extrusion failed.");
        if(solid.SolidOrientation==BrepSolidOrientation.Inward)solid.Flip();return solid;
    }
}
public sealed class PhysicalGeometryComponent:SafeComponent
{
    public PhysicalGeometryComponent():base("SAP Model Physical Geometry","Physical Geometry","Extrude supported native frame profiles and shell thicknesses. Unsupported shapes or offsets return analytical geometry with explicit issues.","08-Display"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","SAP Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGeometryParameter("Geometry","G","Solid section geometry where implemented, otherwise analysis geometry.",GH_ParamAccess.list);p.AddTextParameter("Names","N","Object names in geometry order.",GH_ParamAccess.list);p.AddTextParameter("Issues","I","Explicit physical-preview limitations.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);var g=PhysicalGeometry.Create(m,out var issues);da.SetDataList(0,g);da.SetDataList(1,m.Definition.Elements.Select(e=>e.Name));da.SetDataList(2,issues);}
}
public sealed class BakeGeometryComponent:SafeComponent
{
    private bool wasBake;private Guid[] ids=[];private List<string> issues=[];
    public BakeGeometryComponent():base("Bake SAP Geometry","Bake","Bake analytical or physical geometry into Rhino as one undoable operation. Physical mode defaults to valid closed Breps only, with skipped elements listed in Issues.","08-Display"){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddGenericParameter("Model","M","SAP Model.",GH_ParamAccess.item);
        p.AddBooleanParameter("Physical","P","Use physical section and thickness geometry.",GH_ParamAccess.item,true);
        p.AddBooleanParameter("Bake","B","Rising edge bakes to Rhino.",GH_ParamAccess.item,false);
        p.AddBooleanParameter("Solid only","S","In Physical mode, bake only valid closed Breps; omit nodes, symbols and analytical fallbacks. Ignored in analytical mode.",GH_ParamAccess.item,true);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)
    {
        p.AddTextParameter("Object IDs","I","Baked Rhino object GUIDs.",GH_ParamAccess.list);
        p.AddTextParameter("Issues","W","Skipped elements, physical-geometry limitations and bake failures from the last bake.",GH_ParamAccess.list);
    }
    protected override void Solve(IGH_DataAccess da)
    {
        if(da.Iteration!=0)throw new ArgumentException("Bake one unified Model.");
        bool bake=Item<bool>(da,2),trigger=bake&&!wasBake;wasBake=bake;
        if(trigger)
        {
            ids=[];issues=[];
            var model=Native<SapModel>(da,0);var doc=Rhino.RhinoDoc.ActiveDoc??throw new ArgumentException("No active Rhino document.");
            IReadOnlyList<(string Name,GeometryBase Geometry)> geometry;
            if(Item<bool>(da,1)&&Item<bool>(da,3))
                geometry=PhysicalGeometry.CreateSolids(model,out issues).Select(s=>(s.Name,(GeometryBase)s.Brep)).ToArray();
            else
            {
                var all=Item<bool>(da,1)?PhysicalGeometry.Create(model,out issues):model.Definition.Elements.Select(PhysicalGeometry.AnalysisGeometry).ToArray();
                geometry=all.Select((g,i)=>(model.Definition.Elements[i].Name,g)).ToArray();
            }
            uint undo=doc.BeginUndoRecord("Bake SAP Geometry");var added=new List<Guid>();
            try
            {
                foreach(var item in geometry)
                {
                    var id=doc.Objects.Add(item.Geometry,new Rhino.DocObjects.ObjectAttributes{Name=item.Name});
                    if(id==Guid.Empty)issues.Add(item.Name+": Rhino rejected the geometry during bake.");else added.Add(id);
                }
                ids=added.ToArray();
            }
            finally{foreach(var item in geometry)item.Geometry.Dispose();doc.EndUndoRecord(undo);doc.Views.Redraw();}
        }
        if(issues.Count>0)AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,"Some elements could not be baked as solids. See Issues.");
        da.SetDataList(0,ids.Select(id=>id.ToString()));da.SetDataList(1,issues);
    }
}
