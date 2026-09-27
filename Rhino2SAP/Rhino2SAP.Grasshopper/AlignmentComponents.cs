using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public static class ElementAlignment
{
    public static Fragment Align(Fragment source,Vector3d direction,ElementKind? kind=null,Point3d? target=null)
    {
        var operations=source.Operations.ToList();
        foreach(var e in source.Elements.Where(e=>(kind==null||e.Kind==kind)&&e.Kind is ElementKind.Frame or ElementKind.Area))
        {
            var reference=target.HasValue?target.Value-new Point3d(e.Points.Average(p=>p.X),e.Points.Average(p=>p.Y),e.Points.Average(p=>p.Z)):direction;
            var normal=e.Kind==ElementKind.Frame?Preview.Point(e.Points[1])-Preview.Point(e.Points[0]):Vector3d.CrossProduct(Preview.Point(e.Points[1])-Preview.Point(e.Points[0]),Preview.Point(e.Points[2])-Preview.Point(e.Points[0]));
            if(!normal.Unitize())throw new ArgumentException("Degenerate element: "+e.Name);
            reference-=normal*(reference*normal);if(!reference.Unitize())throw new ArgumentException("Alignment direction is parallel to the primary axis/area normal: "+e.Name);
            string path=e.Kind+"Obj";
            operations.RemoveAll(o=>o.Key.StartsWith(path+".SetLocalAxes")&&o.Arguments.TryGetValue("Name",out var n)&&n.GetString()==e.Name);
            operations.Add(Operation.Create(path+".SetLocalAxes",("Name",e.Name),("Ang",0d)));
            operations.Add(Operation.Create(path+".SetLocalAxesAdvanced",("Name",e.Name),("Active",true),("Plane2",e.Kind==ElementKind.Frame?12:31),("PlVectOpt",3),("PlCSys","Global"),("PlDir",new[]{1,2}),("PlPt",new[]{"None","None"}),("PlVect",new[]{reference.X,reference.Y,reference.Z})));
        }
        return new(operations,source.Elements.Select(e=>e with { Results=null }));
    }
}
public abstract class AlignComponent:SafeComponent
{
    protected abstract ElementKind? Kind{get;}
    protected virtual int Mode=>0;
    protected AlignComponent(string name):base(name,name,"Set native advanced axes with a projected global reference vector. Frame local 2 / area local 1 follows the projection; connectivity and area normal are preserved.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definition","M","Elements or Model.",GH_ParamAccess.item);if(Mode==1)p.AddPointParameter("Target","P","Reference point relative to each element centroid.",GH_ParamAccess.item);else if(Mode==2)p.AddPlaneParameter("Plane","P","Plane X direction defines area local 1.",GH_ParamAccess.item);else p.AddVectorParameter("Direction","D","Desired global direction.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Definition","M","Copy with updated axes. Model results are cleared.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var input=Unwrap(Item<object>(da,0));var f=FragmentOf(input);var aligned=ElementAlignment.Align(f,Mode==0?Item<Vector3d>(da,1):Mode==2?Item<Plane>(da,1).XAxis:Vector3d.Zero,Kind,Mode==1?Item<Point3d>(da,1):null);Output(da,0,input is SapModel m?new SapModel(aligned,m.Units,m.Tolerance):aligned);}
}
public sealed class AlignFrameComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Frame;public AlignFrameComponent():base("Align SAP Frame Local Axis"){} }
public sealed class AlignFrameToPointComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Frame;protected override int Mode=>1;public AlignFrameToPointComponent():base("Align SAP Frame Axis To Point"){} }
public sealed class AlignAreaComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Area;public AlignAreaComponent():base("Align SAP Area Local Axis"){} }
public sealed class AlignAreaToPlaneComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Area;protected override int Mode=>2;public AlignAreaToPlaneComponent():base("Align SAP Area Axis To Plane"){} }
public sealed class AlignModelComponent:AlignComponent{protected override ElementKind? Kind=>null;public AlignModelComponent():base("Align SAP Model Local Axes"){} }
public sealed class ClearResultsComponent:SafeComponent
{
    public ClearResultsComponent():base("Clear SAP Model Results","Clear Results","Return an unchanged model definition without results.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Model with results removed.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Native<SapModel>(da,0).WithoutResults());
}
public sealed class DefinitionInfoComponent:SafeComponent
{
    public DefinitionInfoComponent():base("SAP Definition Info","Definition Info","Inspect collected materials, sections, properties, loads and assignments.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Definition","D","SAP definition or Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Methods","M","SDK method keys.",GH_ParamAccess.list);p.AddTextParameter("Arguments","A","Named argument dictionaries (JSON).",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));da.SetDataList(0,f.Operations.Select(o=>o.Key));da.SetDataList(1,f.Operations.Select(o=>System.Text.Json.JsonSerializer.Serialize(o.Arguments)));}
}
