#!/usr/bin/env python3
"""Map every asset in the game's original asset bundles to its file in the AssetRipper export.
Usage: bundle_manifest.py <apk assets dir> <obb dir> <ExportedProject/Assets> <out.json>
Output: {pack: {"bundles": {id: {"parent": p, "assets": [[name, exported path], ...]}}, "scenes": {...}, "folders": {...}, "version": v}}"""
import UnityPy, json, os, sys, re, collections
APK, OBB, E, OUT = sys.argv[1:5]
EXT = {"GameObject": [".prefab"], "Texture2D": [".png", ".asset"], "Material": [".mat"], "TextAsset": [".txt", ".bytes", ".json", ".asset"],
       "MonoBehaviour": [".asset"], "AudioClip": [".mp3", ".ogg", ".wav"], "Mesh": [".asset"], "AnimationClip": [".anim"],
       "Font": [".ttf", ".otf", ".fontsettings"], "Shader": [".shader"], "Cubemap": [".cubemap", ".asset"]}
TYPEDIRS = {d for d in os.listdir(E) if os.path.isdir(os.path.join(E, d))}
idx = collections.defaultdict(list)
for root, ds, fs in os.walk(E):
    for f in fs:
        if not f.endswith(".meta"):
            stem, ext = os.path.splitext(f)
            idx[(stem.lower(), ext.lower())].append(os.path.relpath(os.path.join(root, f), E).replace(os.sep, "/"))
def norm(p):
    return re.sub(r"(fileID|guid): [0-9a-f-]+", "", open(os.path.join(E, p), errors="replace").read())
packs = json.load(open(os.path.join(APK, "packs.txt"))) + sorted(d for d in os.listdir(OBB) if os.path.isfile(os.path.join(OBB, d, "toc.txt")))
out = {}; warn = []
for p in packs:
    d = os.path.join(APK, p) if os.path.isdir(os.path.join(APK, p)) else os.path.join(OBB, p)
    toc = json.load(open(os.path.join(d, "toc.txt")))
    bundles = {}
    for b, info in toc["bundles"].items():
        want = {x.split("/")[-1].lower() for x in info["paths"]}
        env = UnityPy.load(os.path.join(d, "bundle_" + b + ".assetbundle"))
        types = collections.defaultdict(set)
        for o in env.objects:
            if o.type.name in EXT:
                try:
                    n = o.peek_name() if hasattr(o, "peek_name") else o.read().m_Name
                except Exception:
                    continue
                if n and n.lower() in want:
                    types[n.lower()].add(o.type.name)
        assets = []
        for n in sorted(want):
            if not types[n]:
                warn.append(f"{p}/{b}: {n} not found in bundle"); continue
            for t in sorted(types[n]):
                cands = [c for e in EXT[t] for c in idx.get((n, e), []) if "/" not in c or c.split("/")[0] in (t, "Resources") or not c.split("/")[0] in TYPEDIRS]
                if not cands:
                    warn.append(f"{p}/{b}: {n} ({t}) not in export"); continue
                cands.sort(key=lambda c: (c.count("/"), c))
                if len(cands) > 1 and len({norm(c) for c in cands}) > 1:
                    warn.append(f"{p}/{b}: {n} ({t}) has {len(cands)} differing exports, using {cands[0]}")
                assets.append([n, cands[0]])
        bundles[b] = {"parent": info.get("parent", ""), "assets": assets}
    out[p] = {"bundles": bundles, "scenes": toc.get("scenes", {}), "folders": toc.get("folders", {}), "version": toc.get("version", "")}
json.dump(out, open(OUT, "w"), indent=1)
print("\n".join(warn[:60])); print(len(warn), "warnings")
