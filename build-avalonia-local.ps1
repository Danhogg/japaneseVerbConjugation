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

# Read version from MSBuild (VersionPrefix or Version)
$version = $null
try {
    $vp = (dotnet msbuild $project -getProperty:VersionPrefix -nologo | Where-Object { $_ -and $_.Trim() }) | Select-Object -Last 1
    if ($vp) {
        $version = $vp.Trim()
    }
    if ([string]::IsNullOrWhiteSpace($version)) {
        $v = (dotnet msbuild $project -getProperty:Version -nologo | Where-Object { $_ -and $_.Trim() }) | Select-Object -Last 1
        if ($v) {
            $version = $v.Trim()
        }
    }
} catch {
    $version = $null
}
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "0.0.0"
}

if ([string]::IsNullOrWhiteSpace($OutputFolder)) {
    $OutputFolder = "JapaneseConjugationTraining-Avalonia-v$version-$Runtime"
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

Write-Host "Copying data files..." -ForegroundColor Yellow
$dataDir = Join-Path $OutputFolder "Data"
New-Item -ItemType Directory -Force -Path $dataDir | Out-Null

$src = "JapaneseVerbConjugation.Core\Data"
$files = @("N5.csv", "N4.csv", "jmdict-eng-3.6.1.json.gz")
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
