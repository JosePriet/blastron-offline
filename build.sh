#!/bin/bash
# Rebuild the offline Blastron APK from the original Kabam release.
#   ./build.sh <blastron-2-3-0.apk> <main.12475.com.kabam.blastbots.obb> <keystore.jks> [out.apk]
# Needs: dotnet SDK 8 (csc + Mono.Cecil via NuGet), python3, java, uber-apk-signer.jar in tools/.
# The keystore (alias "blastron") is not in git; ask the owner for it.
set -euo pipefail
APK=$1; OBB=$2; KS=$3; OUT=${4:-blastron-offline.apk}
HERE=$(cd "$(dirname "$0")" && pwd); W=$(mktemp -d)
unzip -q "$APK" -d "$W/apk"
mkdir -p "$W/obb" && tar xzf "$OBB" -C "$W/obb"
M=$W/apk/assets/bin/Data/Managed
CSC=$( (ls /usr/lib/dotnet/sdk/*/Roslyn/bincore/csc.dll "$HOME"/.dotnet/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null || true) | head -1)

# 1. on-device server, store and catalog (compiled against the game's own Mono 2.0 assemblies)
dotnet "$CSC" -nologo -nostdlib -noconfig -target:library -langversion:7.3 -optimize -out:"$W/SparxOffline.dll" \
  -r:$M/mscorlib.dll -r:$M/System.dll -r:$M/System.Core.dll -r:$M/UnityEngine.dll \
  -r:$M/Assembly-CSharp-firstpass.dll -r:$M/Assembly-CSharp.dll "$HERE"/src/offline/*.cs

# 2. redirect EndPointFactory/ProviderFactory, mark the bundled OBB packs mounted
dotnet build -c Release -o "$W/patcher" "$HERE/src/patcher" >/dev/null
dotnet "$W/patcher/patcher.dll" patch $M/Assembly-CSharp-firstpass.dll "$W/SparxOffline.dll" "$W/Assembly-CSharp-firstpass.dll" obb-in-apk no-dead-sdks ${PATCH_EXTRA:-}

# 3. modern Android: targetSdk 24. lib/ held Unity 4's ARMv6/v7 proxy stubs, whose private linker
#    crashes in JNI_OnLoad on current Android; ship the real ARMv7 engine libraries there instead.
python3 "$HERE/tools/axml_sdk.py" "$W/apk/AndroidManifest.xml" "$W/AndroidManifest.xml" 24
for f in libunity.so libmono.so; do python3 "$HERE/tools/elf_textrel.py" "$W/apk/assets/libs/armeabi-v7a/$f" "$W/$f"; done
python3 "$HERE/tools/elf_textrel.py" "$W/apk/lib/armeabi-v7a/libenetlib.so" "$W/libenetlib.so"

# 4. OBB packs become built-in packs
python3 -c "import json,sys;a=json.load(open(sys.argv[1]));b=json.load(open(sys.argv[2]));open(sys.argv[3],'w').write(json.dumps(a+b,separators=(',',':')))" \
  "$W/obb/packs.txt" "$W/apk/assets/packs.txt" "$W/packs.txt"

MD=assets/bin/Data/Managed; LB=lib/armeabi-v7a
python3 "$HERE/tools/repack.py" "$APK" "$W/unsigned.apk" AndroidManifest.xml="$W/AndroidManifest.xml" assets/packs.txt="$W/packs.txt" \
  $MD/Assembly-CSharp-firstpass.dll="$W/Assembly-CSharp-firstpass.dll" $MD/SparxOffline.dll="$W/SparxOffline.dll" \
  $LB/libunity.so="$W/libunity.so" $LB/libmono.so="$W/libmono.so" $LB/libenetlib.so="$W/libenetlib.so" \
  $(for p in $(python3 -c "import json;print(' '.join(json.load(open('$W/obb/packs.txt'))))"); do echo "+assets/$p=$W/obb/$p"; done)
java -jar "$HERE/tools/uber-apk-signer.jar" -a "$W/unsigned.apk" -o "$W/signed" --ks "$KS" --ksAlias blastron --ksPass android --ksKeyPass android
cp "$W"/signed/*.apk "$OUT"; echo "built $OUT"
