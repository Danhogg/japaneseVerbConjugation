param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [string]$OutputFolder = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Fail([string]$message) {
    Write-Host ""
    Write-Host "[ERROR] $message" -ForegroundColor Red
    exit 1
}

$project = "JapaneseVerbConjugation.AvaloniaUI\JapaneseVerbConjugation.AvaloniaUI.csproj"
if (-not (Test-Path $project)) { Fail "Avalonia project not found: $project" }

if ([string]::IsNullOrWhiteSpace($OutputFolder)) {
    $OutputFolder = "JapaneseConjugationTraining-Avalonia-local-$Runtime"
}

Write-Host "Publishing Avalonia ($Configuration, $Runtime)..." -ForegroundColor Cyan
Write-Host "Output: $OutputFolder" -ForegroundColor Gray

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $OutputFolder

if ($LASTEXITCODE -ne 0) { Fail "Publish failed." }

Write-Host "Copying CSV data files..." -ForegroundColor Yellow
$dataDir = Join-Path $OutputFolder "Data"
New-Item -ItemType Directory -Force -Path $dataDir | Out-Null

$src = "JapaneseVerbConjugation.Core\Data"
$files = @("N5.csv", "N4.csv")
foreach ($f in $files) {
    $from = Join-Path $src $f
    $to = Join-Path $dataDir $f
    if (Test-Path $from) {
        Copy-Item $from $to -Force
    } else {
        Write-Host "Warning: missing $from" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "[OK] Publish complete." -ForegroundColor Green
Write-Host "Run from: $OutputFolder" -ForegroundColor White
