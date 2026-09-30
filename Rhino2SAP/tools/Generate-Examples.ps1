$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$stage=Join-Path $root ('.local\examples-'+[Guid]::NewGuid().ToString('N'))
dotnet run --project (Join-Path $PSScriptRoot 'GenerateExamples') -c Release -- $root $stage
if($LASTEXITCODE){throw 'Example generation failed.'}
$report=Get-Content -LiteralPath (Join-Path $stage 'validation.json') -Raw | ConvertFrom-Json
if($report.Examples.Count -eq 0 -or $report.CoveredTopics -ne $report.Topics){throw 'Incomplete examples/topic coverage report.'}
foreach($entry in $report.Examples){
    $stem=[IO.Path]::GetFileNameWithoutExtension($entry.File)
    if(-not $entry.RoundTripGh -or -not $entry.RoundTripGhx -or -not $entry.ActionsDisabled){throw "Failed validation: $stem"}
    foreach($ext in '.gh','.ghx','.png','.md'){if(-not (Test-Path -LiteralPath (Join-Path $stage ($stem+$ext)))){throw "Missing example output: $stem$ext"}}
}
$destination=Join-Path $root 'examples';New-Item -ItemType Directory -Path $destination -Force | Out-Null
Get-ChildItem -LiteralPath $stage -File | Copy-Item -Destination $destination -Force
# Keep image-link casing consistent when refreshing older Windows-generated examples.
foreach($file in Get-ChildItem -LiteralPath $stage -File){
    $target=Get-ChildItem -LiteralPath $destination -File | Where-Object Name -eq $file.Name
    if($target.Name -cne $file.Name){
        $temporary=Join-Path $destination ('.case-'+[Guid]::NewGuid().ToString('N')+'.tmp')
        Move-Item -LiteralPath $target.FullName -Destination $temporary
        Move-Item -LiteralPath $temporary -Destination (Join-Path $destination $file.Name)
    }
}
$archive=Join-Path $root 'artifacts/Rhino2SAP_Examples.zip'
New-Item -ItemType Directory -Path (Split-Path $archive) -Force | Out-Null
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $archive -Force
Write-Output "Published $($report.Examples.Count) examples with canvas previews, guides and $($report.CoveredTopics) topics: $destination"
Write-Output "Archive: $archive"
