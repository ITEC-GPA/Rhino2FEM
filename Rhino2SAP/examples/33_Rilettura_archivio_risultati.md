# 33 Rilettura archivio risultati

Analisi, derivazione del percorso archivio e rilettura verificata dei risultati nel Model.

Modalita: Analisi e rilettura su richiesta.

[Apri GH](33_Rilettura_archivio_risultati.gh) · [GHX](33_Rilettura_archivio_risultati.ghx) · [Canvas](33_rilettura_archivio_risultati.png)

## Passaggi sul canvas

**RHINO2SAP / 33 Rilettura archivio risultati**

Analisi, derivazione del percorso archivio e rilettura verificata dei risultati nel Model.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / SEZIONE**

Sezione rettangolare 0.20 x 0.10 m. E=210e6 kN/m2. I nomi delle definizioni devono essere univoci.

**02  ELEMENTI / VINCOLO**

Mensola di 3 m. Incastro al nodo BASE. Il nodo TIP coincide con l'estremita del frame F1.

**03  PATTERN / CARICO**

Pattern Q senza peso proprio. Forza al TIP: -1 kN lungo Z per la mensola orizzontale, +1 kN lungo X per quella verticale.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

**06  ANALISI SAP**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_33_Rilettura_archivio_risultati.sdb

**07  ARCHIVIO**

SAP File Paths deriva .sdb.results.json dal Path prodotto dal run: non serve mantenere due percorsi a mano. Read Results verifica sia il modello sia l'hash del file SAP.

**08  QUERY**

Questa rilettura non apre SAP e non ricalcola. Funziona con l'archivio prodotto dal plugin; non importa automaticamente risultati di un file esterno senza archivio.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| SAP Node Element | 07 Elementi |  |
| SAP Node Restraint | 08 Vincoli e molle | Name ← BASE.Element |
| SAP Node Element | 07 Elementi |  |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Node Load Force | 13 Carichi nodali | Name ← TIP.Element; LoadPat ← Q.Pattern |
| Build SAP Model | 01 Modello | Definitions ← Frame F1.Element, SetRestraint.Definition, SetLoadForce.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← File di destinazione.Panel; Run ← RUN SAP.Boolean Toggle |
| SAP File Paths | 26 Analisi e file | Path ← Analisi e risultati.Path |
| Clear SAP Model Results | 01 Modello | Model ← Analisi e risultati.Model |
| Read SAP Results Into Model | 26 Analisi e file | Model ← Rimuovi risultati.Model; Archive ← Percorsi associati.Results archive |
| SAP Model Frame Force | 28 Risultati beam | Model ← Rileggi risultati.Model |
| Read SAP API Table | 26 Analisi e file | File ← Analisi e risultati.Path; Run ← READ API.Boolean Toggle |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
