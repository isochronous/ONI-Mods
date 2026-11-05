# Handoff — OM-011-fridge-thresholds-patch

**Scope:** `mods/fridge-thresholds-extended`
**Store:** `C:/dev/ONI-Mods`
**Task scratch:** `C:/dev/ONI-Mods/.context/tasks/OM-011-fridge-thresholds-patch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-011-fridge-thresholds-patch/units/03-review.md`

```
Task: OM-011 — review Fridge Thresholds Extended
Lens: review (read-only; write only your report). Do not launch the game.
Diff: `git diff b257266..1e1d658` on branch OM-011-fridge-thresholds-patch in C:\dev\ONI-Mods.

Check, with evidence against the real code (thresholds source C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\Steam\2959340381\Source\Storage.cs; decompiles in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\{freezer-src,ir-src,srt-src,vanilla}):
1. Correctness of the Freezer prefix: does it reproduce the thresholds mod's vanilla Refrigerator path (Storage.cs:346-366) exactly — fraction, operational check, LastSetFlag change detection, port id, logic meter? Every reflected member name/signature/binding flag matches the actual types? Return-true fallbacks when the thresholds component is absent?
2. OnCopySettings postfix: right source object, right members, same as Storage.cs:328-344.
3. IR prefix: the five class names exactly match the decompile; null check prevents double-add; does the thresholds mod's own `OnSpawn` work on these prefabs (port 0 is FULL)?
4. Absence safety: with any of the three mods missing, nothing throws and no patch signature names a foreign type (JIT safety). Logs are clear and once.
5. oni-moono constraints from units/01-research-report.md Q5.
6. Repo conventions: compare against mods/vent-freeze-fix and mods/smart-weight-plate (naming, comment density, Mod.cs shape, csproj, mod.yaml/mod_info.yaml, README Workshop style, CHANGELOG style, sln/README/.github additions consistent with commit 88214fb).
7. The implement report's open item: Freezer's thresholds component may spawn after Freezer itself, so the first signal after load could be stale. Judge whether that is real (component order on the Freezer prefab: thresholds adds it in a config postfix) and whether the proposed fix is right.

Report: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\03-review-report.md — findings ranked by severity, each with file:line, failure scenario, and a suggested fix; say plainly what you verified vs inferred. If none are blocking, say so.
Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods

⚠ 2 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: mods:domains/game-interfaces.md, mods:overview.md. Give them `applies_to` or tags through the write path.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
