$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$catalogue=Get-Content (Join-Path $root 'docs\api-catalogue.json') -Raw | ConvertFrom-Json -AsHashtable
$definitions=@();$classes=[System.Collections.Generic.List[string]]::new()
$classes.Add('// Generated from official MIDAS API JSON examples by tools/Generate-Components.ps1.')
$classes.Add('namespace Rhino2MidasGen.Grasshopper;')
foreach($entry in $catalogue){
    $articleId=[regex]::Match($entry.Url,'articles/(\d+)').Groups[1].Value
    $key=$entry.Endpoint.Replace('/','_').Replace('-','_')+'_'+$articleId
    $schema=@{};$example=$null;$item=$false
    if($entry.Endpoint.StartsWith('db/')){
        if($entry.Endpoint -in @('db/UNIT','db/NODE','db/ELEM')){continue}
        $sample=@($entry.Samples | Where-Object {$_.ContainsKey('Assign')}) | Select-Object -First 1
        if(-not $sample){continue}
        $example=@($sample.Assign.Values)[0]
        $endpoint=$entry.Endpoint.Split('/')[1]
        $schemaEntry=@($entry.Samples | Where-Object {$_.ContainsKey($endpoint) -and $_[$endpoint].ContainsKey('properties')}) | Select-Object -First 1
        if($schemaEntry){$schema=$schemaEntry[$endpoint]}
        if($example.ContainsKey('ITEMS') -and $example.Keys.Count -eq 1 -and $example.ITEMS.Count -gt 0){$item=$true;$example=$example.ITEMS[0];$example.Remove('ID');if($schema.properties.ITEMS){$schema=$schema.properties.ITEMS.items}}
        $example.Remove('NO')
        $title=($entry.Title -replace '"','\"' -replace '[\r\n]',' ').Trim()
        $classes.Add("public sealed class Api_$($key)Component:NativeDefinitionComponent { protected override string Key=>`"$key`"; public Api_$($key)Component():base(`"Midas GEN $title`",`"$endpoint`",`"14-Native API`"){} }")
    }
    elseif($entry.Endpoint -eq 'post/TABLE'){
        $sample=@($entry.Samples | Where-Object {$_.Argument -and $_.Argument.TABLE_TYPE -and $_.Argument.TABLE_TYPE -ne 'string'}) | Select-Object -First 1
        if(-not $sample){continue}
        $example=$sample.Argument;$example.Remove('EXPORT_PATH')
        $title=($entry.Title -replace '"','\"' -replace '[\r\n]',' ').Trim()
        $classes.Add("public sealed class Request_$($key)Component:NativeResultRequestComponent { protected override string Key=>`"$key`"; public Request_$($key)Component():base(`"Midas GEN $title Request`"){} }")
    }else{continue}
    $definitions+=@{Key=$key;Endpoint=$entry.Endpoint;Title=$entry.Title;Url=$entry.Url;Example=$example;Schema=$schema;Item=$item}
}
$definitions | ConvertTo-Json -Depth 100 | Set-Content (Join-Path $root 'docs\api-definitions.json') -Encoding utf8
$classes | Set-Content (Join-Path $root 'Rhino2MidasGen.Grasshopper\NativeComponents.g.cs') -Encoding utf8
Write-Output "$($definitions.Count) native definition/request components generated."
