import struct, sys
# Sets android:targetSdkVersion on <uses-sdk> in a binary AndroidManifest.xml.
src, dst, newval = sys.argv[1], sys.argv[2], int(sys.argv[3])
b = bytearray(open(src,'rb').read())
TARGET = 0x01010270
pos = 8; resids = []; strings = []
while pos < len(b):
    ctype, hsize, csize = struct.unpack_from('<HHI', b, pos)
    if ctype == 0x0180:
        resids = list(struct.unpack_from('<%dI' % ((csize-8)//4), b, pos+8))
    elif ctype == 0x0102:
        attr_start, attr_size, attr_count = struct.unpack_from('<HHH', b, pos+16+8)
        a = pos + 16 + attr_start
        for i in range(attr_count):
            off = a + i*attr_size
            ns, name, raw, sz, r0, typ, data = struct.unpack_from('<IIIHBBI', b, off)
            if name < len(resids) and resids[name] == TARGET:
                print('targetSdkVersion was', data, 'type', hex(typ))
                assert typ == 0x10
                struct.pack_into('<I', b, off+16, newval)
                print('now', newval)
    pos += csize
open(dst,'wb').write(b)
