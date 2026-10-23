#!/usr/bin/env python3
"""missions.xml (havoc-dev 2017 archive) -> missions.json for the in-APK local server.

usage: missions_to_json.py missions.xml out.json

Keeps only what the server needs: locations, feature locks, campaigns (in file order) and missions.
XML comments (commented-out campaigns and speech) are dropped by the parser, as the original server's would be.
"""
import json, sys
import xml.etree.ElementTree as ET

src, dst = sys.argv[1:3]
root = ET.parse(src).getroot()
num = lambda e, tag, d=0: int((e.findtext(tag) or "").strip() or d)
txt = lambda e, tag: (e.findtext(tag) or "").strip()


def lines(e):
    if e is None:
        return []
    out = []
    for l in e.findall("line"):
        d = {"commander": txt(l, "commander"), "text": txt(l, "text")}
        if num(l, "enemy"):
            d["enemy"] = 1
        out.append(d)
    return out


locations = [{"id": num(l, "id"), "name": txt(l, "name"), "map": txt(l, "map")} for l in root.findall("location")]
locks = []
for f in root.findall("feature_lock"):
    d = {"id": txt(f, "id")}
    for k in ("mission_req", "research_req", "level_req", "num_pvp", "achievement_req"):
        if f.find(k) is not None:
            d[k] = num(f, k)
    locks.append(d)

campaigns = []
for c in root.findall("campaign"):
    campaigns.append({"id": num(c, "id"), "name": txt(c, "name"), "region": num(c, "region"),
                      "req": [int(r.text) for r in c.findall("campaign_req")],
                      "achievement_req": num(c, "achievement_req"),
                      "missions": [int(m.text) for m in c.iter("mission_id")]})

missions = {}
for m in root.findall("mission"):
    rw = m.find("rewards")
    missions[txt(m, "id")] = {
        "name": txt(m, "name"), "desc": txt(m, "desc"), "enemy": txt(m, "enemy_name"),
        "health": num(m, "health"), "type": num(m, "mission_type"), "map": txt(m, "map"),
        "commander_item_id": num(m, "commander_item_id"),
        "deck": [[int(c.get("id")), int(c.get("level") or 1), int(c.get("slot") or 0)] for c in m.iter("card")],
        "gold": num(rw, "gold") if rw is not None else 0, "sp": num(rw, "sp") if rw is not None else 0,
        "speech": lines(m.find("speech")), "speech_map": lines(m.find("speech_map")),
    }

json.dump({"locations": locations, "feature_locks": locks, "campaigns": campaigns, "missions": missions},
          open(dst, "w", encoding="utf-8"), ensure_ascii=False, separators=(",", ":"))
print(f"{len(locations)} locations, {len(campaigns)} campaigns, {len(missions)} missions -> {dst}")
