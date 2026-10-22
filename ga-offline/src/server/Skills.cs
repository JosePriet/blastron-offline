using System.Collections.Generic;
using System.Reflection;
using Tyrant.Data;

namespace GAOffline
{
    // The three unit skills in the rebuilt cards.xml that the game's CombatSim has no rules for. Their effects follow the
    // in-game descriptions quoted on the wiki; the client already has renderers for all three action types.
    public static class Skills
    {
        public static void AddTo(CombatSim sim)
        {
            var map = (Dictionary<string, CombatSimSkill>)typeof(CombatSimSkillFactory)
                .GetField("skillMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sim.GetSkillFactory());
            map["flank"] = new Flank();
            map["laser"] = new Laser();
            map["shrapnel"] = new Shrapnel();
        }

        static int Lane(int position) { return position % 4; }

        // Damage of one of the actor's attacks against target, as CombatSimSkillAttack computes it
        // (rally, berserk, suppress, mark, armor less pierce), before barriers.
        static int AttackDamage(CombatSimCard actor, CombatSimCard target)
        {
            int dmg = actor.Attack + actor.GetEffectValue("berserk") + actor.GetEffectValue("rally")
                      + target.GetEffectValue("enfeeble") - actor.GetEffectValue("weaken");
            int armor = target.GetSkillValue("armored") - actor.GetSkillValue("pierce");
            if (armor > 0) dmg -= armor;
            return dmg < 0 ? 0 : dmg;
        }

        // "Unit will move to unprotected enemy lanes": a blocked unit moves to an open spot in a lane the enemy has left
        // empty, so its attack reaches the enemy commander. Logged like Cover (the client moves it to the new position).
        class Flank : CombatSimSkill
        {
            public override void HandleSkill(CardSkill skill, CombatSimCard actor, int phase, bool isCurrentTurn)
            {
                if (phase != 5 || actor.GetTargetCard().CardUID < 0) return;
                var open = new List<int>();
                for (int lane = 0; lane < 4; lane++)
                    for (int row = 0; row < 2; row++)
                    {
                        int pos = actor.owner.GetPositionFromLaneAndRow(lane, row);
                        if (actor.owner.GetCardUIDByPosition(pos) == 0 && actor.owner.GetTargetCard(pos).CardUID < 0) { open.Add(pos); break; }
                    }
                if (open.Count == 0) return;
                int to = SynapseRandom.Shuffle(open)[0];
                actor.Position = to;
                var a = new ServiceCombatActionData { SkillID = skill.SkillID, Value = to, CardUID = actor.CardUID };
                a.AddTarget(actor.CardUID);
                actor.owner.GetCombatSim().AddActionData(a);
            }
        }

        // "Attacks target all units in a lane": the units behind the one this unit attacks are hit with the same attack.
        class Laser : CombatSimSkill
        {
            public override void HandleSkill(CardSkill skill, CombatSimCard actor, int phase, bool isCurrentTurn)
            {
                if (phase != 5) return;
                CombatSimCard first = actor.GetTargetCard();
                if (first.CardUID < 0) return;
                var opp = actor.owner.GetOpponent();
                for (int row = 0; row < 3; row++)
                {
                    int uid = opp.GetCardUIDByPosition(opp.GetPositionFromLaneAndRow(Lane(actor.Position), row));
                    if (uid == 0 || uid == first.CardUID) continue;
                    CombatSimCard t = null;
                    foreach (var c in opp.GetCardsInField()) if (c.CardUID == uid) t = c;
                    if (t == null) continue;
                    ClearStatusData();
                    int dmg = AttackDamage(actor, t);
                    int pierce = actor.GetSkillValue("pierce");
                    CheckTargetBarrier(actor, t, ref dmg, ref pierce);
                    t.TakeDamage(dmg);
                    AddActionData("laser", actor, dmg, t.CardUID, false);
                }
            }
        }

        // "Attacks cause additional damage to adjacent units": X damage to the units beside the attacked unit
        // (same row, neighbouring lanes). Camouflage evades it and barriers absorb it, as with Strike.
        class Shrapnel : CombatSimSkill
        {
            public override void HandleSkill(CardSkill skill, CombatSimCard actor, int phase, bool isCurrentTurn)
            {
                if (phase != 5) return;
                CombatSimCard hit = actor.GetTargetCard();
                if (hit.CardUID < 0) return;
                var opp = actor.owner.GetOpponent();
                int lane = Lane(hit.Position), row = hit.Position / 4;
                foreach (int l in new[] { lane - 1, lane + 1 })
                {
                    if (l < 0 || l > 3) continue;
                    int uid = opp.GetCardUIDByPosition(opp.GetPositionFromLaneAndRow(l, row));
                    if (uid == 0) continue;
                    CombatSimCard t = null;
                    foreach (var c in opp.GetCardsInField()) if (c.CardUID == uid) t = c;
                    if (t == null) continue;
                    ClearStatusData();
                    int dmg = skill.X + t.GetEffectValue("enfeeble");
                    if (CheckTargetEvade(t)) dmg = 0;
                    else
                    {
                        int pierce = actor.GetSkillValue("pierce");
                        CheckTargetBarrier(actor, t, ref dmg, ref pierce);
                        t.TakeDamage(dmg);
                    }
                    AddActionData("shrapnel", actor, dmg, t.CardUID, false);
                }
            }
        }
    }
}
