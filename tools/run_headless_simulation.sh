#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT_DIR/tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj"
DATA_DIR="${1:-$ROOT_DIR/My project/Assets/Data}"
PLAYER_FACTION_ID="${2:-faction_qin_shi_huang}"

DOTNET_BIN="${DOTNET_BIN:-}"
if [[ -z "$DOTNET_BIN" ]]; then
  if command -v dotnet >/dev/null 2>&1; then
    DOTNET_BIN="dotnet"
  elif [[ -x "/c/Program Files/dotnet/dotnet.exe" ]]; then
    DOTNET_BIN="/c/Program Files/dotnet/dotnet.exe"
  elif [[ -x "/c/Program Files (x86)/dotnet/dotnet.exe" ]]; then
    DOTNET_BIN="/c/Program Files (x86)/dotnet/dotnet.exe"
  else
    echo "dotnet SDK is required to run the headless simulation harness." >&2
    echo "Install .NET SDK 8+, then run: tools/run_headless_simulation.sh [data-dir] [player-faction-id]" >&2
    exit 127
  fi
fi

if "$DOTNET_BIN" --list-runtimes | grep -q 'Microsoft.NETCore.App 10\.'; then
  FRAMEWORK="net10.0"
elif "$DOTNET_BIN" --list-runtimes | grep -q 'Microsoft.NETCore.App 8\.'; then
  FRAMEWORK="net8.0"
else
  echo "A .NET 8 or .NET 10 runtime is required to run the headless simulation harness." >&2
  exit 127
fi

"$DOTNET_BIN" run --project "$PROJECT" --framework "$FRAMEWORK" -- "$DATA_DIR" "$PLAYER_FACTION_ID"
