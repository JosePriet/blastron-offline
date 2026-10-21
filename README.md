# Blastron offline: Windows port (branch `pc-port`)

The offline Blastron 2.3.0, rebuilt as a Windows game from the Android release. The gameplay, the on-device server
(`project/Assets/Scripts/Assembly-CSharp/Offline/`) and the free store are the same as the Android build on branch `offline-mod`.

## Status
The project compiles outside Unity. It has not been opened in the Unity editor or built into an .exe yet.

## Build (Windows)
1. Install **Unity 4.1.5f1** (Unity download archive) and activate it with a free Unity account.
2. Open `project/` in Unity 4.1.5. The first import takes a while.
3. Run the menu **Blastron > Build Windows (packs + player)**. In batch mode:
   `Unity.exe -batchmode -quit -projectPath <repo>\project -executeMethod BlastronPcBuild.BuildAll -logFile build.log`
4. The output goes to `BlastronPC/` next to `project/`: `Blastron.exe`, `Blastron_Data/` and `android/` (the game's content packs; the folder name is what the game's loader expects).

## How the project was made
1. AssetRipper (1.x) exported `blastron-2-3-0.apk` plus the OBB packs as a Unity 4.1.5f1 project.
2. `tools/pc/prepare_project.sh <ExportedProject> <apk assets dir> <obb dir> <offline src>` then:
   - `pc_patch.py` fixes 3 decompiler outputs that don't compile and adds the offline server. It turns the Android-only
     plugins into no-ops on PC: ads, install tracking, OBB download, APK signature, web views, device locale and ID.
     The native Android name keyboard is replaced by a text box.
   - It ports the 8 GLES-only shaders AssetRipper couldn't decompile (`tools/pc/shaders/`) from the original GLSL to Cg.
   - `bundle_manifest.py` maps every asset in the original asset bundles to its exported file (`project/BlastronBuild/bundles.json`).
     Unity's build script (`project/Assets/Editor/BlastronPcBuild.cs`) uses this map to rebuild each bundle for Windows.
3. `compile_check.sh` compiles the scripts with Roslyn against .NET 2.0/3.5 reference assemblies to catch errors without Unity.
