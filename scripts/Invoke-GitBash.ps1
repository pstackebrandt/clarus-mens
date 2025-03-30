# Invoke-GitBash.ps1
# =================
#
# Purpose:
# Provides functions to properly invoke Git Bash commands from PowerShell
# while handling path conversions and environment setup.
#
# Functions:
# - Convert-ToGitBashPath: Converts Windows paths to Git Bash format
# - Invoke-GitBash: Executes scripts in Git Bash environment
#
# Usage:
# ```powershell
# # Load the module
# . ./scripts/Invoke-GitBash.ps1
#
# # Convert a path
# $gitBashPath = Convert-ToGitBashPath "C:\Users\you\project\script.sh"
#
# # Run a script
# Invoke-GitBash "./scripts/your-script.sh"
#
# # Run with arguments
# Invoke-GitBash "./scripts/your-script.sh" "arg1" "arg2"
# ```
#
# Requirements:
# - Git for Windows installed (default location: C:\Program Files\Git)
# - PowerShell 5.1 or later
#
# Notes:
# - Paths are automatically converted to Git Bash format
# - Exit codes are properly propagated
# - Error handling is included
# - Execution feedback is provided

function Convert-ToGitBashPath {
    param (
        [Parameter(Mandatory = $true)]
        [string]$WindowsPath
    )
    
    # Convert to forward slashes
    $path = $WindowsPath -replace '\\', '/'
    
    # Convert drive letter format
    if ($path -match '^[A-Za-z]:') {
        $driveLetter = $path.Substring(0, 1).ToLower()
        $path = "/c$($path.Substring(2))"
    }
    
    return $path
}

function Invoke-GitBash {
    param (
        [Parameter(Mandatory = $true)]
        [string]$ScriptPath,
        
        [Parameter(Mandatory = $false)]
        [string[]]$Arguments
    )
    
    $gitBashExe = 'C:\Program Files\Git\bin\bash.exe'
    if (-not (Test-Path $gitBashExe)) {
        throw "Git Bash not found at: $gitBashExe"
    }
    
    # Convert the script path to Git Bash format
    $gitBashPath = Convert-ToGitBashPath (Resolve-Path $ScriptPath)
    
    Write-Host "🚀 Executing Git Bash script: $gitBashPath"
    
    # Build the command array
    $cmdArgs = @()
    if ($Arguments) {
        $cmdArgs = @($gitBashPath) + $Arguments
    }
    else {
        $cmdArgs = @($gitBashPath)
    }
    
    # Execute the script
    & $gitBashExe $cmdArgs
    
    # Return the exit code
    return $LASTEXITCODE
} 