import sys, os, zipfile
# repack.py SRC DST [entry=file ...] [+dir=localdir ...]
#   entry=file   replace (or add, deflated) one APK entry
#   +dir=local   add every file under local as dir/<relpath>, stored uncompressed
src, dst, *args = sys.argv[1:]
rep = {}; adds = []
for a in args:
    k, v = a.split('=', 1)
    if k.startswith('+'):
        base = k[1:].rstrip('/')
        for root, _, files in os.walk(v):
            for f in sorted(files):
                p = os.path.join(root, f)
                adds.append((base + '/' + os.path.relpath(p, v).replace(os.sep, '/'), p))
    else:
        rep[k] = v
zi = zipfile.ZipFile(src)
zo = zipfile.ZipFile(dst, 'w')
seen = set()
for info in zi.infolist():
    n = info.filename
    if n.startswith('META-INF/') and n.endswith(('.SF', '.RSA', '.DSA', '.MF')):
        continue
    data = open(rep[n], 'rb').read() if n in rep else zi.read(n)
    seen.add(n)
    ni = zipfile.ZipInfo(n, date_time=info.date_time); ni.compress_type = info.compress_type; ni.external_attr = info.external_attr
    zo.writestr(ni, data)
for n, p in rep.items():
    if n not in seen:
        ni = zipfile.ZipInfo(n, date_time=(2014, 3, 4, 0, 0, 0)); ni.compress_type = zipfile.ZIP_DEFLATED
        zo.writestr(ni, open(p, 'rb').read())
for n, p in adds:
    assert n not in seen, n
    ni = zipfile.ZipInfo(n, date_time=(2014, 3, 4, 0, 0, 0)); ni.compress_type = zipfile.ZIP_STORED
    zo.writestr(ni, open(p, 'rb').read())
zo.close()
