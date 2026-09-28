# Rhino2FEM — plugin strutturali

La cartella contiene quattro progetti autonomi per **Rhino 8 / Grasshopper, Windows x64**:

| Progetto | Solver | Guida |
|---|---|---|
| **Rhino2Straus** | Straus7 R3 API | [README](Rhino2Straus/README.md) |
| **Rhino2SAP** | SAP2000 26 API | [README](Rhino2SAP/README.md) |
| **Rhino2Midas** | MIDAS Civil NX REST API | [README](Rhino2Midas/README.md) |
| **Rhino2MidasGen** | MIDAS GEN NX REST API | [README](Rhino2MidasGen/README.md) |

Ogni progetto ha la propria soluzione, il proprio `build.ps1`, test, documentazione e pacchetto in `artifacts`. I quattro plugin condividono l'impostazione: materiali/proprietà → elementi/attributi/carichi → Model → preview/export/analisi → risultati incorporati.

Rhino2Midas e Rhino2MidasGen usano direttamente le API dei rispettivi prodotti per definizioni, salvataggio, analisi e risultati; la precedente gestione MGT/MCT è rimossa. GEN aggiunge pareti, piani e diaframmi. I due plugin hanno connessioni, assembly e GUID distinti e possono convivere in Grasshopper. Il collaudo numerico richiede connessioni API attive: [Civil](Rhino2Midas/docs/VALIDATION.md), [GEN](Rhino2MidasGen/docs/VALIDATION.md).

I sorgenti legacy e i vecchi binari sono rimossi dalla cartella di lavoro. I dettagli della copia di recupero esterna sono in [CLEANUP](Rhino2Midas/docs/CLEANUP.md). I quattro progetti non referenziano le vecchie cartelle.

Rhino2Straus conserva il proprio repository come sottomodulo. Per scaricare tutti i sorgenti:

```powershell
git clone --recurse-submodules https://github.com/ITEC-GPA/Rhino2FEM.git
# Per un clone già esistente:
git submodule update --init --recursive
```

La cartella principale e il repository GitHub si chiamano **Rhino2FEM**. Il repository è privato: per clonarlo occorre autenticarsi con un account autorizzato. I pacchetti compilati in `artifacts` sono generati dai rispettivi `build.ps1` e non vengono versionati.

Il controllo congiunto del 27 settembre 2026 copre **1.634 componenti e 10.850 ingressi**, con tutti i plugin caricati nella stessa sessione Rhino/Grasshopper. Inventari, default e limiti delle prove: [Straus](Rhino2Straus/docs/INPUT-AUDIT.md), [SAP](Rhino2SAP/docs/INPUT-AUDIT.md), [Civil](Rhino2Midas/docs/INPUT-AUDIT.md), [GEN](Rhino2MidasGen/docs/INPUT-AUDIT.md).


Quattro definizioni Grasshopper con slider e componenti collegati sono disponibili nella [guida agli esempi](ESEMPI-GRASSHOPPER.md): mensola Straus, portale SAP2000, trave Civil NX e parete GEN NX. I file .gh/.ghx si trovano nelle cartelle examples dei rispettivi progetti e si possono esplorare senza avviare i solver.
