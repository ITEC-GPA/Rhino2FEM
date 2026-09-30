# Catalogo componenti Rhino2SAP

**565 componenti in 34 argomenti**, tutti nella scheda **SAP2000** di Grasshopper. I titoli qui sotto corrispondono ai pannelli del plugin; i numeri mantengono l'ordine dei gruppi. GUID, nomi e icone sono verificati dal caricamento reale in Rhino.

| Argomento / pannello | Numero |
|---|---:|
| [01 Modello](#01-modello) | 8 |
| [02 Materiali](#02-materiali) | 30 |
| [03 Sezioni beam](#03-sezioni-beam) | 46 |
| [04 Section Designer](#04-section-designer) | 24 |
| [05 Piastre e solidi](#05-piastre-e-solidi) | 11 |
| [06 Link e cavi](#06-link-e-cavi) | 21 |
| [07 Elementi](#07-elementi) | 8 |
| [08 Vincoli e molle](#08-vincoli-e-molle) | 20 |
| [09 Assi e offset](#09-assi-e-offset) | 23 |
| [10 Mesh e assegnazioni](#10-mesh-e-assegnazioni) | 48 |
| [11 Masse](#11-masse) | 9 |
| [12 Load pattern](#12-load-pattern) | 6 |
| [13 Carichi nodali](#13-carichi-nodali) | 2 |
| [14 Carichi beam](#14-carichi-beam) | 8 |
| [15 Carichi plate](#15-carichi-plate) | 10 |
| [16 Altri carichi](#16-altri-carichi) | 21 |
| [17 Vento](#17-vento) | 37 |
| [18 Sisma](#18-sisma) | 31 |
| [19 Casi statici](#19-casi-statici) | 15 |
| [20 Casi non lineari](#20-casi-non-lineari) | 15 |
| [21 Fasi costruttive](#21-fasi-costruttive) | 15 |
| [22 Modale e spettri](#22-modale-e-spettri) | 22 |
| [23 Time history](#23-time-history) | 35 |
| [24 Altri casi dinamici](#24-altri-casi-dinamici) | 22 |
| [25 Combinazioni](#25-combinazioni) | 7 |
| [26 Analisi e file](#26-analisi-e-file) | 8 |
| [27 Risultati nodali](#27-risultati-nodali) | 10 |
| [28 Risultati beam](#28-risultati-beam) | 5 |
| [29 Risultati plate](#29-risultati-plate) | 12 |
| [30 Risultati solidi-link](#30-risultati-solidi-link) | 8 |
| [31 Risultati modali](#31-risultati-modali) | 6 |
| [32 Risultati e query](#32-risultati-e-query) | 12 |
| [33 Preview e bake](#33-preview-e-bake) | 6 |
| [34 Utility](#34-utility) | 4 |

## 01 Modello

| Componente | Metodo API |
|---|---|
| [Build SAP Model](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [Clear SAP Model Results](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [Decompose SAP Model](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [Merge SAP Models](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [SAP Axial Verification Example](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |
| [SAP Model Summary](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [SAP Model Units](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [Validate SAP Model](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |

## 02 Materiali

| Componente | Metodo API |
|---|---|
| [SAP Database Material](../Rhino2SAP.Grasshopper/ReadComponents.cs) | — |
| [SAP Isotropic Material](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |
| [SAP Material Coupled Model Type](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetCoupledModelTypeComponent.cs) | `PropMaterial.SetCoupledModelType` |
| [SAP Material Damping](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetDampingComponent.cs) | `PropMaterial.SetDamping` |
| [SAP Material Library](../Rhino2SAP.Grasshopper/ReadComponents.cs) | — |
| [SAP Material Material](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetMaterialComponent.cs) | `PropMaterial.SetMaterial` |
| [SAP Material MPAnisotropic](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetMPAnisotropicComponent.cs) | `PropMaterial.SetMPAnisotropic` |
| [SAP Material MPIsotropic](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetMPIsotropicComponent.cs) | `PropMaterial.SetMPIsotropic` |
| [SAP Material MPOrthotropic](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetMPOrthotropicComponent.cs) | `PropMaterial.SetMPOrthotropic` |
| [SAP Material MPUniaxial](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetMPUniaxialComponent.cs) | `PropMaterial.SetMPUniaxial` |
| [SAP Material OAluminum](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOAluminumComponent.cs) | `PropMaterial.SetOAluminum` |
| [SAP Material OCold Formed](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOColdFormedComponent.cs) | `PropMaterial.SetOColdFormed` |
| [SAP Material OConcrete](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOConcreteComponent.cs) | `PropMaterial.SetOConcrete` |
| [SAP Material OConcrete 1](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOConcrete_1Component.cs) | `PropMaterial.SetOConcrete_1` |
| [SAP Material OConcrete 2](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOConcrete_2Component.cs) | `PropMaterial.SetOConcrete_2` |
| [SAP Material ONo Design](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetONoDesignComponent.cs) | `PropMaterial.SetONoDesign` |
| [SAP Material ORebar](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetORebarComponent.cs) | `PropMaterial.SetORebar` |
| [SAP Material ORebar 1](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetORebar_1Component.cs) | `PropMaterial.SetORebar_1` |
| [SAP Material OSteel](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOSteelComponent.cs) | `PropMaterial.SetOSteel` |
| [SAP Material OSteel 1](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOSteel_1Component.cs) | `PropMaterial.SetOSteel_1` |
| [SAP Material OTendon](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOTendonComponent.cs) | `PropMaterial.SetOTendon` |
| [SAP Material OTendon 1](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetOTendon_1Component.cs) | `PropMaterial.SetOTendon_1` |
| [SAP Material SSCurve](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetSSCurveComponent.cs) | `PropMaterial.SetSSCurve` |
| [SAP Material Temp](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetTempComponent.cs) | `PropMaterial.SetTemp` |
| [SAP Material Von Mises Plasticity Parameters](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetVonMisesPlasticityParametersComponent.cs) | `PropMaterial.SetVonMisesPlasticityParameters` |
| [SAP Material Weight And Mass](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetWeightAndMassComponent.cs) | `PropMaterial.SetWeightAndMass` |
| [SAP Prop Material Time Dep Concrete CEBFIP90](../Rhino2SAP.Grasshopper/Generated/PropMaterial_TimeDep_SetConcreteCEBFIP90Component.cs) | `PropMaterial.TimeDep.SetConcreteCEBFIP90` |
| [SAP Prop Material Time Dep Concrete Scale Factors](../Rhino2SAP.Grasshopper/Generated/PropMaterial_TimeDep_SetConcreteScaleFactorsComponent.cs) | `PropMaterial.TimeDep.SetConcreteScaleFactors` |
| [SAP Prop Material Time Dep Tendon CEBFIP90](../Rhino2SAP.Grasshopper/Generated/PropMaterial_TimeDep_SetTendonCEBFIP90Component.cs) | `PropMaterial.TimeDep.SetTendonCEBFIP90` |
| [SAP Prop Material Time Dep Tendon Scale Factors](../Rhino2SAP.Grasshopper/Generated/PropMaterial_TimeDep_SetTendonScaleFactorsComponent.cs) | `PropMaterial.TimeDep.SetTendonScaleFactors` |

## 03 Sezioni beam

| Componente | Metodo API |
|---|---|
| [SAP Frame Section Angle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetAngleComponent.cs) | `PropFrame.SetAngle` |
| [SAP Frame Section Angle 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetAngle_1Component.cs) | `PropFrame.SetAngle_1` |
| [SAP Frame Section Auto Select Aluminum](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetAutoSelectAluminumComponent.cs) | `PropFrame.SetAutoSelectAluminum` |
| [SAP Frame Section Auto Select Cold Formed](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetAutoSelectColdFormedComponent.cs) | `PropFrame.SetAutoSelectColdFormed` |
| [SAP Frame Section Auto Select Steel](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetAutoSelectSteelComponent.cs) | `PropFrame.SetAutoSelectSteel` |
| [SAP Frame Section Channel](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetChannelComponent.cs) | `PropFrame.SetChannel` |
| [SAP Frame Section Channel 2](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetChannel_2Component.cs) | `PropFrame.SetChannel_2` |
| [SAP Frame Section Circle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetCircleComponent.cs) | `PropFrame.SetCircle` |
| [SAP Frame Section Cold Box](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdBoxComponent.cs) | `PropFrame.SetColdBox` |
| [SAP Frame Section Cold C](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdCComponent.cs) | `PropFrame.SetColdC` |
| [SAP Frame Section Cold Hat](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdHatComponent.cs) | `PropFrame.SetColdHat` |
| [SAP Frame Section Cold I](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdIComponent.cs) | `PropFrame.SetColdI` |
| [SAP Frame Section Cold L](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdLComponent.cs) | `PropFrame.SetColdL` |
| [SAP Frame Section Cold Pipe](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdPipeComponent.cs) | `PropFrame.SetColdPipe` |
| [SAP Frame Section Cold T](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdTComponent.cs) | `PropFrame.SetColdT` |
| [SAP Frame Section Cold Z](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetColdZComponent.cs) | `PropFrame.SetColdZ` |
| [SAP Frame Section Cover Plated I](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetCoverPlatedIComponent.cs) | `PropFrame.SetCoverPlatedI` |
| [SAP Frame Section Dbl Angle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetDblAngleComponent.cs) | `PropFrame.SetDblAngle` |
| [SAP Frame Section Dbl Angle 2](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetDblAngle_2Component.cs) | `PropFrame.SetDblAngle_2` |
| [SAP Frame Section Dbl Channel](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetDblChannelComponent.cs) | `PropFrame.SetDblChannel` |
| [SAP Frame Section Dbl Channel 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetDblChannel_1Component.cs) | `PropFrame.SetDblChannel_1` |
| [SAP Frame Section General](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetGeneralComponent.cs) | `PropFrame.SetGeneral` |
| [SAP Frame Section General 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetGeneral_1Component.cs) | `PropFrame.SetGeneral_1` |
| [SAP Frame Section Hybrid ISection](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetHybridISectionComponent.cs) | `PropFrame.SetHybridISection` |
| [SAP Frame Section Hybrid USection](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetHybridUSectionComponent.cs) | `PropFrame.SetHybridUSection` |
| [SAP Frame Section Import Prop](../Rhino2SAP.Grasshopper/Generated/PropFrame_ImportPropComponent.cs) | `PropFrame.ImportProp` |
| [SAP Frame Section ISection](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetISectionComponent.cs) | `PropFrame.SetISection` |
| [SAP Frame Section ISection 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetISection_1Component.cs) | `PropFrame.SetISection_1` |
| [SAP Frame Section Material](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetMaterialComponent.cs) | `PropFrame.SetMaterial` |
| [SAP Frame Section Modifiers](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetModifiersComponent.cs) | `PropFrame.SetModifiers` |
| [SAP Frame Section Non Prismatic](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetNonPrismaticComponent.cs) | `PropFrame.SetNonPrismatic` |
| [SAP Frame Section Notional Size](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetNotionalSizeComponent.cs) | `PropFrame.SetNotionalSize` |
| [SAP Frame Section Pipe](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetPipeComponent.cs) | `PropFrame.SetPipe` |
| [SAP Frame Section Precast Box](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetPrecastBoxComponent.cs) | `PropFrame.SetPrecastBox` |
| [SAP Frame Section Precast I](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetPrecastIComponent.cs) | `PropFrame.SetPrecastI` |
| [SAP Frame Section Precast I 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetPrecastI_1Component.cs) | `PropFrame.SetPrecastI_1` |
| [SAP Frame Section Precast Super T](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetPrecastSuperTComponent.cs) | `PropFrame.SetPrecastSuperT` |
| [SAP Frame Section Precast U](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetPrecastUComponent.cs) | `PropFrame.SetPrecastU` |
| [SAP Frame Section Rebar Beam](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetRebarBeamComponent.cs) | `PropFrame.SetRebarBeam` |
| [SAP Frame Section Rebar Column](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetRebarColumnComponent.cs) | `PropFrame.SetRebarColumn` |
| [SAP Frame Section Rectangle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetRectangleComponent.cs) | `PropFrame.SetRectangle` |
| [SAP Frame Section Tee](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetTeeComponent.cs) | `PropFrame.SetTee` |
| [SAP Frame Section Tee 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetTee_1Component.cs) | `PropFrame.SetTee_1` |
| [SAP Frame Section Trapezoidal](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetTrapezoidalComponent.cs) | `PropFrame.SetTrapezoidal` |
| [SAP Frame Section Tube](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetTubeComponent.cs) | `PropFrame.SetTube` |
| [SAP Frame Section Tube 1](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetTube_1Component.cs) | `PropFrame.SetTube_1` |

## 04 Section Designer

| Componente | Metodo API |
|---|---|
| [SAP Frame Section SDSection](../Rhino2SAP.Grasshopper/Generated/PropFrame_SetSDSectionComponent.cs) | `PropFrame.SetSDSection` |
| [SAP Prop Frame SDShape Angle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetAngleComponent.cs) | `PropFrame.SDShape.SetAngle` |
| [SAP Prop Frame SDShape Channel](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetChannelComponent.cs) | `PropFrame.SDShape.SetChannel` |
| [SAP Prop Frame SDShape Dbl Angle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetDblAngleComponent.cs) | `PropFrame.SDShape.SetDblAngle` |
| [SAP Prop Frame SDShape ISection](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetISectionComponent.cs) | `PropFrame.SDShape.SetISection` |
| [SAP Prop Frame SDShape Pipe](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetPipeComponent.cs) | `PropFrame.SDShape.SetPipe` |
| [SAP Prop Frame SDShape Plate](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetPlateComponent.cs) | `PropFrame.SDShape.SetPlate` |
| [SAP Prop Frame SDShape Polygon](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetPolygonComponent.cs) | `PropFrame.SDShape.SetPolygon` |
| [SAP Prop Frame SDShape Ref Circle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetRefCircleComponent.cs) | `PropFrame.SDShape.SetRefCircle` |
| [SAP Prop Frame SDShape Ref Line](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetRefLineComponent.cs) | `PropFrame.SDShape.SetRefLine` |
| [SAP Prop Frame SDShape Reinf Circle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetReinfCircleComponent.cs) | `PropFrame.SDShape.SetReinfCircle` |
| [SAP Prop Frame SDShape Reinf Corner](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetReinfCornerComponent.cs) | `PropFrame.SDShape.SetReinfCorner` |
| [SAP Prop Frame SDShape Reinf Edge](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetReinfEdgeComponent.cs) | `PropFrame.SDShape.SetReinfEdge` |
| [SAP Prop Frame SDShape Reinf Line](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetReinfLineComponent.cs) | `PropFrame.SDShape.SetReinfLine` |
| [SAP Prop Frame SDShape Reinf Rectangular](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetReinfRectangularComponent.cs) | `PropFrame.SDShape.SetReinfRectangular` |
| [SAP Prop Frame SDShape Reinf Single](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetReinfSingleComponent.cs) | `PropFrame.SDShape.SetReinfSingle` |
| [SAP Prop Frame SDShape Shape Concrete Model](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetShapeConcreteModelComponent.cs) | `PropFrame.SDShape.SetShapeConcreteModel` |
| [SAP Prop Frame SDShape Shape Reinforcement](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetShapeReinforcementComponent.cs) | `PropFrame.SDShape.SetShapeReinforcement` |
| [SAP Prop Frame SDShape Solid Circle](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetSolidCircleComponent.cs) | `PropFrame.SDShape.SetSolidCircle` |
| [SAP Prop Frame SDShape Solid Rect](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetSolidRectComponent.cs) | `PropFrame.SDShape.SetSolidRect` |
| [SAP Prop Frame SDShape Solid Sector](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetSolidSectorComponent.cs) | `PropFrame.SDShape.SetSolidSector` |
| [SAP Prop Frame SDShape Solid Segment](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetSolidSegmentComponent.cs) | `PropFrame.SDShape.SetSolidSegment` |
| [SAP Prop Frame SDShape Tee](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetTeeComponent.cs) | `PropFrame.SDShape.SetTee` |
| [SAP Prop Frame SDShape Tube](../Rhino2SAP.Grasshopper/Generated/PropFrame_SDShape_SetTubeComponent.cs) | `PropFrame.SDShape.SetTube` |

## 05 Piastre e solidi

| Componente | Metodo API |
|---|---|
| [SAP Area Property Asolid](../Rhino2SAP.Grasshopper/Generated/PropArea_SetAsolidComponent.cs) | `PropArea.SetAsolid` |
| [SAP Area Property Modifiers](../Rhino2SAP.Grasshopper/Generated/PropArea_SetModifiersComponent.cs) | `PropArea.SetModifiers` |
| [SAP Area Property Notional Size](../Rhino2SAP.Grasshopper/Generated/PropArea_SetNotionalSizeComponent.cs) | `PropArea.SetNotionalSize` |
| [SAP Area Property Plane](../Rhino2SAP.Grasshopper/Generated/PropArea_SetPlaneComponent.cs) | `PropArea.SetPlane` |
| [SAP Area Property Shell](../Rhino2SAP.Grasshopper/Generated/PropArea_SetShellComponent.cs) | `PropArea.SetShell` |
| [SAP Area Property Shell 1](../Rhino2SAP.Grasshopper/Generated/PropArea_SetShell_1Component.cs) | `PropArea.SetShell_1` |
| [SAP Area Property Shell Design](../Rhino2SAP.Grasshopper/Generated/PropArea_SetShellDesignComponent.cs) | `PropArea.SetShellDesign` |
| [SAP Area Property Shell Layer](../Rhino2SAP.Grasshopper/Generated/PropArea_SetShellLayerComponent.cs) | `PropArea.SetShellLayer` |
| [SAP Area Property Shell Layer 1](../Rhino2SAP.Grasshopper/Generated/PropArea_SetShellLayer_1Component.cs) | `PropArea.SetShellLayer_1` |
| [SAP Area Property Shell Layer 2](../Rhino2SAP.Grasshopper/Generated/PropArea_SetShellLayer_2Component.cs) | `PropArea.SetShellLayer_2` |
| [SAP Prop Solid Prop](../Rhino2SAP.Grasshopper/Generated/PropSolid_SetPropComponent.cs) | `PropSolid.SetProp` |

## 06 Link e cavi

| Componente | Metodo API |
|---|---|
| [SAP Link Property Damper](../Rhino2SAP.Grasshopper/Generated/PropLink_SetDamperComponent.cs) | `PropLink.SetDamper` |
| [SAP Link Property Damper Bilinear](../Rhino2SAP.Grasshopper/Generated/PropLink_SetDamperBilinearComponent.cs) | `PropLink.SetDamperBilinear` |
| [SAP Link Property Damper Friction Spring](../Rhino2SAP.Grasshopper/Generated/PropLink_SetDamperFrictionSpringComponent.cs) | `PropLink.SetDamperFrictionSpring` |
| [SAP Link Property Damper Linear Exponential](../Rhino2SAP.Grasshopper/Generated/PropLink_SetDamperLinearExponentialComponent.cs) | `PropLink.SetDamperLinearExponential` |
| [SAP Link Property Friction Isolator](../Rhino2SAP.Grasshopper/Generated/PropLink_SetFrictionIsolatorComponent.cs) | `PropLink.SetFrictionIsolator` |
| [SAP Link Property Gap](../Rhino2SAP.Grasshopper/Generated/PropLink_SetGapComponent.cs) | `PropLink.SetGap` |
| [SAP Link Property Hook](../Rhino2SAP.Grasshopper/Generated/PropLink_SetHookComponent.cs) | `PropLink.SetHook` |
| [SAP Link Property Linear](../Rhino2SAP.Grasshopper/Generated/PropLink_SetLinearComponent.cs) | `PropLink.SetLinear` |
| [SAP Link Property Multi Linear Elastic](../Rhino2SAP.Grasshopper/Generated/PropLink_SetMultiLinearElasticComponent.cs) | `PropLink.SetMultiLinearElastic` |
| [SAP Link Property Multi Linear Plastic](../Rhino2SAP.Grasshopper/Generated/PropLink_SetMultiLinearPlasticComponent.cs) | `PropLink.SetMultiLinearPlastic` |
| [SAP Link Property Multi Linear Points](../Rhino2SAP.Grasshopper/Generated/PropLink_SetMultiLinearPointsComponent.cs) | `PropLink.SetMultiLinearPoints` |
| [SAP Link Property PDelta](../Rhino2SAP.Grasshopper/Generated/PropLink_SetPDeltaComponent.cs) | `PropLink.SetPDelta` |
| [SAP Link Property Plastic Wen](../Rhino2SAP.Grasshopper/Generated/PropLink_SetPlasticWenComponent.cs) | `PropLink.SetPlasticWen` |
| [SAP Link Property Rubber Isolator](../Rhino2SAP.Grasshopper/Generated/PropLink_SetRubberIsolatorComponent.cs) | `PropLink.SetRubberIsolator` |
| [SAP Link Property Spring Data](../Rhino2SAP.Grasshopper/Generated/PropLink_SetSpringDataComponent.cs) | `PropLink.SetSpringData` |
| [SAP Link Property TCFriction Isolator](../Rhino2SAP.Grasshopper/Generated/PropLink_SetTCFrictionIsolatorComponent.cs) | `PropLink.SetTCFrictionIsolator` |
| [SAP Link Property Triple Pendulum Isolator](../Rhino2SAP.Grasshopper/Generated/PropLink_SetTriplePendulumIsolatorComponent.cs) | `PropLink.SetTriplePendulumIsolator` |
| [SAP Link Property Weight And Mass](../Rhino2SAP.Grasshopper/Generated/PropLink_SetWeightAndMassComponent.cs) | `PropLink.SetWeightAndMass` |
| [SAP Prop Cable Modifiers](../Rhino2SAP.Grasshopper/Generated/PropCable_SetModifiersComponent.cs) | `PropCable.SetModifiers` |
| [SAP Prop Cable Prop](../Rhino2SAP.Grasshopper/Generated/PropCable_SetPropComponent.cs) | `PropCable.SetProp` |
| [SAP Prop Tendon Prop](../Rhino2SAP.Grasshopper/Generated/PropTendon_SetPropComponent.cs) | `PropTendon.SetProp` |

## 07 Elementi

| Componente | Metodo API |
|---|---|
| [Mesh to SAP Areas](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [Polyline to SAP Frames](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP Area Element](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP Cable Element](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP Frame Element](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP Link Element](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP Node Element](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP Solid Element](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |

## 08 Vincoli e molle

| Componente | Metodo API |
|---|---|
| [SAP Area Spring](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetSpringComponent.cs) | `AreaObj.SetSpring` |
| [SAP Constraint Def Beam](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetBeamComponent.cs) | `ConstraintDef.SetBeam` |
| [SAP Constraint Def Body](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetBodyComponent.cs) | `ConstraintDef.SetBody` |
| [SAP Constraint Def Diaphragm](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetDiaphragmComponent.cs) | `ConstraintDef.SetDiaphragm` |
| [SAP Constraint Def Equal](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetEqualComponent.cs) | `ConstraintDef.SetEqual` |
| [SAP Constraint Def Line](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetLineComponent.cs) | `ConstraintDef.SetLine` |
| [SAP Constraint Def Local](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetLocalComponent.cs) | `ConstraintDef.SetLocal` |
| [SAP Constraint Def Plate](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetPlateComponent.cs) | `ConstraintDef.SetPlate` |
| [SAP Constraint Def Rod](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetRodComponent.cs) | `ConstraintDef.SetRod` |
| [SAP Constraint Def Weld](../Rhino2SAP.Grasshopper/Generated/ConstraintDef_SetWeldComponent.cs) | `ConstraintDef.SetWeld` |
| [SAP Frame Releases](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetReleasesComponent.cs) | `FrameObj.SetReleases` |
| [SAP Frame Spring](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetSpringComponent.cs) | `FrameObj.SetSpring` |
| [SAP Frame TCLimits](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetTCLimitsComponent.cs) | `FrameObj.SetTCLimits` |
| [SAP Node Constraint](../Rhino2SAP.Grasshopper/Generated/PointObj_SetConstraintComponent.cs) | `PointObj.SetConstraint` |
| [SAP Node Panel Zone](../Rhino2SAP.Grasshopper/Generated/PointObj_SetPanelZoneComponent.cs) | `PointObj.SetPanelZone` |
| [SAP Node Restraint](../Rhino2SAP.Grasshopper/Generated/PointObj_SetRestraintComponent.cs) | `PointObj.SetRestraint` |
| [SAP Node Spring](../Rhino2SAP.Grasshopper/Generated/PointObj_SetSpringComponent.cs) | `PointObj.SetSpring` |
| [SAP Node Spring Coupled](../Rhino2SAP.Grasshopper/Generated/PointObj_SetSpringCoupledComponent.cs) | `PointObj.SetSpringCoupled` |
| [SAP Solid Spring](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetSpringComponent.cs) | `SolidObj.SetSpring` |
| [SAP Tendon Obj TCLimits](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetTCLimitsComponent.cs) | `TendonObj.SetTCLimits` |

## 09 Assi e offset

| Componente | Metodo API |
|---|---|
| [Align SAP Area Axis To Plane](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [Align SAP Area Local Axis](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [Align SAP Frame Axis To Point](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [Align SAP Frame Local Axis](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [Align SAP Model Local Axes](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [SAP Area Local Axes](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLocalAxesComponent.cs) | `AreaObj.SetLocalAxes` |
| [SAP Area Local Axes Advanced](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLocalAxesAdvancedComponent.cs) | `AreaObj.SetLocalAxesAdvanced` |
| [SAP Area Offsets](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetOffsetsComponent.cs) | `AreaObj.SetOffsets` |
| [SAP Cable Obj Insertion Point](../Rhino2SAP.Grasshopper/Generated/CableObj_SetInsertionPointComponent.cs) | `CableObj.SetInsertionPoint` |
| [SAP Coord Sys Coord Sys](../Rhino2SAP.Grasshopper/Generated/CoordSys_SetCoordSysComponent.cs) | `CoordSys.SetCoordSys` |
| [SAP Frame End Length Offset](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetEndLengthOffsetComponent.cs) | `FrameObj.SetEndLengthOffset` |
| [SAP Frame End Skew](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetEndSkewComponent.cs) | `FrameObj.SetEndSkew` |
| [SAP Frame Insertion Point](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetInsertionPointComponent.cs) | `FrameObj.SetInsertionPoint` |
| [SAP Frame Insertion Point 1](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetInsertionPoint_1Component.cs) | `FrameObj.SetInsertionPoint_1` |
| [SAP Frame Local Axes](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLocalAxesComponent.cs) | `FrameObj.SetLocalAxes` |
| [SAP Frame Local Axes Advanced](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLocalAxesAdvancedComponent.cs) | `FrameObj.SetLocalAxesAdvanced` |
| [SAP Link Local Axes](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetLocalAxesComponent.cs) | `LinkObj.SetLocalAxes` |
| [SAP Link Local Axes Advanced](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetLocalAxesAdvancedComponent.cs) | `LinkObj.SetLocalAxesAdvanced` |
| [SAP Node Local Axes](../Rhino2SAP.Grasshopper/Generated/PointObj_SetLocalAxesComponent.cs) | `PointObj.SetLocalAxes` |
| [SAP Node Local Axes Advanced](../Rhino2SAP.Grasshopper/Generated/PointObj_SetLocalAxesAdvancedComponent.cs) | `PointObj.SetLocalAxesAdvanced` |
| [SAP Solid Local Axes](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLocalAxesComponent.cs) | `SolidObj.SetLocalAxes` |
| [SAP Solid Local Axes Advanced](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLocalAxesAdvancedComponent.cs) | `SolidObj.SetLocalAxesAdvanced` |
| [SAP Tendon Obj Local Axes](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLocalAxesComponent.cs) | `TendonObj.SetLocalAxes` |

## 10 Mesh e assegnazioni

| Componente | Metodo API |
|---|---|
| [SAP Area Auto Mesh](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetAutoMeshComponent.cs) | `AreaObj.SetAutoMesh` |
| [SAP Area Edge Constraint](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetEdgeConstraintComponent.cs) | `AreaObj.SetEdgeConstraint` |
| [SAP Area Group Assign](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetGroupAssignComponent.cs) | `AreaObj.SetGroupAssign` |
| [SAP Area Mat Temp](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetMatTempComponent.cs) | `AreaObj.SetMatTemp` |
| [SAP Area Material Overwrite](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetMaterialOverwriteComponent.cs) | `AreaObj.SetMaterialOverwrite` |
| [SAP Area Modifiers](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetModifiersComponent.cs) | `AreaObj.SetModifiers` |
| [SAP Area Opening](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetOpeningComponent.cs) | `AreaObj.SetOpening` |
| [SAP Area Property](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetPropertyComponent.cs) | `AreaObj.SetProperty` |
| [SAP Area Thickness](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetThicknessComponent.cs) | `AreaObj.SetThickness` |
| [SAP Cable Obj Cable Data](../Rhino2SAP.Grasshopper/Generated/CableObj_SetCableDataComponent.cs) | `CableObj.SetCableData` |
| [SAP Cable Obj Group Assign](../Rhino2SAP.Grasshopper/Generated/CableObj_SetGroupAssignComponent.cs) | `CableObj.SetGroupAssign` |
| [SAP Cable Obj Mat Temp](../Rhino2SAP.Grasshopper/Generated/CableObj_SetMatTempComponent.cs) | `CableObj.SetMatTemp` |
| [SAP Cable Obj Material Overwrite](../Rhino2SAP.Grasshopper/Generated/CableObj_SetMaterialOverwriteComponent.cs) | `CableObj.SetMaterialOverwrite` |
| [SAP Cable Obj Modifiers](../Rhino2SAP.Grasshopper/Generated/CableObj_SetModifiersComponent.cs) | `CableObj.SetModifiers` |
| [SAP Cable Obj Output Stations](../Rhino2SAP.Grasshopper/Generated/CableObj_SetOutputStationsComponent.cs) | `CableObj.SetOutputStations` |
| [SAP Cable Obj Property](../Rhino2SAP.Grasshopper/Generated/CableObj_SetPropertyComponent.cs) | `CableObj.SetProperty` |
| [SAP Frame Auto Mesh](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetAutoMeshComponent.cs) | `FrameObj.SetAutoMesh` |
| [SAP Frame Curved](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetCurvedComponent.cs) | `FrameObj.SetCurved` |
| [SAP Frame Design Procedure](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetDesignProcedureComponent.cs) | `FrameObj.SetDesignProcedure` |
| [SAP Frame Fireproofing](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetFireproofingComponent.cs) | `FrameObj.SetFireproofing` |
| [SAP Frame Fireproofing 1](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetFireproofing_1Component.cs) | `FrameObj.SetFireproofing_1` |
| [SAP Frame Group Assign](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetGroupAssignComponent.cs) | `FrameObj.SetGroupAssign` |
| [SAP Frame Lateral Bracing](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLateralBracingComponent.cs) | `FrameObj.SetLateralBracing` |
| [SAP Frame Mat Temp](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetMatTempComponent.cs) | `FrameObj.SetMatTemp` |
| [SAP Frame Material Overwrite](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetMaterialOverwriteComponent.cs) | `FrameObj.SetMaterialOverwrite` |
| [SAP Frame Modifiers](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetModifiersComponent.cs) | `FrameObj.SetModifiers` |
| [SAP Frame Output Stations](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetOutputStationsComponent.cs) | `FrameObj.SetOutputStations` |
| [SAP Frame PDelta Force](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetPDeltaForceComponent.cs) | `FrameObj.SetPDeltaForce` |
| [SAP Frame Section](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetSectionComponent.cs) | `FrameObj.SetSection` |
| [SAP Frame Straight](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetStraightComponent.cs) | `FrameObj.SetStraight` |
| [SAP Group Def Group](../Rhino2SAP.Grasshopper/Generated/GroupDef_SetGroupComponent.cs) | `GroupDef.SetGroup` |
| [SAP Link Group Assign](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetGroupAssignComponent.cs) | `LinkObj.SetGroupAssign` |
| [SAP Link Property](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetPropertyComponent.cs) | `LinkObj.SetProperty` |
| [SAP Link Property FD](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetPropertyFDComponent.cs) | `LinkObj.SetPropertyFD` |
| [SAP Node Group Assign](../Rhino2SAP.Grasshopper/Generated/PointObj_SetGroupAssignComponent.cs) | `PointObj.SetGroupAssign` |
| [SAP Node Merge Number](../Rhino2SAP.Grasshopper/Generated/PointObj_SetMergeNumberComponent.cs) | `PointObj.SetMergeNumber` |
| [SAP Node Special Point](../Rhino2SAP.Grasshopper/Generated/PointObj_SetSpecialPointComponent.cs) | `PointObj.SetSpecialPoint` |
| [SAP Solid Auto Mesh](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetAutoMeshComponent.cs) | `SolidObj.SetAutoMesh` |
| [SAP Solid Edge Constraint](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetEdgeConstraintComponent.cs) | `SolidObj.SetEdgeConstraint` |
| [SAP Solid Group Assign](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetGroupAssignComponent.cs) | `SolidObj.SetGroupAssign` |
| [SAP Solid Mat Temp](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetMatTempComponent.cs) | `SolidObj.SetMatTemp` |
| [SAP Solid Property](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetPropertyComponent.cs) | `SolidObj.SetProperty` |
| [SAP Tendon Obj Discretization](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetDiscretizationComponent.cs) | `TendonObj.SetDiscretization` |
| [SAP Tendon Obj Group Assign](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetGroupAssignComponent.cs) | `TendonObj.SetGroupAssign` |
| [SAP Tendon Obj Loaded Group](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLoadedGroupComponent.cs) | `TendonObj.SetLoadedGroup` |
| [SAP Tendon Obj Mat Temp](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetMatTempComponent.cs) | `TendonObj.SetMatTemp` |
| [SAP Tendon Obj Property](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetPropertyComponent.cs) | `TendonObj.SetProperty` |
| [SAP Tendon Obj Tendon Data](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetTendonDataComponent.cs) | `TendonObj.SetTendonData` |

## 11 Masse

| Componente | Metodo API |
|---|---|
| [SAP Area Mass](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetMassComponent.cs) | `AreaObj.SetMass` |
| [SAP Cable Obj Mass](../Rhino2SAP.Grasshopper/Generated/CableObj_SetMassComponent.cs) | `CableObj.SetMass` |
| [SAP Frame Mass](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetMassComponent.cs) | `FrameObj.SetMass` |
| [SAP Material Mass Source](../Rhino2SAP.Grasshopper/Generated/PropMaterial_SetMassSourceComponent.cs) | `PropMaterial.SetMassSource` |
| [SAP Node Mass](../Rhino2SAP.Grasshopper/Generated/PointObj_SetMassComponent.cs) | `PointObj.SetMass` |
| [SAP Node Mass By Volume](../Rhino2SAP.Grasshopper/Generated/PointObj_SetMassByVolumeComponent.cs) | `PointObj.SetMassByVolume` |
| [SAP Node Mass By Weight](../Rhino2SAP.Grasshopper/Generated/PointObj_SetMassByWeightComponent.cs) | `PointObj.SetMassByWeight` |
| [SAP Source Mass Default](../Rhino2SAP.Grasshopper/Generated/SourceMass_SetDefaultComponent.cs) | `SourceMass.SetDefault` |
| [SAP Source Mass Mass Source](../Rhino2SAP.Grasshopper/Generated/SourceMass_SetMassSourceComponent.cs) | `SourceMass.SetMassSource` |

## 12 Load pattern

| Componente | Metodo API |
|---|---|
| [SAP Load Pattern](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |
| [SAP Load Patterns Add](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AddComponent.cs) | `LoadPatterns.Add` |
| [SAP Load Patterns Load Type](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_SetLoadTypeComponent.cs) | `LoadPatterns.SetLoadType` |
| [SAP Load Patterns Self WTMultiplier](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_SetSelfWTMultiplierComponent.cs) | `LoadPatterns.SetSelfWTMultiplier` |
| [SAP Node Pattern By Pressure](../Rhino2SAP.Grasshopper/Generated/PointObj_SetPatternByPressureComponent.cs) | `PointObj.SetPatternByPressure` |
| [SAP Node Pattern By XYZ](../Rhino2SAP.Grasshopper/Generated/PointObj_SetPatternByXYZComponent.cs) | `PointObj.SetPatternByXYZ` |

## 13 Carichi nodali

| Componente | Metodo API |
|---|---|
| [SAP Node Load Displ](../Rhino2SAP.Grasshopper/Generated/PointObj_SetLoadDisplComponent.cs) | `PointObj.SetLoadDispl` |
| [SAP Node Load Force](../Rhino2SAP.Grasshopper/Generated/PointObj_SetLoadForceComponent.cs) | `PointObj.SetLoadForce` |

## 14 Carichi beam

| Componente | Metodo API |
|---|---|
| [SAP Frame Load Deformation](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadDeformationComponent.cs) | `FrameObj.SetLoadDeformation` |
| [SAP Frame Load Distributed](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadDistributedComponent.cs) | `FrameObj.SetLoadDistributed` |
| [SAP Frame Load Gravity](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadGravityComponent.cs) | `FrameObj.SetLoadGravity` |
| [SAP Frame Load Point](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadPointComponent.cs) | `FrameObj.SetLoadPoint` |
| [SAP Frame Load Strain](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadStrainComponent.cs) | `FrameObj.SetLoadStrain` |
| [SAP Frame Load Target Force](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadTargetForceComponent.cs) | `FrameObj.SetLoadTargetForce` |
| [SAP Frame Load Temperature](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadTemperatureComponent.cs) | `FrameObj.SetLoadTemperature` |
| [SAP Frame Load Transfer](../Rhino2SAP.Grasshopper/Generated/FrameObj_SetLoadTransferComponent.cs) | `FrameObj.SetLoadTransfer` |

## 15 Carichi plate

| Componente | Metodo API |
|---|---|
| [SAP Area Load Gravity](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadGravityComponent.cs) | `AreaObj.SetLoadGravity` |
| [SAP Area Load Pore Pressure](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadPorePressureComponent.cs) | `AreaObj.SetLoadPorePressure` |
| [SAP Area Load Rotate](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadRotateComponent.cs) | `AreaObj.SetLoadRotate` |
| [SAP Area Load Strain](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadStrainComponent.cs) | `AreaObj.SetLoadStrain` |
| [SAP Area Load Surface Pressure](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadSurfacePressureComponent.cs) | `AreaObj.SetLoadSurfacePressure` |
| [SAP Area Load Temperature](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadTemperatureComponent.cs) | `AreaObj.SetLoadTemperature` |
| [SAP Area Load Uniform](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadUniformComponent.cs) | `AreaObj.SetLoadUniform` |
| [SAP Area Load Uniform To Frame](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadUniformToFrameComponent.cs) | `AreaObj.SetLoadUniformToFrame` |
| [SAP Area Load Wind Pressure](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadWindPressureComponent.cs) | `AreaObj.SetLoadWindPressure` |
| [SAP Area Load Wind Pressure 1](../Rhino2SAP.Grasshopper/Generated/AreaObj_SetLoadWindPressure_1Component.cs) | `AreaObj.SetLoadWindPressure_1` |

## 16 Altri carichi

| Componente | Metodo API |
|---|---|
| [SAP Cable Obj Load Deformation](../Rhino2SAP.Grasshopper/Generated/CableObj_SetLoadDeformationComponent.cs) | `CableObj.SetLoadDeformation` |
| [SAP Cable Obj Load Distributed](../Rhino2SAP.Grasshopper/Generated/CableObj_SetLoadDistributedComponent.cs) | `CableObj.SetLoadDistributed` |
| [SAP Cable Obj Load Gravity](../Rhino2SAP.Grasshopper/Generated/CableObj_SetLoadGravityComponent.cs) | `CableObj.SetLoadGravity` |
| [SAP Cable Obj Load Strain](../Rhino2SAP.Grasshopper/Generated/CableObj_SetLoadStrainComponent.cs) | `CableObj.SetLoadStrain` |
| [SAP Cable Obj Load Target Force](../Rhino2SAP.Grasshopper/Generated/CableObj_SetLoadTargetForceComponent.cs) | `CableObj.SetLoadTargetForce` |
| [SAP Cable Obj Load Temperature](../Rhino2SAP.Grasshopper/Generated/CableObj_SetLoadTemperatureComponent.cs) | `CableObj.SetLoadTemperature` |
| [SAP Link Load Deformation](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetLoadDeformationComponent.cs) | `LinkObj.SetLoadDeformation` |
| [SAP Link Load Gravity](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetLoadGravityComponent.cs) | `LinkObj.SetLoadGravity` |
| [SAP Link Load Target Force](../Rhino2SAP.Grasshopper/Generated/LinkObj_SetLoadTargetForceComponent.cs) | `LinkObj.SetLoadTargetForce` |
| [SAP Load Patterns Auto Seastate Auto](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeastate_SetAutoComponent.cs) | `LoadPatterns.AutoSeastate.SetAuto` |
| [SAP Load Patterns Auto Seastate None](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeastate_SetNoneComponent.cs) | `LoadPatterns.AutoSeastate.SetNone` |
| [SAP Solid Load Gravity](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLoadGravityComponent.cs) | `SolidObj.SetLoadGravity` |
| [SAP Solid Load Pore Pressure](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLoadPorePressureComponent.cs) | `SolidObj.SetLoadPorePressure` |
| [SAP Solid Load Strain](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLoadStrainComponent.cs) | `SolidObj.SetLoadStrain` |
| [SAP Solid Load Surface Pressure](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLoadSurfacePressureComponent.cs) | `SolidObj.SetLoadSurfacePressure` |
| [SAP Solid Load Temperature](../Rhino2SAP.Grasshopper/Generated/SolidObj_SetLoadTemperatureComponent.cs) | `SolidObj.SetLoadTemperature` |
| [SAP Tendon Obj Load Deformation](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLoadDeformationComponent.cs) | `TendonObj.SetLoadDeformation` |
| [SAP Tendon Obj Load Force Stress](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLoadForceStressComponent.cs) | `TendonObj.SetLoadForceStress` |
| [SAP Tendon Obj Load Gravity](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLoadGravityComponent.cs) | `TendonObj.SetLoadGravity` |
| [SAP Tendon Obj Load Strain](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLoadStrainComponent.cs) | `TendonObj.SetLoadStrain` |
| [SAP Tendon Obj Load Temperature](../Rhino2SAP.Grasshopper/Generated/TendonObj_SetLoadTemperatureComponent.cs) | `TendonObj.SetLoadTemperature` |

## 17 Vento

| Componente | Metodo API |
|---|---|
| [SAP Load Patterns Auto Wind API4 F2008](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetAPI4F2008Component.cs) | `LoadPatterns.AutoWind.SetAPI4F2008` |
| [SAP Load Patterns Auto Wind API4 F2008 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetAPI4F2008_1Component.cs) | `LoadPatterns.AutoWind.SetAPI4F2008_1` |
| [SAP Load Patterns Auto Wind API4 F2013](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetAPI4F2013Component.cs) | `LoadPatterns.AutoWind.SetAPI4F2013` |
| [SAP Load Patterns Auto Wind ASCE702](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASCE702Component.cs) | `LoadPatterns.AutoWind.SetASCE702` |
| [SAP Load Patterns Auto Wind ASCE705](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASCE705Component.cs) | `LoadPatterns.AutoWind.SetASCE705` |
| [SAP Load Patterns Auto Wind ASCE710](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASCE710Component.cs) | `LoadPatterns.AutoWind.SetASCE710` |
| [SAP Load Patterns Auto Wind ASCE716](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASCE716Component.cs) | `LoadPatterns.AutoWind.SetASCE716` |
| [SAP Load Patterns Auto Wind ASCE788](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASCE788Component.cs) | `LoadPatterns.AutoWind.SetASCE788` |
| [SAP Load Patterns Auto Wind ASCE795](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASCE795Component.cs) | `LoadPatterns.AutoWind.SetASCE795` |
| [SAP Load Patterns Auto Wind ASNZS117022002](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASNZS117022002Component.cs) | `LoadPatterns.AutoWind.SetASNZS117022002` |
| [SAP Load Patterns Auto Wind ASNZS117022011](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetASNZS117022011Component.cs) | `LoadPatterns.AutoWind.SetASNZS117022011` |
| [SAP Load Patterns Auto Wind BOCA96](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetBOCA96Component.cs) | `LoadPatterns.AutoWind.SetBOCA96` |
| [SAP Load Patterns Auto Wind BS639995](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetBS639995Component.cs) | `LoadPatterns.AutoWind.SetBS639995` |
| [SAP Load Patterns Auto Wind Chinese2002](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetChinese2002Component.cs) | `LoadPatterns.AutoWind.SetChinese2002` |
| [SAP Load Patterns Auto Wind Chinese2002 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetChinese2002_1Component.cs) | `LoadPatterns.AutoWind.SetChinese2002_1` |
| [SAP Load Patterns Auto Wind Chinese2010](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetChinese2010Component.cs) | `LoadPatterns.AutoWind.SetChinese2010` |
| [SAP Load Patterns Auto Wind Eurocode12005](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetEurocode12005Component.cs) | `LoadPatterns.AutoWind.SetEurocode12005` |
| [SAP Load Patterns Auto Wind Eurocode12005 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetEurocode12005_1Component.cs) | `LoadPatterns.AutoWind.SetEurocode12005_1` |
| [SAP Load Patterns Auto Wind Exposure](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetExposureComponent.cs) | `LoadPatterns.AutoWind.SetExposure` |
| [SAP Load Patterns Auto Wind Exposure 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetExposure_1Component.cs) | `LoadPatterns.AutoWind.SetExposure_1` |
| [SAP Load Patterns Auto Wind IS8751987](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetIS8751987Component.cs) | `LoadPatterns.AutoWind.SetIS8751987` |
| [SAP Load Patterns Auto Wind IS8752015](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetIS8752015Component.cs) | `LoadPatterns.AutoWind.SetIS8752015` |
| [SAP Load Patterns Auto Wind Mexican](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetMexicanComponent.cs) | `LoadPatterns.AutoWind.SetMexican` |
| [SAP Load Patterns Auto Wind NBCC2005](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNBCC2005Component.cs) | `LoadPatterns.AutoWind.SetNBCC2005` |
| [SAP Load Patterns Auto Wind NBCC2010](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNBCC2010Component.cs) | `LoadPatterns.AutoWind.SetNBCC2010` |
| [SAP Load Patterns Auto Wind NBCC2010 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNBCC2010_1Component.cs) | `LoadPatterns.AutoWind.SetNBCC2010_1` |
| [SAP Load Patterns Auto Wind NBCC2015](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNBCC2015Component.cs) | `LoadPatterns.AutoWind.SetNBCC2015` |
| [SAP Load Patterns Auto Wind NBCC95](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNBCC95Component.cs) | `LoadPatterns.AutoWind.SetNBCC95` |
| [SAP Load Patterns Auto Wind None](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNoneComponent.cs) | `LoadPatterns.AutoWind.SetNone` |
| [SAP Load Patterns Auto Wind NTC2008](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNTC2008Component.cs) | `LoadPatterns.AutoWind.SetNTC2008` |
| [SAP Load Patterns Auto Wind NTC2008 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNTC2008_1Component.cs) | `LoadPatterns.AutoWind.SetNTC2008_1` |
| [SAP Load Patterns Auto Wind NTC2018](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNTC2018Component.cs) | `LoadPatterns.AutoWind.SetNTC2018` |
| [SAP Load Patterns Auto Wind NTC2018 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetNTC2018_1Component.cs) | `LoadPatterns.AutoWind.SetNTC2018_1` |
| [SAP Load Patterns Auto Wind SP20133302016](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetSP20133302016Component.cs) | `LoadPatterns.AutoWind.SetSP20133302016` |
| [SAP Load Patterns Auto Wind UBC94](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetUBC94Component.cs) | `LoadPatterns.AutoWind.SetUBC94` |
| [SAP Load Patterns Auto Wind UBC97](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetUBC97Component.cs) | `LoadPatterns.AutoWind.SetUBC97` |
| [SAP Load Patterns Auto Wind User Load](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoWind_SetUserLoadComponent.cs) | `LoadPatterns.AutoWind.SetUserLoad` |

## 18 Sisma

| Componente | Metodo API |
|---|---|
| [SAP Load Patterns Auto Seismic AS11702007](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetAS11702007Component.cs) | `LoadPatterns.AutoSeismic.SetAS11702007` |
| [SAP Load Patterns Auto Seismic ASCE716](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetASCE716Component.cs) | `LoadPatterns.AutoSeismic.SetASCE716` |
| [SAP Load Patterns Auto Seismic ASCE716 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetASCE716_1Component.cs) | `LoadPatterns.AutoSeismic.SetASCE716_1` |
| [SAP Load Patterns Auto Seismic BOCA96](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetBOCA96Component.cs) | `LoadPatterns.AutoSeismic.SetBOCA96` |
| [SAP Load Patterns Auto Seismic Chinese2002](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetChinese2002Component.cs) | `LoadPatterns.AutoSeismic.SetChinese2002` |
| [SAP Load Patterns Auto Seismic Chinese2010](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetChinese2010Component.cs) | `LoadPatterns.AutoSeismic.SetChinese2010` |
| [SAP Load Patterns Auto Seismic Diaphragm Eccentricity Override](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetDiaphragmEccentricityOverrideComponent.cs) | `LoadPatterns.AutoSeismic.SetDiaphragmEccentricityOverride` |
| [SAP Load Patterns Auto Seismic Eurocode82004](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetEurocode82004Component.cs) | `LoadPatterns.AutoSeismic.SetEurocode82004` |
| [SAP Load Patterns Auto Seismic Eurocode82004 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetEurocode82004_1Component.cs) | `LoadPatterns.AutoSeismic.SetEurocode82004_1` |
| [SAP Load Patterns Auto Seismic IBC2003](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetIBC2003Component.cs) | `LoadPatterns.AutoSeismic.SetIBC2003` |
| [SAP Load Patterns Auto Seismic IBC2006](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetIBC2006Component.cs) | `LoadPatterns.AutoSeismic.SetIBC2006` |
| [SAP Load Patterns Auto Seismic IBC2009](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetIBC2009Component.cs) | `LoadPatterns.AutoSeismic.SetIBC2009` |
| [SAP Load Patterns Auto Seismic IBC2012](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetIBC2012Component.cs) | `LoadPatterns.AutoSeismic.SetIBC2012` |
| [SAP Load Patterns Auto Seismic IS1893 2002](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetIS1893_2002Component.cs) | `LoadPatterns.AutoSeismic.SetIS1893_2002` |
| [SAP Load Patterns Auto Seismic NBCC2005](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNBCC2005Component.cs) | `LoadPatterns.AutoSeismic.SetNBCC2005` |
| [SAP Load Patterns Auto Seismic NBCC2010](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNBCC2010Component.cs) | `LoadPatterns.AutoSeismic.SetNBCC2010` |
| [SAP Load Patterns Auto Seismic NBCC2015](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNBCC2015Component.cs) | `LoadPatterns.AutoSeismic.SetNBCC2015` |
| [SAP Load Patterns Auto Seismic NBCC95](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNBCC95Component.cs) | `LoadPatterns.AutoSeismic.SetNBCC95` |
| [SAP Load Patterns Auto Seismic NEHRP97](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNEHRP97Component.cs) | `LoadPatterns.AutoSeismic.SetNEHRP97` |
| [SAP Load Patterns Auto Seismic None](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNoneComponent.cs) | `LoadPatterns.AutoSeismic.SetNone` |
| [SAP Load Patterns Auto Seismic NTC2008](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNTC2008Component.cs) | `LoadPatterns.AutoSeismic.SetNTC2008` |
| [SAP Load Patterns Auto Seismic NTC2018](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNTC2018Component.cs) | `LoadPatterns.AutoSeismic.SetNTC2018` |
| [SAP Load Patterns Auto Seismic NZS11702004](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNZS11702004Component.cs) | `LoadPatterns.AutoSeismic.SetNZS11702004` |
| [SAP Load Patterns Auto Seismic NZS11702004 1](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNZS11702004_1Component.cs) | `LoadPatterns.AutoSeismic.SetNZS11702004_1` |
| [SAP Load Patterns Auto Seismic NZS11702004 2](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetNZS11702004_2Component.cs) | `LoadPatterns.AutoSeismic.SetNZS11702004_2` |
| [SAP Load Patterns Auto Seismic UBC94](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetUBC94Component.cs) | `LoadPatterns.AutoSeismic.SetUBC94` |
| [SAP Load Patterns Auto Seismic UBC97](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetUBC97Component.cs) | `LoadPatterns.AutoSeismic.SetUBC97` |
| [SAP Load Patterns Auto Seismic UBC97 Iso](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetUBC97IsoComponent.cs) | `LoadPatterns.AutoSeismic.SetUBC97Iso` |
| [SAP Load Patterns Auto Seismic User Coefficient](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetUserCoefficientComponent.cs) | `LoadPatterns.AutoSeismic.SetUserCoefficient` |
| [SAP Load Patterns Auto Seismic User Load](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetUserLoadComponent.cs) | `LoadPatterns.AutoSeismic.SetUserLoad` |
| [SAP Load Patterns Auto Seismic User Load Value](../Rhino2SAP.Grasshopper/Generated/LoadPatterns_AutoSeismic_SetUserLoadValueComponent.cs) | `LoadPatterns.AutoSeismic.SetUserLoadValue` |

## 19 Casi statici

| Componente | Metodo API |
|---|---|
| [SAP Buckling Case](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |
| [SAP Linear Static Case](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |
| [SAP Load Cases Buckling Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_Buckling_SetCaseComponent.cs) | `LoadCases.Buckling.SetCase` |
| [SAP Load Cases Buckling Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_Buckling_SetInitialCaseComponent.cs) | `LoadCases.Buckling.SetInitialCase` |
| [SAP Load Cases Buckling Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_Buckling_SetLoadsComponent.cs) | `LoadCases.Buckling.SetLoads` |
| [SAP Load Cases Buckling Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_Buckling_SetParametersComponent.cs) | `LoadCases.Buckling.SetParameters` |
| [SAP Load Cases Hyper Static Base Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_HyperStatic_SetBaseCaseComponent.cs) | `LoadCases.HyperStatic.SetBaseCase` |
| [SAP Load Cases Hyper Static Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_HyperStatic_SetCaseComponent.cs) | `LoadCases.HyperStatic.SetCase` |
| [SAP Load Cases Static Linear Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinear_SetCaseComponent.cs) | `LoadCases.StaticLinear.SetCase` |
| [SAP Load Cases Static Linear Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinear_SetInitialCaseComponent.cs) | `LoadCases.StaticLinear.SetInitialCase` |
| [SAP Load Cases Static Linear Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinear_SetLoadsComponent.cs) | `LoadCases.StaticLinear.SetLoads` |
| [SAP Load Cases Static Linear Multistep Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinearMultistep_SetCaseComponent.cs) | `LoadCases.StaticLinearMultistep.SetCase` |
| [SAP Load Cases Static Linear Multistep Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinearMultistep_SetInitialCaseComponent.cs) | `LoadCases.StaticLinearMultistep.SetInitialCase` |
| [SAP Load Cases Static Linear Multistep Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinearMultistep_SetLoadsComponent.cs) | `LoadCases.StaticLinearMultistep.SetLoads` |
| [SAP Load Cases Static Linear Multistep Loads 1](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticLinearMultistep_SetLoads_1Component.cs) | `LoadCases.StaticLinearMultistep.SetLoads_1` |

## 20 Casi non lineari

| Componente | Metodo API |
|---|---|
| [SAP Load Cases Static Nonlinear Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetCaseComponent.cs) | `LoadCases.StaticNonlinear.SetCase` |
| [SAP Load Cases Static Nonlinear Geometric Nonlinearity](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetGeometricNonlinearityComponent.cs) | `LoadCases.StaticNonlinear.SetGeometricNonlinearity` |
| [SAP Load Cases Static Nonlinear Hinge Unloading](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetHingeUnloadingComponent.cs) | `LoadCases.StaticNonlinear.SetHingeUnloading` |
| [SAP Load Cases Static Nonlinear Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetInitialCaseComponent.cs) | `LoadCases.StaticNonlinear.SetInitialCase` |
| [SAP Load Cases Static Nonlinear Load Application](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetLoadApplicationComponent.cs) | `LoadCases.StaticNonlinear.SetLoadApplication` |
| [SAP Load Cases Static Nonlinear Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetLoadsComponent.cs) | `LoadCases.StaticNonlinear.SetLoads` |
| [SAP Load Cases Static Nonlinear Mass Source](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetMassSourceComponent.cs) | `LoadCases.StaticNonlinear.SetMassSource` |
| [SAP Load Cases Static Nonlinear Modal Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetModalCaseComponent.cs) | `LoadCases.StaticNonlinear.SetModalCase` |
| [SAP Load Cases Static Nonlinear Multistep Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearMultistep_SetCaseComponent.cs) | `LoadCases.StaticNonlinearMultistep.SetCase` |
| [SAP Load Cases Static Nonlinear Multistep Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearMultistep_SetInitialCaseComponent.cs) | `LoadCases.StaticNonlinearMultistep.SetInitialCase` |
| [SAP Load Cases Static Nonlinear Multistep Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearMultistep_SetLoadsComponent.cs) | `LoadCases.StaticNonlinearMultistep.SetLoads` |
| [SAP Load Cases Static Nonlinear Results Saved](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetResultsSavedComponent.cs) | `LoadCases.StaticNonlinear.SetResultsSaved` |
| [SAP Load Cases Static Nonlinear Sol Control Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetSolControlParametersComponent.cs) | `LoadCases.StaticNonlinear.SetSolControlParameters` |
| [SAP Load Cases Static Nonlinear Target Force Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinear_SetTargetForceParametersComponent.cs) | `LoadCases.StaticNonlinear.SetTargetForceParameters` |
| [SAP Nonlinear Static Case](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |

## 21 Fasi costruttive

| Componente | Metodo API |
|---|---|
| [SAP Load Cases Static Nonlinear Staged Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetCaseComponent.cs) | `LoadCases.StaticNonlinearStaged.SetCase` |
| [SAP Load Cases Static Nonlinear Staged Geometric Nonlinearity](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetGeometricNonlinearityComponent.cs) | `LoadCases.StaticNonlinearStaged.SetGeometricNonlinearity` |
| [SAP Load Cases Static Nonlinear Staged Hinge Unloading](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetHingeUnloadingComponent.cs) | `LoadCases.StaticNonlinearStaged.SetHingeUnloading` |
| [SAP Load Cases Static Nonlinear Staged Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetInitialCaseComponent.cs) | `LoadCases.StaticNonlinearStaged.SetInitialCase` |
| [SAP Load Cases Static Nonlinear Staged Mass Source](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetMassSourceComponent.cs) | `LoadCases.StaticNonlinearStaged.SetMassSource` |
| [SAP Load Cases Static Nonlinear Staged Material Nonlinearity](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetMaterialNonlinearityComponent.cs) | `LoadCases.StaticNonlinearStaged.SetMaterialNonlinearity` |
| [SAP Load Cases Static Nonlinear Staged Results Saved](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetResultsSavedComponent.cs) | `LoadCases.StaticNonlinearStaged.SetResultsSaved` |
| [SAP Load Cases Static Nonlinear Staged Sol Control Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetSolControlParametersComponent.cs) | `LoadCases.StaticNonlinearStaged.SetSolControlParameters` |
| [SAP Load Cases Static Nonlinear Staged Stage Data](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetStageDataComponent.cs) | `LoadCases.StaticNonlinearStaged.SetStageData` |
| [SAP Load Cases Static Nonlinear Staged Stage Data 1](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetStageData_1Component.cs) | `LoadCases.StaticNonlinearStaged.SetStageData_1` |
| [SAP Load Cases Static Nonlinear Staged Stage Data 2](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetStageData_2Component.cs) | `LoadCases.StaticNonlinearStaged.SetStageData_2` |
| [SAP Load Cases Static Nonlinear Staged Stage Definitions](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetStageDefinitionsComponent.cs) | `LoadCases.StaticNonlinearStaged.SetStageDefinitions` |
| [SAP Load Cases Static Nonlinear Staged Stage Definitions 1](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetStageDefinitions_1Component.cs) | `LoadCases.StaticNonlinearStaged.SetStageDefinitions_1` |
| [SAP Load Cases Static Nonlinear Staged Stage Definitions 2](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetStageDefinitions_2Component.cs) | `LoadCases.StaticNonlinearStaged.SetStageDefinitions_2` |
| [SAP Load Cases Static Nonlinear Staged Target Force Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_StaticNonlinearStaged_SetTargetForceParametersComponent.cs) | `LoadCases.StaticNonlinearStaged.SetTargetForceParameters` |

## 22 Modale e spettri

| Componente | Metodo API |
|---|---|
| [SAP Load Cases Modal Eigen Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalEigen_SetCaseComponent.cs) | `LoadCases.ModalEigen.SetCase` |
| [SAP Load Cases Modal Eigen Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalEigen_SetInitialCaseComponent.cs) | `LoadCases.ModalEigen.SetInitialCase` |
| [SAP Load Cases Modal Eigen Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalEigen_SetLoadsComponent.cs) | `LoadCases.ModalEigen.SetLoads` |
| [SAP Load Cases Modal Eigen Number Modes](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalEigen_SetNumberModesComponent.cs) | `LoadCases.ModalEigen.SetNumberModes` |
| [SAP Load Cases Modal Eigen Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalEigen_SetParametersComponent.cs) | `LoadCases.ModalEigen.SetParameters` |
| [SAP Load Cases Modal Ritz Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalRitz_SetCaseComponent.cs) | `LoadCases.ModalRitz.SetCase` |
| [SAP Load Cases Modal Ritz Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalRitz_SetInitialCaseComponent.cs) | `LoadCases.ModalRitz.SetInitialCase` |
| [SAP Load Cases Modal Ritz Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalRitz_SetLoadsComponent.cs) | `LoadCases.ModalRitz.SetLoads` |
| [SAP Load Cases Modal Ritz Number Modes](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModalRitz_SetNumberModesComponent.cs) | `LoadCases.ModalRitz.SetNumberModes` |
| [SAP Load Cases Response Spectrum Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetCaseComponent.cs) | `LoadCases.ResponseSpectrum.SetCase` |
| [SAP Load Cases Response Spectrum Damp Constant](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetDampConstantComponent.cs) | `LoadCases.ResponseSpectrum.SetDampConstant` |
| [SAP Load Cases Response Spectrum Damp Interpolated](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetDampInterpolatedComponent.cs) | `LoadCases.ResponseSpectrum.SetDampInterpolated` |
| [SAP Load Cases Response Spectrum Damp Overrides](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetDampOverridesComponent.cs) | `LoadCases.ResponseSpectrum.SetDampOverrides` |
| [SAP Load Cases Response Spectrum Damp Proportional](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetDampProportionalComponent.cs) | `LoadCases.ResponseSpectrum.SetDampProportional` |
| [SAP Load Cases Response Spectrum Diaphragm Eccentricity Override](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetDiaphragmEccentricityOverrideComponent.cs) | `LoadCases.ResponseSpectrum.SetDiaphragmEccentricityOverride` |
| [SAP Load Cases Response Spectrum Dir Comb](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetDirCombComponent.cs) | `LoadCases.ResponseSpectrum.SetDirComb` |
| [SAP Load Cases Response Spectrum Eccentricity](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetEccentricityComponent.cs) | `LoadCases.ResponseSpectrum.SetEccentricity` |
| [SAP Load Cases Response Spectrum Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetLoadsComponent.cs) | `LoadCases.ResponseSpectrum.SetLoads` |
| [SAP Load Cases Response Spectrum Modal Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetModalCaseComponent.cs) | `LoadCases.ResponseSpectrum.SetModalCase` |
| [SAP Load Cases Response Spectrum Modal Comb](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetModalCombComponent.cs) | `LoadCases.ResponseSpectrum.SetModalComb` |
| [SAP Load Cases Response Spectrum Modal Comb 1](../Rhino2SAP.Grasshopper/Generated/LoadCases_ResponseSpectrum_SetModalComb_1Component.cs) | `LoadCases.ResponseSpectrum.SetModalComb_1` |
| [SAP Response Spectrum Function](../Rhino2SAP.Grasshopper/FunctionComponents.cs) | `Func.FuncRS.SetUser` |

## 23 Time history

| Componente | Metodo API |
|---|---|
| [SAP Load Cases Dir Hist Linear Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistLinear_SetCaseComponent.cs) | `LoadCases.DirHistLinear.SetCase` |
| [SAP Load Cases Dir Hist Linear Damp Proportional](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistLinear_SetDampProportionalComponent.cs) | `LoadCases.DirHistLinear.SetDampProportional` |
| [SAP Load Cases Dir Hist Linear Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistLinear_SetInitialCaseComponent.cs) | `LoadCases.DirHistLinear.SetInitialCase` |
| [SAP Load Cases Dir Hist Linear Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistLinear_SetLoadsComponent.cs) | `LoadCases.DirHistLinear.SetLoads` |
| [SAP Load Cases Dir Hist Linear Time Integration](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistLinear_SetTimeIntegrationComponent.cs) | `LoadCases.DirHistLinear.SetTimeIntegration` |
| [SAP Load Cases Dir Hist Linear Time Step](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistLinear_SetTimeStepComponent.cs) | `LoadCases.DirHistLinear.SetTimeStep` |
| [SAP Load Cases Dir Hist Nonlinear Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetCaseComponent.cs) | `LoadCases.DirHistNonlinear.SetCase` |
| [SAP Load Cases Dir Hist Nonlinear Damp Proportional](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetDampProportionalComponent.cs) | `LoadCases.DirHistNonlinear.SetDampProportional` |
| [SAP Load Cases Dir Hist Nonlinear Geometric Nonlinearity](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetGeometricNonlinearityComponent.cs) | `LoadCases.DirHistNonlinear.SetGeometricNonlinearity` |
| [SAP Load Cases Dir Hist Nonlinear Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetInitialCaseComponent.cs) | `LoadCases.DirHistNonlinear.SetInitialCase` |
| [SAP Load Cases Dir Hist Nonlinear Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetLoadsComponent.cs) | `LoadCases.DirHistNonlinear.SetLoads` |
| [SAP Load Cases Dir Hist Nonlinear Mass Source](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetMassSourceComponent.cs) | `LoadCases.DirHistNonlinear.SetMassSource` |
| [SAP Load Cases Dir Hist Nonlinear Sol Control Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetSolControlParametersComponent.cs) | `LoadCases.DirHistNonlinear.SetSolControlParameters` |
| [SAP Load Cases Dir Hist Nonlinear Time Integration](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetTimeIntegrationComponent.cs) | `LoadCases.DirHistNonlinear.SetTimeIntegration` |
| [SAP Load Cases Dir Hist Nonlinear Time Step](../Rhino2SAP.Grasshopper/Generated/LoadCases_DirHistNonlinear_SetTimeStepComponent.cs) | `LoadCases.DirHistNonlinear.SetTimeStep` |
| [SAP Load Cases Mod Hist Linear Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetCaseComponent.cs) | `LoadCases.ModHistLinear.SetCase` |
| [SAP Load Cases Mod Hist Linear Damp Constant](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetDampConstantComponent.cs) | `LoadCases.ModHistLinear.SetDampConstant` |
| [SAP Load Cases Mod Hist Linear Damp Interpolated](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetDampInterpolatedComponent.cs) | `LoadCases.ModHistLinear.SetDampInterpolated` |
| [SAP Load Cases Mod Hist Linear Damp Overrides](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetDampOverridesComponent.cs) | `LoadCases.ModHistLinear.SetDampOverrides` |
| [SAP Load Cases Mod Hist Linear Damp Proportional](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetDampProportionalComponent.cs) | `LoadCases.ModHistLinear.SetDampProportional` |
| [SAP Load Cases Mod Hist Linear Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetLoadsComponent.cs) | `LoadCases.ModHistLinear.SetLoads` |
| [SAP Load Cases Mod Hist Linear Modal Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetModalCaseComponent.cs) | `LoadCases.ModHistLinear.SetModalCase` |
| [SAP Load Cases Mod Hist Linear Motion Type](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetMotionTypeComponent.cs) | `LoadCases.ModHistLinear.SetMotionType` |
| [SAP Load Cases Mod Hist Linear Time Step](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistLinear_SetTimeStepComponent.cs) | `LoadCases.ModHistLinear.SetTimeStep` |
| [SAP Load Cases Mod Hist Nonlinear Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetCaseComponent.cs) | `LoadCases.ModHistNonlinear.SetCase` |
| [SAP Load Cases Mod Hist Nonlinear Damp Constant](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetDampConstantComponent.cs) | `LoadCases.ModHistNonlinear.SetDampConstant` |
| [SAP Load Cases Mod Hist Nonlinear Damp Interpolated](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetDampInterpolatedComponent.cs) | `LoadCases.ModHistNonlinear.SetDampInterpolated` |
| [SAP Load Cases Mod Hist Nonlinear Damp Overrides](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetDampOverridesComponent.cs) | `LoadCases.ModHistNonlinear.SetDampOverrides` |
| [SAP Load Cases Mod Hist Nonlinear Damp Proportional](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetDampProportionalComponent.cs) | `LoadCases.ModHistNonlinear.SetDampProportional` |
| [SAP Load Cases Mod Hist Nonlinear Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetInitialCaseComponent.cs) | `LoadCases.ModHistNonlinear.SetInitialCase` |
| [SAP Load Cases Mod Hist Nonlinear Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetLoadsComponent.cs) | `LoadCases.ModHistNonlinear.SetLoads` |
| [SAP Load Cases Mod Hist Nonlinear Modal Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetModalCaseComponent.cs) | `LoadCases.ModHistNonlinear.SetModalCase` |
| [SAP Load Cases Mod Hist Nonlinear Sol Control Parameters](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetSolControlParametersComponent.cs) | `LoadCases.ModHistNonlinear.SetSolControlParameters` |
| [SAP Load Cases Mod Hist Nonlinear Time Step](../Rhino2SAP.Grasshopper/Generated/LoadCases_ModHistNonlinear_SetTimeStepComponent.cs) | `LoadCases.ModHistNonlinear.SetTimeStep` |
| [SAP Time History Function](../Rhino2SAP.Grasshopper/FunctionComponents.cs) | `Func.FuncTH.SetUser` |

## 24 Altri casi dinamici

| Componente | Metodo API |
|---|---|
| [SAP Load Cases External Results Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_ExternalResults_SetCaseComponent.cs) | `LoadCases.ExternalResults.SetCase` |
| [SAP Load Cases External Results Number Steps](../Rhino2SAP.Grasshopper/Generated/LoadCases_ExternalResults_SetNumberStepsComponent.cs) | `LoadCases.ExternalResults.SetNumberSteps` |
| [SAP Load Cases Moving Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_Moving_SetCaseComponent.cs) | `LoadCases.Moving.SetCase` |
| [SAP Load Cases Moving Directional Factors](../Rhino2SAP.Grasshopper/Generated/LoadCases_Moving_SetDirectionalFactorsComponent.cs) | `LoadCases.Moving.SetDirectionalFactors` |
| [SAP Load Cases Moving Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_Moving_SetInitialCaseComponent.cs) | `LoadCases.Moving.SetInitialCase` |
| [SAP Load Cases Moving Lanes Loaded](../Rhino2SAP.Grasshopper/Generated/LoadCases_Moving_SetLanesLoadedComponent.cs) | `LoadCases.Moving.SetLanesLoaded` |
| [SAP Load Cases Moving Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_Moving_SetLoadsComponent.cs) | `LoadCases.Moving.SetLoads` |
| [SAP Load Cases Moving Multi Lane SF](../Rhino2SAP.Grasshopper/Generated/LoadCases_Moving_SetMultiLaneSFComponent.cs) | `LoadCases.Moving.SetMultiLaneSF` |
| [SAP Load Cases PSD Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_PSD_SetCaseComponent.cs) | `LoadCases.PSD.SetCase` |
| [SAP Load Cases PSD Damp Constant](../Rhino2SAP.Grasshopper/Generated/LoadCases_PSD_SetDampConstantComponent.cs) | `LoadCases.PSD.SetDampConstant` |
| [SAP Load Cases PSD Damp Interpolated](../Rhino2SAP.Grasshopper/Generated/LoadCases_PSD_SetDampInterpolatedComponent.cs) | `LoadCases.PSD.SetDampInterpolated` |
| [SAP Load Cases PSD Freq Data](../Rhino2SAP.Grasshopper/Generated/LoadCases_PSD_SetFreqDataComponent.cs) | `LoadCases.PSD.SetFreqData` |
| [SAP Load Cases PSD Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_PSD_SetInitialCaseComponent.cs) | `LoadCases.PSD.SetInitialCase` |
| [SAP Load Cases PSD Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_PSD_SetLoadsComponent.cs) | `LoadCases.PSD.SetLoads` |
| [SAP Load Cases Steady State Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_SteadyState_SetCaseComponent.cs) | `LoadCases.SteadyState.SetCase` |
| [SAP Load Cases Steady State Damp Constant](../Rhino2SAP.Grasshopper/Generated/LoadCases_SteadyState_SetDampConstantComponent.cs) | `LoadCases.SteadyState.SetDampConstant` |
| [SAP Load Cases Steady State Damp Interpolated](../Rhino2SAP.Grasshopper/Generated/LoadCases_SteadyState_SetDampInterpolatedComponent.cs) | `LoadCases.SteadyState.SetDampInterpolated` |
| [SAP Load Cases Steady State Freq Data](../Rhino2SAP.Grasshopper/Generated/LoadCases_SteadyState_SetFreqDataComponent.cs) | `LoadCases.SteadyState.SetFreqData` |
| [SAP Load Cases Steady State Initial Case](../Rhino2SAP.Grasshopper/Generated/LoadCases_SteadyState_SetInitialCaseComponent.cs) | `LoadCases.SteadyState.SetInitialCase` |
| [SAP Load Cases Steady State Loads](../Rhino2SAP.Grasshopper/Generated/LoadCases_SteadyState_SetLoadsComponent.cs) | `LoadCases.SteadyState.SetLoads` |
| [SAP Power Spectrum Function](../Rhino2SAP.Grasshopper/FunctionComponents.cs) | `Func.FuncPSD.SetUser` |
| [SAP Steady State Function](../Rhino2SAP.Grasshopper/FunctionComponents.cs) | `Func.FuncSS.SetUser` |

## 25 Combinazioni

| Componente | Metodo API |
|---|---|
| [SAP Load Combination](../Rhino2SAP.Grasshopper/ConvenienceComponents.cs) | — |
| [SAP Resp Combo Add](../Rhino2SAP.Grasshopper/Generated/RespCombo_AddComponent.cs) | `RespCombo.Add` |
| [SAP Resp Combo Case List](../Rhino2SAP.Grasshopper/Generated/RespCombo_SetCaseListComponent.cs) | `RespCombo.SetCaseList` |
| [SAP Resp Combo Case List 1](../Rhino2SAP.Grasshopper/Generated/RespCombo_SetCaseList_1Component.cs) | `RespCombo.SetCaseList_1` |
| [SAP Resp Combo Note](../Rhino2SAP.Grasshopper/Generated/RespCombo_SetNoteComponent.cs) | `RespCombo.SetNote` |
| [SAP Resp Combo Type](../Rhino2SAP.Grasshopper/Generated/RespCombo_SetTypeComponent.cs) | `RespCombo.SetType` |
| [SAP Resp Combo Type OAPI](../Rhino2SAP.Grasshopper/Generated/RespCombo_SetTypeOAPIComponent.cs) | `RespCombo.SetTypeOAPI` |

## 26 Analisi e file

| Componente | Metodo API |
|---|---|
| [Export SAP2000 Model](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [Import SAP Model](../Rhino2SAP.Grasshopper/ImportModelComponent.cs) | — |
| [Read SAP API Table](../Rhino2SAP.Grasshopper/ReadComponents.cs) | — |
| [Read SAP Geometry](../Rhino2SAP.Grasshopper/ReadComponents.cs) | — |
| [Read SAP Results Into Model](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [Run SAP Model Cases and Combinations](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [SAP Analyze and Embed Results](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [SAP File Paths](../Rhino2SAP.Grasshopper/FilePathsComponent.cs) | — |

## 27 Risultati nodali

| Componente | Metodo API |
|---|---|
| [SAP Model Assembled Joint Mass](../Rhino2SAP.Grasshopper/Generated/Results_AssembledJointMassComponent.cs) | `Results.AssembledJointMass` |
| [SAP Model Assembled Joint Mass 1](../Rhino2SAP.Grasshopper/Generated/Results_AssembledJointMass_1Component.cs) | `Results.AssembledJointMass_1` |
| [SAP Model Joint Acc](../Rhino2SAP.Grasshopper/Generated/Results_JointAccComponent.cs) | `Results.JointAcc` |
| [SAP Model Joint Acc Abs](../Rhino2SAP.Grasshopper/Generated/Results_JointAccAbsComponent.cs) | `Results.JointAccAbs` |
| [SAP Model Joint Displ](../Rhino2SAP.Grasshopper/Generated/Results_JointDisplComponent.cs) | `Results.JointDispl` |
| [SAP Model Joint Displ Abs](../Rhino2SAP.Grasshopper/Generated/Results_JointDisplAbsComponent.cs) | `Results.JointDisplAbs` |
| [SAP Model Joint React](../Rhino2SAP.Grasshopper/Generated/Results_JointReactComponent.cs) | `Results.JointReact` |
| [SAP Model Joint Resp Spec](../Rhino2SAP.Grasshopper/Generated/Results_JointRespSpecComponent.cs) | `Results.JointRespSpec` |
| [SAP Model Joint Vel](../Rhino2SAP.Grasshopper/Generated/Results_JointVelComponent.cs) | `Results.JointVel` |
| [SAP Model Joint Vel Abs](../Rhino2SAP.Grasshopper/Generated/Results_JointVelAbsComponent.cs) | `Results.JointVelAbs` |

## 28 Risultati beam

| Componente | Metodo API |
|---|---|
| [Decompose SAP Beam](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |
| [SAP Beam Results N V T M](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |
| [SAP Model Frame Force](../Rhino2SAP.Grasshopper/Generated/Results_FrameForceComponent.cs) | `Results.FrameForce` |
| [SAP Model Frame Force Diagram](../Rhino2SAP.Grasshopper/ResultTools.cs) | — |
| [SAP Model Frame Joint Force](../Rhino2SAP.Grasshopper/Generated/Results_FrameJointForceComponent.cs) | `Results.FrameJointForce` |

## 29 Risultati plate

| Componente | Metodo API |
|---|---|
| [Decompose SAP Plate](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |
| [SAP Model Area Force Shell](../Rhino2SAP.Grasshopper/Generated/Results_AreaForceShellComponent.cs) | `Results.AreaForceShell` |
| [SAP Model Area Joint Force Plane](../Rhino2SAP.Grasshopper/Generated/Results_AreaJointForcePlaneComponent.cs) | `Results.AreaJointForcePlane` |
| [SAP Model Area Joint Force Shell](../Rhino2SAP.Grasshopper/Generated/Results_AreaJointForceShellComponent.cs) | `Results.AreaJointForceShell` |
| [SAP Model Area Strain Shell](../Rhino2SAP.Grasshopper/Generated/Results_AreaStrainShellComponent.cs) | `Results.AreaStrainShell` |
| [SAP Model Area Strain Shell Layered](../Rhino2SAP.Grasshopper/Generated/Results_AreaStrainShellLayeredComponent.cs) | `Results.AreaStrainShellLayered` |
| [SAP Model Area Stress Plane](../Rhino2SAP.Grasshopper/Generated/Results_AreaStressPlaneComponent.cs) | `Results.AreaStressPlane` |
| [SAP Model Area Stress Shell](../Rhino2SAP.Grasshopper/Generated/Results_AreaStressShellComponent.cs) | `Results.AreaStressShell` |
| [SAP Model Area Stress Shell Layered](../Rhino2SAP.Grasshopper/Generated/Results_AreaStressShellLayeredComponent.cs) | `Results.AreaStressShellLayered` |
| [SAP Plate Results Forces and Moments](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |
| [SAP Plate Results Strains](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |
| [SAP Plate Results Stresses](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |

## 30 Risultati solidi-link

| Componente | Metodo API |
|---|---|
| [SAP Model Link Deformation](../Rhino2SAP.Grasshopper/Generated/Results_LinkDeformationComponent.cs) | `Results.LinkDeformation` |
| [SAP Model Link Force](../Rhino2SAP.Grasshopper/Generated/Results_LinkForceComponent.cs) | `Results.LinkForce` |
| [SAP Model Link Joint Force](../Rhino2SAP.Grasshopper/Generated/Results_LinkJointForceComponent.cs) | `Results.LinkJointForce` |
| [SAP Model Panel Zone Deformation](../Rhino2SAP.Grasshopper/Generated/Results_PanelZoneDeformationComponent.cs) | `Results.PanelZoneDeformation` |
| [SAP Model Panel Zone Force](../Rhino2SAP.Grasshopper/Generated/Results_PanelZoneForceComponent.cs) | `Results.PanelZoneForce` |
| [SAP Model Solid Joint Force](../Rhino2SAP.Grasshopper/Generated/Results_SolidJointForceComponent.cs) | `Results.SolidJointForce` |
| [SAP Model Solid Strain](../Rhino2SAP.Grasshopper/Generated/Results_SolidStrainComponent.cs) | `Results.SolidStrain` |
| [SAP Model Solid Stress](../Rhino2SAP.Grasshopper/Generated/Results_SolidStressComponent.cs) | `Results.SolidStress` |

## 31 Risultati modali

| Componente | Metodo API |
|---|---|
| [SAP Model Buckling Factor](../Rhino2SAP.Grasshopper/Generated/Results_BucklingFactorComponent.cs) | `Results.BucklingFactor` |
| [SAP Model Modal Load Participation Ratios](../Rhino2SAP.Grasshopper/Generated/Results_ModalLoadParticipationRatiosComponent.cs) | `Results.ModalLoadParticipationRatios` |
| [SAP Model Modal Participating Mass Ratios](../Rhino2SAP.Grasshopper/Generated/Results_ModalParticipatingMassRatiosComponent.cs) | `Results.ModalParticipatingMassRatios` |
| [SAP Model Modal Participation Factors](../Rhino2SAP.Grasshopper/Generated/Results_ModalParticipationFactorsComponent.cs) | `Results.ModalParticipationFactors` |
| [SAP Model Modal Period](../Rhino2SAP.Grasshopper/Generated/Results_ModalPeriodComponent.cs) | `Results.ModalPeriod` |
| [SAP Model Mode Shape](../Rhino2SAP.Grasshopper/Generated/Results_ModeShapeComponent.cs) | `Results.ModeShape` |

## 32 Risultati e query

| Componente | Metodo API |
|---|---|
| [Query SAP Element Results](../Rhino2SAP.Grasshopper/ElementResultComponents.cs) | — |
| [SAP Model Base React](../Rhino2SAP.Grasshopper/Generated/Results_BaseReactComponent.cs) | `Results.BaseReact` |
| [SAP Model Base React With Centroid](../Rhino2SAP.Grasshopper/Generated/Results_BaseReactWithCentroidComponent.cs) | `Results.BaseReactWithCentroid` |
| [SAP Model Deformed Geometry](../Rhino2SAP.Grasshopper/ResultTools.cs) | — |
| [SAP Model Generalized Displ](../Rhino2SAP.Grasshopper/Generated/Results_GeneralizedDisplComponent.cs) | `Results.GeneralizedDispl` |
| [SAP Model Result Cases](../Rhino2SAP.Grasshopper/ResultTools.cs) | — |
| [SAP Model Result Extrema](../Rhino2SAP.Grasshopper/ResultTools.cs) | — |
| [SAP Model Section Cut Analysis](../Rhino2SAP.Grasshopper/Generated/Results_SectionCutAnalysisComponent.cs) | `Results.SectionCutAnalysis` |
| [SAP Model Section Cut Design](../Rhino2SAP.Grasshopper/Generated/Results_SectionCutDesignComponent.cs) | `Results.SectionCutDesign` |
| [SAP Model Step Label](../Rhino2SAP.Grasshopper/Generated/Results_StepLabelComponent.cs) | `Results.StepLabel` |
| [SAP Model Verification Data](../Rhino2SAP.Grasshopper/ResultTools.cs) | — |
| [SAP Result Quantities](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |

## 33 Preview e bake

| Componente | Metodo API |
|---|---|
| [Bake SAP Geometry](../Rhino2SAP.Grasshopper/PhysicalGeometry.cs) | — |
| [Preview SAP Model](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [Rhino2SAP Panel](../Rhino2SAP.Grasshopper/DisplaySettings.cs) | — |
| [SAP Display Settings](../Rhino2SAP.Grasshopper/DisplaySettings.cs) | — |
| [SAP Model Geometry](../Rhino2SAP.Grasshopper/ModelComponents.cs) | — |
| [SAP Model Physical Geometry](../Rhino2SAP.Grasshopper/PhysicalGeometry.cs) | — |

## 34 Utility

| Componente | Metodo API |
|---|---|
| [Filter SAP Elements](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
| [SAP API Signature](../Rhino2SAP.Grasshopper/ReadComponents.cs) | — |
| [SAP Definition Info](../Rhino2SAP.Grasshopper/AlignmentComponents.cs) | — |
| [SAP Element Info](../Rhino2SAP.Grasshopper/ElementComponents.cs) | — |
