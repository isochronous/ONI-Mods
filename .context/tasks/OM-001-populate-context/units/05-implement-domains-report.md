# Unit 05 report: domains change-sets for five mods

Files: `.context/tasks/OM-001-populate-context/changesets/domains-<mod>.json`, one per mod, 2 create ops each (`domains/entities.md` with `applies_to: ["src/<Project>/**"]`, `domains/glossary.md` with no coverage). No tags. Nothing applied, nothing committed.

## Per mod

- smart-weight-plate: a weight plate with low/high thresholds and a latch. Entities: the building and its config, the `SmartWeightPlate` component (saved fields, masses, latch), side screen, five Harmony targets (including the optional Better Automation Overlay postfix), business rules (weighing, inclusive latch, equal-threshold case, 0.2.0 invert migration), code paths, tests. Glossary: High/Low Threshold, reservoir semantics, FloorSwitchActivator, Pickupable, settings version; avoid "activate/deactivate threshold" and "pressure plate".
- storage-pod-redux: Storage Pod and Cool Pod, 1x1 buildable anywhere. Entities: both configs, Options, `Buildings` decision state, `ModDetection`/Ronivan's Fridge Pod check, three patches, rules (original prefab IDs, original-mod stand-down, Auto mode, registration gate). Glossary: Pod, Mini Fridge, Anywhere rule, Kitchen refrigerator tag, Full output, prefab/static ID; avoid "Fridge Pod" and ambiguous "Storage Pod mod".
- sweep-zones: painted sweep/mop zones applied every second. Entities: `ZoneStore`, `SweepZoneTool` and `ZoneMode`, `ToolIcon`, hotkey, four patches, rules (valid zone cell, marking exclusions, mop limits, pruning, display without overlays). Glossary: sweep, mop, debris, PrioritySetting, DragTool, ToolParameterMenu; avoid "overlay" for the display and erase-vs-cancel confusion.
- upgrade-planner: box-drag rebuild tool with Change material and Replace type tabs. Entities: `UpgradeTool`, plans and `Matcher`/`Upgrade`, `Family`, rules, `Orders`/`PlacementQueue`, presets file, window, compat classes (Blueprints Expanded, Suppress Notifications, No Manual Delivery, Adaptive Building Menu), hotkey, six patches plus the compat prefixes. Glossary: replacement layer/tags/candidate, utility building, Reconstructable, MaterialSelectionPanel, instant build, variant; avoid "upgrade" as a game action and "reconstruct" for building over.
- vent-freeze-fix: repair of tiles formed at liquid vents after loading. Entities: insulated vent storage patch, `VentFreezeWatcher` (saved snapshots), `DigRequests`, Options, `DebugSave`, six patch targets, rules (80% rule, verdict conditions, landing cell, one-second window). Glossary: packet, super-cooled liquid, phase transition handler, insulated storage modifiers, dig, foundation tile; avoid "fix" for the tile conversion (it is "repair").

## TODO(owner)s left

None. Sections that cannot apply were cut: State Transitions in all five (no mod has states beyond the SWP latch, which is covered under rules).

## `write plan` summary (run from C:/dev/ONI-Mods, `--at .`)

All five accepted, identical shape, for example:

    task   OM-001
    node    C:/dev/ONI-Mods/mods/<mod> [aggregator]
    branch  OM-001-populate-context
    create  .context/domains/entities.md   by the narrative writer of record
    create  .context/domains/glossary.md   by the narrative writer of record
    narrative  domains/entities.md created
    narrative  domains/glossary.md created
    plan only — nothing was written.

Run for smart-weight-plate, storage-pod-redux, sweep-zones, upgrade-planner and vent-freeze-fix; each printed the two `created` lines and no refusal.

## Friction

- `write plan` labels each mod node `[aggregator]` although these are leaf mods. Plan accepted anyway; no action taken.
- The brief for this unit matched nothing and listed 15 records without coverage or tags; not acted on (out of scope).
- Generation used a script in the scratchpad (`.../scratchpad/cs/gen.mjs`) that reads the markdown bodies and writes the JSON, to avoid escaping. Not part of the store.
