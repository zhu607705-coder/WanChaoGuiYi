# Unity Handoff Checklist

This checklist is for the machine that has Unity Editor installed.

## Goal

Prove the same war loop that passed on the laptop also boots, compiles, and runs inside Unity Editor before any scene setup work begins.

## Before Opening Unity

1. Pull or copy the repository.
2. From the repository root, run the one-step handoff gate:

```bash
tools/verify_unity_handoff.sh
```

This gate runs:

```bash
python3 tools/unity/preflight_without_unity.py
tools/verify_headless_war.sh WanChaoGuiYi/Assets/Data faction_qin_shi_huang
```

Expected headless result:

```text
DATA VALIDATION PASSED
[domain-core] OK: Domain folder is Unity-free and adapters delegate to Domain types.
Headless war verification: passed=True scenarioCount=4
[PASS] defender_holds_and_attacker_retreats turns=2
[PASS] attacker_wins_and_occupies turns=2
[PASS] reinforcement_joins_existing_engagement turns=2
[PASS] active_retreat_leaves_engagement turns=2
```

The command rewrites `tools/headless_runner/latest-war-report.json`. That file is generated output and should not be committed.

## Unity Editor Smoke

1. Open `WanChaoGuiYi` as the Unity project.
2. Let Unity import packages and compile scripts.
3. Run PlayMode tests:

```bash
tools/unity/run_playmode_tests.sh
```

4. If the script cannot find Unity, set:

```bash
export UNITY_BIN="/path/to/Unity.app/Contents/MacOS/Unity"
tools/unity/run_playmode_tests.sh
```

## Expected Result

- `GameManager` creates `DataRepository`, `WorldState`, `MapQueryService`, and `MapCommandService`.
- The default player faction is `faction_qin_shi_huang`.
- The first war turn creates contact but does not resolve battle.
- The second war turn resolves battle and runs economy.
- Restarting the game rebinds the map-led war systems to the new `WorldState`.

## If Something Fails

- Data validation failure: fix the JSON table or reference listed by `tools/validate_data.py` before opening Unity.
- Domain validation failure: keep runtime Domain scripts Unity-free and move Unity-specific behavior behind adapters.
- Headless war failure: inspect `tools/headless_runner/latest-war-report.json`; the failing scenario includes `failureStage`, `failureReason`, `phaseResults`, `assertions`, `keyDeltas`, and `logs`.
- Unity compile failure: fix compiler errors first, then rerun `tools/unity/run_playmode_tests.sh`.
- PlayMode failure: compare Unity logs with the headless report to determine whether the issue is runtime logic or Unity binding.
