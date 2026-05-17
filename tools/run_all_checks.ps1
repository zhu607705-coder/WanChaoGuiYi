#!/usr/bin/env pwsh
# Unified test gate: runs ALL verification suites in dependency order.
# Used by CI and as the local "are we green?" command.
#
# Usage:
#   tools\run_all_checks.ps1
#   tools\run_all_checks.ps1 -SkipPlaywright   # for fast local re-runs

[CmdletBinding()]
param(
    [switch]$SkipPlaywright = $false
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

function Step($name, [scriptblock]$command, $cwd) {
    Write-Host "`n=== $name ===" -ForegroundColor Cyan
    Push-Location $cwd
    try {
        $start = Get-Date
        $global:LASTEXITCODE = 0
        & $command
        $exit = $LASTEXITCODE
        $elapsed = (Get-Date) - $start
        if ($exit -ne 0) {
            Write-Host "[FAIL] $name (exit=$exit, $([int]$elapsed.TotalSeconds)s)" -ForegroundColor Red
            exit $exit
        }
        Write-Host "[OK] $name ($([int]$elapsed.TotalSeconds)s)" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Step 1: data contract validation (cheapest, run first)
Step "validate_domain_core.py" { python tools\validate_domain_core.py } $repoRoot
Step "validate_web_data_source.py" { python tools\validate_web_data_source.py } $repoRoot

# Step 2: domain-core C# xunit suite (~2s, 70 tests)
Step "Domain Core xunit (70+ tests)" { dotnet test --nologo --no-restore } "$repoRoot\tools\headless_runner\WanChaoGuiYiTests"

# Step 3: headless war 16 scenarios
Step "headless war (16 scenarios)" { powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1 } $repoRoot

# Step 4: Web TypeScript checks
Step "Web typecheck" { npm run typecheck } "$repoRoot\web-strategy-map"
Step "Web vitest (50+ tests)" { npm run test:unit } "$repoRoot\web-strategy-map"
Step "Web build" { npm run build } "$repoRoot\web-strategy-map"

# Step 5: Playwright (~3.6m, optional)
if (-not $SkipPlaywright) {
    Step "Playwright UI (22 tests)" { npm run test:ui } "$repoRoot\web-strategy-map"
}

Write-Host "`n[ALL GREEN]" -ForegroundColor Green
