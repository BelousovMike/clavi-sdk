#!/usr/bin/env bash
# Bump package versions across the monorepo.
# Usage: ./scripts/set-version.sh 0.1.0-preview.4
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VER="${1:?version required, e.g. 0.1.0-preview.4}"

PY_VER="$VER"
if [[ "$VER" =~ ^([0-9]+\.[0-9]+\.[0-9]+)-preview\.([0-9]+)$ ]]; then
  PY_VER="${BASH_REMATCH[1]}b${BASH_REMATCH[2]}"
elif [[ "$VER" =~ ^([0-9]+\.[0-9]+\.[0-9]+)-rc\.([0-9]+)$ ]]; then
  PY_VER="${BASH_REMATCH[1]}rc${BASH_REMATCH[2]}"
fi

# .NET — Directory.Build.props <Version>
perl -i -0pe "s|<Version>[^<]+</Version>|<Version>${VER}</Version>|" \
  "$ROOT/dotnet/Directory.Build.props"

# Java — only the project version under artifactId clavi-admin-client
perl -i -0pe "s|(<artifactId>clavi-admin-client</artifactId>\s*<version>)[^<]+(</version>)|\${1}${VER}\${2}|" \
  "$ROOT/java/pom.xml"

export SET_VERSION_ROOT="$ROOT"
export SET_VERSION_VER="$VER"
export SET_VERSION_PY="$PY_VER"

python3 <<'PY'
import json
import os
import re
from pathlib import Path

root = Path(os.environ["SET_VERSION_ROOT"])
ver = os.environ["SET_VERSION_VER"]
py_ver = os.environ["SET_VERSION_PY"]

for pkg in (root / "js" / "packages").iterdir():
    p = pkg / "package.json"
    if not p.is_file():
        continue
    data = json.loads(p.read_text())
    data["version"] = ver
    deps = data.get("dependencies") or {}
    for name in list(deps):
        if name.startswith("@clavi/"):
            deps[name] = ver
    if deps:
        data["dependencies"] = deps
    p.write_text(json.dumps(data, indent=2) + "\n")

toml = root / "python" / "pyproject.toml"
text = toml.read_text()
text = re.sub(r'(?m)^version = ".*"$', f'version = "{py_ver}"', text, count=1)
toml.write_text(text)

print(f"Set version {ver} (python {py_ver})")
PY
