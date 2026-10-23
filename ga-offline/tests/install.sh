#!/bin/bash
# Install and launch Global Assault builds on the running emulator; everything goes to $1.
#   A: release.apk (targetSdk 23), installed as a user would (storage permission not granted)
#   D: release.apk forced to the ARM libraries (--abi armeabi-v7a, run by the image's ARM translator), as on a phone
#   C: release-v1.apk (targetSdk 21, the first release), to tell an SDK-target problem from an emulator one
# A run that stays black gets a native stack dump (debuggerd) and a Java one (SIGQUIT).
OUT=${1:-out}; mkdir -p $OUT; PKG=com.kongregate.mobile.globalassault.google
adb shell getprop ro.build.version.sdk > $OUT/sdk.txt
adb shell getprop ro.product.cpu.abilist > $OUT/abilist.txt
adb root >/dev/null 2>&1; sleep 5; adb wait-for-device
run() { # name apk install-flags
  local N=$1 D=$OUT/$1; mkdir -p $D
  adb uninstall $PKG >/dev/null 2>&1
  adb install $3 $2 > $D/install.txt 2>&1; echo "exit $?" >> $D/install.txt
  adb shell dumpsys package $PKG | grep -E "primaryCpuAbi|targetSdk|versionName|WRITE_EXTERNAL" > $D/package.txt
  adb logcat -c
  adb shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 > $D/launch.txt 2>&1
  # a targetSdk < 23 app first shows the permission review screen: press Continue, then dismiss any old-app warning
  if [ "$N" = C ]; then sleep 8; adb exec-out screencap -p > $D/shot_0.png; adb shell input tap 976 2203; sleep 4; adb shell input keyevent 66; fi
  for i in 1 2 3 4; do
    sleep 25
    adb shell pidof $PKG >> $D/pid.txt; echo "--- $((i*25))s" >> $D/pid.txt
    adb exec-out screencap -p > $D/shot_$i.png
  done
  local P=$(adb shell pidof $PKG | tr -d '\r')
  if [ -n "$P" ]; then
    adb shell debuggerd -b $P > $D/native_stacks.txt 2>&1
    adb shell kill -3 $P; sleep 3
    adb shell 'cat /data/anr/$(ls -t /data/anr | head -1)' > $D/java_stacks.txt 2>&1
  fi
  adb logcat -d > $D/logcat.txt
  adb logcat -d | grep -E "GAOffline|Unity|Mono|mono|AndroidRuntime|FATAL|DEBUG|linker|dlopen" | tail -400 > $D/game.txt
  adb shell ls -lR /data/data/$PKG /sdcard/Android/data/$PKG > $D/files.txt 2>&1
  adb shell am force-stop $PKG
}
run A ga-offline/tests/apk/release.apk ""
run D ga-offline/tests/apk/release.apk "--abi armeabi-v7a"
[ -f ga-offline/tests/apk/release-v1.apk ] && run C ga-offline/tests/apk/release-v1.apk ""
true
