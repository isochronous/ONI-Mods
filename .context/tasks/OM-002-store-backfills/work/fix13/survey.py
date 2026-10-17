import json, re, sys, os, collections
sys.stdout.reconfigure(encoding='utf-8')
S = os.path.dirname(os.path.abspath(__file__))
ROOT = 'C:/dev/ONI-Mods'
blocks = json.load(open(os.path.join(S, 'map.json'), encoding='utf-8'))['records']
docs = json.load(open(os.path.join(S, 'docs.json'), encoding='utf-8'))['records']

def slug(t):
    t = t.lower()
    t = re.sub(r'[^a-z0-9]+', '-', t).strip('-')
    return t

def parse(path):
    lines = open(path, encoding='utf-8').read().split('\n')
    # skip frontmatter
    i = 0
    if lines and lines[0] == '---':
        i = 1
        while lines[i] != '---':
            i += 1
        i += 1
    out = {}
    stack = []  # (level, slug)
    cur = None
    infence = False
    for ln in lines[i:]:
        if ln.startswith('```'):
            infence = not infence
        m = None if infence else re.match(r'^(#{1,6}) (.*)$', ln)
        if m:
            lvl = len(m.group(1))
            if lvl == 1:
                cur = None
                stack = []
                continue
            while stack and stack[-1][0] >= lvl:
                stack.pop()
            stack.append((lvl, slug(m.group(2))))
            cur = '/'.join(s for _, s in stack)
            out[cur] = {'heading': m.group(2), 'lines': []}
            continue
        if cur is None:
            continue
        if ln.startswith('**Id:**') or ln.startswith('**Tags:**'):
            continue
        out[cur]['lines'].append(ln.rstrip())
    for k, v in out.items():
        v['text'] = '\n'.join(v['lines']).strip()
    return out

mods = sorted(d for d in os.listdir(ROOT + '/mods') if os.path.isdir(f'{ROOT}/mods/{d}/.context'))
blocktags = {}
for b in blocks:
    blocktags[(b['node'], b['id'])] = b['tags']
doctags = {(d['node'], d['id']): d['tags'] for d in docs}

# declared terms per node
declared = {}
for b in blocks:
    if b['id'].startswith('tags.md#'):
        declared.setdefault(b['node'], set()).add(b['id'].split('#', 1)[1])

if __name__ == '__main__':
    print({k: len(v) for k, v in declared.items()})
