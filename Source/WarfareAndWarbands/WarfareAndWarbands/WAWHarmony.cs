using HarmonyLib;
using Verse;
using WarfareAndWarbands.UI;
using WarfareAndWarbands.Warband.HarmonyPatches;
using WarfareAndWarbands.Warfare.HarmonyPatches;
using RimWorld;
using WarfareAndWarbands.CharacterCustomization;
using System.Collections.Generic;
using WarfareAndWarbands.Warband.Compatibility_BetterGC;

namespace WarfareAndWarbands.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public static class WAWHarmony
    {
        const string betterGC = "GwinnBleidd.MothballedAndDeadPawns";

        static WAWHarmony()
        {
            Harmony harmony;
            if ((harmony = WAWHarmony.harmony) == null)
            {
                harmony = (WAWHarmony.harmony = new Harmony("thumb.WAW"));
            }
            WAWHarmony.harmony = harmony;
            CommsPatch.PatchHarmony();
            GetHomeFactionPatch.PatchHarmony();
            ExitMapPatch.PatchHarmony();
            SettlemntDestroyedPatch.PatchHarmony();
            CaravanGizmosPatch.PatchHarmony();
            WorldPawnPatch.PatchHarmony();
            // Compatiblity with betterGC
            if (ModsConfig.IsActive(betterGC))
            {
                BetterGCHarmony.PatchHarmony();
            }
            WAWHarmony.harmony.PatchAll();
            Log.Message("WAW: patches successful");
        }



        public static Harmony harmony;
    }


}
