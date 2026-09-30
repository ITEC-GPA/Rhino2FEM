# 01 Trave carico preview bake

Trave a mensola con carico nodale, slider di sezione, preview e bake solido.

Modalita: Modellazione locale.

[Apri GH](01_Trave_carico_preview_bake.gh) · [GHX](01_Trave_carico_preview_bake.ghx) · [Canvas](01_trave_carico_preview_bake.png)

## Passaggi sul canvas

**RHINO2SAP / 01  TRAVE / PREVIEW / BREP**

Unita: kN, m, C. Trave a mensola lunga 3 m, sezione 0.20 x 0.10 m.
F = -10 kN lungo Z; incastro al nodo BASE.
Prova a modificare altezza, larghezza o vettore del carico. Spegni Mostra carichi.
Le coordinate di asse e nodi sono incorporate; se le cambi, mantienile coerenti.

**Carico: FX FY FZ MX MY MZ**

0
0
-10
0
0
0

**Preview e bake**

B = Brep chiusi, N = nomi SAP.
Brep: tasto destro > Bake.
Oppure BAKE SOLIDI: False > True.
La geometria non richiede SAP.
Modificare gli switch nel componente Display.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material; T3 ← Altezza h [m].Number Slider; T2 ← Larghezza b [m].Number Slider |
| SAP Frame Element | 07 Elementi | Property ← Rettangolo.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← Nodo BASE.Element |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Node Load Force | 13 Carichi nodali | Name ← Nodo TIP.Element; LoadPat ← Pattern Q.Pattern; Value ← Carico: FX FY FZ MX MY MZ.Panel |
| Build SAP Model | 01 Modello | Definitions ← Beam F1.Element, Incastro.Definition, Forza al TIP.Definition |
| SAP Display Settings | 33 Preview e bake | Loads ← Mostra carichi.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Display.Settings |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Model Summary | 01 Modello | Model ← Build Model.Model |
| Bake SAP Geometry | 33 Preview e bake | Model ← Build Model.Model; Bake ← BAKE SOLIDI.Boolean Toggle |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
