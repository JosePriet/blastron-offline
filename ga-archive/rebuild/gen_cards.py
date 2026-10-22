"""Generate a cards.xml for the Global Assault client from the wiki stats and the archived missions.xml.

Usage: gen_cards.py units_wiki.json missions.xml resources_list.txt out_dir

Writes out_dir/cards.xml (client schema, see Tyrant.Data/CardMapper.cs) and out_dir/cards_report.json
(how every id was resolved, so each choice can be audited).
"""
import hashlib, json, os, re, statistics, sys
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape

UNITS, MISSIONS, RESOURCES, OUT = sys.argv[1:5]
NEW_ID_START = 2001  # above every archived card id except 8001, outside the 1000-1999 "commander" band

# Wiki skill name -> (client skill id, icon sprite, order). Icons marked guess in the plan: icon_missile, icon_bullets.
SKILLS = {
    "armor": ("armored", "icon_armored"), "armored": ("armored", "icon_armored"),
    "barrier": ("barrier", "icon_barrier"), "barrier all": ("barrierall", "icon_barrier_all"),
    "strike": ("strike", "icon_missile"), "strike all": ("strikeall", "icon_strike_all"),
    "suppress": ("weaken", "icon_suppression"), "suppress all": ("weakenall", "icon_suppression_all"),
    "mark": ("enfeeble", "icon_mark"), "mark all": ("enfeebleall", "icon_mark_all"),
    "rally": ("rally", "icon_rally"), "rally all": ("rallyall", "icon_rally_all"),
    "camouflage": ("evade", "icon_camo"), "camoflage": ("evade", "icon_camo"),
    "retaliate": ("counter", "icon_counter"), "incendiary": ("poison", "icon_incendiary"),
    "berserk": ("berserk", "icon_berserk"), "blitz": ("blitz", "icon_blitz"), "cover": ("cover", "icon_cover"),
    "flank": ("flank", "icon_flank"), "laser": ("laser", "icon_laser"), "pierce": ("pierce", "icon_bullets"),
    "shrapnel": ("shrapnel", "icon_shrapnel"),
}
# In-game descriptions as quoted on the wiki Skills page.
SKILL_TEXT = {
    "armored": ("Armor", "Advanced alloys reduce damage taken by attacks."),
    "barrier": ("Barrier", "Creates a protective barrier around units."),
    "barrierall": ("Barrier All", "Creates a protective barrier around all allied units."),
    "strike": ("Strike", "Launches a missile at a random enemy unit."),
    "strikeall": ("Strike All", "Launches missiles at all enemy units."),
    "weaken": ("Suppress", "Smoke screen reduces the effectiveness of a random enemy unit's attack."),
    "weakenall": ("Suppress All", "Smoke screen reduces the effectiveness of all enemy units' attacks."),
    "enfeeble": ("Mark", "Increase damage dealt to a random enemy unit."),
    "enfeebleall": ("Mark All", "Increase damage dealt to all enemy units."),
    "rally": ("Rally", "Increases the attack of a random allied unit."),
    "rallyall": ("Rally All", "Increases the attack of all allied units."),
    "evade": ("Camouflage", "Active camouflage allows a unit to evade enemy skills."),
    "counter": ("Retaliate", "When attacked on an enemy's turn, this unit will attack back."),
    "poison": ("Incendiary", "Target takes fire damage over time."),
    "berserk": ("Berserk", "Damage is increased after each successful attack."),
    "blitz": ("Blitz", "Unit is able to attack immediately after being deployed."),
    "cover": ("Cover", "Unit will move to unprotected lanes."),
    "flank": ("Flank", "Unit will move to unprotected enemy lanes."),
    "laser": ("Laser", "Attacks target all units in a lane."),
    "pierce": ("Pierce", "High-velocity weaponry pierces through armor."),
    "shrapnel": ("Shrapnel", "Attacks cause additional damage to adjacent units."),
}
SKILL_ORDER = list(SKILL_TEXT)

norm = lambda s: re.sub(r"[^a-z0-9]", "", s.lower())
pick = lambda key, seq: seq[int(hashlib.sha1(key.encode()).hexdigest(), 16) % len(seq)]

units = {n: u for n, u in json.load(open(UNITS)).items() if u["levels"] and all(u["levels"])}
by_norm = {norm(n): n for n in units}

# Bundled unit meshes (Resources units/*), minus command centers and wreck states.
meshes = sorted(l.strip().split("/", 1)[1] for l in open(RESOURCES) if l.startswith("units/"))
unit_meshes = [m for m in meshes if "commandcenter" not in m]

# --- missions: ids, name hints (attribute + developer comment), where each id is used ---
raw = open(MISSIONS, encoding="utf-8", errors="replace").read()
live = re.sub(r"<!--(?!\s*[^<>]{1,40}?\s*-->).*?-->", "", raw, flags=re.S)  # drop commented-out blocks, keep short trailing labels
root = ET.fromstring(re.sub(r"<!--.*?-->", "", live, flags=re.S))
order = [m.text for c in root.findall("campaign") for m in c.iter("mission_id")]
pos = {mid: i / max(1, len(order) - 1) for i, mid in enumerate(order)}
hints, uses = {}, {}
for m in re.finditer(r'<card\s+id=["\'](\d+)["\']([^>]*?)/?>[ \t]*(?:<!--\s*([^<>]*?)\s*-->)?', live):
    cid, attrs, comment = m.groups()
    nm = re.search(r'name=["\']([^"\']*)["\']', attrs)
    for h in filter(None, [nm and nm.group(1), comment]):
        hints.setdefault(cid, []).append(h.strip())
for mission in root.findall("mission"):
    mid = mission.findtext("id")
    for c in mission.iter("card"):
        uses.setdefault(c.get("id"), []).append((pos.get(mid, 1.0), int(c.get("level") or 1)))

# --- resolve every mission id ---
report, cards = {}, {}
taken_wiki = {}
for cid in sorted(uses, key=int):
    names = hints.get(cid, [])
    # "Zeus+" / "Zeus++" mark the dual / quad fusion of a unit, not the base unit, so they never match by name.
    full = [h for h in names if "+" not in h]
    wiki = [by_norm[norm(h)] for h in full if norm(h) in by_norm]
    if wiki:
        best = max(set(wiki), key=wiki.count)
        conf = "high" if len(set(wiki)) == 1 and len(set(map(norm, full))) == 1 else "medium"
        cards[cid] = {"wiki": best, "name": best}
        taken_wiki.setdefault(best, cid)
        report[cid] = {"source": "wiki", "unit": best, "hints": sorted(set(names)), "confidence": conf}

# player stat templates per star for enemies: wiki units grouped by stars
by_star = {}
for n, u in sorted(units.items()):
    by_star.setdefault(u["stars"], []).append(n)
qs = sorted(statistics.median(p for p, _ in v) for v in uses.values())


def tier_for(cid):
    q = statistics.median(p for p, _ in uses[cid])
    rank = sum(1 for x in qs if x <= q) / len(qs)  # percentile of this id's typical campaign position
    return min(5, 1 + int(rank * 5))


for cid in sorted(uses, key=int):
    if cid in cards:
        continue
    star = tier_for(cid)
    need = max(l for _, l in uses[cid])
    # Template: a wiki unit with enough levels for the deepest deck level this id is used at, preferring the
    # star tier implied by campaign position and the shortest level table that fits.
    fits = [n for n in units if len(units[n]["levels"]) >= need] or [max(units, key=lambda n: len(units[n]["levels"]))]
    best_star = min({units[n]["stars"] for n in fits}, key=lambda st: abs(st - star))
    pool = [n for n in fits if units[n]["stars"] == best_star]
    shortest = min(len(units[n]["levels"]) for n in pool)
    template = pick(cid, sorted(n for n in pool if len(units[n]["levels"]) == shortest))
    names = sorted(set(hints.get(cid, [])))
    name = names[0] if names else f"Shattered Sun Unit {cid}"
    cards[cid] = {"wiki": template, "name": name, "enemy": True}
    report[cid] = {"source": "derived", "template": template, "stars": units[template]["stars"], "position_stars": star, "hints": names,
                   "median_position": round(statistics.median(p for p, _ in uses[cid]), 3),
                   "max_deck_level": max(l for _, l in uses[cid])}

# wiki player units not used in any mission get fresh ids
next_id = NEW_ID_START
for n in sorted(units):
    if n not in taken_wiki:
        cards[str(next_id)] = {"wiki": n, "name": n}
        report[str(next_id)] = {"source": "wiki-new-id", "unit": n}
        next_id += 1


# Developer shorthand in missions.xml comments -> bundled mesh.
MESH_ALIASES = {"lngrng": "longrangetank", "lngrngtank": "longrangetank", "lngtanktank": "longrangetank", "helo": "h4helo"}


def mesh_for(name, key):
    nn = norm(name)
    if nn in MESH_ALIASES:
        return MESH_ALIASES[nn], "own"
    own = [m for m in unit_meshes if nn == m or nn.endswith(m) or (len(m) > 3 and m in nn) or (len(nn) > 3 and m.endswith(nn))]
    if own:
        return max(own, key=len), "own"
    return pick(key, unit_meshes), "borrowed"


# --- write cards.xml ---
used_skills = set()


def skill_xml(level):
    out = []
    for s in level["skills"]:
        sid, _ = SKILLS[s["name"].lower()]
        used_skills.add(sid)
        x = f' x="{s["x"]}"' if s["x"] else ""
        out.append(f'\t\t<skill id="{sid}"{x}/>')
    return out


lines = ['<?xml version="1.0" encoding="UTF-8"?>', "<!-- Rebuilt for the offline revival; see cards_report.json for provenance. -->", "<root>"]
for cid in sorted(cards, key=int):
    c = cards[cid]
    u = units[c["wiki"]]
    hinted = [h for h in report[cid].get("hints", []) if "+" not in h]
    candidates = ([c["wiki"]] if not c.get("enemy") else []) + hinted or [c["name"]]
    found = [mesh_for(n, "mesh" + cid) for n in candidates]
    mesh, how = next((f for f in found if f[1] == "own"), found[0])
    report[cid]["mesh"], report[cid]["mesh_source"] = mesh, how
    lv = list(u["levels"])
    need = max((l for _, l in uses.get(cid, [])), default=0)
    while len(lv) < need:  # deck uses a level past the table: extend by the last step (the client has no clamp)
        a, b = (lv[-2], lv[-1]) if len(lv) > 1 else (lv[-1], lv[-1])
        lv.append({"attack": b["attack"] + max(0, b["attack"] - a["attack"]),
                   "health": b["health"] + max(1, b["health"] - a["health"]), "skills": b["skills"]})
        report[cid]["padded_levels"] = report[cid].get("padded_levels", 0) + 1
    lines += ["\t<unit>", f"\t\t<id>{cid}</id>", f"\t\t<name>{escape(c['name'])}</name>",
              f"\t\t<picture>{mesh}_256</picture>", f"\t\t<mesh>{mesh}</mesh>", "\t\t<asset_bundle>0</asset_bundle>",
              f"\t\t<rarity>{u['stars']}</rarity>", "\t\t<type>1</type>",
              f"\t\t<set>{2 if c.get('enemy') else 1}</set>",
              f"\t\t<attack>{lv[0]['attack']}</attack>", f"\t\t<health>{lv[0]['health']}</health>"]
    lines += skill_xml(lv[0])
    for i, l in enumerate(lv[1:], start=2):
        lines += ["\t\t<upgrade>", f"\t\t<level>{i}</level>", f"\t\t<attack>{l['attack']}</attack>", f"\t\t<health>{l['health']}</health>"]
        lines += ["\t" + s for s in skill_xml(l)]
        lines.append("\t\t</upgrade>")
    lines.append("\t</unit>")

for sid in sorted(used_skills, key=SKILL_ORDER.index):
    name, desc = SKILL_TEXT[sid]
    icon = next(i for s, i in SKILLS.values() if s == sid)
    lines += ["\t<skillType>", f"\t\t<id>{sid}</id>", f"\t\t<name>{name}</name>", f"\t\t<icon>{icon}</icon>",
              f"\t\t<desc>{escape(desc)}</desc>", f"\t\t<order>{SKILL_ORDER.index(sid) + 1}</order>", "\t</skillType>"]
lines += ["\t<cardSet><id>1</id><name>Havoc</name><picture>icon_units</picture><visible>1</visible></cardSet>",
          "\t<cardSet><id>2</id><name>Enemy</name><picture>icon_units</picture><visible>0</visible></cardSet>",
          "\t<unit_type><id>1</id><name>Unit</name><image>icon_units</image></unit_type>", "</root>"]

os.makedirs(OUT, exist_ok=True)
open(os.path.join(OUT, "cards.xml"), "w", encoding="utf-8").write("\n".join(lines) + "\n")
json.dump(report, open(os.path.join(OUT, "cards_report.json"), "w"), indent=1, sort_keys=True)
src = [r["source"] for r in report.values()]
print(f"cards {len(cards)}: wiki-mapped mission ids {src.count('wiki')}, derived enemies {src.count('derived')}, "
      f"new-id player units {src.count('wiki-new-id')}; skills {len(used_skills)}; "
      f"own meshes {sum(1 for r in report.values() if r.get('mesh_source') == 'own')}")
