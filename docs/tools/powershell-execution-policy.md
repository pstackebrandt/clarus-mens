# PowerShell Execution Policy Guide

## Table of Contents

- [Table of Contents](#table-of-contents)
- [Purpose](#purpose)
- [Running Update-Version.ps1](#running-update-versionps1)
- [Temporary Bypass (Development)](#temporary-bypass-development)

## Purpose

This guide explains the role of PowerShell
Execution Policies in the project. It offers basic
guidance on running essential scripts like
Update-Version.ps1 using a temporary bypass,
suitable for development.

## Running Update-Version.ps1

The `Update-Version.ps1` script updates version
numbers in `Directory.Build.props` per Semantic
Versioning (SemVer). It requires proper execution
policy settings to run.

## Temporary Bypass (Development)

For development, this project uses the temporary
bypass approach, allowing unsigned scripts to run
in the current session:

```powershell
Set-ExecutionPolicy Bypass -Scope Process
# Run from solution root
.\scripts\Update-Version.ps1 -VersionType minor
```
