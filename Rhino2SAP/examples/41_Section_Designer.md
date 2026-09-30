# 41 Section Designer

Sezione Section Designer con una forma rettangolare piena. Esempio di definizioni dipendenti.

Modalita: Modellazione locale.

[Apri GH](41_Section_Designer.gh) · [GHX](41_Section_Designer.ghx) · [Canvas](41_section_designer.png)

## Passaggi sul canvas

**RHINO2SAP / 41 Section Designer**

Sezione Section Designer con una forma rettangolare piena. Esempio di definizioni dipendenti.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  SEZIONE SD**

Il materiale base e STEEL; DesignType=0. SDShape aggiunge una forma alla sezione esistente.

**02  FORMA**

Rettangolo centrato: H=.30, W=.20, nessuna rotazione. La preview fisica di Section Designer non e supportata; consultare Issues.

**03  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**04  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section SDSection | 04 Section Designer | MatProp ← Acciaio.Material |
| SAP Prop Frame SDShape Solid Rect | 04 Section Designer | Name ← SetSDSection.Definition; MatProp ← Acciaio.Material |
| SAP Frame Element | 07 Elementi | Property ← SetSolidRect.Definition |
| Build SAP Model | 01 Modello | Definitions ← SD F1.Element |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
