using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.HarmonyPatches;

namespace WarfareAndWarbands.Warfare.HarmonyPatches
{
    public static class SettlemntDestroyedPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(Settlement), "PostRemove");
            HarmonyMethod postfix = new HarmonyMethod(typeof(SettlemntDestroyedPatch).GetMethod("RemoveSettlemntPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void RemoveSettlemntPatch(ref Settlement __instance)
        {
            GameComponent_WAW.Instance.DecreaseDurability(__instance.Faction, 15);
        }
    }
}
