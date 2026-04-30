#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_PATH="${1:-$ROOT_DIR/WanChaoGuiYi}"
RESULTS_PATH="${UNITY_TEST_RESULTS:-/tmp/wanchao-playmode-results.xml}"
LOG_PATH="${UNITY_LOG_PATH:-/tmp/wanchao-playmode.log}"

resolve_unity() {
  if [[ -n "${UNITY_BIN:-}" && -x "$UNITY_BIN" ]]; then
    printf '%s\n' "$UNITY_BIN"
    return 0
  fi

  local candidate
  for candidate in \
    /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
    "$HOME"/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity
  do
    if [[ -x "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  if command -v Unity >/dev/null 2>&1; then
    command -v Unity
    return 0
  fi

  return 1
}

UNITY_EXECUTABLE="$(resolve_unity || true)"
if [[ -z "$UNITY_EXECUTABLE" ]]; then
  echo "Unity editor executable not found. Set UNITY_BIN=/path/to/Unity or install Unity 2022.3.0f1." >&2
  exit 127
fi

"$UNITY_EXECUTABLE" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -runTests \
  -testPlatform PlayMode \
  -testResults "$RESULTS_PATH" \
  -logFile "$LOG_PATH"

echo "PlayMode test results: $RESULTS_PATH"
echo "Unity log: $LOG_PATH"
