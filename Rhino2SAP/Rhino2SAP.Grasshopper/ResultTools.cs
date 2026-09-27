using System.Text.Json;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public sealed class ResultCasesComponent:SafeComponent
{
    public ResultCasesComponent():base("SAP Model Result Cases","Cases","List stored case/combination and step labels.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Cases and steps","C","case | step type | step number.",GH_ParamAccess.list);p.AddTextParameter("Quantities","Q","Captured methods.",GH_ParamAccess.list);p.AddTextParameter("Issues","I","Failed quantities.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);var r=m.Results??throw new ArgumentException("No results.");r.RequireValidFor(m);da.SetDataList(0,r.Tables.Where(t=>t.Columns.ContainsKey("LoadCase")).SelectMany(t=>Enumerable.Range(0,t.RowCount).Select(i=>ResultRows.Text(t,"LoadCase",i)+" | "+ResultRows.Text(t,"StepType",i)+" | "+ResultRows.Number(t,"StepNum",i))).Distinct());da.SetDataList(1,r.Tables.Select(t=>t.Method));da.SetDataList(2,r.Issues.Select(i=>i.Method+": "+i.Message));}
}
public static class ResultRows
{
    public static string Text(ResultTable t,string column,int row)=>t.Columns.TryGetValue(column,out var a)&&a.ValueKind==JsonValueKind.Array&&row<a.GetArrayLength()?a[row].GetString()??"":"";
    public static double Number(ResultTable t,string column,int row)=>t.Columns.TryGetValue(column,out var a)&&a.ValueKind==JsonValueKind.Array&&row<a.GetArrayLength()?a[row].GetDouble():0;
    public static ResultTable Table(SapModel model,string key){var r=model.Results??throw new ArgumentException("No results in Model.");r.RequireValidFor(model);return r.Tables.SingleOrDefault(t=>t.Method==key)??throw new ArgumentException("Missing quantity: "+key);}
}
public sealed class ResultEnvelopeComponent:SafeComponent
{
    public ResultEnvelopeComponent():base("SAP Model Result Extrema","Extrema","Find signed min/max of one native result column and retain governing case, object and row. Extrema of different columns need not be simultaneous.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);p.AddTextParameter("Quantity","Q","Example Results.FrameForce.",GH_ParamAccess.item,"Results.FrameForce");p.AddTextParameter("Column","C","Example P, V2, V3, T, M2, M3.",GH_ParamAccess.item,"M3");p.AddTextParameter("Object","O","Optional object name.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddNumberParameter("Minimum","Min","Signed minimum.",GH_ParamAccess.item);p.AddNumberParameter("Maximum","Max","Signed maximum.",GH_ParamAccess.item);p.AddTextParameter("Governing rows","G","Min and max: case, object, station/step and row index.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var t=ResultRows.Table(Native<SapModel>(da,0),Item<string>(da,1));string c=Item<string>(da,2),filter=Item<string>(da,3);if(!t.Columns.TryGetValue(c,out var values)||values.ValueKind!=JsonValueKind.Array)throw new ArgumentException("Choose a numeric result column.");var rows=Enumerable.Range(0,values.GetArrayLength()).Where(i=>filter.Length==0||ResultRows.Text(t,"Obj",i)==filter).ToArray();if(rows.Length==0)throw new ArgumentException("No matching results.");int min=rows.MinBy(i=>values[i].GetDouble()),max=rows.MaxBy(i=>values[i].GetDouble());da.SetData(0,values[min].GetDouble());da.SetData(1,values[max].GetDouble());da.SetDataList(2,new[]{min,max}.Select(i=>$"{ResultRows.Text(t,"LoadCase",i)} | {ResultRows.Text(t,"Obj",i)} | station {ResultRows.Number(t,"ObjSta",i)} | {ResultRows.Text(t,"StepType",i)} {ResultRows.Number(t,"StepNum",i)} | row {i}"));}
}
public sealed class DeformedGeometryComponent:SafeComponent
{
    public DeformedGeometryComponent():base("SAP Model Deformed Geometry","Deformed","Move analysis geometry using joint displacements transformed from native local axes to global axes. Frames interpolate endpoints; no internal beam deflection interpolation.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Solved Model with Results.JointDispl.",GH_ParamAccess.item);p.AddTextParameter("Case","C","One exact case/combination.",GH_ParamAccess.item);p.AddTextParameter("Step type","T","Empty for static; use Step/Mode/etc. as reported.",GH_ParamAccess.item,"");p.AddNumberParameter("Step","S","Exact step number (0 for ordinary static).",GH_ParamAccess.item,0);p.AddNumberParameter("Scale","F","Visual displacement magnification.",GH_ParamAccess.item,1);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Geometry","G","Deformed geometry-only fragment.",GH_ParamAccess.item);p.AddVectorParameter("Translations","U","Global translations before visual scaling.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da)
    {
        var m=Native<SapModel>(da,0);string c=Item<string>(da,1),type=Item<string>(da,2);double step=Item<double>(da,3),scale=Item<double>(da,4);var d=ResultRows.Table(m,"Results.JointDispl");var coords=ResultRows.Table(m,"Rhino2SAP.NodeCoordinates");var axes=ResultRows.Table(m,"Rhino2SAP.NodeAxes");
        if(type is "Max" or "Min")throw new ArgumentException("Envelope extrema do not represent one simultaneous deformed shape. Choose a physical case/step.");
        var translations=new Dictionary<string,Vector3d>();
        foreach(int i in Enumerable.Range(0,d.RowCount).Where(i=>ResultRows.Text(d,"LoadCase",i)==c&&ResultRows.Text(d,"StepType",i)==type&&Math.Abs(ResultRows.Number(d,"StepNum",i)-step)<1e-9))
        {
            string name=ResultRows.Text(d,"Obj",i);int a=Enumerable.Range(0,axes.RowCount).First(j=>ResultRows.Text(axes,"Name",j)==name);var t=axes.Columns["Matrix"][a];double x=ResultRows.Number(d,"U1",i),y=ResultRows.Number(d,"U2",i),z=ResultRows.Number(d,"U3",i);
            var u=new Vector3d(t[0].GetDouble()*x+t[1].GetDouble()*y+t[2].GetDouble()*z,t[3].GetDouble()*x+t[4].GetDouble()*y+t[5].GetDouble()*z,t[6].GetDouble()*x+t[7].GetDouble()*y+t[8].GetDouble()*z);
            if(!translations.TryAdd(name,u))throw new ArgumentException("Multiple displacement states match. Select exactly one case/step.");
        }
        if(translations.Count==0)throw new ArgumentException("No displacement rows match this case/step.");
        var lookup=Enumerable.Range(0,coords.RowCount).Select(i=>(Point:new Position(ResultRows.Number(coords,"X",i),ResultRows.Number(coords,"Y",i),ResultRows.Number(coords,"Z",i)),Name:ResultRows.Text(coords,"Name",i))).ToArray();
        Position Move(Position p){var node=lookup.FirstOrDefault(x=>x.Point.DistanceTo(p)<=m.Tolerance);if(node.Name==null||!translations.TryGetValue(node.Name,out var u))throw new ArgumentException("Missing displacement at a model vertex.");return new(p.X+scale*u.X,p.Y+scale*u.Y,p.Z+scale*u.Z);}
        Output(da,0,new Fragment(elements:m.Definition.Elements.Select(e=>new Element(e.Name,e.Kind,e.Points.Select(Move),e.Property))));da.SetDataList(1,translations.Values);
    }
}
public sealed class FrameDiagramComponent:SafeComponent
{
    public FrameDiagramComponent():base("SAP Model Frame Force Diagram","Diagram","Draw a native frame-force column at object stations. Direction is an explicit global plotting direction; output values keep SAP signs.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);p.AddTextParameter("Frame","F","Frame name.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Case/combination.",GH_ParamAccess.item);p.AddTextParameter("Column","V","P, V2, V3, T, M2 or M3.",GH_ParamAccess.item,"M3");p.AddTextParameter("Step type","T","Exact native step type.",GH_ParamAccess.item,"");p.AddNumberParameter("Step","S","Exact native step number.",GH_ParamAccess.item,0);p.AddVectorParameter("Direction","D","Global plotting direction (projected perpendicular to frame axis).",GH_ParamAccess.item,Vector3d.ZAxis);p.AddNumberParameter("Scale","A","Length/result-unit display scale.",GH_ParamAccess.item,1);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddCurveParameter("Diagram","D","Force diagram polyline.",GH_ParamAccess.item);p.AddLineParameter("Ordinates","O","Diagram ordinates.",GH_ParamAccess.list);p.AddNumberParameter("Stations","S","Distances from object I-end.",GH_ParamAccess.list);p.AddNumberParameter("Values","V","Native values.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);string frame=Item<string>(da,1),c=Item<string>(da,2),col=Item<string>(da,3),type=Item<string>(da,4);double step=Item<double>(da,5),scale=Item<double>(da,7);if(!new[]{"P","V2","V3","T","M2","M3"}.Contains(col))throw new ArgumentException("Invalid frame force column.");var e=m.Definition.Elements.Single(e=>e.Kind==ElementKind.Frame&&e.Name==frame);var line=new Line(Preview.Point(e.Points[0]),Preview.Point(e.Points[1]));var axis=line.Direction;axis.Unitize();var direction=Item<Vector3d>(da,6);direction-=axis*(direction*axis);if(!direction.Unitize())throw new ArgumentException("Plot direction is parallel to frame.");var t=ResultRows.Table(m,"Results.FrameForce");var rows=Enumerable.Range(0,t.RowCount).Where(i=>ResultRows.Text(t,"Obj",i)==frame&&ResultRows.Text(t,"LoadCase",i)==c&&ResultRows.Text(t,"StepType",i)==type&&Math.Abs(ResultRows.Number(t,"StepNum",i)-step)<1e-9).OrderBy(i=>ResultRows.Number(t,"ObjSta",i)).ToArray();if(rows.Length<2)throw new ArgumentException("Not enough matching result stations.");var stations=rows.Select(i=>ResultRows.Number(t,"ObjSta",i)).ToArray();var values=rows.Select(i=>ResultRows.Number(t,col,i)).ToArray();var ordinates=stations.Select((s,i)=>new Line(line.From+axis*s,line.From+axis*s+direction*(scale*values[i]))).ToArray();da.SetData(0,new Polyline(ordinates.Select(l=>l.To)).ToNurbsCurve());da.SetDataList(1,ordinates);da.SetDataList(2,stations);da.SetDataList(3,values);}
}
public sealed class VerificationDataComponent:SafeComponent
{
    public VerificationDataComponent():base("SAP Model Verification Data","Verification Data","Expose verified native result tables and complete model definitions for downstream engineering checks. Does not perform code checks.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Elements","E","Element records.",GH_ParamAccess.list);p.AddTextParameter("Definitions JSON","D","API method/argument definitions, in Model units.",GH_ParamAccess.item);p.AddGenericParameter("Result tables","R","Immutable native tables.",GH_ParamAccess.list);p.AddIntegerParameter("Units","U","SAP eUnits.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Missing quantities must be evaluated by the checker.",GH_ParamAccess.list);p.AddBooleanParameter("Complete","C","All requested queries succeeded; not an engineering approval.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var m=Native<SapModel>(da,0);var r=m.Results??throw new ArgumentException("No results.");r.RequireValidFor(m);da.SetDataList(0,m.Definition.Elements);da.SetData(1,JsonSerializer.Serialize(m.Definition.Operations,ApiSchema.JsonOptions));da.SetDataList(2,r.Tables);da.SetData(3,m.Units);da.SetDataList(4,r.Issues.Select(i=>i.Method+": "+i.Message));da.SetData(5,r.IsComplete);}
}
