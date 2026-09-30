# 04 Creazione unione modelli

Creare due sottomodelli e unirli. Unita e tolleranza devono coincidere; gli elementi condivisi si raccolgono una volta.

Modalita: Modellazione locale.

[Apri GH](04_Creazione_unione_modelli.gh) · [GHX](04_Creazione_unione_modelli.ghx) · [Canvas](04_creazione_unione_modelli.png)

## Passaggi sul canvas

**RHINO2SAP / 04 Creazione unione modelli**

Creare due sottomodelli e unirli. Unita e tolleranza devono coincidere; gli elementi condivisi si raccolgono una volta.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  PROPRIETA**

Un materiale e una sezione comuni a entrambi i sottomodelli.

**02  ELEMENTI**

Due frame consecutivi; il nodo comune viene riconosciuto dalla tolleranza. Esempio geometrico senza analisi.

**03  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  UNIONE**

Merge mantiene i nomi e rifiuta geometrie diverse con lo stesso nome. Il modello risultante contiene due frame e tre nodi.

**06  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| Build SAP Model | 01 Modello | Definitions ← F1.Element |
| Build SAP Model | 01 Modello | Definitions ← F2.Element |
| Merge SAP Models | 01 Modello | Models ← Build Model.Model, Build Model.Model |
| Decompose SAP Model | 01 Modello | Model ← Unisci.Model |
| Validate SAP Model | 01 Modello | Model ← Unisci.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Unisci.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
