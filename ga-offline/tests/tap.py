# usage: tap.py ui.xml "regex" -> prints "x y" of the first clickable/visible node whose text or content-desc matches
import re, sys
xml = open(sys.argv[1], encoding='utf-8', errors='ignore').read()
pat = re.compile(sys.argv[2], re.I)
for m in re.finditer(r'<node [^>]*>', xml):
    n = m.group(0)
    t = re.search(r' text="([^"]*)"', n); d = re.search(r'content-desc="([^"]*)"', n)
    r = re.search(r'resource-id="([^"]*)"', n)
    label = (t.group(1) if t else '') + ' ' + (d.group(1) if d else '') + ' ' + (r.group(1) if r else '')
    if not label.strip() or not pat.search(label): continue
    b = re.search(r'bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"', n)
    if not b: continue
    x1, y1, x2, y2 = map(int, b.groups())
    if x2 - x1 < 4 or y2 - y1 < 4: continue
    print((x1 + x2) // 2, (y1 + y2) // 2)
    break
