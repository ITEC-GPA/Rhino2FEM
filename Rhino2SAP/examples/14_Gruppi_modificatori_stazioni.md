# 14 Gruppi modificatori stazioni

Gruppi, modificatori di rigidezza e stazioni di output su un frame.

Modalita: Modellazione locale.

[Apri GH](14_Gruppi_modificatori_stazioni.gh) · [GHX](14_Gruppi_modificatori_stazioni.ghx) · [Canvas](14_gruppi_modificatori_stazioni.png)

## Passaggi sul canvas

**RHINO2SAP / 14 Gruppi modificatori stazioni**

Gruppi, modificatori di rigidezza e stazioni di output su un frame.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / SEZIONE**

Sezione rettangolare 0.20 x 0.10 m. E=210e6 kN/m2. I nomi delle definizioni devono essere univoci.

**02  ELEMENTI / VINCOLO**

Mensola di 3 m. Incastro al nodo BASE. Il nodo TIP coincide con l'estremita del frame F1.

**03  PATTERN / CARICO**

Pattern Q senza peso proprio. Forza al TIP: -1 kN lungo Z per la mensola orizzontale, +1 kN lungo X per quella verticale.

**04  ASSEGNAZIONI**

Gruppo BEAMS; modificatori tutti 1 salvo I3=.8. Cinque stazioni minime. Gli attributi sono comandi differiti: vengono applicati all'export.

**05  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**06  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← BASE.Element |
| SAP Node Element | 07 Elementi |  |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Node Load Force | 13 Carichi nodali | Name ← TIP.Element; LoadPat ← Q.Pattern |
| SAP Group Def Group | 10 Mesh e assegnazioni |  |
| SAP Frame Group Assign | 10 Mesh e assegnazioni | Name ← Frame F1.Element; GroupName ← SetGroup.Definition |
| SAP Frame Modifiers | 10 Mesh e assegnazioni | Name ← SetGroupAssign.Definition |
| SAP Frame Output Stations | 10 Mesh e assegnazioni | Name ← SetModifiers.Definition |
| Build SAP Model | 01 Modello | Definitions ← SetOutputStations.Definition, SetRestraint.Definition, SetLoadForce.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
