using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.UI;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.Warfare.UI
{
    internal static class WAWUI
    {
        public static void DoWindowContents(Rect inRect, Window window, Map map)
        {
            Rect exitButtonRect = new Rect(430, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                window.Close();
            }
            // display factions
            var visibleFactions = Find.FactionManager.AllFactionsVisible.Where(x => !x.def.isPlayer && !x.defeated);
            Rect outRect = new Rect(inRect.x, exitButtonRect.yMax + 50f, inRect.width, 200f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 30f, (float)(visibleFactions.Count() * (descriptionHeight + 10)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float factionPosition = outRect.y;
            foreach (Faction f in visibleFactions)
            {
                Widgets.ButtonImage(new Rect(30, factionPosition, 24, 24), f.def.FactionIcon, f.Color, false, null);
                Widgets.Label(new Rect(75, factionPosition, descriptionWidth, descriptionHeight), f.GetWarDurabilityString());
                factionPosition += descriptionHeight + 10;
            }

            Widgets.EndScrollView();
            Rect arrangeWarbandWindowButtonRect = new Rect(100, 400, 100, 50);
            bool arrangeWarbandWindow = Widgets.ButtonText(arrangeWarbandWindowButtonRect, "ArrangeWarband".Translate());
            if (arrangeWarbandWindow)
            {
                window.Close();
                Find.WindowStack.Add(new Window_ArrangeWarband(map));
            }

            Rect characterCustomizationButtonRect = new Rect(arrangeWarbandWindowButtonRect.xMax + 10, 400, 100, 50);
            bool customizationWindow = Widgets.ButtonText(characterCustomizationButtonRect, "WAW.CustomizeCharacter".Translate());
            if (customizationWindow)
            {
                window.Close();
                Find.WindowStack.Add(new Window_Customization());
            }

        }
        private static Vector2 scrollPosition;
        static readonly float descriptionHeight = 50f;
        static readonly float descriptionWidth = 300f;
    }
}
