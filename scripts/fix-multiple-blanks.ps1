# Fix-Multiple-Blanks.ps1
# ====================
#
# PURPOSE:
# Fixes multiple consecutive blank lines in markdown files (MD012).
# Also ensures file ends with a single newline (MD047).
#
# USAGE:
#   ./scripts/fix-multiple-blanks.ps1                      # Fixes all markdown files
#   ./scripts/fix-multiple-blanks.ps1 -FilePath docs/file.md  # Fixes a specific file
#   ./scripts/fix-multiple-blanks.ps1 -FilePath docs/file.md -Verbose  # With detailed output
#
# PARAMETERS:
#   -FilePath   The path to the markdown file to fix (default: processes all .md files)

[CmdletBinding()]
param (
    [Parameter(Position = 0)]
    [string]$FilePath
)

function Repair-MultipleBlankLines {
    param (
        [Parameter(Mandatory = $true)]
        [string]$Path
    )
    
    Write-Host "📄 Processing '$Path'..."
    
    try {
        # Read the file content as raw text to preserve all original formatting
        $content = Get-Content -Path $Path -Raw
        
        if (-not $content) {
            Write-Warning "File is empty: $Path"
            return
        }
        
        # Make a backup of the original file (just in case)
        $backupPath = "$Path.bak"
        Copy-Item -Path $Path -Destination $backupPath -Force
        Write-Verbose "Created backup at $backupPath"
        
        # Step 1: Fix trailing newlines - ensure exactly one newline at the end
        $fixedContent = $content.TrimEnd("`r", "`n") + "`n"
        Write-Verbose "Fixed trailing newlines"
        
        # Step 2: Fix consecutive blank lines (3+ newlines → 2 newlines)
        # This preserves a single blank line between content blocks
        $pattern = '(\r?\n){3,}'
        $replacement = "`$1`$1"
        $fixedContent = [regex]::Replace($fixedContent, $pattern, $replacement)
        Write-Verbose "Fixed consecutive blank lines"
        
        # Write the fixed content back to the file
        Set-Content -Path $Path -Value $fixedContent -NoNewline
        
        Write-Host "✅ Fixed markdown formatting issues in '$Path'"
        Write-Verbose "  (Backup created at $backupPath)"
    }
    catch {
        Write-Error "Error processing file: $_"
        # Try to restore from backup if available
        if (Test-Path $backupPath) {
            Write-Warning "Restoring from backup after error"
            Copy-Item -Path $backupPath -Destination $Path -Force
        }
    }
}

# Process all markdown files or a specific file
if ($FilePath) {
    if (-not (Test-Path $FilePath)) {
        Write-Error "File not found: $FilePath"
        exit 1
    }
    
    Repair-MultipleBlankLines -Path $FilePath
}
else {
    $mdFiles = Get-ChildItem -Path . -Filter "*.md" -Recurse
    foreach ($file in $mdFiles) {
        Repair-MultipleBlankLines -Path $file.FullName
    }
} 