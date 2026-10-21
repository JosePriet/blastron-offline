# Blastron v3 changes (v2 + crash fixes from emulator testing)

Output: `/mnt/project-files/blastron/blastron-offline-v3.apk`. Source: repo JosePriet/blastron-offline, branch `offline-mod` (`build.sh`, `src/patcher/Program.cs`).
Everything in v1 and v2 still applies. This file lists only what v3 adds or changes.

Directives:
- Owner, 02:47Z: "2 if it doesn't work then go ahead with a patch that fixes it."
- Coordinator: offline-only changes, no gameplay changes; log every change.

Tags: [T] = asked, [X] = not asked, [F] = fix for a problem found in testing, [D] = tooling.
Status: built and signed. Emulator result is listed at the end.

## Evidence (GitHub Actions emulator, Android 11 x86/x86_64 with ARM translation)
- v2: the install succeeds, then the app crashes immediately (SIGSEGV in `JNI_OnLoad`) inside Unity 4's ARMv6/v7 "proxy" loader stubs.
- v3 with only fix 1: the engine starts and the intro story plays. About 1 s after the MobileAppTracker plugin initialised and Chartboost got an HTTP 404, the app crashed with a native SIGSEGV. I could not tell from the logs which of these three caused it: MobileAppTracker, Chartboost, or the APK signature check that runs next.

## Changes
1. [F] `lib/armeabi-v7a/libunity.so` and `libmono.so` are now the real ARMv7 engine libraries from `assets/libs/armeabi-v7a/`, not Unity's ~50 KB proxy stubs, whose private linker crashes on current Android. The v2 ELF patch is applied to them (DT_TEXTREL→DT_DEBUG, code segment RWX). The copies in `assets/libs/` are left in place, unchanged. The stubs from v2 are no longer shipped.
2. [F][X] `Assembly-CSharp-firstpass.dll`: the `EB.Sparx.Protect.CalculateSha1` and `CalculateHmac` bodies are replaced with `cb("offline"); return <empty enumerator>;`. The native `libapkprotect` APK-signature check no longer runs. Its result was only sent to the dead login server, and the local server already ignored it.
3. [F][X] `Assembly-CSharp-firstpass.dll`: `ChartboostPlugin.Start` now sets `appId = null` first. It still sets `Instance` (the loading screen waits for it) but never starts an ad session. `CacheInterstitial`, `ShowInterstitial`, `ShowMoreApps` and `CacheMoreApps` are empty. Chartboost's ad server answers 404.
4. [F][X] `Assembly-CSharp-firstpass.dll`: `MobileAppTrackerPlugin.Start` is empty. It was an install-tracking SDK; on Android 11 it throws a SecurityException reading the device ID. `SetUserId` is already guarded by `_initialized`, so it is now a no-op.
5. [D] `build.sh`: a new patcher step `no-dead-sdks`. The csc lookup no longer fails under `set -e` when `~/.dotnet` is missing.

Items 2-4 are tagged [X]: they were not explicitly requested, but they only remove code that contacted dead services. There is no gameplay effect. The game no longer shows Chartboost ads.

## Emulator result (runs 4-7, Android 11 with ARM translation)
- Fixes 2-4 did not stop the crash (run 4).
- Test-only builds forwarded the game log to logcat and traced the content loader (patcher modes `diag-log` and `diag-trace`, never shipped). The crash happens when Unity decodes a PNG loaded through WWW: first the splash logo, then the `badges/hof_level_0` badge once the logo load was skipped (run 7). JPGs, `packs.txt`, every TOC and all asset bundles load correctly. On x86_64, run 7 reached bundle extraction with no crash before the test window ended.
- Most likely cause: the emulator's ARM translator (libndk_translation) on Unity's PNG decoder. It is unconfirmed until a run on a real ARM device; I asked the owner to test on the S22.
- Fixes 2-4 are kept anyway (no gameplay effect, and they remove calls to dead services).
6. [D] `src/patcher`: test-only modes `diag-log`, `diag-trace` (via `PATCH_EXTRA` in build.sh) and `empty-enum`. The shipped v3 does not use them.
