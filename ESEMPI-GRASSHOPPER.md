# Esempi Grasshopper — Rhino2FEM

Quattro definizioni **.gh** con componenti reali collegati, slider, gruppi colorati e note sul canvas. Le geometrie sono generate nella definizione: non occorrono file Rhino esterni. Accanto a ogni .gh trovi lo stesso documento in **.ghx**, leggibile come XML, e un inventario dei collegamenti in .connections.json.

## Aprire e provare

1. Installa il pacchetto completo del plugin corrispondente in Rhino 8 / Grasshopper, comprese le DLL distribuite con il .gha; riavvia Rhino dopo l'installazione.
2. Imposta il documento Rhino in **metri** e apri un esempio .gh in Grasshopper. Le unità dei modelli sono **kN / m / C**.
3. Esegui **Zoom Extents** nel viewport Rhino. Sul canvas segui i gruppi numerati da sinistra a destra e modifica gli slider gialli.
4. Controlla `VALIDAZIONE` (True), il riepilogo e la geometria. `Decompose Model` mostra gli elementi del Model; il componente seguente ne espone i dati.

Gli esempi costruiscono e visualizzano il modello senza avviare Straus7, SAP2000 o le API MIDAS. Non contengono risultati di analisi precalcolati. `Validate` verifica i dati del modello, non la sua adeguatezza strutturale. Le combinazioni proposte sono dimostrative.

| Esempio | File da aprire | Componenti del plugin | Modello iniziale |
|---|---|---:|---|
| Straus7: mensola parametrica | [FEM_01_Straus_Mensola.gh](Rhino2Straus/examples/FEM_01_Straus_Mensola.gh) | 18 | 2 nodi, 1 trave |
| SAP2000: portale parametrico | [FEM_02_SAP_Portale.gh](Rhino2SAP/examples/FEM_02_SAP_Portale.gh) | 17 | 4 nodi, 3 frame |
| Civil NX: trave e combinazione | [FEM_03_Civil_Trave.gh](Rhino2Midas/examples/FEM_03_Civil_Trave.gh) | 20 | 2 nodi, 1 trave |
| GEN NX: parete e piani | [FEM_04_GEN_Parete.gh](Rhino2MidasGen/examples/FEM_04_GEN_Parete.gh) | 19 | 4 nodi, 1 parete |

## 01 — Straus7: mensola

Lunghezza 3 m; sezione rettangolare b = 0,20 m e h = 0,30 m; acciaio E = 210·10⁶ kN/m²; incastro all'origine e forza Fz = −10 kN all'estremo libero, caso LC1.

Provare a cambiare **L**, **b**, **h** e **Fz**. Il punto finale, la trave e il nodo caricato seguono L. Il toggle `Incastro: 6 DOF` alimenta tutte e sei le condizioni del supporto: lasciarlo True per conservare lo schema della mensola.

Flusso: Construct Point → Line → Frame element; Material + Frame Section → Frame Property → Frame element; Node Element → Support / Load nodal; tutto confluisce in Build Model → Validate / Summary / Display Manager / Physical Geometry / Decompose Model.

## 02 — SAP2000: portale

Portale nel piano XZ, luce L = 6 m e altezza H = 3 m, entrambe le basi incastrate. Sezione in calcestruzzo b = 0,25 m e h = 0,35 m; E = 30·10⁶ kN/m². G contiene il peso proprio; Q applica q = −8 kN/m in Z globale **alla sola trave orizzontale**. La combinazione ULS usa 1,35 G + 1,50 Q.

Provare a cambiare **L**, **H**, le dimensioni della sezione e **q**. `SAP Frame Element` associa in ordine le tre linee ai nomi ColL, Beam e ColR. Il carico usa esplicitamente il nome Beam. La lista unità fornisce un unico valore `kN_m_C = 6` al Build.

Il componente nativo `SAP Frame Rectangle` raccoglie la dipendenza del materiale; i componenti di vincolo e carico aggiungono comandi differiti al Model. Nessun comando viene eseguito in SAP aprendo questo file.

## 03 — Civil NX: trave e combinazione

Mensola di 6 m; sezione rettangolare in calcestruzzo b = 0,30 m e h = 0,50 m; E = 30·10⁶ kN/m². G è il peso proprio, Q è q = −8 kN/m in GZ, ULS = 1,35 G + 1,50 Q.

Gli slider **L**, **b**, **h** e **q** mostrano la propagazione delle modifiche. I nodi espliciti 1 e 2 vengono passati alla trave, il supporto è sul nodo 1. `Midas Static Load Case` emette sia la definizione del caso sia il suo nome: quest'ultimo è collegato ai carichi e alla combinazione, mentre le definizioni entrano nel Build.

## 04 — GEN NX: parete, piani e diaframma

Parete nel piano XZ: larghezza 4 m, altezza 3 m, spessore 0,20 m, E = 30·10⁶ kN/m². I nodi inferiori 1 e 2 sono incastrati; i nodi superiori 3 e 4 ricevono ciascuno FX = +5 kN nel caso WIND, per un totale di 10 kN.

Modificare **L**, **h** e **t**. I quattro vertici e il piano Roof seguono le dimensioni; il piano Base resta a Z = 0. Il toggle `Diaframma Roof` controlla il diaframma del piano superiore. Il Wall ID iniziale è 1, distinto concettualmente dall'ID dell'elemento.

Per cambiare la forza, clic destro sull'ingresso `Loads` di `Midas GEN Nodal Load`: i sei numeri sono nell'ordine FX, FY, FZ, MX, MY, MZ. Il medesimo elenco viene applicato a entrambi i nodi superiori.

## Verifiche e rigenerazione

Controllati in Rhino/Grasshopper reali: calcolo di tutti i componenti, Model valido, numero di nodi ed elementi, Brep validi e chiusi, salvataggio e riapertura di entrambi i formati e conservazione dei collegamenti. È stata provata anche una variazione del 10% della prima dimensione, poi ripristinata prima del salvataggio.

Volumi iniziali dei solidi separati: Straus **0,18 m³**; SAP **1,05 m³**; Civil **0,90 m³**; GEN **2,40 m³**. Nel portale SAP il valore è la somma dei volumi dei tre elementi, senza unione booleana degli incroci.

Il generatore si trova in [Rhino2MidasGen/tools/ExamplesGenerator](Rhino2MidasGen/tools/ExamplesGenerator/README.md). Dopo aver eseguito build.ps1 per i quattro plugin (assembly nelle rispettive cartelle bin), dalla cartella Rhino2FEM:

```powershell
dotnet run --project Rhino2MidasGen/tools/ExamplesGenerator -c Release -- .
```

Il generatore richiede Rhino 8 installato e funzionante. Il suo caricamento isolato usa API interne di Grasshopper 8; i file .gh/.ghx contengono soltanto componenti standard e dei rispettivi plugin, senza script nascosti né dipendenze dal generatore.