# Impostazione Straus e corrispondenze SAP

Il riferimento è Rhino2Straus, 162 componenti nella cartella adiacente. Il plugin SAP usa lo stesso formato `.gha`, Rhino 8/.NET 8 x64, componenti con GUID stabili, input immutabili, Model unico, preview disattivabile, export/analisi su fronte di Run e risultati riutilizzabili nei componenti downstream. I GUID SAP sono separati da quelli Straus, così i due plugin possono convivere.

Il catalogo SAP contiene **559 componenti**. L'SDK locale è stato indicizzato integralmente; i componenti generati coprono assegnazioni e definizioni selezionate e tutti i metodi di risultato, con socket e valori enum corrispondenti alle firme effettive. Non è dichiarata equivalenza numerica fra i due solver.

| Famiglia Straus | Equivalente SAP |
|---|---|
| Material, Database Steel/Concrete, strengths | Isotropic Material, Material Library, Database Material, metodi PropMaterial per comportamento e resistenze |
| Sezioni rettangolari, circolari, pipe, box, I, T, C, L, generiche | PropFrame.SetRectangle/Circle/Pipe/Tube/ISection/Tee/Channel/Angle/General e relative versioni aggiornate |
| Profili sottili lipped, Z, hat | SetColdC/ColdZ/ColdHat/ColdT/ColdL/ColdI/ColdBox/ColdPipe |
| Librerie di sezioni | PropFrame.ImportProp, librerie XML CSI installate |
| Frame property, assignment, behaviour | Proprietà PropFrame, FrameObj.SetSection/SetModifiers/SetTCLimits e altri attributi nativi |
| Area property, thickness, layered shell | PropArea.SetShell/SetShell_1/SetShellLayer_2, proprietà plane/asolid e modificatori |
| Link rigidi, elastici e vincoli cinematici | PropLink.SetLinear, ConstraintDef.SetBody/SetEqual/SetBeam/SetPlate/SetRod/SetWeld; link nonlinear, isolatori, gap, hook e dampers |
| Nodi, frame, area, solidi, link | Sei famiglie di elementi SAP, inclusi anche cable |
| Mesh to Plates, Polyline to Beams | Mesh to SAP Areas, Polyline to SAP Frames |
| Support, mass, spring, prescribed displacement | PointObj.SetRestraint/SetMass/SetSpring/SetLoadDispl |
| Releases, offset, insertion, orientation | FrameObj.SetReleases/SetInsertionPoint/SetLocalAxes e AreaObj.SetOffsets/SetLocalAxes |
| Allineamento a vettore, punto e piano | Align Frame, Frame To Point, Area, Area To Plane e Model; chiamate agli assi avanzati nativi |
| Groups | GroupDef.SetGroup e SetGroupAssign per ciascuna famiglia |
| Casi, fattori, combinazioni, self-weight | Load Pattern, Linear/Nonlinear/Buckling Case, Load Combination e metodi LoadCases; fattori e moltiplicatore self-weight nei relativi socket |
| Carichi nodali, distribuiti, puntuali, momenti | PointObj.SetLoadForce e FrameObj.SetLoadDistributed/SetLoadPoint, con direzioni e MyType nativi |
| Temperature, strain/preload, pressione e gravità | Famiglie SetLoadTemperature/Strain/SurfacePressure/Gravity/Uniform; dati e convenzioni native SAP |
| Build, merge, validate, summary, units, filters | Componenti SAP dedicati e filtro unico per tutte le famiglie |
| Decompose Model, info elementi/proprietà | Decompose SAP Model/Beam/Plate, Element Info, Definition Info, Read SAP API Table per proprietà native |
| Clear Results | Clear SAP Model Results, inclusi i risultati incorporati negli elementi |
| Export, geometry import, bake | Export SAP2000 Model, Read SAP Geometry, Bake SAP Geometry |
| Analisi lineare, nonlineare, modale, buckling | Configurazione casi LoadCases e unico Analyze and Embed Results; stesso worker per tutti i solver |
| Risultati nodali, frame, shell, solidi, link, modali/buckling | Componenti specifici per tutti i metodi Results; quantità native non riordinate |
| Beam actions e plate actions | Lettori del singolo elemento con risultati incorporati e lettori del Model |
| Diagrammi, deformata, estremi e verifica futura | Frame Force Diagram, Deformed Geometry, Result Extrema, Verification Data |
| Preview Model e geometria fisica | Preview SAP Model, Physical Geometry, preview automatica di Model/elementi/sezioni, Settings e pannello globale |

## Differenze che non vengono mascherate

- BXS e BGL Straus non sono formati/proprietà CSI. Usare le librerie SAP, Section Designer o proprietà General. Non è incluso un convertitore BXS, né vengono inventate proprietà per triangoli/cruciformi/sezioni generiche.
- Il vincolo Straus SectorSymmetry non è riprodotto automaticamente con un vincolo SAP diverso. Sono disponibili i vincoli CSI, con i propri DOF e sistemi di coordinate.
- L'OAPI non espone un metodo `FrameStress` o `FrameStrain` equivalente alle tabelle Straus. Sono disponibili forze frame, azioni nodali e query DatabaseTables; nessun valore di tensione/deformazione di trave viene inventato.
- Le deformazioni shell/solid vengono restituite secondo il loro significato CSI. Non vengono rinominate arbitrariamente come elastiche/totali Straus; non è presente una conversione implicita in curvature.
- Le deformate del plugin interpolano i vertici/estremi. Le deformate interne esatte delle travi richiedono ulteriori dati/stazioni di analisi e non sono simulate con un'interpolazione presentata come risultato nativo.
- Read SAP Geometry importa solo la geometria. Non ricostruisce automaticamente materiali, leggi non lineari e carichi di un modello arbitrario esterno.
- La preview completa è gestibile per categorie; le forme/assegnazioni complesse sono riconoscibili tramite simboli e valori, con limiti elencati nell'uscita Issues e nella guida Preview.
