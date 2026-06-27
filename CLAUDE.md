# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build

From the `Source/` directory:

```
dotnet build
```

The compiled `HayWasteMod.dll` is copied automatically to `Assemblies/`. Note: `.gitignore` excludes `Assemblies/*.dll`, but the DLL is committed to the repo so players can install without compiling — add it back with `git add -f Assemblies/HayWasteMod.dll` after building.

RimWorld DLL references are resolved via hardcoded `HintPath` in [Source/HayWasteMod.csproj](Source/HayWasteMod.csproj). If those paths don't exist locally, set the `RIMWORLD_INSTALL_PATH` environment variable or adjust the `HintPath` values to point to the local RimWorld install. Target: `.NET Framework 4.7.2` (`net472`).

## Architecture

This mod has two layers:

**XML layer** — `Defs/Plant_Hay.xml` defines the `Hay_Waste` ThingDef (the actual item that spawns in-world). It is marked `NeverForNutrition` so animals/colonists won't eat it automatically.

**C# layer** — `Source/HarvestPatch.cs` patches `JobDriver_PlantHarvest.MakeNewToils()` via a Harmony postfix. Rather than modifying any existing toil, it appends a new `Instant`-mode toil at the end of the sequence. That toil calls `TryGenerateHay()`, which:
1. Reads the private `plant` field from `JobDriver_PlantWork` via reflection.
2. Uses `plant.Growth` (0–1) to compute hay amount: `Clamp(Round((1 - growth) * 10), 1, 10)`.
3. Calls `GenDrop.TryDropSpawn()` to place hay near the harvest cell.

The Harmony patch ID is `URAS.HayWasteMod`. `[StaticConstructorOnStartup]` on `HarvestPatch` ensures patching runs at game load.

## Key Details

- The item defName used in C# is `"Hay_Waste"` — must match `Defs/Plant_Hay.xml`.
- The hay formula uses `plant.Growth` (growth fraction at time of harvest), not the actual harvested unit count. This differs from the formula described in AGENT.md/README (`11 - units`).
- The mod loads for RimWorld 1.4, 1.5, and 1.6 from the same root folder (see `.loadFolders.xml`).
- Harmony dependency is pulled via NuGet (`Lib.Harmony 2.2.2`); the bundled `Assemblies/0Harmony.dll` is the shipped copy.
