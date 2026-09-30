# Verifiche

Ambiente: Windows x64, .NET SDK 9.0.318, target .NET 8, Rhino 8 / Grasshopper 8.27, SAP2000 26 installato. Aggiornamento: 30 settembre 2026.

## Completate

- Build Release dell'intera soluzione senza errori né warning.
- **54 verifiche gestite**: firme di tutte le 2.047 funzioni dello schema contro la DLL CSI locale; unità, nomi, saldatura nodi, connettività, dipendenze, carichi condivisi/additivi, ordine di esportazione, fingerprint e archivi; importazione con API simulata, conservazione del file nativo e controlli sulla sorgente.
- Associazione dei risultati alle singole travi/piastre; esclusione delle righe di altri elementi; gestione della colonna `obj` minuscola nelle strain shell; conservazione dopo serializzazione; invalidazione dopo modifica o ricostruzione.
- **565 componenti** caricati e costruiti in Rhino tramite Rhino.Inside: GUID distinti, socket validi e icone 24×24. I 559 precedenti conservano nomi e GUID.
- Catalogo e metadati Grasshopper organizzati in **34 pannelli per argomento** nella scheda SAP2000. Confronto con il catalogo precedente: i 559 componenti esistenti conservano nomi e GUID; sei nuovi componenti completano importazione, percorsi e funzioni dinamiche.
- Collegamenti Grasshopper reali: materiale → sezione nativa; Model → Build; Model risolto sintetico → Decompose → Beam Actions / Plate Actions / Decompose Beam.
- Geometria fisica della sezione rettangolare: Brep chiuso, volume 0,06 m³ per A=0,02 m² e L=3 m.
- Brep chiusi, validi e orientati verso l'esterno: volumi verificati per sezione circolare, pipe e tube cavi, plate con spessore in entrambi gli ordinamenti dei vertici ed elemento solido a otto vertici.
- Cast diretto Model/elemento/sezione → Brep; conservazione delle proprietà dopo decomposizione; copie geometriche indipendenti; rifiuto del cast ambiguo di più elementi a un solo Brep.
- Collegamento Preview → parametro Brep nativo; uscita geometrica conservata a preview spenta; segnalazione ed esclusione degli elementi non ricostruibili.
- Bake effettivo in un documento Rhino di prova tramite il menu nativo della Preview e il componente Bake SAP Geometry: solo Brep chiusi, nomi SAP conservati dal componente di bake e nessuna duplicazione finché il trigger rimane true.
- Controlli di preview e pannello: gestione globale e locale, disattivazione di carichi e interruttore generale. La preview è verificata con dati di prova; non cambia la definizione FEM.

- **42 esempi** nei 34 gruppi: salvataggio/riapertura GH e GHX, uguaglianza delle impronte dei modelli, conteggio fili/oggetti, input richiesti, validazione e Brep chiusi; trigger Run/Bake disattivati.
- Importazione con client API simulato: connettività e proprietà, unità, casi/combinazioni, copia privata, hash, serializzazione, rifiuto di ricostruzioni parziali, conservazione del file nativo in export/analisi e invalidazione dopo modifica del sorgente.

I dati di risultato e le risposte API usati nei test gestiti/Grasshopper sono **fixture sintetiche**, non risultati ottenuti dal solver SAP.

## Collaudo SAP non completato

La prova con timeout di 45 secondi è stata ripetuta il 30 settembre 2026, con lo stesso esito: la chiamata **`cOAPI.ApplicationStart` non ritorna** nell'ambiente corrente. Non è stato possibile arrivare all'importazione nativa, alla creazione del modello, all'analisi o al confronto PL/EA. Non è stata accertata la causa e non viene attribuita automaticamente alla licenza.

Il worker è stato provato con un timeout di avvio di 45 secondi: intercetta il blocco e termina il proprio albero di processi. Nessun modello dell'utente è stato aperto o sovrascritto durante le prove. Nell'uso normale il timeout di avvio è 120 secondi e il limite di analisi è 30 minuti.

Il test nativo predisposto crea una mensola assialmente caricata, L=3 m, A=0,02 m², E=210·10⁶ kN/m², F=1 kN. Confronta Ux con **FL/EA** e la reazione con −1 kN. È da rieseguire quando SAP completa l'avvio API:

```powershell
dotnet run --project Rhino2SAP.Tests -c Release -- --native
```

Il test diretto non usa il timeout del worker. Per una prova limitata usare `--worker-native` dopo aver copiato i file `Rhino2SAP.Worker.*` accanto all'eseguibile dei test, come fatto nella sessione di sviluppo.

## Limiti della verifica

La corrispondenza delle firme non certifica la risposta di tutti i 565 componenti in SAP. L'assegnazione di leggi non lineari, casi dinamici, shell layered, sezioni avanzate e altri metodi nativi resta da collaudare con modelli di riferimento. Ogni errore API resta visibile e non viene rimpiazzato da valori numerici fittizi.

Il flag Finished del solver è controllato, ma non sostituisce un esame di stabilità, convergenza, sensibilità della mesh e validità ingegneristica. I limiti di preview e corrispondenza Straus sono descritti nei documenti dedicati.
