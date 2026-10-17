"""Follow-up to unit 13: give each shared standards document one document tag set in every mod, then
re-derive every shared section's canonical set against that document tag set (unit 13's CANON table).
Reads fresh map snapshots (map.json, docs.json) taken after the fix12 change-sets landed."""
from gen import CANON, tag_op, declared, doctags, blocktags, shared_keys, mods, ROOT
import json

DOC = {
    'standards/code-style.md': ['code-style'],
    'standards/error-handling.md': [],
    'standards/naming-conventions.md': ['code-style'],
}
OUT = f'{ROOT}/.context/tasks/OM-002-store-backfills/changesets'
notes = []
total = 0
for m in mods:
    node = f'mods/{m}'
    dec = declared[node]
    ops = []
    for doc, dset in DOC.items():
        want_doc = [t for t in dset if t in dec]
        op = tag_op(doc, None, list(doctags.get((node, doc), [])), want_doc, dset)
        if op:
            ops.append(op)
        final = {}
        canon = CANON[doc]
        for addr in sorted(canon, key=lambda a: a.count('/')):
            if m not in shared_keys[(doc, addr)]:
                notes.append(f'{m}: {doc}#{addr} differs from the shared text; left alone')
                continue
            anc = set(want_doc)
            parts = addr.split('/')
            for j in range(1, len(parts)):
                anc |= set(final.get('/'.join(parts[:j]), []))
            want = [t for t in canon[addr] if t in dec and t not in anc]
            final[addr] = want
            op = tag_op(doc, addr, list(blocktags[(node, f'{doc}#{addr}')]), want, canon[addr])
            if op:
                ops.append(op)
            for (n2, rid), t2 in blocktags.items():
                if n2 == node and rid.startswith(f'{doc}#{addr}/') and rid.split('#', 1)[1] not in canon:
                    dup = set(t2) & set(want)
                    if dup:
                        notes.append(f'{m}: {rid} repeats {sorted(dup)} from {addr}')
    if ops:
        cs = {'task': 'OM-002', 'node': node, 'ops': ops,
              'why': 'one document tag set per shared standards document in every mod, with each shared section re-derived against it'}
        with open(f'{OUT}/fix14-{m}.json', 'w', encoding='utf-8', newline='\n') as f:
            json.dump(cs, f, indent=1, ensure_ascii=False)
            f.write('\n')
    total += len(ops)
    print(m, len(ops))
print('total ops', total)
for n in notes:
    print('NOTE', n)
