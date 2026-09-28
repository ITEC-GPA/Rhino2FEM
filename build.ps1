[CmdletBinding()]
param([switch]$SkipTests, [switch]$RhinoSmoke, [switch]$KeepBuildOutputs)
$ErrorActionPreference = 'Stop'
foreach ($project in 'Rhino2Straus', 'Rhino2SAP', 'Rhino2Midas', 'Rhino2MidasGen') {
    & (Join-Path $PSScriptRoot "$project/build.ps1") -SkipTests:$SkipTests -RhinoSmoke:$RhinoSmoke -KeepBuildOutputs:$KeepBuildOutputs
}