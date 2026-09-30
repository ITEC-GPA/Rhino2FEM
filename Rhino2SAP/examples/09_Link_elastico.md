# 09 Link elastico

Link assiale elastico: K1=1000 kN/m, forza +1 kN. Atteso spostamento assiale 0.001 m; gli altri gradi di liberta sono vincolati.

Modalita: Modellazione locale.

[Apri GH](09_Link_elastico.gh) · [GHX](09_Link_elastico.ghx) · [Canvas](09_link_elastico.png)

## Passaggi sul canvas

**RHINO2SAP / 09 Link elastico**

Link assiale elastico: K1=1000 kN/m, forza +1 kN. Atteso spostamento assiale 0.001 m; gli altri gradi di liberta sono vincolati.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  PROPRIETA LINK**

DOF, Fixed, Ke e Ce hanno sei valori U1 U2 U3 R1 R2 R3. Solo U1 e elastico. Il link non ha un volume fisico da convertire in Brep.

**02  LINK / NODI**

Link L1 lungo X; nodo BASE incastrato, nodo TIP libero solo in X.

**03  CARICO**

Carico globale FX=1 kN al TIP.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Link Property Linear | 06 Link e cavi |  |
| SAP Link Element | 07 Elementi | Property ← SetLinear.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← BASE.Element |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← TIP.Element |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Node Load Force | 13 Carichi nodali | Name ← TIP.Element; LoadPat ← Q.Pattern |
| Build SAP Model | 01 Modello | Definitions ← L1.Element, SetRestraint.Definition, SetRestraint.Definition, SetLoadForce.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
