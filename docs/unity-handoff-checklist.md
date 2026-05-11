# Unity/Tuanjie Handoff Checklist

This checklist is for a machine that has Unity Editor or Tuanjie installed.

## Goal

Prove the same strategy loop that passed in headless and Web verification can still boot, compile, and run inside the Unity/Tuanjie project before scene setup or visual smoke work begins.

## Before Opening The Editor

1. Pull the repository.
2. From the repository root, run the handoff gate:

```powershell
powershell -ExecutionPolicy Bypass -File tools\verify_unity_handoff.ps1
```

This gate runs:

```powershell
python tools\unity\preflight_without_unity.py
powershell -ExecutionPolicy Bypass -File tools\verify_headless_war.ps1 "My project\Assets\Data" faction_qin_shi_huang
```

Expected result:

```text
[unity-preflight] OK
Headless war verification: passed=True scenarioCount=16
```

The command rewrites `tools/headless_runner/latest-war-report.json`. That file is generated output and should not be committed.

## Editor Smoke

1. Open `My project` as the Unity/Tuanjie project.
2. Let the editor import packages and compile scripts.
3. Run PlayMode tests:

```powershell
powershell -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"
```

or:

```cmd
tools\unity\run_playmode_tests.cmd "My project"
```

If the editor is not auto-detected, set `UNITY_BIN` first:

```powershell
$env:UNITY_BIN="E:\万朝归一\Editor\Tuanjie.exe"
powershell -ExecutionPolicy Bypass -File tools\unity\run_playmode_tests.ps1 "My project"
```

## Expected Result

- `GameManager` creates `DataRepository`, `WorldState`, `MapQueryService`, and `MapCommandService`.
- The default player faction is `faction_qin_shi_huang`.
- Headless war verification reports at least 16 passing scenarios.
- PlayMode results XML reports passing tests and no failed cases.
- Web strategy-map verification remains independent from Unity generated folders.

## If Something Fails

- Data validation failure: fix the JSON table or reference listed by `tools/validate_data.py`.
- Domain validation failure: keep runtime Domain scripts Unity-free and move Unity-specific behavior behind adapters.
- Headless war failure: inspect `tools/headless_runner/latest-war-report.json`; failing scenarios include phase results, assertions, deltas, and logs.
- Editor compile failure: fix compiler errors first, then rerun PlayMode tests.
- PlayMode failure: compare editor logs with the headless report to determine whether the issue is runtime logic or Unity binding.
