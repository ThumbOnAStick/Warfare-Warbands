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


        public static int currentIndex = 0;

        public static List<FactionDef> AllUsableFactions => Find.FactionManager.AllFactions.
            Where(x => x.def.humanlikeFaction && !x.Hidden)
            .Select(x=>x.def)
            .Distinct()
            .ToList();

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

        public static void SetFaction(FactionDef def)
        {
            var facs = AllUsableFactions;
            if (facs.Any(x => x == def))
            {
                currentIndex = facs.FirstIndexOf(x => x == def);
            }
        }

        public static void Draw(Rect inRect)
        {
            var factionDefs = AllUsableFactions;
            Rect elementRect = WarbandUI.CenterRectFor(inRect, new Vector2(100, 100));
            FactionDef currentDef;
            if (factionDefs.Count() > currentIndex)
            {
                currentDef = factionDefs.ElementAtOrDefault(currentIndex);
                if (currentDef != null)
                {
                    GUI.color = currentDef.DefaultColor;
                    Widgets.DrawTextureFitted(elementRect, currentDef.FactionIcon, 1.0f);
                    GUI.color = Color.white;
                    GameComponent_WAW.playerWarband.pawnFactionType = currentDef;
                }
                else
                {
                    currentIndex = 0;
                }
            }
            else
            {
                currentIndex = 0;
            }
            float nextPageButtonWidth = 30;
            Rect nextPageRect = new Rect(new Vector2(elementRect.xMax, elementRect.y), new Vector2(nextPageButtonWidth, 100));
            Rect prevPageRect = new Rect(new Vector2(elementRect.xMin - nextPageButtonWidth, elementRect.y), new Vector2(nextPageButtonWidth, 100));
            if(Widgets.ButtonImage(nextPageRect, TexUI.ArrowTexRight))
            {
                NextPage(factionDefs.Count());
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
