# 10 Cavo e carico distribuito

Proprieta di cavo, connettivita e carico distribuito. Il comportamento a fune richiede una configurazione non lineare in SAP.

Modalita: Modellazione locale.

[Apri GH](10_Cavo_e_carico_distribuito.gh) · [GHX](10_Cavo_e_carico_distribuito.ghx) · [Canvas](10_cavo_e_carico_distribuito.png)

## Passaggi sul canvas

**RHINO2SAP / 10 Cavo e carico distribuito**

Proprieta di cavo, connettivita e carico distribuito. Il comportamento a fune richiede una configurazione non lineare in SAP.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / CAVO**

Area di acciaio = .001 m2. La linea in GH identifica gli estremi; non simula la catenaria.

**02  ELEMENTI**

Cavo tra (0,0,0) e (6,0,1), entrambi gli estremi vincolati. Il disegno mostra l'asse, non un solido di cavo.

**03  CARICO**

Carico verticale -0.2 kN/m, Dir=6 e sistema Global. Definire tensionamento e caso non lineare prima di un uso progettuale.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Prop Cable Prop | 06 Link e cavi | MatProp ← Acciaio.Material |
| SAP Cable Element | 07 Elementi | Property ← SetProp.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← Estremi.Element |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Cable Obj Load Distributed | 16 Altri carichi | Name ← C1.Element; LoadPat ← Q.Pattern |
| Build SAP Model | 01 Modello | Definitions ← SetLoadDistributed.Definition, SetRestraint.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
