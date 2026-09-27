using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Midas.Core;

namespace Rhino2Midas.Grasshopper;

public static class ModelTransform
{
    public static Fragment Rigid(Fragment source,Transform transform)
    {
        if(source.Entries.Any(e=>e.Endpoint is "NODE" or "ELEM" or "ELNK"))throw new ArgumentException("Decode native geometry with Read Active Midas Model before transforming it.");
        var elements=source.Elements.Select(e=>
        {
            var points=e.Points.Select(p=>{var q=Preview.Point(p);q.Transform(transform);return new Position(q.X,q.Y,q.Z);}).ToArray();var copy=e with{Points=Array.AsReadOnly(points),Results=[]};
            if(e.Kind is ElementKind.Node or ElementKind.Solid||e.Points.Count<2||e.Points[0].DistanceTo(e.Points[1])<1e-12)return copy;
            var before=e.Kind==ElementKind.Plate?ElementTools.PlateAxes(e):PhysicalGeometry.Axes(e);
            var direction=e.Kind==ElementKind.Plate?before.X:before.Y;direction.Transform(transform);
            var after=e.Kind==ElementKind.Plate?ElementTools.PlateAxes(copy with{Angle=0}):PhysicalGeometry.Axes(copy with{Angle=0});
            return copy with{Angle=ElementTools.Angle(e.Kind==ElementKind.Plate?after.X:after.Y,direction,e.Kind==ElementKind.Plate?after.Z:after.X)};
        }).ToArray();
        return new(elements,source.Entries);
    }
}
public sealed class MoveModelComponent:SafeComponent
{
    public MoveModelComponent():base("Move Midas Model","Move","Rigid translation of geometry. Global loads and boundary directions stay global. Results are cleared.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definition","D","Model or fragment.",GH_ParamAccess.item);p.AddVectorParameter("Translation","T","Translation in Model length units; default zero.",GH_ParamAccess.item,Vector3d.Zero);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Definition","D","Translated copy.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var raw=Unwrap(Item<object>(da,0));var v=Item<Vector3d>(da,1);if(!v.IsValid)throw new ArgumentException("Invalid translation.");var f=ModelTransform.Rigid(FragmentOf(raw),Transform.Translation(v));Output(da,0,raw is MidasModel m?new MidasModel(f,m.Units,m.Tolerance):f);}
}
public sealed class RotateModelComponent:SafeComponent
{
    public RotateModelComponent():base("Rotate Midas Model","Rotate","Rigid geometry rotation preserving section orientation. Global loads, restraints and global offsets keep their global directions. Results are cleared.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definition","D","Model or fragment.",GH_ParamAccess.item);p.AddNumberParameter("Angle","A","Degrees, right-hand rule; default zero.",GH_ParamAccess.item,0);p.AddVectorParameter("Axis","V","Rotation axis.",GH_ParamAccess.item,Vector3d.ZAxis);p.AddPointParameter("Center","C","Rotation center.",GH_ParamAccess.item,Point3d.Origin);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Definition","D","Rotated copy.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var raw=Unwrap(Item<object>(da,0));var axis=Item<Vector3d>(da,2);double a=Item<double>(da,1);var c=Item<Point3d>(da,3);if(!double.IsFinite(a)||!axis.Unitize()||!c.IsValid)throw new ArgumentException("Invalid rigid rotation.");var f=ModelTransform.Rigid(FragmentOf(raw),Transform.Rotation(a*Math.PI/180,axis,c));Output(da,0,raw is MidasModel m?new MidasModel(f,m.Units,m.Tolerance):f);}
}
