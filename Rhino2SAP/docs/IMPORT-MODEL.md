# Importare un modello SAP esistente

**Import SAP Model**, nel gruppo **26 Analisi e file**, legge un `.sdb` esistente e restituisce un `Model` Grasshopper associato a quella versione del file. Il componente precedente **Read SAP Geometry** rimane disponibile per la sola geometria.

1. Collegare il percorso del file a `Path`.
2. Portare `Run` da False a True. Per aggiornare una lettura, tornare prima a False.
3. Collegare `Model` a Preview, Decompose, Export oppure Analyze and Embed Results.
4. Leggere `Units`, `Cases`, `Combinations`, `Issues` e `Source hash`.

[Esempio 32](../examples/32_Import_modello_SAP_associato.gh) · [Guida dell'esempio](../examples/32_Import_modello_SAP_associato.md).

## Contenuto e conservazione dei dati

La vista Grasshopper ricostruisce nomi, coordinate, connettivita e riferimenti alle proprieta di nodi, frame, area, solidi, link e cable. Legge le proprieta geometriche supportate, i materiali isotropi e le assegnazioni comuni: vincoli, molle, assi, rilasci, modificatori, offset, forze nodali, carichi frame distribuiti/puntuali e pressioni uniformi sulle area. Gli elementi non supportati nella vista, per esempio i tendon, e gli errori di lettura delle proprieta sono segnalati in `Issues`. Non viene inventato un solido per una sezione che il plugin non sa ricostruire.

Il modello SAP nativo rimane **la sorgente completa** per materiali avanzati, sezioni, carichi, casi, combinazioni, mesh del solver e altre definizioni. Export e analisi aprono una copia privata verificata di quel file: non ricostruiscono un modello vuoto a partire dalla sola vista GH. La disponibilita delle preview non determina la conservazione dei dati SAP.

L'associazione e di sola lettura: per modificare il modello importato, modificarlo in SAP e reimportarlo. Il plugin rifiuta aggiunte di comandi, cambi di unita, ricostruzioni da sottoinsiemi o unioni di sorgenti diverse che eliminerebbero silenziosamente il legame nativo. Decomposizione, filtri e cast geometrici restano utilizzabili per ispezione. Un modello creato originariamente in Grasshopper mantiene il consueto flusso parametrico modificabile.

## File, unita e risultati

- Il sorgente viene aperto tramite una copia temporanea e non viene salvato. L'export richiede un percorso diverso dal sorgente anche con `Overwrite=true`.
- Il file deve rimanere disponibile: il Model conserva percorso e SHA256, non incorpora l'intero SDB nel documento GH. Se il file cambia, export e analisi chiedono una nuova importazione.
- Le coordinate mantengono le unita correnti del file SAP. `Units` espone il codice `eUnits`; non avviene una conversione automatica nelle unita del documento Rhino.
- L'analisi puo selezionare i casi e le combinazioni originali; se gli input restano vuoti li usa tutti. I risultati vengono incorporati nel Model e nei beam/plate attraverso lo stesso flusso dei modelli costruiti in GH.
- L'importatore non acquisisce automaticamente i risultati gia presenti nel file. Per questi, usare **Read SAP API Table**; per nuovi risultati incorporati, collegare il Model importato ad **Analyze and Embed Results** e scegliere una nuova destinazione.
- **SAP File Paths** deriva il percorso `.sdb.results.json` dal file prodotto dal run, cosi **Read SAP Results Into Model** segue automaticamente le modifiche del percorso di analisi.

## Verifica

Il flusso di importazione, la conservazione della sorgente, i controlli hash, la serializzazione, le unita e il riuso del file nativo sono verificati con un client API simulato. I componenti e i collegamenti sono verificati anche in Rhino/Grasshopper. Il collaudo con il solver nativo resta distinto: vedere [VALIDATION](VALIDATION.md). Questi controlli non dimostrano la correttezza numerica di un'analisi strutturale.
