using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public static class ElementData
{
    public static Element Get(object raw,ElementKind? kind=null)
    {
        var value=SafeComponent.Unwrap(raw);
        var element=value is Element e?e:value is Fragment f&&f.Elements.Count==1?f.Elements[0]:throw new ArgumentException("Connect one element from Analyze/Decompose Model.");
        if(kind.HasValue&&element.Kind!=kind)throw new ArgumentException("Expected "+kind+" element.");return element;
    }
    public static ResultTable Table(Element element,string quantity,string loadCase,string stepType,double step,bool filterStep)
    {
        var results=element.Results??throw new ArgumentException("This element has no embedded results. Connect the Beams/Plates output of SAP Analyze and Embed Results, or decompose its solved Model.");
        if(!results.Verified)throw new ArgumentException("Element result provenance is unverified.");
        var table=results.Tables.SingleOrDefault(t=>t.Method==quantity)??throw new ArgumentException("Quantity not embedded: "+quantity+". "+string.Join("; ",results.Issues.Where(i=>i.Method==quantity).Select(i=>i.Message)));
        var rows=Enumerable.Range(0,table.RowCount).Where(i=>(loadCase.Length==0||ResultRows.Text(table,"LoadCase",i)==loadCase)&&(!filterStep||ResultRows.Text(table,"StepType",i)==stepType&&Math.Abs(ResultRows.Number(table,"StepNum",i)-step)<1e-9)).ToArray();
        return table.SelectRows(rows);
    }
}
public abstract class DecomposeElementComponent:SafeComponent
{
    protected abstract ElementKind Kind{get;}
    protected DecomposeElementComponent(string name):base(name,name,"Decompose a solved SAP element, retaining its own forces, stresses/strains and joint results.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Element","E","One beam or plate from a solved Model.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGeometryParameter("Geometry","G","Analysis geometry.",GH_ParamAccess.item);p.AddTextParameter("Name","N","Native object name.",GH_ParamAccess.item);p.AddTextParameter("Property","P","SAP section/property name.",GH_ParamAccess.item);p.AddPointParameter("Vertices","V","Original analysis vertices.",GH_ParamAccess.list);p.AddGenericParameter("Results","R","Tables embedded in this element.",GH_ParamAccess.list);p.AddTextParameter("Quantities","Q","Embedded method keys.",GH_ParamAccess.list);p.AddTextParameter("Cases","C","Stored case/combination names.",GH_ParamAccess.list);p.AddTextParameter("Issues","I","Missing requested quantities.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var e=ElementData.Get(Item<object>(da,0),Kind);da.SetData(0,PhysicalGeometry.AnalysisGeometry(e));da.SetData(1,e.Name);da.SetData(2,e.Property);da.SetDataList(3,e.Points.Select(Preview.Point));da.SetDataList(4,e.Results?.Tables??[]);da.SetDataList(5,e.Results?.Tables.Select(t=>t.Method)??[]);da.SetDataList(6,e.Results?.Tables.Where(t=>t.Columns.ContainsKey("LoadCase")).SelectMany(t=>t.Columns["LoadCase"].EnumerateArray().Select(v=>v.GetString())).Distinct()??[]);da.SetDataList(7,e.Results?.Issues.Select(i=>i.Method+": "+i.Message)??[]);}
}
public sealed class DecomposeBeamComponent:DecomposeElementComponent{protected override ElementKind Kind=>ElementKind.Frame;public DecomposeBeamComponent():base("Decompose SAP Beam"){} }
public sealed class DecomposePlateComponent:DecomposeElementComponent{protected override ElementKind Kind=>ElementKind.Area;public DecomposePlateComponent():base("Decompose SAP Plate"){} }

public sealed class QueryElementResultsComponent:SafeComponent
{
    public QueryElementResultsComponent():base("Query SAP Element Results","Element Results","Query an element's embedded result tables by quantity, case and optional exact step. No SAP connection is needed.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Element","E","One solved beam, plate, node, solid or link.",GH_ParamAccess.item);p.AddTextParameter("Quantity","Q","Native method key, e.g. Results.FrameForce or Results.AreaStressShell.",GH_ParamAccess.item,"Results.FrameForce");p.AddTextParameter("Case","C","Empty = all stored cases and combinations.",GH_ParamAccess.item,"");p.AddBooleanParameter("Filter step","F","Restrict to one exact step type and number.",GH_ParamAccess.item,false);p.AddTextParameter("Step type","T","Native label: empty for static, Mode/Step/Max/Min, etc.",GH_ParamAccess.item,"");p.AddNumberParameter("Step","S","Native step number.",GH_ParamAccess.item,0);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddGenericParameter("Table","R","Filtered native table.",GH_ParamAccess.item);p.AddTextParameter("Columns","C","Columns in native SDK order.",GH_ParamAccess.list);p.AddGenericParameter("Rows","V","One branch {row} with values in Columns order. Labels and station/node IDs remain included.",GH_ParamAccess.tree);}
    protected override void Solve(IGH_DataAccess da){var e=ElementData.Get(Item<object>(da,0));var t=ElementData.Table(e,Item<string>(da,1),Item<string>(da,2),Item<string>(da,4),Item<double>(da,5),Item<bool>(da,3));var columns=ApiSchema.Get(t.Method).Parameters.Where(p=>p.ByRef&&p.IsArray&&t.Columns.ContainsKey(p.Name)).Select(p=>p.Name).ToArray();var rows=new GH_Structure<GH_ObjectWrapper>();for(int i=0;i<t.RowCount;i++)foreach(string col in columns)rows.Append(new GH_ObjectWrapper(ApiResultComponent.Scalar(t.Columns[col][i])),new GH_Path(i));da.SetData(0,t);da.SetDataList(1,columns);da.SetDataTree(2,rows);}
}
public abstract class ElementActionsComponent:SafeComponent
{
    protected abstract ElementKind Kind{get;}
    protected abstract string Quantity{get;}
    protected abstract string[] Columns{get;}
    protected ElementActionsComponent(string name):base(name,name,"Read named engineering quantities directly from one solved element. Cases and step labels accompany every value; native axes, signs and Model units are retained.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Element","E","One solved element.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Empty = all stored cases/combinations.",GH_ParamAccess.item,"");p.AddBooleanParameter("Filter step","F","Use exact step filter.",GH_ParamAccess.item,false);p.AddTextParameter("Step type","T","Native step type.",GH_ParamAccess.item,"");p.AddNumberParameter("Step","S","Native step number.",GH_ParamAccess.item,0);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){foreach(string column in Columns)p.AddNumberParameter(column=="P"?"N (P)":column,column,"Native "+column+" in Model units.",GH_ParamAccess.list);p.AddTextParameter("Cases","C","Case/combination per row.",GH_ParamAccess.list);p.AddTextParameter("Step types","T","Step type per row.",GH_ParamAccess.list);p.AddNumberParameter("Steps","S","Step number per row.",GH_ParamAccess.list);p.AddTextParameter("Sample points","Pnt","Area analysis point labels or frame analysis element labels.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var e=ElementData.Get(Item<object>(da,0),Kind);var table=ElementData.Table(e,Quantity,Item<string>(da,1),Item<string>(da,3),Item<double>(da,4),Item<bool>(da,2));for(int i=0;i<Columns.Length;i++)da.SetDataList(i,table.Columns[Columns[i]].EnumerateArray().Select(v=>v.GetDouble()));da.SetDataList(Columns.Length,Enumerable.Range(0,table.RowCount).Select(i=>ResultRows.Text(table,"LoadCase",i)));da.SetDataList(Columns.Length+1,Enumerable.Range(0,table.RowCount).Select(i=>ResultRows.Text(table,"StepType",i)));da.SetDataList(Columns.Length+2,Enumerable.Range(0,table.RowCount).Select(i=>ResultRows.Number(table,"StepNum",i)));da.SetDataList(Columns.Length+3,Enumerable.Range(0,table.RowCount).Select(i=>ResultRows.Text(table,Kind==ElementKind.Area?"PointElm":"Elm",i)));}
}
public sealed class BeamActionsComponent:ElementActionsComponent
{
    protected override ElementKind Kind=>ElementKind.Frame;protected override string Quantity=>"Results.FrameForce";protected override string[] Columns=>["P","V2","V3","T","M2","M3","ObjSta"];
    public BeamActionsComponent():base("SAP Beam Results N V T M"){}
}
public sealed class PlateActionsComponent:ElementActionsComponent
{
    protected override ElementKind Kind=>ElementKind.Area;protected override string Quantity=>"Results.AreaForceShell";protected override string[] Columns=>["F11","F22","F12","M11","M22","M12","V13","V23"];
    public PlateActionsComponent():base("SAP Plate Results Forces and Moments"){}
}
public sealed class PlateStressComponent:ElementActionsComponent
{
    protected override ElementKind Kind=>ElementKind.Area;protected override string Quantity=>"Results.AreaStressShell";protected override string[] Columns=>["S11Top","S22Top","S12Top","S11Bot","S22Bot","S12Bot","SVMTop","SVMBot","S13Avg","S23Avg"];
    public PlateStressComponent():base("SAP Plate Results Stresses"){}
}
public sealed class PlateStrainComponent:ElementActionsComponent
{
    protected override ElementKind Kind=>ElementKind.Area;protected override string Quantity=>"Results.AreaStrainShell";protected override string[] Columns=>["e11top","e22top","g12top","e11bot","e22bot","g12bot","g13avg","g23avg"];
    public PlateStrainComponent():base("SAP Plate Results Strains"){}
}
