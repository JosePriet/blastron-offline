using System.IO;
using UnityEngine;

namespace GAOffline
{
    // Emulator test driver, compiled only into test builds (GA_TEST=1 ./build.sh). Inert unless ga_offline/autotest
    // exists; its content is the number of battles to fight. With autopilot on, it does what a player does from the
    // base: picks the first unfinished campaign, starts it as a map tap would, lets autopilot play the cards, and goes
    // back to the base after the result. Each step is logged with the [GAOffline] tag.
    public class Autotest : MonoBehaviour
    {
        int wanted = -1, started;
        float next, onBaseSince = -1, finishedAt = -1;
        Battle seen;

        void Update()
        {
            if (Time.realtimeSinceStartup < next) return;
            next = Time.realtimeSinceStartup + 2f;
            if (wanted < 0)
            {
                string f = Path.Combine(LocalServer.Dir, "autotest");
                if (!File.Exists(f)) { enabled = false; return; }
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
            if (started >= wanted)
            {
                if (started == wanted) { Debug.Log("[GAOffline] autotest: done"); started++; }
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
