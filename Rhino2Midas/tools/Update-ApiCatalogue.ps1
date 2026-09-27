param([switch]$Refresh)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$cache=Join-Path $env:TEMP 'Rhino2Midas-ApiDocs'
New-Item -ItemType Directory -Force $cache | Out-Null
$indexPath=Join-Path $cache '33016922742937.json'
if($Refresh -or -not (Test-Path $indexPath)){Invoke-WebRequest 'https://support.midasuser.com/api/v2/help_center/en-us/articles/33016922742937.json' -OutFile $indexPath}
$body=(Get-Content $indexPath -Raw | ConvertFrom-Json).article.body
$links=[regex]::Matches($body,'href="https://support.midasuser.com/hc/en-us/articles/(\d+)[^"]*"[^>]*>(.*?)</a>',[System.Text.RegularExpressions.RegexOptions]::Singleline)
$ids=@($links | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique)
$ids | ForEach-Object -ThrottleLimit 6 -Parallel {
    $file=Join-Path $using:cache "$_.json"
    if($using:Refresh -or -not (Test-Path $file)){
        try{Invoke-WebRequest "https://support.midasuser.com/api/v2/help_center/en-us/articles/$_.json" -OutFile $file -ErrorAction Stop}
        catch{Write-Warning "Article $_ could not be downloaded."}
    }
}
$catalogue=@()
if(-not ('MidasJsonSnippets' -as [type])){Add-Type @'
using System.Collections.Generic;
public static class MidasJsonSnippets {
  public static IEnumerable<string> Read(string text) {
    int depth=0,start=0; bool quoted=false,escaped=false;
    for(int i=0;i<text.Length;i++) {
      char c=text[i];
      if(depth==0) {if(c=='{'){start=i;depth=1;quoted=false;escaped=false;}continue;}
      if(quoted){if(escaped)escaped=false;else if(c=='\\')escaped=true;else if(c=='"')quoted=false;continue;}
      if(c=='"')quoted=true;else if(c=='{')depth++;else if(c=='}'&&--depth==0)yield return text.Substring(start,i-start+1);
    }
  }
}
'@}
foreach($id in $ids){
    $file=Join-Path $cache "$id.json"
    if(-not (Test-Path $file)){continue}
    $article=(Get-Content $file -Raw | ConvertFrom-Json).article
    $articleText=[System.Net.WebUtility]::HtmlDecode(($article.body -replace '<[^>]+>',' '))
    $endpoint=[regex]::Match($articleText,'(?:/|\s)(db|doc|post|ope)/([A-Za-z0-9_-]+)').Value.Trim().TrimStart('/')
    if(-not $endpoint){continue}
    $examples=@()
    foreach($code in [MidasJsonSnippets]::Read($articleText.Replace([char]0xA0,' '))){
        try{$json=ConvertFrom-Json $code -AsHashtable -ErrorAction Stop;$examples+=,$json}catch{}
    }
    $catalogue+=@{Title=$article.title;Endpoint=$endpoint;Url=$article.html_url;Updated=$article.updated_at;Samples=$examples}
}
New-Item -ItemType Directory -Force (Join-Path $root 'docs') | Out-Null
$catalogue | ConvertTo-Json -Depth 100 | Set-Content (Join-Path $root 'docs\api-catalogue.json') -Encoding utf8
Write-Output "$($catalogue.Count) documented API entries from $($ids.Count) articles. Cache: $cache"
