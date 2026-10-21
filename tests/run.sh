#!/bin/bash
# Blastron offline smoke test: install, boot past login, open the main menu screens.
# Unity draws its own UI, so taps are by screen position (pixel_4 landscape 2280x1080).
OUT=out; mkdir -p $OUT/frames; PKG=com.kabam.blastbots; APK=tests/apks/blastron.apk
T0=$(date +%s); log() { echo "[$(( $(date +%s) - T0 ))s] $*" | tee -a $OUT/progress.txt; }
a() { timeout 25 adb "$@"; }
alive() { [ "$(timeout 5 adb get-state 2>/dev/null)" = device ]; }
n=0
shot() { n=$((n+1)); f=$(printf "%s/%02d_%s.png" $OUT $n "$1"); timeout 30 adb exec-out screencap -p > "$f"; [ -s "$f" ] || rm -f "$f"; log "shot $1 $([ -f "$f" ] && echo ok || echo FAILED) pid=$(timeout 5 adb shell pidof $PKG)"; }
tap() { log "tap $1 $2 ($3)"; a shell input tap $1 $2; }
frames() { ( i=0; while [ ! -f $OUT/.stop ]; do timeout 10 adb exec-out screencap -p > $OUT/frames/f$(printf %04d $i).png 2>/dev/null || true; i=$((i+1)); sleep 2; done ) & FR=$!; }
finish() {
  touch $OUT/.stop; sleep 3; kill $FR $LC 2>/dev/null
  find $OUT/frames -size 0 -delete
  if alive; then
    a logcat -d -b crash > $OUT/crash.txt 2>&1
    a shell run-as $PKG cat files/offline_server.json > $OUT/offline_server.json 2>/dev/null
    a shell dumpsys activity activities | grep -E "mResumedActivity|topResumedActivity" > $OUT/activity.txt
  fi
  grep -iE "FATAL|SIGSEGV|Fatal signal|text relocation|dlopen|UnsatisfiedLink|Exception|has text|W\+E|not found" $OUT/logcat_live.txt | head -400 > $OUT/errors.txt
  grep -E "LocalEndPoint|Sparx|SparxHub|Login|OBB|obb|pack|toc" $OUT/logcat_live.txt | head -600 > $OUT/sparx.txt
  log "done; errors: $(wc -l < $OUT/errors.txt) lines"
}
{ a shell getprop ro.build.version.release; a shell getprop ro.build.version.sdk; a shell getprop ro.product.cpu.abilist; a shell getprop ro.dalvik.vm.native.bridge; a shell wm size; } > $OUT/device.txt 2>&1; cat $OUT/device.txt
log install; timeout 300 adb install -r $APK > $OUT/install.txt 2>&1; cat $OUT/install.txt
a shell dumpsys package $PKG | grep -E "versionName|targetSdk|primaryCpuAbi" >> $OUT/install.txt
grep -q Success $OUT/install.txt || { log "install FAILED"; exit 0; }
a logcat -c; adb logcat -v time > $OUT/logcat_live.txt 2>&1 & LC=$!
frames
log "launch"; a shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null
for i in 1 2 3 4 5 6; do sleep 10; shot boot_$i; done
# intro story pages (first launch): tap through
for i in $(seq 1 12); do tap 1900 950 intro; sleep 3; done; shot after_intro
sleep 20; shot loading_done
tap 1140 540 start_screen; sleep 25; shot home_1
sleep 15; shot home_2
for i in 1 2 3; do a shell input keyevent KEYCODE_BACK; sleep 3; done; shot after_back
sleep 10; shot final
finish
exit 0
