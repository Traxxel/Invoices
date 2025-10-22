# Push To Git Script
# Verwendung: .\ptg.ps1 "Mein Commit-Kommentar"

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$CommitMessage
)

# Farben für Output
$ErrorColor = "Red"
$SuccessColor = "Green"
$InfoColor = "Cyan"

Write-Host "==================================" -ForegroundColor $InfoColor
Write-Host "Push To Git (ptg.ps1)" -ForegroundColor $InfoColor
Write-Host "==================================" -ForegroundColor $InfoColor
Write-Host ""

# Prüfe ob wir in einem Git-Repository sind
if (-not (Test-Path ".git")) {
    Write-Host "FEHLER: Kein Git-Repository gefunden!" -ForegroundColor $ErrorColor
    exit 1
}

# 1. Git Status anzeigen
Write-Host "Status:" -ForegroundColor $InfoColor
git status --short
Write-Host ""

# 2. Alle Änderungen hinzufügen
Write-Host "Füge alle Änderungen hinzu (git add .)..." -ForegroundColor $InfoColor
git add .

if ($LASTEXITCODE -ne 0) {
    Write-Host "FEHLER: git add fehlgeschlagen!" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "✓ Änderungen hinzugefügt" -ForegroundColor $SuccessColor
Write-Host ""

# 3. Commit erstellen
Write-Host "Erstelle Commit mit Nachricht: '$CommitMessage'..." -ForegroundColor $InfoColor
git commit -m "$CommitMessage"

if ($LASTEXITCODE -ne 0) {
    Write-Host "FEHLER: git commit fehlgeschlagen!" -ForegroundColor $ErrorColor
    Write-Host "Möglicherweise gibt es keine Änderungen zum committen." -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "✓ Commit erstellt" -ForegroundColor $SuccessColor
Write-Host ""

# 4. Push zu GitHub
Write-Host "Pushe zu GitHub..." -ForegroundColor $InfoColor
git push

if ($LASTEXITCODE -ne 0) {
    Write-Host "FEHLER: git push fehlgeschlagen!" -ForegroundColor $ErrorColor
    exit 1
}

Write-Host "✓ Erfolgreich zu GitHub gepusht!" -ForegroundColor $SuccessColor
Write-Host ""
Write-Host "==================================" -ForegroundColor $SuccessColor
Write-Host "FERTIG!" -ForegroundColor $SuccessColor
Write-Host "==================================" -ForegroundColor $SuccessColor

