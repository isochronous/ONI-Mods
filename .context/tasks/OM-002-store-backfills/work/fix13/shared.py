from survey import *

groups = collections.defaultdict(dict)  # (doc, addr) -> mod -> text
heads = {}
for m in mods:
    node = f'mods/{m}'
    base = f'{ROOT}/mods/{m}/.context'
    for sub in ('standards', 'domains'):
        for fn in sorted(os.listdir(f'{base}/{sub}')):
            if not fn.endswith('.md'):
                continue
            doc = f'{sub}/{fn}'
            for addr, v in parse(f'{base}/{doc}').items():
                key = f'{doc}#{addr}'
                if (node, key) not in blocktags:
                    print('MISSING ADDR', node, key)
                groups[(doc, addr)][m] = v['text']
                heads[(doc, addr)] = v['heading']

shared = []
for key, per in sorted(groups.items()):
    texts = collections.Counter(per.values())
    top, n = texts.most_common(1)[0]
    if n >= 3:
        mods_same = [m for m, t in per.items() if t == top]
        tagsets = {m: tuple(blocktags.get((f'mods/{m}', f'{key[0]}#{key[1]}'), [])) for m in mods_same}
        shared.append((key, mods_same, tagsets, top))

if __name__ == '__main__':
    for key, ms, ts, top in shared:
        distinct = collections.Counter(ts.values())
        others = [m for m in mods if m not in ms]
        print(f'== {key[0]}#{key[1]}  copies={len(ms)} distinct_tagsets={len(distinct)}' + (f'  differs_in={others}' if others else ''))
        for t, c in distinct.most_common():
            print(f'    {c:2d} {list(t)}')
    print(len(shared))
