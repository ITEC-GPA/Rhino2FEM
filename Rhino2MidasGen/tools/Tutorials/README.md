# Generatore tutorial Rhino2FEM

Richiede Windows, .NET 8, Rhino 8 e Grasshopper funzionanti. Carica le quattro distribuzioni finali nelle cartelle bin.

Dalla radice, dopo build.ps1:

    dotnet run --project Rhino2MidasGen/tools/Tutorials -c Release -- .
    dotnet run --project Rhino2MidasGen/tools/Tutorials -c Release -- . Rhino2Midas
    dotnet run --project Rhino2MidasGen/tools/Tutorials -c Release -- . 11_Import
    ./clean.ps1

Il filtro è una sottostringa di progetto/nome; Rhino2Midas include anche GEN. Si rigenerano i soli esempi selezionati e gli indici. Le definizioni contengono componenti reali, fili, geometria incorporata e parametri persistenti. Gli ingressi list di Build Model vengono appiattiti per raccogliere un unico Model.

Ogni definizione viene risolta nel vero motore Grasshopper, salvata e riaperta in .gh e .ghx. Si controllano errori, uscite dei rami attivi, Model valido, nodi/elementi, Brep chiusi, oggetti/fili e comandi esterni spenti. Dove presenti slider e Brep attesi, una variazione del 10% deve cambiare il volume; il valore iniziale viene poi ripristinato. I .checks.json registrano le verifiche applicabili.

I rami import/solver/risultati richiedono RUN/READ e i dati reali. Non sono incorporati risultati fittizi. Input vuoti nei rami spenti sono attesi; gli errori di runtime interrompono la generazione.

La registrazione usa API interne di Grasshopper 8. I file prodotti non dipendono dal generatore.

Si confrontano anche le impronte del Model prima e dopo la riapertura. Per ogni plugin gli esempi 12 (analisi) e 14 (rilettura) devono avere impronte identiche; rigenerare 12 prima di 14 se cambia la struttura di riferimento.
