using System.Drawing;
using System.Text.Json;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

internal sealed class Tutorial : IDisposable
{
    internal readonly string Project, Name, Title, Description;
    internal bool Straus => Project == "Rhino2Straus";
    internal bool Sap => Project == "Rhino2SAP";
    internal bool Midas => !Straus && !Sap;
    internal readonly GH_Document Doc = new();
    readonly Dictionary<int,float> next = [];
    readonly Dictionary<int,List<IGH_DocumentObject>> stages = [];
    readonly HashSet<Guid> dormant = [];
    readonly List<(Guid Id,int? Nodes,string? Port,int? Elements)> checks = [];
    readonly List<(Guid Id,int Count)> physicalChecks = [];
    internal readonly List<string> Topics = [];
    internal readonly List<string> Instructions = [];
    internal Tutorial(string project,string name,string title,string description)
    {
        Project=project;Name=name;Title=title;Description=description;
        Doc.Properties.ProjectFileName=name+".gh";Doc.Properties.Description=description;
        Doc.Properties.ZoomFactor=.6f;Doc.Properties.ViewTarget=new System.Drawing.Point(0,0);
    }
    internal T Add<T>(T obj,int column,float height=100) where T:IGH_DocumentObject
    {
        float y=next.GetValueOrDefault(column,230),x=150+column*560;
        obj.CreateAttributes();obj.Attributes.Pivot=new PointF(x,y);Doc.AddObject(obj,false);
        next[column]=y+height+65;
        if(!stages.TryGetValue(column,out var list))stages[column]=list=[];
        list.Add(obj);return obj;
    }
    internal GH_Component C(string type,int column=1,bool inactive=false)
    {
        var t=Host.Assemblies[Project].GetTypes().Single(t=>t.Name==type&&!t.IsAbstract);
        var c=(GH_Component)Activator.CreateInstance(t)!;c.Hidden=true;
        if(type=="BuildModelComponent")foreach(var input in c.Params.Input.Where(p=>p.Access==GH_ParamAccess.list))input.DataMapping=GH_DataMapping.Flatten;
        Add(c,column,60+20*Math.Max(c.Params.Input.Count,c.Params.Output.Count));
        if(inactive)dormant.Add(c.InstanceGuid);return c;
    }
    internal static IGH_Param Port(IEnumerable<IGH_Param> ports,string name)=>ports.SingleOrDefault(p=>p.Name==name)??throw new Exception("Port missing: "+name+"; available: "+string.Join(", ",ports.Select(p=>p.Name)));
    internal void Wire(IGH_DocumentObject source,GH_Component target,string input,string? output=null)
    {
        IGH_Param p=source is GH_Component c?(output==null?c.Params.Output[0]:Port(c.Params.Output,output)):(IGH_Param)source;
        Port(target.Params.Input,input).AddSource(p);
    }
    internal void Wire(IGH_DocumentObject source,IGH_Param target,string? output=null)
    {
        target.AddSource(source is GH_Component c?(output==null?c.Params.Output[0]:Port(c.Params.Output,output)):(IGH_Param)source);
    }
    internal void Set(GH_Component c,string input,params object[] values)=>Set(Port(c.Params.Input,input),values);
    internal static void Set(IGH_Param p,params object[] values)
    {
        switch(p)
        {
            case Param_Number n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Number(Convert.ToDouble(v)));break;
            case Param_Integer n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Integer(Convert.ToInt32(v)));break;
            case Param_String n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_String(Convert.ToString(v)!));break;
            case Param_Boolean n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Boolean((bool)v));break;
            case Param_Point n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Point((Point3d)v));break;
            case Param_Vector n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Vector((Vector3d)v));break;
            case Param_Line n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Line((Line)v));break;
            case Param_Curve n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Curve((Curve)v));break;
            case Param_Mesh n:n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(new GH_Mesh((Mesh)v));break;
            case Param_GenericObject n:
                n.PersistentData.Clear();foreach(var v in values)n.PersistentData.Append(v switch{
                    string s=>new GH_String(s),int i=>new GH_Integer(i),double d=>new GH_Number(d),bool b=>new GH_Boolean(b),
                    _=>throw new ArgumentException("Generic value unsupported: "+v.GetType())});break;
            default:throw new ArgumentException("Persistent input unsupported: "+p.GetType());
        }
    }
    internal T Data<T>(string name,int column,params object[] values)where T:IGH_Param,new()
    {var p=Add(new T{NickName=name},column,50);Set(p,values);return p;}
    internal GH_NumberSlider Slider(string name,double value,double min,double max,int col=0)
    {
        var s=Add(new GH_NumberSlider{NickName=name},col,55);s.Slider.Minimum=(decimal)min;s.Slider.Maximum=(decimal)max;
        s.Slider.DecimalPlaces=3;s.SetSliderValue((decimal)value);s.Attributes.Bounds=new RectangleF(s.Attributes.Pivot,new SizeF(330,30));return s;
    }
    internal GH_BooleanToggle Toggle(string name,int col=0,bool value=false)=>Add(new GH_BooleanToggle{NickName=name,Value=value},col,50);
    internal GH_Panel Note(string name,string text,int col=0,int height=150)
    {
        var p=Add(new GH_Panel{NickName=name,UserText=text},col,height);
        p.Properties.Multiline=true;p.Attributes.Bounds=new RectangleF(p.Attributes.Pivot.X-100,p.Attributes.Pivot.Y-25,445,height);return p;
    }
    internal GH_Panel Show(IGH_DocumentObject source,string label,int col=4,string? output=null)
    {var p=Note(label,"",col,115);Wire(source,p,output);return p;}
    internal GH_Component Gate(IGH_DocumentObject source,GH_BooleanToggle toggle,int col,string? output=null)
    {
        var gate=Add(new Grasshopper.Kernel.Components.GH_StreamGateComponent(),col,120);gate.Hidden=true;
        Wire(source,gate,"Stream",output);Wire(toggle,gate,"Gate");dormant.Add(gate.InstanceGuid);return gate;
    }
    internal GH_Component Review(GH_Component model,int? nodes=null,string? elementPort=null,int? elements=null,int col=4,bool inactive=false,bool physical=true,string? modelOutput=null)
    {
        var validate=C("ValidateModelComponent",col,inactive);Wire(model,validate,"Model",modelOutput);Show(validate,"VALIDO: dati e riferimenti",col);
        var summary=C("ModelSummaryComponent",col,inactive);Wire(model,summary,"Model",modelOutput);Show(summary,"RIEPILOGO",col);
        var preview=C("PreviewModelComponent",col,inactive);Wire(model,preview,"Model",modelOutput);preview.Hidden=false;
        var decompose=C("DecomposeModelComponent",col,inactive);Wire(model,decompose,"Model",modelOutput);
        if(!inactive)checks.Add((validate.InstanceGuid,nodes,elementPort,elements));
        if(physical)
        {
            var geometry=C(Straus?"ModelGeometryComponent":"PhysicalGeometryComponent",col,inactive);Wire(model,geometry,"Model",modelOutput);
            if(!inactive&&elements.HasValue)physicalChecks.Add((geometry.InstanceGuid,elements.Value));
        }
        return decompose;
    }
    void Layout()
    {
        int width=(stages.Keys.DefaultIfEmpty(4).Max()+1)*560;
        var header=new GH_Panel{NickName=Title,UserText=Project+" | "+Title+"\n"+Description+"\n"+(Name.StartsWith("11_")?"Unita: quelle restituite dal lettore; vedere Summary.":"Unita: kN / m / C. Geometrie incorporate. Le combinazioni sono didattiche.")};
        header.CreateAttributes();header.Attributes.Pivot=new PointF(10,10);header.Attributes.Bounds=new RectangleF(10,10,width,145);Doc.AddObject(header,false);
        string[] names=["INPUT E ISTRUZIONI","PROPRIETA / GEOMETRIA","ATTRIBUTI / CARICHI","MODEL / OPERAZIONI","VERIFICA / LETTURA","RISULTATI","APPROFONDIMENTI"];
        Color[] colors=[Color.Goldenrod,Color.SteelBlue,Color.DarkOrange,Color.SeaGreen,Color.MediumPurple,Color.Teal,Color.IndianRed];
        foreach(var (column,objects) in stages)
        {
            var group=new GH_Group{NickName=$"{column+1:00} | {names[Math.Min(column,names.Length-1)]}",Colour=Color.FromArgb(55,colors[column%colors.Length])};
            foreach(var obj in objects)group.AddObject(obj.InstanceGuid);Doc.AddObject(group,false);
        }
    }
    internal void Check(GH_Document doc)
    {
        doc.Enabled=true;doc.NewSolution(false,GH_SolutionMode.Silent);
        var all=doc.Objects.OfType<GH_Component>().ToArray();
        var inactive=new HashSet<Guid>(dormant);
        bool changed;do{changed=false;foreach(var c in all)if(c.Params.Input.SelectMany(p=>p.Sources).Any(s=>inactive.Contains(s.Attributes.GetTopLevel.DocObject.InstanceGuid)))changed|=inactive.Add(c.InstanceGuid);}while(changed);
        foreach(var c in all)
        {
            var errors=c.RuntimeMessages(GH_RuntimeMessageLevel.Error);if(errors.Count>0)throw new Exception(Name+" / "+c.GetType().Name+": "+string.Join("; ",errors));
            if(!inactive.Contains(c.InstanceGuid)&&c.Params.Output.Sum(p=>p.VolatileDataCount)==0)throw new Exception(Name+" / "+c.Name+": no output");
        }
        foreach(var toggle in doc.Objects.OfType<GH_BooleanToggle>().Where(t=>new[]{"RUN","READ","BAKE","REPLACE","OVERWRITE"}.Any(s=>t.NickName.StartsWith(s))))
            if(toggle.Value)throw new Exception("External action enabled: "+toggle.NickName);
        foreach(var expected in checks)
        {
            var validation=all.Single(c=>c.InstanceGuid==expected.Id);
            var flags=validation.Params.Output[0].VolatileData.AllData(true).OfType<GH_Boolean>().ToArray();
            if(flags.Length!=1||!flags[0].Value)throw new Exception(Name+": invalid Model: "+string.Join("; ",validation.Params.Output.Skip(1).SelectMany(p=>p.VolatileData.AllData(true)).Select(x=>x.ToString())));
            var model=validation.Params.Input[0].Sources.Single();
            var d=all.First(c=>c.GetType().Name=="DecomposeModelComponent"&&c.Params.Input[0].Sources.Contains(model));
            if(expected.Nodes.HasValue&&d.Params.Output[0].VolatileDataCount!=expected.Nodes)throw new Exception(Name+": wrong nodes "+d.Params.Output[0].VolatileDataCount+" expected "+expected.Nodes);
            if(expected.Port!=null&&Port(d.Params.Output,expected.Port).VolatileDataCount!=expected.Elements)throw new Exception(Name+": wrong element count in "+expected.Port);
        }
        foreach(var expected in physicalChecks)
        {
            var c=all.Single(c=>c.InstanceGuid==expected.Id);
            var port=c.Params.Output.LastOrDefault(p=>p is Param_Brep)??c.Params.Output[0];
            var breps=port.VolatileData.AllData(true).Select(g=>g.ScriptVariable()).OfType<Brep>().ToArray();
            if(breps.Length!=expected.Count||breps.Any(b=>!b.IsValid||!b.IsSolid))throw new Exception(Name+": invalid physical Breps: "+breps.Length+" expected "+expected.Count);
        }
    }
    string[] ModelFingerprints(GH_Document doc)
    {
        return checks.Select(expected=>
        {
            var validation=doc.Objects.OfType<GH_Component>().Single(c=>c.InstanceGuid==expected.Id);
            var value=validation.Params.Input[0].VolatileData.AllData(true).Single().ScriptVariable();
            if(!Straus)return (string)value.GetType().GetProperty("Fingerprint")!.GetValue(value)!;
            var helper=value.GetType().Assembly.GetTypes().Single(t=>t.Name=="ModelFingerprint");
            return (string)helper.GetMethod("Compute")!.Invoke(null,[value])!;
        }).ToArray();
    }

    bool CheckParametric()
    {
        var slider=Doc.Objects.OfType<GH_NumberSlider>().FirstOrDefault();
        if(slider==null||physicalChecks.Count==0)return false;
        double Volume()
        {
            double total=0;
            foreach(var item in physicalChecks)
            {
                var c=Doc.Objects.OfType<GH_Component>().Single(c=>c.InstanceGuid==item.Id);
                var p=c.Params.Output.LastOrDefault(p=>p is Param_Brep)??c.Params.Output[0];
                foreach(var b in p.VolatileData.AllData(true).Select(g=>g.ScriptVariable()).OfType<Brep>())
                    using(var v=VolumeMassProperties.Compute(b))total+=v?.Volume??0;
            }
            return total;
        }
        decimal original=slider.CurrentValue;
        double before=Volume();
        try
        {
            decimal target=Math.Min(slider.Slider.Maximum,original*1.1m);
            if(target==original)throw new Exception("No room for slider regression");
            slider.SetSliderValue(target);slider.ExpireSolution(false);Check(Doc);
            if(Math.Abs(Volume()-before)<1e-8)throw new Exception(Name+": slider does not change physical geometry");
        }
        finally{slider.SetSliderValue(original);slider.ExpireSolution(false);Check(Doc);}
        return true;
    }

    static int Wires(GH_Document d)=>d.Objects.OfType<GH_Component>().Sum(c=>c.Params.Input.Sum(p=>p.SourceCount))+d.Objects.OfType<IGH_Param>().Sum(p=>p.SourceCount);
    internal void Save()
    {
        Layout();Check(Doc);bool parametric=CheckParametric();var fingerprints=ModelFingerprints(Doc);
        string dir=Path.Combine(Host.Root,Project,"examples","tutorials");Directory.CreateDirectory(dir);
        foreach(string ext in new[]{".gh",".ghx"})
        {
            string path=Path.Combine(dir,Name+ext);
            if(!new GH_DocumentIO(Doc).SaveQuiet(path))throw new Exception("Save failed: "+path);
            var io=new GH_DocumentIO();if(!io.Open(path))throw new Exception("Reload failed: "+path);
            using var copy=io.Document;
            if(copy.ObjectCount!=Doc.ObjectCount||Wires(copy)!=Wires(Doc))throw new Exception("Roundtrip lost objects or wires: "+Name);
            Check(copy);
            if(!ModelFingerprints(copy).SequenceEqual(fingerprints))throw new Exception(Name+": Model fingerprint changed after reopening");
        }
        var components=Doc.Objects.OfType<GH_Component>().ToArray();
        if(Name=="14_Rilettura_risultati")
        {
            string previous=Path.Combine(dir,"12_Analisi_frame.checks.json");
            if(!File.Exists(previous))throw new Exception("Generate tutorial 12 before 14");
            using var reference=JsonDocument.Parse(File.ReadAllText(previous));
            var original=reference.RootElement.GetProperty("Fingerprints").EnumerateArray().Select(v=>v.GetString()).ToArray();
            if(!original.SequenceEqual(fingerprints))throw new Exception(Project+": result reread model differs from analysis tutorial 12");
        }
        var report=new {
            Project,File=Name+".gh",Title,Description,Topics,Instructions,Fingerprints=fingerprints,
            PluginComponents=components.Where(c=>c.GetType().Assembly==Host.Assemblies[Project]).Select(c=>new{Class=c.GetType().Name,c.Name,c.ComponentGuid}).ToArray(),
            Objects=Doc.ObjectCount,Wires=Wires(Doc),ModelChecks=checks.Count,PhysicalChecks=physicalChecks.Count,
            ReopenedGh=true,ReopenedGhx=true,SliderVolumeChangeChecked=parametric,SolverExecuted=false,ExternalFilesWritten=false,
            Messages=components.SelectMany(c=>c.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Select(w=>new{Component=c.Name,Warning=w})).ToArray(),
            Connections=components.SelectMany(c=>c.Params.Input.SelectMany(p=>p.Sources.Select(s=>new{From=s.Attributes.GetTopLevel.DocObject.NickName,Output=s.Name,To=c.Name,Input=p.Name}))).ToArray()
        };
        File.WriteAllText(Path.Combine(dir,Name+".checks.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"PASS {Project}/{Name}: {components.Length} components, {Wires(Doc)} wires; GH/GHX reopened.");
    }
    public void Dispose()=>Doc.Dispose();
}