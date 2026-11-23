# Blastron offline audit

Five defects reproduced in the Android v3 source at `7f6560e`. The five regression groups now pass, and all three production C# files compile together against v3's stripped Unity assemblies. The Android implementation lives on `offline-mod`. The client references below come from the repository's decompiled 2.3.0 client on `pc-port` at `0a758b7`. [Original offline source](https://github.com/JosePriet/blastron-offline/tree/7f6560e6a9a2472c8d96d225ada91c91a47672f3/src/offline) · [Client source and assets](https://github.com/JosePriet/blastron-offline/tree/0a758b702b9840548e28e559176a674e8b9900ec/project/Assets)

## Findings, in priority order

Endpoint, Catalog and Provider below refer to `src/offline/LocalEndPoint.cs`, `OfflineCatalog.cs` and `LocalProvider.cs`. Original line numbers refer to `7f6560e`. CONFIRMED means the host check reproduced the defect or the cited source establishes the stated limitation. Device frequency is unknown.

| ID | Severity | Evidence and original location | Defect | Player impact | Fix |
|---|---|---|---|---|---|
| B01 | High | CONFIRMED · Endpoint:96,132,297 | Saving truncated the sole account file. A failed write was logged and the operation returned success. A broken save was treated as a fresh account. | A player could see a purchase or progress update succeed and lose it, or start over on the next launch. | Implemented staged writes, a commit marker, previous-account recovery and failure responses. A pending copy completes on reload. An unreadable existing account is preserved. Endpoint:82,110,181,190. |
| B02 | High | CONFIRMED · Endpoint:247 · client `KabamAPI.cs:114` | Dispatch wrote every request body to the Unity log. The login client can include account credentials and device information. | A diagnostic log shared after a login or profile update could disclose account data. The test uses a synthetic profile name. | Removed request-body logging from dispatch. Storage errors contain a fixed message. Endpoint:172,310. |
| B03 | Medium | CONFIRMED · Endpoint:339 · Provider:41 · client `WalletManager.cs:141,153,167` | Every verification credited the pack. Multiple purchases of the same product also had the same forwarded payload. | Retrying a completed verification credited extra gears. Deduplicating by product alone would prevent legitimate repeat purchases. | Generate a per-purchase receipt, forward it through the payload the client actually sends, and save it with the credit. A repeat returns the current wallet. Provider:13,53 and Endpoint:406. |
| B04 | Medium | CONFIRMED · Catalog:173,187,195,378 · Endpoint:81 · client `InventoryDB.cs:218,229` | An empty item table led to an out-of-range slice. A store request at that point could cache fallback rivet prices for the session. | A vault request could stop before its callback and leave the screen waiting. Store prices could depend on when the store first loaded. | Complete premature vault requests with a retryable nonfatal error, retain the tutorial token, and cache offers only once eligible item data exists. Catalog:178,264,315,385 and Endpoint:449. |
| B05 | Medium | CONFIRMED · Endpoint:374 · client `DataAPI.cs:16,32` and `DataManager.cs:65` | The data handler discarded the account and fragment portions of the path. Every GET returned the entire local profile. | A partial load populated unrelated progress. A request for another user could read or change the local account. The ordinary full-profile load also uses this route. | Match the offline UID, preserve the legacy full-load response, and return keyed fragments in the dictionary shape the client merges. Endpoint:459. |
| B06 | Device-dependent | SUSPECTED on ARM hardware · `docs/blastron-v3-changes.md:26` | The release notes record a crash during Unity PNG decoding under ARM translation. Translation is a suspected cause. | The recorded emulator run stopped near startup. This audit did not reproduce a physical-device crash. | Collect an ARM device trace before changing assets or native code. |
| B07 | Low for local play | CONFIRMED offline limitation · Endpoint:54,340,355,488 · client `LeaderboardManager.cs:36,78,163` | The server's opponents, leaderboard population and tournament schedule are unavailable. | Players can encounter an unavailable-service response or an empty event list. | Preserve the offline response. A local score history would be a separate feature. |

## Server calls and replacements

The patcher redirects the Sparx endpoint and billing factories. The endpoint queues requests and completes callbacks during `Update`. The account is shared by endpoints in the same process. The build bundles the three expansion content packs. The Android loader and SDK patches are independent of account handling. See `src/patcher/Program.cs:25`, `src/offline/LocalEndPoint.cs:20,63,76`, `build.sh:19,26,33`.

The client files in this table are under `project/Assets/Plugins/Assembly-CSharp-firstpass/EB/Sparx/` on the pinned client revision. A filename prefixed with **game** is under `project/Assets/Scripts/Assembly-CSharp/`. These are source call sites. A compiled SDK method alone does not establish that a particular screen invokes it.

| Calls and client evidence | Local answer and data source |
|---|---|
| `/kabam/register`, `guest`, `login`, `name`, `support` · `KabamAPI.cs:80,115,135,167,195` | Fixed offline identity, locally saved name, empty support URL. Endpoint:268,317–333. The game selects Kabam login in **game** `Setup.cs:160`. |
| `/inventory`, `/inventory/{add,use,purchase}` · `InventoryAPI.cs:72,96` | Local counts. The first inventory read returns null so the client supplies starting items. Purchases debit the saved gear balance and return the wallet with inventory. Endpoint:284,360–385 and `InventoryManager.cs:249,295`. |
| `/wallet/{balance,credit,debit}`, `/store/{payouts,offers,verify-payout}` · `WalletAPI.cs:39,75,92,131,159` | Saved gear balance, reconstructed free gear packs and rivet offers. LocalProvider returns one receipt per purchase. The game handles the soft-currency purchase flow in **game** `StoreCurrencyUI.cs:367`. Endpoint:386–427 and Catalog:343,385. |
| `/ds/{uid}`, `/ds/{uid}/{key}` · `DataAPI.cs:16,32` | Merge posted fragments into the local account. A full read returns all fragments and a keyed read returns even a single fragment wrapped under its key. An absent key returns an empty object. Endpoint:459 and `Data.cs:38`. |
| `/gacha/{getSet,getTimestamp,getTokens,addtoken,pick}` · `GachaAPI.cs:18,37,61,79,98` | Reconstructed boxes and version, saved token counts, a random catalog prize, and the applicable item or wallet update. Rivets and skill points are awarded by the game after the response. Endpoint:428,491 and **game** `GachaManager.cs:474`. |
| `/master/list`; RPCs `joinGame`, `resetServer`, `listGames`, `find`, `findOrCreate`; `/push/key-get` · `GameManagerAPI.cs:57,107,122,148,166,192,216` | One placeholder master satisfies login. RPCs complete with the offline error. Private-game key lookup falls through to the unavailable handler. Endpoint:32,54,342,488. |
| `/telemetry`, `/util/ping`, `/push/{token,apple}`, `/gamecenter/id`, `/tuning` · `TelemetryManager.cs:115`, `Hub.cs:91`, `PushAPI.cs:16,32`, `GameCenterAPI.cs:14`, `TuningManager.cs:32` | Empty acknowledgments or empty tuning list. The client uses bundled balance data. The constructor disables push and the master ping. Endpoint:32,334–353. |
| `/tournaments/{active,report}`, `/messenger/{motd,count}` · **game** `TournamentManager.cs:78,116`, `MessengerManager.cs:44,62` | No active tournament, no message of the day, count zero. A tournament report receives an empty acknowledgment. Endpoint:340,355–359. |
| `/leaderboards/{id}` scores, positions and score submissions; `/store/offerurl`; legacy `/store/verify/{platform}` · `LeaderboardManager.cs:36,78,163`, `WalletAPI.cs:57,109` | The fallback returns the offline error. Campaign and stat screens still contain score-report calls. The free local store uses `verify-payout`. Endpoint:406,488 and **game** `ChampionshipData.cs:289`, `StatManager.cs:118`. |
| `/kabam/{forgot,upgrade,facebook}`; alternate `/login/{email,new,temp,name,udid,facebook}`, Facebook lookup and editor routes · `KabamAPI.cs:97,146,179`, `LoginAPI.cs:35–114`, `FacebookManager.cs:134,156` | These SDK routes have no local identity or social backend. The fallback returns the offline error. Facebook Graph calls to `/me` and `/fql` use the separate Facebook API. Endpoint:317,488 and `FacebookManager.cs:202,251`. |
| `/content` · `ContentManager.cs:258,285,351`; vault debug routes · **game** `DebugMenu.cs:473,507,527` | The Hub's content check gets the offline error, which its callback interprets as no new content. The optional downloader constructs its own HTTP endpoint if configured. Its direct download path is outside the factory hook. The debug-only vault query routes have no local answer. The production vault uses the five gacha routes above. |

### What survives from the original catalog

**CONFIRMED.** The exported inventory contains 89 rows. Applying the offline catalog's hidden, default and price filters leaves 82. All 82 have both prices. Sorting their rivets-per-gear ratios and taking index `count / 2`, as the game patch does, gives **546.25**. The first exported boost, `Boost_2xShots_lvl1`, has quantity 5 and costs 42 in either currency. The data fields are defined in **game** `InventoryDB.cs:12–49`. The calculation is in Catalog:69,113 and the one-line data file is on the pinned client branch. [Original inventory](https://github.com/JosePriet/blastron-offline/blob/0a758b702b9840548e28e559176a674e8b9900ec/project/Assets/inventorydb.json)

**CONFIRMED.** The release APK contains six gear-pack icons and a diamond-token image. The offline implementation defines six free pack amounts, four ordinary boxes, one tutorial box, fixed tier levels and uniform draws. The diamond artwork alone does not establish an original price or prize pool. The original server-side probabilities, event schedule and prices are unavailable. See Catalog:50–67,203–247,329 and `docs/blastron-v1-changes.md:35–46`.

## Reproducing the checks

Use a .NET 8 SDK and the v3 APK. The checks call the production C# endpoint through its request queue. They use the shipped request/response types, JSON codec and field readers. The host supplies a temporary account directory, synthetic inventory, Unity logging and frame callbacks. Each group uses its own temporary account. See [the checks](checks/Program.cs) and [host adapters](checks/Host.cs).

```sh
managed=$(mktemp -d)
unzip -jq "$APK" 'assets/bin/Data/Managed/*.dll' -d "$managed"
dotnet run --project audit/checks/Checks.csproj -p:GameManaged="$managed"
```

To compare with the original production source using the same tests:

```sh
original=$(mktemp -d)
git archive 7f6560e src/offline | tar -x -C "$original"
dotnet run --project audit/checks/Checks.csproj -p:GameManaged="$managed" -p:OfflineSource="$original/src/offline"
```

| Group | Original observation | Edited output |
|---|---|---|
| Interrupted and failed saves | Previous account lost after a torn primary | `PASS interrupted and failed saves` |
| Account logging | Synthetic profile name appeared in log | `PASS account request logging` |
| Free gear verification | Same purchase credited twice | `PASS free gear verification retries` |
| Fragment routing | Keyed read returned unrelated progress | `PASS account and fragment routing` |
| Metadata availability | List range exception escaped the request | `PASS vault metadata startup and retry` |

**CONFIRMED.** The production compile also passed separately with Roslyn, language version 7.3, `-nostdlib`, and the APK's own `mscorlib`, `System`, `System.Core`, `UnityEngine`, `Assembly-CSharp-firstpass` and `Assembly-CSharp` references, as specified in `build.sh:14`. These checks exercise account files and managed callbacks on the host. Rendering, native startup, device power loss and gameplay on physical ARM hardware remain unverified.
