"""Extract per-level unit stats from the Global Assault fandom wiki {{UnitStats}} templates into units.json."""
import json, os, re, sys

WIKI = sys.argv[1]
OUT = sys.argv[2]
STARS = {"one": 1, "two": 2, "three": 3, "four": 4, "five": 5, "six": 6, "seven": 7}
SKILL = re.compile(r"^([A-Za-z][A-Za-z .'-]*?)\s*(\d+)?$")


def parse_template(body):
    """MediaWiki template semantics: unnamed params are numbered 1, 2, ...; "N = v" sets slot N explicitly.
    Each level occupies 6 slots: attack | health | (unused) | skill | skill | skill."""
    params, pos, n = {}, {}, 0
    body = body.strip()
    for tok in body.split("|")[1:] if body.startswith("|") else body.split("|"):
        m = re.match(r"\s*([A-Za-z_]+|\d+)\s*=(.*)", tok, re.S)
        if m and not m.group(1).isdigit():
            params[m.group(1).lower()] = m.group(2).strip()
        elif m:
            pos[int(m.group(1))] = m.group(2).strip()
        else:
            n += 1
            pos.setdefault(n, tok.strip())
    levels = []
    for base in range(1, (max(pos) if pos else 0) + 1, 6):
        atk, hp = pos.get(base, ""), pos.get(base + 1, "")
        if not (atk.isdigit() and hp.isdigit()):
            if atk or hp:
                levels.append(None)  # level exists on the wiki but its numbers are unknown ("??")
            continue
        lvl = {"attack": int(atk), "health": int(hp), "skills": []}
        for k in range(base + 3, base + 6):
            sk = SKILL.match(pos.get(k, "")) if pos.get(k) else None
            if sk:
                lvl["skills"].append({"name": sk.group(1).strip(), "x": int(sk.group(2)) if sk.group(2) else None})
        levels.append(lvl)
    while levels and levels[-1] is None:
        levels.pop()
    return params, levels


units, other = {}, []
for f in sorted(os.listdir(WIKI)):
    text = open(os.path.join(WIKI, f), encoding="utf-8", errors="replace").read()
    m = re.search(r"\{\{\s*UnitStats(.*?)\}\}", text, re.S)
    if not m:
        other.append(f)
        continue
    params, levels = parse_template(m.group(1))
    name = f[:-5].replace("_", " ")
    fuse = re.findall(r"\*\s*Fuse\s+(.+)", text)
    units[name] = {
        "stars": STARS.get(params.get("stars", "").strip().lower()),
        "max_level": int(params["maxlv"]) if params.get("maxlv", "").isdigit() else len(levels),
        "complete": all(levels) and (not params.get("maxlv", "").isdigit() or len(levels) == int(params["maxlv"])),
        "levels": levels,
        "categories": re.findall(r"\[\[Category:([^\]]+)\]\]", text),
        "fusion_from": [re.findall(r"\[\[([^\]|]+)", x) for x in fuse],
        "obtain": re.findall(r"^\*\s*(.+)$", text.split("Fusion Component")[0], re.M),
    }
json.dump(units, open(OUT, "w"), indent=1)
skills = sorted({s["name"] for u in units.values() for l in u["levels"] if l for s in l["skills"]})
empty = [n for n, u in units.items() if not u["levels"]]
bad = [n for n, u in units.items() if not u["complete"]]
print("incomplete", len(bad), bad)
print(f"units {len(units)}, with levels {len(units) - len(empty)}, no levels {len(empty)}: {empty[:10]}")
print("skills", skills)
print("stars", {s: sum(1 for u in units.values() if u['stars'] == s) for s in range(1, 8)}, "unknown", sum(1 for u in units.values() if not u['stars']))
