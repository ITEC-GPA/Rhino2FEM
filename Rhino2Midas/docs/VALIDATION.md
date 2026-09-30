# Verifiche — 26 settembre 2026

## Eseguite

- Build Release .NET 8 di Core, Api, Grasshopper e Tests: **zero errori, zero warning**.
- **49 controlli gestiti**: unità, nodi e connettività, indipendenza degli ID link, unione e conflitti, dipendenze/carichi, geometrie degeneri, fingerprint, serializzazione, appartenenza dei risultati, import API, richieste per piastre e combinazioni, file/hash, overwrite e gestione degli errori.
- Il contratto HTTP è verificato con un trasporto simulato: `/doc/NEW`, `/db`, `/doc/SAVEAS`, `/doc/ANAL`, `/post/TABLE`, `/doc/SAVE`. Questi test **non eseguono Civil**.
- Caricamento con **Rhino.Inside in Rhino 8 e Grasshopper reali**: 434 componenti, GUID unici, parametri e icone validi; tutti i 345 componenti generati di definizione/richiesta risolvono gli esempi documentati.
- Collegamenti GH: esempio → Build → solidi; Model con risultati → Decompose → Beam Actions; cast verso Brep; opzioni collegate alla preview; riconoscimento dei componenti a monte.
- Geometrie: trave rettangolare con volume 0,18 m³, otto forme di sezione incluse cavità, spessore piastra, solidi a 4/6/8 nodi, convenzione beta dei membri verticali.
- Persistenza Grasshopper di definizioni e risultati, apertura/rendering del pannello impostazioni.
- Controllo dei progetti preservati: Rhino2SAP compila e supera 32 test; Rhino2Straus compila e supera 64 test, senza dipendenze dai progetti obsoleti rimossi.

## Non eseguite

Il solver Civil NX non è stato avviato dal plugin durante questo lavoro: `MIDAS_API_URL` e `MIDAS_API_KEY` non erano configurate. Non sono quindi attestati l'accettazione di tutti i payload dalla versione Civil installata, i valori numerici del solver o la convergenza dei casi avanzati. Le funzioni specialistiche del catalogo richiedono le licenze e la versione MIDAS corrispondenti.

## Ripetere il collaudo

```powershell
./build.ps1 -RhinoSmoke
```

Per il test numerico, aprire Civil NX, salvare il proprio lavoro, attivare API Settings → Connect e impostare le due variabili d'ambiente nel terminale. Il comando seguente **sostituisce il documento Civil attivo**, crea una mensola in una nuova cartella temporanea e controlla reazione verticale 10 kN, momento 30 kNm e spostamento verticale compatibile con la flessibilità flessionale/tagliante:

```powershell
dotnet run --project Rhino2Midas.Tests -c Release -- --native --replace-active-document
```

L'esempio usa E=210 GPa, sezione rettangolare 200×300 mm, L=3 m e forza verticale 10 kN. Il riferimento Euler–Bernoulli è 0,95238 mm; il controllo ammette la deformabilità a taglio attivata nella sezione MIDAS. Percorso e archivi del test vengono stampati e conservati per l'ispezione.

## Limiti del contratto

Il preflight controlla dati e riferimenti principali, non la stabilità della struttura né tutte le dipendenze possibili dei JSON avanzati. Le definizioni native rimangono fedeli ai nomi e alle unità Civil. Un errore nativo interrompe la scrittura e viene mostrato, senza tentativi automatici o risultati sintetici. L'import delle tabelle selezionate non equivale a una copia universale di qualunque file Civil.

Fonti: [indice ufficiale API MIDAS](https://support.midasuser.com/hc/en-us/articles/33016922742937-MIDAS-API-Online-Manual), esempi/schema collegati dal catalogo, manuale Civil NX `CVLw.chm` dell'installazione locale per geometria e convenzioni. Ogni componente API espone il proprio riferimento specifico.

## Aggiornamento del 30 settembre 2026

61 controlli gestiti superati. I nuovi test verificano tabelle predefinite, proprietà/carichi/vincoli/unità, gravità nativa, tabelle aggiuntive, GET esclusivi, deduplicazione, tabelle facoltative assenti e arresto su errori di autenticazione, geometria indispensabile o connessione. Il trasporto è simulato: non sono prove numeriche del solver.