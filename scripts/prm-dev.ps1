<#
.SYNOPSIS
    PRM Tool — local dev helpers (build, API, SMTP test, scheduler).

.EXAMPLE
    .\scripts\prm-dev.ps1 stop-api
    .\scripts\prm-dev.ps1 build
    .\scripts\prm-dev.ps1 start-api
    .\scripts\prm-dev.ps1 test-email -To hariadhikari860@gmail.com
    .\scripts\prm-dev.ps1 run-scheduler
    .\scripts\prm-dev.ps1 check-smtp
    .\scripts\prm-dev.ps1 restart-api
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('stop-api', 'build', 'start-api', 'restart-api', 'test-email', 'run-scheduler', 'check-smtp', 'help')]
    [string] $Action = 'help',

    [string] $To = 'hariadhikari860@gmail.com',
    [string] $ApiUrl = 'https://localhost:7171',
    [string] $DbServer = '(localdb)\MSSQLLocalDB',
    [string] $Database = 'PRMToolDb_Resources'
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent
$ApiProject = Join-Path $Root 'ProjectManagementSystem\ProjectManagementSystem.csproj'
$Solution = Join-Path $Root 'ProjectManagementSystem\ProjectManagementSystem.sln'

function Write-Title([string] $Text) {
    Write-Host "`n=== $Text ===" -ForegroundColor Cyan
}

function Stop-PrmApi {
    Write-Title 'Stopping PRM API'
    $procs = Get-Process -Name 'ProjectManagementSystem' -ErrorAction SilentlyContinue
    if (-not $procs) {
        Write-Host 'No ProjectManagementSystem process running.' -ForegroundColor Yellow
        return
    }
    foreach ($p in $procs) {
        Write-Host "Stopping PID $($p.Id) ..."
        Stop-Process -Id $p.Id -Force
    }
    Start-Sleep -Seconds 1
    Write-Host 'API stopped.' -ForegroundColor Green
}

function Build-Prm {
    Write-Title 'Building solution'
    Stop-PrmApi
    dotnet build $Solution
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    Write-Host 'Build succeeded.' -ForegroundColor Green
}

function Start-PrmApi {
    Write-Title 'Starting API (https profile)'
    $listening = netstat -ano 2>$null | Select-String ':7171\s'
    if ($listening) {
        Write-Host 'Port 7171 is already in use. Run: .\scripts\prm-dev.ps1 stop-api' -ForegroundColor Yellow
        return
    }
    Write-Host "API will listen at $ApiUrl"
    Write-Host 'Press Ctrl+C in that window to stop.' -ForegroundColor DarkGray
    Push-Location (Split-Path $ApiProject -Parent)
    try {
        dotnet run --launch-profile https
    }
    finally {
        Pop-Location
    }
}

function Invoke-PrmApiPost([string] $Path) {
    $url = "$ApiUrl$Path"
    Write-Host "POST $url"
    $response = curl.exe -k -s -X POST $url -H 'Content-Type: application/json'
    Write-Host $response
    if ($response -notmatch '"success":true') {
        throw "Request failed: $response"
    }
}

function Send-TestEmail {
    Write-Title "SMTP test email -> $To"
    Invoke-PrmApiPost "/api/dev/send-test-email?to=$([uri]::EscapeDataString($To))"
    Write-Host 'Check inbox and spam folder.' -ForegroundColor Green
}

function Run-Scheduler {
    Write-Title 'Running background scheduler (dev endpoint)'
    Invoke-PrmApiPost '/api/dev/run-scheduler'
    Write-Host 'Scheduler run completed (timesheet reminders, freeze, at-risk emails).' -ForegroundColor Green
}

function Show-SmtpConfig {
    Write-Title 'SMTP settings from database'
    $query = @"
SELECT EmailEnabled, SmtpHost, SmtpPort, SmtpUsername,
  CASE WHEN SmtpPassword IS NULL OR SmtpPassword = '' THEN '(empty)' ELSE '(set)' END AS SmtpPassword,
  EmailFromAddress
FROM SystemConfig
"@
    sqlcmd -S $DbServer -d $Database -Q $query -W -s '|'
}

switch ($Action) {
    'stop-api'      { Stop-PrmApi }
    'build'         { Build-Prm }
    'start-api'     { Start-PrmApi }
    'restart-api'   { Stop-PrmApi; Start-PrmApi }
    'test-email'    { Send-TestEmail }
    'run-scheduler' { Run-Scheduler }
    'check-smtp'    { Show-SmtpConfig }
    default {
        Write-Host @"

PRM Tool — dev script

  .\scripts\prm-dev.ps1 stop-api       Stop running API (fixes DLL lock on build)
  .\scripts\prm-dev.ps1 build          Stop API + dotnet build
  .\scripts\prm-dev.ps1 start-api      Start API on https://localhost:7171
  .\scripts\prm-dev.ps1 restart-api    Stop then start API
  .\scripts\prm-dev.ps1 test-email     Send SMTP test (default: hariadhikari860@gmail.com)
  .\scripts\prm-dev.ps1 run-scheduler  Run timesheet + at-risk notification jobs
  .\scripts\prm-dev.ps1 check-smtp     Show SMTP config from database

Options:
  -To email@example.com   Recipient for test-email
  -ApiUrl https://...     API base URL (default: https://localhost:7171)

Typical workflow:
  1. .\scripts\prm-dev.ps1 build
  2. .\scripts\prm-dev.ps1 start-api          (new terminal, or run in background)
  3. .\scripts\prm-dev.ps1 test-email
  4. .\scripts\prm-dev.ps1 run-scheduler

"@
    }
}
