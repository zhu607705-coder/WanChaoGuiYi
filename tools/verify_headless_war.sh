#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

python3 "$ROOT_DIR/tools/validate_data.py"
python3 "$ROOT_DIR/tools/validate_domain_core.py"
"$ROOT_DIR/tools/run_headless_simulation.sh" "$@"
python3 - "$ROOT_DIR/tools/headless_runner/latest-war-report.json" <<'PY'
import json
import sys

path = sys.argv[1]
with open(path, "r", encoding="utf-8") as handle:
    report = json.load(handle)

if report.get("runName") != "headless_war_four_scenarios":
    raise SystemExit("Unexpected runName in " + path)
if report.get("passed") is not True:
    raise SystemExit("Headless war report did not pass")
if report.get("scenarioCount") != 4:
    raise SystemExit("Expected scenarioCount=4 in " + path)
if report.get("passedCount") != 4 or report.get("failedCount") != 0:
    raise SystemExit("Expected all four scenarios to pass in " + path)
PY
