$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$schema=Get-Content (Join-Path $root 'docs\sap-api-schema.json') -Raw | ConvertFrom-Json
$count=0
$generated=Join-Path $root 'Rhino2SAP.Grasshopper\Generated'
New-Item -ItemType Directory -Force $generated | Out-Null
function Human([string]$text){return (($text -replace '_',' ' -creplace '([a-z0-9])([A-Z])','$1 $2') -replace '\s+',' ').Trim()}
foreach($m in $schema){
    $include=$false;$result=$false
    if($m.Path -eq 'Results'){$include=$true;$result=$true}
    elseif($m.Method -match '^(Set|Add$|ImportProp$)' -and $m.Path -match '^(PropMaterial|PropFrame|PropArea|PropLink|PropSolid|PropCable|PropTendon|ConstraintDef|GroupDef|CoordSys|SourceMass|LoadPatterns|LoadCases\.|RespCombo|PointObj|FrameObj|AreaObj|SolidObj|LinkObj|CableObj|TendonObj)'){
        if($m.Method -match 'Selected|GUID|SetName|SetLoad.*WithGUID') {continue}
        $include=$true
    }
    if(-not $include){continue}
    # Keep all documented API versions; suffixes are part of the native method name.
    if(@($m.Parameters | Where-Object { $_.Type -notmatch '^(System\.(String|Boolean|Int32|Double)(\[\])?|SAP2000v1\.e\w+(\[\])?)$' }).Count){continue}
    $class=($m.Key -replace '[^a-zA-Z0-9_]','_')+'Component'
    $prefix=switch -Regex ($m.Path){'^PropFrame$' {'Frame Section';break};'^PropArea$' {'Area Property';break};'^PropLink$' {'Link Property';break};'^PropMaterial$' {'Material';break};'^PointObj$' {'Node';break};'^FrameObj$' {'Frame';break};'^AreaObj$' {'Area';break};'^SolidObj$' {'Solid';break};'^LinkObj$' {'Link';break};'^Results$' {'Model';break};default{Human $m.Path.Replace('.',' ')}}
    $title='SAP '+$prefix+' '+(Human ($m.Method -replace '^Set',''))
    $base=if($result){'ApiResultComponent'}else{'ApiCommandComponent'}
    # Actual ribbon topics are assigned centrally by ComponentTopics.ForApi.
    $ctor='base("'+$m.Key+'","'+$title+'")'
    $content="// Generated from the installed SAP2000 26 SDK; see tools/Generate-Components.ps1.`nnamespace Rhino2SAP.Grasshopper;`npublic sealed class $class : $base`n{`n    protected override string MethodKey => `"$($m.Key)`";`n    public $class() : $ctor { }`n}`n"
    Set-Content (Join-Path $generated ($class+'.cs')) $content -Encoding utf8
    $count++
}
Write-Output "$count API components generated. Build and run tools/PluginSmoke, then tools/Update-Catalogue.ps1 to publish the actual ribbon topics and GUIDs."
