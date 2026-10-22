using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace GAOffline
{
    // Data embedded in GAOffline.dll at build time:
    //   missions.json  converted from the archived missions.xml (tools/missions_to_json.py)
    //   cards.xml      rebuilt unit file (ga-archive/rebuild/gen_cards.py)
    //   tutorial1.xml  archived tutorial script
    //   rewards.json   Supply Drop pool and campaign unit rewards (tools/rewards_to_json.py, from the archived wiki)
    public static class GameData
    {
        static Dictionary<string, object> missions;
        static Dictionary<string, Dictionary<string, object>> missionById;

        public static string Resource(string name)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (s == null) return null;
                using (var r = new StreamReader(s, Encoding.UTF8)) return r.ReadToEnd();
            }
        }

        static Dictionary<string, object> Missions
        {
            get
            {
                if (missions == null)
                {
                    missions = (Dictionary<string, object>)Json.Parse(Resource("missions.json"));
                    missionById = new Dictionary<string, Dictionary<string, object>>();
                    foreach (var kv in Json.O(missions, "missions")) missionById[kv.Key] = (Dictionary<string, object>)kv.Value;
                }
                return missions;
            }
        }

        public static List<object> Locations { get { return Json.L(Missions, "locations"); } }

        public static List<object> Campaigns { get { return Json.L(Missions, "campaigns"); } }

        public static List<object> FeatureLocks { get { return Json.L(Missions, "feature_locks"); } }

        static Dictionary<string, object> rewards;

        public static Dictionary<string, object> Rewards
        {
            get { return rewards ?? (rewards = (Dictionary<string, object>)Json.Parse(Resource("rewards.json"))); }
        }

        public static Dictionary<string, object> Mission(long id)
        {
            var m = Missions;
            Dictionary<string, object> r;
            return missionById.TryGetValue(id.ToString(), out r) ? r : null;
        }
    }
}
