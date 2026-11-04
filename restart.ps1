# Rebuilds the API and restarts the API (5200) and frontend (3200). Logs go to .logs/.
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$ports = 5200, 3200
$logs = Join-Path $root ".logs"
New-Item -ItemType Directory -Force $logs | Out-Null

function Get-Listeners {
    @(Get-NetTCPConnection -State Listen -LocalPort $ports -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique)
}

# Only this project's servers are stopped; anything else on these ports is an error.
foreach ($id in Get-Listeners) {
    $command = (Get-CimInstance Win32_Process -Filter "ProcessId=$id").CommandLine
    if ($command -notmatch "ExpenseTracker") { throw "Port in use by another program (process $id): $command" }
    Write-Host "Stopping process $id"
    Stop-Process -Id $id -Force -Confirm:$false
}
for ($i = 0; $i -lt 20 -and (Get-Listeners).Count -gt 0; $i++) { Start-Sleep -Milliseconds 500 }
if ((Get-Listeners).Count -gt 0) { throw "The ports did not free up." }

Write-Host "Building the API"
& dotnet build (Join-Path $root "backend/ExpenseTracker.slnx") --nologo -v q
if ($LASTEXITCODE -ne 0) { throw "The build failed, so nothing was started." }

Start-Process dotnet -ArgumentList "run --no-build --project ExpenseTracker.Api --launch-profile http" -WorkingDirectory (Join-Path $root "backend") -WindowStyle Hidden `
    -RedirectStandardOutput (Join-Path $logs "api.log") -RedirectStandardError (Join-Path $logs "api.err.log")
Start-Process cmd -ArgumentList "/c npm run dev" -WorkingDirectory (Join-Path $root "frontend") -WindowStyle Hidden `
    -RedirectStandardOutput (Join-Path $logs "web.log") -RedirectStandardError (Join-Path $logs "web.err.log")

function Wait-For($name, $url, $log) {
    for ($i = 0; $i -lt 90; $i++) {
        try {
            if ((Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 30).StatusCode -eq 200) { Write-Host "$name is up: $url"; return }
        } catch { Start-Sleep -Seconds 1 }
    }
    throw "$name did not start. See $log"
}

Wait-For "API" "http://localhost:5200/health" (Join-Path $logs "api.err.log")
Wait-For "Frontend" "http://localhost:3200" (Join-Path $logs "web.err.log")
