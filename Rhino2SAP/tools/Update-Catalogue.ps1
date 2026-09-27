$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$entries=Get-Content (Join-Path $root 'docs\components.json') -Raw|ConvertFrom-Json
$groups=@($entries|Group-Object Category|Sort-Object Name)
$lines=@('# Catalogo componenti Rhino2SAP','',"**$($entries.Count) componenti in $($groups.Count) argomenti**, tutti nella scheda **SAP2000** di Grasshopper. I titoli qui sotto corrispondono ai pannelli del plugin; i numeri mantengono l'ordine dei gruppi. GUID, nomi e icone sono verificati dal caricamento reale in Rhino.",'','| Argomento / pannello | Numero |','|---|---:|')
$lines+=$groups|ForEach-Object{'| ['+$_.Name+'](#'+($_.Name.ToLowerInvariant() -replace ' ','-')+') | '+$_.Count+' |'}
foreach($group in $groups){
    $lines+=@('',('## '+$group.Name),'','| Componente | Metodo API |','|---|---|')
    $lines+=$group.Group|Sort-Object Name|ForEach-Object{'| ['+$_.Name+'](../'+$_.File+') | '+$(if($_.Api){'`'+$_.Api+'`'}else{'—'})+' |'}
}
$lines|Set-Content (Join-Path $root 'docs\COMPONENTS.md') -Encoding utf8
Write-Output "$($entries.Count) components in $($groups.Count) Grasshopper topic panels."
