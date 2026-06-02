# scan-wiki-index.ps1
# Initialization Hook for LLM Wiki - Detects relevant knowledge for the current task

param (
    [string]$Query = "",
    [string]$WikiPath = "./wiki",
    [string]$IndexPath = "./wiki/index.md"
)

if (-not $Query) {
    Write-Host "No query provided. Returning Index only."
    if (Test-Path $IndexPath) {
        Get-Content $IndexPath
    }
    exit 0
}

Write-Host "🔍 Scanning Wiki Index for: $Query" -ForegroundColor Cyan

$results = @()

# 1. Search in Index.md summaries
if (Test-Path $IndexPath) {
    $indexLines = Get-Content $IndexPath | Select-String -Pattern $Query
    foreach ($line in $indexLines) {
        if ($line.Line -match "\[(.*?)\]\((.*?)\)") {
            $results += $Matches[2]
        }
    }
}

# 2. Search in all wiki file contents (Deep Scan)
$files = Get-ChildItem -Path $WikiPath -Filter "*.md" -Recurse | Where-Object { $_.Name -notmatch "index|log|schema" }
foreach ($file in $files) {
    if (Select-String -Path $file.FullName -Pattern $Query -Quiet) {
        $relPath = $file.FullName.Replace((Get-Item ".").FullName + "\", "")
        $results += $relPath
    }
}

$uniqueResults = $results | Select-Object -Unique

if ($uniqueResults.Count -gt 0) {
    Write-Host "Found $($uniqueResults.Count) relevant wiki pages:" -ForegroundColor Green
    foreach ($res in $uniqueResults) {
        Write-Host "-> $res"
    }
} else {
    Write-Host "❓ No direct matches found in Wiki. Proceeding with general knowledge." -ForegroundColor Yellow
}

return $uniqueResults
