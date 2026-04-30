#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

python3 "$ROOT_DIR/tools/validate_data.py"
python3 "$ROOT_DIR/tools/validate_domain_core.py"
"$ROOT_DIR/tools/run_headless_simulation.sh" "$@"
