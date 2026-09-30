# Leggere un modello Civil NX esistente

Aprire examples/tutorials/11_Import_model.gh, configurare API Connection e aprire il modello nel programma Civil NX. Attivare la sua API e portare READ MODEL da false a true.

Il componente Read Active Midas Model restituisce un Model direttamente collegabile a Validate, Summary, Preview e Decompose. Non crea o salva documenti: esegue soltanto richieste GET.

Tables in ingresso è opzionale. Le tabelle automatiche sono UNIT, NODE, ELEM, STYP, MATL, SECT, THIK, STLD, CONS, NSPR, NMAS, CNLD, BMLD, PRES, SELF, LCOM-GEN, FRLS, OFFS, ELNK, GRUP, BNGR e LDGR. Per informazioni specialistiche aggiungere i relativi nomi /db, uno per riga.

Tables in uscita indica le tabelle ricevute. Issues riporta quelle indisponibili, le dipendenze mancanti e il perimetro dello snapshot. Gli errori su UNIT/NODE/ELEM, autenticazione o connessione impediscono la pubblicazione di un nuovo Model. Le tabelle facoltative indisponibili vengono segnalate conservando le altre.

Il Model mantiene unità, connettività e record nativi delle tabelle lette. Non cambiare solo l'etichetta delle unità per convertire coordinate o carichi. Lo snapshot non comprende automaticamente tutte le impostazioni specialistiche del file: verificarne la copertura prima di esportare o analizzare. I risultati si leggono con i componenti dedicati.

I file tutorial partono con i comandi esterni spenti e non contengono risultati sintetici. La verifica HTTP dell'importatore è automatica con trasporto simulato; la versione nativa installata va collaudata con la sua connessione e licenza.
