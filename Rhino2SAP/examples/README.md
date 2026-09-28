# Esempi Grasshopper — Rhino2SAP

Tre definizioni con componenti Rhino2SAP realmente collegati, dati geometrici incorporati, gruppi colorati e note sul canvas. Non serve un modello `.3dm` esterno. Ogni esempio è disponibile in formato **`.gh`** (da aprire normalmente) e **`.ghx`** (lo stesso documento in XML), con un'anteprima PNG del canvas.

Richiedono Rhino 8 / .NET 8 e l'intera cartella del plugin Rhino2SAP installata. Aprire **un solo esempio alla volta**, quindi eseguire Zoom Extents nel viewport Rhino. Le unità dei modelli sono **kN, m, C**: impostare il documento Rhino in metri per una lettura coerente della geometria.

## 01 — Trave, carico, preview e bake

[Apri 01_Trave_carico_preview_bake.gh](01_Trave_carico_preview_bake.gh) · [Canvas](01_Trave_carico_preview_bake.png)

Mensola lunga 3 m, sezione rettangolare 0,20 × 0,10 m, acciaio E = 210·10⁶ kN/m². Nodo BASE incastrato; forza di −10 kN in Z globale sul nodo TIP. Peso proprio disattivato nel load pattern.

Flusso: materiale → sezione → frame; nodi → incastro/carico → Build Model → validazione, riepilogo, preview e Brep. Due slider regolano altezza e larghezza. Il pannello numerico del carico contiene, nell'ordine, **FX, FY, FZ, MX, MY, MZ**, una componente per riga.

Provare a cambiare le dimensioni della sezione e a spegnere `Mostra carichi`. Per inserire il solido in Rhino, usare Bake sul parametro **Solidi Brep**, oppure portare `BAKE SOLIDI` da False a True. Dopo il bake riportarlo a False prima di un nuovo impulso. Il modello geometrico iniziale produce un Brep chiuso di 0,06 m³. L'asse del frame e i nodi sono dati incorporati distinti: se se ne modificano le coordinate, mantenerle coerenti.

## 02 — Piastra mesh e pressione

[Apri 02_Piastra_mesh_pressione.gh](02_Piastra_mesh_pressione.gh) · [Canvas](02_Piastra_mesh_pressione.png)

Piastra di 4 × 4 m, discretizzata in quattro shell quadrangolari. Spessore iniziale 0,20 m, E = 30·10⁶ kN/m², ν = 0,20. Gli otto nodi di bordo sono incastrati; il nodo centrale è libero. La pressione iniziale è −5 kN/m² lungo Z globale.

Mostra l'uso di **Mesh to SAP Areas**, assegnazione uniforme al gruppo `ALL`, nodi multipli, Build, decomposizione e interrogazione dei nomi delle plate. Gli slider regolano pressione e spessore. `Thickness` e `Bending` sono collegati allo stesso valore. L'uscita Breps contiene quattro solidi separati: volume totale iniziale 3,2 m³.

La mesh è deliberatamente grossolana per rendere leggibili i collegamenti. Questo esempio serve a costruire e visualizzare il modello; non contiene un'analisi già eseguita né una verifica di convergenza.

## 03 — Analisi assiale e risultati

[Apri 03_Analisi_assiale_risultati.gh](03_Analisi_assiale_risultati.gh) · [Canvas](03_Analisi_assiale_risultati.png)

La stessa mensola del primo esempio è caricata con **+1 kN in X**. Il canvas mostra anche il caso lineare `SLS`, la combinazione didattica `ULS_DEMO = 1,5 SLS`, il componente di analisi e i lettori dei risultati.

1. Nel pannello **Path**, scegliere il percorso di un nuovo file `.sdb` scrivibile. `Overwrite` è disattivato.
2. Portare **RUN SAP** da False a True. Il plugin deve poter avviare SAP2000 tramite API.
3. Consultare `Issues analisi`, il pannello N, lo spostamento del nodo TIP, il diagramma assiale e le righe native della query per `ULS_DEMO`.
4. Per rilanciare il calcolo dopo una modifica, riportare RUN SAP a False e poi a True; scegliere un nuovo file o attivare consapevolmente Overwrite.

Per i valori iniziali, il riferimento elastico assiale è **Ux = FL/(EA) = 7,142857·10⁻⁷ m** e **N = +1 kN** nel caso SLS. Per ULS_DEMO i valori sono moltiplicati per 1,5. Sono valori teorici di confronto, non risultati del solver memorizzati nel file. I nomi SLS/ULS_DEMO e il fattore 1,5 hanno solo scopo didattico e non definiscono combinazioni normative.

I componenti a valle dell'analisi restano senza dati fino al primo run completato e mostrano `Waiting for input`. Usare il plugin aggiornato insieme agli esempi per questa gestione dell'attesa. Nessun risultato sintetico viene presentato come risultato SAP. Il collaudo numerico del solver resta distinto dal controllo dei file: nella precedente sessione SAP si arrestava durante `ApplicationStart`.

## Controlli e rigenerazione

`validation.json` registra i controlli eseguiti dal generatore: salvataggio e riapertura di `.gh` e `.ghx`, conservazione di oggetti e fili, validazione dei modelli, Brep chiusi e trigger Run/Bake disattivati. Queste verifiche non avviano SAP e non effettuano bake.

Dal repository, per rigenerare file e anteprime con Rhino installato:

```powershell
./build.ps1
./tools/Generate-Examples.ps1
```
