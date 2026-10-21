# Blastron v2 changes (v1 plus modern-Android fixes)

Output: `/mnt/project-files/blastron/blastron-offline-v2.apk` (92 MB, includes the OBB data). Source: `/mnt/project-files/blastron/src/`.
Everything in `blastron-v1-changes.md` still applies, unchanged. This file lists only what v2 adds.

Directives for v2:
- Owner, 02:40Z: "make sure the version of blastron I sent works with modern android versions."
- Owner, 02:47Z: "2 if it doesn't work then go ahead with a patch that fixes it. 3, ship the obb with the app."
- Coordinator: report incompatibilities and the minimal fix before changing; log every change.

Tags: [T] = asked, [X] = not asked, [F] = fix, [D] = tooling.
Status: **built and signed, not run yet.** No emulator or device test has happened.

## Fix 2: install and load on Android 14/15+ (owner approved 02:47Z)
1. [T] `AndroidManifest.xml`: in the binary XML, only the `android:targetSdkVersion` value changes, 17 → 24. Android 14 blocks < 23 and Android 15 blocks < 24. Decoded with apktool, the manifest is otherwise identical to the original. minSdk stays 10.
2. [T] `lib/armeabi-v7a/libunity.so`, `libmono.so`, `libenetlib.so`: in each, the DT_TEXTREL dynamic entry is retagged DT_DEBUG and the executable PT_LOAD segment is marked RWX. Android refuses text relocations when targetSdk ≥ 23; W+X segments are allowed below targetSdk 26. No code bytes change. `libunity.so`/`libmono.so` here are Unity's small loader stubs; the real engine libraries in `assets/libs/armeabi-v7a/` are loaded by the stub's own linker and are unchanged.
3. [X] Side effects of targetSdk 24 that I accepted without changing anything: storage and phone-state permissions are no longer auto-granted. Unity then saves to app-internal storage, and its device-ID read is already wrapped in try/catch. A v1 save does not carry over to v2.

## Fix 3: ship the OBB inside the APK (owner approved 02:47Z)
4. [T] The OBB's three content packs (`characters_ext`, `scenes_ext`, `singleplayer_ext`, 53 MB) are added unmodified as `assets/<pack>/...`, stored uncompressed.
5. [T] `assets/packs.txt`: the three packs are prepended, in the same order the OBB mount used, so they load as built-in packs ahead of the original seven.
6. [T] `Assembly-CSharp-firstpass.dll`: a third method body changes. `EB.Sparx.ObbManager.Check()` is now `IsMounted = true; return null;`, so the loading screen no longer looks for or downloads an external OBB. Verified by a decompile diff: only `EndPointFactory.Create`, `ProviderFactory.Create` and `ObbManager.Check` differ from the original.
7. [T] The separate `.obb` file is no longer needed.

## Still not fixable
8. 64-bit-only devices (for example Pixel 7 and newer) can't run 32-bit ARM Unity 4 code. Samsung S22 can.

## Tooling [D]
- `src/axml_sdk.py`: patches targetSdkVersion in a binary manifest.
- `src/elf_textrel.py`: the ELF patch from item 2.
- `src/repack.py`: now also adds whole directories, uncompressed.
- `src/patcher/Program.cs`: adds the `obb-in-apk` step.
