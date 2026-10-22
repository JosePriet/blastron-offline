#!/usr/bin/env python3
"""Equipment, components and their Armory recipes for the offline server -> data/equipment.json.

Usage: equipment_to_json.py Equipment.wiki Armory.wiki dimensions_items.php out.json

Sources:
  - Equipment.wiki (archived Global Assault wiki, live API): each item's stars, player level requirement, effect and
    components. Components the wiki gives as "?" use the recipe of a same-star item (OURS below).
  - Armory.wiki: Armory build time by stars ("Equipment Fusion Times").
  - dimensions_items.php (archived Synapse Games item list, havoc-dev): the real item ids.
  - Sprite names: the client's UI atlas (sharedassets1 EquipmentCollection). Items without a sprite of their own use a
    similar one (SPRITE below).
  - Research Credits and time by stars: the wiki's Research_Lab "Equipment Research" table.
"""
import json
import re
import sys

EQUIP, ARMORY, ITEMS, OUT = sys.argv[1:5]

ids = {}
for i, name, _ in re.findall(r"(\d+),([^,\n]+),([^\n]+)", open(ITEMS, encoding="utf-8", errors="ignore").read()):
    ids[name.strip()] = int(i)
ALIAS = {"Inspired Banner": "Inspiring Banner", "Staballoys": "Staballoy"}

# skill names on the wiki -> the client's skill ids (CombatSimSkillFactory); camouflage is evade
SKILLS = {"pierce": "pierce", "armor": "armored", "strike": "strike", "strike all": "strikeall", "rally": "rally",
          "rally all": "rallyall", "retaliate": "counter", "incendiary": "poison", "camouflage": "evade",
          "suppress": "weaken", "suppress all": "weakenall", "barrier": "barrier", "barrier all": "barrierall",
          "mark": "enfeeble", "mark all": "enfeebleall"}

# components: stars are ours (display only)
COMPONENTS = {"Havoc Tech": (1, "havoc_tech1"), "Rare Havoc Tech": (2, "havoc_tech2"), "Elite Havoc Tech": (3, "havoc_tech3"),
              "Polymer": (2, "polymer1"), "Composite Polymer": (3, "polymer2"), "Alloy": (2, "alloy1"),
              "Staballoy": (3, "alloy2"), "Uranium": (4, "uranium")}

SPRITE = {"Light AP Rounds": "light_AP_rounds", "AP Rounds": "AP_rounds", "Heavy AP Rounds": "heavy_AP_rounds",
          "Camouflage Device": "camouflage_plating", "Ghost System": "ghost_plating", "Seeker Missile": "seeking_missile",
          "Scatter Missile": "seeking_missile", "Heavy Barrier": "prism_barrier", "Infrared Tracking": "infrared_scope",
          "Thick Smog": "engulfing_smog", "Encouraging Banner": "flag1", "Stirring Banner": "flag2",
          "Motivating Banner": "flag3", "Inspiring Banner": "flag3"}

# OURS: the wiki lists "?" for these; each takes the recipe of a same-star item
OURS = {"Billowing Smog": "Camouflage Device", "Light Barrier": "Scatter Missile", "Engulfing Smog": "Inspiring Banner",
        "Prism Barrier": "Infrared Tracking"}

RESEARCH = {1: (250, 3600), 2: (300, 6 * 3600), 3: (500, 12 * 3600), 4: (750, 2 * 86400), 5: (1200, 4 * 86400)}

times = {}
for stars, t in re.findall(r"\[\[Equipment#(\w+)_Stars?_Equipment\|[^\]]*\]\]\s*\|\s*(\w+)", open(ARMORY).read()):
    n = {"One": 1, "Two": 2, "Three": 3, "Four": 4, "Five": 5}[stars]
    times[n] = int(t[:-1]) * {"m": 60, "h": 3600}[t[-1]]


def name_of(s):
    s = re.sub(r"\s+", " ", s).strip()
    return ALIAS.get(s, s)


items, recipes = [], {}
wiki = open(EQUIP).read()
for stars_word, table in re.findall(r"==(\w+) Stars? Equipment==(.*?)\|\}", wiki, re.S):
    stars = {"One": 1, "Two": 2, "Three": 3, "Four": 4, "Five": 5}[stars_word]
    for row in table.split("|-")[1:]:
        cells = [c.strip() for c in re.split(r"\n\|", "\n" + row.strip())[1:]]
        if len(cells) < 4:
            continue
        name, level, effect, comps = name_of(cells[0]), int(re.sub(r"\D", "", cells[1])), cells[2], cells[3]
        e = {"id": ids[name], "name": name, "rarity": stars, "level_req": level}
        parts = []
        for n, what in re.findall(r"(\d+)\s*x?\s+([A-Za-z ]+?)(?=\s*(?:\d|$))", comps.replace("\n", " ") + " "):
            parts.append({"item_id": ids[name_of(what)], "number": int(n)})
        if name == "Havoc Tech":  # converts Rare Havoc Tech into 20 Havoc Tech
            recipes[name] = {"number": 20, "resource": parts}
            continue
        attack = health = 0
        skills = []
        for stat, x in re.findall(r"([A-Za-z ]+?)\s*\+(\d+)", effect):
            stat = stat.strip().lower()
            if stat == "attack":
                attack = int(x)
            elif stat == "defense":
                health = int(x)
            else:
                skills.append({"id": SKILLS[stat], "x": int(x)})
        # no "skill" key without skills: the client offers an item with an empty skill list for any unit
        if skills:
            e["skill"] = skills
        e.update({"attack": attack, "health": health, "picture": SPRITE.get(name, re.sub(r"\W+", "_", name.lower()))})
        items.append(e)
        recipes[name] = {"number": 1, "resource": parts}

for name, like in OURS.items():
    assert not recipes[name]["resource"], name
    recipes[name] = dict(recipes[like], ours=like)

for name, (stars, picture) in COMPONENTS.items():
    items.append({"id": ids[name], "name": name, "rarity": stars, "level_req": 5 if name == "Havoc Tech" else 0,
                  "attack": 0, "health": 0, "picture": picture, "component": 1})

by_name = {e["name"]: e for e in items}
fusions = []
for name, r in recipes.items():
    e = by_name[name]
    cost, research = RESEARCH[e["rarity"]]
    f = {"id": 1000 + e["id"], "name": name, "item_id": e["id"], "number": r["number"], "time": times[e["rarity"]],
         "research_cost": cost, "research_time": research, "level_req": e["level_req"], "resource": r["resource"]}
    if "ours" in r:
        f["components_from"] = r["ours"]
    fusions.append(f)

json.dump({"equipment": items, "fusion_recipes": fusions}, open(OUT, "w"), indent=1)
print(f"equipment {sum(1 for e in items if 'component' not in e)}, components {len(COMPONENTS)}, recipes {len(fusions)}")
