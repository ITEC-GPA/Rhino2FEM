param([string]$InstallDir = 'C:\Program Files\Computers and Structures\SAP2000 26')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $InstallDir 'SAP2000v1.dll'))
$methods = [Collections.Generic.List[object]]::new()
function Visit([Type]$Type, [string]$Path, [int]$Depth) {
    if ($Depth -gt 3) { return }
    foreach ($method in $Type.GetMethods() | Where-Object { -not $_.IsSpecialName }) {
        $parameters = @($method.GetParameters() | ForEach-Object {
            $t = $_.ParameterType
            if ($t.IsByRef) { $t = $t.GetElementType() }
            $enumType = if ($t.IsArray) { $t.GetElementType() } else { $t }
            $default = if ($_.HasDefaultValue -and $_.DefaultValue -isnot [DBNull] -and $_.DefaultValue -isnot [Reflection.Missing]) { $_.DefaultValue } else { $null }
            if ($default -is [Enum]) { $default = [int]$default }
            $enumValues = @(); if($enumType.IsEnum){$enumValues=@([Enum]::GetValues($enumType) | ForEach-Object { [ordered]@{ Name=$_.ToString(); Value=[int]$_ } })}
            [ordered]@{ Name=$_.Name; Type=$t.FullName; ByRef=$_.ParameterType.IsByRef; Optional=$_.IsOptional; Default=$default; Enum=$enumValues }
        })
        $methods.Add([ordered]@{ Key=($Path+'.'+$method.Name).TrimStart('.'); Path=$Path; Method=$method.Name; ReturnType=$method.ReturnType.FullName; Parameters=$parameters })
    }
    foreach ($property in $Type.GetProperties()) {
        if ($property.PropertyType.Namespace -eq 'SAP2000v1') { Visit $property.PropertyType (($Path+'.'+$property.Name).TrimStart('.')) ($Depth+1) }
    }
}
Visit ($assembly.GetType('SAP2000v1.cSapModel')) '' 0
New-Item -ItemType Directory -Force (Join-Path $root 'docs') | Out-Null
$methods | ConvertTo-Json -Depth 15 | Set-Content (Join-Path $root 'docs\sap-api-schema.json') -Encoding utf8
[ordered]@{ Product='SAP2000 26'; Assembly=$assembly.FullName; SDK=(Join-Path $InstallDir 'SAP2000v1.dll'); Sha256=(Get-FileHash (Join-Path $InstallDir 'SAP2000v1.dll')).Hash; Documentation=(Join-Path $InstallDir 'CSI_OAPI_Documentation.chm'); MethodCount=$methods.Count } | ConvertTo-Json | Set-Content (Join-Path $root 'docs\sdk-source.json') -Encoding utf8
Write-Output "Inspected $($methods.Count) API signatures."
