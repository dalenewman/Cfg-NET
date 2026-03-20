#!/usr/bin/env bash
set -euo pipefail

# Pack all packable SDK-style projects under src/
# Usage: ./scripts/nuget-pack.sh [--id-prefix PREFIX] [--configuration CONFIG] [--output DIR] [--dry-run]

ID_PREFIX="Cfg"
CONFIGURATION="Release"
OUTPUT="artifacts"
DRY_RUN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --id-prefix)      ID_PREFIX="$2"; shift 2 ;;
    --configuration)  CONFIGURATION="$2"; shift 2 ;;
    --output)         OUTPUT="$2"; shift 2 ;;
    --dry-run)        DRY_RUN=true; shift ;;
    *)                echo "Unknown option: $1"; exit 1 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

mkdir -p "$ROOT_DIR/$OUTPUT"

echo "Discovering packable projects (prefix: $ID_PREFIX)..."

FOUND=0
while IFS= read -r csproj; do
  # Skip test projects
  if grep -q '<IsPackable>false</IsPackable>' "$csproj" 2>/dev/null; then
    continue
  fi

  # Check if it matches the prefix
  PACKAGE_ID=$(sed -n 's/.*<PackageId>\([^<]*\)<\/PackageId>.*/\1/p' "$csproj" 2>/dev/null | head -1)
  if [[ -z "$PACKAGE_ID" ]]; then
    continue
  fi
  if [[ -n "$ID_PREFIX" && "$PACKAGE_ID" != "$ID_PREFIX"* ]]; then
    continue
  fi

  FOUND=$((FOUND + 1))
  if $DRY_RUN; then
    echo "  [dry-run] would pack: $PACKAGE_ID ($csproj)"
  else
    echo "  Packing: $PACKAGE_ID"
    dotnet pack "$csproj" \
      --configuration "$CONFIGURATION" \
      --output "$ROOT_DIR/$OUTPUT" \
      -p:ContinuousIntegrationBuild=true \
      --verbosity quiet
  fi
done < <(find "$ROOT_DIR/src" -name '*.csproj' -type f | sort)

echo "Done. $FOUND package(s) found."
