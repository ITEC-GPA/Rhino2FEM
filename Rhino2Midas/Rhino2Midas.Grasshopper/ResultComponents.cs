using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2Midas.Core;
using Rhino2Midas.Api;
using Data=Rhino2Midas.Core.Native;

namespace Rhino2Midas.Grasshopper;

public static class ResultTools
{
    public static IReadOnlyList<ResultTable> Tables(object? input)=>SafeComponent.Unwrap(input) switch
    {MidasModel m=>m.Results?.Tables??throw new ArgumentException("Model has no stored results."),Fragment f=>ElementTools.Single(f).Results,Element e=>e.Results,ResultTable t=>[t],_=>throw new ArgumentException("Use a solved Model, element or result table.")};
    public static ResultTable Table(object? input,string type,string load="")
    {
        var found=Tables(input).Where(t=>t.Type.Equals(type,StringComparison.OrdinalIgnoreCase)).ToArray();if(found.Length==0)throw new ArgumentException("Missing result quantity "+type);
        if(found.Any(t=>!t.Columns.SequenceEqual(found[0].Columns)))throw new ArgumentException("Incompatible column layouts for "+type+". Select one native table first.");
        var t=new ResultTable(found[0].Name,type,found[0].Columns,found.SelectMany(t=>t.Rows).ToArray());return load.Length>0?t.Filter("Load",load):t;
    }
    public static GH_Structure<GH_String> Tree(ResultTable table){var tree=new GH_Structure<GH_String>();for(int i=0;i<table.Rows.Count;i++)tree.AppendRange(table.Rows[i].Select(s=>new GH_String(s)),new GH_Path(i));return tree;}
    public static string State(ResultTable t,IReadOnlyList<string> row)=>string.Join(" | ",new[]{"Load","Stage","Step","Part"}.Where(c=>t.Columns.Contains(c)).Select(c=>row[t.Column(c)]));
}
public sealed class QueryResultsComponent:SafeComponent
{
    public QueryResultsComponent():base("Query Midas Element Results","Query Results","Query stored native tables from a Model or individual element. No solver call.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Source","S","Solved Model/element/table.",GH_ParamAccess.item);p.AddTextParameter("Quantity","Q","Native TABLE_TYPE, e.g. BEAMFORCE.",GH_ParamAccess.item,"BEAMFORCE");p.AddTextParameter("Case","C","Optional exact native Load value.",GH_ParamAccess.item,"");p.AddTextParameter("Column","V","Numeric column to extract; blank = return table only.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Table","T","Filtered table.",GH_ParamAccess.item);p.AddTextParameter("Columns","C","Native headers.",GH_ParamAccess.list);p.AddTextParameter("Rows","R","One tree branch per row.",GH_ParamAccess.tree);p.AddNumberParameter("Values","V","Selected numeric column.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var t=ResultTools.Table(Item<object>(da,0),Item<string>(da,1),Item<string>(da,2));da.SetData(0,t);da.SetDataList(1,t.Columns);da.SetDataTree(2,ResultTools.Tree(t));string col=Item<string>(da,3);if(col.Length>0)da.SetDataList(3,t.Numbers(col));}
}
public abstract class ActionsComponent:SafeComponent
{
    protected abstract string Quantity{get;}protected abstract string[] Columns{get;}
    protected ActionsComponent(string name):base(name,"Actions","Read native quantities from Model or one element with embedded results. Native signs and row ordering are retained.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Source","S","Solved Model or element.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Optional exact case name as returned in Load.",GH_ParamAccess.item,"");}
    protected override void RegisterOutputParams(GH_OutputParamManager p){foreach(var c in Columns)p.AddNumberParameter(c,c,"Native "+c+" in Model units.",GH_ParamAccess.list);p.AddTextParameter("States","S","Governing Load/Stage/Step/Part for every row.",GH_ParamAccess.list);p.AddGenericParameter("Table","T","Full table with native identifiers.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var t=ResultTools.Table(Item<object>(da,0),Quantity,Item<string>(da,1));for(int i=0;i<Columns.Length;i++)da.SetDataList(i,t.Numbers(Columns[i]));da.SetDataList(Columns.Length,t.Rows.Select(r=>ResultTools.State(t,r)));da.SetData(Columns.Length+1,t);}
}
public sealed class BeamActionsComponent:ActionsComponent{protected override string Quantity=>"BEAMFORCE";protected override string[] Columns=>["Axial","Shear-y","Shear-z","Torsion","Moment-y","Moment-z"];public BeamActionsComponent():base("Midas Beam Actions"){} }
public sealed class PlateActionsComponent:ActionsComponent{protected override string Quantity=>"PLATEFORCEUL";protected override string[] Columns=>["Fxx","Fyy","Fxy","Mxx","Myy","Mxy","Vxx","Vyy"];public PlateActionsComponent():base("Midas Plate Actions"){} }
public sealed class NodeDisplacementsComponent:ActionsComponent{protected override string Quantity=>"DISPLACEMENTG";protected override string[] Columns=>["DX","DY","DZ","RX","RY","RZ"];public NodeDisplacementsComponent():base("Midas Node Displacements"){} }
public sealed class NodeReactionsComponent:ActionsComponent{protected override string Quantity=>"REACTIONG";protected override string[] Columns=>["FX","FY","FZ","MX","MY","MZ"];public NodeReactionsComponent():base("Midas Node Reactions"){} }
public sealed class TrussActionsComponent:ActionsComponent{protected override string Quantity=>"TRUSSFORCE";protected override string[] Columns=>["Force-I","Force-J"];public TrussActionsComponent():base("Midas Truss Actions"){} }
public sealed class SolidStressComponent:ActionsComponent{protected override string Quantity=>"SOLIDSL";protected override string[] Columns=>["Sig-xx","Sig-yy","Sig-zz","Sig-xy","Sig-yz","Sig-xz"];public SolidStressComponent():base("Midas Solid Stress"){} }
public sealed class ElasticLinkActionsComponent:ActionsComponent{protected override string Quantity=>"ELASTICLINK";protected override string[] Columns=>["Axial","Shear-y","Shear-z","Torsion","Moment-y","Moment-z"];public ElasticLinkActionsComponent():base("Midas Elastic Link Actions"){} }
public sealed class ResultCasesComponent:SafeComponent
{
    public ResultCasesComponent():base("Midas Model Result Cases","Result Cases","List captured native quantities and case/stage/step states.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("States","S","Load | Stage | Step | Part.",GH_ParamAccess.list);p.AddTextParameter("Quantities","Q","Stored TABLE_TYPEs.",GH_ParamAccess.list);p.AddTextParameter("Issues","I","Failed/missing queries.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var m=Native<MidasModel>(da,0);var tables=ResultTools.Tables(m);da.SetDataList(0,tables.SelectMany(t=>t.Rows.Select(r=>ResultTools.State(t,r))).Distinct());da.SetDataList(1,tables.Select(t=>t.Type).Distinct());da.SetDataList(2,m.Results!.Issues);}
}
public sealed class ResultExtremaComponent:SafeComponent
{
    public ResultExtremaComponent():base("Midas Result Extrema","Extrema","Signed min/max of one numeric column with governing native rows. Different component extremes need not be simultaneous.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Table","T","Filtered native result table.",GH_ParamAccess.item);p.AddTextParameter("Column","C","Numeric header.",GH_ParamAccess.item,"Moment-y");}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddNumberParameter("Minimum","Min","Signed minimum.",GH_ParamAccess.item);p.AddNumberParameter("Maximum","Max","Signed maximum.",GH_ParamAccess.item);p.AddTextParameter("Governing rows","R","Min and max rows with all native identifiers.",GH_ParamAccess.tree);}
    protected override void Solve(IGH_DataAccess da){var t=Native<ResultTable>(da,0);var values=t.Numbers(Item<string>(da,1));if(values.Count==0||values.Any(v=>!double.IsFinite(v)))throw new ArgumentException("No finite result rows.");int min=Enumerable.Range(0,values.Count).MinBy(i=>values[i]),max=Enumerable.Range(0,values.Count).MaxBy(i=>values[i]);da.SetData(0,values[min]);da.SetData(1,values[max]);da.SetDataTree(2,ResultTools.Tree(new(t.Name,t.Type,t.Columns,[t.Rows[min],t.Rows[max]])));}
}
public sealed class FilterResultTableComponent:SafeComponent
{
    public FilterResultTableComponent():base("Filter Midas Result Table","Filter Results","Filter a table by an exact native column value (node, element, stage, step, part or case). Chain filters.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Table","T","Native result table.",GH_ParamAccess.item);p.AddTextParameter("Column","C","Exact header.",GH_ParamAccess.item,"Elem");p.AddTextParameter("Value","V","Value to match.",GH_ParamAccess.item);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Table","T","Filtered table.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>da.SetData(0,Native<ResultTable>(da,0).Filter(Item<string>(da,1),Item<string>(da,2)));
}
public sealed class DeformedGeometryComponent:SafeComponent
{
    public DeformedGeometryComponent():base("Midas Deformed Geometry","Deformed","Translate vertices using global nodal results. Beam interiors interpolate the end displacements.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Exact native Load label.",GH_ParamAccess.item);p.AddTextParameter("Stage","St","Exact stage; empty if table has no stage.",GH_ParamAccess.item,"");p.AddTextParameter("Step","S","Exact step; empty if table has no step.",GH_ParamAccess.item,"");p.AddNumberParameter("Scale","F","Visual displacement scale.",GH_ParamAccess.item,1);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGeometryParameter("Geometry","G","Deformed analytical geometry.",GH_ParamAccess.list);p.AddVectorParameter("Translations","U","Global nodal translations before scaling.",GH_ParamAccess.list);p.AddIntegerParameter("Node IDs","ID","IDs corresponding to translations.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da)
    {
        var m=Native<MidasModel>(da,0);string load=Item<string>(da,1);if(load.Contains(":max",StringComparison.OrdinalIgnoreCase)||load.Contains(":min",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Envelope extrema are not a simultaneous deformed state.");var t=ResultTools.Table(m,"DISPLACEMENTG",load);
        foreach(var (name,index) in new[]{("Stage",2),("Step",3)})if(t.Columns.Contains(name))t=t.Filter(name,Item<string>(da,index));
        double scale=Item<double>(da,4);if(!double.IsFinite(scale))throw new ArgumentException("Invalid scale.");var displacement=new Dictionary<int,Vector3d>();
        foreach(var row in t.Rows){int id=int.Parse(row[t.Column("Node")]);var v=new Vector3d(Data.Real(row[t.Column("DX")]),Data.Real(row[t.Column("DY")]),Data.Real(row[t.Column("DZ")]));if(!displacement.TryAdd(id,v))throw new ArgumentException("More than one displacement state for node "+id);}
        if(displacement.Count==0)throw new ArgumentException("No matching displacement state.");
        var geometry=new List<GeometryBase>();foreach(var e in m.Definition.Elements){var ids=e.Kind==ElementKind.Node?new[]{e.Id}:e.NodeIds;var points=e.Points.Select((p,i)=>displacement.TryGetValue(ids[i],out var v)?new Position(p.X+scale*v.X,p.Y+scale*v.Y,p.Z+scale*v.Z):throw new ArgumentException("Missing displacement for node "+ids[i])).ToArray();geometry.Add(PhysicalGeometry.Analytical(e with{Points=points}));}
        da.SetDataList(0,geometry);da.SetDataList(1,displacement.Values);da.SetDataList(2,displacement.Keys);
    }
}
public sealed class BeamDiagramComponent:SafeComponent
{
    public BeamDiagramComponent():base("Midas Beam Force Diagram","Diagram","Plot native beam force stations. The input table must contain one beam and one physical case/stage/step.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Beam","B","Single beam fragment.",GH_ParamAccess.item);p.AddGenericParameter("Table","T","BEAMFORCE table filtered to one state.",GH_ParamAccess.item);p.AddTextParameter("Column","C","Axial, Shear-y, Shear-z, Torsion, Moment-y, Moment-z.",GH_ParamAccess.item,"Moment-y");p.AddVectorParameter("Direction","D","Global plotting direction.",GH_ParamAccess.item,Vector3d.ZAxis);p.AddNumberParameter("Scale","S","Length/result-unit.",GH_ParamAccess.item,.01);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddCurveParameter("Diagram","D","Polyline through requested stations.",GH_ParamAccess.item);p.AddLineParameter("Ordinates","O","Native ordinates.",GH_ParamAccess.list);p.AddNumberParameter("Values","V","Native values.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da)
    {
        var e=ElementTools.Single(FragmentOf(Item<object>(da,0)));if(e.Kind!=ElementKind.Beam)throw new ArgumentException("Use one beam.");var t=Native<ResultTable>(da,1).ForElement(e);if(t.Type!="BEAMFORCE")throw new ArgumentException("Use BEAMFORCE.");int part=t.Column("Part"),value=t.Column(Item<string>(da,2));var a=PhysicalGeometry.Axes(e);var direction=Item<Vector3d>(da,3);direction-=a.X*(direction*a.X);if(!direction.Unitize())throw new ArgumentException("Plot direction is parallel to beam.");
        double Station(string s)=>s.Trim().Replace("Part","").ToUpperInvariant() switch{"I" or "1" or "I[1]"=>0,"J" or "J[2]"=>1,"1/4"=>.25,"2/4"=>.5,"3/4"=>.75,_=>throw new ArgumentException("Unsupported native station label "+s)};
        var rows=t.Rows.Select(r=>(Station:Station(r[part]),Value:Data.Real(r[value]))).OrderBy(r=>r.Station).ToArray();if(rows.Length<2||rows.Select(r=>r.Station).Distinct().Count()!=rows.Length)throw new ArgumentException("Select exactly one case/stage/step and at least two unique stations.");
        var line=new Line(Preview.Point(e.Points[0]),Preview.Point(e.Points[1]));double scale=Item<double>(da,4);var ordinates=rows.Select(r=>new Line(line.PointAt(r.Station),line.PointAt(r.Station)+direction*(r.Value*scale))).ToArray();da.SetData(0,new Polyline(ordinates.Select(l=>l.To)).ToNurbsCurve());da.SetDataList(1,ordinates);da.SetDataList(2,rows.Select(r=>r.Value));
    }
}
public sealed class VerificationDataComponent:SafeComponent
{
    public VerificationDataComponent():base("Midas Verification Data","Verification Data","Expose model, properties, native quantities and source hashes for a future checker. This component does not perform design checks.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Solved Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Elements","E","Element records with embedded results.",GH_ParamAccess.list);p.AddTextParameter("Definitions JSON","D","Complete API definitions.",GH_ParamAccess.item);p.AddGenericParameter("Results","R","Native result tables.",GH_ParamAccess.list);p.AddGenericParameter("Units","U","Model units.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Missing/failed requests.",GH_ParamAccess.list);p.AddBooleanParameter("Complete","C","All requested result queries returned data. Not an engineering approval.",GH_ParamAccess.item);p.AddTextParameter("Fingerprint","H","Source model fingerprint.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var m=Native<MidasModel>(da,0);var r=m.Results??throw new ArgumentException("No results.");da.SetDataList(0,m.Definition.Elements);da.SetData(1,ModelArchive.Json(ApiModel.Writes(m)));da.SetDataList(2,r.Tables);da.SetData(3,m.Units);da.SetDataList(4,r.Issues);da.SetData(5,r.AnalysisCompleted&&r.Issues.Count==0);da.SetData(6,m.Fingerprint);}
}
