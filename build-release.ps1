# Build Release Script for Japanese Verb Conjugation
# This script builds a self-contained release version ready for distribution

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Japanese Verb Conjugation - Release Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$projectPath = "japaneseVerbConjugation"
$outputPath = "Release"
$runtime = "win-x64"

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

# Run tests first
Write-Host "Running unit tests..." -ForegroundColor Yellow
dotnet test --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed! Aborting build." -ForegroundColor Red
    exit 1
}

# Build Release version
Write-Host "Building Release version..." -ForegroundColor Green
dotnet build $projectPath --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
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

# Verify data files are included
Write-Host "Verifying data files..." -ForegroundColor Yellow
$dataFiles = @(
    "jmdict-eng-3.6.1.json.gz",
    "N5.csv",
    "N4.csv"
)

foreach ($file in $dataFiles) {
    $sourcePath = Join-Path $projectPath "Data" $file
    $destPath = Join-Path $outputPath $file
    
    if (Test-Path $sourcePath) {
        if (-not (Test-Path $destPath)) {
            Write-Host "Copying $file..." -ForegroundColor Yellow
            Copy-Item $sourcePath -Destination $destPath -Force
        }
    } else {
        Write-Host "Warning: $file not found in Data folder" -ForegroundColor Yellow
    }
}

# Display results
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Output location: $((Get-Location).Path)\$outputPath" -ForegroundColor White
Write-Host ""
Write-Host "Files included:" -ForegroundColor White
Get-ChildItem $outputPath -File | ForEach-Object {
    $size = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($size MB)" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Ready for distribution!" -ForegroundColor Green
