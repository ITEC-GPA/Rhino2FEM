using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino2MidasGen.Core;

namespace Rhino2MidasGen.Grasshopper;

public static class Identity
{
    // Stable across catalogue regeneration and independent from Straus component IDs.
    public static Guid For(string name)=>new(MD5.HashData(Encoding.UTF8.GetBytes("Rhino2MidasGen/1/"+name)));
}
public sealed class PluginInfo:GH_AssemblyInfo
{
    public override string Name=>"Rhino2MidasGen";
    public override Bitmap Icon=>ComponentIcons.Get(Name,"Model");
    public override string Description=>"Rhino 8 / Grasshopper modelling, GEN NX export, analysis and results.";
    public override Guid Id=>Identity.For("Library");
    public override string AuthorName=>"Rhino2MidasGen";
    public override string AuthorContact=>"";
}
public abstract class SafeComponent:GH_Component
{
    protected SafeComponent(string name,string nickname,string description,string category):base(name,nickname,description,"Midas GEN NX",ComponentTopics.For(name,category)){}
    protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
    {base.AppendAdditionalComponentMenuItems(menu);Menu_AppendItem(menu,"Visualizzazione…",(_,_)=>DisplayWindow.Open());Menu_AppendItem(menu,"Guida",(_,_)=>DisplayWindow.Open(1));Menu_AppendItem(menu,"About",(_,_)=>DisplayWindow.Open(2));}
    public override Guid ComponentGuid=>Identity.For(GetType().Name);
    protected override Bitmap Icon=>ComponentIcons.Get(Name,SubCategory);
    public bool PreviewManaged=>OnPingDocument()?.Objects.OfType<PreviewModelComponent>().Any(p=>!p.Hidden&&!p.Locked&&p.Manages(this))==true;
    public override void DrawViewportWires(IGH_PreviewArgs args){if(!PreviewManaged)base.DrawViewportWires(args);}
    public override void DrawViewportMeshes(IGH_PreviewArgs args){if(!PreviewManaged)base.DrawViewportMeshes(args);}
    protected sealed override void SolveInstance(IGH_DataAccess da)
    {try{Message=null;Solve(da);}catch(Exception ex){AddRuntimeMessage(GH_RuntimeMessageLevel.Error,ex.GetBaseException().Message);}}
    protected abstract void Solve(IGH_DataAccess da);
    protected static T Item<T>(IGH_DataAccess da,int index){T value=default!;if(!da.GetData(index,ref value))throw new ArgumentException($"Missing input {index+1}.");return value;}
    public static object? Unwrap(object? value)=>value is IGH_Goo goo?goo.ScriptVariable():value;
    protected static T Native<T>(IGH_DataAccess da,int index)=>Unwrap(Item<object>(da,index)) is T v?v:throw new ArgumentException($"Input {index+1} requires {typeof(T).Name}.");
    protected static List<T> NativeList<T>(IGH_DataAccess da,int index){var values=new List<object>();da.GetDataList(index,values);return values.Select(Unwrap).Select(v=>v is T x?x:throw new ArgumentException($"Input {index+1} requires {typeof(T).Name}.")).ToList();}
    protected static Fragment FragmentOf(object? value)=>Unwrap(value) switch{Fragment f=>f,MidasModel m=>m.Definition,Element e=>new Fragment(elements:[e]),ApiEntry op=>new Fragment(entries:[op]),_=>throw new ArgumentException("Expected a Midas GEN fragment, element, or Model.")};
    protected static void Output(IGH_DataAccess da,int i,object value)=>da.SetData(i,value is MidasModel m?new ModelGoo(m):value is Fragment f?new FragmentGoo(f):value);
}

public sealed class FragmentGoo:GH_Goo<Fragment>,IGH_PreviewData
{
    public FragmentGoo(){}public FragmentGoo(Fragment value){Value=value;}
    public override bool IsValid=>Value!=null;
    public override string TypeName=>"Midas GEN Definition";
    public override string TypeDescription=>"Immutable Midas GEN objects and deferred assignments";
    public override IGH_Goo Duplicate()=>new FragmentGoo(Value);
    public override object ScriptVariable()=>Value;
    public override string ToString()=>Value?.ToString()??"Null Midas GEN definition";
    public override bool CastTo<Q>(ref Q target)
    {
        if(typeof(Q)==typeof(GH_Brep)||typeof(Q)==typeof(Brep))return BrepCasting.TryCast(()=>PreviewScene.For(Value),out target);
        return base.CastTo(ref target);
    }
    public BoundingBox ClippingBox{get{if(Value==null||!DisplayPreferences.Current.Enabled||!DisplayPreferences.Current.Automatic)return BoundingBox.Empty;var scene=PreviewScene.For(Value);scene.Prepare(DisplayPreferences.Current);return scene.Bounds;}}
    public void DrawViewportWires(GH_PreviewWireArgs args){if(Value!=null&&DisplayPreferences.Current.Automatic)PreviewScene.For(Value).DrawWires(args.Pipeline,DisplayPreferences.Current,args.Color);}
    public void DrawViewportMeshes(GH_PreviewMeshArgs args){if(Value!=null&&DisplayPreferences.Current.Automatic)PreviewScene.For(Value).DrawMeshes(args.Pipeline,DisplayPreferences.Current,args.Material);}
    public override bool Write(GH_IO.Serialization.GH_IWriter writer){if(Value==null)return false;writer.SetString("Midas",ModelArchive.SerializeFragment(Value));return true;}
    public override bool Read(GH_IO.Serialization.GH_IReader reader){if(!reader.ItemExists("Midas"))return false;Value=ModelArchive.DeserializeFragment(reader.GetString("Midas"));return true;}
}
public sealed class ModelGoo:GH_Goo<MidasModel>,IGH_PreviewData
{
    public ModelGoo(){}public ModelGoo(MidasModel value){Value=value;}
    public override bool IsValid=>Value!=null;
    public override string TypeName=>"Midas GEN Model";
    public override string TypeDescription=>"Midas GEN model with units, geometry, definitions and optional results";
    public override IGH_Goo Duplicate()=>new ModelGoo(Value);
    public override object ScriptVariable()=>Value;
    public override string ToString()=>Value?.ToString()??"Null Midas GEN model";
    public override bool CastTo<Q>(ref Q target)
    {
        if(typeof(Q)==typeof(GH_Brep)||typeof(Q)==typeof(Brep))return BrepCasting.TryCast(()=>PreviewScene.For(Value),out target);
        return base.CastTo(ref target);
    }
    public BoundingBox ClippingBox{get{if(Value==null||!DisplayPreferences.Current.Enabled||!DisplayPreferences.Current.Automatic)return BoundingBox.Empty;var scene=PreviewScene.For(Value);scene.Prepare(DisplayPreferences.Current);return scene.Bounds;}}
    public void DrawViewportWires(GH_PreviewWireArgs args){if(Value!=null&&DisplayPreferences.Current.Automatic)PreviewScene.For(Value).DrawWires(args.Pipeline,DisplayPreferences.Current,args.Color);}
    public void DrawViewportMeshes(GH_PreviewMeshArgs args){if(Value!=null&&DisplayPreferences.Current.Automatic)PreviewScene.For(Value).DrawMeshes(args.Pipeline,DisplayPreferences.Current,args.Material);}
    public override bool Write(GH_IO.Serialization.GH_IWriter writer){if(Value==null)return false;writer.SetString("Midas",ModelArchive.Serialize(Value));return true;}
    public override bool Read(GH_IO.Serialization.GH_IReader reader){if(!reader.ItemExists("Midas"))return false;Value=ModelArchive.Deserialize(reader.GetString("Midas"));return true;}
}
internal static class BrepCasting
{
    public static bool TryCast<Q>(Func<PreviewScene> scene,out Q target)
    {
        target=default!;
        try
        {
            var brep=scene().CopySingleBrep();if(brep==null)return false;
            target=typeof(Q)==typeof(GH_Brep)?(Q)(object)new GH_Brep(brep):(Q)(object)brep;
            return true;
        }
        catch(ArgumentException){return false;}
        catch(InvalidOperationException){return false;}
    }
}
public static class Preview
{
    public static Point3d Point(Position p)=>new(p.X,p.Y,p.Z);
    public static Position Position(Point3d p)=>new(p.X,p.Y,p.Z);
    public static BoundingBox Bounds(IEnumerable<Element> elements)=>new(elements.SelectMany(e=>e.Points).Select(Point));
    public static Mesh Mesh(Element e)
    {
        var mesh=new Mesh();foreach(var p in e.Points)mesh.Vertices.Add(Point(p));
        if(e.Kind is ElementKind.Plate or ElementKind.Wall){if(e.Points.Count==3)mesh.Faces.AddFace(0,1,2);else mesh.Faces.AddFace(0,1,2,3);}
        if(e.Kind==ElementKind.Solid&&e.Points.Count==8){mesh.Faces.AddFace(0,3,2,1);mesh.Faces.AddFace(4,5,6,7);mesh.Faces.AddFace(0,1,5,4);mesh.Faces.AddFace(1,2,6,5);mesh.Faces.AddFace(2,3,7,6);mesh.Faces.AddFace(3,0,4,7);}
        if(e.Kind==ElementKind.Solid&&e.Points.Count==4){mesh.Faces.AddFace(0,2,1);mesh.Faces.AddFace(0,1,3);mesh.Faces.AddFace(1,2,3);mesh.Faces.AddFace(2,0,3);}
        if(e.Kind==ElementKind.Solid&&e.Points.Count==6){mesh.Faces.AddFace(0,2,1);mesh.Faces.AddFace(3,4,5);mesh.Faces.AddFace(0,1,4,3);mesh.Faces.AddFace(1,2,5,4);mesh.Faces.AddFace(2,0,3,5);}
        mesh.Normals.ComputeNormals();return mesh;
    }
    public static void Draw(IEnumerable<Element> elements,GH_PreviewWireArgs args)
    {foreach(var e in elements){if(e.Kind==ElementKind.Node)args.Pipeline.DrawPoint(Point(e.Points[0]),Rhino.Display.PointStyle.RoundSimple,3,args.Color);else if(e.Points.Count==2)args.Pipeline.DrawLine(Point(e.Points[0]),Point(e.Points[1]),args.Color,args.Thickness);else args.Pipeline.DrawMeshWires(Mesh(e),args.Color);}}
    public static void Shade(IEnumerable<Element> elements,GH_PreviewMeshArgs args)
    {foreach(var e in elements.Where(e=>e.Kind is ElementKind.Plate or ElementKind.Wall or ElementKind.Solid))args.Pipeline.DrawMeshShaded(Mesh(e),args.Material);}
}
