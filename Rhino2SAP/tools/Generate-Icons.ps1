$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$staging = Join-Path $root ('.local\icon-render-' + [Guid]::NewGuid().ToString('N'))
dotnet run --project (Join-Path $PSScriptRoot 'IconGallery') -c Release -- $root $staging
if ($LASTEXITCODE) { throw 'Icon rendering failed.' }
$entries = Get-Content -LiteralPath (Join-Path $root 'docs\components.json') -Raw | ConvertFrom-Json
$icons = @(Get-ChildItem -LiteralPath (Join-Path $staging 'Icons') -Filter '*.png' -File)
$sheets = @(Get-ChildItem -LiteralPath $staging -Filter '*.png' -File)
if ($icons.Count -ne $entries.Count + 1 -or $sheets.Count -ne [Math]::Ceiling($entries.Count / 96.0) + 1) {
    throw 'Incomplete icon output.'
}
$destination = Join-Path $root 'Rhino2SAP.Grasshopper\Assets\Icons'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$icons | Copy-Item -Destination $destination -Force
$sheets | Copy-Item -Destination (Join-Path $root 'docs') -Force
Write-Output "Published $($icons.Count) icons and $($sheets.Count) gallery sheets."
