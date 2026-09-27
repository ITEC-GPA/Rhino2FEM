# Impostazione condivisa

| Funzione di Rhino2SAP / Rhino2Straus | Rhino2Midas |
|---|---|
| Progetto autonomo, soluzione e pacchetto | Core, Api, Grasshopper, Tests; build.ps1 e ZIP |
| Ribbon per argomento e icone | Pannelli numerati in italiano, icone 24 px, catalogo generato dai componenti caricati |
| Materiali e sezioni | Materiali isotropi/database, otto sezioni parametriche, sezioni native, spessori; record API avanzati |
| Geometria FEM | Nodi, travi, piastre, solidi, truss, tension/compression-only, link elastici, mesh e polilinee |
| Attributi su copie | Vincoli, molle, masse, svincoli, offset, proprietà, assi locali e record API |
| Carichi | Nodi, travi, momenti, pressione, peso proprio; componenti nativi per carichi mobili, temperatura, precompressione, fasi e dinamica |
| Casi e combinazioni | STLD, LCOM e definizioni native; richieste risultati per casi e combinazioni |
| Model | Build, Merge, Validate, Summary, Clear Results, Decompose, filtri |
| Utility geometriche | Allineamenti trave/piastra, assi, traslazione e rotazione rigida |
| Preview e pannello | Visibilità globale e per Model, categorie, simboli, scale, filtro caso, Guida/About |
| Brep e bake | Solidi chiusi, cast diretto degli elementi, estrazione e bake con nomi/Undo |
| Export e analisi | API REST Civil NX; fronte Run; controllo overwrite e documento attivo |
| Risultati negli elementi | Immutabili, fingerprint, unità e hash del file; archivi JSON e persistenza GH |
| Lettori | Travi, piastre, nodi, solidi, aste e link; richieste native e query per tutte le tabelle del catalogo |
| Post-processing | Deformata, diagrammi trave, filtri, estremi con righe governanti, dati per futuro verificatore |
| Import | Tabelle selezionate del documento Civil attivo, archivi Model/risultati |
| Collaudo | Contratti API con trasporto simulato e test reali Rhino/Grasshopper |

## Differenze da rispettare

- L'API Civil NX opera sul documento attivo; non offre l'isolamento del worker SAP usato da Rhino2SAP. Il componente richiede l'abilitazione esplicita della sostituzione del documento.
- L'analisi esegue `/doc/ANAL` con le impostazioni native raccolte nel Model. Modale, buckling, non lineare, dinamica e fasi richiedono le rispettive definizioni native e richieste risultati; non vengono tradotti i parametri specifici dei solver SAP/Straus.
- Gli ID API avanzati possono apparire dentro oggetti JSON annidati. Merge rifiuta i conflitti; non esegue una rinumerazione parziale potenzialmente incoerente.
- Il catalogo offre la struttura delle API documentate, non una certificazione dell'esecuzione di tutte le funzioni. Le varianti M1/Hyper-S sono specifiche delle versioni che le supportano.
- Le forme di sezione non ricostruibili geometricamente rimangono disponibili per l'export API e per la visualizzazione analitica. Gli elementi omessi dai Brep sono riportati in Issues.
- `Verification Data` prepara dati per un futuro verificatore. Non esegue verifiche normative e non converte automaticamente le convenzioni di altri solver.
