#!/usr/bin/env python3
"""Supply Drop pool and campaign unit rewards for the offline server -> data/rewards.json.

Usage: rewards_to_json.py units_wiki.json cards_report.json missions.json out.json

Sources (archived Global Assault wiki, units_wiki.json "obtain" lines):
  - "Open in Supply Drop" (the standard drop, bought with Credits; the Credits page gives the price, 100) puts a
    unit in the pool. Lines naming only Silver/Gold/Platinum drops or crates do not. "Very small chance"/"Very Rare"
    in the standard drop is treated like any other line.
  - 4-star units: the wiki lists none in the standard drop, which more likely means the pages omit it than that the
    drop never gave them (user decision 2026-10-22). The base 4-star units the wiki lists in the Silver or Platinum
    Supply Drop are added.
  - "Reward for completing <campaign>" / "Reward from <campaign>" gives the unit for finishing that campaign.
Odds are ours (the drop odds were server-side and are not archived): a share per star tier, lower for rarer tiers,
split evenly among the tier's units. The 5-star tier as a whole is 0.75% a drop, the lowest a tier of 5 stars or fewer
may get (user decision 2026-10-22).
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
# share of all drops per star tier, in hundredths of a percent
TIER = {1: 6625, 2: 2500, 3: 500, 4: 300, 5: 75}
FLOOR = 0.0075

def lines(u):
    o = u.get("obtain") or []
    return o if isinstance(o, list) else [o]

def clean(s):
    return re.sub(r"\[\[|\]\]|<Mission X>,?\s*|[<>]", "", s)

pool, rewards, unmatched = [], {}, []

def add(name, u):
    if not any(p["id"] == card[name] for p in pool):
        pool.append({"id": card[name], "unit": name, "stars": int(u.get("stars") or 1)})

for name, u in sorted(units.items()):
    if name not in card:
        continue
    for line in map(clean, lines(u)):
        low = line.lower()
        if re.search(r"\bsupply drop\b", low) and not re.search(r"(silver|gold|golr|platinum)\s+(or\s+\w+\s+)?supply drop", low.split("supply drop")[0] + "supply drop") or \
           re.search(r"in (the )?standard supply drop|in supply drop", low):
            add(name, u)
        elif int(u.get("stars") or 0) == 4 and re.search(r"(silver|platinum) supply", low):
            add(name, u)
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

# per-unit weight = tier share / units in the tier (scaled by 1000 to stay whole)
count = {t: sum(1 for p in pool if p["stars"] == t) for t in TIER}
for p in pool:
    p["weight"] = round(TIER[p["stars"]] * 1000 / count[p["stars"]])
total = sum(p["weight"] for p in pool)
for t in TIER:
    share = sum(p["weight"] for p in pool if p["stars"] == t) / total
    assert t > 5 or not count[t] or share >= FLOOR - 1e-6, f"{t}* tier at {share:.3%}, below {FLOOR:.2%}"
    print(f"  {t}*: {count[t]} units, {share:.3%} of drops")
out = {"supply_drop": {"name": "Supply Drop", "cost": 100, "pool": sorted(pool, key=lambda p: p["id"])},
       "campaign_rewards": rewards}
json.dump(out, open(OUT, "w"), indent=1)
print(f"pool {len(pool)} units, campaign rewards {sum(map(len, rewards.values()))} units in {len(rewards)} campaigns")
for n, l in unmatched:
    print("  no campaign for:", n, "|", l)
