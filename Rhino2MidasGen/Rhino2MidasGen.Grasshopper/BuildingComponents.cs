using Grasshopper.Kernel;
using Rhino2MidasGen.Core;

namespace Rhino2MidasGen.Grasshopper;

public sealed class WallElementComponent:ElementComponent
{
    protected override ElementKind Kind=>ElementKind.Wall;
    public WallElementComponent():base("Wall"){}
    protected override void RegisterInputParams(GH_InputParamManager p)
    {
        base.RegisterInputParams(p);
        p.AddIntegerParameter("Wall ID","W","Native WALL group ID, shared by the mesh elements of the same wall. Distinct from element ID.",GH_ParamAccess.item,1);
        p.AddIntegerParameter("Subtype","S","1 membrane, 2 plate.",GH_ParamAccess.item,2);
        p.AddIntegerParameter("Formulation","F","0 plate base, 1 CRB pin, 2 CRB fixed.",GH_ParamAccess.item,0);
    }
    protected override string Extra(IGH_DataAccess da)=>Building.WallData(Item<int>(da,6),Item<int>(da,7),Item<int>(da,8));
}
public sealed class StoryComponent:SafeComponent
{
    public StoryComponent():base("Midas GEN Story","Story","Define a GEN floor level and optional rigid diaphragm. Wind widths/eccentricities default to zero; use the native Story Data component to specify them.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddIntegerParameter("ID","ID","Positive native story ID.",GH_ParamAccess.item,1);p.AddTextParameter("Name","N","Unique story name.",GH_ParamAccess.item,"Roof");p.AddNumberParameter("Level","Z","Global Z elevation in model length units.",GH_ParamAccess.item,3);p.AddBooleanParameter("Rigid diaphragm","D","Enable the native floor diaphragm.",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Story","S","Connect to Build Model.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Building.Story(Item<int>(da,0),Item<string>(da,1),Item<double>(da,2),Item<bool>(da,3)));
}
public sealed class WallExampleComponent:SafeComponent
{
    public WallExampleComponent():base("Midas GEN Wall and Story Example","Wall Example","4 × 3 m wall, 200 mm thickness, fixed base, 10 kN lateral load; two story levels. Offline example in kN/m.","07-Model"){}
    protected override void RegisterInputParams(GH_InputParamManager p){}
    protected override void RegisterOutputParams(GH_OutputParamManager p)=>p.AddGenericParameter("Model","M","Connect to Preview or Analyze.",GH_ParamAccess.item);
    protected override void Solve(IGH_DataAccess da)=>Output(da,0,Building.Example());
}
public sealed class DecomposeWallComponent:ElementInfoComponent
{
    public DecomposeWallComponent():base("Decompose Midas GEN Wall"){}
    protected override void RegisterOutputParams(GH_OutputParamManager p){base.RegisterOutputParams(p);p.AddIntegerParameter("Wall ID","W","Native WALL group ID used for wall resultants.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da){var e=ElementTools.Single(FragmentOf(Item<object>(da,0)));if(e.Kind!=ElementKind.Wall)throw new ArgumentException("Connect one wall element.");base.Solve(da);da.SetData(7,Building.WallId(e));}
}
public sealed class WallActionsComponent:SafeComponent
{
    private static readonly string[] Actions=["Axial","Shear-y","Shear-z","Torsion","Moment-y","Moment-z"];
    public WallActionsComponent():base("Midas GEN Wall Actions","Wall Actions","Native resultants for the WALL group across its stories, not forces of a single mesh element. Choose Story and top/bottom block; output preserves native row identity.","12-Results"){}
    protected override void RegisterInputParams(GH_InputParamManager p){p.AddGenericParameter("Source","S","Solved Model, wall fragment or WALL_FORCE_MOMENT table.",GH_ParamAccess.item);p.AddTextParameter("Case","C","Optional exact Load label.",GH_ParamAccess.item,"");p.AddTextParameter("Story","St","Optional exact Story label. Empty returns all stories in the WALL group.",GH_ParamAccess.item,"");p.AddBooleanParameter("Bottom","B","False: first native block (top); true: second native block (bottom).",GH_ParamAccess.item,false);}
    protected override void RegisterOutputParams(GH_OutputParamManager p){foreach(var a in Actions)p.AddNumberParameter(a,a,"Native group resultant in model units.",GH_ParamAccess.list);p.AddTextParameter("States","S","Story | Wall | Load | Part for every row.",GH_ParamAccess.list);p.AddGenericParameter("Table","T","Complete filtered table, including both top and bottom.",GH_ParamAccess.item);}
    protected override void Solve(IGH_DataAccess da)
    {
        var table=ResultTools.Table(Item<object>(da,0),"WALL_FORCE_MOMENT",Item<string>(da,1));string story=Item<string>(da,2);if(story.Length>0)table=table.Filter("Story",story);int occurrence=Item<bool>(da,3)?1:0;
        for(int i=0;i<Actions.Length;i++)da.SetDataList(i,table.Numbers(Actions[i],occurrence));
        da.SetDataList(6,table.Rows.Select(r=>string.Join(" | ",new[]{"Story","Wall","Load"}.Select(c=>r[table.Column(c)]).Append(r[table.Column("Part",occurrence)]))));da.SetData(7,table);
    }
}
