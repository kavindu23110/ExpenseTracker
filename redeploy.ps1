# Rebuilds the Docker images from the current code and replaces the running containers.
# Docker writes progress to stderr, so failures are judged by exit code.
$ErrorActionPreference = "Continue"
Set-Location $PSScriptRoot

& docker info --format "{{.ServerVersion}}" *> $null
if ($LASTEXITCODE -ne 0) { throw "Docker isn't running. Start Docker, then try again." }

Write-Host "Building and starting the containers"
& docker compose up -d --build
if ($LASTEXITCODE -ne 0) { throw "The deploy failed. See the messages above." }

# The frontend passes this through to the API, so one call checks both containers and the database link.
$check = "fetch('http://localhost:3000/api/auth/session').then((r) => process.exit(r.ok ? 0 : 1)).catch(() => process.exit(1))"
for ($i = 0; $i -lt 60; $i++) {
    & docker compose exec -T web node -e $check *> $null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Deployed. Open http://172.30.0.50:3000 with WARP connected."
        exit 0
    }
    Start-Sleep -Seconds 2
}
& docker compose logs --tail 20 api
throw "The containers started, but the app isn't answering. The API log is above."
