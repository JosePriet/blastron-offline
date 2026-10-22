using System;
using System.Collections.Generic;

namespace GAOffline
{
    // The Credits store and campaign unit rewards (user decision 2026-10-22). One item, the Supply Drop: 100 Credits
    // (archived wiki, "Credits" page) for one random unit from the units the wiki lists as "Open in Supply Drop", plus
    // the base 4-star units it lists in the Silver/Platinum drops. Odds are a share per star tier, split evenly within
    // the tier (our shares, 5-star tier 0.75%; the drop odds were server-side; tools/rewards_to_json.py). Finishing a campaign for the first time gives
    // the units the wiki lists as its reward. Finishing the tutorial campaign tops Credits up to one drop, as the
    // tutorial's own line says ("We've given you Credits to buy a Supply Drop").
    public static class Store
    {
        const int SupplyDropId = 1;
        const int TutorialCampaign = 1;
        static readonly Random Rng = new Random();

        static Dictionary<string, object> Drop { get { return Json.O(GameData.Rewards, "supply_drop"); } }

        public static List<object> Items()
        {
            var d = Drop;
            return new List<object> { new Dictionary<string, object> {
                { "id", SupplyDropId }, { "item_type", 1 }, { "card", 0 }, { "sc_cost", Json.I(d, "cost") }, { "hc_cost", 0 },
                { "name", Json.S(d, "name") }, { "desc", "One random unit." }, { "model", "prefab_bronze_crate" }, { "priority", 1 } } };
        }

        public static Dictionary<string, object> Buy(Save s, int itemId)
        {
            int cost = Json.I(Drop, "cost");
            if (itemId != SupplyDropId || s.Int("money") < cost)
            {
                var e = Api.Squads(s);
                e["result"] = false;
                e["result_message"] = itemId != SupplyDropId ? "That item is not available." : "Not enough Credits.";
                return e;
            }
            s.Set("money", s.Int("money") - cost);
            var unit = s.AddUnit(Roll(), 1);
            s.Write();
            var r = Api.Squads(s);
            r["store_items"] = Items();
            r["new_units"] = new List<object> { unit };
            r["new_units_mesh"] = "prefab_bronze_crate";
            return r;
        }

        static int Roll()
        {
            var pool = Json.L(Drop, "pool");
            int total = 0;
            foreach (Dictionary<string, object> p in pool) total += Json.I(p, "weight");
            int x = Rng.Next(total);
            foreach (Dictionary<string, object> p in pool)
            {
                x -= Json.I(p, "weight");
                if (x < 0) return Json.I(p, "id");
            }
            return Json.I((Dictionary<string, object>)pool[0], "id");
        }

        // Units for finishing campaign `id` the first time; returns their card ids for the battle reward.
        public static List<object> CampaignReward(Save s, int id)
        {
            var cards = new List<object>();
            var list = Json.L(Json.O(GameData.Rewards, "campaign_rewards"), id.ToString());
            if (list != null)
                foreach (object c in list)
                {
                    s.AddUnit(Convert.ToInt32(c), 1);
                    cards.Add(Convert.ToInt32(c));
                }
            if (id == TutorialCampaign && s.Int("money") < Json.I(Drop, "cost")) s.Set("money", Json.I(Drop, "cost"));
            return cards;
        }
    }
}
