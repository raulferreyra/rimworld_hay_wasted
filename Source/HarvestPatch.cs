using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace HayWasteMod
{
    /// <summary>
    /// Main Harmony Patch for the Hay Waste Mod.
    /// Intercepts plant harvest to generate hay as a byproduct inversely proportional to yield.
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
        /// Uses a postfix to execute code after the original method.
        /// </summary>
        [HarmonyPatch(typeof(JobDriver_PlantHarvest), "MakeNewToils")]
        public static class PlantHarvestPatch
        {
            [HarmonyPostfix]
            public static void Postfix(JobDriver_PlantHarvest __instance, ref IEnumerable<Toil> __result)
            {
                try
                {
                    // Wrap the original toils to add hay generation at the end
                    __result = WrapToilsWithHayGeneration(__instance, __result);
                }
                catch (Exception ex)
                {
                    Log.Error($"[HayWasteMod] Error in plant harvest patch: {ex.Message}\n{ex.StackTrace}");
                }
            }

            /// <summary>
            /// Wraps the toil sequence to add hay generation after the harvest job completes.
            /// </summary>
            private static IEnumerable<Toil> WrapToilsWithHayGeneration(JobDriver_PlantHarvest harvester, IEnumerable<Toil> baseToils)
            {
                Toil lastToil = null;
                
                // Iterate through toils and track the last one
                foreach (Toil toil in baseToils)
                {
                    yield return toil;
                    lastToil = toil;
                }

                // Add a final toil that generates hay after harvest completes
                if (lastToil != null)
                {
                    Toil hayToil = new Toil();
                    hayToil.initAction = () =>
                    {
                        TryGenerateHay(harvester);
                    };
                    hayToil.defaultCompleteMode = ToilCompleteMode.Instant;
                    yield return hayToil;
                }
            }
        }

        /// <summary>
        /// Attempts to generate hay byproduct from harvesting.
        /// </summary>
        private static void TryGenerateHay(JobDriver_PlantHarvest harvester)
        {
            try
            {
                // Access the protected Plant property through reflection
                var plantField = typeof(JobDriver_PlantWork).GetField("plant", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                Plant plant = plantField?.GetValue(harvester) as Plant;
                
                // Safety checks
                if (plant == null || plant.Destroyed || plant.Map == null)
                    return;

                // Only generate hay for harvestable plants
                if (plant.def?.plant == null || plant.def.plant.harvestYield <= 0)
                    return;

                // Estimate yield based on plant growth (0 to 1)
                float yieldEstimate = Mathf.Max(0f, Mathf.Min(1f, plant.Growth));
                
                // Calculate hay amount: inverse relationship to growth
                // More growth = less hay waste, less growth = more hay waste
                int hayAmount = Mathf.Clamp(Mathf.RoundToInt((1f - yieldEstimate) * 10f), 1, 10);

                // Spawn the hay
                SpawnHayByproduct(plant, hayAmount);
            }
            catch (Exception ex)
            {
                Log.Error($"[HayWasteMod] Error generating hay: {ex.Message}\n{ex.StackTrace}");
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
                // Get the hay thingdef
                ThingDef hayDef = ThingDef.Named("Hay_Waste");
                
                if (hayDef == null)
                {
                    Log.Error("[HayWasteMod] Hay_Waste ThingDef not found! Check if mod is loaded correctly.");
                    return;
                }

                // Create a new stack of hay
                Thing hay = ThingMaker.MakeThing(hayDef);
                hay.stackCount = amount;

                // Try to drop it at the plant location
                GenDrop.TryDropSpawn(hay, plant.Position, plant.Map, ThingPlaceMode.Near, out Thing resultHay);
                
                if (resultHay != null)
                {
                    Log.Message($"[HayWasteMod] Generated {amount} hay from {plant.def.label} harvest at {plant.Position}");
                }
                else
                {
                    Log.Warning($"[HayWasteMod] Failed to spawn hay at {plant.Position}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[HayWasteMod] Error spawning hay: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
