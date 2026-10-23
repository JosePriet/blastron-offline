#!/bin/bash
# Install and launch ga-offline/tests/apk/release.apk on the running emulator; everything goes to $1.
OUT=${1:-out}; mkdir -p $OUT; PKG=com.kongregate.mobile.globalassault.google
adb shell getprop ro.build.version.sdk > $OUT/sdk.txt
adb shell getprop ro.product.cpu.abilist > $OUT/abilist.txt
adb install -r ga-offline/tests/apk/release.apk > $OUT/install.txt 2>&1; echo "exit $?" >> $OUT/install.txt
adb shell dumpsys package $PKG | grep -E "primaryCpuAbi|targetSdk|versionName" > $OUT/package.txt
adb logcat -c
adb shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 > $OUT/launch.txt 2>&1
for i in 1 2 3 4 5 6; do
  sleep 30
  adb shell pidof $PKG >> $OUT/pid.txt; echo "--- $((i*30))s" >> $OUT/pid.txt
  adb exec-out screencap -p > $OUT/shot_$i.png
done
# one tap in the middle to get past any dialog, then a last look
adb shell input tap 540 1200; sleep 20; adb exec-out screencap -p > $OUT/shot_7.png
adb shell pidof $PKG >> $OUT/pid.txt
adb logcat -d > $OUT/logcat.txt
adb logcat -d | grep -E "GAOffline|Unity|AndroidRuntime|FATAL|DEBUG" | tail -400 > $OUT/game.txt
adb shell run-as $PKG ls 2>/dev/null > $OUT/files.txt
adb shell ls /sdcard/Android/data/$PKG/files/ga_offline >> $OUT/files.txt 2>&1
true
