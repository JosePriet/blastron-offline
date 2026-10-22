using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GAOffline
{
    // Emulator test driver, compiled only into test builds (GA_TEST=1 ./build.sh). Inert unless ga_offline/autotest
    // exists; its content is the number of battles to fight. With autopilot on, it does what a player does from the
    // base: buys two Supply Drops, picks the first unfinished campaign, starts it as a map tap would, lets autopilot play the cards, and goes
    // back to the base after the result. Each step is logged with the [GAOffline] tag.
    public class Autotest : MonoBehaviour
    {
        static readonly int[] ToBuy = { 1, 1, 2, 3, 4, 304 };
        int wanted = -1, started, bought, step;
        bool off;
        float next, onBaseSince = -1, finishedAt = -1;
        Battle seen;

        // input diagnostics for the emulator's injected touches (run A's tutorial drags): where Unity sees each touch
        // begin and end, and whether the game's drag controller picked up a hand card
        void Start()
        {
            FingerGestures.OnFingerDragBegin += (finger, pos, start) =>
                Debug.Log("[GAOffline] autotest: finger drag begin at " + pos + " from " + start);
            Messenger<UnitInfoThumbnail>.AddListener("UNIT_DRAG_STARTED",
                t => Debug.Log("[GAOffline] autotest: unit drag started, unit " + t.getUnitIndex()));
        }

        void LogTouches()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Ended)
                    Debug.Log("[GAOffline] autotest: touch " + t.phase + " at " + t.position + " on " + Screen.width + "x" + Screen.height);
            }
        }

        void Update()
        {
            LogTouches();
            if (off || Time.realtimeSinceStartup < next) return;
            next = Time.realtimeSinceStartup + 2f;
            if (wanted < 0)
            {
                string f = Path.Combine(LocalServer.Dir, "autotest");
                if (!File.Exists(f)) { off = true; return; }  // driver off; touch logging stays on
                int.TryParse(File.ReadAllText(f).Trim(), out wanted);
                Debug.Log("[GAOffline] autotest: " + wanted + " battles");
            }
            if (!SynCore.AbstractManagers.IsReady) return;
            if (!Managers.Player.AutoPilotSetting) Managers.Player.AutoPilotSetting = true;
            var screen = Managers.ScreenMgr.CurrentScreen;
            if (Battle.LastFinished != null && Battle.LastFinished != seen)
            {
                seen = Battle.LastFinished;
                finishedAt = Time.realtimeSinceStartup;
            }
            if (screen == TScreen.Combat && Battle.Current == null && finishedAt > 0 && Time.realtimeSinceStartup - finishedAt > 20f)
            {
                Debug.Log("[GAOffline] autotest: back to base");
                finishedAt = -1;
                Managers.ScreenMgr.NavigateTo(TScreen.Base, false, string.Empty);
                return;
            }
            if (screen != TScreen.Base || Battle.Current != null) { onBaseSince = -1; return; }
            if (onBaseSince < 0) onBaseSince = Time.realtimeSinceStartup;
            if (Time.realtimeSinceStartup - onBaseSince < 8f) return;
            // store first, through the client's own store call: two standard drops (Credits), then one of each Volt
            // Coin tier and one event crate (the seeded save has 300 Credits and 300 Volt Coins)
            if (bought < ToBuy.Length)
            {
                var item = Managers.Store.GetStoreItem(ToBuy[bought]);
                bought++;
                if (item == null) { Debug.Log("[GAOffline] autotest: store item " + ToBuy[bought - 1] + " missing"); return; }
                Debug.Log("[GAOffline] autotest: buying " + item.Name + " (" + item.Cost + ", type " + item.ItemType + "), Credits "
                          + Managers.Player.Gold + ", Volt Coins " + Managers.Player.WarBonds);
                Managers.Store.BuyStoreItem(item);
                onBaseSince = Time.realtimeSinceStartup;
                return;
            }
            // then the unit systems and the lab, through the client's own managers: upgrade a unit and speed it up, collect
            // the Generator, research Generator 2 and recipe 47, fuse the seeded max-level Cobra (4) and Bazooka Jeep (5)
            // into an RPG Tank and speed it up, salvage a unit
            if (step < 11)
            {
                var t = Managers.Player.taskData;
                Debug.Log("[GAOffline] autotest: unit step " + step + ", Credits " + Managers.Player.Gold + "/" + Managers.Player.MaxGold
                          + ", Generator " + Managers.Upgrade.getUpgradeLevel(2) + ", recipe 47 research " + Managers.Upgrade.getUpgradeLevel(547)
                          + ", Scrap " + Managers.Player.SalvagePoints + ", Volt Coins "
                          + Managers.Player.WarBonds + ", upgrading " + (t != null ? t.UnitUpgradeIndex : -1) + ", fusing " + (t != null ? t.ForgeItem : -1)
                          + ", units " + Managers.Deck.Inventory.Count);
                switch (step++)
                {
                    case 0: Managers.Store.UpgradeCard(1); break;
                    case 1: Managers.Card.rushUpgrading(); break;
                    case 2: Managers.Upgrade.collectMoney(); break;
                    case 3: Managers.Upgrade.startUpgrade(2); break;     // Generator level 2
                    case 4: Managers.Upgrade.rushUpgrade(); break;
                    case 5: Managers.Upgrade.startUpgrade(547); break;   // Armory research for recipe 47
                    case 6: Managers.Upgrade.rushUpgrade(); break;
                    case 7: Managers.Fusion.startFusion(47, new List<int> { 4, 5 }, 0); break;
                    case 8: Managers.Store.rushForging(); break;
                    case 9: Managers.Store.SalvageCard(6); break;
                }
                onBaseSince = Time.realtimeSinceStartup;
                return;
            }
            if (started >= wanted)
            {
                if (started == wanted)
                {
                    // collect every finished objective through the client's own call
                    var list = Managers.Achievement.getAchievements();
                    if (list != null)
                        foreach (ServiceAchievementData a in list)
                        {
                            Debug.Log("[GAOffline] autotest: objective " + a.achievement_id + " " + a.name + " " + a.progress + "/" + a.max_progress);
                            if (a.isComplete()) Managers.Achievement.completeAchievement(a.achievement_id);
                        }
                    Debug.Log("[GAOffline] autotest: done, Credits " + Managers.Player.Gold + ", Volt Coins " + Managers.Player.WarBonds
                              + ", units " + Managers.Deck.Inventory.Count);
                    started++;
                }
                return;
            }
            int id = 0, location = 0;
            foreach (var c in Managers.Mission.CurrentCampaigns)
                if (c.Level <= 0) { id = c.CampaignID; location = c.Location; break; }
            if (id == 0) { Debug.Log("[GAOffline] autotest: no open campaign"); started = wanted; return; }
            started++;
            onBaseSince = -1;
            Debug.Log("[GAOffline] autotest: battle " + started + ", campaign " + id + " at location " + location);
            Managers.Mission.startCampaign(id);
            Managers.Combat.StartMission(location);
        }
    }
}
