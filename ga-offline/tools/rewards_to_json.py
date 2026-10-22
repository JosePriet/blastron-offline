#!/usr/bin/env python3
"""Supply Drop tiers, event crates and campaign unit rewards for the offline server -> data/rewards.json.

Usage: rewards_to_json.py units_wiki.json cards_report.json missions.json event_box.xml out.json

Sources (archived Global Assault wiki, units_wiki.json "obtain" lines, and the archived server event_box.xml):
  - Supply Drop tiers (user decision 2026-10-22). The standard drop (100 Credits, wiki "Credits" page) holds the units
    the wiki lists as "Open in Supply Drop". The Silver, Gold and Platinum drops (Volt Coins) hold the units the wiki
    lists in those drops. Where a tier's odds include a star rating the wiki lists no unit for in that tier, the
    units of that rating from all the drop tiers are used.
  - Odds are ours (the drop odds were server-side and are not archived): a share per star rating for each tier, in
    the shape of League of War's published chest ladder (user decision), split evenly among the rating's units. The
    5-star share is never below 0.75% a drop (user decision 2026-10-22). Prices of the Volt Coin tiers are ours too.
  - Event crates: the archived event_box.xml, with its exact contents, counts and Volt Coin prices. Each opening
    takes one unit out of the crate. Only crates whose units all exist in the rebuilt cards are offered; test boxes
    are skipped.
  - "Reward for completing <campaign>" / "Reward from <campaign>" gives the unit for finishing that campaign.
"""
import json, re, sys

UNITS, REPORT, MISSIONS, BOXES, OUT = sys.argv[1:6]
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
# share of a tier's drops per star rating, in hundredths of a percent; currency "sc" = Credits, "hc" = Volt Coins
TIERS = [
    {"id": 1, "key": "standard", "name": "Supply Drop", "currency": "sc", "cost": 100, "model": "prefab_bronze_crate",
     "share": {1: 6925, 2: 2500, 3: 500, 5: 75}},
    {"id": 2, "key": "silver", "name": "Silver Supply Drop", "currency": "hc", "cost": 20, "model": "prefab_silver_crate",
     "share": {2: 7000, 3: 2400, 4: 500, 5: 100}},
    {"id": 3, "key": "gold", "name": "Gold Supply Drop", "currency": "hc", "cost": 40, "model": "prefab_gold_crate",
     "share": {2: 3000, 3: 6000, 4: 850, 5: 150}},
    {"id": 4, "key": "platinum", "name": "Platinum Supply Drop", "currency": "hc", "cost": 60, "model": "prefab_platinum_crate",
     "share": {3: 8900, 4: 1000, 5: 100}},
]
FLOOR = 0.0075
TIER_RE = {"silver": r"silver", "gold": r"\bgol[dr]\b", "platinum": r"platinum"}

def lines(u):
    o = u.get("obtain") or []
    return o if isinstance(o, list) else [o]

def clean(s):
    return re.sub(r"\[\[|\]\]|<Mission X>,?\s*|[<>]", "", s)

rewards, unmatched = {}, []
listed = {t["key"]: {} for t in TIERS}  # tier -> card id -> (unit, stars)

for name, u in sorted(units.items()):
    if name not in card:
        continue
    stars = int(u.get("stars") or 1)
    for line in map(clean, lines(u)):
        low = line.lower()
        if re.search(r"\bsupply drop\b", low) and not re.search(r"(silver|gold|golr|platinum)\s+(or\s+\w+\s+)?supply drop", low.split("supply drop")[0] + "supply drop") or \
           re.search(r"in (the )?standard supply drop|in supply drop", low):
            listed["standard"][card[name]] = (name, stars)
        if "supply" in low or "drop" in low:
            for key, pat in TIER_RE.items():
                if re.search(pat, low):
                    listed[key][card[name]] = (name, stars)
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

anytier = {}
for key in listed:
    anytier.update(listed[key])
drops = []
for t in TIERS:
    pool = []
    for st, share in sorted(t["share"].items()):
        units_st = {c: v for c, v in listed[t["key"]].items() if v[1] == st} or {c: v for c, v in anytier.items() if v[1] == st}
        assert units_st, f"{t['key']}: no {st}* units"
        for c, (n, _) in sorted(units_st.items()):
            # per-unit weight = share / units at that rating (scaled by 1000 to stay whole)
            pool.append({"id": c, "unit": n, "stars": st, "weight": round(share * 1000 / len(units_st))})
    total = sum(p["weight"] for p in pool)
    five = sum(p["weight"] for p in pool if p["stars"] == 5) / total
    assert five >= FLOOR - 1e-6, f"{t['key']}: 5* at {five:.3%}"
    print(f"{t['name']}: {len(pool)} units, " + ", ".join(
        f"{st}* {sum(p['weight'] for p in pool if p['stars'] == st) / total:.2%}" for st in sorted(t["share"])))
    drops.append({k: t[k] for k in ("id", "key", "name", "currency", "cost", "model")} | {"pool": pool})

# event crates: newest definition of each name, kept only when every unit in it exists in the rebuilt cards
xml = re.sub(r"(?s)<!--.*?-->", "", open(BOXES, errors="ignore").read())
cards_known = set(map(int, report))
crates, skipped = {}, []
for b in re.findall(r"(?s)<event_box>(.*?)</event_box>", xml):
    name = re.search(r"<name>(.*?)</name>", b).group(1)
    if "TEST" in name:
        continue
    cs = [(int(c), int(n), int(l or 0)) for c, n, l in re.findall(r"<card id='(\d+)' count='(\d+)'(?: label='(\d+)')?", b)]
    cost = re.search(r"<hc_cost>(\d+)</hc_cost>", b)
    crates[name] = {"box_id": int(re.search(r"<id>(\d+)</id>", b).group(1)), "name": name, "cost": int(cost.group(1)) if cost else 0,
                    "cards": [{"id": c, "count": n, "label": l} for c, n, l in cs]}
offered = []
for name, c in crates.items():
    missing = [x["id"] for x in c["cards"] if x["id"] not in cards_known]
    if missing or not c["cards"] or not c["cost"]:
        skipped.append((name, len(missing), len(c["cards"])))
    else:
        offered.append(c)
print(f"event crates offered {len(offered)}: " + ", ".join(c["name"] for c in offered))
print(f"event crates skipped (units missing from the rebuilt cards) {len(skipped)}")
out = {"drops": drops, "event_crates": offered, "campaign_rewards": rewards}
json.dump(out, open(OUT, "w"), indent=1)
print(f"campaign rewards {sum(map(len, rewards.values()))} units in {len(rewards)} campaigns")
for n, l in unmatched:
    print("  no campaign for:", n, "|", l)
