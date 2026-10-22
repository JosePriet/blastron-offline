using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GAOffline
{
    // The player's profile, formerly kept by the server. Stored as ga_offline/save.json under persistentDataPath.
    public class Save
    {
        public Dictionary<string, object> D;
        public bool IsNew;

        static Save current;

        static string PathName { get { return Path.Combine(LocalServer.Dir, "save.json"); } }

        public static Save Current
        {
            get
            {
                if (current == null) current = Load();
                return current;
            }
        }

        static Save Load()
        {
            try
            {
                if (File.Exists(PathName))
                {
                    var d = Json.Parse(File.ReadAllText(PathName, Encoding.UTF8)) as Dictionary<string, object>;
                    if (d != null) return new Save { D = d };
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[GAOffline] save unreadable, starting a new one: " + e.Message);
                try { File.Copy(PathName, PathName + ".bad", true); } catch (Exception) { }
            }
            var s = new Save { D = NewProfile(), IsNew = true };
            s.Write();
            return s;
        }

        public void Write()
        {
            string tmp = PathName + ".tmp";
            File.WriteAllText(tmp, Json.Write(D), new UTF8Encoding(false));
            if (File.Exists(PathName)) File.Delete(PathName);
            File.Move(tmp, PathName);
        }

        // New commander. The first mission (the tutorial) plays the Cobra (card 2), Darius (502) and the Javelin (10):
        // tutorial1.xml points at exactly those three cards in the opening hand, so they are the starting squad.
        static Dictionary<string, object> NewProfile()
        {
            var units = new List<object>();
            int[] starter = { 2, 502, 10 };
            for (int i = 0; i < starter.Length; i++)
                units.Add(new Dictionary<string, object> {
                    { "unit_index", i + 1 }, { "unit_id", starter[i] }, { "level", 1 }, { "sp", 0 }, { "deck_id", 1 }, { "slot_id", i } });
            return new Dictionary<string, object> {
                { "user_id", "1" },
                { "password", "offline" },
                { "name", "Commander" },
                { "install_date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                { "money", 0 }, { "tokens", 40 }, { "salvage", 0 },  // 40 Volt Coins at the start, as in the 2014 videos
                { "level", 1 }, { "xp", 0 },
                { "energy", 100 }, { "stamina", 10 },
                { "flags", new Dictionary<string, object> { { "music", 1 }, { "sound", 1 }, { "tutorial", 0 }, { "autopilot", 0 }, { "speed", 1 } } },
                { "active_deck", 1 },
                { "next_unit_index", starter.Length + 1 },
                { "units", units },
                { "decks", new List<object> { new Dictionary<string, object> { { "deck_id", 1 }, { "name", "Squad 1" }, { "commander_item_id", 0 } } } },
                { "items", new Dictionary<string, object>() },
                { "campaigns", new Dictionary<string, object>() },
                { "missions", new Dictionary<string, object>() },
            };
        }

        public int Int(string k) { return Json.I(D, k); }

        // A new unit in the inventory (no squad); the entry is what user_units / new_units carry.
        public Dictionary<string, object> AddUnit(int cardId, int level)
        {
            int index = Int("next_unit_index");
            Set("next_unit_index", index + 1);
            var u = new Dictionary<string, object> {
                { "unit_index", index }, { "unit_id", cardId }, { "level", level }, { "sp", 0 }, { "deck_id", 0 }, { "slot_id", -1 } };
            Units.Add(u);
            return u;
        }

        public void Set(string k, object v) { D[k] = v; }

        public List<object> Units { get { return Json.L(D, "units"); } }

        public List<object> Decks { get { return Json.L(D, "decks"); } }

        public Dictionary<string, object> Flags { get { return Json.O(D, "flags"); } }

        public Dictionary<string, object> Campaign(long id)
        {
            var all = Json.O(D, "campaigns");
            return Json.O(all, id.ToString());
        }

        public int CampaignLevel(long id) { return Json.I(Campaign(id), "level"); }

        public bool MissionComplete(long id) { return Json.I(Json.O(Json.O(D, "missions"), id.ToString()), "complete") > 0; }
    }
}
