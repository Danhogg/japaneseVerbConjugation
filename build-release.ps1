# Build Release Script for Japanese Verb Conjugation
# Deterministic CI-friendly: clean -> test (Release) -> publish -> copy data -> zip

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Fail([string]$message) {
    Write-Host ""
    Write-Host "[ERROR] $message" -ForegroundColor Red
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Japanese Verb Conjugation - Release Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ----------------------------
# Configuration
# ----------------------------
$solutionPath = "japaneseVerbConjugation.sln"
$projectPath  = "japaneseVerbConjugation"
$projectCsproj = Join-Path $projectPath "JapaneseVerbConjugation.csproj"
$testCsproj    = "japaneseVerbConjugationTests\japaneseVerbConjugationTests.csproj"
$runtime       = "win-x64"

if (-not (Test-Path $solutionPath)) { Fail "Solution not found: $solutionPath" }
if (-not (Test-Path $projectCsproj)) { Fail "Project file not found: $projectCsproj" }
if (-not (Test-Path $testCsproj)) { Fail "Test project file not found: $testCsproj" }

# Read version from .csproj file
$csprojContent = Get-Content $projectCsproj -Raw
if ($csprojContent -match '<Version>([^<]+)</Version>') {
    $version = $matches[1]
    Write-Host "Building version: $version" -ForegroundColor Cyan
} else {
    $version = "1.0.0"
    Write-Host "Version not found in .csproj, defaulting to: $version" -ForegroundColor Yellow
}

# Build output names with version
$baseFolderName  = "JapaneseConjugationTraining"
$outputFolderName = "$baseFolderName-v$version"
$outputPath       = $outputFolderName
$zipFileName      = "$outputFolderName.zip"

Write-Host ""
Write-Host "Output folder: $outputPath" -ForegroundColor Gray
Write-Host "ZIP name:      $zipFileName" -ForegroundColor Gray
Write-Host ""

# ----------------------------
# Clean
# ----------------------------
Write-Host "Cleaning previous build artifacts..." -ForegroundColor Yellow

if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

# Clean solution (Release only; Debug is irrelevant for CI determinism)
dotnet clean $solutionPath -c Release | Out-Host
if ($LASTEXITCODE -ne 0) { Fail "dotnet clean failed." }

# Nuke bin/obj to avoid any weirdness (CI or local)
Get-ChildItem -Path . -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path . -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue |
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# ----------------------------
# Restore (optional but clearer logs)
# ----------------------------
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore $solutionPath | Out-Host
if ($LASTEXITCODE -ne 0) { Fail "dotnet restore failed." }

# ----------------------------
# Test (Release) - DO NOT use --no-build
# ----------------------------
Write-Host "Running tests (Release)..." -ForegroundColor Yellow
dotnet test $testCsproj -c Release --verbosity normal | Out-Host
if ($LASTEXITCODE -ne 0) { Fail "Tests failed." }

# ----------------------------
# Publish self-contained executable
# ----------------------------
Write-Host "Publishing self-contained executable (Release, $runtime)..." -ForegroundColor Green

dotnet publish $projectCsproj `
    -c Release `
    -r $runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $outputPath | Out-Host

if ($LASTEXITCODE -ne 0) { Fail "Publish failed." }

# ----------------------------
# Remove PDBs (optional)
# ----------------------------
Write-Host "Removing PDB files (if any)..." -ForegroundColor Yellow
Get-ChildItem -Path $outputPath -Filter "*.pdb" -Recurse -ErrorAction SilentlyContinue |
    Remove-Item -Force -ErrorAction SilentlyContinue

# ----------------------------
# Copy data files
# ----------------------------
Write-Host "Copying data files..." -ForegroundColor Yellow
$dataFolder = Join-Path $outputPath "Data"
New-Item -ItemType Directory -Path $dataFolder -Force | Out-Null

$dataFiles = @(
    "jmdict-eng-3.6.1.json.gz",
    "N5.csv",
    "N4.csv"
)

foreach ($file in $dataFiles) {
    $sourcePath = Join-Path (Join-Path $projectPath "Data") $file
    $destPath   = Join-Path $dataFolder $file

    if (Test-Path $sourcePath) {
        Write-Host "  Copying $file" -ForegroundColor Gray
        Copy-Item $sourcePath -Destination $destPath -Force
    } else {
        Write-Host "  Warning: missing $file at $sourcePath" -ForegroundColor Yellow
    }
}

# ----------------------------
# Create ZIP
# ----------------------------
Write-Host ""
Write-Host "Creating distribution ZIP..." -ForegroundColor Green

if (Test-Path $zipFileName) {
    Remove-Item $zipFileName -Force
}

try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        (Resolve-Path $outputPath).Path,
        (Join-Path (Get-Location) $zipFileName),
        [System.IO.Compression.CompressionLevel]::Optimal,
        $false
    )

    $zipSize = [math]::Round((Get-Item $zipFileName).Length / 1MB, 2)
    Write-Host "  [OK] ZIP created: $zipFileName ($zipSize MB)" -ForegroundColor Green
} catch {
    Fail "Failed to create ZIP: $($_.Exception.Message)"
}

# ----------------------------
# Summary
# ----------------------------
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Output folder: $((Get-Location).Path)\$outputPath" -ForegroundColor White
Write-Host "ZIP file:      $((Get-Location).Path)\$zipFileName" -ForegroundColor White
Write-Host ""
Write-Host "Ready for distribution!" -ForegroundColor Green
