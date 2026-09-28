# Generatore degli esempi Rhino2FEM

Crea quattro definizioni .gh e .ghx nelle cartelle examples dei quattro progetti. Ogni grafo usa componenti Grasshopper nativi e del relativo plugin, con geometria parametrica, proprietà, vincoli, carichi, Model, validazione, preview e decomposizione.

Prerequisiti: Windows, .NET 8, Rhino 8 e build.ps1 di Rhino2Straus, Rhino2SAP, Rhino2Midas e Rhino2MidasGen (assembly nelle rispettive cartelle bin). Dalla radice Rhino2FEM:

```powershell
dotnet run --project Rhino2MidasGen/tools/ExamplesGenerator -c Release -- .
```

Il processo Rhino.Inside è isolato e carica solo gli assembly necessari. Le API interne `m_comServer` e `LoadExternalFiles` servono esclusivamente a limitare questo caricamento nel generatore. Le definizioni salvate sono normali documenti Grasshopper e non hanno questa dipendenza.

Prima di salvare, il generatore controlla il calcolo dei componenti, la validazione dei modelli, nodi, elementi e volumi fisici; aumenta del 10% la prima dimensione e verifica la propagazione, quindi ripristina i valori iniziali. Riapre sia .gh sia .ghx, verifica conteggio degli oggetti e dei collegamenti e ripete il calcolo. Il file .connections.json contiene l'inventario dei collegamenti e gli esiti. Il solver strutturale non viene avviato.

Guida d'uso: [ESEMPI-GRASSHOPPER.md](../../../ESEMPI-GRASSHOPPER.md).