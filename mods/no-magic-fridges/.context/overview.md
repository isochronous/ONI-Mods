---
description: "Overview of the No Magic Fridges mod: how powered fridges behave as a thermostat-controlled insulated box, its Workshop version and supported content, and a map of its source, tests and publish folders"
---

# no-magic-fridges

## What this is
**Id:** blk-9f37b243039c

No Magic Fridges is an Oxygen Not Included mod that makes a powered fridge behave like an insulated box with a thermostat. Vanilla attaches a simulated 1 degC heat reservoir to every stored item, which cools warm food but also warms deep-frozen food back up to 1 degC. With this mod a powered fridge only ever cools what is warmer than 1 degC, never heats what is colder, lets its contents share heat with each other, and insulates cold food from the room. An unpowered fridge is exactly vanilla. Version 0.2.2 is on the Steam Workshop (id 3805326379); `mod.yaml` sets `supportedContent: ALL` and `minimumSupportedBuild: 744825`.

## Where things are
**Id:** blk-5104096f454c

- `src/NoMagicFridges/`: the mod. `Mod.cs` (entry point), `Patches.cs` (stops the vanilla reservoir being attached and adds the thermostat to the fridge prefabs), `FridgeThermostat.cs` (the thermal model, run once a second), `mod.yaml`, `mod_info.yaml`.
- `src/NoMagicFridges.Tests/`: in-game tests of fridge behaviour (`Fridges.cs`).
- `publish/`: Workshop material (`workshop-description.txt`, `workshop-id.txt`).
- `README.md`, `CHANGELOG.md`: user-facing description and release history.
- `.context/` holds `overview.md` (this file), `domains/` (entities and glossary), `standards/` (code style, error handling, naming conventions), `decisions/`, `retrospectives/`, `projects.md` and `META.md`.
