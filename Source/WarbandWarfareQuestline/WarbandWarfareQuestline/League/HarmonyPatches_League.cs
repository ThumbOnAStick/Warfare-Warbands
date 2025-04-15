using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.Skirmish;

namespace WarbandWarfareQuestline.League
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches_League
    {
        public static Harmony harmony;
        static HarmonyPatches_League()
        {
            Harmony harmony;
            if ((harmony = HarmonyPatches_League.harmony) == null)
            {
                harmony = (HarmonyPatches_League.harmony = new Harmony("thumb.WAW.league"));
            }
            HarmonyPatches_League.harmony = harmony;
            PatchHarmony();
            Log.Message("WAWLeague: Patch Successful");
        }
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(Settlement), "get_TradePriceImprovementOffsetForPlayer");
            HarmonyMethod postfix = new HarmonyMethod(typeof(HarmonyPatches_League).GetMethod("TradeImprovementPatch"));
            harmony.Patch(original, postfix);
        }

        public static void TradeImprovementPatch(ref float __result)
        {
            __result += 1f;
        }
    }
}
