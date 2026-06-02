# SkillReducer.ps1
# Implementation of Documentation Minifier for LLM Context Windows
# Part of the LLM Wiki Architecture

param (
    [string]$WikiPath = "./wiki",
    [string]$OutputPath = "./.agent/references/wiki-shortmap.md"
)

Write-Host "Starting SkillReducer..." -ForegroundColor Cyan

if (-not (Test-Path $WikiPath)) {
    Write-Error "Wiki path not found: $WikiPath"
    exit 1
}

$output = "## LLM WIKI SHORTMAP`n"
$output += "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm')`n"
$output += "Mode: Compressed Context`n`n"

$files = Get-ChildItem -Path $WikiPath -Filter "*.md" -Recurse

foreach ($file in $files) {
    if ($file.Name -eq "wiki-shortmap.md") { continue }
    
    Write-Host "Processing $($file.Name)..." -ForegroundColor Gray
    $content = Get-Content $file.FullName -Raw
    
    # --- MINIFICATION LOGIC ---
    
    # 1. Remove HTML comments
    $content = $content -replace "(?s)<!--.*?-->", ""
    
    # 2. Strip Unicode emojis (Simplified for .NET Regex)
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, "[^\x00-\x7F]+", "")
    
    # 3. Replace verbose phrasing
    $content = $content -replace "Furthermore|Moreover|In addition", "+"
    $content = $content -replace "However|On the other hand", "!"
    $content = $content -replace "For example|e\.g\.", "ex:"
    $content = $content -replace "Therefore|Consequently", "->"
    
    # 4. Strip excessive whitespace
    $content = $content -replace "\s+", " "
    $content = $content -replace "\n\s*\n", "`n"
    
    # 5. Limit per-file length in shortmap (Summary focus)
    $relPath = $file.FullName.Replace((Get-Item $WikiPath).FullName + "\", "")
    $output += "### [$relPath]`n$($content.Trim())`n`n"
}

# Ensure directory exists
$outputDir = [System.IO.Path]::GetDirectoryName($OutputPath)
if ($outputDir -and -not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$output | Out-File -FilePath $OutputPath -Encoding utf8
Write-Host "Minification complete. Shortmap saved to: $OutputPath" -ForegroundColor Green
