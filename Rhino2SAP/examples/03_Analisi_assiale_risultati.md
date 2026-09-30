# 03 Analisi assiale risultati

Analisi assiale, combinazione e confronto con lo spostamento teorico FL/EA.

Modalita: Analisi SAP su richiesta.

[Apri GH](03_Analisi_assiale_risultati.gh) · [GHX](03_Analisi_assiale_risultati.ghx) · [Canvas](03_analisi_assiale_risultati.png)

## Passaggi sul canvas

**RHINO2SAP / 03  ANALISI E RISULTATI**

Unita: kN, m, C. Trave a mensola lunga 3 m, sezione 0.20 x 0.10 m.
F = +1 kN lungo X. E = 210e6 kN/m2. Atteso Ux = FL/EA = 7.142857e-7 m.
Il ramo di analisi e sotto: RUN SAP parte da False.

**Carico: FX FY FZ MX MY MZ**

1
0
0
0
0
0

**Preview e bake**

B = Brep chiusi, N = nomi SAP.
Brep: tasto destro > Bake.
Oppure BAKE SOLIDI: False > True.
La geometria non richiede SAP.
Modificare gli switch nel componente Display.

**05  ESEGUIRE L'ANALISI**

1. Scegli un nuovo percorso .sdb nel pannello Path.
2. Porta RUN SAP da False a True. Per rilanciare, torna prima a False.
3. I componenti dei risultati si popolano dopo il completamento dell'analisi.
Prima del run restano in attesa: non contengono risultati simulati.
SLS e ULS_DEMO sono nomi didattici; il fattore 1.5 non rappresenta una combinazione normativa.

**Path: scegliere un file .sdb**

C:\Users\Public\Documents\Rhino2SAP_03_assiale.sdb

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
| Build SAP Model | 01 Modello | Definitions ← Beam F1.Element, Incastro.Definition, Forza al TIP.Definition, Caso SLS.Case, ULS_DEMO = 1.5 SLS.Combination |
| SAP Display Settings | 33 Preview e bake | Loads ← Mostra carichi.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Display.Settings |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Model Summary | 01 Modello | Model ← Build Model.Model |
| Bake SAP Geometry | 33 Preview e bake | Model ← Build Model.Model; Bake ← BAKE SOLIDI.Boolean Toggle |
| SAP Linear Static Case | 19 Casi statici | Definitions ← Pattern Q.Pattern |
| SAP Load Combination | 25 Combinazioni |  |
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← Path: scegliere un file .sdb.Panel; Run ← RUN SAP.Boolean Toggle |
| SAP Beam Results N V T M | 28 Risultati beam | Element ← Analizza e incorpora.Beams |
| SAP Model Joint Displ | 27 Risultati nodali | Model ← Analizza e incorpora.Model |
| Query SAP Element Results | 32 Risultati e query | Element ← Analizza e incorpora.Beams |
| SAP Model Frame Force Diagram | 28 Risultati beam | Model ← Analizza e incorpora.Model |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
