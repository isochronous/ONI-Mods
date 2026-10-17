from survey import *
for doc in ['standards/README.md', 'standards/code-style.md', 'standards/error-handling.md', 'standards/naming-conventions.md']:
    print('==', doc)
    for m in mods:
        print(f'   {m:32s} {doctags.get((f"mods/{m}", doc))}')
# glosses
gl = {}
for m in mods:
    txt = open(f'{ROOT}/mods/{m}/.context/tags.md', encoding='utf-8').read()
    for mm in re.finditer(r'^## (\S+)\n\*\*Id:\*\*[^\n]*\n\n(.*?)\n', txt, re.M):
        gl.setdefault(mm.group(1), (mm.group(2), []))[1].append(m)
for t, (g, ms) in sorted(gl.items()):
    print(f'{t} [{len(ms)}]: {g[len("A record earns this term when it is about "):]}')
    if len(ms) < 17:
        print('      in:', ' '.join(ms))
