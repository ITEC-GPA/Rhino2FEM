# 36 Risultati link

Link assiale elastico: K1=1000 kN/m, forza +1 kN. Atteso spostamento assiale 0.001 m; gli altri gradi di liberta sono vincolati.

Modalita: Analisi SAP su richiesta.

[Apri GH](36_Risultati_link.gh) · [GHX](36_Risultati_link.ghx) · [Canvas](36_risultati_link.png)

## Passaggi sul canvas

**RHINO2SAP / 36 Risultati link**

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

**06  ANALISI SAP**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_36_Risultati_link.sdb

**07  RISULTATI LINK**

Forze e deformazioni native. I numeri restano vuoti fino all'esecuzione SAP.

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
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← File di destinazione.Panel; Run ← RUN SAP.Boolean Toggle |
| SAP Model Link Force | 30 Risultati solidi-link | Model ← Analisi e risultati.Model |
| SAP Model Link Deformation | 30 Risultati solidi-link | Model ← Analisi e risultati.Model |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
