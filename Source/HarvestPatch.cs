using System;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace HayWasteMod
{
    [StaticConstructorOnStartup]
    public static class HarvestPatch
    {
        static HarvestPatch()
        {
            var harmony = new Harmony("URAS.HayWasteMod");
            harmony.PatchAll();
        }

        // Patches Plant.PlantCollected() which fires at the exact moment of harvest,
        // while the plant still exists and has valid Position/Map/Growth data.
        [HarmonyPatch(typeof(Plant), "PlantCollected")]
        public static class PlantCollectedPatch
        {
            // Capture growth data BEFORE the plant is destroyed.
            public static void Prefix(Plant __instance, out (IntVec3 pos, Map map, int hay) __state)
            {
                __state = default;
                try
                {
                    if (__instance?.def?.plant == null || __instance.def.plant.harvestYield <= 0)
                        return;
                    if (__instance.Destroyed || __instance.Map == null)
                        return;

                    int hayAmount = Mathf.Clamp(Mathf.RoundToInt((1f - __instance.Growth) * 10f), 1, 10);
                    __state = (__instance.Position, __instance.Map, hayAmount);
                }
                catch (Exception ex)
                {
                    Log.Error($"[HayWasteMod] Error in PlantCollected Prefix: {ex.Message}");
                }
            }

            // Spawn hay AFTER the harvest completes, using the pre-captured state.
            public static void Postfix((IntVec3 pos, Map map, int hay) __state)
            {
                try
                {
                    if (__state.map == null || __state.hay == 0)
                        return;

                    SpawnHayByproduct(__state.pos, __state.map, __state.hay);
                }
                catch (Exception ex)
                {
                    Log.Error($"[HayWasteMod] Error in PlantCollected Postfix: {ex.Message}");
                }
            }
        }

        private static void SpawnHayByproduct(IntVec3 position, Map map, int amount)
        {
            ThingDef hayDef = ThingDef.Named("Hay_Waste");
            if (hayDef == null)
            {
                Log.Error("[HayWasteMod] Hay_Waste ThingDef not found! Check if mod is loaded correctly.");
                return;
            }

            Thing hay = ThingMaker.MakeThing(hayDef);
            hay.stackCount = amount;
            GenDrop.TryDropSpawn(hay, position, map, ThingPlaceMode.Near, out _);
        }
    }
}
