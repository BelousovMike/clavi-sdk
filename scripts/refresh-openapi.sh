#!/usr/bin/env bash
# Pull live OpenAPI from a running Clavi stack into openapi/openapi.v1.json
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
HOST="${CLAVI_E2E_HOST:-clavi.clavi.local}"
OUT="${ROOT}/openapi/openapi.v1.json"
curl -skf -H "Host: ${HOST}" "https://${HOST}/swagger/v1/swagger.json" -o "${OUT}"
echo "Wrote ${OUT} ($(wc -c <"${OUT}") bytes)"
