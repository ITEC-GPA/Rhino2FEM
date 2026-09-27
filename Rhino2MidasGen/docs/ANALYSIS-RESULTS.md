# Analisi e risultati

`Analyze` valida il Model, crea il documento, imposta unità/tipo struttura, scrive le definizioni e la geometria tramite API, salva il `.mgb`, esegue `/doc/ANAL`, legge le tabelle, salva GEN e associa i risultati al Model. Non usa MGT/MCT né file di testo per trasferire le tabelle.

Per default richiede spostamenti e reazioni globali, forze trave, risultanti piastra per unità di lunghezza e tensioni, tensioni dei solidi, azioni/tensioni delle aste e azioni dei link presenti. I casi sono STLD `(ST)` e combinazioni generali additive attive `(CB)`. Le richieste piastra non mediano i risultati ai nodi.

Collegare i componenti **Result Request** all'ingresso `Requests` per scegliere quantità, ID, casi, stazioni, fasi e opzioni. Una lista esplicita sostituisce le richieste automatiche. I nomi di richiesta seguono MIDAS, per esempio `LC1(ST)`, `ULS(CB)`, `Summation(CS)`. Le etichette restituite nella colonna Load possono essere diverse, per esempio `LC1`: i lettori usano esattamente quelle restituite. Le tabelle modali/globali possono omettere ID e casi.

Gli output sono Model, Beams, Plates e Issues. `Decompose Midas GEN Model` espone tutte le famiglie e le definizioni; gli elementi conservano le proprie righe di risultato. `Beam Actions`, `Plate Actions`, `Node Displacements/Reactions`, `Truss Actions`, `Solid Stress` e `Elastic Link Actions` leggono le grandezze dedicate; `Query Midas GEN Element Results` accede alle altre colonne. Ogni tabella conserva identificativi, caso, fase, passo e posizione restituiti dall'API.

Le tabelle mancanti o fallite sono elencate in Issues; l'esecuzione fallisce se non torna alcuna riga utilizzabile. `AnalysisCompleted` indica che la chiamata ANAL è riuscita e sono tornati risultati: **non certifica la convergenza di ogni caso richiesto**. L'API Project Status elenca dati del modello e non è usata come attestazione di convergenza. Controllare sempre messaggi e log nativi GEN, specialmente nelle analisi avanzate.

Quando sono presenti pareti, la richiesta automatica aggiunge `WALL_FORCE_MOMENT` per tutti i piani. Le righe si associano al campo nativo WALL, distinto dal numero dell'elemento. Ogni elemento dello stesso gruppo conserva le risultanti dell'intero gruppo nei diversi piani; **non sommarle fra elementi della stessa parete**. `Wall Actions` filtra il piano e distingue le colonne ripetute dei blocchi top/bottom. Nelle query generiche il nome della colonna seleziona la prima occorrenza; usare il lettore dedicato per il blocco inferiore.

Per i risultati di edificio collegare le richieste native Story (drift, forze, spostamenti e altre tabelle documentate). Alcune richieste non hanno COMPONENTS: il plugin conserva questa convenzione. Adattare Story names, casi e opzioni presenti negli esempi ufficiali. Non si interpretano automaticamente parametri normativi o risultati di verifica.

## Archivi

- `nome.mgb`: modello nativo GEN NX.
- `nome.mgb.model.json`: definizioni e risultati Rhino2MidasGen, senza credenziali.
- `nome.mgb.results.json`: risultati, unità, fingerprint delle definizioni e SHA256 del file GEN.

`Read Midas GEN Results into Model` rifiuta fingerprint/unità differenti e un file GEN assente o modificato. I risultati in un archivio Model o in un file GH sono una fotografia dichiarata del calcolo salvato; non richiedono di riaprire GEN. Una modifica geometrica, un attributo o Merge rimuove i risultati. Il salvataggio dei sidecar è atomico, quello del file nativo è gestito da GEN e può lasciare dati parziali in caso di errore.

`Read Active Midas GEN Model` legge UNIT/NODE/ELEM e le tabelle aggiuntive indicate. È un import del sottoinsieme richiesto: includere materiali, proprietà, link, carichi e impostazioni necessarie. I tipi di elemento sconosciuti sono conservati come record API senza inventarne una geometria. `Read Midas GEN Native Result Table` legge il documento attivo senza attribuirlo automaticamente a un Model Grasshopper.

## Post-processing

La deformata richiede un unico stato coerente per nodo e rifiuta etichette max/min e stati duplicati. I diagrammi trave richiedono una singola trave e una tabella filtrata sullo stato desiderato. `Result Extrema` conserva le righe governanti: estremi di componenti diverse non sono necessariamente simultanei. `Verification Data` include unità, geometria, record nativi, risultati e Issues per un successivo verificatore.
