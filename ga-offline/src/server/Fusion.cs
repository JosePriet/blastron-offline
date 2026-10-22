using System;
using System.Collections.Generic;

namespace GAOffline
{
    // Unit fusion (startFusion, completeStoreItem, rushFusion). Recipes: rewards.json "fusion_recipes", from the
    // archived wiki's "Fuse X and Y" lines (tools/rewards_to_json.py). As in the 2014 videos, the parts must be at
    // their maximum level, they are used up when the fusion starts, and the new unit (level 1) arrives when the timer
    // ends. One fusion runs at a time. A recipe must be researched first (Research, user decision 2026-10-22).
    public static class Fusion
    {
        static List<object> Recipes { get { return Json.L(GameData.Rewards, "fusion_recipes"); } }

        // each recipe is researched in the lab's Armory tab first (Research, id 500 + recipe id)
        public static List<object> Served(Save s)
        {
            var list = new List<object>();
            foreach (Dictionary<string, object> f in Recipes)
            {
                var d = new Dictionary<string, object>(f);
                int research = Research.FusionBase + Json.I(f, "id");
                d["upgrade_id"] = research;
                d["researched"] = Research.Level(s, research) > 0 ? 1 : 0;
                list.Add(d);
            }
            return list;
        }

        static Dictionary<string, object> Recipe(int id)
        {
            foreach (Dictionary<string, object> f in Recipes) if (Json.I(f, "id") == id) return f;
            return null;
        }

        // save "forge": {"id", "start", "end"}
        public static void Tasks(Save s, Dictionary<string, object> t)
        {
            var fo = Json.O(s.D, "forge");
            if (fo == null) return;
            t["forging_item_id"] = Json.I(fo, "id");
            t["forging_start_time"] = Json.N(fo, "start");
            t["forging_end_time"] = Json.N(fo, "end");
        }

        // components: "[unit_index,unit_index]"
        public static Dictionary<string, object> Start(Save s, int fusionId, string components)
        {
            var f = Recipe(fusionId);
            if (f == null) return Units.Refuse(s, "That fusion is not available.");
            if (Json.O(s.D, "forge") != null) return Units.Refuse(s, "Another fusion is already running.");
            if (Research.Level(s, Research.FusionBase + fusionId) <= 0) return Units.Refuse(s, "Research this fusion first.");
            var need = new List<int>();
            foreach (Dictionary<string, object> r in Json.L(f, "resource"))
                for (int i = 0; i < Math.Max(1, Json.I(r, "number")); i++) need.Add(Json.I(r, "card_id"));
            var parts = new List<Dictionary<string, object>>();
            var busy = Json.O(s.D, "upgrade");
            foreach (object o in Json.Parse(components) as List<object> ?? new List<object>())
            {
                var u = Units.Find(s, Convert.ToInt32(o));
                if (u == null || parts.Contains(u)) return Units.Refuse(s, "That unit is not in your inventory.");
                if (busy != null && Json.I(busy, "index") == Json.I(u, "unit_index")) return Units.Refuse(s, "That unit is upgrading.");
                int id = Json.I(u, "unit_id");
                if (Json.I(u, "level", 1) < Units.MaxLevel(id)) return Units.Refuse(s, "Both units must be at their maximum level.");
                if (!need.Remove(id)) return Units.Refuse(s, "Those units do not make this fusion.");
                parts.Add(u);
            }
            if (need.Count > 0) return Units.Refuse(s, "Those units do not make this fusion.");
            foreach (var u in parts) s.Units.Remove(u);
            s.D["forge"] = new Dictionary<string, object> { { "id", fusionId }, { "start", Api.Now }, { "end", Api.Now + Json.I(f, "time") } };
            s.Write();
            return Units.Reply(s);
        }

        // the client asks once the timer has run out
        public static Dictionary<string, object> Complete(Save s)
        {
            var fo = Json.O(s.D, "forge");
            if (fo == null || Json.N(fo, "end") > Api.Now) return Units.Reply(s);
            var f = Recipe(Json.I(fo, "id"));
            s.D.Remove("forge");
            if (f != null) Objectives.Fused(s, Json.I(f, "id"));
            var unit = f != null ? s.AddUnit(Json.I(f, "card_id"), 1) : null;
            s.Write();
            var r = Units.Reply(s);
            r["store_items"] = Store.Items(s);
            if (unit != null) r["new_units"] = new List<object> { Units.Decorated(unit) };
            return r;
        }

        public static Dictionary<string, object> Rush(Save s)
        {
            var fo = Json.O(s.D, "forge");
            if (fo == null) return Units.Refuse(s, "No fusion is running.");
            long left = Json.N(fo, "end") - Api.Now;
            int cost = left > 0 ? (int)Math.Ceiling(left / 3600.0 * Units.VcPerHour) : 0;
            if (s.Int("tokens") < cost) return Units.Refuse(s, "Not enough Volt Coins.");
            s.Set("tokens", s.Int("tokens") - cost);
            fo["end"] = Api.Now;
            return Complete(s);
        }
    }
}
