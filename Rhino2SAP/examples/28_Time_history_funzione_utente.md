# 28 Time history funzione utente

Analisi dinamica modale lineare con impulso triangolare applicato al pattern Q.

Modalita: Analisi SAP su richiesta.

[Apri GH](28_Time_history_funzione_utente.gh) · [GHX](28_Time_history_funzione_utente.ghx) · [Canvas](28_time_history_funzione_utente.png)

## Passaggi sul canvas

**RHINO2SAP / 28 Time history funzione utente**

Analisi dinamica modale lineare con impulso triangolare applicato al pattern Q.
Unita: kN, m, C. Leggere le colonne da sinistra a destra. Input e geometria sono incorporati.
I valori sono didattici. Run e Bake partono da False. Le analisi SAP non sono state eseguite dal generatore.

**01  MATERIALE / SEZIONE**

Sezione rettangolare 0.20 x 0.10 m. E=210e6 kN/m2. I nomi delle definizioni devono essere univoci.

**02  ELEMENTI / VINCOLO**

Mensola di 3 m. Incastro al nodo BASE. Il nodo TIP coincide con l'estremita del frame F1.

**03  PATTERN / CARICO**

Pattern Q senza peso proprio. Forza al TIP: -1 kN lungo Z per la mensola orizzontale, +1 kN lungo X per quella verticale.

**04  MASSA / MODALE**

Massa unitaria al TIP; caso MODAL con tre modi richiesti. La massa include anche gli elementi. Periodi e masse partecipanti vanno controllati dopo il run.

**05  STORIA TEMPORALE**

Funzione PULSE: (t, fattore) = (0,0), (.1,1), (.2,0), (2,0). Il fattore moltiplica il carico Q, non e un accelerogramma.

**06  CARICO / PASSO**

LoadType=Load, LoadName=Q. 200 passi da .01 s; scala temporale Tf=1 e arrivo At=0. Smorzamento modale 5%.

**07  MODELLO**

Gli output Definition includono le dipendenze a monte. Collegare tutti i rami finali a Definitions; Build raccoglie una sola volta le dipendenze condivise.

**08  CONTROLLO / PREVIEW**

Valid = controlli gestiti. Non certifica la stabilita strutturale. Preview espone i Brep disponibili anche quando la visualizzazione e spenta.

**09  ANALISI SAP**

Scegliere un nuovo file .sdb nel pannello. Run: False > True. Per ripetere tornare prima a False. Overwrite=false. Occorre SAP2000 utilizzabile con API.

**File di destinazione**

C:\Users\Public\Documents\Rhino2SAP_28_Time_history_funzione_utente.sdb

**10  LETTURA**

Usare Case e StepNum per distinguere gli istanti; non mescolare valori di passi diversi.

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
| SAP Node Mass | 11 Masse | Name ← TIP.Element |
| SAP Source Mass Mass Source | 11 Masse | Definitions ← SetMass.Definition |
| SAP Load Cases Modal Eigen Case | 22 Modale e spettri | Definitions ← SetMassSource.Definition |
| SAP Load Cases Modal Eigen Number Modes | 22 Modale e spettri | Name ← SetCase.Definition |
| SAP Time History Function | 23 Time history |  |
| SAP Load Cases Mod Hist Linear Case | 23 Time history | Definitions ← PULSE.Definition, SetNumberModes.Definition |
| SAP Load Cases Mod Hist Linear Modal Case | 23 Time history | Name ← SetCase.Definition |
| SAP Load Cases Mod Hist Linear Loads | 23 Time history | Definitions ← Q.Pattern; Name ← SetModalCase.Definition |
| SAP Load Cases Mod Hist Linear Time Step | 23 Time history | Name ← SetLoads.Definition |
| SAP Load Cases Mod Hist Linear Damp Constant | 23 Time history | Name ← SetTimeStep.Definition |
| Build SAP Model | 01 Modello | Definitions ← Frame F1.Element, SetRestraint.Definition, SetLoadForce.Definition, SetDampConstant.Definition |
| Validate SAP Model | 01 Modello | Model ← Build Model.Model |
| SAP Display Settings | 33 Preview e bake | Enabled ← PREVIEW.Boolean Toggle |
| Preview SAP Model | 33 Preview e bake | Model ← Build Model.Model; Settings ← Visualizzazione.Settings |
| SAP Analyze and Embed Results | 26 Analisi e file | Model ← Build Model.Model; Path ← File di destinazione.Panel; Run ← RUN SAP.Boolean Toggle |
| SAP Model Joint Displ | 27 Risultati nodali | Model ← Analisi e risultati.Model |

## Verifica effettuata

Salvataggio e riapertura di GH e GHX; connessioni, impronte dei modelli, input richiesti, assenza di errori di soluzione e Brep chiusi. Tutti i trigger Run/Bake sono False. Nessuna analisi numerica SAP eseguita. I componenti dei risultati restano in attesa fino al calcolo. Le geometrie per cui il volume non e supportato sono indicate da Preview → Issues.
