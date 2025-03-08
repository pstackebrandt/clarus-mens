# PowerShell Execution Policy Guide

## Running Update-Version.ps1

The `Update-Version.ps1` script updates version numbers in `Directory.Build.props`
following Semantic Versioning (SemVer), but requires appropriate PowerShell
execution policy settings to run successfully.

## Execution Options

### Option 1: Temporary Bypass (Session Only)

This option applies only to the current PowerShell session and is useful for
one-time executions:

```powershell
Set-ExecutionPolicy Bypass -Scope Process
.\Update-Version.ps1 -VersionType minor
```

### Option 2: Development Environment Setup

For development environments where you'll frequently run local scripts:

```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

This allows unsigned local scripts to run while still requiring signatures for
downloaded scripts. After setting this once, you can run scripts normally:

```powershell
.\Update-Version.ps1 -VersionType minor
```

### Option 3: One-line Execution Bypass

Run the script directly with execution policy bypass:

```powershell
powershell.exe -ExecutionPolicy Bypass -File .\Update-Version.ps1 -VersionType minor
```

### Option 4: Script Signing (Production Environments)

For production environments, consider signing your scripts with a code-signing
certificate:

1. Obtain a code-signing certificate
2. Sign the script using:

   ```powershell
   Set-AuthenticodeSignature -FilePath .\Update-Version.ps1 -Certificate (Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert)
   ```

## Current Project Practice

This project currently uses the temporary bypass approach (Option 1) during
development:

```powershell
Set-ExecutionPolicy Bypass -Scope Process
```

## Understanding Execution Policies

PowerShell execution policies are security features that control the conditions
under which PowerShell loads configuration files and runs scripts.

Common execution policy settings:

| Policy       | Description                                                |
| ------------ | ---------------------------------------------------------- |
| Restricted   | Doesn't load configuration files or run scripts            |
| AllSigned    | Requires all scripts to be signed by a trusted publisher   |
| RemoteSigned | Requires scripts downloaded from the internet to be signed |
| Bypass       | Nothing is blocked and there are no warnings or prompts    |
| Unrestricted | Loads all configuration files and runs all scripts         |

Check your current policy settings with:

```powershell
Get-ExecutionPolicy -List
```

## Security Considerations

- **RemoteSigned** provides a good balance of security and usability for most
  development environments
- **Bypass** should be used cautiously and only for trusted scripts
- Consider the security implications before changing execution policies
- When possible, use the most restrictive policy that still allows you to work
  efficiently
