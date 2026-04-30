#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj"
DATA_DIR="${1:-$ROOT_DIR/WanChaoGuiYi/Assets/Data}"
PLAYER_FACTION_ID="${2:-faction_qin_shi_huang}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet SDK is required to run the headless simulation harness." >&2
  echo "Install .NET SDK 8+, then run: tools/run_headless_simulation.sh [data-dir] [player-faction-id]" >&2
  exit 127
fi

if dotnet --list-runtimes | grep -q 'Microsoft.NETCore.App 10\.'; then
  FRAMEWORK="net10.0"
elif dotnet --list-runtimes | grep -q 'Microsoft.NETCore.App 8\.'; then
  FRAMEWORK="net8.0"
else
  echo "A .NET 8 or .NET 10 runtime is required to run the headless simulation harness." >&2
  exit 127
fi

dotnet run --project "$PROJECT" --framework "$FRAMEWORK" -- "$DATA_DIR" "$PLAYER_FACTION_ID"
