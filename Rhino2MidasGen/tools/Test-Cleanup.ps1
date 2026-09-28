[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sandbox = Join-Path ([IO.Path]::GetTempPath()) ('Rhino2FEM-cleanup-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $sandbox | Out-Null
function Check([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message } }
function Marker([string]$Path) {
    New-Item -ItemType Directory -Path (Split-Path $Path -Parent) -Force | Out-Null
    [IO.File]::WriteAllText($Path, 'keep-or-delete-test')
}
try {
    foreach ($project in 'Rhino2Straus', 'Rhino2SAP', 'Rhino2Midas', 'Rhino2MidasGen') {
        $fixture = Join-Path $sandbox $project
        New-Item -ItemType Directory -Path $fixture | Out-Null
        Copy-Item -LiteralPath (Join-Path $repo "$project/clean.ps1") -Destination $fixture
        Copy-Item -LiteralPath (Join-Path $repo "$project/Directory.Build.props") -Destination $fixture
        Marker (Join-Path $fixture "$project.sln")
        $currentZip = "$project.zip"
        if ($project -eq 'Rhino2Straus') {
            [xml]$props = Get-Content -LiteralPath (Join-Path $fixture 'Directory.Build.props') -Raw
            $currentZip = "$project-$($props.Project.PropertyGroup.Version).zip"
        }
        $keep = @('bin/current.gha', 'src/Program.cs', 'docs/guide.md', 'examples/model.gh', 'artifacts/Examples.zip', "artifacts/$currentZip")
        $remove = @('src/bin/Debug/old.gha', 'src/obj/old.cache', 'tools/check/bin/old.dll', "artifacts/$project/old.gha", "artifacts/$project-0.0.0.zip")
        foreach ($relative in ($keep + $remove)) { Marker (Join-Path $fixture $relative) }
        $clean = Join-Path $fixture 'clean.ps1'
        & $clean -WhatIf | Out-Null
        foreach ($relative in ($keep + $remove)) { Check (Test-Path -LiteralPath (Join-Path $fixture $relative)) "WhatIf modified $relative" }
        & $clean | Out-Null
        foreach ($relative in $keep) { Check (Test-Path -LiteralPath (Join-Path $fixture $relative)) "Cleanup removed $relative" }
        foreach ($relative in $remove) { Check (-not (Test-Path -LiteralPath (Join-Path $fixture $relative))) "Cleanup left $relative" }
        & $clean | Out-Null
        Check (Test-Path -LiteralPath (Join-Path $fixture 'bin/current.gha')) 'Repeated cleanup removed the plugin'
        Marker (Join-Path $fixture 'src/bin/preserved.dll')
        & $clean -KeepBuildOutputs -ResetPackage | Out-Null
        Check (-not (Test-Path -LiteralPath (Join-Path $fixture 'bin'))) 'Package reset left old files'
        Check (Test-Path -LiteralPath (Join-Path $fixture 'src/bin/preserved.dll')) 'Package reset removed intermediates'

        # A junction inside a generated folder must abort before removing any files.
        $outside = Join-Path $sandbox "$project-outside"
        Marker (Join-Path $outside 'untouched.txt')
        $link = Join-Path $fixture 'src/bin/external'
        New-Item -ItemType Junction -Path $link -Target $outside | Out-Null
        try {
            $blocked = $false
            try { & $clean | Out-Null } catch { $blocked = $_.Exception.Message -like 'Linked*' }
            Check $blocked 'Cleanup did not reject linked content'
            Check (Test-Path -LiteralPath (Join-Path $outside 'untouched.txt')) 'Cleanup touched the external path'
            Check (Test-Path -LiteralPath (Join-Path $fixture 'src/bin/preserved.dll')) 'Cleanup removed files before validating all targets'
        } finally {
            # Remove the junction itself, never recurse through its target.
            if (Test-Path -LiteralPath $link) { Remove-Item -LiteralPath $link -Force }
        }
        Write-Output "PASS ${project}: dry run, preservation, obsolete outputs, repeat cleanup, package reset and junction protection."
    }
} finally {
    $full = [IO.Path]::GetFullPath($sandbox)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if (-not $full.StartsWith($tempRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path $full -Leaf) -notlike 'Rhino2FEM-cleanup-*') { throw "Unsafe test cleanup: $full" }
    $links = @(Get-ChildItem -LiteralPath $full -Recurse -Force | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint })
    if ($links.Count) { throw "Test cleanup contains links: $full" }
    Remove-Item -LiteralPath $full -Recurse -Force
}