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
using Verse.Sound;
using WarfareAndWarbands.HarmonyPatches;

namespace WarfareAndWarbands.Mercenary.HarmonyPatches
{
    public static class MapParentGizmosPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(MapParent), nameof(MapParent.GetGizmos));
            HarmonyMethod postfix = new HarmonyMethod(typeof(MapParentGizmosPatch).GetMethod("MapParentPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        private static Command Command_OpenWarbandLootWindow(MapParent mapP)
        {
            return new Command_Action()
            {
                icon = TexCommand.SelectCarriedThing,
                defaultLabel = "WAW.OpenLootWindow".Translate(),
                defaultDesc = "WAW.OpenLootWindow.Desc".Translate(),
                action = delegate
                {
                    if(GenHostility.AnyHostileActiveThreatTo(mapP.Map, Faction.OfPlayer))
                    {
                        Messages.Message("WAW.HostilityDetected".Translate(), MessageTypeDefOf.NeutralEvent);
                        return;
                    }
                    Find.WindowStack.Add(new Dialog_WarbandLoot(mapP));
                }
            };

        }

        public static void MapParentPatch(MapParent __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance == null) return;

            if (!__instance.HasMap) return;

            __result = __result.AddItem(Command_OpenWarbandLootWindow(__instance));
        }
    }
}
