using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

internal static partial class TopicExamples
{
    private static IEnumerable<Example> FilesAndResults(Assembly a)
    {
        var export=new TopicGraph(a,"30_Export_SDB","Esportazione di un modello GH in un nuovo file SAP, senza eseguire il solver.");var beam=Beam(export);var model=BeamModel(export,beam);export.Preview(model);export.Execute(model,[],true);yield return export.Finish(1,mode:"Export SAP su richiesta");
        yield return Import(a,false);yield return Import(a,true);yield return Reread(a);yield return BeamResults(a);yield return PlateResults(a);
        yield return Link(a,true);yield return Solid(a,true);yield return ModalResults(a);yield return Utilities(a);yield return PreviewBake(a);
    }
    private static Example Import(Assembly a,bool full)
    {
        var g=new TopicGraph(a,full?"32_Import_modello_SAP_associato":"31_Import_sola_geometria",full?"Aprire un SDB esistente e ottenere il Model Grasshopper associato, con unita, nomi, casi, combinazioni e hash della sorgente.":"Importazione della sola geometria da un SDB. Questo output non comprende materiali, proprieta o carichi.");
        g.Step("FILE ESISTENTE", "Sostituire il percorso con un .sdb reale. IMPORT SAP deve passare da False a True. Il sorgente viene aperto tramite una copia privata.");
        var path=g.Panel("File SAP esistente",@"C:\Users\Public\Documents\modello_esistente.sdb");var run=g.Toggle("IMPORT SAP");
        var read=g.Add(full?"ImportModelComponent":"ReadGeometryComponent",full?"Import Model":"Read Geometry");g.G.Wire(path,read,"Path");g.G.Wire(run,read,"Run");
        g.Step(full?"MODEL ASSOCIATO":"GEOMETRIA",full?"Il Model mantiene il file sorgente e il suo hash. La vista GH ricostruisce le parti supportate; tutte le definizioni native restano nel file. Modificare in SAP e reimportare per aggiornare.":"Element Info espone nomi e tipi. Per creare un modello nuovo occorre riassegnare le proprieta; usare Import Model se si vuole mantenere il modello SAP completo.");
        if(full)
        {
            var decompose=g.Add("DecomposeModelComponent","Decomponi");g.Link(read,decompose,"Model");g.Output(decompose,"Units","Unita originali");g.Output(read,"Cases","Casi nel file");g.Output(read,"Issues","Copertura della vista GH");
            g.Preview(read);g.Execute(read,[],true);
        }
        else
        {
            var info=g.Add("ElementInfoComponent","Nomi / tipi");g.Link(read,info,"Definition");g.Output(info,"Names","Nomi nativi");g.Output(info,"Kinds","Tipi di elemento");
        }
        return g.Finish(0,false,"Import SAP su richiesta");
    }
    private static Example Reread(Assembly a)
    {
        var g=new TopicGraph(a,"33_Rilettura_archivio_risultati","Analisi, derivazione del percorso archivio e rilettura verificata dei risultati nel Model.");var b=Beam(g);var model=BeamModel(g,b);g.Preview(model);var run=g.Execute(model,["Q"]);
        g.Step("ARCHIVIO", "SAP File Paths deriva .sdb.results.json dal Path prodotto dal run: non serve mantenere due percorsi a mano. Read Results verifica sia il modello sia l'hash del file SAP.");
        var paths=g.Add("FilePathsComponent","Percorsi associati");g.Wire(run,"Path",paths,"Path");
        var clear=g.Add("ClearResultsComponent","Rimuovi risultati");g.Link(run,clear,"Model");
        var read=g.Add("ReadResultArchiveComponent","Rileggi risultati");g.Link(clear,read,"Model");g.Wire(paths,"Results archive",read,"Archive");
        g.Step("QUERY", "Questa rilettura non apre SAP e non ricalcola. Funziona con l'archivio prodotto dal plugin; non importa automaticamente risultati di un file esterno senza archivio.");Reader(g,read,"Results.FrameForce","M3");
        var api=g.Add("ReadApiTableComponent","Query nativa alternativa",("Method","Results.FrameForce"),("Arguments JSON","{\"Name\":\"ALL\",\"ItemTypeElm\":2}"),("Cases",new[]{"Q"}));g.Wire(run,"Path",api,"File");var trigger=g.Toggle("READ API");g.G.Wire(trigger,api,"Run");g.Output(api,"Columns","Colonne API");return g.Finish(1,mode:"Analisi e rilettura su richiesta");
    }
    private static Example BeamResults(Assembly a)
    {
        var g=new TopicGraph(a,"34_Risultati_beam_diagrammi_deformata","Risultati nel beam, interrogazione delle righe, estremi, diagramma M3 e deformata del modello.");var b=Beam(g);var model=BeamModel(g,b);g.Preview(model);var run=g.Execute(model,["Q"]);
        g.Step("BEAM / QUERY", "Collegare Beams, non il Model non risolto. N=P, V2, V3, T, M2, M3 negli assi locali. Le stazioni conservano l'ordine e gli identificativi del solver.");
        var decompose=g.Add("DecomposeBeamComponent","Beam risolto");g.Wire(run,"Beams",decompose,"Element");g.Output(decompose,"Quantities","Quantita incorporate");
        var actions=g.Add("BeamActionsComponent","N V T M",("Case","Q"));g.Wire(run,"Beams",actions,"Element");g.Output(actions,"M3","M3 [kNm]");
        var query=g.Add("QueryElementResultsComponent","Righe beam",("Quantity","Results.FrameForce"),("Case","Q"));g.Wire(run,"Beams",query,"Element");g.Output(query,"Columns","Ordine delle colonne");
        g.Step("DIAGRAMMA / ESTREMI", "Il diagramma usa M3 del caso Q; scala .1 m/kNm. Gli estremi mantengono caso, oggetto e riga governante.");
        var diagram=g.Add("FrameDiagramComponent","Diagramma M3",("Frame","F1"),("Case","Q"),("Column","M3"),("Scale",.1));g.Link(run,diagram,"Model");diagram.Hidden=false;
        var envelope=g.Add("ResultEnvelopeComponent","Estremi M3",("Quantity","Results.FrameForce"),("Column","M3"));g.Link(run,envelope,"Model");g.Output(envelope,"Governing rows","Righe governanti");
        g.Step("DEFORMATA", "Scala visiva 1000. La deformata interpola gli estremi dei frame, non la curvatura interna. Non usare uno step Max/Min per una forma simultanea.");
        var deform=g.Add("DeformedGeometryComponent","Deformata",("Case","Q"),("Scale",1000d));g.Link(run,deform,"Model");deform.Hidden=false;g.Output(deform,"Translations","Spostamenti globali reali [m]");
        return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static Example PlateResults(Assembly a)
    {
        var g=new TopicGraph(a,"35_Risultati_plate_forze_tensioni_deformazioni","Shell: lettura delle azioni per unita di lunghezza, tensioni top/bottom e deformazioni nei singoli elementi.");var model=MeshModel(g);g.Preview(model);var run=g.Execute(model,["Q"]);
        g.Step("PLATE RISOLTE", "Plates contiene quattro elementi, ciascuno con le proprie righe di risultati. Conservare il ramo GH e il nome dell'elemento nell'esportazione dei valori.");
        var dec=g.Add("DecomposePlateComponent","Decompose Plate");g.Wire(run,"Plates",dec,"Element");g.Output(dec,"Name","Nomi shell");
        var forces=g.Add("PlateActionsComponent","F M V",("Case","Q"));g.Wire(run,"Plates",forces,"Element");g.Output(forces,"M11","M11 [kNm/m]");
        g.Step("TENSIONI / DEFORMAZIONI", "Top/bottom seguono l'asse locale 3 della shell. Le tensioni sono kN/m2; le deformazioni normali e di taglio sono adimensionali.");
        var stress=g.Add("PlateStressComponent","Tensioni",("Case","Q"));g.Wire(run,"Plates",stress,"Element");g.Output(stress,"SVMTop","Von Mises top");
        var strain=g.Add("PlateStrainComponent","Deformazioni",("Case","Q"));g.Wire(run,"Plates",strain,"Element");g.Output(strain,"e11top","e11 top");return g.Finish(4,mode:"Analisi SAP su richiesta");
    }
    private static Example ModalResults(Assembly a)
    {
        var g=new TopicGraph(a,"38_Risultati_modali_masse_forme","Periodi, rapporti di massa partecipante e forme modali. Caso MODAL su una mensola con massa concentrata.");var b=Beam(g,true);var modal=Modal(g,b);var model=BeamModel(g,b,modal);g.Preview(model);
        var run=g.Execute(model,["MODAL"],quantities:["Results.ModalPeriod","Results.ModalParticipatingMassRatios","Results.ModalParticipationFactors","Results.ModeShape"]);
        g.Step("PERIODI / MASSE", "Confrontare StepNum (modo) fra le tabelle. Le forme modali hanno normalizzazione del solver e non sono spostamenti dovuti a un carico statico.");Reader(g,run,"Results.ModalPeriod","Period");Reader(g,run,"Results.ModalParticipatingMassRatios","SumUX");Reader(g,run,"Results.ModalParticipationFactors","UX");Reader(g,run,"Results.ModeShape","U1");return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static Example Utilities(Assembly a)
    {
        var g=new TopicGraph(a,"39_Utility_firme_unita_query","Ispezione delle firme API, delle unita SAP, delle definizioni raccolte e dei dati pronti per verifiche esterne.");var b=Beam(g);var model=BeamModel(g,b);g.Preview(model);
        g.Step("SDK / UNITA", "API Signature usa lo schema dell'SDK installato. Units elenca eUnits: selezionare le unita prima di costruire, senza presumere conversioni automatiche.");
        var help=g.Add("ApiHelpComponent","Firma SDK",("Method","FrameObj.SetLoadDistributed"));g.Output(help,"Parameters","Nomi, tipi ed enum");
        var units=g.Add("UnitsComponent","Unita");g.Output(units,"Names","Sistemi unita");
        g.Step("DEFINIZIONI / CATALOGO", "Definition Info rende visibili i comandi differiti. Quantities elenca tutte le tabelle richiedibili al run.");
        var info=g.Add("DefinitionInfoComponent","Comandi");g.Link(model,info,"Definition");g.Output(info,"Methods","Metodi raccolti");
        var catalogue=g.Add("ResultCatalogueComponent","Quantita risultati");g.Output(catalogue,"Methods","Metodi Results");
        var run=g.Execute(model,["Q"]);g.Step("DATI PER VERIFICHE", "Verification Data espone risultati e definizioni. Complete indica letture API riuscite, non un giudizio di sicurezza o una verifica normativa.");
        var data=g.Add("VerificationDataComponent","Dati verifica");g.Link(run,data,"Model");g.Output(data,"Complete","Complete");var cases=g.Add("ResultCasesComponent","Casi / step");g.Link(run,cases,"Model");g.Output(cases,"Cases and steps","Casi effettivamente letti");return g.Finish(1,mode:"Modellazione e analisi su richiesta");
    }
    private static Example PreviewBake(Assembly a)
    {
        var g=new TopicGraph(a,"40_Preview_interruttori_cast_Brep_bake","Gestione locale della visualizzazione, output Brep e bake solido; nessuna connessione a SAP necessaria.");var b=Beam(g);var model=BeamModel(g,b);g.Preview(model);
        g.Step("CAST DIRETTO", "Il singolo beam puo essere collegato direttamente a un parametro Brep. Per piu elementi usare l'uscita Breps della Preview.");
        g.BrepCast(b.Frame);
        var geom=g.Add("ModelGeometryComponent","Geometria FEM");g.Link(model,geom,"Model");g.Output(geom,"Names","Nomi della geometria analitica");
        g.Step("BAKE", "BAKE SOLIDI: False > True. Physical=true e Solid only=true. Spegnere la preview non cancella gli oggetti gia inseriti nel documento Rhino.");
        var toggle=g.Toggle("BAKE SOLIDI");var bake=g.Add("BakeGeometryComponent","Bake solido",("Physical",true),("Solid only",true));g.Link(model,bake,"Model");g.G.Wire(toggle,bake,"Bake");
        return g.Finish(1);
    }
}
