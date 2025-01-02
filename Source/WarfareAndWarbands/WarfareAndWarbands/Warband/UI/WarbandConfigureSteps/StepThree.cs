using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace WarfareAndWarbands.Warband.UI.WarbandConfigureSteps
{
    internal static class StepThree
    {
        public static void Draw(Rect inRect, ref Vector2 scrollPosition, int pawnKindsEachRow, float descriptionHeight, float descriptionWidth,
            float entryWidth, float entryHeight, Warband warband = null)
        {
            WarbandUI.DrawPawnSelection(inRect, ref scrollPosition, pawnKindsEachRow, descriptionHeight, descriptionWidth, entryWidth, entryHeight, warband);
            WarbandUI.DrawResetButton(); 

        }
    }
}
