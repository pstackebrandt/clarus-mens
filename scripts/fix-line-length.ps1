# Fix-Line-Length.ps1
# ===================
#
# PURPOSE:
# Fixes line length issues in markdown files by wrapping lines longer than 120 characters.
# This helps comply with markdownlint rule MD013 (line-length).
#
# USAGE:
#   ./scripts/fix-line-length.ps1                         # Fixes template-usage.md (default)
#   ./scripts/fix-line-length.ps1 -FilePath docs/other-file.md  # Fixes a specific file
#   ./scripts/fix-line-length.ps1 -FilePath path/to/file.md -MaxLength 80  # Custom length
#
# PARAMETERS:
#   -FilePath   The path to the markdown file to fix (default: "docs/template-usage.md")
#   -MaxLength  The maximum allowed line length (default: 120)
#
# NOTES:
#   - The script preserves content while splitting long lines at word boundaries
#   - Designed to work with markdownlint validation rules
#   - Safe to run multiple times on the same file
#   - Works well with our project's markdown linting workflow

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string]$FilePath = "docs/template-usage.md",
    
    [Parameter(Position = 1)]
    [int]$MaxLength = 120
)

# Verify file exists
if (-not (Test-Path $FilePath)) {
    Write-Error "File not found: $FilePath"
    exit 1
}

# Verify file is markdown
if (-not $FilePath.EndsWith(".md")) {
    Write-Warning "The file '$FilePath' doesn't have a .md extension. Continuing anyway..."
}

Write-Host "📏 Processing '$FilePath' (max length: $MaxLength characters)..."

# Read and process the file
$content = Get-Content $FilePath -Raw
$lines = $content -split "`n"
$newLines = @()
$changedLines = 0

# Process each line
foreach ($line in $lines) {
    if ($line.Length -gt $MaxLength) {
        # Split long lines at word boundaries
        $words = $line -split ' '
        $currentLine = ""
        foreach ($word in $words) {
            if (($currentLine.Length + $word.Length + 1) -le $MaxLength) {
                if ($currentLine -eq "") {
                    $currentLine = $word
                }
                else {
                    $currentLine = "$currentLine $word"
                }
            }
            else {
                $newLines += $currentLine
                $currentLine = $word
                $changedLines++
            }
        }
        if ($currentLine -ne "") {
            $newLines += $currentLine
        }
    }
    else {
        $newLines += $line
    }
}

# Save the fixed content
$newContent = $newLines -join "`n"
Set-Content -Path $FilePath -Value $newContent -Encoding UTF8

# Report results
if ($changedLines -gt 0) {
    Write-Host "✅ Fixed $changedLines line length issues in '$FilePath'"
}
else {
    Write-Host "✓ No line length issues found in '$FilePath'"
} 