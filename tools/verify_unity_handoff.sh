#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DATA_DIR="${1:-$ROOT_DIR/My project/Assets/Data}"
PLAYER_FACTION_ID="${2:-faction_qin_shi_huang}"

PYTHON_BIN="${PYTHON_BIN:-}"
if [[ -z "$PYTHON_BIN" ]]; then
  if command -v python >/dev/null 2>&1; then
    PYTHON_BIN="python"
  elif command -v python3 >/dev/null 2>&1; then
    PYTHON_BIN="python3"
  else
    echo "Python is required to run the Unity handoff gate." >&2
    exit 127
  fi
fi

"$PYTHON_BIN" "$ROOT_DIR/tools/unity/preflight_without_unity.py"
PYTHON_BIN="$PYTHON_BIN" "$ROOT_DIR/tools/verify_headless_war.sh" "$DATA_DIR" "$PLAYER_FACTION_ID"

cat <<'MSG'

Unity handoff gate passed.
Next on the Unity machine:
  tools/unity/run_playmode_tests.sh

The PlayMode runner auto-detects ../Editor/Tuanjie.exe beside this repository when present.

If Unity is not auto-detected on macOS:
  export UNITY_BIN="/path/to/Unity.app/Contents/MacOS/Unity"
  tools/unity/run_playmode_tests.sh

If Unity is not auto-detected on Windows Git Bash:
  export UNITY_BIN="/e/万朝归一/Editor/Tuanjie.exe"
  tools/unity/run_playmode_tests.sh
MSG
