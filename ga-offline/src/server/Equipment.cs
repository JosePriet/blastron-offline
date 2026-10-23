using System;
using System.Collections.Generic;

namespace GAOffline
{
    // Equipment (user decision 2026-10-22: "a core function"). Items, components, recipes and stats: data/equipment.json
    // (tools/equipment_to_json.py, from the archived wiki and the archived Synapse item list). As on the wiki:
    //  - each item is researched in the lab's Equipment tab (Research, id 500 + recipe id), then built in the Armory
    //    from components (Fusion; recipe id 1000 + item id);
    //  - one item per unit; attaching and removing are free; an item raises attack, defense or a skill the unit
    //    already has;
    //  - components dropped from PvP battles. PvP is locked offline, so campaign wins drop them (Drop: rates ours).
    // The client's combat code ignores equipment (the server applied it), so a battle uses a copy of an equipped
    // unit's card with the item's bonuses added (Battle.EquippedCard).
    public static class Equipment
    {
        static Dictionary<string, object> data;
        static Dictionary<string, object> Data
        {
            get { return data ?? (data = (Dictionary<string, object>)Json.Parse(GameData.Resource("equipment.json"))); }
        }

        public static List<object> Definitions() { return Json.L(Data, "equipment"); }
        public static List<object> Recipes() { return Json.L(Data, "fusion_recipes"); }

        public static Dictionary<string, object> Find(int id)
        {
            foreach (Dictionary<string, object> e in Definitions()) if (Json.I(e, "id") == id) return e;
            return null;
        }

        // save "inventory": {"<item id>": number owned, equipped ones included}
        static Dictionary<string, object> Owned(Save s)
        {
            var o = Json.O(s.D, "inventory");
            if (o == null) { o = new Dictionary<string, object>(); s.D["inventory"] = o; }
            return o;
        }

        public static int Count(Save s, int id) { return Json.I(Owned(s), id.ToString()); }

        public static void Add(Save s, int id, int n)
        {
            var o = Owned(s);
            o[id.ToString()] = Math.Max(0, Json.I(o, id.ToString()) + n);
        }

        static int Used(Save s, int id, int exceptIndex)
        {
            int n = 0;
            foreach (Dictionary<string, object> u in s.Units)
                if (Json.I(u, "equipment_id") == id && Json.I(u, "unit_index") != exceptIndex) n++;
            return n;
        }

        public static List<object> Inventory(Save s)
        {
            var list = new List<object>();
            foreach (var kv in Owned(s))
                if (Convert.ToInt32(kv.Value) > 0)
                    list.Add(new Dictionary<string, object> { { "item_id", int.Parse(kv.Key) }, { "num_owned", Convert.ToInt32(kv.Value) } });
            return list;
        }

        public static Dictionary<string, object> Equip(Save s, int index, int id)
        {
            var u = Units.Find(s, index);
            var e = Find(id);
            if (u == null) return Units.Refuse(s, "That unit is not in your inventory.");
            if (e == null || Json.I(e, "component") > 0) return Units.Refuse(s, "That equipment is invalid");
            if (Count(s, id) - Used(s, id, index) <= 0) return Units.Refuse(s, "You have none of that equipment left.");
            foreach (Dictionary<string, object> k in Json.L(e, "skill"))
                if (!Units.HasSkill(Json.I(u, "unit_id"), Json.I(u, "level", 1), Json.S(k, "id")))
                    return Units.Refuse(s, "This unit does not have the skill that equipment improves.");
            u["equipment_id"] = id;
            s.Write();
            return Units.Reply(s);
        }

        public static Dictionary<string, object> Unequip(Save s, int index)
        {
            var u = Units.Find(s, index);
            if (u == null) return Units.Refuse(s, "That unit is not in your inventory.");
            u.Remove("equipment_id");
            s.Write();
            return Units.Reply(s);
        }

        // components from a campaign win (ours; on the wiki only PvP dropped them). Rarer components open up at the
        // player levels where the wiki's recipes first need them.
        static readonly System.Random Rng = new System.Random();
        static readonly object[][] Drops = {
            //            item, player level, chance %, min, max
            new object[] { 1001, 1, 100, 2, 4 },   // Havoc Tech
            new object[] { 1002, 11, 50, 2, 4 },   // Rare Havoc Tech
            new object[] { 1021, 12, 10, 1, 1 },   // Alloy
            new object[] { 1011, 12, 10, 1, 1 },   // Polymer
            new object[] { 1003, 16, 50, 3, 6 },   // Elite Havoc Tech
            new object[] { 1022, 16, 5, 1, 1 },    // Staballoy
            new object[] { 1012, 19, 5, 1, 1 },    // Composite Polymer
            new object[] { 1031, 21, 4, 1, 1 },    // Uranium
        };

        public static List<object> Drop(Save s)
        {
            var items = new List<object>();
            foreach (var d in Drops)
            {
                if (s.Int("level") < (int)d[1] || Rng.Next(100) >= (int)d[2]) continue;
                int n = Rng.Next((int)d[3], (int)d[4] + 1);
                Add(s, (int)d[0], n);
                items.Add(new Dictionary<string, object> { { "item_id", (int)d[0] }, { "number", n } });
            }
            return items;
        }
    }
}
