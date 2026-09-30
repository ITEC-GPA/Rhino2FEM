# Raccolta tematica Grasshopper

Aprire i file .gh con Rhino 8 e il plugin completo dalla rispettiva cartella bin. Ogni file contiene geometria, gruppi e istruzioni.

- [Rhino2Straus](Rhino2Straus/examples/tutorials/README.md): 16 esempi.
- [Rhino2SAP](Rhino2SAP/examples/tutorials/README.md): 16 esempi.
- [Rhino2Midas](Rhino2Midas/examples/tutorials/README.md): 16 esempi.
- [Rhino2MidasGen](Rhino2MidasGen/examples/tutorials/README.md): 17 esempi.

[Importare un modello esistente](IMPORTAZIONE-MODELLI.md): il tutorial 11 dei quattro plugin collega il lettore al Model, alla validazione e alla preview. Per le raccolte specifiche di SAP e Straus consultare anche i rispettivi examples/README.md.

Rigenerazione dalla radice Rhino2FEM, dopo build.ps1:

    dotnet run --project Rhino2MidasGen/tools/Tutorials -c Release -- .

Aggiungere un filtro come 02_Mesh o Rhino2SAP per rigenerare una parte. Eseguire poi clean.ps1 per rimuovere gli intermedi.
