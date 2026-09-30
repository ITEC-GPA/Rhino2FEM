# 39 Utility firme unita query

Ispezione delle firme API, delle unita SAP, delle definizioni raccolte e dei dati pronti per verifiche esterne.

Modalita: Modellazione e analisi su richiesta.

[Apri GH](39_Utility_firme_unita_query.gh) · [GHX](39_Utility_firme_unita_query.ghx) · [Canvas](39_utility_firme_unita_query.png)

## Passaggi sul canvas

**RHINO2SAP / 39 Utility firme unita query**

Ispezione delle firme API, delle unita SAP, delle definizioni raccolte e dei dati pronti per verifiche esterne.
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

**06  SDK / UNITA**

API Signature usa lo schema dell'SDK installato. Units elenca eUnits: selezionare le unita prima di costruire, senza presumere conversioni automatiche.

**07  DEFINIZIONI / CATALOGO**

Definition Info rende visibili i comandi differiti. Quantities elenca tutte le tabelle richiedibili al run.

**08  ANALISI SAP**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_39_Utility_firme_unita_query.sdb

**09  DATI PER VERIFICHE**

Verification Data espone risultati e definizioni. Complete indica letture API riuscite, non un giudizio di sicurezza o una verifica normativa.

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
| SAP API Signature | 34 Utility |  |
| SAP Model Units | 01 Modello |  |
| SAP Definition Info | 34 Utility | Definition ← Build Model.Model |
| SAP Result Quantities | 32 Risultati e query |  |
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← File di destinazione.Panel; Run ← RUN SAP.Boolean Toggle |
| SAP Model Verification Data | 32 Risultati e query | Model ← Analisi e risultati.Model |
| SAP Model Result Cases | 32 Risultati e query | Model ← Analisi e risultati.Model |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
