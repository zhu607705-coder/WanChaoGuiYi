#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_PATH="${1:-$ROOT_DIR/My project}"
OUTPUT_DIR="${UNITY_OUTPUT_DIR:-$ROOT_DIR/.outputs/tuanjie}"
RESULTS_PATH="${UNITY_TEST_RESULTS:-$OUTPUT_DIR/wanchao-playmode-results.xml}"
LOG_PATH="${UNITY_LOG_PATH:-$OUTPUT_DIR/wanchao-playmode.log}"
mkdir -p "$OUTPUT_DIR"

PYTHON_BIN="${PYTHON_BIN:-}"
if [[ -z "$PYTHON_BIN" ]]; then
  if command -v python >/dev/null 2>&1; then
    PYTHON_BIN="python"
  elif command -v python3 >/dev/null 2>&1; then
    PYTHON_BIN="python3"
  else
    echo "Python is required to parse PlayMode test results." >&2
    exit 127
  fi
fi

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

"$PYTHON_BIN" - "$RESULTS_PATH" <<'PY'
import sys
import xml.etree.ElementTree as ET

path = sys.argv[1]
root = ET.parse(path).getroot()
if root.tag != "test-run":
    raise SystemExit(f"PlayMode test result root should be <test-run>: {path}")

def get_int(name):
    value = root.attrib.get(name)
    if value is None:
        raise SystemExit(f"PlayMode test result is missing {name!r} attribute.")
    return int(value)

total = get_int("total")
passed = get_int("passed")
failed = get_int("failed")
skipped = get_int("skipped")
if total <= 0:
    raise SystemExit(f"PlayMode test result has no tests: {path}")
if passed <= 0:
    raise SystemExit(f"PlayMode test result has no passing tests: {path}")
if failed != 0:
    raise SystemExit(f"PlayMode test result has failures: failed={failed}")
if passed + failed + skipped > total:
    raise SystemExit(f"PlayMode test result counts are inconsistent: total={total} passed={passed} failed={failed} skipped={skipped}")

skipped_cases = [
    case for case in root.iter("test-case")
    if case.attrib.get("result") in {"Skipped", "Ignored"} or case.attrib.get("label") == "Ignored"
]
if skipped == 0:
    if skipped_cases:
        raise SystemExit("PlayMode XML contains skipped test-cases but root skipped=0.")
elif skipped == 1 and len(skipped_cases) == 1:
    skipped_name = skipped_cases[0].attrib.get("fullname") or skipped_cases[0].attrib.get("name") or ""
    if "VisualSmokeCaptureTests" not in skipped_name:
        raise SystemExit(f"Unexpected skipped PlayMode test: {skipped_name}")
else:
    raise SystemExit(f"Unexpected PlayMode skipped count: root={skipped} cases={len(skipped_cases)}")

print(f"PlayMode test summary: total={total} passed={passed} failed={failed} skipped={skipped}")
PY

echo "PlayMode test results: $RESULTS_PATH"
echo "Unity log: $LOG_PATH"
