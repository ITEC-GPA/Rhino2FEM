# 22 Sisma coefficiente utente

Forza sismica equivalente da coefficiente utente. Valori dimostrativi, senza applicazione di una normativa.

Modalita: Modellazione locale.

[Apri GH](22_Sisma_coefficiente_utente.gh) · [GHX](22_Sisma_coefficiente_utente.ghx) · [Canvas](22_sisma_coefficiente_utente.png)

## Passaggi sul canvas

**RHINO2SAP / 22 Sisma coefficiente utente**

Forza sismica equivalente da coefficiente utente. Valori dimostrativi, senza applicazione di una normativa.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / SEZIONE**

Sezione rettangolare 0.20 x 0.10 m. E=210e6 kN/m2. I nomi delle definizioni devono essere univoci.

**02  ELEMENTI / VINCOLO**

Mensola di 3 m. Incastro al nodo BASE. Il nodo TIP coincide con l'estremita del frame F1.

**03  PATTERN / CARICO**

Pattern Q senza peso proprio. Forza al TIP: -1 kN lungo Z per la mensola orizzontale, +1 kN lungo X per quella verticale.

**04  DIAFRAMMA / MASSA**

Diaframma D1 in piano XY: asse Z globale. Nodo TIP a quota 3 m; massa traslazionale unitaria. Esempio minimo, non edificio di progetto.

**05  SISMA UTENTE**

EQX: Type=5. DirFlag=1 (X), C=.10, K=1, quote da 0 a 3 m; eccentricita zero. Coefficiente puramente didattico.

**06  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**07  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

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
| SAP Constraint Def Diaphragm | 08 Vincoli e molle |  |
| SAP Node Constraint | 08 Vincoli e molle | Name ← TIP.Element; ConstraintName ← SetDiaphragm.Definition |
| SAP Node Mass | 11 Masse | Name ← TIP.Element |
| SAP Source Mass Mass Source | 11 Masse | Definitions ← SetMass.Definition |
| SAP Load Pattern | 12 Load pattern |  |
| SAP Load Patterns Auto Seismic User Coefficient | 18 Sisma | Definitions ← SetConstraint.Definition; Name ← EQX.Pattern |
| Build SAP Model | 01 Modello | Definitions ← Frame F1.Element, SetRestraint.Definition, SetLoadForce.Definition, SetMassSource.Definition, SetUserCoefficient.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
