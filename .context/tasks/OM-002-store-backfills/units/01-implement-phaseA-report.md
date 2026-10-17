# Unit 01 report — Phase A, root and mods overviews

## What went in
- Root (`changesets/phaseA-root.json`, node `.`): What this is — the monorepo of 17 isochronous ONI mods, one solution, shared build kit via the `common/` submodule, released per mod as `<mod>-v<version>`, history merged Oct 2026. Where things are — mods/, common/ (and its tools), ONI-Mods.sln, Directory.Build.props, README.md, .github, .claude/commands/release-mods.md, git plumbing, .context documents; notes there is no top-level tools/ or scripts/.
- Mods (`changesets/phaseA-mods.json`, node `mods`): What this is — shape of a mod folder plus one line per each of the 17 mods from its README. Where things are — per-mod folder layout (src, Tests, publish, README, CHANGELOG, tools in 5 mods), .context pointer.

## TODO(owner) left
None.

## write plan summaries
- phaseA-root.json: accepted, plan only. update .context/overview.md; "What this is" 1 line/92 B removed, +3 lines/+759 B; "Where things are" 1 line/59 B removed, +11 lines/+1478 B.
- phaseA-mods.json: accepted, plan only. update .context/overview.md; "What this is" 1 line/92 B removed, +21 lines/+2613 B; "Where things are" 1 line/59 B removed, +6 lines/+614 B.

## Friction
- Packet's brief pointed 15 identical try/catch standards records (other mods' error-handling.md) that do not apply to overview work; nothing governing was missing. The 8 KB brief was spent on those.
- A python heredoc through Bash did not apply a `\n`-escaped replacement (backslashes collapse, as memory notes); fixed with the Edit tool.
- `write schema` for the changeset envelope is long; the row-mode schema alone does not show the `ops` array wrapper, I took it from the envelope schema.
