"""Collect surviving Global Assault data and community references from the Wayback Machine and wikis."""
import json, os, re, sys, time, urllib.parse, urllib.request

OUT = sys.argv[1] if len(sys.argv) > 1 else "out"
UA = {"User-Agent": "Mozilla/5.0 (game-preservation research)"}
LOG = []


def log(msg):
    print(msg, flush=True)
    LOG.append(msg)


def get(url, tries=5):
    for i in range(tries):
        try:
            with urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=60) as r:
                return r.read()
        except urllib.error.HTTPError as e:
            if e.code in (404, 403, 410):
                return None
            time.sleep(2 ** i * 3)
        except Exception:
            time.sleep(2 ** i * 3)
    return None


def save(rel, data):
    p = os.path.join(OUT, rel)
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, "wb") as f:
        f.write(data)
    return p


def cdx(prefix, extra=""):
    url = ("https://web.archive.org/cdx/search/cdx?url=" + urllib.parse.quote(prefix, safe="/*:.")
           + "&output=json&fl=timestamp,original,statuscode,mimetype,length&filter=statuscode:200&collapse=digest&limit=50000" + extra)
    raw = get(url)
    rows = json.loads(raw)[1:] if raw and raw.strip() else []
    return rows, raw or b"[]"


WANT = re.compile(r"(\.xml|\.unity3d|\.json|api\.php)", re.I)
PREFIXES = {
    "havoc": "havoc.synapse-games.com/*",
    "havoc-dev": "havoc-dev.synapse-games.com/*",
    "cdn": "d3splaxnu2bep2.cloudfront.net/havoc/*",
    "kong-game": "www.kongregate.com/games/kongregate/global-assault*",
    "kong-forums": "www.kongregate.com/forums/*global-assault*",
}

summary = {}
for name, prefix in PREFIXES.items():
    rows, raw = cdx(prefix)
    save(f"cdx/cdx-{name}.json", raw)
    by_url = {}
    for ts, orig, sc, mt, ln in rows:
        by_url.setdefault(orig.split("?")[0] if "api.php" not in orig else orig, []).append(ts)
    log(f"[cdx] {name}: {len(rows)} captures, {len(by_url)} distinct urls")
    got = 0
    for orig, stamps in sorted(by_url.items()):
        path_part = urllib.parse.urlparse(orig if "://" in orig else "http://" + orig)
        is_data = WANT.search(orig)
        is_page = name == "kong-forums" or name == "kong-game"
        if not (is_data or is_page):
            continue
        stamps = sorted(stamps)
        picks = {stamps[-1], stamps[0]} if is_data else {stamps[-1]}
        for ts in picks:
            data = get(f"https://web.archive.org/web/{ts}id_/{orig}")
            if not data:
                continue
            fname = re.sub(r"[^A-Za-z0-9._-]", "_", (path_part.path.strip("/") or "index") + ("_" + path_part.query if path_part.query else ""))[:180]
            sub = "files" if is_data else "reference/kongregate"
            p = save(f"{sub}/{path_part.netloc}/{ts}_{fname}", data)
            got += 1
            log(f"  saved {p} ({len(data)} bytes)")
            time.sleep(1)
    summary[name] = {"captures": len(rows), "urls": len(by_url), "downloaded": got}

# Fandom wiki: live MediaWiki API first, then Wayback CDX for anything missing.
for host in ("cryhavoc.fandom.com", "cryhavoc.wikia.com"):
    api = f"https://{host}/api.php"
    titles, cont = [], ""
    while True:
        raw = get(f"{api}?action=query&list=allpages&aplimit=500&format=json{cont}")
        if not raw:
            break
        try:
            j = json.loads(raw)
        except ValueError:
            break
        titles += [p["title"] for p in j.get("query", {}).get("allpages", [])]
        if "continue" not in j:
            break
        cont = "&apcontinue=" + urllib.parse.quote(j["continue"]["apcontinue"])
    log(f"[wiki] {host} live API: {len(titles)} pages")
    n = 0
    for t in titles:
        raw = get(f"{api}?action=parse&page={urllib.parse.quote(t)}&prop=wikitext&format=json")
        if raw:
            try:
                wt = json.loads(raw)["parse"]["wikitext"]["*"]
            except (ValueError, KeyError):
                continue
            save(f"reference/wiki-live/{re.sub(r'[^A-Za-z0-9._-]', '_', t)}.wiki", wt.encode())
            n += 1
    summary[f"wiki-live-{host}"] = {"pages": len(titles), "saved": n}
    if titles:
        break

rows, raw = cdx("cryhavoc.wikia.com/wiki/*")
rows2, raw2 = cdx("cryhavoc.fandom.com/wiki/*")
save("cdx/cdx-wiki-wikia.json", raw)
save("cdx/cdx-wiki-fandom.json", raw2)
latest = {}
for ts, orig, *_ in rows + rows2:
    key = urllib.parse.urlparse(orig if "://" in orig else "http://" + orig).path
    if ":" in key.split("/wiki/")[-1] and not key.split("/wiki/")[-1].startswith("Category"):
        continue
    if key not in latest or ts > latest[key][0]:
        latest[key] = (ts, orig)
n = 0
for key, (ts, orig) in sorted(latest.items()):
    data = get(f"https://web.archive.org/web/{ts}id_/{orig}")
    if data:
        save(f"reference/wiki-wayback/{ts}_{re.sub(r'[^A-Za-z0-9._-]', '_', key.split('/wiki/')[-1])[:150]}.html", data)
        n += 1
        time.sleep(1)
summary["wiki-wayback"] = {"pages": len(latest), "saved": n}
log(f"[wiki] wayback: {n}/{len(latest)} pages")

# Reddit search (public JSON).
raw = get("https://www.reddit.com/search.json?q=%22global+assault%22+kongregate&limit=100")
if raw:
    save("reference/reddit/search.json", raw)
summary["reddit"] = {"saved": bool(raw)}

with open(os.path.join(OUT, "SUMMARY.json"), "w") as f:
    json.dump(summary, f, indent=1)
with open(os.path.join(OUT, "LOG.txt"), "w") as f:
    f.write("\n".join(LOG))
log(json.dumps(summary, indent=1))
