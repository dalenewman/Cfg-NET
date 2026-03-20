#!/usr/bin/env bash
set -euo pipefail

# Pack all packable SDK-style projects under src/
# Usage: ./scripts/nuget-pack.sh [--configuration CONFIG] [--output DIR] [--dry-run]

CONFIGURATION="Release"
OUTPUT="artifacts"
DRY_RUN=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --configuration)  CONFIGURATION="$2"; shift 2 ;;
    --output)         OUTPUT="$2"; shift 2 ;;
    --dry-run)        DRY_RUN=true; shift ;;
    *)                echo "Unknown option: $1"; exit 1 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

mkdir -p "$ROOT_DIR/$OUTPUT"

echo "Discovering packable projects..."

FOUND=0
while IFS= read -r csproj; do
  # Only pack projects with IsPackable=true
  if ! grep -q '<IsPackable>true</IsPackable>' "$csproj" 2>/dev/null; then
    continue
  fi

  PACKAGE_ID=$(sed -n 's/.*<PackageId>\([^<]*\)<\/PackageId>.*/\1/p' "$csproj" 2>/dev/null | head -1)
  if [[ -z "$PACKAGE_ID" ]]; then
    PACKAGE_ID="$(basename "$csproj" .csproj)"
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
