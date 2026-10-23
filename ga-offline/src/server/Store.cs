using System;
using System.Collections.Generic;

namespace GAOffline
{
    // The store and campaign unit rewards (user decisions 2026-10-22). Data: tools/rewards_to_json.py -> rewards.json.
    //  - Supply Drops: the standard drop for 100 Credits (archived wiki, "Credits" page), the Rare (45) and Elite (100)
    //    drops for Volt Coins as in the 2014 store, and Platinum from player level 10. Each gives one random unit; odds
    //    are a share per star rating, split evenly among that rating's units (our shares; the real ones were
    //    server-side).
    //  - Event crates (store item_type 2, the client's gacha popup): the archived event_box.xml contents, counts and
    //    Volt Coin prices. Each opening takes one unit out of the crate, weighted by what is left; an emptied crate
    //    is restocked.
    // Finishing a campaign for the first time gives the units the wiki lists as its reward. Winning the tutorial's
    // first battle tops Credits up to one drop, as the tutorial's own line says ("We've given you Credits to buy a
    // Supply Drop").
    public static class Store
    {
        public const int TutorialCampaign = 1;
        const int GachaType = 2;
        static readonly Random Rng = new Random();

        static List<object> Drops { get { return Json.L(GameData.Rewards, "drops"); } }
        static List<object> Crates { get { return Json.L(GameData.Rewards, "event_crates"); } }
        static Dictionary<string, object> Standard { get { return (Dictionary<string, object>)Drops[0]; } }

        public static List<object> Items(Save s)
        {
            var items = new List<object>();
            foreach (Dictionary<string, object> d in Drops)
            {
                if (Locked(s, d)) continue;
                bool credits = Json.S(d, "currency") == "sc";
                items.Add(new Dictionary<string, object> {
                    { "id", Json.I(d, "id") }, { "item_type", 1 }, { "card", 0 },
                    { "sc_cost", credits ? Json.I(d, "cost") : 0 }, { "hc_cost", credits ? 0 : Json.I(d, "cost") },
                    { "name", Json.S(d, "name") }, { "desc", "One random unit." }, { "model", Json.S(d, "model") },
                    { "priority", Json.I(d, "id") } });
            }
            long forever = Api.Now + 10L * 365 * 24 * 3600;
            foreach (Dictionary<string, object> c in Crates)
            {
                var received = Received(s, Json.I(c, "box_id"));
                var got = new Dictionary<string, object>();
                foreach (var kv in received) got[kv.Key] = kv.Value;
                items.Add(new Dictionary<string, object> {
                    { "id", Json.I(c, "box_id") }, { "item_type", GachaType }, { "name", Json.S(c, "name") },
                    { "hc_cost", Json.I(c, "cost") }, { "wb_cost", Json.I(c, "cost") },
                    { "card", Json.L(c, "cards") }, { "cards_received", got },
                    { "start_time", 0 }, { "end_time", forever }, { "model", "eventcrate" }, { "priority", 10 } });
            }
            return items;
        }

        public static Dictionary<string, object> Buy(Save s, int itemId)
        {
            Dictionary<string, object> drop = null, crate = null;
            foreach (Dictionary<string, object> d in Drops) if (Json.I(d, "id") == itemId) drop = d;
            foreach (Dictionary<string, object> c in Crates) if (Json.I(c, "box_id") == itemId) crate = c;
            if (drop != null && Locked(s, drop)) drop = null;
            if (drop == null && crate == null) return Refuse(s, "That item is not available.");
            bool credits = drop != null && Json.S(drop, "currency") == "sc";
            string wallet = credits ? "money" : "tokens";
            int cost = Json.I(drop ?? crate, "cost");
            if (s.Int(wallet) < cost) return Refuse(s, credits ? "Not enough Credits." : "Not enough Volt Coins.");
            s.Set(wallet, s.Int(wallet) - cost);
            int cardId = drop != null ? Roll(Json.L(drop, "pool")) : Draw(s, crate);
            var unit = s.AddUnit(cardId, 1);
            s.Write();
            string mesh = drop != null ? Json.S(drop, "model") : "eventcrate";
            // no "store_items" here: refreshing the open Store grid throws in the client (PagedGrid.UpdateElements),
            // which stops the reply's handling before the new-unit popup and its crate animation open. The grid
            // reloads the items each time the Store is opened.
            var r = Api.Squads(s);
            r["new_units"] = new List<object> { Units.Decorated(unit) };
            r["new_units_mesh"] = mesh;
            return r;
        }

        // the client's "Recharge the missing resources?" popup: Credits for Volt Coins at the client's own price
        // (StoreManager.getResourceCost with its built-in SC_BASE 0.2 and SC_EXP 0.75; the server never overrode them)
        public static Dictionary<string, object> BuyCredits(Save s, int amount)
        {
            if (amount <= 0) return Refuse(s, "Nothing to recharge.");
            if (amount > Research.MoneyCap(s) - s.Int("money")) return Refuse(s, "You must upgrade your Generator before you can afford that.");
            int cost = (int)Math.Ceiling((double)0.2f * Math.Pow(amount, 0.75)); // float 0.2 as the client, so whole results round alike
            if (s.Int("tokens") < cost) return Refuse(s, "Not enough Volt Coins.");
            s.Set("tokens", s.Int("tokens") - cost);
            s.Set("money", s.Int("money") + amount);
            s.Write();
            return Api.Squads(s); // no "store_items": see Buy
        }

        // higher drop tiers are offered from a player level on (rewards.json unlock_level)
        static bool Locked(Save s, Dictionary<string, object> d) { return s.Int("level") < Json.I(d, "unlock_level"); }

        static Dictionary<string, object> Refuse(Save s, string message)
        {
            var e = Api.Squads(s);
            e["result"] = false;
            e["result_message"] = message;
            return e;
        }

        static int Roll(List<object> pool)
        {
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

        // save "crates": {"<box_id>": {"<card_id>": taken}}
        static Dictionary<string, object> Received(Save s, int boxId)
        {
            var all = Json.O(s.D, "crates");
            if (all == null) { all = new Dictionary<string, object>(); s.D["crates"] = all; }
            var got = Json.O(all, boxId.ToString());
            if (got == null) { got = new Dictionary<string, object>(); all[boxId.ToString()] = got; }
            return got;
        }

        static int Draw(Save s, Dictionary<string, object> crate)
        {
            var got = Received(s, Json.I(crate, "box_id"));
            var left = new List<KeyValuePair<int, int>>();
            int total = 0;
            foreach (Dictionary<string, object> c in Json.L(crate, "cards"))
            {
                int n = Json.I(c, "count") - Json.I(got, Json.I(c, "id").ToString());
                if (n > 0) { left.Add(new KeyValuePair<int, int>(Json.I(c, "id"), n)); total += n; }
            }
            if (total == 0)
            {
                got.Clear();  // emptied: restock
                return Draw(s, crate);
            }
            int x = Rng.Next(total), id = left[0].Key;
            foreach (var kv in left)
            {
                x -= kv.Value;
                if (x < 0) { id = kv.Key; break; }
            }
            got[id.ToString()] = Json.I(got, id.ToString()) + 1;
            return id;
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
            return cards;
        }

        // The tutorial's first battle is followed by its store step ("We've given you Credits to buy a Supply Drop"):
        // winning it the first time tops Credits up to one drop.
        public static void TutorialCredits(Save s)
        {
            int cost = Json.I(Standard, "cost");
            if (s.Int("money") < cost) s.Set("money", cost);
        }

        // Credits never pass the cap (the Generator's Max Credits, see Research) and a balance above it is kept
        public static void AddMoney(Save s, int n) { int m = s.Int("money"); s.Set("money", Math.Max(m, Math.Min(Research.MoneyCap(s), m + n))); }
    }
}
