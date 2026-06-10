#!/usr/bin/env pwsh
# ============================================
#  Kickstart Template - Setup Script
#  Windows/macOS/Linux (PowerShell 7+ veya Windows PowerShell 5.1+)
# ============================================

param(
    [string]$ProjectName
)

$ErrorActionPreference = "Stop"
$OriginalName = "Kickstart"
$OriginalNameLower = "kickstart"

# --- Renk fonksiyonları ---
function Write-Step   { param($msg) Write-Host "  [*] $msg" -ForegroundColor Cyan }
function Write-Ok     { param($msg) Write-Host "  [+] $msg" -ForegroundColor Green }
function Write-Err    { param($msg) Write-Host "  [!] $msg" -ForegroundColor Red }
function Write-Warn   { param($msg) Write-Host "  [~] $msg" -ForegroundColor Yellow }

# --- Banner ---
Write-Host ""
Write-Host "  ============================================" -ForegroundColor Magenta
Write-Host "   Kickstart Template - Otomatik Kurulum" -ForegroundColor Magenta
Write-Host "  ============================================" -ForegroundColor Magenta
Write-Host ""

# --- Proje adı al ---
if (-not $ProjectName) {
    $ProjectName = Read-Host "  Proje adini girin (ornek: MyAwesomeProject)"
}

if ([string]::IsNullOrWhiteSpace($ProjectName)) {
    Write-Err "Proje adi bos olamaz!"
    exit 1
}

# Proje adı validasyonu
if ($ProjectName -notmatch '^[a-zA-Z][a-zA-Z0-9]*$') {
    Write-Err "Proje adi harf ile baslamali ve sadece harf/rakam icermelidir."
    Write-Err "Ornek: MyProject, ECommerceApp, CRMSystem"
    exit 1
}

$ProjectNameLower = $ProjectName.ToLower()
$ScriptRoot = $PSScriptRoot
if (-not $ScriptRoot) { $ScriptRoot = Get-Location }

Write-Host ""
Write-Step "Proje: $ProjectName"
Write-Host ""

# --- 1. Dosya icerikleri degistir ---
Write-Step "Dosya icerikleri guncelleniyor..."

$extensions = @("*.cs", "*.csproj", "*.sln", "*.json", "*.http", "*.vue", "*.ts", "*.js", "*.md", "*.css", "*.scss")
$fileCount = 0
$errorCount = 0

foreach ($ext in $extensions) {
    $files = Get-ChildItem -Path $ScriptRoot -Recurse -Filter $ext -File -ErrorAction SilentlyContinue |
             Where-Object { $_.FullName -notlike "*node_modules*" -and $_.FullName -notlike "*.git*" -and $_.FullName -notlike "*.template.config*" }
    
    foreach ($file in $files) {
        try {
            $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
            $hasChange = $false
            
            if ($content.Contains($OriginalName)) {
                $content = $content.Replace($OriginalName, $ProjectName)
                $hasChange = $true
            }
            if ($content.Contains($OriginalNameLower)) {
                $content = $content.Replace($OriginalNameLower, $ProjectNameLower)
                $hasChange = $true
            }
            
            if ($hasChange) {
                $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
                [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
                $fileCount++
            }
        }
        catch {
            Write-Warn "Atlandi: $($file.Name) - $($_.Exception.Message)"
            $errorCount++
        }
    }
}

Write-Ok "$fileCount dosya guncellendi"

# --- 2. Dosya adlarini degistir (derinlikten yuzeye) ---
Write-Step "Dosya adlari guncelleniyor..."

$renamedFiles = 0
Get-ChildItem -Path $ScriptRoot -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*$OriginalName*" -and $_.FullName -notlike "*.git*" -and $_.FullName -notlike "*.template.config*" } |
    Sort-Object { $_.FullName.Length } -Descending |
    ForEach-Object {
        try {
            $newName = $_.Name.Replace($OriginalName, $ProjectName)
            if ($newName -ne $_.Name) {
                Rename-Item -Path $_.FullName -NewName $newName
                $renamedFiles++
            }
        }
        catch {
            Write-Warn "Dosya yeniden adlandirilamadi: $($_.Name) - $($_.Exception.Message)"
            $errorCount++
        }
    }

Write-Ok "$renamedFiles dosya yeniden adlandirildi"

# --- 3. Klasor adlarini degistir (derinlikten yuzeye) ---
Write-Step "Klasor adlari guncelleniyor..."

$renamedDirs = 0
do {
    $changed = $false
    Get-ChildItem -Path $ScriptRoot -Recurse -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "*$OriginalName*" -and $_.FullName -notlike "*.git*" -and $_.FullName -notlike "*.template.config*" } |
        Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object {
            try {
                $newName = $_.Name.Replace($OriginalName, $ProjectName)
                if ($newName -ne $_.Name) {
                    Rename-Item -Path $_.FullName -NewName $newName
                    $renamedDirs++
                    $changed = $true
                }
            }
            catch {
                Write-Warn "Klasor yeniden adlandirilamadi: $($_.Name) - $($_.Exception.Message)"
                $errorCount++
            }
        }
} while ($changed)

Write-Ok "$renamedDirs klasor yeniden adlandirildi"

# --- 4. Temizlik ---
Write-Step "Temizlik yapiliyor..."

# .template.config'i sil (dotnet new icin gereksiz artik)
$templateConfig = Join-Path $ScriptRoot ".template.config"
if (Test-Path $templateConfig) {
    Remove-Item -Path $templateConfig -Recurse -Force
    Write-Ok ".template.config silindi"
}

# EF Core migrations (yeni projede tek InitialCreate ile baslamak icin)
$migrationsPath = Join-Path $ScriptRoot "Backend\$ProjectName.Infrastructure\Migrations"
if (Test-Path $migrationsPath) {
    Remove-Item -Path $migrationsPath -Recurse -Force
    Write-Ok "Migrations klasoru silindi (dotnet ef migrations add InitialCreate ...)"
}

# Setup script'lerini sil
$setupFiles = @("setup.ps1", "setup.sh")
foreach ($f in $setupFiles) {
    $path = Join-Path $ScriptRoot $f
    if (Test-Path $path) {
        Remove-Item -Path $path -Force
    }
}
Write-Ok "Setup dosyalari silindi"

# --- Sonuc ---
Write-Host ""
Write-Host "  ============================================" -ForegroundColor Green
Write-Host "   KURULUM TAMAMLANDI!" -ForegroundColor Green
Write-Host "  ============================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Proje: $ProjectName" -ForegroundColor White
Write-Host "  Guncellenen dosyalar: $fileCount" -ForegroundColor White
Write-Host "  Yeniden adlandirilan: $($renamedFiles + $renamedDirs)" -ForegroundColor White
if ($errorCount -gt 0) {
    Write-Warn "Atlanilan: $errorCount (detaylar yukarda)"
}
Write-Host ""
Write-Host "  Sonraki adimlar:" -ForegroundColor Yellow
Write-Host "    1. cd Backend && dotnet restore" -ForegroundColor White
Write-Host "    2. appsettings.json'dan PostgreSQL baglantisini duzenle" -ForegroundColor White
Write-Host "    3. dotnet ef migrations add InitialCreate --project $ProjectName.Infrastructure --startup-project $ProjectName.API" -ForegroundColor White
Write-Host "    4. dotnet ef database update --project $ProjectName.Infrastructure --startup-project $ProjectName.API" -ForegroundColor White
Write-Host "    5. dotnet run --project $ProjectName.API" -ForegroundColor White
Write-Host ""
Write-Host "    6. cd Frontend && npm install && npm run dev" -ForegroundColor White
Write-Host ""
Write-Host "  Admin giris: admin@$ProjectNameLower.com / Admin123!" -ForegroundColor Cyan
Write-Host ""
