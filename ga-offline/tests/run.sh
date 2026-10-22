#!/bin/bash
# Global Assault offline smoke test (API 19 x86 emulator, Nexus 5, landscape 1920x1080). Android 4.4 runs apps on
# Dalvik: on ART (5.0+) x86 images the game's Mono (Unity 4.6.3) hangs in a fault-handler loop on its first null check.
#  Run O: the original 1.30.1 APK, as a baseline (no server: expected to stall or show a connection error).
#  Run A: the offline APK, fresh install: the local server answers boot; a new commander starts in the tutorial mission.
#  Run B: the offline APK with a save that has campaign 1 finished: boots to the base screen.
OUT=${OUT:-out}; mkdir -p $OUT/frames; PKG=com.kongregate.mobile.globalassault.google
T=ga-offline/tests; DATA=/sdcard/Android/data/$PKG/files/ga_offline
T0=$(date +%s); log() { echo "[$(( $(date +%s) - T0 ))s] $*" | tee -a $OUT/progress.txt; }
a() { timeout 25 adb "$@"; }
# screenshots via a file: adb exec-out needs Android 5.0+
cap() { local t=/sdcard/cap_$RANDOM.png; timeout 20 adb shell screencap -p $t >/dev/null 2>&1 && timeout 20 adb pull $t "$1" >/dev/null 2>&1; timeout 10 adb shell rm -f $t >/dev/null 2>&1; }
SDK=$(timeout 10 adb shell getprop ro.build.version.sdk | tr -d '\r'); G=; [ "${SDK:-0}" -ge 23 ] && G=-g
gpid() { timeout 5 adb shell ps 2>/dev/null | tr -d '\r' | awk -v p=$PKG '$NF==p {print $2}' | head -1; }
n=0
shot() { n=$((n+1)); f=$(printf "%s/%02d_%s.png" $OUT $n "$1"); cap "$f"; [ -s "$f" ] || rm -f "$f"; log "shot $1 pid=$(gpid)"; }
tap() { log "tap $1 $2 ($3)"; a shell input tap $1 $2; }
# Android's "built for an older version of Android" warning (targetSdk 21) sits over the game until OK is pressed.
dismiss() {
  for i in 1 2 3; do
    a shell uiautomator dump /sdcard/ui.xml >/dev/null 2>&1; a pull /sdcard/ui.xml $OUT/ui.xml >/dev/null 2>&1
    xy=$(python3 $T/tap.py $OUT/ui.xml '^OK\b|android:id/button1' 2>/dev/null)
    if [ -n "$xy" ]; then tap $xy sdk_warning_ok; return; fi
    sleep 3
  done
  log "no sdk warning found"
}
frames() { ( i=0; while [ ! -f $OUT/.stop ]; do cap $OUT/frames/$1$(printf %04d $i).png || true; i=$((i+1)); sleep 3; done ) & FR=$!; }
stopframes() { touch $OUT/.stop; sleep 4; kill $FR 2>/dev/null; rm -f $OUT/.stop; }
gamelog() { pid=$(gpid); a logcat -d -v time > $OUT/$1_logcat.txt 2>&1; grep -E "\( *$pid\)|Unity|GAOffline" $OUT/$1_logcat.txt | grep -v RcsClient | head -3000 > $OUT/$1_game.txt; log "$1: $(wc -l < $OUT/$1_game.txt) game log lines"; }
launch() { a logcat -c; a shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null; sleep 4; dismiss; }
{ a shell getprop ro.build.version.release; a shell getprop ro.product.cpu.abilist; a shell wm size; } > $OUT/device.txt 2>&1

# let the freshly booted system settle (run 4: system_server restarted during run O)
sleep 60
adb root >/dev/null 2>&1; sleep 5; adb wait-for-device
log "run O: original APK"
timeout 300 adb install -r $G ${ABI:+--abi $ABI} $T/apk/original.apk > $OUT/O_install.txt 2>&1; cat $OUT/O_install.txt
frames o; launch
for i in 1 2 3 4 5 6; do sleep 10; shot O_$i; done
a shell debuggerd -b $(gpid) > $OUT/O_stacks.txt 2>&1
stopframes; gamelog O
a uninstall $PKG >/dev/null 2>&1

log "run A: offline APK, fresh"
timeout 300 adb install -r $G ${ABI:+--abi $ABI} $T/apk/ga.apk > $OUT/A_install.txt 2>&1; cat $OUT/A_install.txt
grep -q Success $OUT/A_install.txt || { log "install FAILED"; exit 0; }
a shell dumpsys package $PKG | grep -E "primaryCpuAbi|targetSdk" > $OUT/A_abi.txt
frames a; launch
for i in 1 2 3 4 5 6 7 8 9; do sleep 10; shot A_boot_$i; done
for i in 1 2 3 4 5 6; do tap 1140 540 A_center; sleep 5; done; shot A_after_taps
sleep 20; shot A_final
a shell debuggerd -b $(gpid) > $OUT/A_stacks.txt 2>&1
a shell ls -laR /data/data/$PKG > $OUT/A_internal.txt 2>&1; a shell cat /data/data/$PKG/files/ga_offline/log.txt > $OUT/A_trace_internal.txt 2>&1
stopframes; gamelog A
a shell ls -laR /sdcard/Android/data/$PKG > $OUT/A_files.txt 2>&1; a shell cat $DATA/log.txt > $OUT/A_trace.txt 2>&1; a shell cat $DATA/save.json > $OUT/A_save.json 2>&1

log "run B: seeded save (campaign 1 done)"
a shell am force-stop $PKG; sleep 2
a shell mkdir -p $DATA; a push $T/seed_save.json $DATA/save.json > $OUT/B_push.txt 2>&1; cat $OUT/B_push.txt
frames b; launch
for i in 1 2 3 4 5 6 7 8; do sleep 10; shot B_boot_$i; done
tap 1140 540 B_center; sleep 8; shot B_after_tap
sleep 15; shot B_final
stopframes; gamelog B
a shell cat $DATA/save.json > $OUT/B_save.json 2>&1; a shell cat $DATA/log.txt > $OUT/B_trace.txt 2>&1
a logcat -d -b crash > $OUT/crash.txt 2>&1
log "done"
exit 0
