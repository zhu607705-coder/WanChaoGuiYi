$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$DataDir = if ($args.Count -ge 1) { $args[0] } else { Join-Path $RootDir "My project/Assets/Data" }
$PlayerFactionId = if ($args.Count -ge 2) { $args[1] } else { "faction_qin_shi_huang" }

$PythonBin = $env:PYTHON_BIN
if (-not $PythonBin) {
    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
    if ($pythonCommand) {
        $PythonBin = $pythonCommand.Source
    } else {
        $python3Command = Get-Command python3 -ErrorAction SilentlyContinue
        if ($python3Command) {
            $PythonBin = $python3Command.Source
        } else {
            Write-Error "Python is required to run the Unity handoff gate."
        }
    }
}

& $PythonBin (Join-Path $RootDir "tools/unity/preflight_without_unity.py")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& (Join-Path $RootDir "tools/verify_headless_war.ps1") $DataDir $PlayerFactionId
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "Unity handoff gate passed."
Write-Host "Next on the Unity machine:"
Write-Host "  tools/unity/run_playmode_tests.sh"
Write-Host ""
Write-Host "If Unity is not auto-detected on Windows PowerShell:"
$LocalTuanjie = Join-Path (Split-Path -Parent $RootDir) "Editor/Tuanjie.exe"
Write-Host ('  $env:UNITY_BIN="' + $LocalTuanjie.Replace("\", "/") + '"')
Write-Host "  tools/unity/run_playmode_tests.ps1"
