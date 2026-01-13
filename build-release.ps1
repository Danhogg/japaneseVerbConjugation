# Build Release Script for Japanese Verb Conjugation
# This script builds a self-contained release version ready for distribution

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Japanese Verb Conjugation - Release Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$projectPath = "japaneseVerbConjugation"
$runtime = "win-x64"

# Read version from .csproj file
$csprojPath = Join-Path $projectPath "JapaneseVerbConjugation.csproj"
$csprojContent = Get-Content $csprojPath -Raw
if ($csprojContent -match '<Version>([^<]+)</Version>') {
    $version = $matches[1]
    Write-Host "Building version: $version" -ForegroundColor Cyan
} else {
    $version = "1.0.0"
    Write-Host "Version not found in .csproj, defaulting to: $version" -ForegroundColor Yellow
}

# Build output names with version
$baseFolderName = "JapaneseConjugationTraining"
$outputFolderName = "$baseFolderName-v$version"
$outputPath = $outputFolderName
$zipFileName = "$outputFolderName.zip"

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

# Clean all configurations to avoid cached Debug builds
Write-Host "Cleaning solution (all configurations)..." -ForegroundColor Yellow
dotnet clean --configuration Debug 2>&1 | Out-Null
dotnet clean --configuration Release

Write-Host "Building and running tests in Release mode..." -ForegroundColor Yellow
dotnet test --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed! Aborting build." -ForegroundColor Red
    exit 1
}

# Publish self-contained executable
Write-Host "Publishing self-contained executable..." -ForegroundColor Green
dotnet publish $projectPath `
    -c Release `
    -r $runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $outputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

# Remove PDB files if any were created (not needed for distribution)
Write-Host "Cleaning up debug files..." -ForegroundColor Yellow
Get-ChildItem -Path $outputPath -Filter "*.pdb" -Recurse | ForEach-Object {
    Write-Host "  Removing $($_.Name)..." -ForegroundColor Gray
    Remove-Item $_.FullName -Force
}

# Copy data files to Data subfolder (app expects Data/ folder structure)
Write-Host "Copying data files..." -ForegroundColor Yellow
$dataFolder = Join-Path $outputPath "Data"
if (-not (Test-Path $dataFolder)) {
    New-Item -ItemType Directory -Path $dataFolder -Force | Out-Null
}

$dataFiles = @(
    "jmdict-eng-3.6.1.json.gz",
    "N5.csv",
    "N4.csv"
)

foreach ($file in $dataFiles) {
    $sourcePath = Join-Path (Join-Path $projectPath "Data") $file
    $destPath = Join-Path $dataFolder $file
    
    if (Test-Path $sourcePath) {
        Write-Host "  Copying $file..." -ForegroundColor Yellow
        Copy-Item $sourcePath -Destination $destPath -Force
    } else {
        Write-Host "  Warning: $file not found in source Data folder" -ForegroundColor Yellow
    }
}

# Create ZIP file for distribution
Write-Host ""
Write-Host "Creating distribution ZIP..." -ForegroundColor Green

# Remove old ZIP if it exists
if (Test-Path $zipFileName) {
    Remove-Item $zipFileName -Force
}

# Create ZIP file
try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($outputPath, $zipFileName, [System.IO.Compression.CompressionLevel]::Optimal, $false)
    
    $zipSize = [math]::Round((Get-Item $zipFileName).Length / 1MB, 2)
    Write-Host "  [OK] ZIP created: $zipFileName ($zipSize MB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to create ZIP: $_" -ForegroundColor Red
    Write-Host "  You can manually zip the $outputFolderName folder" -ForegroundColor Yellow
}

# Display results
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Output folder: $((Get-Location).Path)\$outputPath" -ForegroundColor White
Write-Host "ZIP file: $((Get-Location).Path)\$zipFileName" -ForegroundColor White
Write-Host ""
Write-Host "Files included:" -ForegroundColor White
Get-ChildItem $outputPath -File -Recurse | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 2)
    $relativePath = $_.FullName.Replace((Get-Location).Path + "\$outputPath\", "")
    Write-Host "  - $relativePath ($size MB)" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Ready for distribution!" -ForegroundColor Green
Write-Host "Share the ZIP file: $zipFileName" -ForegroundColor Cyan