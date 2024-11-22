using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband.HarmonyPatches
{
    internal class WorldObjectGizmosPatch
    {
        public static void PatchHarmony()
        { 
            MethodInfo original = AccessTools.Method(typeof(Pawn_RoyaltyTracker), "GetGizmos");
            HarmonyMethod postfix = new HarmonyMethod(typeof(WorldObjectGizmosPatch).GetMethod("GizmosPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void GizmosPatch(ref IEnumerable<Gizmo> __result, ref MapParent __instance, Pawn p)
        {
            if (__instance.HasMap)
                __result.AddItem(WarbandUI.RetreatAllPawns(__instance.Map));

        }
    }
}
