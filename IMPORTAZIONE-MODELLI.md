# Modello esistente → Model Grasshopper

Aprire **11_Import_model.gh** in examples/tutorials del progetto corrispondente. Collegare il Model del lettore a Preview, Summary, Validate e Decompose. Il comando parte spento: portarlo da false a true per importare o aggiornare.

| Prodotto | Componente | Origine | Comando |
|---|---|---|---|
| Straus7 R3 | Read Straus Model | Model path: file .st7 | Read |
| SAP2000 | Import SAP Model | Path: file .sdb | Run |
| Civil NX | Read Active Midas Model | Documento attivo e API Civil | Run |
| GEN NX | Read Active Midas GEN Model | Documento attivo e API GEN | Run |

## Civil NX e GEN NX

Configurare API Connection del prodotto corretto, aprire il modello in MIDAS e attivare la sua API. Non occorre passare da MGT/MCT.

Lasciare **Tables scollegato** per leggere UNIT, NODE, ELEM, STYP, MATL, SECT, THIK, STLD, CONS, NSPR, NMAS, CNLD, BMLD, PRES, SELF, LCOM-GEN, FRLS, OFFS, ELNK, GRUP, BNGR e LDGR. GEN legge anche STOR con livelli e impostazioni dei diaframmi. Il Model conserva le unità del documento e i dati originali delle tabelle ricevute.

Per dati specialistici aggiungere a Tables i nomi delle ulteriori tabelle /db, uno per riga. I nomi sono normalizzati e deduplicati. L'uscita **Tables** elenca le tabelle ricevute; **Issues** segnala tabelle assenti, dipendenze mancanti e limiti della ricostruzione. Gli elementi non visualizzabili mantengono il record nativo quando la loro tabella è stata letta.

La lettura usa solo GET: non crea, salva o analizza documenti. UNIT, NODE ed ELEM sono indispensabili; errori di autenticazione o connessione interrompono l'importazione. Una tabella facoltativa indisponibile viene segnalata, conservando le altre. Lo snapshot non comprende automaticamente ogni impostazione specialistica del file: aggiungere le tabelle necessarie e controllare Issues prima di esportare o calcolare. I risultati si leggono separatamente.

## SAP2000

Import SAP Model associa la rappresentazione Grasshopper alla versione del file sorgente verificata mediante hash. Le definizioni native complete rimangono nel file associato; export e analisi usano una copia verificata. Conservare il sorgente. Per modificare le definizioni native, aggiornarle in SAP e ripetere l'importazione. Issues descrive i limiti della rappresentazione GH. Il lettore della sola geometria resta disponibile.

## Straus7

Read Straus Model legge una copia .st7 e normalizza le unità in m/kN/kPa/tonnellate/°C. Identity map collega numeri e label nativi agli ID del Model, preservando nodi distinti coincidenti. Freedom case sceglie il caso dei vincoli. Report elenca le omissioni: con Allow partial=false il lettore trattiene il Model quando rileva dati strutturali non supportati. Allow partial=true accetta esplicitamente le omissioni indicate. Occorre la licenza API Straus7 R3.

## Dopo la lettura

1. Controllare Summary, Issues/Report e Validate.
2. Usare Decompose per nodi, elementi e definizioni.
3. Conservare le unità: cambiarne solo l'etichetta non converte le coordinate.
4. Leggere i risultati separatamente, usando il Model e lo stato sorgente corrispondenti.

I tutorial sono verificati con i solver spenti. I test dell'importazione MIDAS usano risposte HTTP simulate e non attestano ogni tabella su qualsiasi versione o licenza.
