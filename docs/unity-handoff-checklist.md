# Unity Handoff Checklist

This checklist is for the machine that has Unity or Tuanjie Editor installed.

## Goal

Prove the same war loop that passed on the laptop also boots, compiles, and runs inside Unity/Tuanjie Editor before any scene setup work begins.

## Before Opening Unity

1. Pull or copy the repository.
2. From the repository root, run the one-step handoff gate in Git Bash on Windows/macOS/Linux:

```bash
tools/verify_unity_handoff.sh
```

or in Windows PowerShell:

```powershell
./tools/verify_unity_handoff.ps1
```

or in Windows Command Prompt:

```bat
tools\verify_unity_handoff.cmd
```

This gate runs:

```bash
python tools/unity/preflight_without_unity.py
tools/verify_headless_war.sh "My project/Assets/Data" faction_qin_shi_huang
```

The PowerShell entrypoint runs the same checks through `tools/verify_headless_war.ps1`.

`tools/verify_unity_handoff.sh` auto-detects `python` first and falls back to `python3`, so Windows Git Bash environments with a broken `python3` launcher can still run the gate.

Expected headless result:

```text
DATA VALIDATION PASSED
[domain-core] OK: Domain folder is Unity-free and adapters delegate to Domain types.
Headless war verification: passed=True scenarioCount=14
[PASS] defender_holds_and_attacker_retreats turns=2
[PASS] attacker_wins_and_occupies turns=2
[PASS] reinforcement_joins_existing_engagement turns=2
[PASS] active_retreat_leaves_engagement turns=2
[PASS] tax_pressure_raises_rebellion turns=0
[PASS] conscription_consumes_population_and_manpower turns=0
[PASS] war_consumes_grain_and_supply turns=1
[PASS] low_supply_reduces_battle_power turns=2
[PASS] border_control_costs_and_diplomatic_pressure turns=0
[PASS] region_specialization_and_governance_forecasts turns=0
[PASS] occupation_control_chain_progression turns=0
[PASS] connected_campaign_vision_and_interception turns=1
[PASS] relief_and_tax_pressure_causality turns=0
[PASS] food_supply_order_diplomacy_coupling turns=0
```

The command rewrites `tools/headless_runner/latest-war-report.json`. That file is generated output and should not be committed.

## Unity/Tuanjie Editor Smoke

1. Open `My project` as the Unity project.
2. Let Unity import packages and compile scripts.
3. Run PlayMode tests:

```bash
tools/unity/run_playmode_tests.sh
```

or in Windows PowerShell:

```powershell
./tools/unity/run_playmode_tests.ps1
```

or in Windows Command Prompt:

```bat
tools\unity\run_playmode_tests.cmd
```

4. The runner auto-detects `../Editor/Tuanjie.exe` beside this repository when present. If the script cannot find Unity or Tuanjie, set the platform-specific editor path:

macOS:

```bash
export UNITY_BIN="/path/to/Unity.app/Contents/MacOS/Unity"
tools/unity/run_playmode_tests.sh
```

Windows Git Bash with Unity Hub-compatible editor:

```bash
export UNITY_BIN="/c/Program Files/Unity/Hub/Editor/2022.3.62t7/Editor/Unity.exe"
tools/unity/run_playmode_tests.sh
```

Windows Git Bash with standalone Tuanjie:

```bash
export UNITY_BIN="/e/万朝归一/Editor/Tuanjie.exe"
tools/unity/run_playmode_tests.sh
```

Windows PowerShell:

```powershell
$env:UNITY_BIN="E:/万朝归一/Editor/Tuanjie.exe"
./tools/unity/run_playmode_tests.ps1
```

## Expected Result

- `GameManager` creates `DataRepository`, `WorldState`, `MapQueryService`, and `MapCommandService`.
- The default player faction is `faction_qin_shi_huang`.
- The first war turn creates contact but does not resolve battle.
- The second war turn resolves battle and runs economy.
- Restarting the game rebinds the map-led war systems to the new `WorldState`.

## Visual Smoke Screenshots

Run this only on a Unity/Tuanjie machine with an active graphics device:

```powershell
./tools/unity/run_visual_smoke_tests.ps1
```

or in Windows Command Prompt:

```bat
tools\unity\run_visual_smoke_tests.cmd
```

The visual runner copies `My project` into `.outputs/tuanjie/visual-project-copy`, runs `VisualSmokeCaptureTests` without `-nographics`, verifies these 12 `unity-*.png` screenshots are non-empty `1600x900` renders, then deletes the temporary screenshots and project copy:

- `unity-map-hud.png`
- `unity-region-building-panel.png`
- `unity-weather-panel.png`
- `unity-governance-default.png`
- `unity-governance-forecast.png`
- `unity-occupation-chain.png`
- `unity-war-route-risk.png`
- `unity-map-lens-risk.png`
- `unity-outliner.png`
- `unity-diplomacy-bridge.png`
- `unity-war-route.png`
- `unity-battle-report.png`

If the first isolated import is stopped by the known URP `Cannot create required material because shader is null` log, the runner retries once on the same imported copy.

## If Something Fails

- Data validation failure: fix the JSON table or reference listed by `tools/validate_data.py` before opening Unity.
- Domain validation failure: keep runtime Domain scripts Unity-free and move Unity-specific behavior behind adapters.
- Headless war failure: inspect `tools/headless_runner/latest-war-report.json`; the failing scenario includes `failureStage`, `failureReason`, `phaseResults`, `assertions`, `keyDeltas`, and `logs`.
- Unity compile failure: fix compiler errors first, then rerun `tools/unity/run_playmode_tests.sh`.
- PlayMode failure: compare Unity logs with the headless report to determine whether the issue is runtime logic or Unity binding.
- VisualSmoke failure: inspect `.outputs/tuanjie/visual-smoke-graphics-copy-results.xml` and `.outputs/tuanjie/visual-smoke-graphics-copy.log`; screenshots remain temporary and should not be committed.
