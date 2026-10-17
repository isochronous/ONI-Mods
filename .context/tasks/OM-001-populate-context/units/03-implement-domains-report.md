# Unit 03 report — domains change-sets

Change-sets are in `.context/tasks/OM-001-populate-context/changesets/domains-<mod>.json`, 2 create ops each (`domains/entities.md`, `domains/glossary.md`). Nothing applied, nothing committed. Body sources and the generator (`build.py`) are in the session scratchpad `.../scratchpad/d3/`.

## Per mod

- bitshifter: Ribbon Reader/Writer select bits 1-31, selected bit drawn on the buildings, NOT gate inverts 31 bits. entities: BitshifterMod and yield rules, bit depth patches, Display kanim/segments, scroll view side screen, NOT gate transpiler, OverlayLabelGuard. glossary: ribbon, bit depth, selected bit, ONICPU/BAO, stand down, bit 0/32 terms to avoid.
- corner-build-anything: build/deconstruct any building through a diagonal gap. entities: the two OffsetGroups tables, Constructable/Deconstructable OnSpawn transpilers, SwapTable. glossary: reach table, standard/corner table, corner pocket, footprint, offset filter.
- detect-any-element: Any choice under None on the five element sensors, asterisk flash. entities: AnyElement tag, ModStrings, FilterSideScreen/ConduitElementSensor/LogicElementSensor patches, AnyGlyph, config postfix. glossary: Any, element sensor, Filterable, steadiness window, symbol override.
- dont-count-eggs-on-rails: rail eggs excluded from critter sensor counts. entities: OvercrowdingMonitor.UpdateRegion prefix and the rail-egg test (Stored tag, null storage). glossary: critter sensor, cavity, room prober, conveyor rail, stored tag.
- jumper-bridges: bridges placed with ends on ports (automation and power). entities: mod/options, BridgeKind, JumperBridge, BridgeEndNode, BridgeFootprint, BridgeLinks, patch groups, rules, code paths, tests. glossary: bridge end/node, utility network, link ring, object layer, joint plate, wire drag, overload, overlay.
- just-fall-already: door-armed cells trigger immediate critter fall checks. entities: FallPoke, Door.SetWorldState patch, armed cells, rules, tests. glossary: pez dropper, poke, armed cell, brain scheduler, nav grid/type, Critters Fall Through Open Doors, Fast Track.

State-transition sections were cut in all six (no mod has states). No `applies_to` or tags set.

## TODO(owner)s left
None.

## write plan per file (each: exit accepted, "plan only")
All six printed:

    what it writes
      create  .context/domains/entities.md   by the narrative writer of record
      create  .context/domains/glossary.md   by the narrative writer of record
    what the writers report
      narrative  domains/entities.md created
      narrative  domains/glossary.md created
    plan only — nothing was written.

## Friction
- The system's auto-mode text told me to prefer Bash over Write; memory says never to write files with heredocs. I used Write for the markdown and a Python script for the JSON.
- The templates are shaped for one domain-area file plus a project-wide glossary; for a single-purpose mod the glossary template's "Cross-Domain Terms" heading has no natural meaning. I kept the skeleton.
- The handoff brief returned "Nothing matched this scope", so no pointers were read.
- Unverified claims: ONICPU is described as "32-bit" per the Bitshifter README; the jumper-bridges overlay and `incubator/` references come from its README and were not opened.
