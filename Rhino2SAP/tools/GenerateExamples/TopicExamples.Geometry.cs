using System.Reflection;
using Grasshopper.Kernel;
using Rhino.Geometry;

internal static partial class TopicExamples
{
    public static IEnumerable<Example> Create(Assembly assembly)
    {
        yield return Merge(assembly);
        yield return Polylines(assembly);
        var mesh = new TopicGraph(assembly,"06_Mesh_triangolare_shell","Da mesh triangolare a shell: connettivita, vincoli, pressione e decomposizione.");
        var meshModel=MeshModel(mesh,true);mesh.Preview(meshModel);yield return mesh.Finish(8);
        yield return Sections(assembly);
        yield return Materials(assembly);
        yield return Link(assembly,false);
        yield return Cable(assembly);
        yield return Solid(assembly,false);
        yield return Supports(assembly);
        yield return Axes(assembly);
        yield return Assignments(assembly);
        yield return Mass(assembly);
        foreach(var example in LoadsAndCases(assembly))yield return example;
        foreach(var example in FilesAndResults(assembly))yield return example;
        yield return SectionDesigner(assembly);
        yield return Harmonic(assembly,true);
    }

    private static Example Merge(Assembly a)
    {
        var g=new TopicGraph(a,"04_Creazione_unione_modelli","Creare due sottomodelli e unirli. Unita e tolleranza devono coincidere; gli elementi condivisi si raccolgono una volta.");
        g.Step("PROPRIETA", "Un materiale e una sezione comuni a entrambi i sottomodelli.");var section=Rectangle(g,Material(g));
        g.Step("ELEMENTI", "Due frame consecutivi; il nodo comune viene riconosciuto dalla tolleranza. Esempio geometrico senza analisi.");
        var first=g.Add("FrameElementComponent","F1",("Name","F1"),("Line",new Line(0,0,0,3,0,0)));g.Link(section,first,"Property");
        var second=g.Add("FrameElementComponent","F2",("Name","F2"),("Line",new Line(3,0,0,6,0,0)));g.Link(section,second,"Property");
        var m1=g.Model(first);var m2=g.Model(second);
        g.Step("UNIONE", "Merge mantiene i nomi e rifiuta geometrie diverse con lo stesso nome. Il modello risultante contiene due frame e tre nodi.");
        var merged=g.Add("MergeModelsComponent","Unisci");g.Link(m1,merged,"Models");g.Link(m2,merged,"Models");
        var parts=g.Add("DecomposeModelComponent","Decomponi");g.Link(merged,parts,"Model");g.Output(parts,"Units","Unita SAP");g.Preview(merged);return g.Finish(2);
    }
    private static Example Polylines(Assembly a)
    {
        var g=new TopicGraph(a,"05_Polilinea_portale_frame","Polilinea di un portale: un frame per segmento, selezione per nome e decomposizione beam.");
        g.Step("PROPRIETA", "Sezione 0.20 x 0.10 m; il materiale passa alla sezione e da questa a tutti i segmenti.");var section=Rectangle(g,Material(g));
        g.Step("POLILINEA", "Portale largo 4 m e alto 3 m. Segmenti F1, F2 e F3. Le coordinate sono editabili negli input incorporati.");
        var frames=g.Add("PolylineFramesComponent","Portale",("Polyline",new PolylineCurve(new[]{new Point3d(0,0,0),new Point3d(0,0,3),new Point3d(4,0,3),new Point3d(4,0,0)})));g.Link(section,frames,"Property");
        var nodes=g.Add("NodeElementComponent","Basi",("Name",new[]{"B1","B2"}),("Point",new[]{Point3d.Origin,new Point3d(4,0,0)}));var fix=Support(g,nodes);
        var model=g.Model(frames,fix);g.Step("FILTRI", "Filtrare prima di aggiungere nuovi carichi: Filter conserva le proprieta e rimuove le assegnazioni. Decompose Beam restituisce linea e vertici anche prima del calcolo.");
        var filter=g.Add("ElementFilterComponent","Traverso F2",("Kind",1),("Names",new[]{"F2"}));g.Link(model,filter,"Definition");
        var beam=g.Add("DecomposeBeamComponent","Beam");g.Link(filter,beam,"Element");g.Output(beam,"Vertices","Estremi del traverso");g.Preview(model);return g.Finish(3);
    }
    private static Example Sections(Assembly a)
    {
        var g=new TopicGraph(a,"07_Sezioni_rettangolo_I_tubo_pipe","Quattro sezioni: rettangolare, doppio T, tubolare rettangolare e circolare cavo. Preview e solidi reali.");
        g.Step("MATERIALE", "Acciaio isotropo, unita kN/m. Le dimensioni sotto sono in metri.");var material=Material(g);
        g.Step("SEZIONI", "R: 0.20 x 0.10. I: h=.30, b=.15, tf=.012, tw=.008. RHS: .25 x .15 x .01. CHS: diametro .20, t=.01.");
        var sections=new[]{Rectangle(g,material),g.Api("PropFrame.SetISection",("Name","I300"),("T3",.3),("T2",.15),("Tf",.012),("Tw",.008),("T2b",.15),("Tfb",.012)),g.Api("PropFrame.SetTube",("Name","RHS"),("T3",.25),("T2",.15),("Tf",.01),("Tw",.01)),g.Api("PropFrame.SetPipe",("Name","CHS"),("T3",.2),("TW",.01))};
        foreach(var section in sections.Skip(1))g.Link(material,section,"MatProp");
        g.Step("FRAME", "Quattro assi paralleli lunghi 3 m. Esempio geometrico: non contiene vincoli o carichi.");
        var frames=sections.Select((s,i)=>{var f=g.Add("FrameElementComponent","F"+(i+1),("Name","F"+(i+1)),("Line",new Line(0,i,0,3,i,0)));g.Link(s,f,"Property");return f;}).ToArray();
        var model=g.Model(frames);g.Preview(model);return g.Finish(4);
    }
    private static Example Materials(Assembly a)
    {
        var g=new TopicGraph(a,"08_Materiali_elastici_libreria_CSI","Confronto fra materiale elastico esplicito e materiale dalla libreria europea CSI.");
        g.Step("ELASTICO", "Material definisce E, Poisson, dilatazione e peso specifico. Non aggiunge da solo dati normativi di progetto.");var explicitSection=Rectangle(g,Material(g));
        g.Step("LIBRERIA CSI", "DB Material richiede SAP per risolvere le proprieta al momento dell'export. Standard e grado devono esistere nella libreria installata.");
        var library=g.Add("DatabaseMaterialComponent","S355 da database",("Name","S355_DB"));var dbSection=Rectangle(g,library,"R_DB");
        var info=g.Add("DefinitionInfoComponent","Definizione nativa");g.Link(library,info,"Definition");g.Output(info,"Arguments","Standard / grado richiesti");
        g.Step("ELEMENTI", "La forma geometrica delle due sezioni coincide. Il materiale di database verra completato da SAP.");
        var f1=g.Add("FrameElementComponent","Elastico",("Name","F1"),("Line",new Line(0,0,0,3,0,0)));g.Link(explicitSection,f1,"Property");
        var f2=g.Add("FrameElementComponent","Database",("Name","F2"),("Line",new Line(0,1,0,3,1,0)));g.Link(dbSection,f2,"Property");var model=g.Model(f1,f2);g.Preview(model);return g.Finish(2);
    }
    private static Example Link(Assembly a,bool results)
    {
        var g=new TopicGraph(a,results?"36_Risultati_link":"09_Link_elastico", "Link assiale elastico: K1=1000 kN/m, forza +1 kN. Atteso spostamento assiale 0.001 m; gli altri gradi di liberta sono vincolati.");
        g.Step("PROPRIETA LINK", "DOF, Fixed, Ke e Ce hanno sei valori U1 U2 U3 R1 R2 R3. Solo U1 e elastico. Il link non ha un volume fisico da convertire in Brep.");
        var prop=g.Api("PropLink.SetLinear",("Name","K1000"),("DOF",new[]{true,false,false,false,false,false}),("Fixed",new bool[6]),("Ke",new double[]{1000,0,0,0,0,0}),("Ce",Zero),("DJ2",0d),("DJ3",0d));
        g.Step("LINK / NODI", "Link L1 lungo X; nodo BASE incastrato, nodo TIP libero solo in X.");var link=g.Add("LinkElementComponent","L1",("Name","L1"),("Line",new Line(0,0,0,1,0,0)));g.Link(prop,link,"Property");
        var fix=Support(g,Node(g,"BASE",Point3d.Origin));var tip=Node(g,"TIP",new Point3d(1,0,0));var guided=Support(g,tip,[false,true,true,true,true,true]);
        g.Step("CARICO", "Carico globale FX=1 kN al TIP.");var pattern=Pattern(g);var force=g.Api("PointObj.SetLoadForce",("Value",new double[]{1,0,0,0,0,0}));g.Link(tip,force,"Name");g.Link(pattern,force,"LoadPat");var model=g.Model(link,fix,guided,force);g.Preview(model);
        if(results){var run=g.Execute(model,["Q"],quantities:["Results.LinkForce","Results.LinkDeformation","Results.JointDispl"]);g.Step("RISULTATI LINK", "Forze e deformazioni native. I numeri restano vuoti fino all'esecuzione SAP.");Reader(g,run,"Results.LinkForce","P");Reader(g,run,"Results.LinkDeformation","U1");}
        return g.Finish(0,mode:results?"Analisi SAP su richiesta":"Modellazione locale");
    }
    private static Example Cable(Assembly a)
    {
        var g=new TopicGraph(a,"10_Cavo_e_carico_distribuito","Proprieta di cavo, connettivita e carico distribuito. Il comportamento a fune richiede una configurazione non lineare in SAP.");
        g.Step("MATERIALE / CAVO", "Area di acciaio = .001 m2. La linea in GH identifica gli estremi; non simula la catenaria.");var mat=Material(g);var prop=g.Api("PropCable.SetProp",("Name","CABLE"),("Area",.001));g.Link(mat,prop,"MatProp");
        g.Step("ELEMENTI", "Cavo tra (0,0,0) e (6,0,1), entrambi gli estremi vincolati. Il disegno mostra l'asse, non un solido di cavo.");var cable=g.Add("CableElementComponent","C1",("Name","C1"),("Line",new Line(0,0,0,6,0,1)));g.Link(prop,cable,"Property");
        var nodes=g.Add("NodeElementComponent","Estremi",("Name",new[]{"B1","B2"}),("Point",new[]{Point3d.Origin,new Point3d(6,0,1)}));var fix=Support(g,nodes);
        g.Step("CARICO", "Carico verticale -0.2 kN/m, Dir=6 e sistema Global. Definire tensionamento e caso non lineare prima di un uso progettuale.");var pattern=Pattern(g);var load=g.Api("CableObj.SetLoadDistributed",("MyType",1),("Dir",6),("Value",-.2));g.Link(cable,load,"Name");g.Link(pattern,load,"LoadPat");var model=g.Model(load,fix);g.Preview(model);return g.Finish(0);
    }
    private static Example Solid(Assembly a,bool results)
    {
        var g=new TopicGraph(a,results?"37_Risultati_solidi":"11_Solido_otto_nodi", "Un esaedro 1 x 1 x 1 m, base vincolata e forze verticali sui quattro nodi superiori.");
        g.Step("MATERIALE / SOLIDO", "Calcestruzzo isotropo. Rotazioni del materiale A=B=C=0; Incompatible=false.");var mat=Material(g,true);var prop=g.Api("PropSolid.SetProp",("Name","SOLID"),("A",0d),("B",0d),("C",0d),("Incompatible",false));g.Link(mat,prop,"MatProp");
        g.Step("CONNETTIVITA", "Vertici: quattro inferiori antiorari, poi i corrispondenti superiori. Nessuna tetraedrizzazione implicita.");Point3d[] vertices=[new(0,0,0),new(1,0,0),new(1,1,0),new(0,1,0),new(0,0,1),new(1,0,1),new(1,1,1),new(0,1,1)];
        var solid=g.Add("SolidElementComponent","S1",("Name","S1"),("Vertices",vertices));g.Link(prop,solid,"Property");var bottom=g.Add("NodeElementComponent","Base",("Name",new[]{"B1","B2","B3","B4"}),("Point",vertices[..4]));var fix=Support(g,bottom);
        var top=g.Add("NodeElementComponent","Sommita",("Name",new[]{"T1","T2","T3","T4"}),("Point",vertices[4..]));
        g.Step("CARICHI", "Quattro forze FZ=-1 kN. Risultante totale -4 kN; questo non equivale a una mesh raffinata per valutare i picchi.");var pattern=Pattern(g);var force=g.Api("PointObj.SetLoadForce",("Value",new double[]{0,0,-1,0,0,0}));g.Link(top,force,"Name");g.Link(pattern,force,"LoadPat");var model=g.Model(solid,fix,force);g.Preview(model);
        if(results){var run=g.Execute(model,["Q"],quantities:["Results.SolidStress","Results.SolidStrain","Results.JointDispl"]);g.Step("TENSIONI / DEFORMAZIONI", "Colonne native riferite ai punti del solido; conservare gli identificativi quando si interpretano i valori.");Reader(g,run,"Results.SolidStress","S11");Reader(g,run,"Results.SolidStrain","E11");}
        return g.Finish(1,mode:results?"Analisi SAP su richiesta":"Modellazione locale");
    }
    private static Example Supports(Assembly a)
    {
        var g=new TopicGraph(a,"12_Vincoli_molle_rilasci","Mensola con molla verticale al TIP e rilascio torsionale all'estremita J del frame.");var b=Beam(g);
        g.Step("MOLLA / RILASCIO", "K nodale in ordine U1 U2 U3 R1 R2 R3; K3=100 kN/m. Rilascio R1 al solo estremo J. Gli altri rilasci restano False.");
        var spring=g.Api("PointObj.SetSpring",("K",new double[]{0,0,100,0,0,0}),("IsLocalCSys",false));g.Link(b.Tip,spring,"Name");
        var releases=g.Api("FrameObj.SetReleases",("II",new bool[6]),("JJ",new[]{false,false,false,true,false,false}),("StartValue",Zero),("EndValue",Zero));g.Link(b.Frame,releases,"Name");var model=BeamModel(g,b,spring,releases);g.Preview(model);return g.Finish(1);
    }
    private static Example Axes(Assembly a)
    {
        var g=new TopicGraph(a,"13_Assi_locali_offset","Orientare l'asse locale 2 e assegnare offset alle estremita. La connettivita geometrica resta la stessa.");var b=Beam(g);
        g.Step("ASSI / OFFSET", "Asse locale 2 verso Y globale. Offset I e J di 0.10 m. Il plugin segnala che il Brep fisico con offset non e ricostruito; resta l'asse FEM.");
        var align=g.Add("AlignFrameComponent","Locale 2 -> Y",("Direction",Vector3d.YAxis));g.Link(b.Frame,align,"Definition");
        var offset=g.Api("FrameObj.SetEndLengthOffset",("AutoOffset",false),("Length1",.1),("Length2",.1),("RZ",.5));g.Link(align,offset,"Name");var model=g.Model(offset,b.Support,b.Load);g.Preview(model);return g.Finish(0);
    }
    private static Example Assignments(Assembly a)
    {
        var g=new TopicGraph(a,"14_Gruppi_modificatori_stazioni","Gruppi, modificatori di rigidezza e stazioni di output su un frame.");var b=Beam(g);
        g.Step("ASSEGNAZIONI", "Gruppo BEAMS; modificatori tutti 1 salvo I3=.8. Cinque stazioni minime. Gli attributi sono comandi differiti: vengono applicati all'export.");
        var group=g.Api("GroupDef.SetGroup",("Name","BEAMS"));var assign=g.Api("FrameObj.SetGroupAssign");g.Link(group,assign,"GroupName");g.Link(b.Frame,assign,"Name");
        var modifier=g.Api("FrameObj.SetModifiers",("Value",new double[]{1,1,1,1,1,.8,1,1}));g.Link(assign,modifier,"Name");
        var stations=g.Api("FrameObj.SetOutputStations",("MyType",2),("MaxSegSize",.5),("MinSections",5));g.Link(modifier,stations,"Name");var model=g.Model(stations,b.Support,b.Load);g.Preview(model);return g.Finish(1);
    }
    private static GH_Component AddMass(TopicGraph g,GH_Component tip)
    {
        var mass=g.Api("PointObj.SetMass",("M",new double[]{1,1,1,0,0,0}),("IsLocalCSys",false));g.Link(tip,mass,"Name");
        var source=g.Api("SourceMass.SetMassSource",("Name","MASS"),("MassFromElements",true),("MassFromMasses",true),("MassFromLoads",false),("IsDefault",true),("NumberLoads",0),("LoadPat",new[]{"Q"}),("SF",new[]{0d}));g.Link(mass,source);return source;
    }
    private static Example Mass(Assembly a)
    {
        var g=new TopicGraph(a,"15_Masse_sorgente_modale","Massa nodale e mass source. In kN-m-s la massa e kN*s2/m: non inserire direttamente un peso.");var b=Beam(g,true);
        g.Step("MASSA", "Massa traslazionale 1 nelle tre direzioni al TIP. La mass source include masse proprie degli elementi e masse assegnate; esclude i carichi.");var source=AddMass(g,b.Tip);var model=BeamModel(g,b,source);g.Preview(model);return g.Finish(1);
    }
    private static Example SectionDesigner(Assembly a)
    {
        var g=new TopicGraph(a,"41_Section_Designer","Sezione Section Designer con una forma rettangolare piena. Esempio di definizioni dipendenti.");
        g.Step("SEZIONE SD", "Il materiale base e STEEL; DesignType=0. SDShape aggiunge una forma alla sezione esistente.");var mat=Material(g);var sd=g.Api("PropFrame.SetSDSection",("Name","SD_RECT"));g.Link(mat,sd,"MatProp");
        g.Step("FORMA", "Rettangolo centrato: H=.30, W=.20, nessuna rotazione. La preview fisica di Section Designer non e supportata; consultare Issues.");
        var shape=g.Api("PropFrame.SDShape.SetSolidRect",("ShapeName","RECT1"),("SSOverwrite","Default"),("XCenter",0d),("YCenter",0d),("H",.3),("W",.2),("Rotation",0d));g.Link(sd,shape,"Name");g.Link(mat,shape,"MatProp");
        var frame=g.Add("FrameElementComponent","SD F1",("Name","F1"),("Line",new Line(0,0,0,3,0,0)));g.Link(shape,frame,"Property");var model=g.Model(frame);g.Preview(model);return g.Finish(0);
    }
}
