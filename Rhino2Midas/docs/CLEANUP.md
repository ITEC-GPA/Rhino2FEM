# Pulizia della cartella

Restano **Rhino2Straus, Rhino2SAP e Rhino2Midas**, con README e metadati Git. Non ci sono riferimenti di progetto alle cartelle rimosse.

Prima della rimozione è stato verificato un backup esterno di **577 file**, comprendente sorgenti e binari legacy, esclusi obj e cache .vs:

    C:\Users\G4B9B~1.PAC\AppData\Local\Temp\Rhino2-legacy-backup-20260926-141946.zip

SHA256: `DB9A22CADADDE160EE70613534B274FB8FAEC10A7E9DAE4D6699D1B28BB66EEA`

Il backup contiene anche le modifiche preesistenti a `CircularSectionShearCheckComponent.cs`, `RectangularSectionShearCheckComponent.cs` e il file non tracciato `FailureDomain2DComponent.cs`.

Cartelle rimosse: Fem2Rhino, Fem2Rhino.Grasshopper, Fem2Rhino.Grasshopper.Reading, GrasshopperHelper, ProxyHelper, Rhino2Fem.Grasshopper.Straus, Rhino2Fem.Straus.Grasshopper, Rhino2Midas.Core, Rhino2Midas.Grasshopper, Rhino2Midas.Rhino, bin e .vs. Rimossa anche la soluzione Rhino2Fem.sln.

I tre nuovi progetti mantengono le proprie soluzioni, librerie, strumenti, documentazione e pacchetti. SAP e Straus compilano autonomamente e superano rispettivamente 32 e 64 test; Midas supera 49 test gestiti e il collaudo Rhino descritto in VALIDATION.

Il backup è in una cartella temporanea esterna al repository: conservarlo altrove se si desidera mantenerlo a lungo.

Aggiornamento del 27 settembre 2026: aggiunto il quarto progetto Rhino2MidasGen, controllati gli ingressi dei quattro plugin e predisposta la cartella principale con nome Rhino2FEM. Rhino2Straus conserva il proprio repository come sottomodulo. Commit e push sono stati richiesti esplicitamente dall'utente.
