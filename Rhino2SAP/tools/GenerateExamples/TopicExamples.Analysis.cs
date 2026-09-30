using System.Reflection;
using Grasshopper.Kernel;
using Rhino.Geometry;

internal static partial class TopicExamples
{
    private static IEnumerable<Example> LoadsAndCases(Assembly a)
    {
        yield return Patterns(a);yield return NodalLoads(a);yield return FrameLoads(a);yield return PlateLoads(a);
        yield return Thermal(a);yield return WindSeismic(a,false);yield return WindSeismic(a,true);
        yield return StaticCombinations(a);yield return Nonlinear(a);yield return Buckling(a);yield return Stages(a);
        yield return ModalSpectrum(a);yield return TimeHistory(a);yield return Harmonic(a,false);
    }
    private static Example Patterns(Assembly a)
    {
        var g=new TopicGraph(a,"16_Pattern_peso_proprio","Separare DEAD e Q: il peso proprio si attiva una sola volta, in DEAD.");var b=Beam(g);
        g.Step("DEAD", "Type=1 e Self weight=1. Q ha Self weight=0. SAP genera un caso statico omonimo per ciascun pattern.");var dead=Pattern(g,"DEAD",1,1);
        var model=BeamModel(g,b,dead);g.Preview(model);return g.Finish(1);
    }
    private static Example NodalLoads(Assembly a)
    {
        var g=new TopicGraph(a,"17_Forze_momenti_cedimenti","Forza nodale, momento e cedimento imposto. Vettori a sei componenti e sistemi di riferimento espliciti.");var b=Beam(g);
        g.Step("MOMENTO / CEDIMENTO", "Aggiunta di MY=2 kNm in Q. Cedimento UZ=-.001 m della BASE in SETTLE. Il nodo BASE e vincolato; il cedimento e un carico, non una traslazione geometrica.");
        var moment=g.Api("PointObj.SetLoadForce",("Value",new double[]{0,0,0,0,2,0}),("Replace",false),("CSys","Global"));g.Link(b.Tip,moment,"Name");g.Link(b.Pattern,moment,"LoadPat");
        var pattern=Pattern(g,"SETTLE");var settlement=g.Api("PointObj.SetLoadDispl",("Name","BASE"),("Value",new double[]{0,0,-.001,0,0,0}),("CSys","Global"));g.Link(b.Support,settlement);g.Link(pattern,settlement,"LoadPat");
        var model=BeamModel(g,b,moment,settlement);g.Preview(model);return g.Finish(1);
    }
    private static Example FrameLoads(Assembly a)
    {
        var g=new TopicGraph(a,"18_Carichi_frame_distribuiti_puntuali","Carico trapezoidale e carico puntuale su frame: distanze relative, segni e assi globali.");var b=Beam(g);
        g.Step("CARICHI FRAME", "Distribuito da -2 a -4 kN/m lungo tutta la trave; forza -3 kN a meta luce. MyType=1, Dir=6, Global, RelDist=true, Replace=false.");
        var distributed=g.Api("FrameObj.SetLoadDistributed",("MyType",1),("Dir",6),("Dist1",0d),("Dist2",1d),("Val1",-2d),("Val2",-4d),("Replace",false));g.Link(b.Frame,distributed,"Name");g.Link(b.Pattern,distributed,"LoadPat");
        var point=g.Api("FrameObj.SetLoadPoint",("MyType",1),("Dir",6),("Dist",.5),("Val",-3d),("Replace",false));g.Link(distributed,point,"Name");g.Link(b.Pattern,point,"LoadPat");var model=g.Model(point,b.Support,b.Load);g.Preview(model);return g.Finish(1);
    }
    private static Example PlateLoads(Assembly a)
    {
        var g=new TopicGraph(a,"19_Plate_pressione_mesh_SAP","Pressione uniforme sulle shell e automesh SAP. La mesh GH degli oggetti e la mesh di analisi SAP sono livelli diversi.");var model=MeshModel(g);
        g.Step("AUTOMESH SAP", "Ogni oggetto A1-A4 e suddiviso 2 x 2 dal solver (MeshType=1). La preview mostra le quattro shell GH, non la mesh interna generata da SAP.");
        var auto=g.Api("AreaObj.SetAutoMesh",("Name","ALL"),("ItemType",1),("MeshType",1),("N1",2),("N2",2));g.Link(model,auto);var rebuilt=g.Model(auto);g.Preview(rebuilt);return g.Finish(4);
    }
    private static Example Thermal(Assembly a)
    {
        var g=new TopicGraph(a,"20_Temperatura_plate","Incremento termico uniforme sulle shell: coefficiente di dilatazione, pattern dedicato e vincoli.");var model=MeshModel(g);
        g.Step("TEMPERATURA", "Pattern TEMP; incremento +20 C. MyType=1 significa variazione uniforme, non gradiente. Il vincolo al bordo impedisce la libera dilatazione.");
        var pattern=Pattern(g,"TEMP");var load=g.Api("AreaObj.SetLoadTemperature",("Name","ALL"),("ItemType",1),("MyType",1),("Value",20d));g.Link(model,load);g.Link(pattern,load,"LoadPat");var result=g.Model(load);g.Preview(result);return g.Finish(4);
    }
    private static Example WindSeismic(Assembly a,bool seismic)
    {
        var g=new TopicGraph(a,seismic?"22_Sisma_coefficiente_utente":"21_Vento_diaframma", seismic?"Forza sismica equivalente da coefficiente utente. Valori dimostrativi, senza applicazione di una normativa.":"Carico vento assegnato a un diaframma rigido orizzontale con forze e momento espliciti.");var b=Beam(g,true);
        g.Step("DIAFRAMMA / MASSA", "Diaframma D1 in piano XY: asse Z globale. Nodo TIP a quota 3 m; massa traslazionale unitaria. Esempio minimo, non edificio di progetto.");
        var diaphragm=g.Api("ConstraintDef.SetDiaphragm",("Name","D1"),("Axis",3),("CSys","Global"));var assign=g.Api("PointObj.SetConstraint");g.Link(b.Tip,assign,"Name");g.Link(diaphragm,assign,"ConstraintName");var mass=AddMass(g,b.Tip);
        g.Step(seismic?"SISMA UTENTE":"VENTO UTENTE",seismic?"EQX: Type=5. DirFlag=1 (X), C=.10, K=1, quote da 0 a 3 m; eccentricita zero. Coefficiente puramente didattico.":"WIND: Type=6. FX=10 kN, FY=0, MZ=2 kNm, punto (0,0). Il carico usa la definizione D1 e le sue assegnazioni.");
        var pattern=Pattern(g,seismic?"EQX":"WIND",seismic?5:6);
        var load=seismic?g.Api("LoadPatterns.AutoSeismic.SetUserCoefficient",("DirFlag",1),("Eccen",0d),("UserZ",true),("TopZ",3d),("BottomZ",0d),("C",.1),("K",1d)):
            g.Api("LoadPatterns.AutoWind.SetUserLoad",("FX",10d),("FY",0d),("MZ",2d),("X",0d),("Y",0d));
        g.Link(pattern,load,"Name");g.Link(assign,load);if(!seismic)g.Link(diaphragm,load,"Diaph");var model=BeamModel(g,b,mass,load);g.Preview(model);return g.Finish(1);
    }
    private static Example StaticCombinations(Assembly a)
    {
        var g=new TopicGraph(a,"23_Casi_statici_combinazioni_inviluppo","Pattern, casi e combinazioni sono oggetti diversi. Esempio di combinazione lineare e inviluppo annidato.");var b=Beam(g);
        g.Step("CASI", "SLS=1*Q e NEG=-1*Q. Le liste Patterns e Factors devono avere la stessa lunghezza.");
        var sls=g.Add("LinearCaseComponent","SLS",("Name","SLS"),("Patterns",new[]{"Q"}),("Factors",new[]{1d}));g.Link(b.Pattern,sls);
        var neg=g.Add("LinearCaseComponent","NEG",("Name","NEG"),("Patterns",new[]{"Q"}),("Factors",new[]{-1d}));g.Link(b.Pattern,neg);
        g.Step("COMBINAZIONI", "ULS_DEMO=1.5*SLS (non normativa). ENV racchiude ULS_DEMO e NEG: Is combination=True solo per il primo termine. Estremi di colonne diverse non sono simultanei.");
        var uls=g.Add("CombinationComponent","ULS_DEMO",("Name","ULS_DEMO"),("Cases",new[]{"SLS"}),("Factors",new[]{1.5}));
        var env=g.Add("CombinationComponent","ENV",("Name","ENV"),("Type",1),("Cases",new[]{"ULS_DEMO","NEG"}),("Factors",new[]{1d,1d}),("Is combination",new[]{true,false}));var model=BeamModel(g,b,sls,neg,uls,env);g.Preview(model);return g.Finish(1);
    }
    private static Example Nonlinear(Assembly a)
    {
        var g=new TopicGraph(a,"24_Analisi_non_lineare_PDelta","Definizione di un caso statico non lineare con P-Delta e lettura degli spostamenti.");var b=Beam(g,true);
        g.Step("NON LINEARE", "Caso NL=Q. NLGeomType=1 attiva P-Delta; per grandi spostamenti il valore e 2. Il materiale di questo esempio resta elastico.");
        var nl=g.Add("NonlinearCaseComponent","NL",("Name","NL"),("Patterns",new[]{"Q"}),("Factors",new[]{1d}));g.Link(b.Pattern,nl);
        var settings=g.Api("LoadCases.StaticNonlinear.SetGeometricNonlinearity",("NLGeomType",1));g.Link(nl,settings,"Name");var model=BeamModel(g,b,settings);g.Preview(model);
        var run=g.Execute(model,["NL"]);g.Step("RISULTATI", "Selezionare lo step coerente con il caso. Nessun risultato viene inventato prima del solver.");Reader(g,run,"Results.JointDispl","U1");return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static Example Buckling(Assembly a)
    {
        var g=new TopicGraph(a,"25_Buckling_autovalori","Colonna a mensola compressa: caso di buckling e fattori critici. Il fattore moltiplica il carico di riferimento.");var b=Beam(g,true);g.Set(b.Load,("Value",new double[]{0,0,-1,0,0,0}));
        g.Step("BUCKLING", "Caso BUCK = 1*Q. Non equivale a una verifica normativa di stabilita; valutare discretizzazione e condizioni al contorno.");
        var buck=g.Add("BucklingCaseComponent","BUCK",("Name","BUCK"),("Patterns",new[]{"Q"}),("Factors",new[]{1d}));g.Link(b.Pattern,buck);var model=BeamModel(g,b,buck);g.Preview(model);
        var run=g.Execute(model,["BUCK"],quantities:["Results.BucklingFactor"]);g.Step("FATTORI", "I valori e i numeri di modo arrivano dal solver SAP.");Reader(g,run,"Results.BucklingFactor","Factor");return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static Example Stages(Assembly a)
    {
        var g=new TopicGraph(a,"26_Fasi_costruttive","Due fasi: attivazione della struttura e applicazione del carico Q. Durate nulle, senza reologia.");var b=Beam(g);
        g.Step("CASO / FASI", "STAGED, due fasi con output. I componenti usano SetStageDefinitions_2 e SetStageData_2 dell'SDK locale.");
        var cs=g.Api("LoadCases.StaticNonlinearStaged.SetCase",("Name","STAGED"));
        var stages=g.Api("LoadCases.StaticNonlinearStaged.SetStageDefinitions_2",("NumberStages",2),("Duration",new[]{0d,0d}),("Output",new[]{true,true}),("OutputName",new[]{"BUILD","LOAD"}),("Comment",new[]{"Attivazione","Carico Q"}));g.Link(cs,stages,"Name");
        g.Step("OPERAZIONI", "Fase 1: Operation=1 aggiunge il gruppo ALL, eta 28 giorni. Fase 2: Operation=4 applica Q agli oggetti del gruppo ALL, fattore 1.");
        var add=g.Api("LoadCases.StaticNonlinearStaged.SetStageData_2",("Stage",1),("NumberOperations",1),("Operation",new[]{1}),("ObjectType",new[]{"Group"}),("ObjectName",new[]{"ALL"}),("Age",new[]{28d}),("MyType",new[]{"Load"}),("MyName",new[]{"Q"}),("SF",new[]{0d}));g.Link(stages,add,"Name");g.Link(b.Pattern,add);
        var load=g.Api("LoadCases.StaticNonlinearStaged.SetStageData_2",("Stage",2),("NumberOperations",1),("Operation",new[]{4}),("ObjectType",new[]{"Group"}),("ObjectName",new[]{"ALL"}),("Age",new[]{0d}),("MyType",new[]{"Load"}),("MyName",new[]{"Q"}),("SF",new[]{1d}));g.Link(add,load,"Name");var model=BeamModel(g,b,load);g.Preview(model);g.Execute(model,["STAGED"]);return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static GH_Component Modal(TopicGraph g,BeamParts b)
    {
        g.Step("MASSA / MODALE", "Massa unitaria al TIP; caso MODAL con tre modi richiesti. La massa include anche gli elementi. Periodi e masse partecipanti vanno controllati dopo il run.");var mass=AddMass(g,b.Tip);
        var modal=g.Api("LoadCases.ModalEigen.SetCase",("Name","MODAL"));g.Link(mass,modal);
        var modes=g.Api("LoadCases.ModalEigen.SetNumberModes",("MaxModes",3),("MinModes",1));g.Link(modal,modes,"Name");return modes;
    }
    private static Example ModalSpectrum(Assembly a)
    {
        var g=new TopicGraph(a,"27_Modale_spettro_risposta","Caso modale e spettro utente. Le ordinate spettrali sono didattiche e non rappresentano uno spettro normativo.");var b=Beam(g,true);var modal=Modal(g,b);
        g.Step("FUNZIONE SPETTRALE", "RS_DEMO: periodi 0, .1, .5, 1, 2, 4 s; ordinate normalizzate .1, .25, .25, .12, .06, .03. Smorzamento .05. Il fattore 9.81 converte le ordinate in m/s2.");
        var fn=g.Add("SpectrumFunctionComponent","RS_DEMO",("Name","RS_DEMO"),("NumberItems",6),("Period",new[]{0d,.1,.5,1,2,4}),("Value",new[]{.1,.25,.25,.12,.06,.03}),("DampRatio",.05));
        var cs=g.Api("LoadCases.ResponseSpectrum.SetCase",("Name","RSX"));g.Link(fn,cs);g.Link(modal,cs);
        var modes=g.Api("LoadCases.ResponseSpectrum.SetModalCase",("ModalCase","MODAL"));g.Link(cs,modes,"Name");
        var load=g.Api("LoadCases.ResponseSpectrum.SetLoads",("NumberLoads",1),("LoadName",new[]{"U1"}),("Func",new[]{"RS_DEMO"}),("SF",new[]{9.81}),("CSys",new[]{"Global"}),("Ang",new[]{0d}));g.Link(modes,load,"Name");
        var damp=g.Api("LoadCases.ResponseSpectrum.SetDampConstant",("Damp",.05));g.Link(load,damp,"Name");var model=BeamModel(g,b,damp);g.Preview(model);g.Execute(model,["MODAL","RSX"]);return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static Example TimeHistory(Assembly a)
    {
        var g=new TopicGraph(a,"28_Time_history_funzione_utente","Analisi dinamica modale lineare con impulso triangolare applicato al pattern Q.");var b=Beam(g,true);var modal=Modal(g,b);
        g.Step("STORIA TEMPORALE", "Funzione PULSE: (t, fattore) = (0,0), (.1,1), (.2,0), (2,0). Il fattore moltiplica il carico Q, non e un accelerogramma.");
        var fn=g.Add("TimeHistoryFunctionComponent","PULSE",("Name","PULSE"),("NumberItems",4),("MyTime",new[]{0d,.1,.2,2}),("Value",new[]{0d,1,0,0}));
        var cs=g.Api("LoadCases.ModHistLinear.SetCase",("Name","TH"));g.Link(fn,cs);g.Link(modal,cs);
        var use=g.Api("LoadCases.ModHistLinear.SetModalCase",("ModalCase","MODAL"));g.Link(cs,use,"Name");
        g.Step("CARICO / PASSO", "LoadType=Load, LoadName=Q. 200 passi da .01 s; scala temporale Tf=1 e arrivo At=0. Smorzamento modale 5%.");
        var load=g.Api("LoadCases.ModHistLinear.SetLoads",("NumberLoads",1),("LoadType",new[]{"Load"}),("LoadName",new[]{"Q"}),("Func",new[]{"PULSE"}),("SF",new[]{1d}),("Tf",new[]{1d}),("At",new[]{0d}),("CSys",new[]{"Global"}),("Ang",new[]{0d}));g.Link(use,load,"Name");g.Link(b.Pattern,load);
        var time=g.Api("LoadCases.ModHistLinear.SetTimeStep",("NStep",200),("Dt",.01));g.Link(load,time,"Name");var damp=g.Api("LoadCases.ModHistLinear.SetDampConstant",("Damp",.05));g.Link(time,damp,"Name");
        var model=BeamModel(g,b,damp);g.Preview(model);var run=g.Execute(model,["MODAL","TH"]);g.Step("LETTURA", "Usare Case e StepNum per distinguere gli istanti; non mescolare valori di passi diversi.");Reader(g,run,"Results.JointDispl","U1");return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
    private static Example Harmonic(Assembly a,bool psd)
    {
        string family=psd?"PSD":"SteadyState",caseName=psd?"PSD":"HARMONIC";
        var g=new TopicGraph(a,psd?"42_Densita_spettrale_PSD":"29_Risposta_armonica",psd?"Caso PSD con funzione utente costante in frequenza. Le ordinate sono una densita spettrale, non ampiezze deterministiche.":"Risposta armonica in frequenza con carico di ampiezza unitaria e fase zero.");var b=Beam(g,true);var modal=Modal(g,b);
        g.Step("FUNZIONE IN FREQUENZA", "Tre punti a .1, 5, 10 Hz; ordinate unitarie. Esempio di collegamento API, da dimensionare per il problema dinamico reale.");
        var fn=g.Add(psd?"PowerSpectrumFunctionComponent":"SteadyStateFunctionComponent","FREQ",("Name","FREQ"),("NumberItems",3),("Frequency",new[]{.1,5,10}),("Value",new[]{1d,1,1}));
        var cs=g.Api("LoadCases."+family+".SetCase",("Name",caseName));g.Link(fn,cs);g.Link(modal,cs);
        var load=g.Api("LoadCases."+family+".SetLoads",("NumberLoads",1),("LoadType",new[]{"Load"}),("LoadName",new[]{"Q"}),("Func",new[]{"FREQ"}),("SF",new[]{1d}),("PhaseAngle",new[]{0d}),("CSys",new[]{"Global"}),("Ang",new[]{0d}));g.Link(cs,load,"Name");g.Link(b.Pattern,load);
        g.Step("FREQUENZE", "Da .1 a 10 Hz, 50 incrementi, aggiunta della frequenza specificata 1 Hz. Nessuna aggiunta automatica dei modi o deviazioni modali.");
        var freq=g.Api("LoadCases."+family+".SetFreqData",("FreqFirst",.1),("FreqLast",10d),("FreqNumIncs",50),("FreqAddModal",false),("FreqAddModalDev",false),("FreqAddSpecified",true),("ModalCase","MODAL"),("FreqNumModalDev",0),("FreqModalDev",new[]{0d}),("FreqNumSpecified",1),("FreqSpecified",new[]{1d}));g.Link(load,freq,"Name");var model=BeamModel(g,b,freq);g.Preview(model);g.Execute(model,["MODAL",caseName]);return g.Finish(1,mode:"Analisi SAP su richiesta");
    }
}
