# 20 Temperatura plate

Incremento termico uniforme sulle shell: coefficiente di dilatazione, pattern dedicato e vincoli.

Modalita: Modellazione locale.

[Apri GH](20_Temperatura_plate.gh) · [GHX](20_Temperatura_plate.ghx) · [Canvas](20_temperatura_plate.png)

## Passaggi sul canvas

**RHINO2SAP / 20 Temperatura plate**

Incremento termico uniforme sulle shell: coefficiente di dilatazione, pattern dedicato e vincoli.
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

**05  TEMPERATURA**

Pattern TEMP; incremento +20 C. MyType=1 significa variazione uniforme, non gradiente. Il vincolo al bordo impedisce la libera dilatazione.

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
| SAP Load Pattern | 12 Load pattern |  |
| SAP Area Load Temperature | 15 Carichi plate | Definitions ← Build Model.Model; LoadPat ← TEMP.Pattern |
| Build SAP Model | 01 Modello | Definitions ← SetLoadTemperature.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
