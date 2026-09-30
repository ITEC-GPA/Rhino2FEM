using System.Reflection;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

internal sealed record Example(string Name,GraphBuilder Graph,int ExpectedBreps,
    string Description = "", bool RequiresModel = true, string Mode = "Modellazione locale");
internal static class Examples
{
    public static IEnumerable<Example> Create(Assembly assembly)
    {
        var beam=Beam(assembly,false);yield return new("01_Trave_carico_preview_bake",beam.Graph,1,"Trave a mensola con carico nodale, slider di sezione, preview e bake solido.");
        yield return Plate(assembly);
        var analysis=Beam(assembly,true);AddAnalysis(analysis.Graph,analysis.Model,analysis.Pattern);yield return new("03_Analisi_assiale_risultati",analysis.Graph,1,"Analisi assiale, combinazione e confronto con lo spostamento teorico FL/EA.",Mode:"Analisi SAP su richiesta");
        foreach(var example in TopicExamples.Create(assembly))yield return example;
    }
    static (GraphBuilder Graph,GH_Component Model,GH_Component Pattern) Beam(Assembly assembly,bool axial)
    {
        var g=new GraphBuilder(assembly,axial?"03 Analisi assiale e risultati":"01 Trave: carico, preview e bake");
        g.Note("RHINO2SAP / "+(axial?"03  ANALISI E RISULTATI":"01  TRAVE / PREVIEW / BREP"),
            "Unita: kN, m, C. Trave a mensola lunga 3 m, sezione 0.20 x 0.10 m.\n"+
            (axial?"F = +1 kN lungo X. E = 210e6 kN/m2. Atteso Ux = FL/EA = 7.142857e-7 m.\nIl ramo di analisi e sotto: RUN SAP parte da False.":"F = -10 kN lungo Z; incastro al nodo BASE.\nProva a modificare altezza, larghezza o vettore del carico. Spegni Mostra carichi.\nLe coordinate di asse e nodi sono incorporate; se le cambi, mantienile coerenti."),70,20,1170,160);
        var material=g.Component("MaterialComponent",400,340,"Acciaio");g.Values(material,"Name","S355");g.Values(material,"Weight density",78.5);
        var h=g.Slider("Altezza h [m]",.20m,.05m,1m,120,590);var b=g.Slider("Larghezza b [m]",.10m,.03m,.8m,120,660);
        var section=g.Component("PropFrame_SetRectangleComponent",800,340,"Rettangolo");g.Values(section,"Name","R20x10");g.Wire(material,0,section,"MatProp");g.Wire(h,section,"T3");g.Wire(b,section,"T2");
        g.Group("01  MATERIALE / SEZIONE",Color.SteelBlue,material,h,b,section);
        var frame=g.Component("FrameElementComponent",800,760,"Beam F1");g.Values(frame,"Name","F1");g.Values(frame,"Line",new Line(0,0,0,3,0,0));g.Wire(section,0,frame,"Property");
        var start=g.Component("NodeElementComponent",400,950,"Nodo BASE");g.Values(start,"Name","BASE");g.Values(start,"Point",new Point3d(0,0,0));
        var end=g.Component("NodeElementComponent",400,1110,"Nodo TIP");g.Values(end,"Name","TIP");g.Values(end,"Point",new Point3d(3,0,0));
        var support=g.Component("PointObj_SetRestraintComponent",800,970,"Incastro");g.Wire(start,0,support,"Name");g.Values(support,"Value",true,true,true,true,true,true);
        g.Group("02  ELEMENTO / NODI / VINCOLO",Color.MediumPurple,frame,start,end,support);
        var pattern=g.Component("LoadPatternComponent",1270,330,"Pattern Q");g.Values(pattern,"Name","Q");g.Values(pattern,"Self weight",0d);
        var vector=g.Note("Carico: FX FY FZ MX MY MZ",axial?"1\n0\n0\n0\n0\n0":"0\n0\n-10\n0\n0\n0",1100,500,275,220);
        vector.Properties.Multiline=false;
        var force=g.Component("PointObj_SetLoadForceComponent",1270,880,"Forza al TIP");g.Wire(end,0,force,"Name");g.Wire(pattern,0,force,"LoadPat");g.Wire(vector,force,"Value");g.Values(force,"CSys","Global");g.Values(force,"Replace",false);
        g.Group("03  PATTERN / CARICO NODALE",Color.DarkOrange,pattern,vector,force);
        var model=g.Component("BuildModelComponent",1670,930,"Build Model");g.Wire(frame,0,model,"Definitions");g.Wire(support,0,model,"Definitions");g.Wire(force,0,model,"Definitions");
        g.CommonOutputs(model,1760,270);
        return(g,model,pattern);
    }
    static Example Plate(Assembly assembly)
    {
        var g=new GraphBuilder(assembly,"02 Piastra mesh e pressione");
        g.Note("RHINO2SAP / 02  PIASTRA DISCRETIZZATA","Unita: kN, m, C. Piastra 4 x 4 m; 4 shell quadrangolari, spessore 0.20 m.\n8 nodi di bordo incastrati; nodo centrale libero. Pressione -5 kN/m2 lungo Z globale.\nMesh e coordinate sono incorporate: non serve un file Rhino.\nMesh volutamente grossolana per illustrare i collegamenti; raffinarla prima di un uso progettuale.",70,20,1170,160);
        var material=g.Component("MaterialComponent",390,350,"Calcestruzzo");g.Values(material,"Name","C30_DEMO");g.Values(material,"Type",2);g.Values(material,"E",30e6);g.Values(material,"Poisson",.2);g.Values(material,"Weight density",25d);
        var thickness=g.Slider("Spessore t [m]",.2m,.05m,.5m,100,620);
        var shell=g.Component("PropArea_SetShell_1Component",820,360,"Shell sottile");g.Values(shell,"Name","SHELL20");g.Values(shell,"ShellType",1);g.Values(shell,"IncludeDrillingDOF",true);g.Values(shell,"MatAng",0d);g.Wire(material,0,shell,"MatProp");g.Wire(thickness,shell,"Thickness");g.Wire(thickness,shell,"Bending");
        g.Group("01  MATERIALE / PROPRIETA SHELL",Color.SteelBlue,material,thickness,shell);
        var mesh=new Mesh();for(int y=0;y<3;y++)for(int x=0;x<3;x++)mesh.Vertices.Add(x*2,y*2,0);for(int y=0;y<2;y++)for(int x=0;x<2;x++){int i=y*3+x;mesh.Faces.AddFace(i,i+1,i+4,i+3);}mesh.Normals.ComputeNormals();
        var meshInput=g.Parameter<Param_Mesh>("Mesh 2x2",390,790,mesh);
        var areas=g.Component("MeshAreasComponent",820,760,"4 plate A1-A4");g.Wire(meshInput,areas,"Mesh");g.Wire(shell,0,areas,"Property");
        var boundary=mesh.Vertices.Select(p=>new Point3d(p.X,p.Y,p.Z)).Where(p=>p.X==0||p.X==4||p.Y==0||p.Y==4).ToArray();
        var nodes=g.Component("NodeElementComponent",390,1050,"Nodi di bordo");g.Values(nodes,"Name",Enumerable.Range(1,boundary.Length).Select(i=>(object)("B"+i)).ToArray());g.Values(nodes,"Point",boundary.Cast<object>().ToArray());
        var fix=g.Component("PointObj_SetRestraintComponent",820,1040,"Incastri bordo");g.Wire(nodes,0,fix,"Name");g.Values(fix,"Value",true,true,true,true,true,true);
        g.Group("02  MESH / ELEMENTI / BORDO",Color.MediumPurple,meshInput,areas,nodes,fix);
        var pattern=g.Component("LoadPatternComponent",1270,350,"Pattern Q");g.Values(pattern,"Name","Q");
        var pressure=g.Slider("Pressione Z [kN/m2]",-5m,-20m,0m,1100,575);
        var loading=g.Component("AreaObj_SetLoadUniformComponent",1270,920,"Pressione sulle shell");g.Wire(areas,0,loading,"Definitions");g.Values(loading,"Name","ALL");g.Values(loading,"ItemType",1);g.Wire(pattern,0,loading,"LoadPat");g.Wire(pressure,loading,"Value");g.Values(loading,"Dir",6);g.Values(loading,"CSys","Global");
        var note=g.Note("Assegnazione a tutte le shell","Name=ALL, ItemType=1: gruppo ALL.\nDir=6: Z Global; Value<0: verso -Z.\nSelf weight=0: solo carico assegnato.",1100,650,310,150);
        g.Group("03  CARICO UNIFORME / GRUPPO",Color.DarkOrange,pattern,pressure,loading,note);
        var model=g.Component("BuildModelComponent",1660,940,"Build Model");g.Wire(loading,0,model,"Definitions");g.Wire(fix,0,model,"Definitions");g.CommonOutputs(model,1760,270);
        var decompose=g.Component("DecomposeModelComponent",1760,1260,"Decomponi modello");g.Wire(model,0,decompose,"Model");
        var info=g.Component("ElementInfoComponent",2200,1300,"Nomi delle plate");g.Wire(decompose,2,info,"Definition");
        var names=g.Note("Plate del modello",string.Empty,2530,1220,350,180);names.AddSource(info.Params.Output[0]);g.Group("05  DECOMPOSIZIONE / INTERROGAZIONE",Color.CadetBlue,decompose,info,names);
        return new("02_Piastra_mesh_pressione",g,4,"Piastra 2 x 2 shell, pressione uniforme e vincoli di bordo.");
    }
    static void AddAnalysis(GraphBuilder g,GH_Component model,GH_Component pattern)
    {
        g.Note("05  ESEGUIRE L'ANALISI","1. Scegli un nuovo percorso .sdb nel pannello Path.\n2. Porta RUN SAP da False a True. Per rilanciare, torna prima a False.\n3. I componenti dei risultati si popolano dopo il completamento dell'analisi.\nPrima del run restano in attesa: non contengono risultati simulati.\nSLS e ULS_DEMO sono nomi didattici; il fattore 1.5 non rappresenta una combinazione normativa.",60,1440,980,220);
        var staticCase=g.Component("LinearCaseComponent",440,1870,"Caso SLS");g.Values(staticCase,"Name","SLS");g.Values(staticCase,"Patterns","Q");g.Values(staticCase,"Factors",1d);g.Wire(pattern,0,staticCase,"Definitions");
        var combo=g.Component("CombinationComponent",870,1870,"ULS_DEMO = 1.5 SLS");g.Values(combo,"Name","ULS_DEMO");g.Values(combo,"Cases","SLS");g.Values(combo,"Factors",1.5);
        g.Wire(staticCase,0,model,"Definitions");g.Wire(combo,0,model,"Definitions");
        var path=g.Note("Path: scegliere un file .sdb",@"C:\Users\Public\Documents\Rhino2SAP_03_assiale.sdb",1100,1530,470,100);
        var run=g.Toggle("RUN SAP",false,1120,1810);
        var analyze=g.Component("AnalyzeAndEmbedComponent",1690,1840,"Analizza e incorpora");g.Wire(model,0,analyze,"Model");g.Wire(path,analyze,"Path");g.Wire(run,analyze,"Run");g.Values(analyze,"Cases","SLS");g.Values(analyze,"Combinations","ULS_DEMO");
        var log=g.Note("Issues analisi",string.Empty,1500,2070,370,160);log.AddSource(analyze.Params.Output[2]);
        g.Group("05  CASO / COMBINAZIONE / ANALISI SAP",Color.IndianRed,staticCase,combo,path,run,analyze,log);
        var beam=g.Component("BeamActionsComponent",2180,1840,"N V T M");g.Wire(analyze,3,beam,"Element");g.Values(beam,"Case","SLS");
        var n=g.Note("N [kN]: atteso +1 nel caso SLS",string.Empty,2580,1580,410,120);n.AddSource(beam.Params.Output[0]);
        var joint=g.Component("Results_JointDisplComponent",2180,2160,"Spostamento TIP");g.Wire(analyze,0,joint,"Model");g.Values(joint,"Object","TIP");g.Values(joint,"Case","SLS");
        var ux=g.Note("U1 [m]: atteso 7.142857e-7",string.Empty,2580,2000,410,130);ux.AddSource(joint.Params.Output.Single(p=>p.Name=="U1"));
        var query=g.Component("QueryElementResultsComponent",2180,2480,"Interroga elemento");g.Wire(analyze,3,query,"Element");g.Values(query,"Quantity","Results.FrameForce");g.Values(query,"Case","ULS_DEMO");
        var rows=g.Note("Righe native / ULS_DEMO",string.Empty,2580,2330,410,230);rows.AddSource(query.Params.Output[2]);
        var diagram=g.Component("FrameDiagramComponent",2180,2780,"Diagramma N");g.Wire(analyze,0,diagram,"Model");g.Values(diagram,"Frame","F1");g.Values(diagram,"Case","SLS");g.Values(diagram,"Column","P");g.Values(diagram,"Scale",.2);diagram.Hidden=false;
        g.Group("06  RISULTATI BEAM / NODO / QUERY / DIAGRAMMA",Color.MediumPurple,beam,n,joint,ux,query,rows,diagram);
    }
}
