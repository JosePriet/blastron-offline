#!/bin/bash
# Global Assault offline smoke test.
#  Run A: fresh install -> local server answers boot; a new commander starts in the tutorial mission.
#  Run B: a save with campaign 1 finished -> boots to the base screen.
OUT=${OUT:-out}; mkdir -p $OUT/frames; PKG=com.kongregate.mobile.globalassault.google; APK=ga-offline/tests/apk/ga.apk
DATA=/sdcard/Android/data/$PKG/files/ga_offline
T0=$(date +%s); log() { echo "[$(( $(date +%s) - T0 ))s] $*" | tee -a $OUT/progress.txt; }
a() { timeout 25 adb "$@"; }
n=0
shot() { n=$((n+1)); f=$(printf "%s/%02d_%s.png" $OUT $n "$1"); timeout 30 adb exec-out screencap -p > "$f"; [ -s "$f" ] || rm -f "$f"; log "shot $1 pid=$(timeout 5 adb shell pidof $PKG)"; }
tap() { log "tap $1 $2 ($3)"; a shell input tap $1 $2; }
frames() { ( i=0; while [ ! -f $OUT/.stop ]; do timeout 10 adb exec-out screencap -p > $OUT/frames/$1$(printf %04d $i).png 2>/dev/null || true; i=$((i+1)); sleep 3; done ) & FR=$!; }
stopframes() { touch $OUT/.stop; sleep 4; kill $FR 2>/dev/null; rm -f $OUT/.stop; }
{ a shell getprop ro.build.version.release; a shell getprop ro.product.cpu.abilist; a shell wm size; } > $OUT/device.txt 2>&1
log install; timeout 300 adb install -r -g $APK > $OUT/install.txt 2>&1; cat $OUT/install.txt
grep -q Success $OUT/install.txt || { log "install FAILED"; exit 0; }
a logcat -c; adb logcat -v time > $OUT/logcat_live.txt 2>&1 & LC=$!

log "run A: fresh"; frames a
a shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null
for i in 1 2 3 4 5 6 7 8 9; do sleep 10; shot A_boot_$i; done
for i in 1 2 3 4 5 6; do tap 1140 540 A_center; sleep 5; done; shot A_after_taps
sleep 20; shot A_final
stopframes
a shell ls -la $DATA > $OUT/A_files.txt 2>&1; a shell cat $DATA/save.json > $OUT/A_save.json 2>&1

log "run B: seeded save (campaign 1 done)"
a shell am force-stop $PKG; sleep 2
a push ga-offline/tests/seed_save.json $DATA/save.json > $OUT/B_push.txt 2>&1; cat $OUT/B_push.txt
frames b
a shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null
for i in 1 2 3 4 5 6 7 8; do sleep 10; shot B_boot_$i; done
tap 1140 540 B_center; sleep 8; shot B_after_tap
a shell input keyevent KEYCODE_BACK; sleep 5; shot B_after_back
sleep 15; shot B_final
stopframes
a shell cat $DATA/save.json > $OUT/B_save.json 2>&1

kill $LC 2>/dev/null
a logcat -d -b crash > $OUT/crash.txt 2>&1
grep -E "GAOffline|Unity|server api|NullReference|Exception|Fatal|FATAL" $OUT/logcat_live.txt | head -1500 > $OUT/unity.txt
log "done"
exit 0
