# Rhino2Midas

Plugin **Grasshopper per Rhino 8 / .NET 8 / Windows x64**, dedicato a **MIDAS Civil NX con API REST**. Riprende l'organizzazione di Rhino2SAP e Rhino2Straus: definizioni immutabili, Model unico, attributi, anteprima, Brep e bake, export, analisi, risultati incorporati e strumenti di interrogazione.

**434 componenti in 30 pannelli per argomento**, con icone a 24 px. Il catalogo include 345 componenti nativi di definizione e richiesta risultati.

Il trasferimento avviene direttamente tramite `/db`, `/doc` e `/post/TABLE`. **Non viene creato o importato alcun file MGT/MCT.** Civil classico senza API NX non è supportato.

[Componenti](docs/COMPONENTS.md) · [Corrispondenze con gli altri progetti](docs/PARITY.md) · [Preview](docs/PREVIEW.md) · [Analisi e risultati](docs/ANALYSIS-RESULTS.md) · [Collaudo e limiti](docs/VALIDATION.md)

## Installazione

1. Usare `artifacts/Rhino2Midas.zip`, oppure eseguire `./build.ps1`.
2. Copiare **il contenuto di bin (o i file nella radice dello ZIP)** in una cartella Rhino2Midas in Grasshopper → File → Special Folders → Components Folder. Sono necessari il `.gha` e gli assembly Core e Api.
3. Sbloccare i file scaricati e riavviare Rhino 8.27 o successivo in modalità .NET 8.
4. In Civil NX aprire **Apps → API Settings**, ottenere Base URL e MAPI-Key e attivare **Connect**. Civil NX deve essere sullo stesso computer di Rhino per il salvataggio e la verifica dei file.
5. Inserire `Midas API Connection` in Grasshopper, tasto destro → **Configure Civil NX API**. La chiave resta in memoria fino alla chiusura di Rhino. In alternativa impostare `MIDAS_API_URL` e `MIDAS_API_KEY` nell'ambiente del processo Rhino.

Non distribuire chiavi nei pannelli o nei file `.gh`. Il plugin non le salva nei propri archivi. Le librerie proprietarie Rhino/Grasshopper/MIDAS non sono incluse nel pacchetto.

## Flusso

```mermaid
flowchart LR
    A[Materiali e sezioni] --> B[Elementi, attributi e carichi]
    B --> C[Build Midas Model]
    C --> D[Preview Midas Model]
    D --> E[Breps e bake]
    C --> F[Midas Analyze and Embed Results]
    F --> G[Model con risultati]
    G --> H[Decompose Midas Model]
    H --> I[Azioni, diagrammi e verifiche]
```

I componenti di modellazione non richiedono Civil aperto. Collegare definizioni di materiale e proprietà agli elementi conserva le dipendenze; gli ingressi accettano anche ID numerici, purché le definizioni corrispondenti confluiscano nel Model. `Build Midas Model` salda i nodi impliciti entro la tolleranza e conserva i nodi espliciti, anche coincidenti. Gli ID di nodi, elementi e link elastici appartengono a spazi distinti. Gli attributi producono copie e invalidano i risultati.

`Merge Midas Models` richiede unità e tolleranza uguali e ID compatibili. Una dipendenza condivisa non raddoppia i carichi; contributi creati separatamente restano distinti. Gli ID non vengono rinumerati automaticamente: i riferimenti degli oggetti API avanzati devono rimanere coerenti.

Usare `Midas Cantilever Example` per l'esempio offline, oppure leggere [examples/Cantilever.model.json](examples/Cantilever.model.json) con Read File → Midas Model from JSON. La visualizzazione espone solidi chiusi anche a preview spenta.

## Export e calcolo

Collegare un Model a `Export Midas Civil Model` oppure `Midas Analyze and Embed Results`, impostare un percorso `.mcb` e portare `Run` da false a true. Salvare prima il proprio lavoro in Civil e abilitare **Replace active document**: l'API crea un nuovo documento nella sessione Civil connessa. **Overwrite** è disattivato per default.

Le modifiche agli input non avviano automaticamente il solver. Il calcolo è sincrono; il timeout HTTP è di 30 minuti. Non vengono ripetute automaticamente le chiamate fallite. Dopo un timeout controllare Civil, dove l'operazione può essere ancora in corso. Un errore può lasciare il documento Civil parzialmente compilato; il salvataggio nativo non è una transazione atomica.

Il collaudo numerico sul solver Civil NX **non è stato eseguito in questa sessione**, perché manca una connessione API configurata. Build, test gestiti e verifiche Rhino sono documentati in [VALIDATION](docs/VALIDATION.md).

## Unità

Tutti gli ingressi dimensionali usano le unità del Model. Default **KN, M, KJ, C**. Le coordinate Rhino non vengono convertite. Modulo elastico e tensioni: F/L²; peso specifico: F/L³; carichi distribuiti: F/L; momenti: F·L. Le risultanti piastra per unità di lunghezza hanno unità F/L e F·L/L. Le masse usano unità coerenti F·s²/L. Gli angoli di orientamento sono in gradi, le rotazioni di risultato in radianti.

In assenza di una definizione `STYP`, viene impostata una struttura 3D con gravità 9,806 m/s² convertita nell'unità di lunghezza e senza trasformare automaticamente il peso proprio in massa. Collegare il componente nativo Structure Type per impostazioni diverse.

## Sviluppo

```powershell
./build.ps1 -RhinoSmoke
./tools/Update-ApiCatalogue.ps1
./tools/Generate-Components.ps1
dotnet run --project Rhino2Midas.Tests -c Release
dotnet run --project tools/PluginSmoke -c Release
```

Il catalogo è ricavato dal [manuale ufficiale MIDAS API](https://support.midasuser.com/hc/en-us/articles/33016922742937-MIDAS-API-Online-Manual). Ogni componente nativo restituisce il link della propria pagina. I valori iniziali sono **esempi della documentazione**, da adattare agli ID, alle unità e alle dipendenze del modello; non sono valori raccomandati per il progetto. Le funzioni disponibili dipendono dalla versione e dalla licenza Civil NX.

## Compilazione e pulizia

`./build.ps1` crea nella directory di questo progetto una sola cartella **`bin`** pronta per Grasshopper: un file `.gha` e le DLL/runtime necessari. Non cambiare i GUID per risolvere caricamenti doppi.

La build elimina i vecchi `bin/obj` intermedi, ricrea il contenuto di `bin`, esegue i test e rimuove di nuovo gli intermedi anche in caso di errore. Conserva in `artifacts` lo ZIP della versione corrente; elimina i vecchi ZIP del plugin e le vecchie distribuzioni estratte. Gli archivi degli esempi restano disponibili. Documentazione ed esempi sono nei sorgenti e nello ZIP, senza duplicati nella cartella `bin`.

- `./clean.ps1 -WhatIf`: mostra cosa verrebbe rimosso.
- `./clean.ps1`: pulisce gli intermedi e i vecchi pacchetti, conservando `bin`.
- `./build.ps1 -KeepBuildOutputs`: conserva gli intermedi per debug/sviluppo; al termine usare `./clean.ps1`.
- `./build.ps1 -SkipTests`: compila e pulisce saltando i test gestiti.

Caricare **solo questa cartella `bin`** tramite GrasshopperDeveloperSettings, oppure copiarne l'intero contenuto in un'unica cartella delle Libraries di Grasshopper. Usare una sola delle due modalità e sostituire l'installazione precedente: caricare contemporaneamente copie installate, `bin` e vecchi pacchetti genera duplicati. Le DLL accanto al `.gha` sono necessarie. Chiudere Rhino prima di ricompilare se i file sono in uso.

Gli strumenti Rhino.Inside di verifica/generazione esempi leggono il plugin da `bin`: eseguire prima `./build.ps1`. Dopo l'uso dei tool, `./clean.ps1` rimuove anche i loro intermedi.

## Tutorial e importazione

La [raccolta tematica](examples/tutorials/README.md) contiene file .gh/.ghx con geometrie, collegamenti e istruzioni. Il tutorial 11 legge il documento MIDAS attivo e restituisce il Model: Tables in ingresso è opzionale; Tables in uscita elenca le tabelle ricevute e Issues espone i limiti. [Guida importazione](docs/IMPORT.md).