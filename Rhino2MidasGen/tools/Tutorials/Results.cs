using System.Text;
using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
internal static partial class Suite
{
    static void ResultReaders(Tutorial g,GH_Component solved,GH_BooleanToggle run,bool plate)
    {
        var gate=g.Gate(solved,run,4);
        var cases=g.C(g.Straus?"ModelResultCasesComponent":"ResultCasesComponent",5,true);Live(g,gate,cases,"Model");g.Show(cases,"CASI / STATI",5);
        var d=g.C("DecomposeModelComponent",5,true);Live(g,gate,d,"Model");
        var actions=g.C(g.Straus?(plate?"ModelPlateForceComponent":"ModelBeamActionsComponent"):(plate?"PlateActionsComponent":"BeamActionsComponent"),5,true);
        if(g.Straus)Live(g,gate,actions,"Model");else if(g.Sap)g.Wire(d,actions,"Element",plate?"Area":"Frame");else Live(g,gate,actions,"Source");
        g.Show(actions,plate?"AZIONI DI PIASTRA":"AZIONI DI TRAVE",5);
        var deformed=g.C(g.Straus?"ModelDeformedGeometryComponent":"DeformedGeometryComponent",6,true);Live(g,gate,deformed,"Model");g.Set(deformed,"Scale",100d);deformed.Hidden=false;
        if(!g.Straus)g.Set(deformed,"Case",g.Sap?"Q":"Q(ST)");
        if(g.Straus)
        {
            var reaction=g.C("ModelNodeReactionComponent",6,true);Live(g,gate,reaction,"Model");g.Show(reaction,"REAZIONI",6);
            if(!plate){var diagram=g.C("ModelBeamDiagramComponent",6,true);Live(g,gate,diagram,"Model");g.Set(diagram,"Action","M2");diagram.Hidden=false;}
            else{var moments=g.C("ModelPlateMomentComponent",6,true);Live(g,gate,moments,"Model");g.Show(moments,"MOMENTI",6);}
        }
        else if(g.Sap&&!plate)
        {
            var diagram=g.C("FrameDiagramComponent",6,true);Live(g,gate,diagram,"Model");g.Set(diagram,"Frame","F1");g.Set(diagram,"Case","Q");g.Set(diagram,"Column","M3");diagram.Hidden=false;
            var env=g.C("ResultEnvelopeComponent",6,true);Live(g,gate,env,"Model");g.Set(env,"Quantity","Results.FrameForce");g.Set(env,"Column","M3");g.Show(env,"ESTREMI FIRMATI",6);
        }
        else if(g.Midas)
        {
            var reaction=g.C("NodeReactionsComponent",6,true);Live(g,gate,reaction,"Source");g.Show(reaction,"REAZIONI",6);
            var extrema=g.C("ResultExtremaComponent",6,true);g.Wire(actions,extrema,"Table","Table");g.Set(extrema,"Column",plate?"Mxx":"Moment-y");g.Show(extrema,"ESTREMI FIRMATI",6);
            if(!plate){var diagram=g.C("BeamDiagramComponent",6,true);g.Wire(d,diagram,"Beam","Beam");g.Wire(actions,diagram,"Table","Table");diagram.Hidden=false;}
        }
        g.Note("LETTORI IN ATTESA","Stream Gate chiude il ramo finche RUN/READ=false. Dopo il calcolo scegli un caso reale. MIDAS usa etichette come Q(ST): controlla quelle restituite. Gli inviluppi non sono una deformata simultanea.",0,180);
    }
    static void Analysis(Tutorial g,bool plate)
    {var s=plate?Plate(g):Frame(g);g.Review(s.Model,s.Nodes,s.Port,s.Count);var analyze=Execute(g,s.Model,true,out var run);ResultReaders(g,analyze,run,plate);}
    static void ReadResults(Tutorial g)
    {
        var s=Frame(g);g.Review(s.Model,s.Nodes,s.Port,s.Count);
        string native=NativePath(g).Replace(g.Name,"12_Analisi_frame");var read=g.Toggle("READ RISULTATI");
        var reader=g.C(g.Straus?"ReadResultsIntoModelComponent":"ReadResultArchiveComponent",3,true);g.Wire(s.Model,reader,"Model");
        if(g.Straus){var path=PathInput(g,native);g.Wire(path,reader,"Model path");var result=g.Note("RISULTATO .LSA",Path.ChangeExtension(native,"LSA"),0,80);g.Wire(result,reader,"Result path");g.Wire(read,reader,"Read");}
        else{var path=PathInput(g,native+".results.json");var gate=g.Gate(path,read,2);Live(g,gate,reader,"Archive");}
        ResultReaders(g,reader,read,false);g.Note("PROVENIENZA","Gli archivi verificano impronta e file sorgente. In caso di mismatch ripristinare il Model corretto o ricalcolare; non disattivare la verifica per attribuire risultati a geometrie diverse.",0,150);
    }
    static void Preview(Tutorial g)
    {
        var s=Frame(g);g.Review(s.Model,s.Nodes,s.Port,s.Count);var settings=g.C("DisplaySettingsComponent",5);
        var labels=g.Toggle("Etichette",0,true);g.Wire(labels,settings,"Labels");
        var preview=g.C("PreviewModelComponent",5);g.Wire(s.Model,preview,"Model");g.Wire(settings,preview,"Settings");preview.Hidden=false;
        var geometry=g.C(g.Straus?"ModelGeometryComponent":"PhysicalGeometryComponent",5);g.Wire(s.Model,geometry,"Model");
        if(g.Straus)g.Note("BAKE STRAUS","Model Physical Geometry restituisce Brep chiusi e supporta il bake dal menu contestuale. Bake Straus Geometry serve invece per un file .st7.",0);
        else{var bake=g.C("BakeGeometryComponent",5,true);g.Wire(s.Model,bake,"Model");var run=g.Toggle("BAKE SOLIDI");g.Wire(run,bake,"Bake");g.Set(bake,"Physical",true);}
    }
    static void Archive(Tutorial g)
    {
        var s=Frame(g);var json=g.C("ModelArchiveComponent",3);g.Wire(s.Model,json,"Model");
        var restored=g.C("ReadModelArchiveComponent",3);g.Wire(json,restored,"JSON");g.Review(restored,s.Nodes,s.Port,s.Count);
        var move=g.C("MoveModelComponent",5);g.Wire(restored,move,"Definition");g.Set(move,"Translation",new Vector3d(0,2,0));
        var rotate=g.C("RotateModelComponent",5);g.Wire(move,rotate,"Definition");g.Set(rotate,"Angle",30d);g.Set(rotate,"Axis",Vector3d.ZAxis);
        var transformed=g.C("BuildModelComponent",5);g.Wire(rotate,transformed,"Definitions");g.Review(transformed,s.Nodes,s.Port,s.Count,col:6);
    }
    static void Advanced(Tutorial g)
    {
        var s=Frame(g);g.Review(s.Model,s.Nodes,s.Port,s.Count);
        if(g.Straus)
        {
            foreach(string type in new[]{"RunModelModalComponent","RunModelNonlinearComponent","RunModelBucklingComponent"})
            {
                var c=g.C(type,5,true);g.Wire(s.Model,c,"Model");var path=PathInput(g,NativePath(g).Replace(g.Name,g.Name+"_"+type));g.Wire(path,c,"Model path");var run=g.Toggle("RUN "+type);g.Wire(run,c,"Run");
                if(type.Contains("Nonlinear"))g.Set(c,"Case name","Q");
                if(type.Contains("Buckling")){var lsa=g.Note("LSA INIZIALE",Path.ChangeExtension(NativePath(g).Replace(g.Name,"12_Analisi_frame"),"LSA"),0,80);g.Wire(lsa,c,"Initial LSA");}
            }
        }
        else
        {
            foreach(string type in new[]{"NonlinearCaseComponent","BucklingCaseComponent"})
            {var c=g.C(type,2);g.Set(c,"Name",type.Contains("Nonlinear")?"NL_DEMO":"BUCKLING_DEMO");g.Set(c,"Patterns","Q");g.Set(c,"Factors",1d);g.Wire(s.LoadCase!,c,"Definitions");AddPart(g,s.Model,c,"");}
            var action=Execute(g,s.Model,true,out _);g.Set(action,"Cases","NL_DEMO","BUCKLING_DEMO");g.Note("STEP DEI RISULTATI","Per casi non lineari selezionare lo step. Controllare nel catalogo native le quantita disponibili; il ramo di analisi riporta le mancanze in Issues.",0);
        }
    }
    static void Wall(Tutorial g)
    {
        var source=g.C("WallExampleComponent",1);var d=g.Review(source);
        var wall=g.C("DecomposeWallComponent",5);g.Wire(d,wall,"Element","Wall");g.Show(wall,"DATI PARETE",5);
        var story=g.C("StoryComponent",1);g.Set(story,"ID",3);g.Set(story,"Name","Piano_demo");g.Set(story,"Level",6d);g.Set(story,"Rigid diaphragm",false);
        var model=Model(g,(source,""),(story,""));g.Review(model,col:6);
        g.Note("PIANI","Story aggiunge i dati del piano, non crea automaticamente la geometria della struttura. Il piano aggiuntivo in questo esempio non ha elementi associati.",0);
    }
    static partial void Guides()
    {
        var root=new StringBuilder("# Raccolta tematica Grasshopper\n\nAprire i file .gh con Rhino 8 e il plugin completo dalla rispettiva cartella bin. Ogni file contiene geometria, gruppi e istruzioni.\n\n");
        foreach(string project in Host.Projects)
        {
            string dir=Path.Combine(Host.Root,project,"examples","tutorials");if(!Directory.Exists(dir))continue;
            var guide=new StringBuilder("# "+project+" — tutorial Grasshopper\n\nI modelli costruiti usano kN, m, C; gli importatori riportano le unita native o la conversione dichiarata. RUN, READ, BAKE, REPLACE e OVERWRITE sono inizialmente spenti.\n\n| File | Argomento |\n|---|---|\n");
            foreach(string path in Directory.GetFiles(dir,"*.checks.json").Order())
            {using var r=JsonDocument.Parse(File.ReadAllText(path));string file=r.RootElement.GetProperty("File").GetString()!,title=r.RootElement.GetProperty("Title").GetString()!;guide.AppendLine($"| [{file}]({file}) | {title} |");}
            guide.AppendLine("\nI file .checks.json registrano collegamenti e controlli. Ogni definizione e calcolata, salvata e riaperta in .gh e .ghx. Sono verificati i rami offline, i modelli costruiti e le geometrie. Dove applicabile, una variazione del 10% dello slider deve cambiare il volume dei solidi; il valore iniziale viene ripristinato. Le azioni native restano spente e richiedono applicazione/licenza/API e file reali. Gli avvisi di input vuoto sui rami spenti sono attesi. Nessun risultato fittizio e incorporato.\n\nPer avviare un'azione usare false -> true. Scegliere Path; per MIDAS configurare Connection. Leggere Issues dopo import e analisi. Validate controlla i dati, non dimostra stabilita o adeguatezza strutturale.\n");
            File.WriteAllText(Path.Combine(dir,"README.md"),guide.ToString().TrimEnd()+Environment.NewLine);root.AppendLine($"- [{project}]({project}/examples/tutorials/README.md): {Directory.GetFiles(dir,"*.gh").Length} esempi.");
        }
        root.AppendLine("\n[Importare un modello esistente](IMPORTAZIONE-MODELLI.md): il tutorial 11 dei quattro plugin collega il lettore al Model, alla validazione e alla preview. Per le raccolte specifiche di SAP e Straus consultare anche i rispettivi examples/README.md.\n\nRigenerazione dalla radice Rhino2FEM, dopo build.ps1:\n\n    dotnet run --project Rhino2MidasGen/tools/Tutorials -c Release -- .\n\nAggiungere un filtro come 02_Mesh o Rhino2SAP per rigenerare una parte. Eseguire poi clean.ps1 per rimuovere gli intermedi.");
        File.WriteAllText(Path.Combine(Host.Root,"TUTORIAL-GRASSHOPPER.md"),root.ToString());
    }
}