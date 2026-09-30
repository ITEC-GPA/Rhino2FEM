# 02 Piastra mesh pressione

Piastra 2 x 2 shell, pressione uniforme e vincoli di bordo.

Modalita: Modellazione locale.

[Apri GH](02_Piastra_mesh_pressione.gh) · [GHX](02_Piastra_mesh_pressione.ghx) · [Canvas](02_piastra_mesh_pressione.png)

## Passaggi sul canvas

**RHINO2SAP / 02  PIASTRA DISCRETIZZATA**

Unita: kN, m, C. Piastra 4 x 4 m; 4 shell quadrangolari, spessore 0.20 m.
8 nodi di bordo incastrati; nodo centrale libero. Pressione -5 kN/m2 lungo Z globale.
Mesh e coordinate sono incorporate: non serve un file Rhino.
Mesh volutamente grossolana per illustrare i collegamenti; raffinarla prima di un uso progettuale.

**Assegnazione a tutte le shell**

Name=ALL, ItemType=1: gruppo ALL.
Dir=6: Z Global; Value<0: verso -Z.
Self weight=0: solo carico assegnato.

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
| SAP Area Property Shell 1 | 05 Piastre e solidi | MatProp ← Calcestruzzo.Material; Thickness ← Spessore t [m].Number Slider; Bending ← Spessore t [m].Number Slider |
| Mesh to SAP Areas | 07 Elementi | Mesh ← Mesh 2x2.Mesh; Property ← Shell sottile.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← Nodi di bordo.Element |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Area Load Uniform | 15 Carichi plate | Definitions ← 4 plate A1-A4.Elements; LoadPat ← Pattern Q.Pattern; Value ← Pressione Z [kN/m2].Number Slider |
| Build SAP Model | 01 Modello | Definitions ← Pressione sulle shell.Definition, Incastri bordo.Definition |
| SAP Display Settings | 33 Preview e bake | Loads ← Mostra carichi.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Display.Settings |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Model Summary | 01 Modello | Model ← Build Model.Model |
| Bake SAP Geometry | 33 Preview e bake | Model ← Build Model.Model; Bake ← BAKE SOLIDI.Boolean Toggle |
| Decompose SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Element Info | 34 Utility | Definition ← Decomponi modello.Area |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
