using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader;

namespace WarfareAndWarbands.Warband.HarmonyPatches
{
    public static class GetHomeFactionPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(QuestUtility), "GetExtraHomeFaction");
            HarmonyMethod postfix = new HarmonyMethod(typeof(GetHomeFactionPatch).GetMethod("HomeFactionPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void HomeFactionPatch(Pawn p, ref Faction __result)
        {
            var comp = p.TryGetComp<CompMercenary>();
            if (__result != null ||
                comp == null ||
                p.GetComp<CompMercenary>().IsPlayerControlledMercenary != true ||
PlayerWarbandLeaderUtil.IsLeader(p, out Warband warband)
                )
                return;
            __result = Find.FactionManager.FirstFactionOfDef(WAWDefof.PlayerWarband);

        }
    }
}
