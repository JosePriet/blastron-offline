"""Second archive pass: covers the gaps the verification found in fetch.py."""
import json, os, re, sys, time, urllib.parse, urllib.request

OUT = sys.argv[1] if len(sys.argv) > 1 else "out2"
UA = {"User-Agent": "Mozilla/5.0 (game-preservation research)"}
LOG = []


def log(msg):
    print(msg, flush=True)
    LOG.append(msg)


def get(url, tries=5):
    for i in range(tries):
        try:
            with urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=90) as r:
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


def cdx(url, match="prefix", extra=""):
    q = ("https://web.archive.org/cdx/search/cdx?url=" + urllib.parse.quote(url, safe="/:.")
         + f"&matchType={match}&output=json&fl=timestamp,original,statuscode,mimetype,length&limit=100000" + extra)
    raw = get(q)
    try:
        return (json.loads(raw)[1:] if raw and raw.strip() else []), raw or b"[]"
    except ValueError:
        return [], raw or b"[]"


def fname(orig):
    u = urllib.parse.urlparse(orig if "://" in orig else "http://" + orig)
    return u.hostname or "unknown", re.sub(r"[^A-Za-z0-9._-]", "_", (u.path.strip("/") or "index") + ("_" + u.query if u.query else ""))[:180]


summary = {}
# 1. Every capture on any synapse-games.com host, any status, no collapse.
rows, raw = cdx("synapse-games.com", match="domain")
save("cdx/domain-synapse-games.json", raw)
hosts = sorted({fname(r[1])[0] for r in rows})
log(f"[domain] synapse-games.com: {len(rows)} captures on hosts {hosts}")
summary["synapse-domain"] = {"captures": len(rows), "hosts": hosts}

# 2. Download every distinct capture of anything that is not an ordinary page on game hosts.
SKIP = re.compile(r"\.(png|jpg|gif|css|js|ico|woff2?|ttf)(\?|$)", re.I)
done = 0
seen = set()
for ts, orig, sc, mt, ln in rows:
    host, fn = fname(orig)
    if "havoc" not in host and "havoc" not in orig.lower():
        continue
    if SKIP.search(orig) or (ts, orig) in seen:
        continue
    seen.add((ts, orig))
    data = get(f"https://web.archive.org/web/{ts}id_/{orig}")
    if data:
        save(f"files/{host}/{ts}_{sc}_{fn}", data)
        done += 1
        time.sleep(1)
summary["synapse-downloaded"] = done
log(f"[domain] downloaded {done} captures")

# 3. Exact card file guesses on both servers, http and https.
cards_hits = []
for host in ("havoc.synapse-games.com", "havoc-dev.synapse-games.com"):
    for name in ["cards.xml"] + [f"cards_section_{i}.xml" for i in range(1, 30)] + ["skills.xml", "fusion_recipes_cj2.xml", "levels.xml", "items.xml", "achievements.xml", "raids.xml"]:
        for scheme in ("http", "https"):
            r, _ = cdx(f"{scheme}://{host}/assets/{name}", match="exact")
            for ts, orig, sc, mt, ln in r:
                cards_hits.append([ts, orig, sc])
                data = get(f"https://web.archive.org/web/{ts}id_/{orig}")
                if data:
                    save(f"files/exact/{host}/{ts}_{sc}_{name}", data)
            time.sleep(0.5)
summary["exact-card-hits"] = cards_hits
log(f"[exact] card file hits: {cards_hits}")

# 4. Kongregate: game page and Global Assault forum topics.
kong = {}
for name, url in {
    "kong-game": "www.kongregate.com/games/kongregate/",
    "kong-game-K": "www.kongregate.com/games/Kongregate/",
    "kong-forum-12": "www.kongregate.com/forums/12-kongregate-multiplayer-games/topics/",
    "kong-forum-9268": "www.kongregate.com/forums/9268-kongregate-published-games/topics/",
    "kong-forum-4496": "www.kongregate.com/forums/4496-general/topics/",
}.items():
    r, raw = cdx(url, extra="&filter=statuscode:200&collapse=urlkey")
    hits = [x for x in r if re.search(r"global[-_]?assault|havoc", x[1], re.I)]
    save(f"cdx/{name}.json", json.dumps(hits).encode())
    kong[name] = {"listed": len(r), "global_assault": len(hits)}
    for ts, orig, *_ in hits[:400]:
        data = get(f"https://web.archive.org/web/{ts}id_/{orig}")
        if data:
            host, fn = fname(orig)
            save(f"reference/kongregate/{ts}_{fn}.html", data)
            time.sleep(1)
summary["kongregate"] = kong
log(f"[kong] {kong}")

with open(os.path.join(OUT, "SUMMARY.json"), "w") as f:
    json.dump(summary, f, indent=1)
with open(os.path.join(OUT, "LOG.txt"), "w") as f:
    f.write("\n".join(LOG))
