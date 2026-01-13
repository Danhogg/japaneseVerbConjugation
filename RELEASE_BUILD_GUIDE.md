# Release Build Guide

## Overview
This guide explains how to create a release build of Japanese Verb Conjugation for sharing with testers.

## Prerequisites
- .NET 8.0 SDK installed
- Visual Studio or command line tools

## Building a Release Version

### Option 1: Command Line (Recommended)

1. **Build the Release version:**
   ```powershell
   cd "c:\Users\Dan\source\repos\japaneseVerbConjugation"
   dotnet build --configuration Release
   ```

2. **Publish as Self-Contained (includes .NET runtime):**
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
   ```
   
   This creates a single executable that doesn't require .NET to be installed on the target machine.

3. **Or Publish as Framework-Dependent (requires .NET 8.0 installed):**
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained false
   ```

4. **Output location:**
   - The published files will be in: `japaneseVerbConjugation\bin\Release\net8.0-windows\win-x64\publish\`
   - Look for `JapaneseVerbConjugation.exe`

### Option 2: Visual Studio

1. Set configuration to **Release**
2. Right-click the project → **Publish**
3. Choose **Folder** as publish target
4. Configure:
   - **Target Framework**: net8.0-windows
   - **Deployment Mode**: Self-Contained (or Framework-dependent)
   - **Target Runtime**: win-x64
   - **File publish options**: Produce single file (optional but recommended)

## What to Include in the Release Package

### Required Files:
1. **JapaneseVerbConjugation.exe** - The main executable
2. **jmdict-eng-3.6.1.json.gz** - Dictionary file (from `Data` folder)
3. **N5.csv** - Sample verb list (from `Data` folder)
4. **N4.csv** - Sample verb list (from `Data` folder)

### Optional Files:
- **README.md** - Instructions for testers
- **LICENSE** - If you have one

## Creating a Distribution Package

### Option A: ZIP File (Simple)
1. Create a folder: `JapaneseVerbConjugation-v1.0`
2. Copy the required files into it
3. Zip the folder
4. Share the ZIP file

### Option B: Installer (Advanced)
Consider using:
- **WiX Toolset** - Professional Windows installer
- **Inno Setup** - Free, easy-to-use installer
- **NSIS** - Nullsoft Scriptable Install System

## Testing the Release Build

Before sharing:
1. Test on a clean machine (or VM) without .NET installed (if self-contained)
2. Verify dictionary loads correctly
3. Test import functionality
4. Test all conjugation forms
5. Verify persistence works (saves/loads answers)

## File Structure for Distribution

```
JapaneseVerbConjugation-v1.0/
├── JapaneseVerbConjugation.exe
├── jmdict-eng-3.6.1.json.gz
├── N5.csv
├── N4.csv
└── README.txt (optional)
```

## Notes

- **Self-contained builds** are larger (~70-100MB) but don't require .NET installation
- **Framework-dependent builds** are smaller (~5-10MB) but require .NET 8.0 Desktop Runtime
- The dictionary file is large (~50MB compressed) - consider if you want to include it or have users download separately
- User data is stored in: `%AppData%\JapaneseVerbConjugation\`

## Quick Build Script

Save this as `build-release.ps1`:

```powershell
# Build Release Script
$projectPath = "japaneseVerbConjugation"
$outputPath = "Release"

Write-Host "Building Release version..." -ForegroundColor Green
dotnet build $projectPath --configuration Release

Write-Host "Publishing self-contained executable..." -ForegroundColor Green
dotnet publish $projectPath -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $outputPath

Write-Host "Copying data files..." -ForegroundColor Green
Copy-Item "$projectPath\Data\*" -Destination "$outputPath\" -Recurse

Write-Host "Release build complete! Files in: $outputPath" -ForegroundColor Green
```
