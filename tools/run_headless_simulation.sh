#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj"
DATA_DIR="${1:-$ROOT_DIR/WanChaoGuiYi/Assets/Data}"
PLAYER_FACTION_ID="${2:-faction_qin_shihuang}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet SDK is required to run the headless simulation harness." >&2
  echo "Install .NET SDK 8+, then run: tools/run_headless_simulation.sh [data-dir] [player-faction-id]" >&2
  exit 127
fi

dotnet run --project "$PROJECT" -- "$DATA_DIR" "$PLAYER_FACTION_ID"
