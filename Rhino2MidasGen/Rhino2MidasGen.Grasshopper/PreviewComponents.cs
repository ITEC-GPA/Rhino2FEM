using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2MidasGen.Core;

namespace Rhino2MidasGen.Grasshopper;

public sealed class PreviewModelComponent:SafeComponent
{
    private readonly List<(PreviewScene Scene,DisplayOptions? Options)> scenes=[];
    public PreviewModelComponent():base("Preview Midas GEN Model","Preview","Preview FEM data and physical solids; Breps remain available with display disabled.","13-Preview"){}
    public override bool IsPreviewCapable=>true;
    public bool Manages(GH_Component component)
    {
        var visited=new HashSet<Guid>();var stack=new Stack<IGH_Param>(Params.Input[0].Sources);
        while(stack.TryPop(out var param))
        {
            if(!visited.Add(param.InstanceGuid))continue;
            var owner=param.Attributes?.GetTopLevel.DocObject;
            if(owner==component)return true;
            if(owner is GH_Component c)foreach(var input in c.Params.Input)foreach(var source in input.Sources)stack.Push(source);
            else foreach(var source in param.Sources)stack.Push(source);
        }
        return false;
    }
    public override BoundingBox ClippingBox{get{var box=BoundingBox.Empty;foreach(var s in scenes)box.Union(s.Scene.Bounds);return box;}}
    protected override void BeforeSolveInstance(){scenes.Clear();base.BeforeSolveInstance();}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Model, elements or section.",GH_ParamAccess.item);p.AddGenericParameter("Settings","S","Optional Midas GEN Display Settings.",GH_ParamAccess.item);p[1].Optional=true;}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Status","S","Preview summary.",GH_ParamAccess.item);p.AddTextParameter("Issues","I","Unsupported physical geometry.",GH_ParamAccess.list);p.AddBrepParameter("Breps","B","Closed physical solids for geometry operations and bake.",GH_ParamAccess.list);p.AddTextParameter("Names","N","Element names matching Breps.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var input=Unwrap(Item<object>(da,0));var scene=input is MidasModel m?PreviewScene.For(m):PreviewScene.For(FragmentOf(input));object settings=null!;DisplayOptions? local=da.GetData(1,ref settings)?Unwrap(settings) as DisplayOptions??throw new ArgumentException("Use Midas GEN Display Settings."):null;var o=Options(local);scene.Prepare(o);scenes.Add((scene,local));var solids=scene.CopySolids(out var issues);da.SetData(0,$"{solids.Count} solids; {(o.Enabled?"preview on":"preview off")}");da.SetDataList(1,issues);da.SetDataList(2,solids.Select(s=>s.Brep));da.SetDataList(3,solids.Select(s=>s.Name));}
    private static DisplayOptions Options(DisplayOptions? local){var o=local??DisplayPreferences.Current;return o with{Enabled=o.Enabled&&DisplayPreferences.Current.Enabled};}
    public override void DrawViewportWires(IGH_PreviewArgs args){if(Hidden||Locked)return;foreach(var s in scenes)s.Scene.DrawWires(args.Display,Options(s.Options),Attributes.Selected?Color.LimeGreen:Color.SteelBlue);}
    public override void DrawViewportMeshes(IGH_PreviewArgs args){if(Hidden||Locked)return;foreach(var s in scenes)s.Scene.DrawMeshes(args.Display,Options(s.Options),new Rhino.Display.DisplayMaterial(Attributes.Selected?Color.LimeGreen:Color.SteelBlue,.25));}
}
public sealed class PhysicalGeometryComponent:SafeComponent
{
    public PhysicalGeometryComponent():base("Midas GEN Physical Geometry","Physical","Closed Breps of beams, plates and solids. Unsupported shapes produce explicit issues.","13-Preview"){}
    protected override void RegisterInputParams(GH_InputParamManager p)=>p.AddGenericParameter("Model","M","Model or fragment.",GH_ParamAccess.item);
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddBrepParameter("Breps","B","Closed solids.",GH_ParamAccess.list);p.AddTextParameter("Names","N","Matching element names.",GH_ParamAccess.list);p.AddTextParameter("Issues","I","Elements without physical solids.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){var raw=Unwrap(Item<object>(da,0));var scene=raw is MidasModel m?PreviewScene.For(m):PreviewScene.For(FragmentOf(raw));var solids=scene.CopySolids(out var issues);da.SetDataList(0,solids.Select(s=>s.Brep));da.SetDataList(1,solids.Select(s=>s.Name));da.SetDataList(2,issues);}
}
public sealed class BakeGeometryComponent:SafeComponent
{
    private bool wasRun;
    public BakeGeometryComponent():base("Bake Midas GEN Geometry","Bake","Bake physical solids or analytical geometry into the active Rhino document. Operates on Bake false→true.","13-Preview"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Model","M","Model.",GH_ParamAccess.item);p.AddBooleanParameter("Physical","P","Physical solid geometry.",GH_ParamAccess.item,true);p.AddBooleanParameter("Bake","B","Rising edge bakes once.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){p.AddTextParameter("Object IDs","ID","Created Rhino object GUIDs.",GH_ParamAccess.list);p.AddTextParameter("Issues","I","Omitted elements.",GH_ParamAccess.list);}
    protected override void Solve(IGH_DataAccess da){if(da.Iteration!=0)throw new ArgumentException("Bake one Model per component.");bool run=Item<bool>(da,2);bool trigger=run&&!wasRun;wasRun=run;if(!trigger)return;var doc=Rhino.RhinoDoc.ActiveDoc??throw new InvalidOperationException("No active Rhino document.");var model=Native<MidasModel>(da,0);var geometry=new List<(string Name,GeometryBase Geometry)>();var issues=new List<string>();if(Item<bool>(da,1)){var solids=PhysicalGeometry.CreateSolids(model,out issues);geometry.AddRange(solids.Select(s=>(s.Name,(GeometryBase)s.Brep)));}else geometry.AddRange(model.Definition.Elements.Select(e=>(e.ToString(),PhysicalGeometry.Analytical(e))));var ids=new List<Guid>();uint undo=doc.BeginUndoRecord("Bake Midas GEN Geometry");try{foreach(var g in geometry){var attributes=new Rhino.DocObjects.ObjectAttributes{Name=g.Name};var id=doc.Objects.Add(g.Geometry,attributes);if(id==Guid.Empty)issues.Add("Could not bake "+g.Name);else ids.Add(id);}}finally{doc.EndUndoRecord(undo);doc.Views.Redraw();}da.SetDataList(0,ids.Select(i=>i.ToString()));da.SetDataList(1,issues);}
}
