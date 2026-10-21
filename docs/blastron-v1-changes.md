# Blastron v1 changes (offline build of Blastron 2.3.0, com.kabam.blastbots)

Base: `blastron-2-3-0.apk` + `main.12475.com.kabam.blastbots.obb` from the owner's Blastron folder.
Output: `/mnt/project-files/blastron/blastron-offline-v1.apk`. Source: `/mnt/project-files/blastron/src/`.

Directives:
- Owner: decompile and rebuild the game as-is, as an offline version of itself.
- Coordinator: change only what offline play needs, with no gameplay changes; ask before anything beyond that.
- Owner decision card: "Recreate gacha and store", meaning substitute gacha prize tables and free gold packs.
- Owner follow-up: make it work on modern Android. Not yet done; the fixes are awaiting go-ahead and are listed at the end.

Tags: [T] = asked, [X] = not asked (my addition), [F] = fix for a problem I created, [D] = docs/tooling.
Status: **built and signed, not run yet.** No emulator or device test has happened.

## APK contents
1. [T] `assets/bin/Data/Managed/Assembly-CSharp-firstpass.dll`: exactly two method bodies are replaced (Mono.Cecil, verified by a decompile diff against the original; nothing else differs).
   - `EB.Sparx.EndPointFactory.Create` now returns `new LocalEndPoint(url, options)` instead of an HTTP/WebSocket endpoint to `api.rivets.sparx.io`.
   - `EB.IAP.Internal.ProviderFactory.Create` now returns `new LocalProvider(config)` instead of Google Play billing. Needed for the free gear packs the owner chose.
2. [T] New file `assets/bin/Data/Managed/SparxOffline.dll`, compiled from `src/offline/*.cs`.
3. [T] The original signature was removed and the APK was re-signed with a new key, `blastron/keys/blastron-mod.jks` (alias `blastron`, password `android`, CN=Blastron Offline Mod). The package name, version, manifest, targetSdk (17), native libs, assets and `Assembly-CSharp.dll` are unchanged. Because the key differs, the original Kabam install must be uninstalled first.

## SparxOffline.dll: LocalEndPoint (on-device replacement for the Sparx server)
4. [T] Answers every API path the game calls, on the device. The account is stored in `<persistentDataPath>/offline_server.json`.
5. [T] Login: `/kabam/register` returns naid `1`, an empty `fbAppId` (skips the Facebook SDK init) and salt `offline`. `/kabam/guest` and `/kabam/login` return a guest user (uid 1) with the saved name. `/kabam/name` saves the chosen name. The APK-signature SHA1/HMAC the game sends is ignored.
6. [T] `/tuning` returns an empty list, so the game uses the balance values built into the APK. Any later server-side rebalancing is lost.
7. [T] Inventory (`/inventory`, `/add`, `/use`, `/purchase`) is stored locally. The first sync returns null, so the game grants its own starting inventory. A purchase debits the local gear wallet and returns "nsf" when the balance is short.
8. [T] Wallet (`/wallet/balance`, `/credit`, `/debit`) is stored locally. **Starting gear balance is 0** (the server's value is unknown).
9. [T] Save data (`/ds/...`) is stored locally (merge on POST, full read on GET).
10. [T] Stubs that report success with no content: telemetry, ping, push/apple, gamecenter id, tournament report, `/push/token`, tournament active (none), message of the day (none), message count 0, support url empty.
11. [T] `/master/list` returns one placeholder server (`ws://127.0.0.1:1`), because the game treats an empty list as a fatal login error. Matchmaking RPCs return "not connected", so online PvP shows an error instead of hanging.
12. [X] The LocalEndPoint constructor sets `Config.UsePush = false` and `GameManagerConfig.PingEnabled = false`. Without this, the push subsystem errors (fatal login failure) and the master-server UDP ping would probe the placeholder. Neither has gameplay effect.
13. [T] Unknown paths return the error `ID_SPARX_ERROR_NOT_CONNECTED` (non-fatal). This covers leaderboards, Facebook, store/offerurl, kabam forgot/upgrade/facebook and private-game lookup.
14. [F] The endpoint state is static, so endpoints created for master servers share one state and don't overwrite the save.

## SparxOffline.dll: OfflineCatalog (substitute gacha tables and store catalog). Owner chose "Recreate gacha and store".
15. [T] Four base vault boxes, using the names and tokens the client already references: `LootinatorTier0..3` with `token_bronze/silver/gold/platinum` (indices 0-3 match `DailyOffer.SpinTokenType`). There is also one `tutorial` group box with `token_tutorial`. No `token_diamond` box, since there was no data for it.
16. [T] Prize pools come from the game's own tables (InventoryDB weapons and boosts, CharacterDB head/torso/leg parts). Items marked hidden, default, or free are excluded. Bronze: items with levelRequired ≤ 5, plus rivets. Silver: levelRequired ≤ 12, parts with unlockLevel ≤ 12, plus rivets. Gold: all items, all parts, rivets and 1 skill point. Platinum: the top half of items by cost, all parts, rivets and 1 skill point. Tutorial: the bronze item pool.
17. [X] INVENTED: tier unlock levels 1/5/10/15, and the level cut-offs of 5 and 12 in item 16.
18. [T] Prices are derived from game data. Bronze, silver and gold cost rivets equal to the average rivet price of the items they can drop (rounded to 50). Platinum costs gears equal to the average gear price of its items. Each box can also be opened with 1 token of its type. The rivet prize equals the box price; for platinum it is gears × rivets-per-gear.
19. [X] INVENTED: prizes are picked uniformly at random, the multiplier is 1, opening a box gives 0 XP, and there are no attract images. A weapon prize gives its store quantity, and a part gives 1.
20. [T] Tokens are tracked locally (`/gacha/getTokens`, `/gacha/addtoken`, and a pick with payment "token" spends 1). New accounts start with 1 `token_tutorial`; I assumed the old server granted this for the vault tutorial.
21. [T] Store: six gear packs (one per `UI_currency_hard_1..6` icon) are shown as "FREE". Buying one goes through LocalProvider into the wallet's normal verify step, and `/store/verify-payout` credits the gears.
22. [X] INVENTED: gear pack sizes 25/60/130/280/600/1300.
23. [T] Rivet packs (bought with gears, via the game's existing `PurchaseSoftWithHard`) cost 10/25/50/100 gears. Rivets per gear is the median soft/hard price ratio of items priced in both currencies, falling back to 100.
24. [X] INVENTED: the rivet-pack gear prices and bonuses of 1.0/1.1/1.2/1.3.

## SparxOffline.dll: LocalProvider
25. [T] Marks every offer valid, labels gear packs "FREE", and completes purchases immediately through the wallet's verify step. No Google Play billing connection is made.

## Not changed / still offline-only
26. Online PvP, tournaments, leaderboards, Facebook, Game Center, push messages and the Chartboost ads SDK are untouched. PvP and leaderboards fail with "not connected"; the others are silent.
27. The OBB must still be placed at `Android/obb/com.kabam.blastbots/main.12475.com.kabam.blastbots.obb`.

## Pending, awaiting owner go-ahead (modern Android). NOT in v1.
- Fix 2: targetSdk 17 → 24 (Android 14 blocks < 23, Android 15 blocks < 24), plus patching the ELF headers of `libunity.so`, `libmono.so` and `libenetlib.so` (remove DT_TEXTREL, make the code segment RWX), because Android refuses text relocations at targetSdk ≥ 23.
- Fix 3: bundle the OBB inside the APK.
- Can't be fixed: 64-bit-only devices can't run Unity 4 ARMv7 code.

## Tooling [D]
- `src/patcher/Program.cs`: Mono.Cecil patcher (redirects the two factory methods).
- `src/repack.py`: rebuilds the APK, keeping each entry's compression and dropping old signatures. Signing used uber-apk-signer 1.3.0 (zipalign + v1/v2/v3).
- Compiled with Roslyn csc against the game's own Unity 4 Mono 2.0 mscorlib (`-nostdlib`).
