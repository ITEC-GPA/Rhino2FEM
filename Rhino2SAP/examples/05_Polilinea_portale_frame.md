# 05 Polilinea portale frame

Polilinea di un portale: un frame per segmento, selezione per nome e decomposizione beam.

Modalita: Modellazione locale.

[Apri GH](05_Polilinea_portale_frame.gh) · [GHX](05_Polilinea_portale_frame.ghx) · [Canvas](05_polilinea_portale_frame.png)

## Passaggi sul canvas

**RHINO2SAP / 05 Polilinea portale frame**

Polilinea di un portale: un frame per segmento, selezione per nome e decomposizione beam.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  PROPRIETA**

Sezione 0.20 x 0.10 m; il materiale passa alla sezione e da questa a tutti i segmenti.

**02  POLILINEA**

Portale largo 4 m e alto 3 m. Segmenti F1, F2 e F3. Le coordinate sono editabili negli input incorporati.

**03  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**04  FILTRI**

Filtrare prima di aggiungere nuovi carichi: Filter conserva le proprieta e rimuove le assegnazioni. Decompose Beam restituisce linea e vertici anche prima del calcolo.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material |
| Polyline to SAP Frames | 07 Elementi | Property ← SetRectangle.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← Basi.Element |
| Build SAP Model | 01 Modello | Definitions ← Portale.Elements, SetRestraint.Definition |
| Filter SAP Elements | 34 Utility | Definition ← Build Model.Model |
| Decompose SAP Beam | 28 Risultati beam | Element ← Traverso F2.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
