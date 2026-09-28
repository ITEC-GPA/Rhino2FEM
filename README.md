# Rhino2FEM — plugin strutturali

La cartella contiene quattro progetti autonomi per **Rhino 8 / Grasshopper, Windows x64**:

| Progetto | Solver | Guida |
|---|---|---|
| **Rhino2Straus** | Straus7 R3 API | [README](Rhino2Straus/README.md) |
| **Rhino2SAP** | SAP2000 26 API | [README](Rhino2SAP/README.md) |
| **Rhino2Midas** | MIDAS Civil NX REST API | [README](Rhino2Midas/README.md) |
| **Rhino2MidasGen** | MIDAS GEN NX REST API | [README](Rhino2MidasGen/README.md) |

Ogni progetto ha la propria soluzione, il proprio `build.ps1`, test, documentazione, plugin compilato in `bin` e ZIP in `artifacts`. I quattro plugin condividono l'impostazione: materiali/proprietà → elementi/attributi/carichi → Model → preview/export/analisi → risultati incorporati.

Rhino2Midas e Rhino2MidasGen usano direttamente le API dei rispettivi prodotti per definizioni, salvataggio, analisi e risultati; la precedente gestione MGT/MCT è rimossa. GEN aggiunge pareti, piani e diaframmi. I due plugin hanno connessioni, assembly e GUID distinti e possono convivere in Grasshopper. Il collaudo numerico richiede connessioni API attive: [Civil](Rhino2Midas/docs/VALIDATION.md), [GEN](Rhino2MidasGen/docs/VALIDATION.md).

I sorgenti legacy e i vecchi binari sono rimossi dalla cartella di lavoro. I dettagli della copia di recupero esterna sono in [CLEANUP](Rhino2Midas/docs/CLEANUP.md). I quattro progetti non referenziano le vecchie cartelle.

Rhino2Straus conserva il proprio repository come sottomodulo. Per scaricare tutti i sorgenti:

```powershell
git clone --recurse-submodules https://github.com/ITEC-GPA/Rhino2FEM.git
# Per un clone già esistente:
git submodule update --init --recursive
```

La cartella principale e il repository GitHub si chiamano **Rhino2FEM**. Il repository è privato: per clonarlo occorre autenticarsi con un account autorizzato. Le cartelle `bin` e i pacchetti ZIP in `artifacts` sono generati dai rispettivi `build.ps1` e non vengono versionati.

Il controllo congiunto del 27 settembre 2026 copre **1.634 componenti e 10.850 ingressi**, con tutti i plugin caricati nella stessa sessione Rhino/Grasshopper. Inventari, default e limiti delle prove: [Straus](Rhino2Straus/docs/INPUT-AUDIT.md), [SAP](Rhino2SAP/docs/INPUT-AUDIT.md), [Civil](Rhino2Midas/docs/INPUT-AUDIT.md), [GEN](Rhino2MidasGen/docs/INPUT-AUDIT.md).


Quattro definizioni Grasshopper con slider e componenti collegati sono disponibili nella [guida agli esempi](ESEMPI-GRASSHOPPER.md): mensola Straus, portale SAP2000, trave Civil NX e parete GEN NX. I file .gh/.ghx si trovano nelle cartelle examples dei rispettivi progetti e si possono esplorare senza avviare i solver.

## Compilare e mantenere una sola copia dei plugin

Dalla radice Rhino2FEM:

```powershell
./build.ps1           # compila i quattro progetti, esegue i test e pulisce
./clean.ps1 -WhatIf   # anteprima della sola pulizia
./clean.ps1           # pulizia senza ricompilare, conserva i quattro bin finali
```

Ogni progetto resta autonomo: gli stessi comandi sono disponibili nella sua cartella. Dopo la compilazione le uniche cartelle con plugin caricabili sono:

| Plugin | Cartella finale |
|---|---|
| Straus | `Rhino2Straus/bin` |
| SAP2000 | `Rhino2SAP/bin` |
| Civil NX | `Rhino2Midas/bin` |
| GEN NX | `Rhino2MidasGen/bin` |

Ogni `bin` contiene un solo `.gha` e le DLL/runtime necessari, incluso il Worker per SAP. I `bin/obj` interni, quelli dei test e dei tool e le vecchie distribuzioni estratte vengono rimossi. Gli ZIP di distribuzione rimangono in `artifacts`, con documentazione ed esempi; si conserva solo la versione corrente del plugin. Gli ZIP degli esempi non vengono eliminati.

Per Grasshopper scegliere **una sola modalità**: aggiungere i quattro percorsi `bin` in GrasshopperDeveloperSettings, oppure copiarne il contenuto in quattro cartelle delle Libraries. Rimuovere i riferimenti a percorsi precedenti e non caricare contemporaneamente copie installate e copie locali. Non aggiungere l'intera cartella Rhino2FEM come percorso ricorsivo. I GUID dei componenti rimangono invariati.

La pulizia controlla che tutte le destinazioni siano interne al progetto e non attraversa collegamenti/junction. Chiudere Rhino se tiene bloccati gli assembly. Per conservare gli intermedi durante lo sviluppo usare `./build.ps1 -KeepBuildOutputs`, poi `./clean.ps1`. Il comando diretto `dotnet build` produce gli intermedi standard: per il pacchetto finale pulito usare `build.ps1`.

Verifica della pulizia in cartelle temporanee, inclusa la protezione da junction: `./Rhino2MidasGen/tools/Test-Cleanup.ps1`.