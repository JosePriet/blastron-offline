#!/usr/bin/env python3
"""Raise targetSdkVersion in a binary AndroidManifest.xml.

Usage: patch_manifest.py IN.xml OUT.xml TARGET

Android 14 and later refuse to install apps that target an SDK below 23 ("App not installed as app isn't compatible
with your phone"). The 1.30.1 release targets 21. Its only runtime-permission (dangerous) permission is
WRITE_EXTERNAL_STORAGE, which the game does not need for its own files (getExternalFilesDir needs no permission
from API 19 on), so targeting 23 changes nothing the game relies on.
"""
import struct
import sys

TARGET_SDK_ATTR = 0x01010270  # android:targetSdkVersion
RES_XML_RESOURCE_MAP, RES_XML_START_ELEMENT = 0x0180, 0x0102
TYPE_INT_DEC = 0x10

src, dst, target = sys.argv[1], sys.argv[2], int(sys.argv[3])
b = bytearray(open(src, "rb").read())
res_map, patched, pos = [], 0, 8
while pos < len(b):
    ctype, hsize, csize = struct.unpack_from("<HHI", b, pos)
    if ctype == RES_XML_RESOURCE_MAP:
        res_map = list(struct.unpack_from("<%dI" % ((csize - hsize) // 4), b, pos + hsize))
    elif ctype == RES_XML_START_ELEMENT:
        ext = pos + hsize
        attr_start, attr_size, attr_count = struct.unpack_from("<HHH", b, ext + 8)
        for i in range(attr_count):
            a = ext + attr_start + i * attr_size
            name = struct.unpack_from("<I", b, a + 4)[0]
            if name < len(res_map) and res_map[name] == TARGET_SDK_ATTR:
                dtype, data = b[a + 15], struct.unpack_from("<I", b, a + 16)[0]
                assert dtype == TYPE_INT_DEC, "targetSdkVersion is not a plain integer"
                print(f"targetSdkVersion {data} -> {max(data, target)}")
                struct.pack_into("<I", b, a + 16, max(data, target))
                patched += 1
    pos += csize
assert patched == 1, f"found {patched} targetSdkVersion attributes"
open(dst, "wb").write(b)
