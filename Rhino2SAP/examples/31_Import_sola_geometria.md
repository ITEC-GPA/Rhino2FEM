# 31 Import sola geometria

Importazione della sola geometria da un SDB. Questo output non comprende materiali, proprieta o carichi.

Modalita: Import SAP su richiesta.

[Apri GH](31_Import_sola_geometria.gh) · [GHX](31_Import_sola_geometria.ghx) · [Canvas](31_import_sola_geometria.png)

## Passaggi sul canvas

**RHINO2SAP / 31 Import sola geometria**

Importazione della sola geometria da un SDB. Questo output non comprende materiali, proprieta o carichi.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  FILE ESISTENTE**

Sostituire il percorso con un .sdb reale. IMPORT SAP deve passare da False a True. Il sorgente viene aperto tramite una copia privata.

**File SAP esistente**

C:\Users\Public\Documents\modello_esistente.sdb

**02  GEOMETRIA**

Element Info espone nomi e tipi. Per creare un modello nuovo occorre riassegnare le proprieta; usare Import Model se si vuole mantenere il modello SAP completo.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| Read SAP Geometry | 26 Analisi e file | Path ← File SAP esistente.Panel; Run ← IMPORT SAP.Boolean Toggle |
| SAP Element Info | 34 Utility | Definition ← Read Geometry.Geometry |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
