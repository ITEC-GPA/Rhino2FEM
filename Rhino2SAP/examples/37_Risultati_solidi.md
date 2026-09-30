# 37 Risultati solidi

Un esaedro 1 x 1 x 1 m, base vincolata e forze verticali sui quattro nodi superiori.

Modalita: Analisi SAP su richiesta.

[Apri GH](37_Risultati_solidi.gh) · [GHX](37_Risultati_solidi.ghx) · [Canvas](37_risultati_solidi.png)

## Passaggi sul canvas

**RHINO2SAP / 37 Risultati solidi**

Un esaedro 1 x 1 x 1 m, base vincolata e forze verticali sui quattro nodi superiori.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / SOLIDO**

Calcestruzzo isotropo. Rotazioni del materiale A=B=C=0; Incompatible=false.

**02  CONNETTIVITA**

Vertici: quattro inferiori antiorari, poi i corrispondenti superiori. Nessuna tetraedrizzazione implicita.

**03  CARICHI**

Quattro forze FZ=-1 kN. Risultante totale -4 kN; questo non equivale a una mesh raffinata per valutare i picchi.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

**06  ANALISI SAP**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_37_Risultati_solidi.sdb

**07  TENSIONI / DEFORMAZIONI**

Colonne native riferite ai punti del solido; conservare gli identificativi quando si interpretano i valori.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Prop Solid Prop | 05 Piastre e solidi | MatProp ← Calcestruzzo.Material |
| SAP Solid Element | 07 Elementi | Property ← SetProp.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← Base.Element |
| SAP Node Element | 07 Elementi |  |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Node Load Force | 13 Carichi nodali | Name ← Sommita.Element; LoadPat ← Q.Pattern |
| Build SAP Model | 01 Modello | Definitions ← S1.Element, SetRestraint.Definition, SetLoadForce.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← File di destinazione.Panel; Run ← RUN SAP.Boolean Toggle |
| SAP Model Solid Stress | 30 Risultati solidi-link | Model ← Analisi e risultati.Model |
| SAP Model Solid Strain | 30 Risultati solidi-link | Model ← Analisi e risultati.Model |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
