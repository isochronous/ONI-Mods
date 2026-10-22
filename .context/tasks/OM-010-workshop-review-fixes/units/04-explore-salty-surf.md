# Unit 04 report: Salty Surf element in the sandbox

Model tier: complex (the spec said default). Read only: no code changes, no game launch, no commits.
Game code was decompiled with ilspycmd 9.1 from `F:/SteamLibrary/.../Managed/Assembly-CSharp.dll` and `Assembly-CSharp-firstpass.dll`, as installed on 2026-10-22. Decompiled line numbers are approximate.

## Verdict: false as stated. A related, smaller defect is real.

**The claim** is that Salty Surf creates a custom liquid element with no name string, and that the element shows in the sandbox element picker with a missing-string label.

- **No element.** The mod adds no element in any version. `mods/salty-surf/src/SaltySurf/` holds only `Mod.cs`, `ModStrings.cs`, `mod.yaml`, `mod_info.yaml` and `translations/`. There is no `elements/*.yaml`, no `Substance`, and no `ElementLoader` registration. The tags `salty-surf-v0.1.0` and `salty-surf-v0.2.0` have the same file set, and `git log -S Substance` over `mods/salty-surf` returns nothing.
- **What the mod does create** is a tag, not an element: `Mod.cs:30` `public static readonly Tag Tag = TagManager.Create("SaltySurfWater");`. `Mod.cs:47-58` appends that tag to the `oreTags` of Water and Salt Water, which are vanilla elements.
- **Sandbox picker.** The list is built in `SandboxToolParameterMenu.ConfigureElementSelector` (decompile around lines 457-479). It takes every `ElementLoader.elements` entry that is not `disabled` and has no `GameTags.HideFromSpawnTool` in its `oreTags`. Each entry's label is `element.name + " (" + GetStateString() + ")"`. The mod adds no entry to that list. Water and Salt Water keep their vanilla names there, and the mod's tag never appears in the picker.

**The real defect.** The missing string does exist, but it appears in the element description, not in the sandbox picker:

- `Element.FullDescription` (decompile `Element.cs` around lines 245-261) prints `Properties: {0}`. Here `{0}` is every tag in `oreTags` converted with `Tag.ProperName()`, skipping only the tags in `GameTags.HiddenElementTags`.
- `TagManager.Create(string)` with no proper name stores `""`. `Game.OnSpawn` then calls `TagManager.FillMissingProperNames()` (`Game.cs` around line 952). That function fills each empty name with `Strings.Get("STRINGS.MISC.TAGS." + name.ToUpper())`.
- `STRINGS.MISC.TAGS.SALTYSURFWATER` does not exist, so `Strings.GetInvalidString` returns `MISSING.STRINGS.MISC.TAGS.SALTYSURFWATER`.
- Result: the Water and Salt Water descriptions read roughly `Properties: Any Water, Liquid, MISSING.STRINGS.MISC.TAGS.SALTYSURFWATER`.
- This text shows in two places:
  - the codex pages for Water and Salt Water (`CodexEntryGenerator_Elements.cs:255`)
  - the selected-object info panel description for any cell, debris or bottle of Water or Salt Water (`SimpleInfoScreen.cs:649` and `:654`). The sandbox's own hover and select also go through this panel, which may be what the other developer saw.

This conclusion comes from reading the code and has not been checked in game.

**Cost.** One wrong-looking word appears on two very common elements, on their codex pages and in the info panel. There is no gameplay, save or performance effect. The tag is not saved: chunks get it from the element definition when they load. Water is the most-inspected liquid in the game, so many players will see the text.

**What the tag is for.** It is the mod's single mechanism. It drives the pipe intake (`ConduitConsumer.capacityTag`, `Mod.cs:61-70`), the readiness mass check (`Mod.cs:73-81`) and the spill draw (`Mod.cs:87-108`). The game copies element tags onto every stored chunk, so tagging the element once covers all three. Removing the tag would mean rewriting the mod.

## Hiding options

The question was how to hide the label, and what each way of hiding affects.

| Way | What it hides | Applies here? |
|---|---|---|
| `GameTags.HideFromSpawnTool` in an element's `oreTags` | the element from the sandbox element picker, and its prefabs from the sandbox spawner (`SandboxToolParameterMenu` ~465-469 and ~614) | No. There is no mod element to hide, and putting this tag on Water would remove Water from the sandbox. |
| `GameTags.HideFromCodex` | the element's codex page (`CodexEntryGenerator_Elements.cs:84-88`) | No, for the same reason. |
| `Element.disabled` (from yaml `isDisabled`) | the element from the sandbox, the codex and most element lists | No, for the same reason. |
| **Add the mod's tag to `GameTags.HiddenElementTags`** | only that tag's name in the `Properties:` line. `HiddenElementTags` (`GameTags.cs:1096`, a static mutable `TagSet`) is read in exactly one place, `Element.FullDescription` | **Yes. This is the fix.** |

## Proposed fix (smallest, keeps all behaviour)

The fix is in `Mod.cs`, in the `ElementLoader_Load_Patch.Postfix`:

1. Call `GameTags.HiddenElementTags.Add(SurfWater.Tag);` once.
2. Put the tag at the **front** of `oreTags` instead of appending it, for example `element.oreTags = new[] { SurfWater.Tag }.Concat(element.oreTags).ToArray()` or an equivalent array copy. The reason is a vanilla quirk: the `FullDescription` loop writes `", "` after every tag except the last one, and it does this before it checks the next tag. A hidden tag in last place would leave a trailing `"Any Water, Liquid, "`. A hidden tag in first place adds nothing, and the rest of the line prints exactly as in vanilla.

**Reach.**
- `HiddenElementTags` has no other reader in Assembly-CSharp.
- No game code reads `oreTags` by position: a grep for `oreTags[0]`, `oreTags[1]` and `oreTags.First` found nothing. Every other reader does a membership test (`HasTag`, `Array.IndexOf` in `Recipe.cs:93` and `:268`, the codex and sandbox filters) or copies the whole array (`PrimaryElement.cs:413`). Moving the tag first therefore changes no game behaviour.
- Another mod that indexes `oreTags[0]` would be affected. None is known.

**Behaviour change.** The only visible change is that the bogus entry disappears from the Properties line on Water and Salt Water. Intake, readiness, spill, the info panel requirement line and saves are unchanged.

**Fallback (not recommended).** If hiding were not wanted, the mod could name the tag by adding `ModStrings.MISC.TAGS.SALTYSURFWATER` (`public static class MISC { public static class TAGS { ... } }` inside `ModStrings`). `Isochronous.Translations` registers `ModStrings.X.Y` as `STRINGS.X.Y` (`common/src/Translations.cs:15-19`, `:31`, `:51`), and `FillMissingProperNames` would then pick the name up. It would need entries in `SaltySurf.pot`, `zh.po`, `ko.po` and `ru.po`. A wording that fits Jeremy's ask would be "Surfboard liquid". It is not recommended because it puts a mod-internal word on two vanilla elements' Properties line, while hiding removes the word altogether. There is no element to rename, so the "don't paint it into the map" naming does not apply.

## Tests

No existing `[OniTest]` covers this. `Intake.Tags` (`src/SaltySurf.Tests/Intake.cs:11-22`) checks only `HasTag` and the intake tag.

One test would cover it, in `Intake.cs` with Layer(0) in a loaded game, because `FillMissingProperNames` runs in `Game.OnSpawn`. For Water and Salt Water it would assert that:
- `FullDescription()` contains neither `MISSING` nor `SALTYSURFWATER`
- the `Properties:` line does not end in `", "`
- `HasTag(SurfWater.Tag)` is still true

## Store records

No record is wrong against the code. These were checked:
- blk-5c09c48ae91d
- blk-eab76ffa269c
- blk-0d5a13c73107
- blk-858e2835d046
- blk-99d39a8ff721
- blk-cabaabc55d71
- blk-7c1deca7e340
- blk-ac3b96f5c9c1

There is one gap. Nothing records that a tag created without a proper name surfaces as `MISSING.STRINGS.MISC.TAGS.<NAME>` in element descriptions, or that `HiddenElementTags` is the way to keep a functional tag out of them. That would belong in glossary blk-ac3b96f5c9c1 (oreTags) or in the Business Rules block blk-cabaabc55d71 once the fix lands.

## Handoff pointer feedback

- The 34 `code-style` and `naming-conventions` example blocks are 17 copies of the same two examples, one per mod. They matched only because they name `Mod.cs` or `ModStrings.cs` as an example path. Only the salty-surf copies were worth opening, and neither governs this work.
- `entities.md` and `glossary.md` applied.
- Nothing governing this work was missing.
