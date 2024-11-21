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
using Verse.AI;


namespace WarfareAndWarbands.HarmonyPatches
{
    public static class CommsPatch
    {
        public static void PatchHarmony()
        {
            MethodInfo original = AccessTools.Method(typeof(Building_CommsConsole), "GetFloatMenuOptions");
            HarmonyMethod postfix = new HarmonyMethod(typeof(CommsPatch).GetMethod("CommsMenuPatch"));
            WAWHarmony.harmony.Patch(original, null, postfix);
        }

        public static void CommsMenuPatch(Building_CommsConsole __instance, ref IEnumerable<FloatMenuOption> __result, Pawn myPawn)
        {

            FloatMenuOption floatMenuOption = (FloatMenuOption)Traverse.Create(__instance).Method("GetFailureReason", new object[]
                  {
                    myPawn
                  }).GetValue();
            bool flag = floatMenuOption == null;
            if (flag)
            {
                FloatMenuOption item = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("OpenFactionInformationTab".Translate(), delegate ()
                {
                    Job job = new Job(WAWDefof.GetInformationFromConsole, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }, WAWTex.modIcon, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0, HorizontalJustification.Left, false), myPawn, __instance, "ReservedBy", null);
                __result = __result.AddItem(item);
            }

        }
    }
}
