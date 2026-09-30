# 11 Solido otto nodi

Un esaedro 1 x 1 x 1 m, base vincolata e forze verticali sui quattro nodi superiori.

Modalita: Modellazione locale.

[Apri GH](11_Solido_otto_nodi.gh) · [GHX](11_Solido_otto_nodi.ghx) · [Canvas](11_solido_otto_nodi.png)

## Passaggi sul canvas

**RHINO2SAP / 11 Solido otto nodi**

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

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
