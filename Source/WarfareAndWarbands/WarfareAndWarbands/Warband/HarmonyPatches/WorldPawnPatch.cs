using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader;

namespace WarfareAndWarbands.Warband.HarmonyPatches
{
    public class WorldPawnPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(WorldPawns), "GetSituation");
            HarmonyMethod prefix = new HarmonyMethod(typeof(WorldPawnPatch).GetMethod("SituationPatch"));
            WAWHarmony.harmony.Patch(original, prefix, null);
        }

        public static void SituationPatch(Pawn p, ref WorldPawnSituation __result)
        {
            var comp = p.TryGetComp<CompMercenary>();
            if (comp != null && p.IsLeader())
            {
                __result = WorldPawnSituation.ReservedByQuest;
            }
        }
    }
}
