using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public abstract class ElementComponent:SafeComponent
{
    protected abstract ElementKind Kind { get; }
    protected ElementComponent(string name):base(name,name,"Create a SAP element from Rhino geometry. Connect its named property definition to collect dependencies.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddTextParameter("Name","N","Unique SAP object name.",GH_ParamAccess.item);
        if(Kind==ElementKind.Node)p.AddPointParameter("Point","P","Node point.",GH_ParamAccess.item);
        else if(Kind is ElementKind.Frame or ElementKind.Link or ElementKind.Cable)p.AddLineParameter("Line","L","Object axis (I to J).",GH_ParamAccess.item);
        else p.AddPointParameter("Vertices","V",Kind==ElementKind.Area?"3 or 4 ordered planar vertices.":"8 ordered SAP solid vertices; repeat vertices only for documented degenerate solid forms.",GH_ParamAccess.list);
        if(Kind!=ElementKind.Node)p.AddGenericParameter("Property","P","Named SAP property fragment or property name.",GH_ParamAccess.item);
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Element","E","Element and dependencies.",GH_ParamAccess.item);p.AddTextParameter("Name","N","SAP object name.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da)
    {
        string name=Item<string>(da,0);var points=new List<Point3d>();
        if(Kind==ElementKind.Node)points.Add(Item<Point3d>(da,1));else if(Kind is ElementKind.Frame or ElementKind.Link or ElementKind.Cable){var l=Item<Line>(da,1);points.Add(l.From);points.Add(l.To);}else da.GetDataList(1,points);
        Fragment prior=new();string property="";
        if(Kind!=ElementKind.Node){var raw=Unwrap(Item<object>(da,2));if(raw is Fragment f){prior=f;property=f.Operations.LastOrDefault(o=>o.Key.StartsWith("Prop")&&o.Arguments.ContainsKey("Name"))?.Arguments["Name"].GetString()??throw new ArgumentException("No named property in the supplied fragment.");}else property=Convert.ToString(raw)??"";}
        var element=new Element(name,Kind,points.Select(Preview.Position),property);Output(da,0,Fragment.Combine([prior,new Fragment(elements:[element])]));da.SetData(1,name);
    }
}
public sealed class NodeElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Node;public NodeElementComponent():base("SAP Node Element"){} }
public sealed class FrameElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Frame;public FrameElementComponent():base("SAP Frame Element"){} }
public sealed class AreaElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Area;public AreaElementComponent():base("SAP Area Element"){} }
public sealed class SolidElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Solid;public SolidElementComponent():base("SAP Solid Element"){} }
public sealed class LinkElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Link;public LinkElementComponent():base("SAP Link Element"){} }
public sealed class CableElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Cable;public CableElementComponent():base("SAP Cable Element"){} }

public sealed class MeshAreasComponent:SafeComponent
{
    public MeshAreasComponent():base("Mesh to SAP Areas","Mesh Areas","Create one area per triangle or quad. Use planar faces.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddMeshParameter("Mesh","M","Mesh.",GH_ParamAccess.item);p.AddTextParameter("Prefix","N","Object name prefix.",GH_ParamAccess.item,"A");p.AddGenericParameter("Property","P","Named area property fragment.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Elements","E","Area elements and property dependencies.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var mesh=Item<Mesh>(da,0);string prefix=Item<string>(da,1);var prop=Native<Fragment>(da,2);string name=prop.Operations.Last(o=>o.Key.StartsWith("PropArea.")).Arguments["Name"].GetString()!;var elements=mesh.Faces.Select((f,i)=>new Element(prefix+(i+1),ElementKind.Area,(f.IsTriangle?new[]{f.A,f.B,f.C}:new[]{f.A,f.B,f.C,f.D}).Select(v=>Preview.Position(mesh.Vertices.Point3dAt(v))),name));Output(da,0,Fragment.Combine([prop,new Fragment(elements:elements)]));}
}
public sealed class PolylineFramesComponent:SafeComponent
{
    public PolylineFramesComponent():base("Polyline to SAP Frames","Polyline Frames","One frame per straight polyline segment.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddCurveParameter("Polyline","L","Polyline curve.",GH_ParamAccess.item);p.AddTextParameter("Prefix","N","Name prefix.",GH_ParamAccess.item,"F");p.AddGenericParameter("Property","P","Named frame section.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Elements","E","Frames and property dependencies.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var curve=Item<Curve>(da,0);if(!curve.TryGetPolyline(out var polyline))throw new ArgumentException("Use a polyline curve.");var prop=Native<Fragment>(da,2);string name=prop.Operations.Last(o=>o.Key.StartsWith("PropFrame.")).Arguments["Name"].GetString()!;string prefix=Item<string>(da,1);Output(da,0,Fragment.Combine([prop,new Fragment(elements:polyline.GetSegments().Select((l,i)=>new Element(prefix+(i+1),ElementKind.Frame,[Preview.Position(l.From),Preview.Position(l.To)],name)))]));}
}
public sealed class ElementInfoComponent:SafeComponent
{
    public ElementInfoComponent():base("SAP Element Info","Element Info","Decompose element names, kinds, properties and vertices.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Definition","D","Elements or Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Names","N","Names.",GH_ParamAccess.list);p.AddTextParameter("Kinds","K","Kinds.",GH_ParamAccess.list);p.AddTextParameter("Properties","P","Property names.",GH_ParamAccess.list);p.AddGenericParameter("Elements","E","Individual element fragments.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));da.SetDataList(0,f.Elements.Select(e=>e.Name));da.SetDataList(1,f.Elements.Select(e=>e.Kind.ToString()));da.SetDataList(2,f.Elements.Select(e=>e.Property));da.SetDataList(3,f.Elements.Select(e=>new FragmentGoo(new Fragment(f.Operations,[e],f.NativeSource))));}
}
public sealed class ElementFilterComponent:SafeComponent
{
    public ElementFilterComponent():base("Filter SAP Elements","Filter","Filter by kind and name; preserve property dependencies. Assignment operations should be added after filtering.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definition","D","Elements/Model.",GH_ParamAccess.item);p.AddIntegerParameter("Kind","K","-1 all; 0 Node, 1 Frame, 2 Area, 3 Solid, 4 Link, 5 Cable.",GH_ParamAccess.item,-1);p.AddTextParameter("Names","N","Names; empty = all.",GH_ParamAccess.list);p[2].Optional=true;}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Definition","D","Filtered elements and property definitions.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));int kind=Item<int>(da,1);var names=new List<string>();da.GetDataList(2,names);Output(da,0,new Fragment(f.Operations.Where(o=>o.Stage<40),f.Elements.Where(e=>(kind<0||(int)e.Kind==kind)&&(names.Count==0||names.Contains(e.Name))),f.NativeSource));}
}
