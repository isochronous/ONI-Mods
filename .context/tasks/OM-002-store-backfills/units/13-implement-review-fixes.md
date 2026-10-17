# Unit 13 — implement: draft change-sets for the unit 12 review fixes

## Objective
Draft change-sets that resolve every `fix` finding in `units/12-review-report.md`. The manager plans and applies them. Plan step: "Review fixes".

## Decisions already made
- **F1.1:** the six overviews name `mod_info.yaml` (not `mod.yaml`) for `supportedContent` and `minimumSupportedBuild`. `section` writes on "What this is" or wherever the claim sits; change only that claim.
- **F3.1:** rewrite the `developer-tooling` gloss in every node that declares it to one positive sentence with no boundary clause ("A record earns this term when it is about debug and developer features a mod includes for its author."). A `section` write on that term's block in `tags.md` replaces the body; the writer keeps the Id line.
- **F3.2 / F4.3:** folder-map and index sections ("Where things are" in every overview, the README index sections the review names) are not about the subjects of the files they list. Remove every subject tag from them. Keep `mod-entry-point` as a term; only its use on folder maps goes.
- **F4.1 / F4.2 / F4.4:** sections of the shared standards body with identical text must carry the same tags in every mod. For each such section (match by document path and heading address across the 17 mods), choose ONE canonical tag set from what the section's own text is about — never from the cross-mod examples it quotes — using terms whose glosses fit; then make every mod's copy carry exactly that set restricted to the terms its node's `tags.md` declares, with `add`/`remove` tag ops. A term the document or an enclosing heading already carries is not repeated on the section. The per-mod "<Mod> specifics" sections are not shared and keep their own tags unless the review names them. Do the same for the 17 `standards/README.md` documents (one canonical document tag set: `code-style` plus whatever the shared index genuinely is about).
- **F4.5–F4.8:** remove the named misplaced terms.
- Nits: leave them, except the four `naming-conventions` descriptions that name `ModStrings` in mods with none — fix those with a `describe` op if the schema offers one (`ks write schema --row narrative`), otherwise list them in the report.

## Mechanics
- Shapes: `node c:/dev/ai/icon/bin/ks.mjs write schema --row tags --mode tag`, `--row narrative --mode section`. A tag op names its section by its address (the part after `#` in a record id). Two ops on one destination block in one change-set are refused; merge them.
- One change-set per node: `changesets/fix12-<node-name>.json` (`fix12-root.json` for `.`), envelope `task: "OM-002"`, a `why`, `node`.
- Run `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at .` from `C:/dev/ONI-Mods` for every file until accepted. Never apply, never commit.
- Write JSON with a script (keep it in a uniquely named scratchpad subfolder) or the Write tool, never a heredoc.

## Report
- File: units/13-implement-review-fixes-report.md
- Must contain: the canonical tag set chosen for each shared section and why; per finding, the change-sets that resolve it; the `write plan` result per file; anything left unresolved; friction met.
- Model tier: complex
