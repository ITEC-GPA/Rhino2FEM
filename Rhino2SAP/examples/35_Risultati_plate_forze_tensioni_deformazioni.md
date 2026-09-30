# 35 Risultati plate forze tensioni deformazioni

Shell: lettura delle azioni per unita di lunghezza, tensioni top/bottom e deformazioni nei singoli elementi.

Modalita: Analisi SAP su richiesta.

[Apri GH](35_Risultati_plate_forze_tensioni_deformazioni.gh) · [GHX](35_Risultati_plate_forze_tensioni_deformazioni.ghx) · [Canvas](35_risultati_plate_forze_tensioni_deformazioni.png)

## Passaggi sul canvas

**RHINO2SAP / 35 Risultati plate forze tensioni deformazioni**

Shell: lettura delle azioni per unita di lunghezza, tensioni top/bottom e deformazioni nei singoli elementi.
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

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

**06  ANALISI SAP**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_35_Risultati_plate_forze_tensioni_deformazioni.sdb

**07  PLATE RISOLTE**

Plates contiene quattro elementi, ciascuno con le proprie righe di risultati. Conservare il ramo GH e il nome dell'elemento nell'esportazione dei valori.

**08  TENSIONI / DEFORMAZIONI**

Top/bottom seguono l'asse locale 3 della shell. Le tensioni sono kN/m2; le deformazioni normali e di taglio sono adimensionali.

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
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← File di destinazione.Panel; Run ← RUN SAP.Boolean Toggle |
| Decompose SAP Plate | 29 Risultati plate | Element ← Analisi e risultati.Plates |
| SAP Plate Results Forces and Moments | 29 Risultati plate | Element ← Analisi e risultati.Plates |
| SAP Plate Results Stresses | 29 Risultati plate | Element ← Analisi e risultati.Plates |
| SAP Plate Results Strains | 29 Risultati plate | Element ← Analisi e risultati.Plates |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
