using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
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
using WarfareAndWarbands.Warband;
using WarfareAndWarbands;
using RimWorld.Planet;

namespace WarbandWarfareQuestline.Skirmish
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches_Skirmish
    {
        public static Harmony harmony;

        static HarmonyPatches_Skirmish()
        {
            Harmony harmony;
            if ((harmony = HarmonyPatches_Skirmish.harmony) == null)
            {
                harmony = (HarmonyPatches_Skirmish.harmony = new Harmony("thumb.WAW.skirmish"));
            }
            HarmonyPatches_Skirmish.harmony = harmony;
            PatchHarmony();
            Log.Message("WAWSkirmish: Patch Successful");
        }
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(FactionManager), "Notify_WorldObjectDestroyed");
            HarmonyMethod prefix = new HarmonyMethod(typeof(HarmonyPatches_Skirmish).GetMethod("WorldObjectDestroyedPatch"));
            harmony.Patch(original, prefix);
        }


        public static void WorldObjectDestroyedPatch()
        {
           GameComponent_Skrimish.Instance.OnWorldObjectDestroyed();
        }
    }
}
