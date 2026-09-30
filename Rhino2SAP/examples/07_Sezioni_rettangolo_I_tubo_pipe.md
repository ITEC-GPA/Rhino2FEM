# 07 Sezioni rettangolo I tubo pipe

Quattro sezioni: rettangolare, doppio T, tubolare rettangolare e circolare cavo. Preview e solidi reali.

Modalita: Modellazione locale.

[Apri GH](07_Sezioni_rettangolo_I_tubo_pipe.gh) · [GHX](07_Sezioni_rettangolo_I_tubo_pipe.ghx) · [Canvas](07_sezioni_rettangolo_i_tubo_pipe.png)

## Passaggi sul canvas

**RHINO2SAP / 07 Sezioni rettangolo I tubo pipe**

Quattro sezioni: rettangolare, doppio T, tubolare rettangolare e circolare cavo. Preview e solidi reali.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE**

Acciaio isotropo, unita kN/m. Le dimensioni sotto sono in metri.

**02  SEZIONI**

R: 0.20 x 0.10. I: h=.30, b=.15, tf=.012, tw=.008. RHS: .25 x .15 x .01. CHS: diametro .20, t=.01.

**03  FRAME**

Quattro assi paralleli lunghi 3 m. Esempio geometrico: non contiene vincoli o carichi.

**04  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**05  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

## Componenti e collegamenti

| Componente | Gruppo Grasshopper | Input collegati |
|---|---|---|
| SAP Isotropic Material | 02 Materiali |  |
| SAP Frame Section Rectangle | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Section ISection | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Section Tube | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Section Pipe | 03 Sezioni beam | MatProp ← Acciaio.Material |
| SAP Frame Element | 07 Elementi | Property ← SetRectangle.Definition |
| SAP Frame Element | 07 Elementi | Property ← SetISection.Definition |
| SAP Frame Element | 07 Elementi | Property ← SetTube.Definition |
| SAP Frame Element | 07 Elementi | Property ← SetPipe.Definition |
| Build SAP Model | 01 Modello | Definitions ← F1.Element, F2.Element, F3.Element, F4.Element |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
