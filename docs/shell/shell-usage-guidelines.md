# Shell Usage Guidelines

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Overview](#overview)
- [Why Two Shells?](#why-two-shells)
- [Known Issues with PowerShell](#known-issues-with-powershell)
  - [Git Hooks in PowerShell](#git-hooks-in-powershell)
  - [Project-Specific Examples](#project-specific-examples)
- [Recommended Shell Usage](#recommended-shell-usage)
  - [Use PowerShell For](#use-powershell-for)
  - [Use Git Bash For](#use-git-bash-for)
- [Script Conversion Guidelines](#script-conversion-guidelines)
  - [Converting PowerShell Git Hooks to Bash](#converting-powershell-git-hooks-to-bash)
- [Best Practices](#best-practices)
- [Example: Calling Git Bash from PowerShell](#example-calling-git-bash-from-powershell)
- [Troubleshooting](#troubleshooting)
- [Integrating Git Bash with PowerShell](#integrating-git-bash-with-powershell)
  - [Using Invoke-GitBash Helper](#using-invoke-gitbash-helper)
  - [Lessons Learned](#lessons-learned)
- [Example: Real-World Usage](#example-real-world-usage)

## Overview

This document outlines our recommended approach for using PowerShell and Git Bash in parallel
on Windows development environments. While PowerShell is our primary shell for Windows-specific
tasks, certain Git operations (especially hooks) require Git Bash for reliable execution.

## Why Two Shells?

The need to use both shells stems from fundamental differences in how Windows and Unix-like
systems handle certain operations:

1. **Path Handling**: Windows uses backslashes and drive letters, while Unix-like systems
   use forward slashes and mount points.
2. **Script Execution**: Windows relies on file extensions (`.exe`, `.bat`, `.ps1`) to
   identify executables, while Unix-like systems use file permissions.
3. **Environment Variables**: Windows and Unix-like systems handle environment variables
   differently, affecting script behavior.

## Known Issues with PowerShell

### Git Hooks in PowerShell

We encountered specific issues with Git hooks in PowerShell:

- Git expects hook files without extensions, but Windows requires them
- PowerShell scripts (`.ps1`) are not natively recognized as executable by Git
- Path handling in hook scripts can be problematic due to different path formats
- Our markdown linting pre-commit hook failed to execute properly in PowerShell

### Project-Specific Examples

In our project, we initially implemented Git hooks using PowerShell:

1. **Markdown Linting Hook** (`scripts/lint-markdown.ps1`)
   - Originally implemented as a PowerShell script
   - Required complex wrapper scripts to work as a Git hook
   - Faced execution and path handling issues
   - Should be converted to a bash script

2. **Hook Installation** (`scripts/install-markdown-lint-hook.ps1`)
   - Created multiple files to work around PowerShell limitations:
     - A cmd wrapper script (pre-commit)
     - A PowerShell script (pre-commit.ps1)
   - Complex error-prone setup
   - Should be simplified as a bash script

## Recommended Shell Usage

### Use PowerShell For

- Day-to-day development tasks
- Windows system administration
- .NET development and debugging
- Windows-specific automation
- Scripts that interact with Windows services
- Package management (NuGet, etc.)

### Use Git Bash For

- Git hooks (especially pre-commit hooks)
- Shell scripts that expect Unix-style commands
- Operations requiring Unix-style path handling
- Cross-platform shell scripts
- When following Git-related tutorials that assume a Unix-like environment

## Script Conversion Guidelines

### Converting PowerShell Git Hooks to Bash

When converting Git hook scripts from PowerShell to Bash:

1. **File Naming**
   - Remove `.ps1` extension for hook scripts
   - Use `.sh` extension for supporting scripts
   - Example: `lint-markdown.ps1` → `lint-markdown.sh`

2. **Path Handling**
   - Replace Windows paths with Unix-style paths
   - Use relative paths when possible
   - Use `$(dirname "$0")` for script location
   - Example:

     ```powershell
     # PowerShell
     $scriptPath = Split-Path $MyInvocation.MyCommand.Path -Parent
     ```

     ```bash
     # Bash
     scriptPath="$(dirname "$0")"
     ```

3. **Command Conversion**
   - Replace PowerShell cmdlets with bash commands:

     ```powershell
     # PowerShell
     Get-ChildItem -Filter "*.md" -Recurse
     ```

     ```bash
     # Bash
     find . -name "*.md"
     ```

4. **Error Handling**
   - Replace PowerShell error handling:

     ```powershell
     # PowerShell
     $ErrorActionPreference = "Stop"
     ```

     ```bash
     # Bash
     set -e
     ```

## Best Practices

1. **Script Organization**
   - Keep PowerShell scripts in `.ps1` files
   - Keep Bash scripts in `.sh` files
   - Use appropriate shebang lines in scripts
   - Document which shell is required for each script

2. **Path Handling**
   - Use forward slashes in Git-related paths
   - Use environment variables when possible
   - Consider using relative paths in Git hooks

3. **Git Hook Development**
   - Write Git hooks assuming Git Bash execution
   - Test hooks in Git Bash environment
   - Document hook requirements clearly

## Example: Calling Git Bash from PowerShell

When needed, you can call Git Bash from PowerShell using:

```powershell
& 'C:\Program Files\Git\bin\bash.exe' -c "your-command-here"
```

## Troubleshooting

If you encounter issues:

1. Check which shell is executing the script
2. Verify path formats match the executing shell
3. Ensure scripts have correct line endings (CRLF for Windows, LF for Bash)
4. Check file permissions and execution policies

## Integrating Git Bash with PowerShell

### Using Invoke-GitBash Helper

TODO: This contains detail information about the helper function. We should hold it in the helper or in a specific md
file.

We've created a PowerShell helper function to properly invoke Git Bash scripts from PowerShell.
This solves common issues with path conversion and script execution:

```powershell
# First, load the helper function
. ./scripts/Invoke-GitBash.ps1

# Run a bash script
Invoke-GitBash "./scripts/your-script.sh"

# Run with arguments
Invoke-GitBash "./scripts/your-script.sh" "arg1" "arg2"
```

The helper provides:

- Automatic path conversion between Windows and Unix formats
- Proper script execution in Git Bash environment
- Error handling and exit code propagation
- Clear execution feedback

### Lessons Learned

Through our experience with Git hooks and shell integration, we discovered:

1. **Direct PowerShell Execution Issues**
   - Running Git Bash commands directly in PowerShell often fails
   - Path formats and environment differences cause problems
   - Simple `& 'C:\Program Files\Git\bin\bash.exe'` calls can be unreliable

2. **Path Handling Complexity**
   - Windows and Unix paths need careful conversion
   - Environment variables may need translation
   - Relative paths are more reliable across environments

3. **Script Execution Context**
   - Git Bash scripts need proper environment setup
   - Some tools (like Node.js) behave differently in different shells
   - Exit codes must be properly propagated

4. **Best Practices for Integration**
   - Use the `Invoke-GitBash` helper for all Git Bash script execution
   - Keep Git Bash scripts focused on Git/Unix-style operations
   - Document shell requirements in script headers
   - Test scripts in both environments

## Example: Real-World Usage

Our markdown linting setup demonstrates these principles:

```powershell
# Install the pre-commit hook
Invoke-GitBash "./scripts/install-markdown-lint-hook.sh"

# Run linting manually
Invoke-GitBash "./scripts/lint-markdown.sh"
```

The actual scripts remain pure bash scripts, but we can reliably execute them from PowerShell
using our helper function.
