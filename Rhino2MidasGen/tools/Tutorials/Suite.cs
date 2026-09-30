using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;

internal sealed record Structure(GH_Component Model,GH_Component Elements,GH_Component Material,GH_Component Property,GH_Component BaseNodes,GH_Component TipNodes,GH_Component? LoadCase,int Nodes,int Count,string Port);
internal static partial class Suite
{
    static readonly object[] Fix=[true,true,true,true,true,true];
    static readonly object[] Free=[false,false,false,false,false,false];
    static readonly object[] Zero=[0d,0d,0d,0d,0d,0d];
    internal static void Generate(string? filter)
    {
        foreach(string project in Host.Projects)
        {
            Make(project,"01_Frame_polilinea","Frame, materiali e sezioni","Una polilinea genera tre elementi di un portale 4 x 3 m. Materiale e sezione si propagano agli elementi. Cambia b e h e controlla nodi, elementi e solidi.",g=>{var s=Frame(g,true,false);g.Review(s.Model,s.Nodes,s.Port,s.Count);},filter);
            Make(project,"02_Mesh_quad","Mesh quadrangolare e pressione","Piastra 4 x 3 m: 9 nodi, 4 facce Quad4, spessore 0.20 m. Pressione -5 kN/m2 in Z globale e bordo incastrato. La discretizzazione serve a illustrare il flusso.",g=>{var s=Plate(g,false);g.Review(s.Model,s.Nodes,s.Port,s.Count);},filter);
            Make(project,"03_Mesh_tri","Mesh triangolare","La stessa piastra usa 8 Tri3 e conserva 9 nodi. Confronta questo file con 02: la scelta della mesh cambia gli elementi, non l'area e lo spessore.",g=>{var s=Plate(g,true);g.Review(s.Model,s.Nodes,s.Port,s.Count);},filter);
            Make(project,"04_Solidi","Solidi Hexa8","Un cubo di lato 1 m, otto vertici ordinati, materiale elastico e base incastrata. Il Brep fisico deve essere chiuso. La connettivita conta: non riordinare casualmente i vertici.",g=>{var s=Solid(g);g.Review(s.Model,s.Nodes,s.Port,s.Count);},filter);
            Make(project,"05_Link_elastico","Link e rigidezze","Due nodi distinti collegati da un link elastico. Il primo e vincolato, il secondo e libero. Le prime tre rigidezze sono kN/m, le ultime tre kN m/rad.",g=>Link(g),filter);
        }
        Extra(filter);
        Guides();
    }
    static void Make(string project,string name,string title,string description,Action<Tutorial> recipe,string? filter)
    {
        if(filter!=null&&!(project+"/"+name).Contains(filter,StringComparison.OrdinalIgnoreCase))return;
        using var g=new Tutorial(project,name,title,description);
        g.Topics.Add(title);g.Note("COME USARE",description,0,170);recipe(g);g.Save();
    }
    static GH_Component Material(Tutorial g,bool concrete=false)
    {
        var m=g.C("MaterialComponent",1);
        g.Set(m,"Name",concrete?"C30_DEMO":"S355_DEMO");
        if(g.Straus){g.Set(m,"Modulus elasticity",concrete?30e6:210e6);g.Set(m,"Poisson ratio",concrete?.2:.3);g.Set(m,"Density",concrete?2.55:7.849);}
        else {g.Set(m,"E",concrete?30e6:210e6);g.Set(m,"Poisson",concrete?.2:.3);g.Set(m,"Weight density",concrete?25d:78.5);g.Set(m,"Type",g.Sap?(object)(concrete?2:1):(concrete?"CONC":"STEEL"));}
        return m;
    }
    static GH_Component Nodes(Tutorial g,string name,Point3d[] points,int first=1,int col=1)
    {
        var c=g.C("NodeElementComponent",col);
        g.Set(c,g.Straus?"Node point":"Point",points.Cast<object>().ToArray());
        g.Set(c,g.Straus?"Node ID":g.Sap?"Name":"ID",Enumerable.Range(first,points.Length).Select(i=>g.Sap?(object)(name+i):i).ToArray());
        return c;
    }
    static GH_Component Support(Tutorial g,GH_Component nodes)
    {
        var c=g.C(g.Straus?"NodeSupportComponent":g.Sap?"PointObj_SetRestraintComponent":"SupportComponent",2);
        g.Wire(nodes,c,g.Straus?"Node":g.Sap?"Name":"Nodes");
        if(g.Straus)foreach(string n in new[]{"Dx","Dy","Dz","Mx","My","Mz"})g.Set(c,n,true);
        else if(g.Sap)g.Set(c,"Value",Fix);
        return c;
    }
    static GH_Component Case(Tutorial g,string name="Q",int id=1)
    {
        var c=g.C(g.Sap?"LoadPatternComponent":"LoadCaseComponent",1);g.Set(c,"Name",name);
        if(g.Midas){g.Set(c,"ID",id);g.Set(c,"Type","L");}
        if(g.Sap){g.Set(c,"Type",3);g.Set(c,"Self weight",0d);}
        return c;
    }
    static GH_Component Model(Tutorial g,params (GH_Component Part,string StrausPort)[] parts)
    {
        var m=g.C("BuildModelComponent",3);
        foreach(var (part,port) in parts)g.Wire(part,m,g.Straus?port:"Definitions");
        return m;
    }
    static GH_Component LoadNode(Tutorial g,GH_Component node,GH_Component loadCase,double fz=-10)
    {
        var c=g.C(g.Straus?"LoadNodalComponent":g.Sap?"PointObj_SetLoadForceComponent":"NodalLoadComponent",2);
        g.Wire(node,c,g.Straus?"Node":g.Sap?"Name":"Nodes");
        g.Wire(loadCase,c,g.Straus?"Load case":g.Sap?"LoadPat":"Case",g.Midas?"Name":null);
        if(g.Straus)g.Set(c,"Fz",fz);
        else g.Set(c,g.Sap?"Value":"Loads",0d,0d,fz,0d,0d,0d);
        return c;
    }
    static GH_Component Rect(Tutorial g,GH_Component material)
    {
        var h=g.Slider("h [m]",.3,.1,.8);var b=g.Slider("b [m]",.2,.05,.6);
        var section=g.C(g.Straus?"FrameSectionRectangularComponent":g.Sap?"PropFrame_SetRectangleComponent":"RectangleSectionComponent",1);
        g.Set(section,"Name","R200x300");
        g.Wire(h,section,g.Sap?"T3":"Height");g.Wire(b,section,g.Sap?"T2":"Width");
        if(g.Sap)g.Wire(material,section,"MatProp");
        if(!g.Straus)return section;
        var property=g.C("FramePropertyComponent",1);g.Wire(material,property,"Material");g.Wire(section,property,"Section");g.Set(property,"Name","R200x300");return property;
    }
    static Structure Frame(Tutorial g,bool polyline=false,bool loaded=true,double shift=0,int firstId=1,GH_Component? sharedMaterial=null,GH_Component? sharedProperty=null)
    {
        var material=sharedMaterial??Material(g);var property=sharedProperty??Rect(g,material);
        Point3d[] points=polyline?[new(shift,0,0),new(shift,0,3),new(shift+4,0,3),new(shift+4,0,0)]:[new(shift,0,0),new(shift+3,0,0)];
        var geometry=g.Data<Param_Curve>(polyline?"Polilinea portale":"Asse trave",0,new PolylineCurve(points));
        var element=g.C(polyline?(g.Straus?"PolylineBeamsComponent":g.Sap?"PolylineFramesComponent":"PolylineToBeamsComponent"):(g.Straus?"FrameElementComponent":g.Sap?"FrameElementComponent":"BeamElementComponent"),2);
        if(polyline){g.Wire(geometry,element,"Polyline");g.Wire(property,element,g.Midas?"Section":"Property");if(g.Midas)g.Wire(material,element,"Material");if(!g.Sap)g.Set(element,"First ID",firstId);else g.Set(element,"Prefix","F"+firstId+"_");}
        else if(g.Midas){g.Set(element,"ID",firstId);g.Set(element,"Points",points.Cast<object>().ToArray());g.Wire(material,element,"Material");g.Wire(property,element,"Property");}
        else {
            g.Set(element,g.Straus?"Frame ID":"Name",g.Straus?(object)firstId:"F"+firstId);
            if(g.Straus)g.Wire(geometry,element,"Frame line");else g.Set(element,"Line",new Line(points[0],points[1]));
            g.Wire(property,element,g.Straus?"Frame Property":"Property");
        }
        var bottom=Nodes(g,"BASE",[points[0]],firstId*100);var tip=Nodes(g,"TIP",[points[^1]],firstId*100+1);
        var support=Support(g,bottom);
        var parts=new List<(GH_Component,string)>{(element,"Frame element"),(support,"Node"),(tip,"Node")};
        if(polyline)parts.Add((Support(g,tip),"Node"));
        GH_Component? lc=null;
        if(loaded){lc=Case(g);parts.RemoveAll(p=>p.Item1==tip);parts.Add((LoadNode(g,tip,lc),"Node"));if(!g.Straus)parts.Add((lc,""));}
        var model=Model(g,parts.ToArray());
        return new(model,element,material,property,bottom,tip,lc,points.Length,points.Length-1,g.Straus?"Frame elements":g.Sap?"Frame":"Beam");
    }
    static Mesh Grid(bool triangles)
    {
        var m=new Mesh();for(int j=0;j<3;j++)for(int i=0;i<3;i++)m.Vertices.Add(i*2,j*1.5,0);
        for(int j=0;j<2;j++)for(int i=0;i<2;i++){int a=j*3+i;if(triangles){m.Faces.AddFace(a,a+1,a+4);m.Faces.AddFace(a,a+4,a+3);}else m.Faces.AddFace(a,a+1,a+4,a+3);}
        m.Normals.ComputeNormals();return m;
    }
    static Structure Plate(Tutorial g,bool triangles=false)
    {
        var material=Material(g,true);var thick=g.Slider("Spessore t [m]",.2,.05,.5);
        var section=g.C(g.Straus?"AreaThicknessComponent":g.Sap?"PropArea_SetShell_1Component":"ThicknessComponent",1);
        if(!g.Midas)g.Set(section,"Name","Shell20");g.Wire(thick,section,"Thickness");
        GH_Component property=section;
        if(g.Straus){property=g.C("AreaPropertyComponent",1);g.Wire(section,property,"Section");g.Wire(material,property,"Material");g.Set(property,"Type",0);}
        else if(g.Sap){g.Wire(material,section,"MatProp");g.Set(section,"ShellType",1);g.Set(section,"IncludeDrillingDOF",true);g.Set(section,"MatAng",0d);g.Wire(thick,section,"Bending");}
        var mesh=Grid(triangles);var input=g.Data<Param_Mesh>("Mesh incorporata",0,mesh);
        var plates=g.C(g.Straus?"MeshPlatesComponent":g.Sap?"MeshAreasComponent":"MeshToPlatesComponent",2);
        g.Wire(input,plates,"Mesh");g.Wire(property,plates,g.Midas?"Thickness":"Property");if(g.Midas)g.Wire(material,plates,"Material");
        var boundary=mesh.Vertices.ToPoint3dArray().Where(p=>p.X==0||p.X==4||p.Y==0||p.Y==3).ToArray();
        var nodes=Nodes(g,"B",boundary,100);var support=Support(g,nodes);var lc=Case(g);
        var pressure=g.C(g.Straus?"GlobalPressureComponent":g.Sap?"AreaObj_SetLoadUniformComponent":"PlatePressureComponent",2);
        g.Wire(plates,pressure,g.Straus?"Plate":g.Sap?"Definitions":"Plate");g.Wire(lc,pressure,g.Straus?"Load case":g.Sap?"LoadPat":"Case",g.Midas?"Name":null);
        if(g.Straus)g.Set(pressure,"Pressure",new Vector3d(0,0,-5));
        else if(g.Sap){g.Set(pressure,"Name","ALL");g.Set(pressure,"ItemType",1);g.Set(pressure,"Value",-5d);g.Set(pressure,"Dir",6);g.Set(pressure,"CSys","Global");}
        else{g.Set(pressure,"Pressure",-5d);g.Set(pressure,"Direction","GZ");}
        var parts=new List<(GH_Component,string)>{(pressure,"Area element"),(support,"Node")};if(!g.Straus)parts.Add((lc,""));
        var model=Model(g,parts.ToArray());
        return new(model,plates,material,property,nodes,nodes,lc,9,triangles?8:4,g.Straus?"Area elements":g.Sap?"Area":"Plate");
    }
    static Structure Solid(Tutorial g)
    {
        Point3d[] points=[new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0),new(0,0,1),new(1,0,1),new(1,1,1),new(0,1,1)];
        var material=Material(g,true);var nodes=Nodes(g,"N",points,1);var baseNodes=Nodes(g,"N",points.Take(4).ToArray(),1);var support=Support(g,baseNodes);
        var solid=g.C("SolidElementComponent",2);GH_Component prop=material;
        if(g.Straus){g.Wire(nodes,solid,"Nodes");g.Wire(material,solid,"Material");g.Set(solid,"ID",1);}
        else if(g.Sap){
            prop=g.C("PropSolid_SetPropComponent",1);g.Set(prop,"Name","SOLID");g.Wire(material,prop,"MatProp");
            foreach(var n in new[]{"A","B","C"})g.Set(prop,n,0d);g.Set(prop,"Incompatible",true);
            g.Set(solid,"Name","S1");g.Set(solid,"Vertices",points.Cast<object>().ToArray());g.Wire(prop,solid,"Property");
        }else{g.Set(solid,"ID",1);g.Set(solid,"Points",points.Cast<object>().ToArray());g.Wire(nodes,solid,"Nodes");g.Wire(material,solid,"Material");}
        var model=Model(g,(solid,"Solids"),(support,"Node"));
        return new(model,solid,material,prop,baseNodes,nodes,null,8,1,g.Straus?"Solids":g.Sap?"Solid":"Solid");
    }
    static void Link(Tutorial g)
    {
        var a=Nodes(g,"BASE",[new(0,0,0)],1);var b=Nodes(g,"TIP",[new(0,0,1)],2);var fix=Support(g,a);
        GH_Component link;
        var stiffness=g.Note("K: TX TY TZ RX RY RZ","100000\n100000\n100000\n1000\n1000\n1000",0,160);stiffness.Properties.Multiline=false;
        if(g.Midas){link=g.C("ElasticLinkComponent",2);g.Wire(a,link,"Nodes");g.Wire(b,link,"Nodes");g.Wire(stiffness,link,"Stiffness");}
        else {
            var property=g.C(g.Straus?"ElasticLinkPropertyComponent":"PropLink_SetLinearComponent",1);
            if(g.Straus){foreach(var n in new[]{"Kx","Ky","Kz"})g.Set(property,n,100000d);foreach(var n in new[]{"Rx","Ry","Rz"})g.Set(property,n,1000d);}
            else{g.Set(property,"Name","ELASTIC");g.Set(property,"DOF",Fix);g.Set(property,"Fixed",Free);g.Wire(stiffness,property,"Ke");g.Set(property,"Ce",Zero);g.Set(property,"DJ2",0d);g.Set(property,"DJ3",0d);}
            link=g.C("LinkElementComponent",2);
            if(g.Straus){g.Set(link,"Start Point",new Point3d(0,0,0));g.Set(link,"End Point",new Point3d(0,0,1));g.Wire(property,link,"Link Property");}
            else{g.Set(link,"Name","L1");g.Set(link,"Line",new Line(0,0,0,0,0,1));g.Wire(property,link,"Property");}
        }
        var model=Model(g,(link,"Link element"),(fix,"Node"),(b,"Node"));
        g.Review(model,2,g.Straus?"Link elements":g.Sap?"Link":"ElasticLink",1,physical:false);
        g.Note("LETTURA","Il link e rappresentato dall'asse. Non equivale a una trave con volume fisico. Per un collegamento rigido usare il tipo dedicato o l'opzione Rigid in MIDAS.",0);
    }
    static partial void Extra(string? filter);
    static partial void Guides();
}