# Handoff — OM-009-jumper-heavy-wire

**Scope:** `mods/jumper-bridges/src`, `mods/jumper-bridges/README.md`, `mods/jumper-bridges/CHANGELOG.md`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-009-jumper-heavy-wire/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-009-jumper-heavy-wire/units/06-implement.md`

```
# Unit 06 — implement: let wire bridge ends and joint plate ends share a cell

Step: reverse the no-sharing rule. Ticket: OM-009 (reopened; branch OM-009-jumper-heavy-wire, PR #13).

Jeremy's new decision (do not revisit): a Wire Bridge, Conductive Wire Bridge or Insulated Wire Bridge end and a Heavi-Watt (Conductive) Joint Plate end MAY share a cell, in either order of building. The joined circuit takes the lowest rating and can overload the wire bridge; that is the player's choice. A tile may not be built over such a shared end (including over the wire bridge's end when the plate is the one sharing it). All other decisions in plan.md stand.

Files to modify: mods/jumper-bridges/** as needed —
  - Patches.cs: drop the refusals both ways (IsValidWireBridgeLocation prefix and IsValidHighWattBridgeLocation prefix, the `jointPlate` filtering in PowerPorts.HasEnd / BridgeFootprint.AnyPowerEndAt as it applies). Make sure the IsValidTileLocation refusal covers a shared end whichever bridge was built first and whether or not it holds the WireConnectors slot (see units/01-explore-findings.md Q3 item 3). Simplify any code that existed only for the no-sharing rule.
  - JointPlates.cs: replace NotOnWireBridgeEnd with one test that a plate end on a wire bridge end is valid and both are on one circuit, and that a tile on that shared end is refused. One case per behaviour; paced like the others.
  - Player text: option tooltip (ModStrings.cs + .pot + zh/ko/ru .po), README, CHANGELOG Unreleased bullet, publish/workshop-description.txt — state the rule plainly (ends may share with any power bridge end; no tile over a plate end). No mechanics, no hedging, no warnings about overload beyond a plain fact if the existing text style uses one. Rebuild, then `python common/tools/Translations/translations.py mods/jumper-bridges --build`.
Pattern to follow: existing code and tests in the same files.
Prior reports: units/01-explore-findings.md, 02-, 03-review-findings.md, 04-implement-report.md, 05-review-findings.md.

Acceptance criteria:
  - `dotnet build ONI-Mods.sln -c Release -p:ModDeployFolder=none` succeeds — output tail in report.
  - translations.py check passes — output in report.
  - Do NOT launch the game.
  - Commit all modified files with one terse line, e.g. `OM-009: plate and wire bridge ends may share a cell`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`. Do not push.

Report:
  - File: .context/tasks/OM-009-jumper-heavy-wire/units/06-implement-report.md
  - Must contain: per-file change summary, test names changed, build and translations output verbatim.
  - Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-a0f2a41a1467 · reference
  A `KAnimGraphTileVisualizer` on a standalone GameObject (no building) that stands in for a wire…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/bridgeendnode · names mods/jumper-bridges/src/JumperBridges/BridgeEndNode.cs → mods/jumper-bridges/src
blk-f00f2e956606 · reference
  Registry of where every bridge is, by cell, on the complete and the under-construction prefab.…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/bridgefootprint · names mods/jumper-bridges/src/JumperBridges/BridgeFootprint.cs → mods/jumper-bridges/src
blk-e978d080073f · reference
  What differs between automation and power: the network manager, layers and wire art. Two instan…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/bridgekind · names mods/jumper-bridges/src/JumperBridges/BridgeKind.cs → mods/jumper-bridges/src
blk-67d61b61d321 · reference
  Owns the network manager's link table for groups of bridges that share end cells. The manager k…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/bridgelinks · names mods/jumper-bridges/src/JumperBridges/BridgeLinks.cs → mods/jumper-bridges/src
blk-c5d470758241 · reference
  `KMonoBehaviour` on every complete bridge of an enabled kind. Gives each end that has no wire u…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/jumperbridge · names mods/jumper-bridges/src/JumperBridges/JumperBridge.cs → mods/jumper-bridges/src
blk-10505badb72b · reference
  Line Length
  mods/detect-any-element/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-2dbc3fdcc629 · reference
  Line Length
  mods/multichannel-critter-sensor/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-45611b9a4ffc · reference
  The `UserMod2` entry point. Loads `Options` before `base.OnLoad` (which applies the patches) an…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/jumperbridgesmod · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-527ddcf84ecb · reference
  Line Length
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-5f5635f26138 · reference
  Line Length
  mods/vent-freeze-fix/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-7cc833c742eb · reference
  Line Length
  mods/jumper-bridges/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-81477512dcc1 · reference
  Line Length
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-852815eed994 · reference
  Key Code Paths
  mods/jumper-bridges/.context/domains/entities.md#key-code-paths · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-8f17aea2dd9c · reference
  Line Length
  mods/smart-weight-plate/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-9f56da6c281c · reference
  Line Length
  mods/just-fall-already/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-a120aa101a26 · reference
  Line Length
  mods/refined-building/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-a24a70111ef7 · reference
  Line Length
  mods/motion-sensor-range/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-a92de9fa26ec · reference
  Line Length
  mods/corner-build-anything/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-c02be4756c7c · reference
  Line Length
  mods/sweep-zones/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-c130e4cc3f34 · reference
  Line Length
  mods/storage-pod-redux/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-c900cc8f62e1 · reference
  Line Length
  mods/upgrade-planner/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-d4cd6738c9b6 · reference
  Line Length
  mods/no-magic-fridges/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-e3f6da6e58eb · reference
  Line Length
  mods/salty-surf/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-f9bb4b2c4543 · reference
  Line Length
  mods/bitshifter/.context/standards/code-style.md#formatting/line-length · names mods/jumper-bridges/src/JumperBridges/Mod.cs → mods/jumper-bridges/src
blk-f92ca7ddeb23 · reference
  PLib options saved in the shared config location, both restart-required.
  mods/jumper-bridges/.context/domains/entities.md#key-entities/options · names mods/jumper-bridges/src/JumperBridges/Options.cs → mods/jumper-bridges/src
blk-183bc6e8000f · reference
  Harmony Patches
  mods/motion-sensor-range/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-1ebc400095cd · reference
  Harmony Patches
  mods/corner-build-anything/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-27d655025249 · reference
  Harmony Patches
  mods/jumper-bridges/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-441857351745 · reference
  Harmony Patches
  mods/vent-freeze-fix/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-45b3c9a86343 · reference
  Harmony Patches
  mods/storage-pod-redux/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-516da110cf84 · reference
  Harmony Patches
  mods/smart-weight-plate/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-568d224900b6 · reference
  Harmony Patches
  mods/detect-any-element/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-6d43d7004a79 · reference
  Harmony Patches
  mods/salty-surf/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-a88b0d39bc11 · reference
  The game classes and methods patched, grouped by purpose, all in `mods/jumper-bridges/src/Jumpe…
  mods/jumper-bridges/.context/domains/entities.md#key-entities/harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-bcc50465e110 · reference
  Harmony Patches
  mods/just-fall-already/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
blk-c3e49ad21d32 · reference
  Harmony Patches
  mods/bitshifter/.context/standards/code-style.md#harmony-patches · names mods/jumper-bridges/src/JumperBridges/Patches.cs → mods/jumper-bridges/src
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 13 further candidate(s) matched and are not listed above (8140 of 8192 bytes; the whole selection is 11447). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/jumper-bridges/CHANGELOG.md mods/jumper-bridges/README.md mods/jumper-bridges/src --budget 11447

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/jumper-bridges:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
