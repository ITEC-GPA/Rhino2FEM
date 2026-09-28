$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage=Join-Path $root ('.local\examples-'+[Guid]::NewGuid().ToString('N'))
dotnet run --project (Join-Path $PSScriptRoot 'GenerateExamples') -c Release -- $root $stage
if($LASTEXITCODE){throw 'Example generation failed.'}
$report=Get-Content -LiteralPath (Join-Path $stage 'validation.json') -Raw | ConvertFrom-Json
if($report.Examples.Count -ne 3){throw 'Incomplete examples report.'}
foreach($entry in $report.Examples){
    $stem=[IO.Path]::GetFileNameWithoutExtension($entry.File)
    foreach($ext in '.gh','.ghx','.png'){if(-not (Test-Path -LiteralPath (Join-Path $stage ($stem+$ext)))){throw "Missing example output: $stem$ext"}}
}
$destination=Join-Path $root 'examples';New-Item -ItemType Directory -Path $destination -Force | Out-Null
Get-ChildItem -LiteralPath $stage -File | Copy-Item -Destination $destination -Force
Write-Output "Published 3 examples in GH and GHX format, with 3 canvas previews and validation report: $destination"
