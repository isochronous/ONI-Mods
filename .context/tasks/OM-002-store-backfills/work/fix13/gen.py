from shared import *

CANON = {
    'standards/error-handling.md': {
        'principles': ['vanilla-fallback', 'logging', 'exception-handling'],
        'guarding-patches': ['harmony-patching'],
        'guarding-patches/missing-member-warn-leave-vanilla': ['reflection', 'vanilla-fallback'],
        'guarding-patches/transpiler-count-the-match': ['transpiler', 'vanilla-fallback'],
        'guarding-patches/missing-data-warn-continue': ['vanilla-fallback', 'logging'],
        'guarding-patches/null-checks-on-game-objects': ['vanilla-fallback'],
        'guarding-patches/warn-once': ['logging'],
        'try-catch': ['exception-handling', 'logging'],
        'try-catch/finalizer-for-a-thread-with-no-handler': ['harmony-patching', 'exception-handling'],
        'yielding-to-other-mods': ['mod-compatibility'],
        'logging': ['logging'],
        'logging/log-levels': [],
        'logging/what-to-include': [],
        'anti-patterns-to-avoid': [],
        'anti-patterns-to-avoid/swallowing-exceptions': ['exception-handling'],
        'anti-patterns-to-avoid/throwing-from-a-patch': ['harmony-patching'],
        'anti-patterns-to-avoid/assuming-the-game-s-internals': ['reflection', 'transpiler', 'harmony-patching'],
        'anti-patterns-to-avoid/silent-degradation': ['logging'],
        'anti-patterns-to-avoid/spamming-the-log': ['logging'],
        'project-specific-patterns': [],
        'related': [],
    },
    'standards/code-style.md': {
        'formatting': [], 'formatting/indentation': [], 'formatting/line-length': [], 'formatting/braces': [],
        'language-and-toolchain': [],
        'imports-includes': [], 'imports-includes/organization': [], 'imports-includes/example': [],
        'comments': [], 'comments/when-to-comment': [], 'comments/when-not-to-comment': [], 'comments/format': [],
        'control-structures': [], 'control-structures/conditionals': [],
        'control-structures/loops': ['performance'],
        'control-structures/strings': ['mod-strings'],
        'harmony-patches': ['harmony-patching'],
        'game-components': ['game-component', 'saved-state'],
        'project-specific-rules': ['mod-options', 'logging', 'in-game-testing'],
        'related': [],
    },
    'standards/naming-conventions.md': {
        'files-and-directories': [], 'files-and-directories/general-rules': [],
        'files-and-directories/examples-by-type': [],
        'code-naming': [], 'code-naming/variables': [],
        'code-naming/functions-methods': ['harmony-patching'],
        'code-naming/classes-types-interfaces': ['harmony-patching'],
        'code-naming/booleans': [],
        'code-naming/strings-modstrings': ['mod-strings'],
        'code-naming/ids-and-tags': [],
        'project-specific-conventions': [], 'related': [],
    },
    'standards/README.md': {'standards': []},
}
README_DOC = ['code-style']

FIX11 = {'motion-sensor-range', 'multichannel-critter-sensor', 'natural-backwalls-everywhere',
         'no-magic-fridges', 'refined-building', 'salty-surf'}
DEVTOOL = {'natural-backwalls-everywhere', 'vent-freeze-fix'}
DEVTOOL_GLOSS = 'A record earns this term when it is about debug and developer features a mod includes for its author.'
DESCRIBE = {
    'dont-count-eggs-on-rails': "Naming conventions for Don't Count Eggs on Rails: files, namespaces, variables, methods, patch classes and ids, plus its patch name, `Eggs` test class, log prefix and `staticID`",
    'just-fall-already': "Naming conventions for Just Fall Already!: files, namespaces, variables, methods, patch classes and ids, plus its `FallPoke` patch names, `Doors` test class, log prefix and `staticID`",
    'no-magic-fridges': "Naming conventions for No Magic Fridges: files, variables, methods, types, ids and tags, plus its nested patch class names, FridgeThermostat fields, test class and log prefix",
    'refined-building': "Naming conventions for Refined Building: files, variables, methods, types, ids and tags, plus its test class, log prefix and staticID",
}

shared_keys = {(k[0], k[1]): ms for k, ms, ts, top in shared}
notes = []
summary = {}


def section_body(path, heading):
    lines = open(path, encoding='utf-8').read().split('\n')
    i = lines.index('## ' + heading) + 1
    out = []
    while i < len(lines) and not lines[i].startswith('#'):
        if not (lines[i].startswith('**Id:**') or lines[i].startswith('**Tags:**')):
            out.append(lines[i])
        i += 1
    return '\n'.join(out).strip('\n')


def tag_op(target, section, cur, want, order):
    add = [t for t in order if t in want and t not in cur]
    rem = [t for t in cur if t not in want]
    if not add and not rem:
        return None
    op = {'mode': 'tag', 'target': target}
    if section:
        op['section'] = section
    if add:
        op['add'] = add
    if rem:
        op['remove'] = rem
    return op


def mod_ops(m):
    node = f'mods/{m}'
    dec = declared[node]
    ops = []
    final = {}
    for doc, canon in CANON.items():
        dtags = set(doctags.get((node, doc), []))
        if doc == 'standards/README.md':
            dtags = set(README_DOC)
        for addr in sorted(canon, key=lambda a: a.count('/')):
            if (doc, addr) not in shared_keys:
                raise SystemExit(f'not shared: {doc}#{addr}')
            if doc != 'standards/README.md' and m not in shared_keys[(doc, addr)]:
                notes.append(f'{m}: {doc}#{addr} differs from the shared text; left alone')
                continue
            anc = set(dtags)
            parts = addr.split('/')
            for j in range(1, len(parts)):
                anc |= set(final.get((doc, '/'.join(parts[:j])), []))
            want = [t for t in canon[addr] if t in dec and t not in anc]
            final[(doc, addr)] = want
            cur = blocktags[(node, f'{doc}#{addr}')]
            op = tag_op(doc, addr, cur, want, canon[addr])
            if op:
                ops.append(op)
            # descendants outside the shared set that would repeat a term
            for (n2, rid), t2 in blocktags.items():
                if n2 == node and rid.startswith(f'{doc}#{addr}/'):
                    sub = rid.split('#', 1)[1]
                    if sub not in canon:
                        dup = set(t2) & set(want)
                        if dup:
                            notes.append(f'{m}: {rid} repeats {sorted(dup)} from its parent {addr}')
    # README document tags
    cur = doctags[(node, 'standards/README.md')]
    op = tag_op('standards/README.md', None, cur, README_DOC, README_DOC)
    if op:
        ops.append(op)
    # overview folder map
    cur = blocktags[(node, 'overview.md#where-things-are')]
    if cur:
        ops.append({'mode': 'tag', 'target': 'overview.md', 'section': 'where-things-are', 'remove': list(cur)})
    if m == 'natural-backwalls-everywhere':
        cur = blocktags[(node, 'domains/README.md#domains')]
        ops.append({'mode': 'tag', 'target': 'domains/README.md', 'section': 'domains', 'remove': list(cur)})
    if m == 'salty-surf':
        # the shared parent's body is empty, so its vanilla-fallback tag was for this child; move it down
        ops.append({'mode': 'tag', 'target': 'standards/error-handling.md',
                    'section': 'project-specific-patterns/salty-surf-specifics', 'add': ['vanilla-fallback']})
    if m == 'upgrade-planner':
        ops.append({'mode': 'tag', 'target': 'domains/entities.md', 'section': 'business-rules', 'remove': ['vanilla-fallback']})
    if m in FIX11:
        body = section_body(f'{ROOT}/mods/{m}/.context/overview.md', 'What this is')
        assert body.count('`mod.yaml` sets') == 1, m
        ops.append({'mode': 'section', 'target': 'overview.md', 'section': 'What this is',
                    'body': body.replace('`mod.yaml` sets', '`mod_info.yaml` sets')})
    if m in DESCRIBE:
        ops.append({'mode': 'describe', 'target': 'standards/naming-conventions.md', 'description': DESCRIBE[m]})
    summary[m] = final
    return ops


def root_ops():
    node = '.'
    return [
        {'mode': 'tag', 'target': 'overview.md', 'section': 'what-this-is', 'remove': ['github-issues']},
        {'mode': 'tag', 'target': 'overview.md', 'section': 'where-things-are',
         'remove': list(blocktags[(node, 'overview.md#where-things-are')])},
        {'mode': 'tag', 'target': 'workflows/task-plan/phase-architecture.md',
         'section': 'phase-exit-handoff-run-last-at-the-phase-boundary', 'remove': ['commit-messages']},
        {'mode': 'tag', 'target': 'domains/tooling-friction.md',
         'section': '2026-10-17-icon-init-exclude-does-not-drop-a-top-level-folder-from-the-node-set',
         'remove': ['common-submodule']},
        {'mode': 'tag', 'target': 'testing/unit-testing.md', 'section': 'framework-and-tools',
         'remove': ['mod-build', 'translations']},
        {'mode': 'tag', 'target': 'testing/unit-testing.md', 'section': 'test-structure/file-organization',
         'remove': ['test-assertions']},
    ]


OUT = f'{ROOT}/.context/tasks/OM-002-store-backfills/changesets'
WHY = ('unit 12 review fixes: one tag set per shared standards section across the mods, no subject tags on '
       'folder maps and index sections, misplaced terms removed, mod_info.yaml named in the overviews, '
       'the developer-tooling gloss without its boundary clause')


def write(name, node, ops):
    cs = {'task': 'OM-002', 'why': WHY, 'node': node, 'ops': ops}
    with open(f'{OUT}/fix12-{name}.json', 'w', encoding='utf-8', newline='\n') as f:
        json.dump(cs, f, indent=1, ensure_ascii=False)
        f.write('\n')
    return len(ops)


if __name__ == '__main__':
    import sys
    print('root', write('root', '.', root_ops()))
    for m in mods:
        print(m, write(m, f'mods/{m}', mod_ops(m)))
        if m in DEVTOOL:
            print(m, 'dict', write(f'{m}-dict', f'mods/{m}', [
                {'mode': 'section', 'target': 'tags.md', 'section': 'developer-tooling', 'body': DEVTOOL_GLOSS}]))
    for n in notes:
        print('NOTE', n)
    json.dump({m: {f'{d}#{a}': v for (d, a), v in f.items()} for m, f in summary.items()},
              open(os.path.join(S, 'final.json'), 'w', encoding='utf-8'), indent=1)
