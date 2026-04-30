# Unity Handoff Checklist

This checklist is for the machine that has Unity Editor installed.

## Before Opening Unity

1. Pull or copy the repository.
2. From the repository root, run:

```bash
python3 tools/validate_data.py
python3 tools/validate_domain_core.py
python3 tools/unity/preflight_without_unity.py
tools/run_headless_simulation.sh
```

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
