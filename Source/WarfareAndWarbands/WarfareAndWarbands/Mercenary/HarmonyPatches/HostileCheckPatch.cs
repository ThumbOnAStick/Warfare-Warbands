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
using Verse.AI;
using WarfareAndWarbands.HarmonyPatches;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.Mercenary.HarmonyPatches
{
    public static class AnyHostileToPlayerCheckPatch
    {
        // Make sure it only jumps once
        private static Map lastJumpedMap = null;

        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(GenHostility), 
                nameof(GenHostility.AnyHostileActiveThreatTo),
                new Type[] {typeof(Map), typeof(Faction), typeof(IAttackTarget).MakeByRefType(), typeof(bool), typeof(bool) });
            HarmonyMethod postfix = new HarmonyMethod(typeof(AnyHostileToPlayerCheckPatch).GetMethod("HostileCheckPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void HostileCheckPatch(Map map, Faction faction, bool __result)
        {
            if (faction != Faction.OfPlayer) return;
            if (__result == true) return;
            if(map == null) return;
            if(map.Parent == null) return;
            if (map == lastJumpedMap) return;
            if (!map.mapPawns.FreeColonists.Any(x => x.TryGetComp<CompMercenary>() != null && x.TryGetComp<CompMercenary>().IsPlayerControlledMercenary)) return;
            lastJumpedMap = map;
            CameraJumper.TryJumpAndSelect(map.Parent);
            // Send pack up and leave message
            Messages.Message("WAW.LootTime".Translate(), MessageTypeDefOf.NeutralEvent);
        }
    }
}
