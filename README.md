# Blastron offline

Blastron 2.3.0 (Kabam, `com.kabam.blastbots`, Unity 4.1.5f1) patched to run without its shut-down servers.

- **Download:** see the [`releases` branch](../../tree/releases) (signed APK, OBB already included; uninstall the original Kabam app first).
- **What changed and why:** [`docs/blastron-v2-changes.md`](docs/blastron-v2-changes.md) (builds on [`v1`](docs/blastron-v1-changes.md)).
- **Rebuild:** `./build.sh blastron-2-3-0.apk main.12475.com.kabam.blastbots.obb blastron-mod.jks out.apk`

## How it works
All server traffic in the game goes through `EB.Sparx.EndPointFactory.Create`. That one method now returns
`LocalEndPoint` (`src/offline/`), which answers the game's API on the device and keeps the account in
`offline_server.json`. Google Play billing is replaced by `LocalProvider` (gear packs are free), and the vault
(gacha) tables, which only existed on the server, are rebuilt from the game's own item data (`OfflineCatalog`).
Gameplay code (`Assembly-CSharp.dll`) is untouched.

Modern Android: targetSdk 24 (Android 14/15 install block), the Unity loader stubs' text-relocation flag removed,
and the OBB packs shipped inside the APK. 64-bit-only phones (Pixel 7 and newer) can't run 32-bit Unity 4.

Not available offline: online PvP, tournaments, leaderboards, Facebook.
