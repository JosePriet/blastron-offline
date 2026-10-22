#!/usr/bin/env python3
"""Supply Drop pool and campaign unit rewards for the offline server -> data/rewards.json.

Usage: rewards_to_json.py units_wiki.json cards_report.json missions.json out.json

Sources (archived Global Assault wiki, units_wiki.json "obtain" lines):
  - "Open in Supply Drop" (the standard drop, bought with Credits; the Credits page gives the price, 100) puts a
    unit in the pool. Lines naming only Silver/Gold/Platinum drops or crates do not. "Very small chance"/"Very Rare"
    in the standard drop is kept with a low weight.
  - "Reward for completing <campaign>" / "Reward from <campaign>" gives the unit for finishing that campaign.
Weights by stars are ours (the drop odds were server-side and are not archived).
"""
import json, re, sys

UNITS, REPORT, MISSIONS, OUT = sys.argv[1:5]
units = json.load(open(UNITS))
report = json.load(open(REPORT))
missions = json.load(open(MISSIONS))

# wiki unit -> player card id (lowest id among the player-set cards mapped to that unit)
ids = {}
for cid, r in report.items():
    if r.get("unit") and r.get("source") in ("wiki", "wiki-new-id"):
        ids.setdefault(r["unit"], []).append(int(cid))
card = {n: min(v) for n, v in ids.items()}

campaigns = {c["name"].lower(): c["id"] for c in missions["campaigns"] if c.get("name")}
ALIASES = {"evasion maneuvers": "evasive maneuvers", "buring tide": "burning tide", "grinder": "the grinder"}
WEIGHT = {1: 60, 2: 30, 3: 8, 4: 3, 5: 1}
RARE = 1

def lines(u):
    o = u.get("obtain") or []
    return o if isinstance(o, list) else [o]

def clean(s):
    return re.sub(r"\[\[|\]\]|<Mission X>,?\s*|[<>]", "", s)

pool, rewards, unmatched = [], {}, []
for name, u in sorted(units.items()):
    if name not in card:
        continue
    for line in map(clean, lines(u)):
        low = line.lower()
        if re.search(r"\bsupply drop\b", low) and not re.search(r"(silver|gold|golr|platinum)\s+(or\s+\w+\s+)?supply drop", low.split("supply drop")[0] + "supply drop") or \
           re.search(r"in (the )?standard supply drop|in supply drop", low):
            rare = re.search(r"very (small chance|rare)", low)
            w = RARE if rare else WEIGHT.get(int(u.get("stars") or 1), 1)
            if not any(p["id"] == card[name] for p in pool):
                pool.append({"id": card[name], "unit": name, "stars": u.get("stars"), "weight": w})
        m = re.search(r"reward (?:for completing|from)\s+(?:the\s+)?(?:time-limited\s+)?(.+?)(?:\s+(?:limited time\s+)?campaign\b|\.|$)", line, re.I)
        if m:
            cname = m.group(1).strip().lower()
            cname = ALIASES.get(cname, cname)
            cid = campaigns.get(cname)
            if cid:
                rewards.setdefault(str(cid), [])
                if card[name] not in rewards[str(cid)]:
                    rewards[str(cid)].append(card[name])
            else:
                unmatched.append((name, line))

out = {"supply_drop": {"name": "Supply Drop", "cost": 100, "pool": sorted(pool, key=lambda p: p["id"])},
       "campaign_rewards": rewards}
json.dump(out, open(OUT, "w"), indent=1)
print(f"pool {len(pool)} units, campaign rewards {sum(map(len, rewards.values()))} units in {len(rewards)} campaigns")
for n, l in unmatched:
    print("  no campaign for:", n, "|", l)
