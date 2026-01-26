using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Mercenary.HarmonyPatches;
using WarfareAndWarbands.UI;
using WarfareAndWarbands.Warband.Compatibility_BetterGC;
using WarfareAndWarbands.Warband.HarmonyPatches;
using WarfareAndWarbands.Warfare.HarmonyPatches;

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
            MapParentGizmosPatch.PatchHarmony();
            WAWHarmony.harmony.PatchAll();
            Log.Message("WAW: patches successful");
        }



        public static Harmony harmony;
    }


}
