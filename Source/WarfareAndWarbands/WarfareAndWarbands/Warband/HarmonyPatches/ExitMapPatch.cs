using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.HarmonyPatches;

namespace WarfareAndWarbands.Warband.HarmonyPatches
{
    public static class ExitMapPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn), "ExitMap");
            HarmonyMethod prefix = new HarmonyMethod(typeof(ExitMapPatch).GetMethod("ExitPatch"));
            WAWHarmony.harmony.Patch(original, prefix, null);
        }

        public static void ExitPatch(ref Pawn __instance)
        {
            var comp = __instance.TryGetComp<CompMercenary>();
            if (comp != null && comp.ServesPlayerFaction == true)
            {
                comp.ServesPlayerFaction = false;
                __instance.SetFaction(Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband));
                __instance.DeSpawn();
            }
        }
    }
}
