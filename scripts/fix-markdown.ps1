# Fix-Markdown.ps1
# ================
#
# PURPOSE:
# Runs multiple markdown fixing scripts to correct common issues like line length and blank lines.
#
# USAGE:
#   ./scripts/fix-markdown.ps1                      # Fixes all markdown files
#   ./scripts/fix-markdown.ps1 -FilePath docs/file.md  # Fixes a specific file
#
# PARAMETERS:
#   -FilePath   The path to the markdown file to fix (default: processes all .md files)
#   -FixTypes   Types of fixes to apply ("all", "line-length", "multiple-blanks")

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string]$FilePath,
    
    [Parameter(Position = 1)]
    [ValidateSet("all", "line-length", "multiple-blanks")]
    [string[]]$FixTypes = @("all")
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Invoke-Fix {
    param (
        [string]$FixScript,
        [string]$Path
    )
    
    $scriptPath = Join-Path $scriptDir $FixScript
    if (-not (Test-Path $scriptPath)) {
        Write-Error "Fix script not found: $scriptPath"
        return
    }
    
    Write-Host "🔧 Running $FixScript on '$Path'..." -ForegroundColor Cyan
    
    if ($Path) {
        & $scriptPath -FilePath $Path
    }
    else {
        & $scriptPath
    }
}

# Determine which fixes to apply
$shouldFixLineLength = $FixTypes -contains "all" -or $FixTypes -contains "line-length"
$shouldFixMultipleBlanks = $FixTypes -contains "all" -or $FixTypes -contains "multiple-blanks"

# Apply fixes
if ($shouldFixMultipleBlanks) {
    Invoke-Fix -FixScript "fix-multiple-blanks.ps1" -Path $FilePath
}

if ($shouldFixLineLength) {
    Invoke-Fix -FixScript "fix-line-length.ps1" -Path $FilePath
}

Write-Host "✨ Markdown fixes completed!" -ForegroundColor Green 