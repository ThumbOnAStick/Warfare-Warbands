using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband.HarmonyPatches;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader;

namespace WarfareAndWarbands.Warband.Compatibility_BetterGC
{
    public static class BetterGCHarmony
    {
        public static void PatchHarmony()
        {
            Log.Message("BetterGC patched");
            MethodInfo original = AccessTools.Method(typeof(MothballedAndDeadPawns.Utility), "ShouldDiscardColonist");
            HarmonyMethod postfix = new HarmonyMethod(typeof(BetterGCHarmony).GetMethod("ShouldDiscardPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void ShouldDiscardPatch(Pawn pawn, ref bool __result)
        {
            if (pawn.IsLeader())
            {
                __result = false;
            }
        }
    }
}
