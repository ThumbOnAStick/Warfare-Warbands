using CombatExtended.Lasers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.UI.WarbandConfigureSteps
{
    internal static class StepTwo
    {
        static int currentIndex = 0;

        static void NextPage(int cap)
        {
            if (currentIndex < cap - 1)
                currentIndex++;
        }

        static void PrevPage()
        {
            if (currentIndex > 0)
                currentIndex--;
        }

        public static void Draw(Rect inRect)
        {
            var allFactions = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction && !x.Hidden);
            var factionDefs = allFactions.Select(x => x.def).Distinct();
            Rect elementRect = WarbandUI.CenterRectFor(inRect, new Vector2(100, 100));
            if (allFactions.Count() > currentIndex)
            {

                FactionDef currentDef = factionDefs.ElementAtOrDefault(currentIndex);
                if (currentDef != null)
                {
                    GUI.color = currentDef.DefaultColor;
                    Widgets.DrawTextureFitted(elementRect, currentDef.FactionIcon, 1.0f);
                    GUI.color = Color.white;
                    GameComponent_WAW.playerWarband.pawnFactionType = currentDef;
                }
            }
            float nextPageButtonWidth = 30;
            Rect nextPageRect = new Rect(new Vector2(elementRect.xMax, elementRect.y), new Vector2(nextPageButtonWidth, 100));
            Rect prevPageRect = new Rect(new Vector2(elementRect.xMin - nextPageButtonWidth, elementRect.y), new Vector2(nextPageButtonWidth, 100));
            if(Widgets.ButtonImage(nextPageRect, TexUI.ArrowTexRight))
            {
                NextPage(allFactions.Count());
            }
            if(Widgets.ButtonImage(prevPageRect, TexUI.ArrowTexLeft))
            {
                PrevPage();
            }

            Rect descRect = WarbandUI.CenterRectFor(inRect, new Vector2(300, 100), new Vector2(0, 150));
            Widgets.Label(descRect, "WAW.FactionOverride".Translate());
        }
    }
}
