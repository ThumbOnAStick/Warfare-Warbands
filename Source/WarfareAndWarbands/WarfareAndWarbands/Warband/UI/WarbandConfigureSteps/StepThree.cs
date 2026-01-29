using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;
using WarfareAndWarbands.Warband.VassalWarband;

namespace WarfareAndWarbands.Warband.UI.WarbandConfigureSteps
{
    internal static class StepThree
    {
        public static void Draw(Rect inRect, ref Vector2 scrollPosition, Warband warband = null)
        {
            WarbandUI.DrawPawnSelection(inRect, ref scrollPosition, warband);
            WarbandUI.DrawResetButton(inRect); 
            WarbandUI.DrawShowCustomOnlyButton(inRect);

        }

    }
}
