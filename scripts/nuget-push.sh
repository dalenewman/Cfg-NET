#!/usr/bin/env bash
set -euo pipefail

# Push .nupkg files to NuGet.org or MyGet
# Usage: ./scripts/nuget-push.sh --source nuget|myget|<url> --api-key-env VAR_NAME [--output DIR] [--dry-run]

SOURCE=""
API_KEY_ENV=""
OUTPUT="artifacts"
DRY_RUN=false

NUGET_URL="https://api.nuget.org/v3/index.json"
MYGET_URL="https://www.myget.org/F/transformalize/api/v3/index.json"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --source)       SOURCE="$2"; shift 2 ;;
    --api-key-env)  API_KEY_ENV="$2"; shift 2 ;;
    --output)       OUTPUT="$2"; shift 2 ;;
    --dry-run)      DRY_RUN=true; shift ;;
    *)              echo "Unknown option: $1"; exit 1 ;;
  esac
done

if [[ -z "$SOURCE" ]]; then
  echo "Error: --source is required (nuget, myget, or a URL)"
  exit 1
fi

case "$SOURCE" in
  nuget)  PUSH_URL="$NUGET_URL" ;;
  myget)  PUSH_URL="$MYGET_URL" ;;
  *)      PUSH_URL="$SOURCE" ;;
esac

if [[ -z "$API_KEY_ENV" ]]; then
  echo "Error: --api-key-env is required"
  exit 1
fi

API_KEY="${!API_KEY_ENV:-}"
if [[ -z "$API_KEY" && "$DRY_RUN" == "false" ]]; then
  echo "Error: environment variable $API_KEY_ENV is not set"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

PACKAGES=("$ROOT_DIR/$OUTPUT"/*.nupkg)
if [[ ${#PACKAGES[@]} -eq 0 || ! -e "${PACKAGES[0]}" ]]; then
  echo "No .nupkg files found in $ROOT_DIR/$OUTPUT"
  exit 1
fi

echo "Pushing ${#PACKAGES[@]} package(s) to $PUSH_URL..."

for pkg in "${PACKAGES[@]}"; do
  NAME="$(basename "$pkg")"
  if $DRY_RUN; then
    echo "  [dry-run] would push: $NAME"
  else
    echo "  Pushing: $NAME"
    dotnet nuget push "$pkg" \
      --source "$PUSH_URL" \
      --api-key "$API_KEY" \
      --skip-duplicate
  fi
done

echo "Done."
