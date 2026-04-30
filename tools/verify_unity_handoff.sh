#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA_DIR="${1:-$ROOT_DIR/WanChaoGuiYi/Assets/Data}"
PLAYER_FACTION_ID="${2:-faction_qin_shi_huang}"

python3 "$ROOT_DIR/tools/unity/preflight_without_unity.py"
"$ROOT_DIR/tools/verify_headless_war.sh" "$DATA_DIR" "$PLAYER_FACTION_ID"

cat <<'MSG'

Unity handoff gate passed.
Next on the Unity machine:
  tools/unity/run_playmode_tests.sh

If Unity is not auto-detected:
  export UNITY_BIN="/path/to/Unity.app/Contents/MacOS/Unity"
  tools/unity/run_playmode_tests.sh
MSG
