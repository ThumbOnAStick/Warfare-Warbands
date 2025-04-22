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
            PatchTradePriceImprovement();
            Log.Message("WAWLeague: Patch Successful");
        }
        public static void PatchTradePriceImprovement()
        {
            MethodInfo original1 = AccessTools.Method(typeof(TradeUtility), "GetPricePlayerSell");
            HarmonyMethod postfix1 = new HarmonyMethod(typeof(HarmonyPatches_League).GetMethod("SellingImprovementPatch"));
            harmony.Patch(original1, postfix: postfix1);

            MethodInfo original2 = AccessTools.Method(typeof(TradeUtility), "GetPricePlayerBuy");
            HarmonyMethod postfix2 = new HarmonyMethod(typeof(HarmonyPatches_League).GetMethod("BuyingImprovementPatch"));
            harmony.Patch(original2, postfix: postfix2);

        }

        // Selling improvement
        public static void SellingImprovementPatch(ref float __result, Thing thing)
        {
            // Check if trade deal is available
            if (GameComponent_League.Instance.IsTradeTreatyActive)
            {
                __result = Math.Max(__result, thing.MarketValue);
            }
        }

        // Buying improvement
        public static void BuyingImprovementPatch(ref float __result, Thing thing)
        {
            if (GameComponent_League.Instance.IsTradeTreatyActive)
            {
                __result = Math.Min(__result, thing.MarketValue);
            }
        }
    }
}
