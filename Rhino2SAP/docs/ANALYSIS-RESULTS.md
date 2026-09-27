# Analisi, Model e risultati negli elementi

`SAP Analyze and Embed Results` e `Run SAP Model Cases and Combinations` eseguono:

1. Validazione gestita; avvio di un nuovo SAP nel worker.
2. Modello vuoto, unità, materiali, sezioni, proprietà, pattern, nodi e connettività.
3. Attributi, carichi, casi e combinazioni. Ogni codice di ritorno diverso da zero diventa un errore con il nome del metodo.
4. Salvataggio `.sdb`, selezione dei casi e `Analyze.RunAnalysis`.
5. `Analyze.GetCaseStatus`: ogni caso richiesto deve risultare **4 = Finished**.
6. Selezione dei casi/combinazioni per output e lettura dei risultati, senza sostituire errori con zeri.
7. Incorporazione delle tabelle nel Model e associazione di righe specifiche a ogni `Element.Results`.
8. Pubblicazione dei file, manifest e archivio `.sdb.results.json`.

## Output

| Output | Contenuto |
|---|---|
| Model | Definizione completa e `ResultSet`, con dati anche nei singoli elementi |
| Path | File SAP salvato |
| Issues | Quantità richieste ma non lette |
| Beams | Frame con `Element.Results`, isolabili direttamente |
| Plates | Area con `Element.Results`, isolabili direttamente |

`Decompose SAP Model` restituisce nodi, frame, area, solidi, link, cable, operazioni, unità e tabelle. **Decompose SAP Beam/Plate** espone geometria, nome, proprietà, vertici, tabelle, quantità, casi e problemi del singolo elemento.

## Interrogazione

- **SAP Beam Results N V T M**: N=P, V2, V3, T, M2, M3 e ObjSta; nomi dei casi e passi su uscite parallele.
- **SAP Plate Results Forces and Moments**: F11, F22, F12, M11, M22, M12, V13, V23.
- **SAP Plate Results Stresses / Strains**: superfici top/bottom e componenti di taglio native.
- **Query SAP Element Results**: qualsiasi tabella incorporata, con filtro caso/combinazione e passo esatto. L'albero ha una branca `{riga}` e mantiene le colonne nell'ordine SDK.
- **SAP Model …**: componenti specifici per tutte le 40 quantità `Results` dell'SDK, sul Model complessivo; colonne parallele comprendenti sempre gli identificativi nativi.
- **SAP Model Result Cases / Extrema**, **Frame Force Diagram**, **Deformed Geometry**, **Verification Data**.
- **Read SAP API Table**: lettura esplicita di un metodo `Get…` o `Results…` da un `.sdb`, per sezioni native, proprietà effettive o query avanzate. Le mutazioni non sono ammesse.

`Quantities` in Run consente la scelta delle quantità. Se vuoto, sono lette: spostamenti/reazioni nodali; forze e azioni nodali frame; forze, momenti, tensioni, deformazioni e azioni nodali shell; tensioni solidi; forze link. Modelli con casi modali aggiungono periodi, partecipazioni e forme modali; il buckling aggiunge i fattori. La disponibilità reale dipende dal tipo di elemento e dal caso. Le quantità mancanti sono riportate in Issues.

Le query che richiedono argomenti speciali oltre a `Name=ALL` e `ItemTypeElm=GroupElm` si eseguono tramite Read SAP API Table, fornendo gli argomenti JSON documentati.

## Integrità

L'impronta SHA256 include unità, tolleranza, geometria, proprietà, carichi e casi. I risultati non entrano nell'impronta della definizione. `WithResults` verifica l'associazione e suddivide le tabelle per identità dell'oggetto: un beam non riceve le righe di un altro beam. Le tabelle di spostamento nodale sono associate ai vertici pertinenti mediante le coordinate e i nomi reali esportati.

Build, modifiche agli attributi, riallineamento e Clear Results eliminano i risultati obsoleti, anche dagli elementi. Le copie in sola lettura e i decompositori li conservano. Model ed elementi con risultati possono essere serializzati; i Goo Grasshopper conservano i dati persistenti salvati nei parametri.

Il manifest e l'archivio registrano l'hash del `.sdb`. `Read SAP Results Into Model` legge lo snapshot JSON salvato dal plugin, verifica impronta e hash del file. Non rilegge un file risolto esternamente senza questo archivio; per dati esterni usare le query di sola lettura.

I risultati rimangono una rappresentazione del solver: nessun componente esegue ancora verifiche normative. Le componenti di un inviluppo non sono necessariamente simultanee. La deformata rifiuta gli step Max/Min e interpola soltanto i vertici/estremi degli elementi.
