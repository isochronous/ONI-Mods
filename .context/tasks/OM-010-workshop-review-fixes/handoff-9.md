# Handoff — OM-010-workshop-review-fixes

**Scope:** `mods/vent-freeze-fix`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-010-workshop-review-fixes/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-010-workshop-review-fixes/units/09-implement.md`

```
# Unit 09 — implement: Vent Freeze Fix tooltip translations

Ticket OM-010. The English Repair TOOLTIP in mods/vent-freeze-fix/src/VentFreezeFix/ModStrings.cs changed in aa2fae7 (the watcher now is described as running for the first second after a liquid vent spawns, on load or when built). Bring the .pot and the ko, ru and zh .po files up to date for that string, following how the repo's translation workflow does it (see the mod translations system: `python common/tools/Translations/translations.py <mod-folder> --build`; write .po files with the Write/Edit tools, never shell heredocs). Use the game's own terms in each language, matching the existing lines in those files.

Acceptance criteria:
  - `python common/tools/Translations/translations.py mods/vent-freeze-fix --build` reports ALL CHECKS PASSED — output in report.
  - Touch only mods/vent-freeze-fix/. Do not launch the game.
  - Commit with one terse line `OM-010: Vent Freeze Fix tooltip translations`, trailers `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>` and `Claude-Session: https://claude.ai/code/session_01SuoJMc2zs26eqoCJMtJzCm`.

Report: File .context/tasks/OM-010-workshop-review-fixes/units/09-implement-report.md (commit it with the work); the three new translated lines and the check output. Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-00fe6a155298 · reference
  Examples by Type
  mods/dont-count-eggs-on-rails/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-01b48de8fa57 · reference
  Examples by Type
  mods/motion-sensor-range/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-0b089ce1d390 · reference
  Examples by Type
  mods/smart-weight-plate/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-0c7eed225774 · reference
  Project-Specific Rules
  mods/upgrade-planner/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-0fc5ffaf284d · reference
  Examples by Type
  mods/refined-building/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-19a06aa47b06 · reference
  Examples by Type
  mods/upgrade-planner/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-2963fe01febc · reference
  Examples by Type
  mods/salty-surf/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-447262e58ac0 · reference
  Project-Specific Rules
  mods/no-magic-fridges/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-44860daaca69 · reference
  Examples by Type
  mods/sweep-zones/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-4cf063d3f818 · reference
  Project-Specific Rules
  mods/refined-building/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-5157bbf172ab · reference
  Examples by Type
  mods/storage-pod-redux/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-550895a93c83 · reference
  Project-Specific Rules
  mods/smart-weight-plate/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-67f23c4b455d · reference
  Project-Specific Rules
  mods/multichannel-critter-sensor/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-701ebf962f66 · reference
  Examples by Type
  mods/detect-any-element/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-7ebeb4864e2b · reference
  Examples by Type
  mods/vent-freeze-fix/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-8c7f61bf806a · reference
  Project-Specific Rules
  mods/natural-backwalls-everywhere/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-9543e106622d · reference
  Project-Specific Rules
  mods/sweep-zones/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-966ebe0cbf67 · reference
  Project-Specific Rules
  mods/jumper-bridges/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-9b2fe98dd4dd · reference
  Examples by Type
  mods/just-fall-already/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-9d74aee10321 · reference
  Project-Specific Rules
  mods/just-fall-already/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-a8ba49e2c00b · reference
  Examples by Type
  mods/natural-backwalls-everywhere/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-ae5190625f1d · reference
  Project-Specific Rules
  mods/corner-build-anything/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-bce469611e0a · reference
  Examples by Type
  mods/jumper-bridges/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-c63d3276e032 · reference
  Examples by Type
  mods/no-magic-fridges/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-c963897629f3 · reference
  Examples by Type
  mods/corner-build-anything/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-cdec4d90d437 · reference
  Examples by Type
  mods/multichannel-critter-sensor/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-d691a716fef2 · reference
  Project-Specific Rules
  mods/detect-any-element/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-d6a5f3394410 · reference
  Project-Specific Rules
  mods/storage-pod-redux/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-df727bd6b290 · reference
  Project-Specific Rules
  mods/salty-surf/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-dfc2718274e5 · reference
  Examples by Type
  mods/bitshifter/.context/standards/naming-conventions.md#files-and-directories/examples-by-type · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-e8781e877267 · reference
  Project-Specific Rules
  mods/bitshifter/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-ed4d88eb38f6 · reference
  Project-Specific Rules
  mods/dont-count-eggs-on-rails/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-eebace797a67 · reference
  Project-Specific Rules
  mods/vent-freeze-fix/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
blk-f4dd752ab3b9 · reference
  Project-Specific Rules
  mods/motion-sensor-range/.context/standards/code-style.md#project-specific-rules · names mods/vent-freeze-fix/src/VentFreezeFix/Options.cs → mods/vent-freeze-fix
mods/vent-freeze-fix:domains/entities.md · reference
  The insulated liquid vent storage patch, post-load repair watcher, dig requests, options, debug…
  mods/vent-freeze-fix/.context/domains/entities.md · src/VentFreezeFix/** → mods/vent-freeze-fix · linked from domains/glossary.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

**Truncated.** 4 further candidate(s) matched and are not listed above (8091 of 8192 bytes; the whole selection is 9413). See the rest with:

    ks brief --at C:/dev/ONI-Mods --for mods/vent-freeze-fix --budget 9413

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods/vent-freeze-fix:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
