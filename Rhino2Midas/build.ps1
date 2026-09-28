[CmdletBinding()]
param([switch]$SkipTests, [switch]$RhinoSmoke, [switch]$KeepBuildOutputs)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    # A fresh build prevents old configurations/assemblies from entering the package.
    & (Join-Path $PSScriptRoot 'clean.ps1')
    dotnet build Rhino2Midas.sln -c Release --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    if (-not $SkipTests) {
        dotnet run --project Rhino2Midas.Tests -c Release --no-build
        if ($LASTEXITCODE -ne 0) { throw 'Managed tests failed.' }
    }

    $output = Join-Path $PSScriptRoot 'bin'
    $plugin = Join-Path $PSScriptRoot 'Rhino2Midas.Grasshopper/bin/Release/net8.0-windows'
    $files = @('Rhino2Midas.Grasshopper.gha', 'Rhino2Midas.Core.dll', 'Rhino2Midas.Api.dll', 'Rhino2Midas.Grasshopper.deps.json', 'Rhino2Midas.Grasshopper.runtimeconfig.json')
    # Check all required files before replacing the previous package.
    foreach ($file in $files) {
        if (-not (Test-Path -LiteralPath (Join-Path $plugin $file) -PathType Leaf)) { throw "Missing runtime file: $file" }
    }
    & (Join-Path $PSScriptRoot 'clean.ps1') -KeepBuildOutputs -ResetPackage
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    foreach ($file in $files) { Copy-Item -LiteralPath (Join-Path $plugin $file) -Destination $output -Force }

    if ($RhinoSmoke) {
        dotnet run --project tools/PluginSmoke -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Rhino smoke test failed.' }
        if (-not (Select-String -LiteralPath 'docs/rhino-smoke.log' -Pattern '^SMOKE COMPLETE$' -Quiet)) { throw 'Rhino smoke test did not complete.' }
    }
    # Only runtime files go in bin. Documentation and examples stay in the ZIP/source tree.
    $archiveInputs = @((Join-Path $output '*'), (Join-Path $PSScriptRoot 'README.md'), (Join-Path $PSScriptRoot 'docs'))
    if (Test-Path -LiteralPath (Join-Path $PSScriptRoot 'examples')) { $archiveInputs += Join-Path $PSScriptRoot 'examples' }
    New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'artifacts') -Force | Out-Null
    $archive = Join-Path $PSScriptRoot 'artifacts/Rhino2Midas.zip'
    Compress-Archive -Path $archiveInputs -DestinationPath $archive -Force
    Write-Output "Plugin folder: $output"
    Write-Output "Archive: $archive"
} finally {
    try {
        if (-not $KeepBuildOutputs) { & (Join-Path $PSScriptRoot 'clean.ps1') }
    } finally { Pop-Location }
}