import struct, sys
# Lets a 32-bit ARM library with text relocations load at targetSdk >= 23:
# drops DT_TEXTREL (retagged DT_DEBUG, which the linker ignores) and makes every
# executable PT_LOAD segment also writable so the relocations can be applied.
src, dst = sys.argv[1], sys.argv[2]
b = bytearray(open(src,'rb').read())
assert b[:4] == b'\x7fELF' and b[4] == 1 and b[5] == 1
e_phoff, = struct.unpack_from('<I', b, 28)
e_phentsize, e_phnum = struct.unpack_from('<HH', b, 42)
PT_LOAD, PT_DYNAMIC, PF_X, PF_W = 1, 2, 1, 2
DT_NULL, DT_DEBUG, DT_TEXTREL, DT_FLAGS, DF_TEXTREL = 0, 21, 22, 30, 4
changed = []
for i in range(e_phnum):
    off = e_phoff + i*e_phentsize
    p_type, p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_flags, p_align = struct.unpack_from('<8I', b, off)
    if p_type == PT_LOAD and p_flags & PF_X and not p_flags & PF_W:
        struct.pack_into('<I', b, off+24, p_flags | PF_W); changed.append('PT_LOAD@%x RX->RWX' % p_vaddr)
    if p_type == PT_DYNAMIC:
        d = p_offset
        while d < p_offset + p_filesz:
            tag, val = struct.unpack_from('<iI', b, d)
            if tag == DT_NULL: break
            if tag == DT_TEXTREL:
                struct.pack_into('<i', b, d, DT_DEBUG); changed.append('DT_TEXTREL->DT_DEBUG')
            if tag == DT_FLAGS and val & DF_TEXTREL:
                struct.pack_into('<I', b, d+4, val & ~DF_TEXTREL); changed.append('DF_TEXTREL cleared')
            d += 8
open(dst,'wb').write(b)
print(src.split('/')[-1], changed)
