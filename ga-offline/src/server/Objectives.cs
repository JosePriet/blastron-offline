using System;
using System.Collections.Generic;

namespace GAOffline
{
    // Objectives (the client's achievements: user_achievements, completed_achievements, completeAchievement). The
    // definitions were server-side and are not archived; these are the ones the 2014 videos show:
    //  - one story objective (type 10) per unlocked campaign, "Campaign completed.", progress in missions; its reward
    //    is the campaign's unit reward (the video's Hard Shell objective) plus our 10 Volt Coins per campaign, which
    //    move here from the campaign clear itself;
    //  - "Helicarrier Unleashed" (type 3): Generator to level 2 for 100 Credits, Sensors to level 2 for 5 Volt Coins;
    //  - "Fuse Hyperion Darius" (type 8): the Hyperion Darius fusion for 5 Volt Coins.
    // Left out: "Kongregate to Victory" (needs a Kongregate account) and the timed "Arclight" event chain (its
    // missions are not in the archive).
    public static class Objectives
    {
        const int Story = 10, Research = 3, Fusion = 8, CampaignBase = 1000, CampaignVolt = 10;

        class Def
        {
            public int Id, Type, Progress, Max, Credits, Volt, Upgrade, FusionId, Campaign;
            public string Name, Desc, Portrait;
            public List<object> Units = new List<object>();
        }

        static List<Def> All(Save s)
        {
            var list = new List<Def>();
            foreach (Dictionary<string, object> c in GameData.Campaigns)
            {
                int id = Json.I(c, "id");
                if (!Unlocked(s, c) || id == Store.TutorialCampaign) continue;
                var st = s.Campaign(id);
                var d = new Def { Id = CampaignBase + id, Type = Story, Name = Json.S(c, "name"), Desc = "Campaign completed.", Portrait = "darius",
                                  Max = Json.L(c, "missions").Count, Volt = CampaignVolt, Campaign = id };
                d.Progress = Json.I(st, "level") > 0 ? d.Max : Math.Min(d.Max, Json.I(st, "progress"));
                var units = Json.L(Json.O(GameData.Rewards, "campaign_rewards"), id.ToString());
                if (units != null) d.Units = units;
                list.Add(d);
            }
            list.Add(new Def { Id = 2001, Type = Research, Name = "Helicarrier Unleashed", Desc = "Research the Generator to Level 2.", Portrait = "elia",
                               Max = 2, Progress = Math.Min(2, GAOffline.Research.Level(s, GAOffline.Research.Generator)), Credits = 100, Upgrade = GAOffline.Research.Generator });
            list.Add(new Def { Id = 2002, Type = Research, Name = "Helicarrier Unleashed", Desc = "Research the Sensors to Level 2.", Portrait = "elia",
                               Max = 2, Progress = Math.Min(2, GAOffline.Research.Level(s, GAOffline.Research.Sensors)), Volt = 5, Upgrade = GAOffline.Research.Sensors });
            int hd = 0;
            foreach (Dictionary<string, object> f in Json.L(GameData.Rewards, "fusion_recipes"))
                if (Json.S(f, "name") == "Hyperion Darius") hd = Json.I(f, "id");
            if (hd > 0)
                list.Add(new Def { Id = 3001, Type = Fusion, Name = "Fuse Hyperion Darius", Desc = "Complete the Hyperion Darius Fusion project in the Armory.",
                                   Portrait = "darius", Max = 1, Progress = Json.I(Json.O(s.D, "fused"), hd.ToString()) > 0 ? 1 : 0, Volt = 5, FusionId = hd });
            return list;
        }

        static bool Unlocked(Save s, Dictionary<string, object> c)
        {
            if (Json.I(c, "achievement_req") > 0) return false;
            foreach (object req in Json.L(c, "req"))
                if (Convert.ToInt64(req) < 0 || s.CampaignLevel(Convert.ToInt64(req)) <= 0) return false;
            return true;
        }

        // save "objectives": {"<id>": 1 announced complete, 2 collected}
        static Dictionary<string, object> State(Save s)
        {
            var o = Json.O(s.D, "objectives");
            if (o == null) { o = new Dictionary<string, object>(); s.D["objectives"] = o; }
            return o;
        }

        static Dictionary<string, object> ToJson(Def d)
        {
            var reward = new Dictionary<string, object> { { "sc", d.Credits }, { "hc", d.Volt } };
            if (d.Units.Count > 0) reward["unit"] = new Dictionary<string, object> { { "unit_id", Convert.ToInt32(d.Units[0]) }, { "level", 1 } };
            var req = new Dictionary<string, object>();
            if (d.Upgrade > 0) req["base_upgrade"] = new Dictionary<string, object> { { "upgrade", d.Upgrade } };
            if (d.FusionId > 0) req["fusion"] = new Dictionary<string, object> { { "id", d.FusionId } };
            if (d.Campaign > 0) req["campaign"] = new Dictionary<string, object> { { "id", d.Campaign } };
            return new Dictionary<string, object> {
                { "achievement_id", d.Id }, { "type", d.Type }, { "name", d.Name }, { "description", d.Desc }, { "portrait", d.Portrait },
                { "progress", d.Progress }, { "max_progress", d.Max }, { "reward", reward }, { "req", req } };
        }

        // adds user_achievements, and completed_achievements for objectives that became complete since the last reply
        public static void Attach(Save s, Dictionary<string, object> r)
        {
            var st = State(s);
            var open = new List<object>();
            var done = new List<object>();
            foreach (var d in All(s))
            {
                int state = Json.I(st, d.Id.ToString());
                if (state >= 2) continue;
                open.Add(ToJson(d));
                if (d.Progress >= d.Max && state == 0) { st[d.Id.ToString()] = 1; done.Add(ToJson(d)); }
            }
            r["user_achievements"] = open;
            if (done.Count > 0) { r["completed_achievements"] = done; s.Write(); }
        }

        public static Dictionary<string, object> Collect(Save s, int id)
        {
            Def d = null;
            foreach (var x in All(s)) if (x.Id == id) d = x;
            var st = State(s);
            if (d == null || Json.I(st, id.ToString()) >= 2) return Units.Refuse(s, "That objective is not available.");
            if (d.Progress < d.Max) return Units.Refuse(s, "That objective is not complete yet.");
            st[id.ToString()] = 2;
            Store.AddMoney(s, d.Credits);
            s.Set("tokens", s.Int("tokens") + d.Volt);
            var units = new List<object>();
            foreach (object c in d.Units) units.Add(Units.Decorated(s.AddUnit(Convert.ToInt32(c), 1)));
            s.Write();
            var r = Units.Reply(s);
            if (units.Count > 0) r["new_units"] = units;
            return r;
        }

        // record a finished fusion for "Fuse Hyperion Darius"; save "fused": {"<recipe id>": count}
        public static void Fused(Save s, int recipe)
        {
            var f = Json.O(s.D, "fused");
            if (f == null) { f = new Dictionary<string, object>(); s.D["fused"] = f; }
            f[recipe.ToString()] = Json.I(f, recipe.ToString()) + 1;
        }
    }
}
