using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Midas.Core;
using Data=Rhino2Midas.Core.Native;

namespace Rhino2Midas.Grasshopper;

public sealed class FilterElementsComponent:SafeComponent
{
    public FilterElementsComponent():base("Filter Midas Elements","Filter","Filter elements by kind, native IDs and property ID.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Model or fragment.",GH_ParamAccess.item);p.AddTextParameter("Kind","K","Node Beam Plate Solid Truss Tension Compression ElasticLink, or blank.",GH_ParamAccess.item,"");p.AddIntegerParameter("IDs","ID","Optional selected native IDs.",GH_ParamAccess.list);p[2].Optional=true;p.AddIntegerParameter("Property","P","Zero = all properties.",GH_ParamAccess.item,0);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Elements","E","Matching individual elements with definitions/results.",GH_ParamAccess.list);p.AddIntegerParameter("IDs","ID","Matching IDs.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));string kind=Item<string>(da,1);int property=Item<int>(da,3);var ids=new List<int>();da.GetDataList(2,ids);var found=f.Elements.Where(e=>(kind.Length==0||e.Kind.ToString().Equals(kind,StringComparison.OrdinalIgnoreCase))&&(ids.Count==0||ids.Contains(e.Id))&&(property==0||e.Property==property)).ToArray();da.SetDataList(0,found.Select(e=>new FragmentGoo(f.ForElement(e))));da.SetDataList(1,found.Select(e=>e.Id));}
}
public class ElementInfoComponent:SafeComponent
{
    public ElementInfoComponent():this("Midas Element Info"){}protected ElementInfoComponent(string name):base(name,"Element Info","Read geometry, IDs, material, property, angle and embedded result tables.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Element","E","A single non-node element, or one node.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddIntegerParameter("ID","ID","Element ID.",GH_ParamAccess.item);p.AddGeometryParameter("Geometry","G","Analytical geometry.",GH_ParamAccess.item);p.AddIntegerParameter("Material","M","Material ID.",GH_ParamAccess.item);p.AddIntegerParameter("Property","P","Property ID.",GH_ParamAccess.item);p.AddGenericParameter("Results","R","Embedded native tables.",GH_ParamAccess.list);p.AddNumberParameter("Angle","A","Beta angle in degrees.",GH_ParamAccess.item);p.AddIntegerParameter("Nodes","N","Native connectivity.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));var e=ElementTools.Single(f);da.SetData(0,e.Id);da.SetData(1,PhysicalGeometry.Analytical(e));da.SetData(2,e.Material);da.SetData(3,e.Property);da.SetDataList(4,e.Results);da.SetData(5,e.Angle);da.SetDataList(6,e.NodeIds);}
}
public sealed class DecomposeBeamComponent:ElementInfoComponent{public DecomposeBeamComponent():base("Decompose Midas Beam"){} }
public sealed class DecomposePlateComponent:ElementInfoComponent{public DecomposePlateComponent():base("Decompose Midas Plate"){} }
public sealed class DefinitionInfoComponent:SafeComponent
{
    public DefinitionInfoComponent():base("Midas Definition Info","Definition Info","Inspect material, section, case, load and native record data.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Definition","D","Fragment or Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Endpoints","E","Native table names.",GH_ParamAccess.list);p.AddIntegerParameter("IDs","ID","Native IDs.",GH_ParamAccess.list);p.AddTextParameter("JSON","J","Record contents in Model units.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));da.SetDataList(0,f.Entries.Select(e=>e.Endpoint));da.SetDataList(1,f.Entries.Select(e=>e.Id));da.SetDataList(2,f.Entries.Select(e=>e.Data.GetRawText()));}
}
public static class ElementTools
{
    public static Element Single(Fragment f){var members=f.Elements.Where(e=>e.Kind!=ElementKind.Node).ToArray();return members.Length==1?members[0]:f.Elements.Count==1?f.Elements[0]:throw new ArgumentException("Connect one element.");}
    public static double Angle(Vector3d from,Vector3d to,Vector3d normal)=>Math.Atan2(Vector3d.CrossProduct(from,to)*normal,from*to)*180/Math.PI;
    public static (Vector3d X,Vector3d Y,Vector3d Z) PlateAxes(Element e)
    {var x=Preview.Point(e.Points[1])-Preview.Point(e.Points[0]);var z=Vector3d.CrossProduct(x,Preview.Point(e.Points[2])-Preview.Point(e.Points[0]));if(!x.Unitize()||!z.Unitize())throw new ArgumentException("Degenerate plate.");x.Rotate(e.Angle*Math.PI/180,z);return(x,Vector3d.CrossProduct(z,x),z);}
    public static Fragment Align(Fragment f,Vector3d direction,ElementKind? kind,Point3d? target=null)
    {
        return new(f.Elements.Select(e=>
        {
            if((kind.HasValue&&e.Kind!=kind)||(e.Kind is not (ElementKind.Beam or ElementKind.Plate)))return e with{Results=[]};
            var reference=target.HasValue?target.Value-new Point3d(e.Points.Average(p=>p.X),e.Points.Average(p=>p.Y),e.Points.Average(p=>p.Z)):direction;
            var axes=e.Kind==ElementKind.Plate?PlateAxes(e with{Angle=0}):PhysicalGeometry.Axes(e with{Angle=0});var normal=e.Kind==ElementKind.Plate?axes.Z:axes.X;
            reference-=normal*(normal*reference);if(!reference.Unitize())throw new ArgumentException("Reference direction is parallel to element axis/plate normal.");
            return e with{Angle=Angle(e.Kind==ElementKind.Plate?axes.X:axes.Y,reference,normal),Results=[]};
        }).ToArray(),f.Entries);
    }
}
public abstract class AlignComponent:SafeComponent
{
    protected abstract ElementKind? Kind{get;}protected virtual int Mode=>0;
    protected AlignComponent(string name):base(name,"Align","Set beam local y or plate local x towards a projected vector. Copies inputs and clears results.","04-Attributes"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Definition","D","Model/fragment.",GH_ParamAccess.item);if(Mode==1)p.AddPointParameter("Target","P","Target relative to element centroid.",GH_ParamAccess.item);else if(Mode==2)p.AddPlaneParameter("Plane","P","Plane X direction.",GH_ParamAccess.item);else p.AddVectorParameter("Direction","V","Desired global direction.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Definition","D","Copy with updated axes.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var input=Unwrap(Item<object>(da,0));var f=ElementTools.Align(FragmentOf(input),Mode==0?Item<Vector3d>(da,1):Mode==2?Item<Plane>(da,1).XAxis:Vector3d.Zero,Kind,Mode==1?Item<Point3d>(da,1):null);Output(da,0,input is MidasModel m?new MidasModel(f,m.Units,m.Tolerance):f);}
}
public sealed class AlignBeamComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Beam;public AlignBeamComponent():base("Align Midas Beam Axis"){} }
public sealed class AlignBeamToPointComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Beam;protected override int Mode=>1;public AlignBeamToPointComponent():base("Align Midas Beam to Point"){} }
public sealed class AlignPlateComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Plate;public AlignPlateComponent():base("Align Midas Plate Axis"){} }
public sealed class AlignPlateToPlaneComponent:AlignComponent{protected override ElementKind? Kind=>ElementKind.Plate;protected override int Mode=>2;public AlignPlateToPlaneComponent():base("Align Midas Plate to Plane"){} }
public sealed class AlignModelComponent:AlignComponent{protected override ElementKind? Kind=>null;public AlignModelComponent():base("Align Midas Model Axes"){} }
public sealed class ElementAxesComponent:SafeComponent
{
    public ElementAxesComponent():base("Midas Element Axes","Axes","Local beam/plate coordinate axes, including beta angle.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Element","E","One beam or plate.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddVectorParameter("X","X","Local x.",GH_ParamAccess.item);p.AddVectorParameter("Y","Y","Local y.",GH_ParamAccess.item);p.AddVectorParameter("Z","Z","Local z.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var e=ElementTools.Single(FragmentOf(Item<object>(da,0)));var axes=e.Kind==ElementKind.Plate?ElementTools.PlateAxes(e):PhysicalGeometry.Axes(e);da.SetData(0,axes.X);da.SetData(1,axes.Y);da.SetData(2,axes.Z);}
}
public sealed class AssignPropertyComponent:SafeComponent
{
    public AssignPropertyComponent():base("Assign Midas Element Property","Assign Property","Replace material and section/thickness on copies of selected elements.","02-Properties"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Elements","E","Element fragment.",GH_ParamAccess.item);p.AddGenericParameter("Material","M","New material definition.",GH_ParamAccess.item);p.AddGenericParameter("Property","P","New section or thickness definition.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Elements","E","Modified elements.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da){var f=Native<Fragment>(da,0);var mat=Native<Fragment>(da,1);var prop=Native<Fragment>(da,2);int material=Definitions.PropertyId(mat,"MATL");var entries=f.Entries.Concat(mat.Entries).Concat(prop.Entries).DistinctBy(e=>e.Identity).ToArray();Output(da,0,new Fragment(f.Elements.Select(e=>e.Kind is ElementKind.Node or ElementKind.ElasticLink?e with{Results=[]}:e with{Material=material,Property=Definitions.PropertyId(prop,e.Kind==ElementKind.Plate?"THIK":"SECT"),Results=[]}).ToArray(),entries));}
}
public sealed class ModelGeometryComponent:SafeComponent
{
    public ModelGeometryComponent():base("Midas Model Geometry","Geometry","Extract analytical Rhino geometry and native IDs.","08-Tools"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model/fragment.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGeometryParameter("Geometry","G","Analytical geometry.",GH_ParamAccess.list);p.AddIntegerParameter("IDs","ID","Native IDs.",GH_ParamAccess.list);p.AddTextParameter("Kinds","K","Element types.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var f=FragmentOf(Item<object>(da,0));da.SetDataList(0,f.Elements.Select(PhysicalGeometry.Analytical));da.SetDataList(1,f.Elements.Select(e=>e.Id));da.SetDataList(2,f.Elements.Select(e=>e.Kind.ToString()));}
}
