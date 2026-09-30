# 19 Plate pressione mesh SAP

Pressione uniforme sulle shell e automesh SAP. La mesh GH degli oggetti e la mesh di analisi SAP sono livelli diversi.

Modalita: Modellazione locale.

[Apri GH](19_Plate_pressione_mesh_SAP.gh) · [GHX](19_Plate_pressione_mesh_SAP.ghx) · [Canvas](19_plate_pressione_mesh_sap.png)

## Passaggi sul canvas

**RHINO2SAP / 19 Plate pressione mesh SAP**

Pressione uniforme sulle shell e automesh SAP. La mesh GH degli oggetti e la mesh di analisi SAP sono livelli diversi.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / SHELL**

Shell sottile, 20 cm; materiale elastico didattico. Spessore membranale e flessionale uguali.

**02  MESH / APPOGGI**

Quadrato 4 x 4 m discretizzato 2 x 2. La mesh e incorporata; ogni faccia diventa un oggetto SAP. Gli otto nodi di bordo sono incastrati.

**03  PRESSIONE**

-5 kN/m2 in Z globale (Dir=6). Name=ALL, ItemType=1 assegna il carico al gruppo ALL; Definitions porta la mesh nello stesso ramo.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  AUTOMESH SAP**

Ogni oggetto A1-A4 e suddiviso 2 x 2 dal solver (MeshType=1). La preview mostra le quattro shell GH, non la mesh interna generata da SAP.

**06  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**07  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Area Property Shell 1 | 05 Piastre e solidi | MatProp ← Calcestruzzo.Material |
| Mesh to SAP Areas | 07 Elementi | Property ← SetShell_1.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← Bordo.Element |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Area Load Uniform | 15 Carichi plate | Definitions ← 4 quadrilateri.Elements; LoadPat ← Q.Pattern |
| Build SAP Model | 01 Modello | Definitions ← SetLoadUniform.Definition, SetRestraint.Definition |
| SAP Area Auto Mesh | 10 Mesh e assegnazioni | Definitions ← Build Model.Model |
| Build SAP Model | 01 Modello | Definitions ← SetAutoMesh.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
