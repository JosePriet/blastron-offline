using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GAOffline
{
    // Unit upgrades and salvage (upgradeUnit, rushUnitUpgrade, salvageUnit, bulkSalvage). The client reads each
    // unit's upgrade_cost, upgrade_time and salvage_value, and the running upgrade from user_tasks.
    //  - Upgrade cost and time per level step: the first two steps are the 2014 videos' (5 Scrap / 5 min, 8 Scrap /
    //    15 min, the same for every unit seen); later costs follow Tyrant Unleashed's sp_cost steps (levels.xml),
    //    later times are ours.
    //  - Salvage value: Tyrant Unleashed's salvage table by rarity and level (levels.xml); past its last level the
    //    last value.
    //  - One upgrade runs at a time; Volt Coins finish it at the client's speed-up rate (hc_ph: 6 VC per hour,
    //    rounded up, which matches every speed-up price in the videos).
    public static class Units
    {
        static readonly int[] StepCost = { 5, 8, 15, 30, 75, 150, 200, 250 };
        static readonly int[] StepTime = { 300, 900, 1800, 3600, 7200, 14400, 28800, 43200 };
        static readonly int[][] Salvage = {
            new[] { 1, 2, 5 }, new[] { 5, 10, 15, 20 }, new[] { 20, 25, 30, 40, 50, 65 },
            new[] { 40, 45, 60, 75, 100, 125 }, new[] { 80, 85, 100, 125, 175, 250 }, new[] { 120, 125, 140, 165, 225, 300 } };
        public const int VcPerHour = 6;

        class Info { public int Rarity = 1, MaxLevel = 1; }
        static Dictionary<int, Info> cards;

        static Info Card(int id)
        {
            if (cards == null)
            {
                cards = new Dictionary<int, Info>();
                foreach (Match m in Regex.Matches(GameData.Resource("cards.xml") ?? "", @"<unit>(.*?)</unit>", RegexOptions.Singleline))
                {
                    string u = m.Groups[1].Value;
                    var i = new Info();
                    i.Rarity = Num(u, "rarity", 1);
                    i.MaxLevel = 1 + Regex.Matches(u, "<upgrade>").Count;
                    cards[Num(u, "id", 0)] = i;
                }
            }
            Info r;
            return cards.TryGetValue(id, out r) ? r : new Info();
        }

        static int Num(string u, string tag, int dflt)
        {
            var m = Regex.Match(u, "<" + tag + ">(\\d+)</" + tag + ">");
            return m.Success ? int.Parse(m.Groups[1].Value) : dflt;
        }

        public static int Rarity(int cardId) { return Card(cardId).Rarity; }
        public static int MaxLevel(int cardId) { return Card(cardId).MaxLevel; }

        static int Step(int[] t, int level) { return t[Math.Max(0, Math.Min(t.Length, level) - 1)]; }

        public static int SalvageValue(int cardId, int level)
        {
            var t = Salvage[Math.Max(1, Math.Min(Salvage.Length, Rarity(cardId))) - 1];
            return Step(t, level);
        }

        // user_units entries: the save's unit plus the per-unit values the client shows
        public static List<object> Decorated(Save s)
        {
            var list = new List<object>();
            foreach (Dictionary<string, object> u in s.Units) list.Add(Decorated(u));
            return list;
        }

        public static Dictionary<string, object> Decorated(Dictionary<string, object> u)
        {
            var d = new Dictionary<string, object>(u);
            int id = Json.I(u, "unit_id"), level = Math.Max(1, Json.I(u, "level"));
            bool max = level >= MaxLevel(id);
            d["upgrade_cost"] = max ? 0 : Step(StepCost, level);
            d["upgrade_time"] = max ? 0 : Step(StepTime, level);
            d["salvage_value"] = SalvageValue(id, level);
            return d;
        }

        public static Dictionary<string, object> Find(Save s, int index)
        {
            foreach (Dictionary<string, object> u in s.Units)
                if (Json.I(u, "unit_index") == index) return u;
            return null;
        }

        // save "upgrade": {"index", "start", "end"}; finished upgrades are applied on the next message
        public static void Tick(Save s)
        {
            var up = Json.O(s.D, "upgrade");
            if (up == null || Json.I(up, "index") <= 0 || Json.N(up, "end") > Api.Now) return;
            var u = Find(s, Json.I(up, "index"));
            if (u != null) u["level"] = Math.Min(MaxLevel(Json.I(u, "unit_id")), Json.I(u, "level", 1) + 1);
            s.D.Remove("upgrade");
            s.Write();
        }

        public static Dictionary<string, object> Tasks(Save s)
        {
            var t = new Dictionary<string, object>();
            var up = Json.O(s.D, "upgrade");
            if (up != null)
            {
                t["unit_upgrade_index"] = Json.I(up, "index");
                t["unit_upgrade_start_time"] = Json.N(up, "start");
                t["unit_upgrade_end_time"] = Json.N(up, "end");
            }
            Fusion.Tasks(s, t);
            Research.Tasks(s, t);
            return t;
        }

        public static Dictionary<string, object> Upgrade(Save s, int index)
        {
            var u = Find(s, index);
            if (u == null) return Refuse(s, "That unit is not in your inventory.");
            if (Json.O(s.D, "upgrade") != null) return Refuse(s, "Another unit is already upgrading.");
            int id = Json.I(u, "unit_id"), level = Math.Max(1, Json.I(u, "level"));
            if (level >= MaxLevel(id)) return Refuse(s, "This unit is at its maximum level.");
            int cost = Step(StepCost, level);
            if (s.Int("salvage") < cost) return Refuse(s, "You do not have enough Scrap for that.");
            s.Set("salvage", s.Int("salvage") - cost);
            s.D["upgrade"] = new Dictionary<string, object> { { "index", index }, { "start", Api.Now }, { "end", Api.Now + Step(StepTime, level) } };
            s.Write();
            return Reply(s);
        }

        public static Dictionary<string, object> Rush(Save s)
        {
            var up = Json.O(s.D, "upgrade");
            if (up == null) return Refuse(s, "No unit is upgrading.");
            long left = Json.N(up, "end") - Api.Now;
            int cost = left > 0 ? (int)Math.Ceiling(left / 3600.0 * VcPerHour) : 0;
            if (s.Int("tokens") < cost) return Refuse(s, "Not enough Volt Coins.");
            s.Set("tokens", s.Int("tokens") - cost);
            up["end"] = Api.Now;
            Tick(s);
            return Reply(s);
        }

        public static Dictionary<string, object> SalvageUnit(Save s, int index)
        {
            var u = Find(s, index);
            if (u == null) return Refuse(s, "That unit is not in your inventory.");
            if (Json.I(u, "deck_id") > 0) return Refuse(s, "Remove the unit from its squad first.");
            var up = Json.O(s.D, "upgrade");
            if (up != null && Json.I(up, "index") == index) return Refuse(s, "That unit is upgrading.");
            Remove(s, u);
            s.Write();
            return Reply(s);
        }

        // tier N: every Level 1 unit of rarity N that is in no squad and is not upgrading. The client's count also skips
        // cards with a fusion_level, which the rebuilt cards do not set, so nothing else is skipped.
        public static Dictionary<string, object> BulkSalvage(Save s, int tier)
        {
            var up = Json.O(s.D, "upgrade");
            int busy = up != null ? Json.I(up, "index") : 0;
            var all = new List<object>(s.Units);
            foreach (Dictionary<string, object> u in all)
            {
                int id = Json.I(u, "unit_id");
                if (Json.I(u, "deck_id") <= 0 && Json.I(u, "level", 1) <= 1 && Rarity(id) == tier
                    && Json.I(u, "unit_index") != busy)
                    Remove(s, u);
            }
            s.Write();
            return Reply(s);
        }

        static void Remove(Save s, Dictionary<string, object> u)
        {
            s.Set("salvage", Math.Min(Api.MaxSalvage, s.Int("salvage") + SalvageValue(Json.I(u, "unit_id"), Json.I(u, "level", 1))));
            s.Units.Remove(u);
            var up = Json.O(s.D, "upgrade");
            if (up != null && Json.I(up, "index") == Json.I(u, "unit_index")) s.D.Remove("upgrade");
        }

        public static Dictionary<string, object> Reply(Save s)
        {
            var r = Api.Squads(s);
            r["user_tasks"] = Tasks(s);
            // a lab project can finish during any of these messages (Research.Tick), so its results go out with them
            r["base_upgrades"] = Research.Served(s);
            r["fusion_recipes"] = Fusion.Served(s);
            r["feature_locks"] = Api.FeatureLocks(s);
            return r;
        }

        public static Dictionary<string, object> Refuse(Save s, string message)
        {
            var r = Reply(s);
            r["result"] = false;
            r["result_message"] = message;
            return r;
        }
    }
}
