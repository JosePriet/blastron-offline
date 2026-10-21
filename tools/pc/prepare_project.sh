#!/bin/bash
# Turns an AssetRipper export of Blastron 2.3.0 into the Unity 4.1.5 Windows-port project.
# Usage: prepare_project.sh <ExportedProject dir> <apk assets dir> <obb dir> <offline src dir>
set -euo pipefail
P=$1; APK=$2; OBB=$3; OFF=$4
HERE=$(cd "$(dirname "$0")" && pwd)
python3 -I "$HERE/pc_patch.py" "$P/Assets" "$OFF"
mkdir -p "$P/Assets/Editor" "$P/BlastronBuild/packs"
cp "$HERE/Editor/BlastronPcBuild.cs" "$P/Assets/Editor/"
python3 -I "$HERE/bundle_manifest.py" "$APK" "$OBB" "$P/Assets" "$P/BlastronBuild/bundles.json" | tail -1
# same pack order as the Android v2 build: former OBB packs first
python3 -I - "$APK/packs.txt" "$P/BlastronBuild/packs.txt" <<'PY'
import json, sys
packs = json.load(open(sys.argv[1]))
json.dump(["characters_ext", "scenes_ext", "singleplayer_ext"] + packs, open(sys.argv[2], "w"), separators=(",", ":"))
PY
# toc.txt and loose files of each pack (bundles are rebuilt in Unity)
for d in "$APK"/*/ "$OBB"/*/; do
  n=$(basename "$d"); [ -f "$d/toc.txt" ] || continue
  mkdir -p "$P/BlastronBuild/packs/$n"
  (cd "$d" && find . -type f ! -name '*.assetbundle' -print0 | xargs -0 -I{} cp --parents {} "$P/BlastronBuild/packs/$n/")
done
echo prepared
