param([switch]$SkipTests,[switch]$RhinoSmoke)
$ErrorActionPreference='Stop'
Set-Location $PSScriptRoot
dotnet build Rhino2SAP.sln -c Release --nologo -v minimal
if($LASTEXITCODE){throw 'Build failed.'}
if(-not $SkipTests){dotnet run --project Rhino2SAP.Tests -c Release --no-build;if($LASTEXITCODE){throw 'Managed tests failed.'}}
if($RhinoSmoke){dotnet run --project tools/PluginSmoke -c Release;if($LASTEXITCODE){throw 'Rhino smoke test failed.'}; & (Join-Path $PSScriptRoot 'tools\Update-Catalogue.ps1')}
$output=Join-Path $PSScriptRoot 'artifacts\Rhino2SAP'
New-Item -ItemType Directory -Force $output | Out-Null
$plugin=Join-Path $PSScriptRoot 'Rhino2SAP.Grasshopper\bin\Release\net8.0-windows'
$worker=Join-Path $PSScriptRoot 'Rhino2SAP.Worker\bin\Release\net8.0-windows'
Get-ChildItem $plugin -File | Where-Object { $_.Name -like 'Rhino2SAP*' -and $_.Extension -ne '.pdb' } | Copy-Item -Destination $output -Force
Get-ChildItem $worker -File | Where-Object { $_.Name -like 'Rhino2SAP*' -and $_.Extension -ne '.pdb' } | Copy-Item -Destination $output -Force
Copy-Item README.md $output -Force
$docsFolder=Join-Path $output 'docs';New-Item -ItemType Directory -Force $docsFolder | Out-Null
Get-ChildItem 'docs' -File | Where-Object { $_.Extension -in '.md','.png' -or $_.Name -in 'components.json','INPUT-AUDIT.json' } | Copy-Item -Destination $docsFolder -Force
$iconFolder=Join-Path $output 'Icons';New-Item -ItemType Directory -Force $iconFolder | Out-Null
Get-ChildItem 'Rhino2SAP.Grasshopper\Assets\Icons' -Filter '*.png' -ErrorAction SilentlyContinue | Copy-Item -Destination $iconFolder -Force
$examplesFolder=Join-Path $PSScriptRoot 'examples'
if(Test-Path -LiteralPath $examplesFolder){
    $examplesOutput=Join-Path $output 'examples';New-Item -ItemType Directory -Path $examplesOutput -Force | Out-Null
    Get-ChildItem -LiteralPath $examplesFolder -File | Copy-Item -Destination $examplesOutput -Force
}
Compress-Archive -Path $output -DestinationPath (Join-Path $PSScriptRoot 'artifacts\Rhino2SAP.zip') -Force
Write-Output "Package: $output"
