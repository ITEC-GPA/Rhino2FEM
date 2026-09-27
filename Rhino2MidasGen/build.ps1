param([switch]$SkipTests,[switch]$RhinoSmoke)
$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
    dotnet build Rhino2MidasGen.sln -c Release --nologo -v minimal
    if($LASTEXITCODE){throw 'Build failed.'}
    if(-not $SkipTests){dotnet run --project Rhino2MidasGen.Tests -c Release --no-build; if($LASTEXITCODE){throw 'Managed tests failed.'}}
    if($RhinoSmoke){dotnet run --project tools/PluginSmoke -c Release; if($LASTEXITCODE -or -not (Select-String -LiteralPath 'docs/rhino-smoke.log' -Pattern '^SMOKE COMPLETE$' -Quiet)){throw 'Rhino smoke test did not complete.'}}
    $output=Join-Path $PSScriptRoot 'artifacts\Rhino2MidasGen'
    New-Item -ItemType Directory -Force $output | Out-Null
    $plugin=Join-Path $PSScriptRoot 'Rhino2MidasGen.Grasshopper\bin\Release\net8.0-windows'
    Get-ChildItem $plugin -File | Where-Object { $_.Name -like 'Rhino2MidasGen*' -and $_.Extension -ne '.pdb' } | Copy-Item -Destination $output -Force
    Copy-Item README.md $output -Force
    $docs=Join-Path $output 'docs';New-Item -ItemType Directory -Force $docs | Out-Null
    Get-ChildItem 'docs' -File | Where-Object { $_.Extension -in '.md','.png' -or $_.Name -in 'components.json','INPUT-AUDIT.json' } | Copy-Item -Destination $docs -Force
    if(Test-Path 'examples'){Copy-Item -LiteralPath 'examples' -Destination $output -Recurse -Force}
    Compress-Archive -Path $output -DestinationPath (Join-Path $PSScriptRoot 'artifacts\Rhino2MidasGen.zip') -Force
    Write-Output "Package: $output"
} finally {Pop-Location}
