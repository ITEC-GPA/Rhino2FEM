using System.Reflection;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

internal sealed class GraphBuilder(Assembly assembly,string title)
{
    public string Title { get; }=title;
    public GH_Document Document { get; }=new();
    public T Place<T>(T obj,int x,int y) where T:IGH_DocumentObject
    {
        obj.CreateAttributes();obj.Attributes.Pivot=new PointF(x,y);Document.AddObject(obj,false);
        if(obj is IGH_PreviewObject preview)preview.Hidden=true;
        return obj;
    }
    public GH_Component Component(string name,int x,int y,string? nickname=null)
    {
        var c=Place((GH_Component)Activator.CreateInstance(assembly.GetType("Rhino2SAP.Grasshopper."+name,true)!)!,x,y);
        if(nickname!=null)c.NickName=nickname;
        c.IconDisplayMode=GH_IconDisplayMode.name;
        if(name=="PreviewModelComponent")c.Hidden=false;
        return c;
    }
    public GH_Panel Note(string title,string text,int x,int y,int width=360,int height=125)
    {
        var panel=Place(new GH_Panel{NickName=title,UserText=text},x,y);panel.Properties.Font=new Font("Segoe UI",10);panel.Properties.DrawPaths=false;panel.Properties.DrawIndices=false;panel.Attributes.Bounds=new RectangleF(x,y,width,height);return panel;
    }
    public GH_NumberSlider Slider(string name,decimal value,decimal minimum,decimal maximum,int x,int y)
    {
        var slider=Place(new GH_NumberSlider{NickName=name},x,y);slider.Slider.Minimum=minimum;slider.Slider.Maximum=maximum;slider.Slider.DecimalPlaces=3;slider.SetSliderValue(value);slider.Attributes.Bounds=new RectangleF(x,y,235,22);return slider;
    }
    public GH_BooleanToggle Toggle(string name,bool value,int x,int y)=>Place(new GH_BooleanToggle{NickName=name,Value=value},x,y);
    public void Group(string name,Color colour,params IGH_DocumentObject[] objects)
    {
        var group=Place(new GH_Group{NickName=name,Colour=Color.FromArgb(40,colour),Border=GH_GroupBorder.Box},0,0);
        foreach(var obj in objects)group.AddObject(obj.InstanceGuid);
    }
    public void Wire(GH_Component from,int output,GH_Component to,string input)=>Input(to,input).AddSource(from.Params.Output[output]);
    public void Wire(IGH_Param from,GH_Component to,string input)=>Input(to,input).AddSource(from);
    public void Values(GH_Component component,string input,params object[] values)=>Set(Input(component,input),values);
    public static IGH_Param Input(GH_Component component,string name)=>component.Params.Input.Single(p=>p.Name==name);
    public T Parameter<T>(string name,int x,int y,params object[] values) where T:IGH_Param,new()
    {var param=Place(new T{NickName=name},x,y);Set(param,values);return param;}
    public static void Set(IGH_Param param,params object[] values)
    {
        switch(param)
        {
            case Param_String p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_String((string)v));break;
            case Param_Number p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Number(Convert.ToDouble(v)));break;
            case Param_Integer p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Integer(Convert.ToInt32(v)));break;
            case Param_Boolean p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Boolean((bool)v));break;
            case Param_Point p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Point((Point3d)v));break;
            case Param_Line p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Line((Line)v));break;
            case Param_Curve p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Curve((Curve)v));break;
            case Param_Vector p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Vector((Vector3d)v));break;
            case Param_Plane p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Plane((Plane)v));break;
            case Param_Mesh p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(new GH_Mesh((Mesh)v));break;
            case Param_GenericObject p:p.PersistentData.Clear();foreach(var v in values)p.PersistentData.Append(v switch{string s=>new GH_String(s),double d=>new GH_Number(d),_=>new GH_ObjectWrapper(v)});break;
            default:throw new ArgumentException("Unsupported example parameter "+param.GetType().Name);
        }
    }
    public void CommonOutputs(GH_Component model,int x,int y)
    {
        var settings=Component("DisplaySettingsComponent",x,y+400,"Display");Values(settings,"Labels",true);Values(settings,"Axes",true);
        var show=Toggle("Mostra carichi",true,x-300,y+390);Wire(show,settings,"Loads");
        var preview=Component("PreviewModelComponent",x+380,y+190,"Preview / Brep");Wire(model,0,preview,"Model");Wire(settings,0,preview,"Settings");
        var breps=Place(new Param_Brep{NickName="Solidi Brep"},x+730,y+155);breps.AddSource(preview.Params.Output[2]);
        var validation=Component("ValidateModelComponent",x,y+80,"Controllo modello");Wire(model,0,validation,"Model");
        var valid=Note("Modello valido",string.Empty,x+320,y-20,290,100);valid.AddSource(validation.Params.Output[0]);
        var issues=Note("Preview: limiti / Issues",string.Empty,x+740,y+270,310,145);issues.AddSource(preview.Params.Output[1]);
        var summary=Component("ModelSummaryComponent",x+380,y+360,"Riepilogo");Wire(model,0,summary,"Model");
        var info=Note("Riepilogo modello",string.Empty,x+320,y+420,350,165);info.AddSource(summary.Params.Output[0]);
        info.Properties.Font=new Font("Consolas",8);info.Properties.Wrap=false;
        var bake=Component("BakeGeometryComponent",x+380,y+710,"Bake solidi");Wire(model,0,bake,"Model");
        var bakeToggle=Toggle("BAKE SOLIDI",false,x,y+710);Wire(bakeToggle,bake,"Bake");
        var help=Note("Preview e bake","B = Brep chiusi, N = nomi SAP.\nBrep: tasto destro > Bake.\nOppure BAKE SOLIDI: False > True.\nLa geometria non richiede SAP.\nModificare gli switch nel componente Display.",x+740,y+485,350,240);
        Group("04  CONTROLLO / PREVIEW / BREP / BAKE",Color.MediumSeaGreen,settings,show,preview,breps,validation,valid,issues,summary,info,bake,bakeToggle,help);
    }
}
