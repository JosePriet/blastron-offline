using System;
using System.Collections.Generic;
using System.Reflection;
using Tyrant.Data;
using UnityEngine;

namespace GAOffline
{
    // Campaign battles. The rules are the game's own CombatSim (the client's copy of the server's combat logic,
    // which it used to check the server's turn logs): same phases, draws, delays, lanes, skills and enemy play.
    // The sim's own Run() loop has no turn limit, so the phases are driven from here with a cap.
    public class Battle
    {
        const int MaxTurns = 50;
        // Volt Coins earned offline (user decision 2026-10-22; our amounts): first win of each mission, first clear
        // of each campaign (paid through the campaign's objective, except the tutorial's).
        const int VoltFirstWin = 2, VoltCampaignClear = 10;
        // Win rewards seen in the 2014 videos: 2-3 Scrap per win; 18-34 XP per win (our flat value is 25).
        const int ScrapMin = 2, ScrapMax = 3, XpPerWin = 25;
        static readonly System.Random Rng = new System.Random();

        public static Battle Current;
        public static Battle LastFinished;

        static readonly BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        static readonly MethodInfo HandlePhase = typeof(CombatSim).GetMethod("HandlePhase", Any);
        static readonly MethodInfo TurnData = typeof(CombatSim).GetMethod("GetCurrentTurnData", Any);
        static readonly FieldInfo TurnField = typeof(CombatSim).GetField("currentTurn", Any);

        public long Id;
        public int CampaignId;
        public long MissionId;
        readonly ServiceCombatData head = new ServiceCombatData();
        readonly CombatSim sim = new CombatSim();
        readonly CombatSimDeck player = new CombatSimDeck();
        readonly CombatSimDeck enemy = new CombatSimDeck();
        bool over, won;
        public bool Won { get { return won; } }
        public int Turns { get { return (int)TurnField.GetValue(sim); } }
        Dictionary<string, object> reward;

        public static Battle Start(Save s, int campaignId)
        {
            Dictionary<string, object> camp = null;
            foreach (Dictionary<string, object> c in GameData.Campaigns)
                if (Json.I(c, "id") == campaignId) camp = c;
            if (camp == null) throw new Exception("no campaign " + campaignId);
            var st = s.Campaign(campaignId);
            var missions = Json.L(camp, "missions");
            int index = Json.I(st, "level") > 0 ? 0 : Math.Min(Json.I(st, "progress"), missions.Count - 1);
            long missionId = Convert.ToInt64(missions[index]);
            var m = GameData.Mission(missionId);

            var b = new Battle { Id = Api.Now, CampaignId = campaignId, MissionId = missionId };
            var h = b.head;
            h.BattleID = b.Id;
            h.HostID = int.Parse(Json.S(s.D, "user_id"));
            h.HostIsAttacker = false; // the enemy attacks first; the player defends (CombatSim: ENEMY_ATTACKS_FIRST)
            h.RandomSeed = b.Id;
            h.MissionID = (int)missionId;
            h.EnemyName = Json.S(m, "enemy");
            h.EnemyMaxHealth = Json.I(m, "health");
            h.PlayerMaxHealth = Research.BaseHealth(s);  // Mobile Command level (user decision 2026-10-22)
            h.AttackCommanderID = 0;
            h.DefendCommanderID = 0;

            int uid = 1;
            foreach (List<object> c in Json.L(m, "deck"))
            {
                var card = Map(uid++, Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
                if (card == null) continue;
                card.ForcedLocation = Convert.ToInt32(c[2]) > 0 ? Convert.ToInt32(c[2]) : -1;
                h.AttackDeck.Add(card);
            }
            h.EnemySize = h.AttackDeck.Count;

            var squad = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> u in s.Units)
                if (Json.I(u, "deck_id") == s.Int("active_deck")) squad.Add(u);
            squad.Sort((x, y) => Json.I(x, "slot_id").CompareTo(Json.I(y, "slot_id")));
            foreach (var u in squad)
            {
                var card = Map(uid++, EquippedCard(u), Json.I(u, "level", 1));
                if (card == null) continue;
                // the hand only lets a card be dragged when it carries the player's unit_index (UnitDragController)
                card.UnitIndex = Json.I(u, "unit_index");
                h.DefendDeck.Add(card);
            }
            h.CardMap.AddRange(h.AttackDeck);
            h.CardMap.AddRange(h.DefendDeck);

            Skills.AddTo(b.sim);
            // CombatSim.Init without its unbounded Run()
            TurnData.Invoke(b.sim, new object[] { -1 });
            TurnData.Invoke(b.sim, new object[] { 0 });
            TurnData.Invoke(b.sim, new object[] { 1 });
            TurnField.SetValue(b.sim, 1);
            var d = b.sim.GetCombatData();
            d.BattleID = h.BattleID;
            d.HostID = h.HostID;
            d.RandomSeed = h.RandomSeed;
            d.AttackDeck = h.AttackDeck;
            d.DefendDeck = h.DefendDeck;
            d.CardMap = h.CardMap;
            b.player.Init(b.sim, h.DefendCommanderID, h.PlayerMaxHealth, h.DefendDeck, false);
            b.enemy.Init(b.sim, h.AttackCommanderID, h.EnemyMaxHealth, h.AttackDeck, true);
            b.sim.SetDecks(b.player, b.enemy);
            SynapseRandom.SetSeed(h.RandomSeed);
            b.Run(s);
            Current = b;
            return b;
        }

        // the card a battle uses for this unit: its own, or a copy with the equipped item's bonuses (Equipment); copy ids
        // stay clear of the real card ids (up to 2141)
        const int CardCopyBase = 100000;

        static int EquippedCard(Dictionary<string, object> u)
        {
            int cardId = Json.I(u, "unit_id");
            var e = Equipment.Find(Json.I(u, "equipment_id"));
            Card card = Managers.Card.GetCardByID(cardId);
            if (e == null || card == null) return cardId;
            int copyId = CardCopyBase + Json.I(u, "unit_index");
            var copy = new Card {
                ID = copyId, Name = card.Name, Picture = card.Picture, BundleID = card.BundleID, Type = card.Type,
                MeshName = card.MeshName, Reserve = card.Reserve, Rarity = card.Rarity, Fusion = card.Fusion,
                CardTypes = card.CardTypes, CardSet = card.CardSet, AttackType = card.AttackType,
                IsInfantry = card.IsInfantry, IsHero = card.IsHero, maxLevel = card.maxLevel,
                angle = card.angle, scale = card.scale, stats = new List<CardLevelStats>() };
            foreach (CardLevelStats l in card.stats)
            {
                var c = new CardLevelStats { Level = l.Level, Attack = l.Attack > 0 ? l.Attack + Json.I(e, "attack") : 0, Health = l.Health + Json.I(e, "health"), Cost = l.Cost };
                foreach (CardSkill k in l.Skills)
                {
                    int bonus = 0;
                    foreach (Dictionary<string, object> b in Json.L(e, "skill")) if (Json.S(b, "id") == k.SkillID) bonus += Json.I(b, "x");
                    c.Skills.Add(new CardSkill { SkillID = k.SkillID, X = k.X + bonus, C = k.C, C0 = k.C0, S = k.S });
                }
                copy.stats.Add(c);
            }
            var dict = (Dictionary<int, Card>)typeof(CardManager).GetField("_CardDict", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Managers.Card);
            dict[copyId] = copy;
            return copyId;
        }

        static ServiceCombatCardMapData Map(int uid, int cardId, int level)
        {
            Card card = Managers.Card.GetCardByID(cardId);
            if (card == null)
            {
                Debug.LogWarning("[GAOffline] battle: no card " + cardId);
                return null;
            }
            var c = new ServiceCombatCardMapData(uid, card, level);
            CardLevelStats stats = card.getLevel(level);
            c.Attack = stats.Attack;
            c.Health = stats.Health;
            c.ForcedLocation = -1;
            return c;
        }

        public void Play(Save s, int uid, int position)
        {
            if (over) return;
            player.PlayCard(uid, position);
            Run(s);
        }

        public void Forfeit(Save s)
        {
            if (over) return;
            Finish(s, false);
        }

        void Run(Save s)
        {
            while (!over)
            {
                bool more = (bool)HandlePhase.Invoke(sim, null);
                if (enemy.IsDead() || player.IsDead()) { Finish(s, enemy.IsDead() && !player.IsDead()); return; }
                if ((int)TurnField.GetValue(sim) > MaxTurns) { Finish(s, false); return; }
                if (!more) return; // the player's play phase with cards in hand
            }
        }

        void Finish(Save s, bool victory)
        {
            over = true;
            won = victory;
            var m = GameData.Mission(MissionId);
            reward = new Dictionary<string, object> { { "user_id", Json.S(s.D, "user_id") }, { "gold", 0 }, { "sp", 0 }, { "xp", 0 } };
            if (victory)
            {
                int gold = Json.I(m, "gold");
                reward["gold"] = gold;
                int sp = Json.I(m, "sp") > 0 ? Json.I(m, "sp") : Rng.Next(ScrapMin, ScrapMax + 1);
                reward["sp"] = sp;
                reward["xp"] = XpPerWin;
                Store.AddMoney(s, gold);
                s.Set("salvage", Math.Min(Api.MaxSalvage, s.Int("salvage") + sp));
                Api.AddXp(s, XpPerWin);
                reward["items"] = Equipment.Drop(s);
                var done = Json.O(s.D, "missions");
                var md = Json.O(done, MissionId.ToString()) ?? new Dictionary<string, object>();
                if (Json.I(md, "number") == 0) s.Set("tokens", s.Int("tokens") + VoltFirstWin);
                md["number"] = Json.I(md, "number") + 1;
                md["complete"] = 1;
                done[MissionId.ToString()] = md;
                var camps = Json.O(s.D, "campaigns");
                var st = Json.O(camps, CampaignId.ToString()) ?? new Dictionary<string, object>();
                int count = 0;
                foreach (Dictionary<string, object> c in GameData.Campaigns)
                    if (Json.I(c, "id") == CampaignId) count = Json.L(c, "missions").Count;
                if (Json.I(st, "level") <= 0)
                {
                    int progress = Json.I(st, "progress") + 1;
                    st["progress"] = progress;
                    if (CampaignId == Store.TutorialCampaign && progress == 1) Store.TutorialCredits(s);
                    if (progress >= count)
                    {
                        st["level"] = 1;
                        // other campaigns pay these through their objective (Objectives)
                        if (CampaignId == Store.TutorialCampaign)
                        {
                            s.Set("tokens", s.Int("tokens") + VoltCampaignClear);
                            reward["card"] = Store.CampaignReward(s, CampaignId);
                        }
                    }
                }
                camps[CampaignId.ToString()] = st;
            }
            s.Write();
            Current = null;
            LastFinished = this;
            Debug.Log("[GAOffline] battle " + Id + " campaign " + CampaignId + " mission " + MissionId + (won ? " won" : " lost") + " at turn " + Turns);
        }

        public Dictionary<string, object> ToJson()
        {
            var h = head;
            var r = new Dictionary<string, object> {
                { "battle_id", h.BattleID }, { "host_id", h.HostID }, { "host_is_attacker", 0 },
                { "attack_commander", h.AttackCommanderID }, { "defend_commander", h.DefendCommanderID },
                { "random_seed", h.RandomSeed }, { "max_turns", MaxTurns },
                { "attack_deck", Cards(h.AttackDeck) }, { "defend_deck", Cards(h.DefendDeck) }, { "card_map", Cards(h.CardMap) },
                { "enemy_size", h.EnemySize }, { "enemy_name", h.EnemyName }, { "enemy_id", 0 },
                { "player_maxhealth", h.PlayerMaxHealth }, { "enemy_maxhealth", h.EnemyMaxHealth }, { "mission_id", h.MissionID },
            };
            var turns = new Dictionary<string, object>();
            foreach (ServiceCombatTurnData t in sim.GetCombatData().Turns) turns[t.Turn.ToString()] = Turn(t);
            r["turn"] = turns;
            if (over)
            {
                r["winner"] = won ? 1 : 0;
                r["rewards"] = new List<object> { reward };
            }
            return r;
        }

        static Dictionary<string, object> Cards(List<ServiceCombatCardMapData> cards)
        {
            var d = new Dictionary<string, object>();
            foreach (var c in cards)
            {
                var e = new Dictionary<string, object> {
                    { "unit_id", c.CardID }, { "level", c.Level }, { "health", c.Health }, { "attack", c.Attack }, { "slot", c.ForcedLocation } };
                if (c.UnitIndex > 0) e["unit_index"] = c.UnitIndex;
                d[c.CardUID.ToString()] = e;
            }
            return d;
        }

        static Dictionary<string, object> Turn(ServiceCombatTurnData t)
        {
            var draws = new List<object>();
            foreach (var x in t.Draws) draws.Add(x.CardUID);
            var plays = new List<object>();
            foreach (var x in t.Plays)
                plays.Add(new Dictionary<string, object> { { "card_uid", x.CardUID }, { "field_order", x.FieldOrder }, { "status", Statuses(x.Status) } });
            return new Dictionary<string, object> {
                { "begin", Statuses(t.BeginData) }, { "draws", draws }, { "plays", plays },
                { "upkeep", Actions(t.Upkeep.ConvertAll(x => (ServiceCombatActionData)x)) }, { "actions", Actions(t.Actions) },
                { "end", Statuses(t.EndData) } };
        }

        static List<object> Actions(List<ServiceCombatActionData> list)
        {
            var r = new List<object>();
            foreach (var a in list)
            {
                var targets = new List<object>();
                foreach (int x in a.Targets) targets.Add(x);
                var block = new List<object>();
                if (a.Block != null)
                    foreach (var x in a.Block) block.Add(new Dictionary<string, object> { { "card_uid", x.CardUID }, { "skill", x.SkillID }, { "value", x.Value } });
                r.Add(new Dictionary<string, object> {
                    { "card_uid", a.CardUID }, { "skill_id", a.SkillID }, { "targets", targets }, { "value", a.Value },
                    { "instant", a.Instant ? 1 : 0 }, { "block", block }, { "status", Statuses(a.Status) } });
            }
            return r;
        }

        static List<object> Statuses(List<ServiceCombatStatusData> list)
        {
            var r = new List<object>();
            if (list != null)
                foreach (var x in list) r.Add(new Dictionary<string, object> { { "card_uid", x.CardUID }, { "status_flag", x.StatusFlag }, { "value", x.StatusValue } });
            return r;
        }
    }
}
