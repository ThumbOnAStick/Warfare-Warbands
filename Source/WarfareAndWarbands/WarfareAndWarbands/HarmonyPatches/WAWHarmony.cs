using HarmonyLib;
using Verse;
using WarfareAndWarbands.UI;

namespace WarfareAndWarbands.HarmonyPatches
{
    [StaticConstructorOnStartup]
    public static class WAWHarmony
    {
        static WAWHarmony()
        {
            Harmony harmony;
            if ((harmony = WAWHarmony.harmony) == null)
            {
                harmony = (WAWHarmony.harmony = new Harmony("thumb.WAW"));
            }
            WAWHarmony.harmony = harmony;
            CommsPatch.PatchHarmony();
            WAWHarmony.harmony.PatchAll();
            Log.Message("WAW: patches successful");
        }

        

        public static Harmony harmony;

    }


}
