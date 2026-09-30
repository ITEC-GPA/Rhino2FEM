# 08 Materiali elastici libreria CSI

Confronto fra materiale elastico esplicito e materiale dalla libreria europea CSI.

Modalita: Modellazione locale.

[Apri GH](08_Materiali_elastici_libreria_CSI.gh) · [GHX](08_Materiali_elastici_libreria_CSI.ghx) · [Canvas](08_materiali_elastici_libreria_csi.png)

## Passaggi sul canvas

**RHINO2SAP / 08 Materiali elastici libreria CSI**

Confronto fra materiale elastico esplicito e materiale dalla libreria europea CSI.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  ELASTICO**

Material definisce E, Poisson, dilatazione e peso specifico. Non aggiunge da solo dati normativi di progetto.

**02  LIBRERIA CSI**

DB Material richiede SAP per risolvere le proprieta al momento dell'export. Standard e grado devono esistere nella libreria installata.

**03  ELEMENTI**

La forma geometrica delle due sezioni coincide. Il materiale di database verra completato da SAP.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Database Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← S355 da database.Material |
| SAP Definition Info | 34 Utility | Definition ← S355 da database.Material |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| Build SAP Model | 01 Modello | Definitions ← Elastico.Element, Database.Element |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
