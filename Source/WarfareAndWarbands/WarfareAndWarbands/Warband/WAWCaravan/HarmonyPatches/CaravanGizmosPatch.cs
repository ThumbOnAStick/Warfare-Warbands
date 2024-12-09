using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband.WAWCaravan.UI;

namespace WarfareAndWarbands.Warband.HarmonyPatches
{
    public class CaravanGizmosPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(Caravan), "GetGizmos");
            HarmonyMethod postfix = new HarmonyMethod(typeof(CaravanGizmosPatch).GetMethod("GiszmosPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void GiszmosPatch(ref Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = __result.AddItem(WAWCaravanUI.CreateWarbandWith(__instance));
        }
    }
}
