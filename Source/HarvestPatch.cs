using System;
using Verse;
using RimWorld;
using HarmonyLib;

namespace HayWasteMod
{
    /// <summary>
    /// Main Harmony Patch for the Hay Waste Mod.
    /// Intercepts the plant harvest to generate hay as a byproduct inversely proportional to yield.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarvestPatch
    {
        static HarvestPatch()
        {
            var harmony = new Harmony("URAS.HayWasteMod");
            harmony.PatchAll();
        }

        /// <summary>
        /// Patches JobDriver_PlantHarvest.MakeNewToils() to add hay generation after harvest completes.
        /// </summary>
        [HarmonyPatch(typeof(JobDriver_PlantHarvest), "MakeNewToils")]
        public static class PlantHarvestPatch
        {
            [HarmonyPostfix]
            public static void Postfix(JobDriver_PlantHarvest __instance)
            {
                try
                {
                    var plant = __instance.Plant;
                    
                    // Safety check: ensure plant exists and is valid
                    if (plant == null || plant.Destroyed)
                        return;

                    // Get the yield amount (0-100 represented as 0-1)
                    float yieldPct = plant.YieldPct();
                    
                    // Calculate hay amount using inverse ratio formula
                    // Formula: CantidadHeno = Clamp(11 - UnidadesCosechadas, 1, 10)
                    int harvestedAmount = Mathf.CeilToInt(yieldPct * 10f);
                    int hayAmount = Mathf.Clamp(11 - harvestedAmount, 1, 10);

                    // Spawn hay at plant location or nearby
                    SpawnHayByproduct(plant, hayAmount);
                }
                catch (Exception ex)
                {
                    Log.Error($"[HayWasteMod] Error in plant harvest patch: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Spawns the hay byproduct at or near the plant location.
        /// </summary>
        private static void SpawnHayByproduct(Plant plant, int amount)
        {
            if (plant == null || plant.Map == null)
                return;

            try
            {
                // Get the hay thing def
                ThingDef hayDef = ThingDef.Named("Hay_Waste");
                
                if (hayDef == null)
                {
                    Log.Error("[HayWasteMod] Hay_Waste ThingDef not found!");
                    return;
                }

                // Create a new stack of hay
                Thing hay = ThingMaker.MakeThing(hayDef);
                hay.stackCount = amount;

                // Try to drop it at the plant location
                GenDrop.TryDropSpawn(hay, plant.Position, plant.Map, ThingPlaceMode.Near, out Thing resultHay);
                
                if (resultHay != null)
                {
                    Log.Message($"[HayWasteMod] Generated {amount} hay from plant harvest at {plant.Position}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[HayWasteMod] Error spawning hay: {ex.Message}");
            }
        }
    }
}
