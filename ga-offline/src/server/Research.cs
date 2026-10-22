using System;
using System.Collections.Generic;

namespace GAOffline
{
    // Helicarrier research in the Research Lab (buildBaseUpgrade, rushUpgrade) and the Generator's Credits
    // (collectMoney). Levels, costs, times, player level requirements and stats are the archived wiki's Research_Lab
    // tables; the Generator's bank cap and Max Credits are tripled (user decision 2026-10-22). Upgrade ids follow the
    // client's UpgradeManager (2 Generator, 4 Squad Bay, 5 Sensors); Mobile Command takes id 1. The client's other
    // helicarrier ids (3 Repair Bay, 6 Engineering) have no archived table and are not served. Category 1 is the
    // lab's "Helicarrier" tab (game assets: tabs 1 Helicarrier, 2 Armory, 4 Equipment).
    // Armory research (user decision 2026-10-22): each fusion recipe is a one-level project, id 500 + recipe id, in
    // tab 2, priced by the result unit's rarity with the wiki's Equipment Research tiers (250/300/500/750/1200
    // Credits, 1 h/6 h/12 h/2 d/4 d). The Armory opens once any of them is done.
    public static class Research
    {
        public const int MobileCommand = 1, Generator = 2, SquadBay = 4, Sensors = 5;
        const int Helicarrier = 1, Armory = 2, GeneratorRate = 20, Triple = 3;
        public const int FusionBase = 500;
        static readonly int[] TierCost = { 250, 300, 500, 750, 1200 };
        static readonly int[] TierHours = { 1, 6, 12, 48, 96 };

        // {cost, minutes, player level} for levels 2.. (level 1 is free)
        static readonly int[][] McLevels = { new[] { 150, 15, 5 }, new[] { 250, 60, 10 }, new[] { 500, 120, 15 }, new[] { 750, 240, 20 }, new[] { 1000, 480, 25 }, new[] { 1500, 720, 30 } };
        static readonly int[] McHealth = { 25, 35, 50, 65, 80, 100, 125 };
        static readonly int[][] GenLevels = { new[] { 100, 10, 1 }, new[] { 250, 60, 8 }, new[] { 350, 120, 15 }, new[] { 500, 240, 18 }, new[] { 750, 360, 30 } };
        static readonly int[] GenBank = { 100, 200, 400, 600, 800, 1000 };
        static readonly int[] GenMax = { 250, 500, 750, 1000, 1500, 2500 };
        static readonly int[][] BayLevels = { new[] { 100, 30, 5 }, new[] { 250, 60, 10 }, new[] { 750, 120, 15 }, new[] { 1200, 240, 25 } };
        static readonly int[] BayHealth = { 400, 550, 750, 1000, 1250 };
        static readonly int[][] SensorLevels = { new[] { 50, 15, 1 }, new[] { 150, 60, 4 }, new[] { 200, 120, 6 }, new[] { 350, 240, 10 }, new[] { 500, 480, 18 }, new[] { 1000, 720, 25 } };
        static readonly int[] Scans = { 3, 4, 5, 6, 7, 8, 9 };

        static List<object> defs;

        public static List<object> Definitions()
        {
            if (defs != null) return defs;
            defs = new List<object> {
                Def(MobileCommand, "Mobile Command", "Raises your base's health in battle.", 1, McLevels, (l, d) => d["commander_health"] = McHealth[l - 1]),
                Def(Generator, "Generator", "Raises the Generator's storage and your maximum Credits.", 2, GenLevels, (l, d) => {
                    d["sc_per_hour"] = GeneratorRate; d["bank_max"] = GenBank[l - 1] * Triple; d["sc_max"] = GenMax[l - 1] * Triple; }),
                Def(SquadBay, "Squad Bay", "Raises your squads' health.", 3, BayLevels, (l, d) => { d["squad_health"] = BayHealth[l - 1]; d["num_squads"] = 1; }),
                Def(Sensors, "Sensors", "Raises the number of scans.", 4, SensorLevels, (l, d) => d["num_scans"] = Scans[l - 1]),
            };
            foreach (Dictionary<string, object> f in Json.L(GameData.Rewards, "fusion_recipes"))
            {
                int card = Json.I(f, "card_id"), tier = Math.Max(1, Math.Min(TierCost.Length, Units.Rarity(card)));
                defs.Add(new Dictionary<string, object> {
                    { "id", FusionBase + Json.I(f, "id") }, { "type", Armory }, { "name", Json.S(f, "name") },
                    { "desc", "Research this fusion to build it in the Armory." }, { "order", Json.I(f, "id") }, { "card", card },
                    { "stats", new List<object> { new Dictionary<string, object> {
                        { "level", 1 }, { "sc_cost", TierCost[tier - 1] }, { "time", TierHours[tier - 1] * 3600 }, { "player_level_req", 0 } } } } });
            }
            return defs;
        }

        static Dictionary<string, object> Def(int id, string name, string desc, int order, int[][] levels, Action<int, Dictionary<string, object>> stats)
        {
            var list = new List<object>();
            for (int l = 1; l <= levels.Length + 1; l++)
            {
                var d = new Dictionary<string, object> { { "level", l }, { "sc_cost", 0 }, { "time", 0 }, { "player_level_req", 0 } };
                if (l > 1)
                {
                    d["sc_cost"] = levels[l - 2][0];
                    d["time"] = levels[l - 2][1] * 60;
                    d["player_level_req"] = levels[l - 2][2];
                }
                stats(l, d);
                list.Add(d);
            }
            return new Dictionary<string, object> { { "id", id }, { "type", Helicarrier }, { "name", name }, { "desc", desc }, { "order", order }, { "stats", list } };
        }

        static Dictionary<string, object> Find(int id)
        {
            foreach (Dictionary<string, object> d in Definitions()) if (Json.I(d, "id") == id) return d;
            return null;
        }

        static Dictionary<string, object> Stats(int id, int level)
        {
            var d = Find(id);
            if (d == null) return null;
            foreach (Dictionary<string, object> s in Json.L(d, "stats")) if (Json.I(s, "level") == level) return s;
            return null;
        }

        // save "research": {"<id>": level}; helicarrier research starts at level 1, Armory research at 0 (not done)
        public static int Level(Save s, int id)
        {
            int l = Json.I(Json.O(s.D, "research"), id.ToString());
            return l > 0 ? l : id >= FusionBase ? 0 : 1;
        }

        public static bool ArmoryOpen(Save s)
        {
            foreach (var kv in Json.O(s.D, "research") ?? new Dictionary<string, object>())
                if (int.Parse(kv.Key) >= FusionBase && Convert.ToInt32(kv.Value) > 0) return true;
            return false;
        }

        public static List<object> Served(Save s)
        {
            var list = new List<object>();
            foreach (Dictionary<string, object> d in Definitions())
                list.Add(new Dictionary<string, object> { { "upgrade_id", Json.I(d, "id") }, { "level", Level(s, Json.I(d, "id")) } });
            return list;
        }

        public static int BaseHealth(Save s) { return Json.I(Stats(MobileCommand, Level(s, MobileCommand)), "commander_health"); }
        public static int SquadHealth(Save s) { return Json.I(Stats(SquadBay, Level(s, SquadBay)), "squad_health"); }
        public static int MoneyCap(Save s) { return Json.I(Stats(Generator, Level(s, Generator)), "sc_max"); }
        static int BankCap(Save s) { return Json.I(Stats(Generator, Level(s, Generator)), "bank_max"); }

        // save "lab": {"id", "start", "end"}; finished research is applied on the next message
        public static void Tick(Save s)
        {
            var lab = Json.O(s.D, "lab");
            if (lab == null || Json.N(lab, "end") > Api.Now) return;
            var levels = Json.O(s.D, "research");
            if (levels == null) { levels = new Dictionary<string, object>(); s.D["research"] = levels; }
            int id = Json.I(lab, "id");
            Bank(s);  // the old rate and cap apply up to now
            levels[id.ToString()] = Level(s, id) + 1;
            s.D.Remove("lab");
            s.Write();
        }

        public static void Tasks(Save s, Dictionary<string, object> t)
        {
            var lab = Json.O(s.D, "lab");
            if (lab != null)
            {
                t["upgrading_id"] = Json.I(lab, "id");
                t["upgrade_start_time"] = Json.N(lab, "start");
                t["upgrade_end_time"] = Json.N(lab, "end");
            }
            var g = Gen(s);
            t["money_collect_time"] = Json.N(g, "time");
            t["money_amount"] = Json.I(g, "amount");
        }

        public static Dictionary<string, object> Start(Save s, int id)
        {
            if (Find(id) == null) return Units.Refuse(s, "That research is not available.");
            if (Json.O(s.D, "lab") != null) return Units.Refuse(s, "The Research Lab is busy.");
            var next = Stats(id, Level(s, id) + 1);
            if (next == null) return Units.Refuse(s, "That research is complete.");
            if (s.Int("level") < Json.I(next, "player_level_req")) return Units.Refuse(s, "You need a higher level for that.");
            if (s.Int("money") < Json.I(next, "sc_cost")) return Units.Refuse(s, "Not enough Credits.");
            s.Set("money", s.Int("money") - Json.I(next, "sc_cost"));
            s.D["lab"] = new Dictionary<string, object> { { "id", id }, { "start", Api.Now }, { "end", Api.Now + Json.I(next, "time") } };
            s.Write();
            return Reply(s);
        }

        public static Dictionary<string, object> Rush(Save s)
        {
            var lab = Json.O(s.D, "lab");
            if (lab == null) return Units.Refuse(s, "Nothing is being researched.");
            long left = Json.N(lab, "end") - Api.Now;
            int cost = left > 0 ? (int)Math.Ceiling(left / 3600.0 * Units.VcPerHour) : 0;
            if (s.Int("tokens") < cost) return Units.Refuse(s, "Not enough Volt Coins.");
            s.Set("tokens", s.Int("tokens") - cost);
            lab["end"] = Api.Now;
            Tick(s);
            return Reply(s);
        }

        // save "generator_bank": {"time": last collect, "amount": Credits stored then}
        static Dictionary<string, object> Gen(Save s)
        {
            var g = Json.O(s.D, "generator_bank");
            if (g == null) { g = new Dictionary<string, object> { { "time", Api.Now }, { "amount", 0 } }; s.D["generator_bank"] = g; }
            return g;
        }

        // fold the Credits produced since the last collect into the stored amount, as the client does
        static void Bank(Save s)
        {
            var g = Gen(s);
            long now = Api.Now, since = now - Json.N(g, "time");
            int made = (int)Math.Floor(since * (double)GeneratorRate / 3600.0);
            if (made <= 0) return;
            g["amount"] = Math.Min(BankCap(s), Json.I(g, "amount") + made);
            g["time"] = Json.N(g, "time") + (long)Math.Ceiling(made * 3600.0 / GeneratorRate);
            if (Json.I(g, "amount") >= BankCap(s)) g["time"] = now;
        }

        public static Dictionary<string, object> Collect(Save s)
        {
            Bank(s);
            var g = Gen(s);
            int take = Math.Max(0, Math.Min(Json.I(g, "amount"), MoneyCap(s) - s.Int("money")));
            s.Set("money", s.Int("money") + take);
            g["amount"] = Json.I(g, "amount") - take;
            g["time"] = Api.Now;
            s.Write();
            return Reply(s);
        }

        static Dictionary<string, object> Reply(Save s)
        {
            var r = Units.Reply(s);
            r["base_upgrades"] = Served(s);
            r["fusion_recipes"] = Fusion.Served(s);   // a finished Armory project marks its recipe researched
            r["feature_locks"] = Api.FeatureLocks(s); // and opens the Armory
            return r;
        }
    }
}
