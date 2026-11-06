# Unit 02 report: design, the Ribbon Diagnostic Panel in Bitshifter

Design only; no repo file changed, nothing written to the store. Game build 744825, decompiled code from the unit 01 scratch (`scratchpad/all`, `scratchpad/dec`, `scratchpad/fp`). Mid-task name decision from the coordinator applied throughout: the player-visible name is **Ribbon Diagnostic Panel**, the kanim keeps the name `diagnostic_panel` (no rename; question 6 drops the rename). Paths under `mods/bitshifter/src/Bitshifter/` are shortened to `B/`.

Scratch outputs for the coordinator: `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/57fa2d13-c310-4f59-90ad-9f226f41d92c/scratchpad/om008-design/grid-directions.png` (mock of the three palette directions for the switch grid, wired and unwired), made by `.../scratchpad/om008-design/mock_grid.py`. Flat rectangles at the planned sizes, not game rendering.

## Recommendation

**Approve with modifications.**

The change as proposed holds together. The modifications:

1. **Switch UI: the PLib grid, not the list.** The list fallback looks cheap but breaks when Bitshifter stands down (section 5). The grid needs one patch and reuses repo patterns already in place.
2. **Split the patches with Harmony categories** (`[HarmonyPatchCategory]`, present in the game's 0Harmony 2.4.2: I checked `PatchCategory(Assembly, string)` and `PatchAllUncategorized(Assembly)` by reflection on `OxygenNotIncluded_Data/Managed/0Harmony.dll`). The toolkit patches apply in `OnLoad`, like every other mod in the repo. The Reader/Writer patches keep their late, conditional `OnAllModsLoaded` application.
3. **The output always carries input OR switches, wired or not.** Only the lamps and the controls ignore the switches while the output is unwired. That way attaching a wire gives the right value at once, with no 200 ms pulse of the wrong value (section 3).
4. **Off lamps are hidden, not tinted dark.** The body already draws a bevelled socket under each lamp.

## 1. Structure in Bitshifter

**Files** (new ones in a `Toolkit/` subfolder; namespace stays `Bitshifter`, as `mods/upgrade-planner/src/UpgradePlanner/UI/*.cs` keeps the root namespace):

| File | Holds |
|------|-------|
| `B/Toolkit/Toolkit.cs` | `internal static class Toolkit`: `PatchCategory = "BitshifterToolkit"`, plus the table of toolkit buildings (`Entry(string id, string subcategory, string after, string tech)`), one row per building |
| `B/Toolkit/ToolkitPatches.cs` | `public static class ToolkitPatches` with nested, `[HarmonyPatchCategory(Toolkit.PatchCategory)]`-marked patch classes: `GeneratedBuildings_LoadGeneratedBuildings_Patch` (prefix, loops the table, `ModUtil.AddBuildingToPlanScreen`), `Db_Initialize_Patch` (postfix, loops the table, `Techs.TryGet` + warn-and-continue as in `mods/multichannel-critter-sensor/.../Patches.cs:38-56`), `DetailsScreen_OnPrefabInit_Patch` (postfix, `PUIUtils.AddSideScreenContent<RibbonDiagnosticPanelSideScreen>()`) |
| `B/Toolkit/RibbonDiagnosticPanelConfig.cs` | `public sealed class RibbonDiagnosticPanelConfig : IBuildingConfig`, `public const string ID` |
| `B/Toolkit/RibbonDiagnosticPanel.cs` | the component (state, logic, lamps, status items, copy settings) |
| `B/Toolkit/RibbonDiagnosticPanelSideScreen.cs` | `public sealed class RibbonDiagnosticPanelSideScreen : SideScreenContent, IRender200ms` (PLib) |
| `B/ModStrings.cs` | the mod's first strings (section 4) |
| `B/Mod.cs` | changed (below) |
| `B/translations/Bitshifter.pot`, `zh.po`, `ko.po`, `ru.po` | new |
| `B/anim/assets/diagnostic_panel/` | the kanim (section 6) |

Existing files stay where they are: `Patches.cs`, `SideScreen.cs` and `OverlayLabelGuard.cs` are the Reader/Writer part.

**Mod.cs:**

```csharp
public override void OnLoad(Harmony harmony)
{
	PUtil.InitLibrary(false);
	Translations.Register(harmony, typeof(ModStrings));
	harmony.PatchCategory(typeof(BitshifterMod).Assembly, Toolkit.PatchCategory);
	Debug.Log("[Bitshifter] Loaded version " + ...);
}

public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
{
	OverlayLabelGuard.Apply(harmony);
	string reason = WhyYield(mods);
	if (reason != null) { Debug.Log("[Bitshifter] Reader and Writer left alone: " + reason); return; }
	harmony.PatchAllUncategorized(typeof(BitshifterMod).Assembly);
	...
}
```

- It still does not call `base.OnLoad` (that would `PatchAll` both groups at once). The comment at `B/Mod.cs:38-39` changes to say the toolkit is applied here and the Reader/Writer group later.
- The toolkit applies in `OnLoad`. That is when the two sibling building mods apply theirs (`base.OnLoad` in `SmartWeightPlate/Mod.cs:16`, `MultichannelCritterSensor/Mod.cs:12`). It is earlier than `Db.Initialize`, `LoadGeneratedBuildings` and `DetailsScreen.OnPrefabInit`, and it does not depend on the yield check. `Translations.Register` and `PUtil.InitLibrary` belong in `OnLoad` in any case (the sibling mods' order).
- The log text for standing down: "Disabled:" becomes something that no longer claims the whole mod is off. My pick is "Reader and Writer left alone: <reason>". The Diagnostic Panel stays available.
- `WhyYield` and `Contested` do not change. The toolkit patches touch none of the contested methods.
- `PatchAllUncategorized` runs over the ILRepack-merged assembly, as `PatchAll` does today in every PLib mod in the repo. PLib carries no `[HarmonyPatch]` classes that the sibling mods' `PatchAll` would have exposed.

Rejected:
- **(a)** Plain `PatchAll` in `OnLoad` for the toolkit with an explicit class list for Reader/Writer. Any later Reader/Writer patch that someone forgets to list goes silent.
- **(b)** Toolkit patches applied by hand in `OnAllModsLoaded` (`CreateClassProcessor`). It works, but the toolkit has no reason to patch late, and it is a second mechanism.
- **(c)** Leaving the plan-screen and tech patches inside `PatchAll`. This breaks the user's yield decision: the building would register by reflection (`GeneratedBuildings.LoadGeneratedBuildings` instantiates every `IBuildingConfig`) but would sit in no build menu and no tech.

**How #2-#4 slot in:** each later building adds a `<Name>Config.cs`, a component, a side screen if it needs one, a row in `Toolkit.Buildings`, an `AddSideScreenContent<T>()` line in the one `DetailsScreen` postfix, its strings and a kanim folder. Mod.cs does not change.

## 2. Ids and names

| What | Value | Note |
|------|-------|------|
| Building ID | `RibbonDiagnosticPanel` | Repo rule: PascalCase, matches the config class (`mods/bitshifter` naming standard, "Ids and Tags"). It goes into saves and is never renamed. Collision check: none of the 201 installed Workshop mods (`F:/SteamLibrary/steamapps/workshop/content/457140`) contains `RibbonDiagnosticPanel`, `DiagnosticPanel` or `diagnostic_panel`; vanilla has none. The `Logic` prefix that vanilla uses (`LogicRibbonWriter`) is rejected: it reads as a Klei id, and Klei could ship one. A mod prefix (`Bitshifter_RibbonDiagnosticPanel`) is rejected: the repo rule is no prefix (`SmartWeightPlate`, `MultichannelCritterSensor`), and the name is already specific. **For the user.** |
| Config / component / side screen | `RibbonDiagnosticPanelConfig`, `RibbonDiagnosticPanel`, `RibbonDiagnosticPanelSideScreen` | naming standard |
| Kanim | `diagnostic_panel_kanim` (folder `anim/assets/diagnostic_panel/`) | User decision. Kanim names are one global table, so a generic name is a small collision risk. No installed mod uses it. |
| Port ids | `RibbonDiagnosticPanelInput`, `RibbonDiagnosticPanelOutput` (`HashedString`, public static readonly on the component) | Vanilla pattern `LogicRibbonWriterInput`. Not saved: port values are saved by index (`LogicPorts.serializedOutputValues`). |
| Status item ids | `RibbonDiagnosticPanelInput`, `RibbonDiagnosticPanelSwitches` | |
| Strings | `ModStrings.BUILDINGS.PREFABS.RIBBONDIAGNOSTICPANEL.*` (game-looked-up), `ModStrings.BITSHIFTER.DIAGNOSTICPANEL.*` (mod-private: status items, side screen) | naming standard "Strings (`ModStrings`)"; `common/src/Translations.cs:18` gives the same rule |
| Toolkit patch category | `BitshifterToolkit` | Harmony categories are per assembly. The prefix keeps log and debug output readable. |

**Port cells** (2 × 2, origin (0,0) at bottom-left, unrotatable): **input (0,0) bottom-left, output (1,0) bottom-right.** The signal flows left to right, like the Reader and Writer pair (input (0,0), output (1,0)). Ribbons usually run along the floor.
- The trap: a ribbon dragged through both bottom cells joins input and output into one network, and the panel then latches its own switches into its input. The vanilla Reader/Writer pair has the same trap, with adjacent port cells.
- Alternative: diagonal ports (input (0,1) top-left, output (1,0) bottom-right). A straight run cannot join them, but the ports are less obvious. **For the user**; my pick is the bottom row.

**Building def** (vanilla Writer values, `LogicRibbonWriterConfig.CreateBuildingDef`):
- 2 × 2, HP 30, construction 30 s, `CONSTRUCTION_MASS_KG.TIER0`, `MATERIALS.REFINED_METALS`, melting 1600, `BuildLocationRule.Anywhere`, no noise, decor `PENALTY.TIER0`.
- `Overheatable/Floodable/Entombable = false`, `PermittedRotations.Unrotatable`, `ViewMode = OverlayModes.Logic.ID`, `AudioCategory "Metal"`, `AlwaysOperational`.
- `ObjectLayer.LogicGate` + `SceneLayer.LogicGates` (as the Reader/Writer, so it can sit in front of other buildings and does not use the Building layer).
- `RegisterWithOverlay(Logic.HighlightItemIDs)`, `AddSearchTerms(SEARCH_TERMS.AUTOMATION)`, `GameTags.OverlayInFrontOfConduits`.
- Layer alternative: `SceneLayer.Building` like the critter sensor. **For the user**; my pick is LogicGate, so a debug panel can go over a crowded build.

## 3. Component (`RibbonDiagnosticPanel : KMonoBehaviour, IRender200ms`)

**State.** `[SerializationConfig(MemberSerialization.OptIn)]`:
- one `[Serialize] private int switches;` masked to bits 0-30 with `Mask = LogicGate_UpdateState_Patch.Mask` (or a shared `(1 << 31) - 1`).
- Nothing else is saved. `LogicPorts` already saves the output value, and the input value comes from the network.

**Public surface** (for the side screen and tests):
- `int Switches { get; }`
- `bool IsSwitchOn(int bit)`
- `void SetSwitch(int bit, bool on)`
- `void ToggleSwitch(int bit)`
- `void SetSwitches(int mask)`
- `int InputValue`: the input port's value, or 0 when the input has no network
- `bool OutputWired`
- `int OutputValue`
- `LampState Lamp(int bit)`: `Off`, `Input`, `Switch`.

`bit` is 0-based everywhere in code; shown as `bit + 1` (glossary "Bit 0").

**Update flow.**
- `OnPrefabInit`: `Subscribe(-905833192, OnCopySettings)`, as the Writer does.
- `OnSpawn`:
  - get `LogicPorts`, `KBatchedAnimController`, `KSelectable`
  - `Subscribe(-801688580, OnLogicValueChanged)`, `kbac.Play("on")`
  - `SendOutput()`, `Redraw(force: true)`
  - `selectable.ToggleStatusItem(InputStatus, true, this)`.
- `OnLogicValueChanged(data)`: only when `portID == InputPort`; the output port fires the same event (`LogicPorts.CreatePhysicalPorts`, the sender delegate). Then `SendOutput(); Redraw()`.
- `SetSwitch/ToggleSwitch/SetSwitches`: mask, store, `SendOutput(); Redraw()`. The methods themselves are not gated by wiring, so copy settings and tests work on an unwired panel; the UI does the disabling.
- `Render200ms`: `Redraw()`. This catches a wire added or removed on either port; the game raises no event for that (`LogicPorts.OnLogicNetworkConnectionChanged` is private and only toggles the missing-wire icon). It also catches a stale input after the input wire is cut: `InputValue` returns 0 when `IsPortConnected(InputPort)` is false.
- `SendOutput()`: `ports.SendSignal(OutputPort, rawInput | switches)`.
  - `rawInput` is the full input int, so a Digital CPU bit 32 still passes through.
  - Sent whether or not the output is wired: `LogicPorts.SendSignal` stores the value on the sender with or without a network, and the network takes it when a wire arrives.
  - Rejected: sending input only while unwired. Attaching the wire would emit input-only for up to 200 ms until the poll notices, a wrong pulse that can trigger downstream automation.
  - **For the user**: from the player's side "kept but ignored" still holds, since nothing reads an unwired port. My pick is to always send.
- `Redraw(force)`: compute `(inputValue, switches, outputWired)`. Return unless something changed or `force` is set; `SetSymbolTint` calls `SetDirty` even when the colour is unchanged (`KBatchedAnimController.SetSymbolTint`). Then, per bit:
  - `Switch` when `outputWired && switch bit` → visible, tint purple
  - else `Input` when the input bit is on → visible, tint green
  - else `Off` → `SetSymbolVisiblity(bit_bloom_N, false)`.
  - The cache starts invalid, so the first `Render200ms` after load redraws even if `OnSpawn` ran before the batch group was ready. Tints are not saved (unit 01).
- **Feedback.** Wiring the output back into the input latches, like any vanilla loop. No special handling; one sentence in the README.

**Lamp off state: hide** (`SetSymbolVisiblity(false)`). The body paints a bevelled dark socket under every lamp (unit 01 texture check), and the white digits (`numbers` frame 0) then sit on the dark socket, which reads as "off". Rejected: a dark tint, which costs the same and covers the artist's socket bevel with a flat rectangle.

**Colours.**
- Green: vanilla `LogicRibbonReader.colorOn` (0.341, 0.725, 0.369) = #57B95E. Read from the field with `AccessTools.FieldRefAccess`, as `B/Patches.cs:150` does, or copy the constant. Pick: the constant, since the panel is not a Reader.
- Purple: **#9B59D0, `new Color(0.608f, 0.349f, 0.816f)`**.
  - White digits on it reach a contrast of about 4.4:1; on the vanilla green, about 2.5:1.
  - It stays apart from the green for red-green colour-blind players (it shifts to blue, the green to olive).
  - It does not collide with the vanilla off-red (#F34A47), which the panel never shows.
  - The bloom glows in the tint colour. Confirm the look in game against the lit white digits.
- **For the user** (screenshot).

**Anim when nothing is wired:** `on`, always, with every lamp hidden. It looks the same as `off`, and one pose means no pose switching. `off`, `place` and `ui` stay in the kanim; `off` goes unused.

**Copy settings:** `[MyCmpAdd] CopyBuildingSettings copyBuildingSettings;` plus the `-905833192` handler copies `switches` from the source panel (`SetSwitches(source.Switches)`). The Writer does the same (`LogicRibbonWriter.OnCopySettings`).

## 4. Tooltip

**Route: two custom `StatusItem`s.** The hover card lists status items. The details panel status list shows them with their tooltips. The default `status_overlays` (129022) includes the Logic overlay bit (0x4000), so they should also show while debugging in the automation overlay (confirm in game: `SelectToolHoverTextCard` filters overlay-valid objects).
- Built lazily in static fields on first `OnSpawn`, with the name/tooltip constructor `new StatusItem(id, name, tooltip, "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID, resolve_string_callback: ...)`. `AccessControl.cs:112` is the vanilla precedent. By then the language is final, since `StatusItem` reads its strings at construction.
- The resolve callback gets the component as `data` and formats live values each draw, so the item never needs refreshing.
- `KSelectable.ToggleStatusItem(item, on, this)`: the input item always; the switches item while `OutputWired` (toggled in `Redraw`).
- Rejected: the side screen only (needs a selection, not a hover); the logic-overlay port lines (vanilla, no per-bit text, would need a `SelectToolHoverTextCard` patch).

**Format: the list of on bits, 1-based, with runs collapsed.**
- `Input: 1, 3, 9-12`, `Input: none`; `Switches: 3, 17, 31`.
- The tooltip adds the ribbon value in decimal, `Ribbon value: 3589`. Binary by byte was rejected: written in the panel's reading order (bit 1 first) it runs opposite to ordinary binary (most significant bit first), so either way half the players misread it.
- The list stays readable: the worst case, every other bit, is 16 numbers, about 60 characters.
- No "Output" line: output = input OR switches, and both lines are there.
- **For the user**: list vs binary; my pick is the list.

**Strings** (`ModStrings`, all `LocString`):

```
BUILDINGS.PREFABS.RIBBONDIAGNOSTICPANEL
  NAME    = FormatAsLink("Ribbon Diagnostic Panel", "RIBBONDIAGNOSTICPANEL")
  DESC    = "Seeing every bit of a ribbon at once makes a circuit quick to trace."
  EFFECT  = "Shows all 31 bits of an " + Ribbon + " on a grid of lamps: a lamp lights green for each bit carrying a " + Green + ".\n\n" +
            "With its output wired, it is also a 31-channel switch. Bits switched on light purple, and the output carries every bit that is on at the input or switched on."
  LOGIC_PORT          = "Ribbon Input"
  LOGIC_PORT_ACTIVE   = "Lights a green lamp for each bit carrying a " + Green
  LOGIC_PORT_INACTIVE = "Otherwise, the bit's lamp stays dark"
  LOGIC_PORT_OUTPUT   = "Ribbon Output"
  OUTPUT_PORT_ACTIVE  = "Sends a " + Green + " on each bit that is on at the input or switched on"
  OUTPUT_PORT_INACTIVE= "Otherwise, sends a " + Red + " on that bit"
BITSHIFTER.DIAGNOSTICPANEL
  STATUS_INPUT          = "Input: {0}"
  STATUS_INPUT_TOOLTIP  = "Bits on the input ribbon carrying a " + Green + "\nRibbon value: {1}"
  STATUS_SWITCHES         = "Switches: {0}"
  STATUS_SWITCHES_TOOLTIP = "Bits switched on in the side screen. The output carries these and every input bit."
  BITS_NONE      = "none"
  BITS_SEPARATOR = ", "
  BITS_RANGE     = "{0}-{1}"
  SIDE_SCREEN_TITLE    = "Switches"
  SIDE_SCREEN_WIRED    = "Switched bits join the input on the output."
  SIDE_SCREEN_UNWIRED  = "Wire the output port to use the switches."
  CELL_ON        = "Bit {0}: switched on. Click to switch it off."
  CELL_OFF       = "Bit {0}: switched off. Click to switch it on."
  CELL_INPUT     = "The input has bit {0} on."
  CELL_UNWIRED   = "Bit {0}. Wire the output port to use the switches."
  ALL_OFF        = "All off"
  ALL_OFF_TOOLTIP= "Switch every bit off"
```

`Green`, `Red` and `Ribbon` are built as in `MultichannelCritterSensor/ModStrings.cs:9-11`. The separator and range are strings so that zh can use "、".

## 5. Switch UI

**Grid (recommended).** A PLib `SideScreenContent`, built once in `EnsureBuilt` (SWP pattern, `SmartWeightPlateSideScreen.cs:60-95`), refreshed in `SetTarget` and `Render200ms`.

```
280 px side screen, 8 px inset
+----------------------------------------------+
| Switches                       (screen title)|
| Switched bits join the input on the output.  |   PLabel, TextDarkStyle, wraps
|  [ 1][ 2][ 3][ 4]  [ 5][ 6][ 7][ 8]          |
|  [ 9][10][11][12]  [13][14][15][16]          |   PButton cells 28 x 24, 2 px apart,
|  [17][18][19][20]  [21][22][23][24]          |   16 px gap between nibbles,
|  [25][26][27][28]  [29][30][31]              |   4 rows, 2 px apart = 102 px tall
|                                  [ All off ] |   PButton, right-aligned
+----------------------------------------------+
grid width 8*28 + 6*2 + 16 = 252 px  (fits 264 px inside the inset); total height ~176 px
```

- **Cells:** a `PButton` per bit, `Text = bit + 1` (TextLightStyle 11-13 px, centred), `ToolTip` from `CELL_*`. They sit in four horizontal `PPanel` rows, like UpgradePlanner's `Widgets.AddButtonRows` (`UI/Widgets.cs`), with a fixed 16 px spacer in the middle of each row. `PGridPanel` is not used; no mod in the repo uses it yet. The spare 32nd slot is a blank 28 × 24 spacer, so the grid matches the face.
- **State colour:** `KImage.colorStyleSetting` + `ApplyColorStyleSetting()` on each refresh, the precedent in `MultichannelCritterSensorSideScreen.SetButtonSelected` (`:750-758`). `ColorStyleSetting`s are made with UpgradePlanner's `Palette.Style` helper pattern (`UI/Widgets.cs:37-46`, `ScriptableObject.CreateInstance<ColorStyleSetting>`).
- **Click:** `OnClick = _ => panel.ToggleSwitch(bit)`, then refresh.
- **Disabled (output unwired):** `KButton.isInteractable = false` on every cell and on All off (UpgradePlanner's greyed `BuildingButton`, `UI/Widgets.cs:366-375`). Cells show the input only, as the face does. The top line switches to `SIDE_SCREEN_UNWIRED`. Hover tooltips stay, with `CELL_UNWIRED`.
- **All off** is disabled while no switch is on.
- `GetSideScreenSortOrder` takes the default. `IsValidForTarget`: has a `RibbonDiagnosticPanel`.

**Palette directions** (memory "No default colour schemes": show the user before building; mock at `scratchpad/om008-design/grid-directions.png`):

| | Off | Switch on | Input on (switch off) | Unwired |
|---|---|---|---|---|
| A, game buttons | ButtonBlueStyle | ButtonPinkStyle (the game's "picked") | not shown | everything grey |
| B, panel sockets | socket #2E3138 | lamp purple #9B59D0 | not shown | sockets faded |
| **C, panel mirror (pick)** | socket #2E3138 | lamp purple #9B59D0 | lamp green #57B95E | input green stays, rest faded, no clicks |

The pick is C: the side screen is a copy of the face. The user's own colour rules (green input, purple switch, purple wins) carry over unchanged, and while setting switches the player sees which bits the input already carries. Hover: each colour 15 % lighter. White digits on every fill. **For the user** (pick a direction).

**Patches the grid needs:** 1 (`DetailsScreen.OnPrefabInit` postfix, the repo's standard way to register a PLib side screen). It replaces no game method and touches no vanilla screen, so it behaves the same when Bitshifter stands down.

**Cost:**
- `<UsePLib>true</UsePLib>` (ILRepack merge, `common/oni-mod.targets:68-99`)
- `PUtil.InitLibrary(false)` in OnLoad
- one side-screen class of about 200 lines, its strings.

**Against the user's four criteria:**
- **Works well:** state is read from the component on each refresh. No vanilla state is shared, and it works the same on stand-down.
- **Intuitive:** the layout and colours match the building face 1:1. Click toggles. Disabled cells say why.
- **Looks good:** game fonts and button art (PLib uses the game's assets), game colours or the panel's. To confirm by screenshot.
- **No crazy patches:** one standard postfix.

**List fallback (rejected).** `ILogicRibbonBitSelector` on the component, served by the vanilla `LogicBitSelectorSideScreen` (decompiled `LogicBitSelectorSideScreen`):
- **Stand-down breaks it.** The 31 rows only fit because of Bitshifter's scroll-view postfix (`B/SideScreen.cs:16`), which belongs to the Reader/Writer group and is skipped on stand-down. The list would then run about 1000 px tall. Moving that postfix into the toolkit would wrap Digital CPU's own 32-row Reader/Writer screen as well.
- **The rows are shared.** The vanilla screen instance is shared, and `RefreshToggles` creates rows up to `GetBitDepth()` but never removes them. A 31-row panel followed by a 4-bit Reader/Writer (another mod's stand-down case) leaves 27 extra rows. Clicking one sets an out-of-range bit on that Reader/Writer.
- **No disabled state.** Rows stay clickable while the output is unwired, so clicks would be silently ignored, or a `RefreshToggles`/`UpdateStateVisuals` patch would have to grey them.
- **One row always highlighted.** `ChangeState(0)` marks the "selected" row, which reads as "only this bit" on a multi-select. `GetBitSelection() = -1` avoids it, but leaves no click feedback beyond the state text.
- **Wrong labels.** The rows say "Bit n" plus "Green Signal / Red Signal". Those strings are about signals, not switches. Not a grid; no match with the face.
- Count: 0 patches as long as Bitshifter is active. Correct on stand-down would take 2-3 more patches on a vanilla screen shared with the Reader/Writer, which fails "no crazy patches" and "works well".

**To confirm in game before committing** (screenshots for the user, at human pace):
- (1) the grid in direction C, wired, with input and switches mixed;
- (2) unwired;
- (3) the face next to it, with green and purple lamps and the white digits;
- (4) the hover card, inside and outside the automation overlay.

Build the side screen first in the implementation order, so the screenshots come before the tests are written.

## 6. Art

The repo does:
1. **Move the files.** Copy the artist's three files untouched into `mods/bitshifter/anim-original/diagnostic_panel/` (the MCS precedent keeps the artist's export). The working copies are in `scratchpad/art-3gub/`; the originals are `C:/temp/diagnostic_panel_anim.bytes`, `C:/temp/diagnostic_panel_build.bytes`, `C:/tmp/diagnostic_panel_0.png`. Then write the shipped kanim to `B/anim/assets/diagnostic_panel/` (`diagnostic_panel_anim.bytes`, `_build.bytes`, `_0.png`). No rename (user).
2. **Regenerate `ui`** in the repo's build icon style: the building rendered (body + `numbers`) scaled down to vanilla icon size, over a plain 30 % black ellipse as wide as the building and an eighth as tall (memory "Build icon style"; `tools/make_art.py:229-254` `ui_art`). Done by a new step in `mods/bitshifter/tools/make_art.py`: read `anim-original/diagnostic_panel` with `kanim.py read_kanim`, replace the `ui` symbol's frame, write with `write_kanim`. Then `--check` also covers the panel kanim, and a new export from the artist is re-processed by the same command. The artist's `ui` is a placeholder ("temp", dark red), so this replaces nothing finished.
   - Pose for the icon: the style rule says unlit, which here is a near-black square that will read poorly on the dark build menu. Alternative: a few lamps lit green and purple. **For the user** (screenshot of both); my pick is unlit, the existing rule, unless the screenshot shows it does not read.
3. `place` is already white outline art (unit 01); it stays as is.

Goes back to 3GuB (for the coordinator's message):
1. The large digit under lamp 31 reads **32**.
2. The small label **29** sits about 14 px left of its socket.
3. The spare 32nd cell has no socket and no ribbon mark. Ask whether a static mark painted on the body is wanted; the code needs nothing there.
4. Check white digit contrast on the purple #9B59D0 and the green #57B95E. Say whether the digits should stay white over a lit lamp.

The implementation proceeds on the prototype. The release waits for the fixed export, or the user accepts the prototype for a first release. **For the user.**

## 7. Harness and tests

**TestRunner additions** (architecture trigger: new public kit surface, `common/tools/TestRunner`, namespaces per its naming standard):

| Where | Member | What |
|---|---|---|
| `Kit/Anims.cs` (new), `public static class Anims` in `Isochronous.TestKit` | `public static Color SymbolTint(KBatchedAnimController kbac, KAnimHashedString symbol)` | Symbol index from `KAnimBatchManager.Instance().GetBatchGroupData(kbac.GetBatchGroupID()).GetSymbol(symbol).symbolIndexInSourceBuild` (public). Colour from `kbac.symbolInstanceGpuData` (public getter on `KAnimControllerBase`), private field `symbolInstancesConverter`, public struct field `symbolInstances[i].color`. Throws `OniAssertException` naming the symbol when the build has none. |
| same | `public static bool SymbolVisible(KBatchedAnimController kbac, KAnimHashedString symbol)` | `GetSymbolVisiblity`, wrapped so tests read visibility and tint the same way |
| `Reflect.cs` | `SymbolInstanceGpuDataConverter` (FieldInfo `SymbolInstanceGpuData.symbolInstancesConverter`) | plus its `SelfCheck` line `SymbolInstanceGpuData.symbolInstancesConverter`; same for `DetailsScreenSideScreens` below |
| `Kit/Screens.cs` (new), `public sealed class Screens`, `TestContext.Screens` | `Wait Select(GameObject go)` | `SelectTool.Instance.Select(go.GetComponent<KSelectable>())`, then waits until the details screen shows it (`WaitUntil`, real-time timeout) |
| same | `T SideScreen<T>() where T : SideScreenContent` | the active instance from `DetailsScreen.sideScreens` (private; `Reflect.DetailsScreenSideScreens`), or null |
| same | `void Click(GameObject go)` | pointer down, up and click through `ExecuteEvents` (layer 2). Moves the helper in `Bitshifter.Tests/SideScreen.cs:34-37` into the kit, with down and up added so `KButton` is covered as well as `MultiToggle`. |
| same | `void Deselect()` | `SelectTool.Instance.Select(null)` |
| `Kit/World.cs` | `List<string> StatusItems(GameObject go)` | resolved `entry.GetName()` of each `KSelectable.GetStatusItemGroup()` entry (layer 0 oracle for the hover text) |

The `TestRunner` entities record lists layers 0 and 1. Bitshifter's existing UI tests are already `Layer(2)`; the kit doc should name layer 2 (screens driven by events). The existing `Bitshifter.Tests/SideScreen.cs` can move to `t.Screens` later; not part of this task.

**Test plan** (`Bitshifter.Tests/DiagnosticPanel.cs`, one static class; names start `Bitshifter:`). Every step is paced at human speed: `WaitGameSeconds(1)` after each build, wire drag or toggle, and 0.5 s at the least between UI clicks (memory "Tests at human speed").

**Rig.** Extend `Rig` with a `PanelRig`:
- source: switch(es) + Ribbon Writer, or a second panel used as a 31-bit source;
- ribbon (layer 1 drag) into the panel input;
- panel output, ribbon, Ribbon Reader at bit k, wire, alarm.

Layer 0/1:
1. Lamps follow the input: a Writer at bit n feeds the panel. Switch on: lamp n visible and green (`Anims.SymbolTint`), every other lamp hidden. Switch off: all hidden.
2. A full 31-bit value: a source panel with switches 1, 3, 9-12, 31 drives a monitor panel. The monitor shows exactly those lamps green, and its status item reads `Input: 1, 3, 9-12, 31`.
3. Switches reach the output: output wired to a Reader at bit 17. `SetSwitch(16, true)`: the alarm reads 1, `OutputValue == input | switches`, lamp 17 purple. Off: alarm 0.
4. Purple wins: input bit 3 and switch 3 on: lamp 3 purple. Switch off: green.
5. Unwired output: switches set, output unwired. The lamps show the input only, and the switches status item is absent. Drag a ribbon onto the output (layer 1): within 1 s lamps are purple, and the downstream Reader sees the bits at once (no input-only interval: assert at the first frame the network exists). Deconstruct the wire: back to input only, `Switches` unchanged.
6. Save and load: switches 2 and 30 set. `SaveFile.Capture/Restore`: `Switches` kept, output value kept, tints reapplied after load.
7. Copy settings: panel A's switches copied onto panel B (the game's copy-settings event `-905833192` with A as data).
8. Build menu and research: the def is in `TUNING.BUILDINGS.PLANORDER` Automation, subcategory `wires`, after `LogicRibbonWriter`. `Techs.Get("ParallelAutomation").unlockedItemIDs` contains the id.
9. Stand-down (`Compatibility.cs`, `Explicit`, run with `-Steam 3244925649`): with Digital CPU enabled, the panel is in the build menu and the tech, its side screen opens, and a 32-bit input still shows bits 1-31 and passes bit 32 to the output.

Layer 2:
10. Grid: selecting a wired panel shows `RibbonDiagnosticPanelSideScreen` with 31 cells in four rows of 8, 8, 8, 7. Clicking cell 5 sets bit 4 and colours the cell purple; clicking again clears it. Screenshot.
11. Disabled: on an unwired panel every cell and All off are not interactable, a click changes nothing, and the top line reads the unwired text. Screenshot.
12. All off clears every switch, then disables itself.

## 8. Release surface

- **Version 1.3.0** (minor: new building), in `B/Bitshifter.csproj` `<Version>` and `B/mod_info.yaml` (must match).
- **csproj:** `<UsePLib>true</UsePLib>`, `<UseTranslations>true</UseTranslations>`.
- **Strings and translations:** `ModStrings.cs`. Then `python common/tools/Translations/translations.py bitshifter --build` writes `B/translations/Bitshifter.pot`. `zh.po`, `ko.po`, `ru.po` are written with the Write tool (memory "Mod translations system"), and the checker passes.
- **README:**
  - intro and "What it does": add the panel section.
  - Compatibility: the panel stays when the mod stands down.
  - Art: the panel by 3GuB, and the `make_art.py` step.
  - Implementation notes: two patch groups; "No PLib, no options, no strings" goes.
  - One sentence on the feedback latch.
- **CHANGELOG** `## Unreleased`: one short bullet, e.g. "New building: the Ribbon Diagnostic Panel shows all 31 bits of a ribbon and works as a 31-channel switch." (memory "Changelog style").
- **mod.yaml description:** "Nothing else changes" goes; one sentence for the panel.
- **Workshop** (`publish/workshop-description.txt`):
  - a "What you get" bullet or section for the panel;
  - an art credit line for 3GuB (as `mods/multichannel-critter-sensor/publish/workshop-description.txt:27`);
  - compatibility: the panel stays on stand-down;
  - "Safe to add to or remove from an existing save" now needs "removing the mod removes built Ribbon Diagnostic Panels";
  - tags if the publish material carries them;
  - preview image: a new one is optional. **For the user.**
- **Store records** (through the write path, at completion):
  - `mods/bitshifter:domains/entities.md`:
    - Overview `blk-88f758ea7235` ("adds no buildings")
    - BitshifterMod `blk-976bcafad5d2` ("then `harmony.PatchAll`")
    - Business Rules `blk-de8c66751b82` ("No PLib, no options, no strings"; the stand-down rule)
    - Key Code Paths `blk-2fe104dbf053`
    - new entities: Toolkit, RibbonDiagnosticPanel, its side screen
  - `glossary.md`:
    - "Stand down" `blk-ce9d17157555` ("applies no patches apart from the overlay guard")
    - new terms: Ribbon Diagnostic Panel, switch, lamp
  - `standards/naming-conventions.md` "Bitshifter specifics" `blk-3e9cd9430502`: kanim names, patch category.
  - `standards/code-style.md` "Harmony Patches" `blk-c3e49ad21d32`: Bitshifter applies its toolkit in OnLoad by category and the Reader/Writer group in OnAllModsLoaded.
  - The unit 01 durable facts (kanim loading, texture density, lamp layout).
  - The decision record below.
  - TestRunner entities: kit types `Anims` and `Screens`, layer 2.

## For the user

1. **Switch-grid palette:** A game buttons, B panel sockets, **C panel mirror (pick)**. Mock: `scratchpad/om008-design/grid-directions.png`; confirm with an in-game screenshot.
2. **Lamp purple #9B59D0** (pick), seen in game against the white digits and the vanilla green.
3. **Building id `RibbonDiagnosticPanel`** (pick; saved forever) vs `Bitshifter_RibbonDiagnosticPanel`.
4. **Port cells:** input bottom-left, output bottom-right (**pick**) vs diagonal (input top-left, output bottom-right) to avoid one ribbon joining both.
5. **Output while unwired:** always input OR switches (**pick**, no wrong pulse on wiring) vs input only.
6. **Tooltip format:** list of on bits with ranges, `Input: 1, 3, 9-12` (**pick**) vs binary by byte.
7. **Build icon pose:** unlit per the icon style rule (**pick**) vs a few lamps lit; screenshot both.
8. **Layer:** LogicGate layer like the Reader/Writer (**pick**, can sit over other buildings) vs Building layer.
9. **Release on the prototype art** or wait for 3GuB's fixed export (31 digit, label 29, spare cell). Pick: wait.
10. **New Workshop preview image** or keep the current one. Pick: keep, add a screenshot to the description later.

## Implementation steps (in order)

1. **Art in.** `mods/bitshifter/anim-original/diagnostic_panel/{diagnostic_panel_anim.bytes, diagnostic_panel_build.bytes, diagnostic_panel_0.png}` (copied from the originals). `mods/bitshifter/tools/make_art.py` gets the panel step (ui icon). Run it to write `B/anim/assets/diagnostic_panel/*`, then `--check`.
2. **Build setup.** `B/Bitshifter.csproj` (UsePLib, UseTranslations, Version 1.3.0), `B/mod_info.yaml` (1.3.0).
3. **Strings.** `B/ModStrings.cs`.
4. **Building.** `B/Toolkit/Toolkit.cs`, `B/Toolkit/RibbonDiagnosticPanelConfig.cs`, `B/Toolkit/RibbonDiagnosticPanel.cs` (with status items).
5. **Patches and load.** `B/Toolkit/ToolkitPatches.cs`; `B/Mod.cs` (OnLoad: PLib, Translations, PatchCategory; OnAllModsLoaded: PatchAllUncategorized, stand-down log text).
6. **Side screen.** `B/Toolkit/RibbonDiagnosticPanelSideScreen.cs`. Build and deploy, then screenshots for the user (grid wired and unwired, face, hover card). **Stop for the user's palette and look decision** before the tests.
7. **Harness.** `common/tools/TestRunner/Kit/Anims.cs`, `Kit/Screens.cs`, `Kit/World.cs` (`StatusItems`), `Kit/TestContext.cs` (`Screens` property), `Reflect.cs` (two fields, SelfCheck), TestRunner `README.md`. Committed in `common` (submodule) first, then the bump in ONI-Mods.
8. **Tests.** `B.Tests/DiagnosticPanel.cs` (new), `B.Tests/Rig.cs` (PanelRig), `B.Tests/Compatibility.cs` (stand-down test). Run `Invoke-OniTests.ps1 -Mods Bitshifter`, and the Explicit test with `-Steam 3244925649`.
9. **Translations.** `B/translations/Bitshifter.pot` via `translations.py bitshifter --build`; `zh.po`, `ko.po`, `ru.po` via Write; the checker passes.
10. **Docs and release text.** `mods/bitshifter/README.md`, `CHANGELOG.md`, `B/mod.yaml`, `mods/bitshifter/publish/workshop-description.txt`.
11. **Store.** Bitshifter entities, glossary, naming, code-style records; the decision record; the TestRunner entities record (through the write path).

## Decision record draft (Bitshifter node, `decisions/001-ribbon-toolkit-in-bitshifter.md`)

```
# 001 Ribbon toolkit buildings live in Bitshifter, patched apart from the Reader/Writer

Status: proposed (OM-008)
Tags: mod-entry-point, mod-compatibility, harmony-patching, automation-ribbon

## Context
Bitshifter widened the Ribbon Reader and Writer to 31 bits and added no buildings. Its patches
apply in OnAllModsLoaded, and none apply when it stands down for Digital CPU (Workshop 3244925649)
or for any mod already patching the Reader/Writer bit depth. The user placed a toolkit of 31-bit
ribbon buildings in Bitshifter, starting with the Ribbon Diagnostic Panel (issue #5; #2-#4 follow), and
decided the toolkit stays available when the mod stands down.

## Decision
- The toolkit's Harmony patches carry [HarmonyPatchCategory("BitshifterToolkit")] and are applied
  in OnLoad with harmony.PatchCategory(assembly, "BitshifterToolkit"), together with
  PUtil.InitLibrary and Translations.Register. They never depend on the yield check.
- The Reader/Writer patches stay uncategorized and are applied in OnAllModsLoaded with
  harmony.PatchAllUncategorized(assembly), only when WhyYield returns null. OverlayLabelGuard
  stays applied by hand, always.
- Toolkit code lives in src/Bitshifter/Toolkit/ (namespace Bitshifter). A building adds its config,
  component, optional PLib side screen, strings, kanim folder, one row in Toolkit.Buildings
  (id, plan subcategory, building it follows, tech) and, for a side screen, one
  AddSideScreenContent line in the single DetailsScreen.OnPrefabInit postfix. Mod.cs does not
  change per building.
- Toolkit buildings do not implement ILogicRibbonBitSelector or reuse vanilla Reader/Writer side
  screens; their UI is their own PLib SideScreenContent.

## Consequences
- Bitshifter now references PLib (ILRepack-merged) and has strings and translations.
- A new Reader/Writer patch needs no registration: uncategorized means "yields". A new toolkit
  patch must carry the category, or it silently stands down with the Reader/Writer.
- Needs Harmony 2.3+ (the game ships 2.4.2).
- Toolkit buildings read the full ribbon int, so on Digital CPU's 32-bit ribbons they pass bit 32
  through but show bits 1-31 only.

## Rejected
- A separate companion mod (user decision: one mod).
- PatchAll for both with an explicit class list for one group: a forgotten entry fails silently.
- Toolkit patches applied late in OnAllModsLoaded: no reason to patch late; a second mechanism.
- Leaving toolkit patches in PatchAll: the building registers by reflection but has no build menu
  entry or tech when the mod stands down.
```

## Brief quality (report back)

- **Did not apply once opened:** about 140 of the 193 candidates were other mods' copies of identical standards sections (indentation, braces, comments, Harmony patches, yielding, try/catch, ModStrings naming, transpiler counting, missing data). Their one-line reason, "names mods/bitshifter/src/Bitshifter/Mod.cs", is literally true: they cite it as an example. But they govern their own mods, not this one. I read Bitshifter's own copy of each and skipped the rest, as the spec said. Filtering took an `awk` pass over the saved brief.
- **Missed:** the brief found no decision records; the Bitshifter, TestRunner and tools decision folders are empty. Missing entirely were `common/src/Translations.cs` (the registration and string-branch rule), `common/oni-mod.targets` (UsePLib wiring), and UpgradePlanner's `UI/Widgets.cs` (the `ColorStyleSetting`/disabled-button patterns the grid reuses), which sits outside the scope paths. The memory rules that shaped the design (build icon style, no default colour schemes, tests at human speed) are not store records, so no brief can surface them.
- **Mismatch:** the TestRunner entities record says tests are layer 0 or 1, but `Bitshifter.Tests/SideScreen.cs` already uses `Layer(2)` for UI-event tests.

## ICON tooling friction (for the coordinator to record)

- `ks brief ... --decisions --budget 50502` (to see the 122 truncated candidates): it worked, but returned the same duplicate-heavy list. Finding the handful of distinct records took a scripted filter (`awk` on the saved output, dropping lines matching the duplicated section names). I expected `--decisions` to surface decision records first, or a per-node dedupe of identical section titles. What I did instead: filtered by hand, then `ks read` on Bitshifter's own ids.
- `ks read` with `--store` and both `blk-` and `node:path` ids worked as documented. Nothing in the skills or hooks got in the way. The SubagentStart hook's skill check led me to `frontend-design`, which fit the palette question.
