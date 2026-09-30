using System.Text;
using System.Text.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
internal static partial class Suite
{
    static partial void Extra(string? filter)
    {
        foreach(string p in Host.Projects)
        {
            Make(p,"06_Molle_masse","Vincoli, molle e masse","Mensola con incastro alla base, molla Kz e massa concentrata in punta. Distinguere sempre masse e forze.",Springs,filter);
            Make(p,"07_Assi_offset_svincoli","Assi locali, offset e svincoli","Una catena modifica lo stesso elemento: orientamento, eccentricita, rilascio terminale. Build riceve soltanto l'ultima versione.",Attributes,filter);
            Make(p,"08_Carichi_combinazioni","Casi e combinazioni","Peso proprio G, carico Q e ULS_DEMO = 1.35 G + 1.50 Q. Coefficienti didattici; non sommare copie intermedie dello stesso carico.",Loads,filter);
            Make(p,"09_Unione_filtri","Unione, decomposizione e filtri","Due mensole indipendenti, con ID coordinati, confluiscono in un Model. Decomposizione e filtro recuperano un sottoinsieme.",Merge,filter);
            Make(p,"10_Export","Esportare un modello","Valida il Model, scegli una destinazione nuova e abilita RUN EXPORT. MIDAS richiede anche Replace active document e API configurata.",Export,filter);
                Make(p,"11_Import_model","Modello esistente verso Grasshopper","Leggi il modello nativo e ottieni il Model associato. Controlla Issues per conoscere dati importati e limiti.",Import,filter);
            Make(p,"12_Analisi_frame","Analisi e risultati delle travi","Model -> analisi -> Model con risultati -> casi, azioni, deformata e diagramma. I lettori attendono risultati reali del solver.",g=>Analysis(g,false),filter);
            Make(p,"13_Analisi_mesh","Analisi e risultati delle piastre","Piastra Quad4 con pressione: Model -> solver -> azioni di piastra, spostamenti e reazioni. Nessun risultato e simulato.",g=>Analysis(g,true),filter);
            Make(p,"14_Rilettura_risultati","Risultati gia calcolati","Seleziona il risultato/archivio dell'esempio 12. Il Model deve essere identico a quello dell'analisi; READ legge senza rilanciare il solver.",ReadResults,filter);
            Make(p,"15_Preview_bake","Preview, impostazioni e bake","Confronta assi e solidi fisici. La preview e temporanea; il bake crea oggetti nel documento Rhino. BAKE parte spento.",Preview,filter);
            if(p.StartsWith("Rhino2Midas"))
                Make(p,"16_JSON_trasformazioni","JSON, spostamento e rotazione","Il Model passa a JSON e ritorna senza file. Sposta e ruota una copia: le trasformazioni invalidano eventuali risultati.",Archive,filter);
            else Make(p,"16_Analisi_avanzate","Analisi avanzate","Collegamenti per analisi modale, non lineare e buckling dove disponibili. Controllare massa, casi e stato iniziale; i solver restano spenti.",Advanced,filter);
        }
        Make("Rhino2MidasGen","17_Pareti_piani","Pareti, piani e diaframmi","Modello GEN con parete, piani e diaframma. Il gruppo parete ha identita distinta dall'elemento. Aggiunta di un piano dimostrativo.",Wall,filter);
    }
    static void AddPart(Tutorial g,GH_Component model,GH_Component part,string port)=>g.Wire(part,model,g.Straus?port:"Definitions");
    static void Springs(Tutorial g)
    {
        var s=Frame(g);var spring=g.C(g.Sap?"PointObj_SetSpringComponent":"NodeSpringComponent",2);
        g.Wire(s.TipNodes,spring,g.Straus?"Node":g.Sap?"Name":"Nodes");
        if(g.Straus){g.Set(spring,"Translation",new Vector3d(0,0,1000));g.Set(spring,"Rotation",Vector3d.Zero);}
        else g.Set(spring,g.Sap?"K":"Stiffness",0d,0d,1000d,0d,0d,0d);
        var mass=g.C(g.Sap?"PointObj_SetMassComponent":"NodeMassComponent",2);g.Wire(spring,mass,g.Straus?"Node":g.Sap?"Name":"Nodes");
        if(g.Straus){g.Set(mass,"Translation",new Vector3d(.1,.1,.1));g.Set(mass,"Rotation",Vector3d.Zero);}
        else g.Set(mass,g.Sap?"M":"Masses",.1,.1,.1,0d,0d,0d);
        AddPart(g,s.Model,mass,"Node");g.Review(s.Model,s.Nodes,s.Port,s.Count);
    }
    static void Attributes(Tutorial g)
    {
        var s=Frame(g,false,false);var align=g.C(g.Straus?"AlignBeamAxisComponent":g.Sap?"AlignFrameComponent":"AlignBeamComponent",2);
        g.Wire(s.Elements,align,g.Straus?"Beam":"Definition");g.Set(align,"Direction",Vector3d.ZAxis);
        var offset=g.C(g.Sap?"FrameObj_SetInsertionPointComponent":"BeamOffsetComponent",2);g.Wire(align,offset,g.Sap?"Name":"Beam");
        if(g.Straus)g.Set(offset,"Offset",new Vector3d(0,.05,0));
        else if(g.Sap){g.Set(offset,"CardinalPoint",10);g.Set(offset,"Mirror2",false);g.Set(offset,"StiffTransform",true);g.Set(offset,"Offset1",0d,0d,.05);g.Set(offset,"Offset2",0d,0d,.05);g.Set(offset,"CSys","Global");}
        else{g.Set(offset,"I offset",new Vector3d(0,0,.05));g.Set(offset,"J offset",new Vector3d(0,0,.05));}
        var release=g.C(g.Straus?"BeamReleasesComponent":g.Sap?"FrameObj_SetReleasesComponent":"BeamReleaseComponent",2);g.Wire(offset,release,g.Sap?"Name":"Beam");
        if(g.Midas){g.Set(release,"I mask","000000");g.Set(release,"J mask","000001");}
        else{g.Set(release,g.Straus?"Start":"II",Free);g.Set(release,g.Straus?"End":"JJ",false,false,false,false,false,true);if(g.Sap){g.Set(release,"StartValue",Zero);g.Set(release,"EndValue",Zero);}}
        Tutorial.Port(s.Model.Params.Input,g.Straus?"Frame element":"Definitions").RemoveSource(s.Elements.Params.Output[0]);AddPart(g,s.Model,release,"Frame element");g.Review(s.Model,s.Nodes,s.Port,s.Count,physical:!g.Sap);if(g.Sap){var pg=g.C("PhysicalGeometryComponent",5);g.Wire(s.Model,pg,"Model");g.Show(pg,"LIMITI PREVIEW OFFSET",5,"Issues");g.Note("OFFSET SAP","Per gli insertion/end offset SAP la preview attuale restituisce assi analitici e segnala la limitazione in Issues; non inventa un solido fisico.",0);}
        g.Note("SVINCOLO DIDATTICO","Prima dell'analisi verificare che non introduca gradi di liberta isolati. Gli offset spostano la sezione fisica rispetto all'asse.",0);
    }
    static void Loads(Tutorial g)
    {
        var s=Frame(g);var dead=Case(g,"G",2);GH_Component self;
        if(g.Sap){g.Set(dead,"Type",1);g.Set(dead,"Self weight",1d);self=dead;}
        else{self=g.C("SelfWeightComponent",2);g.Wire(dead,self,g.Straus?"Load case":"Case",g.Midas?"Name":null);}
        AddPart(g,s.Model,self,"Self weight");if(g.Midas)AddPart(g,s.Model,dead,"");
        var combo=g.C(g.Sap?"CombinationComponent":"LoadCombinationComponent",2);g.Set(combo,"Name","ULS_DEMO");
        if(g.Straus)foreach(var(lc,factor)in new[]{(dead,1.35),(s.LoadCase!,1.5)}){var f=g.C("LoadFactorComponent",1);g.Wire(lc,f,"Load case");g.Set(f,"Factor",factor);g.Wire(f,combo,"Load factor");}
        else{g.Set(combo,"Cases","G","Q");g.Set(combo,"Factors",1.35,1.5);}
        AddPart(g,s.Model,combo,"Load combination");g.Review(s.Model,s.Nodes,s.Port,s.Count);
    }
    static void Merge(Tutorial g)
    {
        var a=Frame(g,false,false);var b=Frame(g,false,false,4,2,a.Material,a.Property);var merge=g.C("MergeModelsComponent",3);g.Wire(a.Model,merge,"Models");g.Wire(b.Model,merge,"Models");
        var d=g.Review(merge,4,a.Port,2);var filter=g.C(g.Straus?"FrameFilterComponent":g.Sap?"ElementFilterComponent":"FilterElementsComponent",5);
        if(g.Straus)g.Wire(d,filter,"Frame elements",a.Port);
        else if(g.Sap){g.Wire(merge,filter,"Definition");g.Set(filter,"Kind",1);g.Set(filter,"Names","F1");}
        else{g.Wire(merge,filter,"Model");g.Set(filter,"Kind","Beam");g.Set(filter,"IDs",1);}
        g.Show(filter,"ELEMENTI FILTRATI",5);g.Note("ID E DIPENDENZE","I due modelli condividono materiale e sezione; gli elementi hanno ID distinti. Straus puo rinumerare, SAP/MIDAS richiedono riferimenti coordinati. Il merge invalida risultati precedenti.",0);
    }
    static string NativePath(Tutorial g)=>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Rhino2FEM_Examples",g.Project,g.Name+(g.Straus?".st7":g.Sap?".sdb":g.Project=="Rhino2Midas"?".mcb":".mgb"));
    static GH_Panel PathInput(Tutorial g,string? path=null)=>g.Note("PATH: percorso locale modificabile",path??NativePath(g),0,85);
    static GH_Component Execute(Tutorial g,GH_Component model,bool analyze,out GH_BooleanToggle run)
    {
        var path=PathInput(g);run=g.Toggle(analyze?"RUN ANALISI":"RUN EXPORT");
        var c=g.C(g.Straus?(analyze?"RunModelCasesComponent":"StrausR3ExportComponent"):g.Sap?(analyze?"AnalyzeAndEmbedComponent":"ExportModelComponent"):(analyze?"AnalyzeModelComponent":"ExportModelComponent"),3,true);
        g.Wire(model,c,"Model");g.Wire(path,c,g.Straus?(analyze?"Model path":"Output Path"):"Path");g.Wire(run,c,"Run");
        var overwrite=g.Toggle("OVERWRITE: sostituisci file");g.Wire(overwrite,c,"Overwrite");
        if(g.Midas){var replace=g.Toggle("REPLACE: documento attivo salvato");g.Wire(replace,c,"Replace active document");var connection=g.C("ApiConnectionComponent",1);g.Show(connection,"CONFIGURAZIONE API",1);}
        g.Show(c,"ESITO OPERAZIONE",3,g.Straus?"Status":"Issues");g.Instructions.Add("Controllare Validate, scegliere Path e usare false -> true per avviare l'operazione.");return c;
    }
    static void Export(Tutorial g){var s=Frame(g);g.Review(s.Model,s.Nodes,s.Port,s.Count);Execute(g,s.Model,false,out _);}
    static void Import(Tutorial g)
    {
        var reader=g.C(g.Straus?"ReadStrausModelComponent":g.Sap?"ImportModelComponent":g.Project=="Rhino2Midas"?"ReadCivilModelComponent":"ReadGenModelComponent",2,true);
        var run=g.Toggle("READ MODEL");g.Wire(run,reader,g.Straus?"Read":"Run");
        if(!g.Midas){var path=PathInput(g,NativePath(g).Replace(g.Name,"10_Export"));g.Wire(path,reader,g.Straus?"Model path":"Path");}
        else{var connection=g.C("ApiConnectionComponent",1);g.Show(connection,"CONNESSIONE",1);g.Note("DOCUMENTO ATTIVO","Apri il file nel prodotto corretto e attiva l'API. READ legge il documento attivo senza sostituirlo. Le tabelle aggiuntive sono opzionali.",0);}
        g.Note("UNITA DEL MODELLO",g.Straus?"Il lettore converte in m/kN/kPa/tonnellate/C. Report segnala omissioni; Allow partial parte false.":"Il Model mantiene le unita del documento nativo. Controllare Summary e non forzare kN/m su dati in altre unita.",0);
        if(g.Straus)g.Show(reader,"CORRISPONDENZA ID",3,"Identity map");
        if(g.Sap)g.Note("SORGENTE ASSOCIATA","Conservare il file .sdb: il Model mantiene il collegamento alla versione verificata del sorgente. Per modificare dati nativi aggiornare SAP e importare nuovamente.",0);
        g.Review(g.Gate(reader,run,3),inactive:true,modelOutput:"Target 1");g.Show(reader,"IMPORT / LIMITI",3,g.Straus?"Report":"Issues");if(g.Midas)g.Show(reader,"TABELLE LETTE",3,"Tables");
    }
    static void Live(Tutorial g,GH_Component gate,GH_Component target,string input)=>g.Wire(gate,target,input,"Target 1");
}
