using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAOffline
{
    // Answers the game's api.php messages from the local save. Field names follow the client's own response
    // mappers (InitResponse, APIResponse, ServiceUserData, ServiceCardData, ServiceDeckData, ServiceCampaignData).
    public static class Api
    {
        public const string Version = "offline-1";
        public const int MaxSalvage = 9999;
        // XP from one level to the next, levels 1-7, read off the 2014 videos (40/40/40/80/100/150/150). Past level 7
        // nothing was seen; our extension adds 50 per level.
        static readonly int[] LevelXp = { 40, 40, 40, 80, 100, 150, 150 };
        static int LevelSpan(int level) { return level <= LevelXp.Length ? LevelXp[level - 1] : 150 + 50 * (level - LevelXp.Length); }
        // save "xp" is the running total; the client shows (xp - base_xp) / (next_xp - base_xp)
        static int BaseXp(int level) { int t = 0; for (int l = 1; l < level; l++) t += LevelSpan(l); return t; }

        public static void AddXp(Save s, int n)
        {
            int xp = s.Int("xp") + n, level = Math.Max(1, s.Int("level"));
            while (xp >= BaseXp(level + 1)) level++;
            s.Set("xp", xp);
            s.Set("level", level);
        }

        public static long Now { get { return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; } }

        public static Dictionary<string, object> Base()
        {
            // hc_*: the client's speed-up price, ceil(hours left ^ hc_e * hc_ph) Volt Coins (see Units)
            return new Dictionary<string, object> { { "result", true }, { "time", Now }, { "version", Version },
                { "hc_f", "0" }, { "hc_ph", Units.VcPerHour.ToString() }, { "hc_e", "1" } };
        }

        public static string Handle(string message, Dictionary<string, string> args)
        {
            Save s = Save.Current;
            Units.Tick(s);
            Research.Tick(s);
            Dictionary<string, object> r;
            switch (message)
            {
                case "getUserAccount":
                    r = Base();
                    r["new_user"] = s.D["user_id"];
                    r["new_password"] = s.D["password"];
                    r["new_install"] = s.IsNew ? 1 : 0;
                    r["user_data"] = UserData(s);
                    break;
                case "init":
                case "update":
                case "getUserData":
                case "updateMapData":
                    r = Full(s);
                    break;
                case "setUserFlag":
                    s.Flags[Arg(args, "flag")] = (double)ArgInt(args, "value");
                    s.Write();
                    r = Base();
                    r["user_data"] = UserData(s);
                    break;
                case "setDeckUnits":
                    SetDeckUnits(s, ArgInt(args, "deck_id"), Arg(args, "units"), ArgInt(args, "activeYN") > 0);
                    r = Squads(s);
                    break;
                case "setActiveDeck":
                    s.Set("active_deck", ArgInt(args, "deck_id"));
                    s.Write();
                    r = Squads(s);
                    break;
                case "getStoreData":
                    r = Squads(s);
                    r["store_items"] = Store.Items(s);
                    break;
                case "buyStoreItem":
                    r = Store.Buy(s, ArgInt(args, "item_id"));
                    break;
                case "upgradeUnit":
                    r = Units.Upgrade(s, ArgInt(args, "unit_index"));
                    break;
                case "rushUnitUpgrade":
                    r = Units.Rush(s);
                    break;
                case "salvageUnit":
                    r = Units.SalvageUnit(s, ArgInt(args, "unit_index"));
                    break;
                case "bulkSalvage":
                    r = Units.BulkSalvage(s, ArgInt(args, "tier"));
                    break;
                case "startFusion":
                    r = Fusion.Start(s, ArgInt(args, "fusion_id"), Arg(args, "components"));
                    break;
                case "completeStoreItem":
                    r = Fusion.Complete(s);
                    break;
                case "rushFusion":
                    r = Fusion.Rush(s);
                    break;
                case "buildBaseUpgrade":
                    r = Research.Start(s, ArgInt(args, "upgrade_id"));
                    break;
                case "rushUpgrade":
                    r = Research.Rush(s);
                    break;
                case "collectMoney":
                    r = Research.Collect(s);
                    break;
                case "completeAchievement":
                    r = Objectives.Collect(s, ArgInt(args, "achievement_id"));
                    break;
                case "equipUnit":
                    r = Equipment.Equip(s, ArgInt(args, "unit_index"), ArgInt(args, "equipment_id"));
                    break;
                case "unequipUnit":
                    r = Equipment.Unequip(s, ArgInt(args, "unit_index"));
                    break;
                case "startCampaign":
                    r = BattleReply(s, Battle.Start(s, ArgInt(args, "campaign_id")));
                    break;
                case "playCard":
                    if (Battle.Current != null) Battle.Current.Play(s, ArgInt(args, "card_uid"), ArgInt(args, "field_order"));
                    r = BattleReply(s, lastBattle);
                    break;
                case "getBattleResults":
                    r = BattleReply(s, lastBattle);
                    break;
                case "forfeitBattle":
                    if (Battle.Current != null) Battle.Current.Forfeit(s);
                    r = BattleReply(s, lastBattle);
                    break;
                default:
                    Debug.Log("[GAOffline] unhandled message " + message + ", answering result:true");
                    r = Base();
                    break;
            }
            object ok;
            if (!r.TryGetValue("result", out ok) || true.Equals(ok)) Objectives.Attach(s, r);
            if (r.TryGetValue("result", out ok) && false.Equals(ok))
                Debug.Log("[GAOffline] " + message + " refused: " + Json.S(r, "result_message"));
            return Json.Write(r);
        }

        static Battle lastBattle;

        // battle_data is the whole log so far; the client replays it from the turn it has reached.
        static Dictionary<string, object> BattleReply(Save s, Battle b)
        {
            lastBattle = b;
            if (b == null)
            {
                var e = Base();
                e["result"] = false;
                e["result_message"] = "This battle has ended.";
                return e;
            }
            var r = Squads(s);
            r["battle_data"] = b.ToJson();
            r["user_inventory"] = Equipment.Inventory(s);  // components a win dropped
            return r;
        }

        static string Arg(Dictionary<string, string> args, string k)
        {
            string v;
            return args.TryGetValue(k, out v) ? v : "";
        }

        static int ArgInt(Dictionary<string, string> args, string k)
        {
            int v;
            return int.TryParse(Arg(args, k), out v) ? v : 0;
        }

        // init / update: everything InitResponse and the managers read at startup.
        static Dictionary<string, object> Full(Save s)
        {
            var r = Squads(s);
            r["user_tasks"] = Units.Tasks(s);
            r["user_items"] = Json.O(s.D, "items");
            r["mission_completions"] = Json.O(s.D, "missions");
            r["max_energy"] = 100;
            r["max_stamina"] = 10;
            r["base_upgrades"] = Research.Served(s);
            r["upgrades_data"] = Research.Definitions();
            r["news"] = new List<object>();
            r["store_items"] = Store.Items(s);
            r["fusion_recipes"] = Fusion.Served(s);
            r["equipment"] = Equipment.Definitions();
            r["user_inventory"] = Equipment.Inventory(s);
            r["is_vc_on_sale"] = 0;
            r["feature_locks"] = FeatureLocks(s);
            r["locations"] = GameData.Locations;
            r["current_campaigns"] = Campaigns(s);
            r["client_version"] = 0;
            return r;
        }

        internal static Dictionary<string, object> Squads(Save s)
        {
            var r = Base();
            r["user_data"] = UserData(s);
            var all = Units.Decorated(s);
            r["user_units"] = all;
            var decks = new List<object>();
            foreach (Dictionary<string, object> d in s.Decks)
            {
                int id = Json.I(d, "deck_id");
                var units = new List<object>();
                foreach (Dictionary<string, object> u in all)
                    if (Json.I(u, "deck_id") == id) units.Add(u);
                decks.Add(new Dictionary<string, object> {
                    { "deck_id", id }, { "name", Json.S(d, "name") }, { "commander_item_id", Json.I(d, "commander_item_id") },
                    { "power_level", "1" }, { "units", units },
                    // squad repair: battles leave no damage offline; repair_speed must be non-zero (the client divides by it)
                    { "squad_damage", 0 }, { "max_damage", Research.SquadHealth(s) }, { "repair_speed", 60 }, { "retreat_damage", 0 },
                    { "repair_start_time", 0 }, { "repair_end_time", 0 } });
            }
            r["user_decks"] = decks;
            return r;
        }

        static Dictionary<string, object> UserData(Save s)
        {
            return new Dictionary<string, object> {
                { "user_id", Json.S(s.D, "user_id") },
                { "name", Json.S(s.D, "name") },
                { "install_date", Json.S(s.D, "install_date") },
                { "flags", s.Flags },
                { "last_update_time", Now },
                { "tokens", s.Int("tokens") },
                { "money", s.Int("money") },
                { "money_cap", Research.MoneyCap(s) },
                { "energy", s.Int("energy") },
                { "energy_recharge_time", 60 },
                { "stamina", s.Int("stamina") },
                { "max_stamina", 10 },
                { "stamina_recharge_time", 300 },
                { "last_stam_update_time", Now },
                { "salvage", s.Int("salvage") },
                { "active_deck", s.Int("active_deck") },
                { "level", s.Int("level") },
                { "xp", s.Int("xp") },
                { "base_xp", BaseXp(Math.Max(1, s.Int("level"))) },
                { "next_xp", BaseXp(Math.Max(1, s.Int("level")) + 1) },
                { "caps", new Dictionary<string, object> { { "max_salvage", MaxSalvage }, { "max_cards", 500 } } },
                { "has_kong_id", 0 },
            };
        }

        // feature_lock entries from missions.xml; 1 means unlocked. PvP stays locked: there is no one to fight offline.
        internal static Dictionary<string, object> FeatureLocks(Save s)
        {
            var r = new Dictionary<string, object>();
            foreach (Dictionary<string, object> f in GameData.FeatureLocks)
            {
                string id = Json.S(f, "id");
                bool open;
                if (id == "pvp" || id == "pvp_league") open = false;
                else if (id == "armory") open = Research.ArmoryOpen(s);  // research_req 503: any Armory research (user decision)
                else if (f.ContainsKey("mission_req")) open = s.MissionComplete(Json.N(f, "mission_req"));
                else if (f.ContainsKey("level_req")) open = s.Int("level") >= Json.I(f, "level_req");
                else open = false;
                r[id] = open ? 1 : 0;
            }
            return r;
        }

        // Unlocked campaigns in missions.xml order: finished ones (level >= 1, shown as safe regions) and the ones
        // whose required campaigns are all finished (level 0, shown as mission markers). Campaigns gated on an
        // achievement (campaign_req -1) stay hidden.
        static List<object> Campaigns(Save s)
        {
            var list = new List<object>();
            foreach (Dictionary<string, object> c in GameData.Campaigns)
            {
                long id = Json.N(c, "id");
                if (Json.I(c, "achievement_req") > 0) continue;
                bool open = true;
                foreach (object req in Json.L(c, "req"))
                    if (Convert.ToInt64(req) < 0 || s.CampaignLevel(Convert.ToInt64(req)) <= 0) open = false;
                if (!open) continue;
                var missions = Json.L(c, "missions");
                var st = s.Campaign(id);
                int level = Json.I(st, "level"), progress = Json.I(st, "progress");
                var m = GameData.Mission(Convert.ToInt64(missions[Math.Min(level > 0 ? 0 : progress, missions.Count - 1)]));
                var e = new Dictionary<string, object> {
                    { "id", id }, { "campaign_name", Json.S(c, "name") }, { "name", Json.S(m, "name") }, { "desc", Json.S(m, "desc") },
                    { "location", Json.N(c, "region") }, { "level", level }, { "progress", progress }, { "max_progress", missions.Count },
                    { "mission_type", Json.I(m, "type") },
                    { "speech", new Dictionary<string, object> { { "line", Json.L(m, "speech") } } },
                    { "speech_map", new Dictionary<string, object> { { "line", Json.L(m, "speech_map") } } },
                };
                if (Json.S(m, "map") != "") { e["map"] = Json.S(m, "map"); e["map_bundle"] = 0; }
                list.Add(e);
            }
            return list;
        }

        // units = {"<slot>":"<unit_index>", ...} for one squad; units not listed leave that squad.
        static void SetDeckUnits(Save s, int deckId, string units, bool makeActive)
        {
            var slots = Json.Parse(units) as Dictionary<string, object>;
            foreach (Dictionary<string, object> u in s.Units)
                if (Json.I(u, "deck_id") == deckId) { u["deck_id"] = 0; u["slot_id"] = -1; }
            if (slots != null)
                foreach (var kv in slots)
                {
                    int index = Json.I(slots, kv.Key);
                    foreach (Dictionary<string, object> u in s.Units)
                        if (Json.I(u, "unit_index") == index) { u["deck_id"] = deckId; u["slot_id"] = int.Parse(kv.Key); }
                }
            if (makeActive) s.Set("active_deck", deckId);
            s.Write();
        }
    }
}
