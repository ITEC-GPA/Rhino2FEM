# Verifiche — 27 settembre 2026

## Eseguite

- Build Release .NET 8 di Core, Api, Grasshopper e Tests: **zero errori, zero warning**.
- **63 controlli gestiti**: unità, nodi e connettività, indipendenza degli ID link, unione e conflitti, dipendenze/carichi, geometrie degeneri, fingerprint, serializzazione, appartenenza dei risultati, import API, richieste per piastre e combinazioni, file/hash, overwrite e gestione degli errori.
- Il contratto HTTP è verificato con un trasporto simulato: `/doc/NEW`, `/db`, `/doc/SAVEAS`, `/doc/ANAL`, `/post/TABLE`, `/doc/SAVE`. Questi test **non eseguono GEN**.
- Caricamento con **Rhino.Inside in Rhino 8 e Grasshopper reali**: 478 componenti, GUID unici, parametri e icone validi; tutti i 384 componenti generati di definizione/richiesta risolvono gli esempi documentati.
- Collegamenti GH: esempio → Build → solidi; Model con risultati → Decompose → Beam Actions; cast verso Brep; opzioni collegate alla preview; riconoscimento dei componenti a monte.
- Geometrie: trave rettangolare con volume 0,18 m³, otto forme di sezione incluse cavità, spessore piastra, solidi a 4/6/8 nodi, convenzione beta dei membri verticali.
- Persistenza Grasshopper di definizioni e risultati, apertura/rendering del pannello impostazioni.
- Specifici GEN: rifiuto URL/file Civil, impronta distinta per prodotto, export/import WALL e STOR, diaframmi, richieste Story senza COMPONENTS, associazione delle risultanti per ID WALL e lettura separata delle estremità. La parete di esempio produce un Brep chiuso di 2,4 m³; pareti e piani sopravvivono al salvataggio Grasshopper. I GUID sono confrontati con il catalogo Civil.
- Controllo dei progetti preservati: Rhino2SAP compila e supera 32 test; Rhino2Straus compila e supera 64 test, senza dipendenze dai progetti obsoleti rimossi.

## Non eseguite

Il solver GEN NX non è stato avviato dal plugin durante questo lavoro: `MIDAS_GEN_API_URL` e `MIDAS_GEN_API_KEY` non erano configurate. Non sono quindi attestati l'accettazione di tutti i payload dalla versione GEN installata, i valori numerici del solver o la convergenza dei casi avanzati. Le funzioni specialistiche del catalogo richiedono le licenze e la versione MIDAS corrispondenti.

## Ripetere il collaudo

```powershell
./build.ps1 -RhinoSmoke
```

Per il test numerico, aprire GEN NX, salvare il proprio lavoro, attivare API Settings → Connect e impostare le due variabili d'ambiente nel terminale. Il comando seguente **sostituisce il documento GEN attivo**, crea una mensola in una nuova cartella temporanea e controlla reazione verticale 10 kN, momento 30 kNm e spostamento verticale compatibile con la flessibilità flessionale/tagliante:

```powershell
dotnet run --project Rhino2MidasGen.Tests -c Release -- --native --replace-active-document
```

L'esempio usa E=210 GPa, sezione rettangolare 200×300 mm, L=3 m e forza verticale 10 kN. Il riferimento Euler–Bernoulli è 0,95238 mm; il controllo ammette la deformabilità a taglio attivata nella sezione MIDAS. Percorso e archivi del test vengono stampati e conservati per l'ispezione.

## Limiti del contratto

Il preflight controlla dati e riferimenti principali, non la stabilità della struttura né tutte le dipendenze possibili dei JSON avanzati. Le definizioni native rimangono fedeli agli esempi MIDAS. Il catalogo è condiviso Civil/GEN e comprende voci specialistiche non necessariamente supportate da GEN: verificarne disponibilità nella versione e licenza utilizzate. Un errore nativo interrompe la scrittura e viene mostrato, senza tentativi automatici o risultati sintetici. L'import delle tabelle selezionate non equivale a una copia universale di qualunque file GEN.

Fonti: [indice ufficiale API MIDAS](https://support.midasuser.com/hc/en-us/articles/33016922742937-MIDAS-API-Online-Manual), esempi/schema collegati dal catalogo, [manuale GEN NX](https://support.midasuser.com/hc/en-us/articles/49909210848537-MIDAS-GEN-NX-Online-Manual). Ogni componente API espone il proprio riferimento specifico.
