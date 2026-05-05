#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_PATH="${1:-$ROOT_DIR/My project}"
OUTPUT_DIR="${UNITY_OUTPUT_DIR:-$ROOT_DIR/.outputs/tuanjie}"
RESULTS_PATH="${UNITY_TEST_RESULTS:-$OUTPUT_DIR/wanchao-playmode-results.xml}"
LOG_PATH="${UNITY_LOG_PATH:-$OUTPUT_DIR/wanchao-playmode.log}"
mkdir -p "$OUTPUT_DIR"

resolve_unity() {
  if [[ -n "${UNITY_BIN:-}" && -x "$UNITY_BIN" ]]; then
    printf '%s\n' "$UNITY_BIN"
    return 0
  fi

  local local_tuanjie="$ROOT_DIR/../Editor/Tuanjie.exe"
  if [[ -x "$local_tuanjie" ]]; then
    printf '%s\n' "$local_tuanjie"
    return 0
  fi

  local candidate
  for candidate in \
    /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
    "$HOME"/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
    /c/Program\ Files/Unity/Hub/Editor/*/Editor/Unity.exe \
    /c/Program\ Files/Unity\ */Editor/Unity.exe \
    /c/Program\ Files/Unity/Editor/Unity.exe \
    /c/Program\ Files\ \(x86\)/Unity/Editor/Unity.exe
  do
    if [[ -x "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  local windows_candidate
  for windows_candidate in \
    "C:/Program Files/Unity/Hub/Editor"/*/Editor/Unity.exe \
    "C:/Program Files/Unity "*/Editor/Unity.exe \
    "C:/Program Files/Unity/Editor/Unity.exe" \
    "C:/Program Files (x86)/Unity/Editor/Unity.exe"
  do
    if [[ -x "$windows_candidate" ]]; then
      printf '%s\n' "$windows_candidate"
      return 0
    fi
  done

  if command -v Tuanjie >/dev/null 2>&1; then
    command -v Tuanjie
    return 0
  fi

  if command -v Unity >/dev/null 2>&1; then
    command -v Unity
    return 0
  fi

  return 1
}

UNITY_EXECUTABLE="$(resolve_unity || true)"
if [[ -z "$UNITY_EXECUTABLE" ]]; then
  echo "Unity/Tuanjie editor executable not found. Set UNITY_BIN=/path/to/Unity-or-Tuanjie or install Tuanjie 2022.3.62t7." >&2
  exit 127
fi

"$UNITY_EXECUTABLE" \
  -batchmode \
  -nographics \
  -projectPath "$PROJECT_PATH" \
  -runTests \
  -testPlatform PlayMode \
  -testResults "$RESULTS_PATH" \
  -logFile "$LOG_PATH"

if [[ ! -f "$RESULTS_PATH" ]]; then
  echo "PlayMode test results were not created: $RESULTS_PATH" >&2
  exit 3
fi

echo "PlayMode test results: $RESULTS_PATH"
echo "Unity log: $LOG_PATH"
