# 32 Import modello SAP associato

Aprire un SDB esistente e ottenere il Model Grasshopper associato, con unita, nomi, casi, combinazioni e hash della sorgente.

Modalita: Import SAP su richiesta.

[Apri GH](32_Import_modello_SAP_associato.gh) · [GHX](32_Import_modello_SAP_associato.ghx) · [Canvas](32_import_modello_sap_associato.png)

## Passaggi sul canvas

**RHINO2SAP / 32 Import modello SAP associato**

Aprire un SDB esistente e ottenere il Model Grasshopper associato, con unita, nomi, casi, combinazioni e hash della sorgente.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  FILE ESISTENTE**

Sostituire il percorso con un .sdb reale. IMPORT SAP deve passare da False a True. Il sorgente viene aperto tramite una copia privata.

**File SAP esistente**

C:\Users\Public\Documents\modello_esistente.sdb

**02  MODEL ASSOCIATO**

Il Model mantiene il file sorgente e il suo hash. La vista GH ricostruisce le parti supportate; tutte le definizioni native restano nel file. Modificare in SAP e reimportare per aggiornare.

**03  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

**04  EXPORT SDB**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_32_Import_modello_SAP_associato.sdb

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| Import SAP Model | 26 Analisi e file | Path ← File SAP esistente.Panel; Run ← IMPORT SAP.Boolean Toggle |
| Decompose SAP Model | 01 Modello | Model ← Import Model.Model |
| Validate SAP Model | 01 Modello | Model ← Import Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Import Model.Model; Settings ← Visualizzazione.Settings |
| Export SAP2000 Model | 26 Analisi e file | Model ← Import Model.Model; Path ← File di destinazione.Panel; Run ← EXPORT SAP.Boolean Toggle |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
