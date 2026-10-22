#!/bin/bash
# Build the offline Global Assault APK from the original 1.30.1 release.
#   ./build.sh <Global+Assault_1.30.1.apk> <keystore.jks> [out.apk]
# Inputs that are not in git: the original APK, and the keystore (alias "blastron", reused from the Blastron revival).
# Data: data/missions.json (tools/missions_to_json.py from the archived missions.xml), data/cards.xml
# (ga-archive/rebuild/gen_cards.py), data/tutorial1.xml (archived).
# GA_TEST=1 adds the emulator test driver (src/test). Needs: dotnet SDK 8 (csc + Mono.Cecil via NuGet), python3, java.
set -euo pipefail
APK=$1; KS=$2; OUT=${3:-global-assault-offline.apk}
HERE=$(cd "$(dirname "$0")" && pwd); W=$(mktemp -d)
unzip -q "$APK" 'assets/bin/Data/Managed/*' -d "$W/apk"
M=$W/apk/assets/bin/Data/Managed
CSC=$( (ls /usr/lib/dotnet/sdk/*/Roslyn/bincore/csc.dll "$HOME"/.dotnet/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null || true) | head -1)

# 1. local server, compiled against the game's own Mono 2.0 assemblies, with the game data embedded
dotnet "$CSC" -nologo -nostdlib -noconfig -target:library -langversion:7.3 -optimize -out:"$W/GAOffline.dll" \
  -r:$M/mscorlib.dll -r:$M/System.dll -r:$M/System.Core.dll -r:$M/UnityEngine.dll \
  -r:$M/Assembly-CSharp-firstpass.dll -r:$M/Assembly-CSharp.dll \
  -resource:"$HERE/data/missions.json",missions.json -resource:"$HERE/data/cards.xml",cards.xml \
  -resource:"$HERE/data/tutorial1.xml",tutorial1.xml -resource:"$HERE/data/rewards.json",rewards.json "$HERE"/src/server/*.cs ${GA_TEST:+"$HERE"/src/test/*.cs}

# 2. route WWWManager.Begin through the local server
dotnet build -c Release -o "$W/patcher" "$HERE/src/patcher" >/dev/null
dotnet "$W/patcher/patcher.dll" $M/Assembly-CSharp.dll "$W/GAOffline.dll" "$W/Assembly-CSharp.dll"

MD=assets/bin/Data/Managed
python3 "$HERE/tools/repack.py" "$APK" "$W/unsigned.apk" $MD/Assembly-CSharp.dll="$W/Assembly-CSharp.dll" $MD/GAOffline.dll="$W/GAOffline.dll"
java -jar "$HERE/tools/uber-apk-signer.jar" -a "$W/unsigned.apk" -o "$W/signed" --ks "$KS" --ksAlias blastron --ksPass android --ksKeyPass android >/dev/null
cp "$W"/signed/*.apk "$OUT"; echo "built $OUT"
