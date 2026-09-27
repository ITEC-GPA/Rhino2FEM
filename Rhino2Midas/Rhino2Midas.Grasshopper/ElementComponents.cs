using System.Text.Json;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Midas.Core;

namespace Rhino2Midas.Grasshopper;

public sealed class NodeElementComponent:SafeComponent
{
    public NodeElementComponent():base("Midas Node","Node","Explicit node. Preserve separate coincident node IDs using explicit connectivity on elements.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddPointParameter("Point","P","Rhino coordinate in Model length units.",GH_ParamAccess.item);p.AddIntegerParameter("ID","ID","Positive node ID.",GH_ParamAccess.item,1);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Node","N","Node fragment.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,new Fragment([new(Item<int>(da,1),ElementKind.Node,[Preview.Position(Item<Point3d>(da,0))])]));
}
public abstract class ElementComponent:SafeComponent
{
    protected abstract ElementKind Kind{get;}
    protected ElementComponent(string name):base("Midas "+name,name,"Create an immutable Civil element and collect connected material/section definitions.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        p.AddIntegerParameter("ID","ID","Positive element ID. Use unique IDs across element families.",GH_ParamAccess.item,1);
        p.AddPointParameter("Points","P","Ordered vertices, two for beam/truss, three/four for plate, four/six/eight for solid.",GH_ParamAccess.list);
        p.AddGenericParameter("Material","M","Material definition or native material ID.",GH_ParamAccess.item);
        p.AddGenericParameter("Property","S","Section/thickness definition or ID; solids use 0.",GH_ParamAccess.item);if(Kind==ElementKind.Solid)p[3].Optional=true;
        p.AddNumberParameter("Angle","A","Beta angle in degrees for line elements.",GH_ParamAccess.item,0);
        p.AddGenericParameter("Nodes","N","Optional ordered Node fragments for explicit connectivity. Points must match; supports coincident separate nodes.",GH_ParamAccess.list);p[5].Optional=true;
    }
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Element","E","Element with dependencies.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)
    {
        var deps=new List<Fragment>();int Resolve(int index,string endpoint,bool optional=false)
        {object raw=null!;if(!da.GetData(index,ref raw)){if(optional)return 0;throw new ArgumentException("Missing property.");}var value=Unwrap(raw);if(value is int id)return id;if(value is double number&&number==Math.Truncate(number))return checked((int)number);var fragment=FragmentOf(value);deps.Add(fragment);return Definitions.PropertyId(fragment,endpoint);}
        var points=new List<Point3d>();da.GetDataList(1,points);int count=points.Count;
        if(!(Kind==ElementKind.Plate?count is 3 or 4:Kind==ElementKind.Solid?count is 4 or 6 or 8:count==2))throw new ArgumentException("Invalid number of vertices.");
        int material=Resolve(2,"MATL"),property=Resolve(3,Kind==ElementKind.Plate?"THIK":"SECT",Kind==ElementKind.Solid);
        var rawNodes=new List<object>();da.GetDataList(5,rawNodes);var nodes=rawNodes.Select(FragmentOf).ToArray();deps.AddRange(nodes);
        var ids=nodes.SelectMany(n=>n.Elements.Where(e=>e.Kind==ElementKind.Node)).Select(n=>n.Id).ToArray();
        deps.Add(new([new(Item<int>(da,0),Kind,points.Select(Preview.Position).ToArray(),material,property,Item<double>(da,4),ids)]));Output(da,0,Fragment.Combine(deps));
    }
}
public sealed class BeamElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Beam;public BeamElementComponent():base("Beam"){} }
public sealed class PlateElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Plate;public PlateElementComponent():base("Plate"){} }
public sealed class SolidElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Solid;public SolidElementComponent():base("Solid"){} }
public sealed class TrussElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Truss;public TrussElementComponent():base("Truss"){} }
public sealed class TensionElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Tension;public TensionElementComponent():base("Tension Only"){} }
public sealed class CompressionElementComponent:ElementComponent{protected override ElementKind Kind=>ElementKind.Compression;public CompressionElementComponent():base("Compression Only"){} }
public sealed class ElasticLinkComponent:SafeComponent
{
    public ElasticLinkComponent():base("Midas Elastic Link","Elastic Link","Two-node Civil GEN or RIGID link with native stiffnesses.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Unique link ID.",GH_ParamAccess.item,1);p.AddGenericParameter("Nodes","N","Exactly two explicit Node fragments. Coincident coordinates supported.",GH_ParamAccess.list);p.AddNumberParameter("Stiffness","K","Six stiffnesses: translations F/L, rotations F·L/rad.",GH_ParamAccess.list);p.AddBooleanParameter("Rigid","R","Native rigid link.",GH_ParamAccess.item,false);p.AddTextParameter("Boundary group","G","Existing boundary group name, or blank.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Link","L","Link fragment.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var fragments=NativeList<Fragment>(da,1);var nodes=fragments.SelectMany(f=>f.Elements).Where(e=>e.Kind==ElementKind.Node).ToArray();if(nodes.Length!=2)throw new ArgumentException("Connect exactly two explicit nodes.");var k=new List<double>();da.GetDataList(2,k);Definitions.Six(k,true);var extra=JsonSerializer.Serialize(new{LINK=Item<bool>(da,3)?"RIGID":"GEN",R_S=new bool[6],SDR=k,bSHEAR=false,DR=new[]{.5,.5},BNGR_NAME=Item<string>(da,4)});Output(da,0,Fragment.Combine(fragments.Append(new([new(Item<int>(da,0),ElementKind.ElasticLink,nodes.Select(n=>n.Points[0]).ToArray(),nodeIds:nodes.Select(n=>n.Id).ToArray(),extra:extra)]))));}
}
public sealed class MeshToPlatesComponent:SafeComponent
{
    public MeshToPlatesComponent():base("Midas Mesh to Plates","Mesh Plates","Create triangle/quad plate elements from Rhino mesh faces.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddMeshParameter("Mesh","M","Rhino mesh.",GH_ParamAccess.item);p.AddIntegerParameter("First ID","ID","First element ID.",GH_ParamAccess.item,1);p.AddGenericParameter("Material","Ma","Material definition.",GH_ParamAccess.item);p.AddGenericParameter("Thickness","T","Thickness definition.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Plates","P","Plate fragments.",GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da){var mesh=Item<Mesh>(da,0);var material=Native<Fragment>(da,2);var thickness=Native<Fragment>(da,3);int first=Item<int>(da,1);var output=new List<FragmentGoo>();for(int i=0;i<mesh.Faces.Count;i++){var f=mesh.Faces[i];int[] ids=f.IsTriangle?[f.A,f.B,f.C]:[f.A,f.B,f.C,f.D];output.Add(new(Fragment.Combine([material,thickness,new([new(first+i,ElementKind.Plate,ids.Select(v=>Preview.Position(mesh.Vertices.Point3dAt(v))).ToArray(),Definitions.PropertyId(material,"MATL"),Definitions.PropertyId(thickness,"THIK"))])])));}da.SetDataList(0,output);}
}
public sealed class PolylineToBeamsComponent:SafeComponent
{
    public PolylineToBeamsComponent():base("Midas Polyline to Beams","Polyline Beams","Create one beam per straight polyline segment.","03-Elements"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddCurveParameter("Polyline","P","Polyline only; no implicit approximation.",GH_ParamAccess.item);p.AddIntegerParameter("First ID","ID","First element ID.",GH_ParamAccess.item,1);p.AddGenericParameter("Material","M","Material definition.",GH_ParamAccess.item);p.AddGenericParameter("Section","S","Section definition.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Beams","B","Beam fragments.",GH_ParamAccess.list);
    protected override void Solve(IGH_DataAccess da){if(!Item<Curve>(da,0).TryGetPolyline(out var polyline))throw new ArgumentException("A polyline is required.");var mat=Native<Fragment>(da,2);var sect=Native<Fragment>(da,3);int first=Item<int>(da,1);da.SetDataList(0,polyline.GetSegments().Select((l,i)=>new FragmentGoo(Fragment.Combine([mat,sect,new([new(first+i,ElementKind.Beam,[Preview.Position(l.From),Preview.Position(l.To)],Definitions.PropertyId(mat,"MATL"),Definitions.PropertyId(sect,"SECT"))])]))));}
}
