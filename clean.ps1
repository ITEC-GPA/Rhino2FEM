[CmdletBinding(SupportsShouldProcess)]
param()
$ErrorActionPreference = 'Stop'
foreach ($project in 'Rhino2Straus', 'Rhino2SAP', 'Rhino2Midas', 'Rhino2MidasGen') {
    & (Join-Path $PSScriptRoot "$project/clean.ps1") -WhatIf:$WhatIfPreference
}