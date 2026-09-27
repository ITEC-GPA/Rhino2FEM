using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Rhino.Display;
using Rhino.Geometry;
using Rhino2SAP.Core;

namespace Rhino2SAP.Grasshopper;

public sealed class PreviewScene
{
    private static readonly ConditionalWeakTable<Fragment,PreviewScene> Cache=new();
    private static readonly ConditionalWeakTable<SapModel,PreviewScene> ModelCache=new();
    public static PreviewScene For(Fragment definition)=>Cache.GetValue(definition,f=>new PreviewScene(f));
    public static PreviewScene For(SapModel model)=>ModelCache.GetValue(model,m=>new PreviewScene(m));
    private readonly SapModel model;
    private readonly bool sectionOnly;
    private DisplayOptions? preparedFor;
    private readonly List<(Line Line,Color Color,bool Arrow)> marks=[];
    private readonly List<(Point3d Point,string Text,Color Color)> labels=[];
    private readonly List<(Point3d Point,Color Color,int Size)> dots=[];
    private IReadOnlyList<GeometryBase> physical=[];
    private IReadOnlyList<PhysicalGeometry.Solid>? solidGeometry;
    private List<string> solidIssues=[];
    public IReadOnlyList<string> Issues=>issues;
    private readonly List<string> issues=[];
    public BoundingBox Bounds { get; private set; }
    private static readonly Color LoadColor=Color.FromArgb(225,116,37),SupportColor=Color.FromArgb(137,69,181),AttributeColor=Color.FromArgb(27,146,137);
    private PreviewScene(SapModel value){model=value;Bounds=Preview.Bounds(model.Definition.Elements);}
    private PreviewScene(Fragment definition)
    {
        var section=definition.Operations.LastOrDefault(o=>o.Key.StartsWith("PropFrame.")&&o.Arguments.ContainsKey("MatProp")&&o.Arguments.ContainsKey("Name"));
        sectionOnly=definition.Elements.Count==0&&section!=null;
        if(sectionOnly)definition=new Fragment(definition.Operations,[new Element("Section preview",ElementKind.Frame,[new(0,0,0),new(0,0,.1)],section!.Arguments["Name"].GetString()!)]);
        model=new SapModel(definition);Bounds=Preview.Bounds(model.Definition.Elements);
    }
    public IReadOnlyList<PhysicalGeometry.Solid> CopySolids(out IReadOnlyList<string> issues)
    {
        solidGeometry??=PhysicalGeometry.CreateSolids(model,out solidIssues);
        issues=solidIssues;
        return solidGeometry.Select(s=>new PhysicalGeometry.Solid(s.Name,s.Brep.DuplicateBrep())).ToArray();
    }
    public Brep? CopySingleBrep()
    {
        // A cast to one Brep must never silently discard other model elements.
        if(model.Definition.Elements.Count(e=>e.Kind!=ElementKind.Node)!=1)return null;
        solidGeometry??=PhysicalGeometry.CreateSolids(model,out solidIssues);
        return solidGeometry.Count==1?solidGeometry[0].Brep.DuplicateBrep():null;
    }
    private static Point3d Center(Element e)=>new(e.Points.Average(p=>p.X),e.Points.Average(p=>p.Y),e.Points.Average(p=>p.Z));
    private static double Num(Operation o,string name,double fallback=0)=>o.Arguments.TryGetValue(name,out var v)&&v.ValueKind==JsonValueKind.Number?v.GetDouble():fallback;
    private static string Text(Operation o,string name,string fallback="")=>o.Arguments.TryGetValue(name,out var v)&&v.ValueKind==JsonValueKind.String?v.GetString()!:fallback;
    private static bool Flag(Operation o,string name,bool fallback=false)=>o.Arguments.TryGetValue(name,out var v)&&v.ValueKind is JsonValueKind.True or JsonValueKind.False?v.GetBoolean():fallback;
    private static double[] Numbers(Operation o,string name)=>o.Arguments.TryGetValue(name,out var v)&&v.ValueKind==JsonValueKind.Array?v.EnumerateArray().Select(x=>x.GetDouble()).ToArray():[];
    private static bool[] Flags(Operation o,string name)=>o.Arguments.TryGetValue(name,out var v)&&v.ValueKind==JsonValueKind.Array?v.EnumerateArray().Select(x=>x.GetBoolean()).ToArray():[];
    private static string ShortArguments(Operation op)=>string.Join("; ",op.Arguments.Where(p=>p.Key is not ("Name" or "ItemType" or "Replace" or "GUID" or "Notes")).Take(5).Select(p=>p.Key+"="+JsonSerializer.Serialize(p.Value)));
    private IEnumerable<Element> Targets(Operation op)
    {
        string path=op.Key.Split('.')[0];ElementKind? kind=path switch{"PointObj"=>ElementKind.Node,"FrameObj"=>ElementKind.Frame,"AreaObj"=>ElementKind.Area,"SolidObj"=>ElementKind.Solid,"LinkObj"=>ElementKind.Link,"CableObj"=>ElementKind.Cable,_=>null};
        if(kind==null)return [];
        var candidates=model.Definition.Elements.Where(e=>e.Kind==kind);string name=Text(op,"Name");
        if((int)Num(op,"ItemType")==1)
        {
            if(name=="ALL")return candidates;
            var members=new HashSet<string>();foreach(var assignment in model.Definition.Operations.Where(o=>o.Key==path+".SetGroupAssign"&&Text(o,"GroupName")==name)){if(Flag(assignment,"Remove"))members.Remove(Text(assignment,"Name"));else members.Add(Text(assignment,"Name"));}
            return candidates.Where(e=>members.Contains(e.Name));
        }
        return candidates.Where(e=>e.Name==name);
    }
    public void Prepare(DisplayOptions options)
    {
        if(options==preparedFor)return;preparedFor=options with{};marks.Clear();labels.Clear();dots.Clear();issues.Clear();physical=[];Bounds=Preview.Bounds(model.Definition.Elements);
        if(!options.Enabled)return;
        if(options.Sections){physical=PhysicalGeometry.Create(model,out var physicalIssues);issues.AddRange(physicalIssues);foreach(var g in physical)Bounds.Union(g.GetBoundingBox(true));}
        foreach(var e in model.Definition.Elements)
        {
            if(e.Points.Count==0)continue;
            var center=Center(e);
            if(options.Labels)labels.Add((center,e.Kind+" "+e.Name+(e.Property.Length>0?" / "+e.Property:""),AttributeColor));
            if(options.Axes&&e.Kind is ElementKind.Frame or ElementKind.Area)
            {
                try{var axes=Axes(e);Arrow(center,axes[0]*options.SymbolScale,Color.Red);Arrow(center,axes[1]*options.SymbolScale,Color.Green);Arrow(center,axes[2]*options.SymbolScale,Color.Blue);}catch(Exception ex){issues.Add(e.Name+": "+ex.Message);}
            }
            if(options.Springs&&e.Kind==ElementKind.Link)Zigzag(Preview.Point(e.Points[0]),Preview.Point(e.Points[1]),options.SymbolScale*.15,AttributeColor);
        }
        foreach(var op in model.Definition.Operations)
        {
            bool isLoad=op.Key.Contains(".SetLoad");
            if(isLoad&&(!(op.Key=="PointObj.SetLoadDispl"?options.Supports:options.Loads)||options.LoadPattern.Length>0&&Text(op,"LoadPat")!=options.LoadPattern))continue;
            foreach(var e in Targets(op))
            {
                if(e.Points.Count==0)continue;var at=Center(e);double s=options.SymbolScale;bool handled=false;
                try
                {
                    if(isLoad){Load(op,e,options);handled=true;}
                    else if(op.Key=="PointObj.SetRestraint"&&options.Supports)
                    {
                        var flags=Flags(op,"Value");if(flags.Any(v=>v)){Line(at+new Vector3d(-s/2,-s/2,0),at+new Vector3d(s/2,-s/2,0),SupportColor);Line(at+new Vector3d(-s/2,-s/2,0),at+new Vector3d(0,0,s/2),SupportColor);Line(at+new Vector3d(s/2,-s/2,0),at+new Vector3d(0,0,s/2),SupportColor);dots.Add((at,SupportColor,4));Label(at,"Fix "+string.Join(",",new[]{"U1","U2","U3","R1","R2","R3"}.Where((_,i)=>i<flags.Length&&flags[i])),SupportColor,options);}handled=true;
                    }
                    else if(op.Key=="PointObj.SetSpring"&&options.Springs){Zigzag(at-new Vector3d(0,0,s),at,s*.15,AttributeColor);Label(at,"K="+string.Join(",",Numbers(op,"K").Select(v=>v.ToString("G3"))),AttributeColor,options);handled=true;}
                    else if(op.Key=="PointObj.SetMass"&&options.Masses){dots.Add((at,Color.SlateGray,8));Label(at,"Mass="+string.Join(",",Numbers(op,"M").Select(v=>v.ToString("G3"))),Color.SlateGray,options);handled=true;}
                    else if(op.Key=="FrameObj.SetReleases"&&options.Releases)
                    {for(int end=0;end<2;end++){var flags=Flags(op,end==0?"II":"JJ");if(flags.Any(x=>x)){var p=Preview.Point(e.Points[end]);Ring(p,Vector3d.ZAxis,s*.2,Color.Goldenrod);Label(p,"Release "+(end==0?"I ":"J ")+string.Join(",",new[]{"U1","U2","U3","R1","R2","R3"}.Where((_,i)=>i<flags.Length&&flags[i])),Color.Goldenrod,options);}}handled=true;}
                    else if(op.Key=="FrameObj.SetInsertionPoint"&&options.Offsets)
                    {for(int end=0;end<2;end++){var offsets=Numbers(op,end==0?"Offset1":"Offset2");if(offsets.Length==3){var vector=new Vector3d(offsets[0],offsets[1],offsets[2]);if(Text(op,"CSys","Local")=="Local"){var a=Axes(e);vector=a[0]*offsets[0]+a[1]*offsets[1]+a[2]*offsets[2];}else if(Text(op,"CSys")!="Global")throw new ArgumentException("Named coordinate system offset shown as values only.");Line(Preview.Point(e.Points[end]),Preview.Point(e.Points[end])+vector,Color.Magenta);dots.Add((Preview.Point(e.Points[end])+vector,Color.Magenta,3));}}Label(at,"Insertion "+Num(op,"CardinalPoint"),Color.Magenta,options);handled=true;}
                    else if(op.Key=="AreaObj.SetOffsets"&&options.Offsets){var offsets=Numbers(op,"Offset");var normal=Axes(e)[2];for(int i=0;i<Math.Min(e.Points.Count,offsets.Length);i++)Line(Preview.Point(e.Points[i]),Preview.Point(e.Points[i])+normal*offsets[i],Color.Magenta);Label(at,"Area offset: "+ShortArguments(op),Color.Magenta,options);handled=true;}
                    else if(op.Key.Contains("SetLocalAxes")){handled=true;}
                    if(!handled&&options.OtherAttributes&&!KnownHidden(op,options))Badge(at,op.Key.Split('.').Last()+" "+ShortArguments(op),AttributeColor,options);
                }
                catch(Exception ex){issues.Add(e.Name+" / "+op.Key+": "+ex.Message);Badge(at,op.Key.Split('.').Last()+" "+ShortArguments(op),isLoad?LoadColor:AttributeColor,options);}
            }
        }
        foreach(var line in marks)Bounds.Union(line.Line.BoundingBox);foreach(var dot in dots)Bounds.Union(dot.Point);
    }
    private static bool KnownHidden(Operation o,DisplayOptions s)=>o.Key=="PointObj.SetRestraint"&&!s.Supports||o.Key=="PointObj.SetSpring"&&!s.Springs||o.Key=="PointObj.SetMass"&&!s.Masses||o.Key=="FrameObj.SetReleases"&&!s.Releases||o.Key.Contains("Offset")&&!s.Offsets||o.Key=="FrameObj.SetInsertionPoint"&&!s.Offsets;
    private void Load(Operation op,Element e,DisplayOptions o)
    {
        var at=Center(e);string caseName=Text(op,"LoadPat");
        if(op.Key=="PointObj.SetLoadForce"||op.Key=="PointObj.SetLoadDispl")
        {
            if(op.Key.EndsWith("SetLoadDispl")&&!o.Supports)return;
            string cs=Text(op,"CSys",op.Key.EndsWith("SetLoadForce")?"Global":"Local");if(cs!="Global"&&(cs!="Local"||model.Definition.Operations.Any(x=>x.Key=="PointObj.SetLocalAxes"&&Text(x,"Name")==e.Name)))throw new ArgumentException("Non-global joint directions shown as values; use native coordinate transformation for exact arrows.");
            var v=Numbers(op,"Value");for(int i=0;i<Math.Min(6,v.Length);i++){if(v[i]==0)continue;var axis=new[]{Vector3d.XAxis,Vector3d.YAxis,Vector3d.ZAxis}[i%3];if(i<3)Force(at,axis,v[i],o);else Moment(at,axis,v[i],o);}
            Label(at,caseName+" "+op.Key.Split('.').Last()+" ["+string.Join(",",v.Select(x=>x.ToString("G3")))+"]",LoadColor,o);return;
        }
        if(op.Key is "FrameObj.SetLoadDistributed" or "FrameObj.SetLoadPoint" or "CableObj.SetLoadDistributed" or "CableObj.SetLoadPoint")
        {
            var line=new Line(Preview.Point(e.Points[0]),Preview.Point(e.Points[1]));var direction=LoadDirection(op,e);bool relative=Flag(op,"RelDist",true);double factor=relative?1:1/line.Length;
            if(op.Key.EndsWith("Distributed")){double a=Num(op,"Dist1")*factor,b=Num(op,"Dist2",1)*factor,q1=Num(op,"Val1"),q2=Num(op,"Val2");for(int i=0;i<=5;i++){double t=i/5d;var p=line.PointAt(a+(b-a)*t);double q=q1+(q2-q1)*t;if(Num(op,"MyType",1)==2)Moment(p,direction,q,o);else Force(p,direction,q,o);}Label(at,$"{caseName} q={q1:G4} → {q2:G4}",LoadColor,o);}
            else{var p=line.PointAt(Num(op,"Dist")*factor);double value=Num(op,"Val");if(Num(op,"MyType",1)==2)Moment(p,direction,value,o);else Force(p,direction,value,o);Label(p,$"{caseName} {value:G4}",LoadColor,o);}return;
        }
        if(op.Key is "AreaObj.SetLoadUniform" or "AreaObj.SetLoadUniformToFrame")
        {var direction=LoadDirection(op,e);double value=Num(op,"Value");Force(at,direction,value,o);foreach(var p in e.Points)Force((Preview.Point(p)+at)/2,direction,value,o);Label(at,$"{caseName} q={value:G4}",LoadColor,o);return;}
        // Temperature, pressure faces, strains, target forces and other advanced loads carry explicit values.
        Badge(at,caseName+" "+op.Key.Split('.').Last()+" "+ShortArguments(op),LoadColor,o);
        issues.Add(e.Name+" / "+op.Key+": symbolic preview with native values (no inferred load direction).");
    }
    private Vector3d LoadDirection(Operation o,Element e)
    {
        int dir=(int)Num(o,"Dir");string cs=Text(o,"CSys","Global");
        if(dir is >=1 and <=3)return Axes(e)[dir-1];
        if(cs!="Global")throw new ArgumentException("Named load coordinate systems are displayed symbolically.");
        if(dir is >=4 and <=6)return new[]{Vector3d.XAxis,Vector3d.YAxis,Vector3d.ZAxis}[dir-4];
        if(dir==10)return -Vector3d.ZAxis;
        throw new ArgumentException("Projected-direction load: symbolic preview retains the native direction code.");
    }
    private Vector3d[] Axes(Element e)
    {
        if(e.Kind is ElementKind.Frame or ElementKind.Cable or ElementKind.Link){var a=PhysicalGeometry.FrameAxes(e,model.Definition);return[a.One,a.Two,a.Three];}
        if(e.Kind==ElementKind.Area)
        {
            var three=Vector3d.CrossProduct(Preview.Point(e.Points[1])-Preview.Point(e.Points[0]),Preview.Point(e.Points[2])-Preview.Point(e.Points[0]));if(!three.Unitize())throw new ArgumentException("Degenerate area axes.");
            var one=Vector3d.CrossProduct(Vector3d.ZAxis,three);if(one.Length<1e-8)one=Vector3d.XAxis;one.Unitize();
            var advanced=model.Definition.Operations.LastOrDefault(x=>x.Key=="AreaObj.SetLocalAxesAdvanced"&&Text(x,"Name")==e.Name);
            if(advanced!=null&&Flag(advanced,"Active")){if(Num(advanced,"PlVectOpt")!=3||Text(advanced,"PlCSys")!="Global")throw new ArgumentException("Advanced area axes cannot be reconstructed from this assignment.");var v=Numbers(advanced,"PlVect");one=new(v[0],v[1],v[2]);one-=three*(one*three);one.Unitize();if(Num(advanced,"Plane2")==32)one=Vector3d.CrossProduct(one,three);}
            var angle=model.Definition.Operations.LastOrDefault(x=>x.Key=="AreaObj.SetLocalAxes"&&Text(x,"Name")==e.Name);if(angle!=null)one.Rotate(Num(angle,"Ang")*Math.PI/180,three);return[one,Vector3d.CrossProduct(three,one),three];
        }
        return[Vector3d.XAxis,Vector3d.YAxis,Vector3d.ZAxis];
    }
    private void Line(Point3d a,Point3d b,Color color)=>marks.Add((new Line(a,b),color,false));
    private void Arrow(Point3d at,Vector3d vector,Color color){if(vector.Length>1e-12)marks.Add((new Line(at,at+vector),color,true));}
    private void Force(Point3d at,Vector3d direction,double value,DisplayOptions options){if(!direction.Unitize()||value==0)return;var vector=direction*(value*options.LoadScale);marks.Add((new Line(at-vector,at),LoadColor,true));}
    private void Moment(Point3d at,Vector3d axis,double value,DisplayOptions options){if(value==0)return;var plane=new Plane(at,axis);double r=options.SymbolScale*.6;Point3d prior=plane.PointAt(r,0);for(int i=1;i<=10;i++){double a=Math.Sign(value)*i*Math.PI*1.65/10;var next=plane.PointAt(r*Math.Cos(a),r*Math.Sin(a));marks.Add((new Line(prior,next),LoadColor,i==10));prior=next;}}
    private void Ring(Point3d at,Vector3d axis,double radius,Color color){var p=new Plane(at,axis);for(int i=0;i<12;i++)Line(p.PointAt(radius*Math.Cos(i*Math.PI/6),radius*Math.Sin(i*Math.PI/6)),p.PointAt(radius*Math.Cos((i+1)*Math.PI/6),radius*Math.Sin((i+1)*Math.PI/6)),color);}
    private void Zigzag(Point3d start,Point3d end,double width,Color color){var axis=end-start;var side=Vector3d.CrossProduct(axis,Vector3d.XAxis);if(side.Length<1e-9)side=Vector3d.YAxis;side.Unitize();Point3d prior=start;for(int i=1;i<=8;i++){var next=start+axis*(i/8d)+(i==8?Vector3d.Zero:side*((i%2==0?-1:1)*width));Line(prior,next,color);prior=next;}}
    private void Label(Point3d at,string text,Color color,DisplayOptions o){if(o.Values)labels.Add((at,text,color));}
    private void Badge(Point3d at,string text,Color color,DisplayOptions o){double s=o.SymbolScale*.25;Line(at-new Vector3d(s,0,0),at+new Vector3d(0,0,s),color);Line(at+new Vector3d(0,0,s),at+new Vector3d(s,0,0),color);Line(at+new Vector3d(s,0,0),at-new Vector3d(0,0,s),color);Line(at-new Vector3d(0,0,s),at-new Vector3d(s,0,0),color);Label(at,text,color,o);}
    public void DrawWires(DisplayPipeline display,DisplayOptions options,Color color)
    {
        Prepare(options);if(!options.Enabled)return;
        if(options.Geometry&&!sectionOnly)foreach(var e in model.Definition.Elements){if(e.Points.Count==1)display.DrawPoint(Preview.Point(e.Points[0]),PointStyle.RoundSimple,3,color);else if(e.Points.Count==2)display.DrawLine(Preview.Point(e.Points[0]),Preview.Point(e.Points[1]),color);else display.DrawMeshWires(Preview.Mesh(e),color);}
        if(options.Sections)foreach(var g in physical){if(g is Brep b)display.DrawBrepWires(b,color);else if(g is Mesh m)display.DrawMeshWires(m,color);}
        foreach(var mark in marks){if(mark.Arrow)display.DrawArrow(mark.Line,mark.Color);else display.DrawLine(mark.Line,mark.Color,2);}
        foreach(var dot in dots)display.DrawPoint(dot.Point,PointStyle.RoundSimple,dot.Size,dot.Color);
        foreach(var label in labels)display.Draw2dText(label.Text,label.Color,label.Point,false,11);
    }
    public void DrawMeshes(DisplayPipeline display,DisplayOptions options,DisplayMaterial material)
    {Prepare(options);if(!options.Enabled)return;if(options.Sections)foreach(var g in physical){if(g is Brep b)display.DrawBrepShaded(b,material);else if(g is Mesh m)display.DrawMeshShaded(m,material);}else if(options.Geometry&&!sectionOnly)foreach(var e in model.Definition.Elements.Where(e=>e.Kind is ElementKind.Area or ElementKind.Solid))display.DrawMeshShaded(Preview.Mesh(e),material);}
    public (int Lines,int Arrows,int Labels,int Markers) Diagnostics(DisplayOptions options){Prepare(options);return(marks.Count(m=>!m.Arrow),marks.Count(m=>m.Arrow),labels.Count,dots.Count);}
}
