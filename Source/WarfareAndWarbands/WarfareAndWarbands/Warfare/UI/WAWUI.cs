using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Verse;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.QuickRaid.UI;
using WarfareAndWarbands.UI;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warfare.UI
{
    public static class WAWUI
    {
        public static UnityEvent onLeagueDrawn = new UnityEvent();
        public static UnityEvent onLeagueInit = new UnityEvent();
        public static Rect InRect => _inRect;
        static Rect _inRect;
        private static readonly float TAB_HEIGHT = 35f;
        private static readonly float MENU_TOP_PADDING = 50f;

        public static void InitWindow()
        {
            onLeagueInit.Invoke();
        }

        public static void DoWindowContents(Rect inRect, Window window, Map map, int mode = 0)
        {
            _inRect = inRect;
            Text.Font = GameFont.Small;
            
            // Draw exit button
            Rect exitButtonRect = new Rect(inRect.width - 20, 0, 15, 15);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                window.Close();
            }

            // Draw tab menu
            DrawSelectionMenu(inRect, window as Window_WAW);

            // Content area starts below the tab menu
            Rect contentRect = new Rect(
                inRect.x, 
                MENU_TOP_PADDING + TAB_HEIGHT, 
                inRect.width, 
                inRect.height - (MENU_TOP_PADDING + TAB_HEIGHT));

            switch (mode)
            {
                case 0:
                    DrawWarbandPanel(contentRect, window, map);
                    break;
                case 1:
                    onLeagueDrawn.Invoke();
                    break;
                case 2:
                    DrawWarfarePanel(contentRect);
                    break;
            }
        }

        private static void DrawSelectionMenu(Rect inRect, Window_WAW window)
        {
            if (window == null) return;
            
            Rect menuRect = new Rect(0, MENU_TOP_PADDING, inRect.width, inRect.height);
            Widgets.DrawMenuSection(menuRect);
            TabDrawer.DrawTabs(menuRect, window.Tabs, 200f);
        }

        private static void DrawWarbandPanel(Rect contentRect, Window window, Map map)
        {
            int width = 300;
            int height = 120;
            
            // Center the arrange warband button in the content area
            Rect arrangeWarbandWindowButtonRect = WarbandUI.CenterRectFor(contentRect, new Vector2(width, height));
            bool arrangeWarbandWindow = Widgets.ButtonText(arrangeWarbandWindowButtonRect, "ArrangeWarband".Translate());
            if (arrangeWarbandWindow)
            {
                window.Close();
                Find.WindowStack.Add(new Window_ArrangeWarband(map));
            }

            float buttonSpacing = 20f;
            float smallButtonWidth = 150f;
            float smallButtonHeight = 60f;

            Rect characterCustomizationButtonRect = new Rect(
                arrangeWarbandWindowButtonRect.x,
                arrangeWarbandWindowButtonRect.yMax + buttonSpacing,
                smallButtonWidth,
                smallButtonHeight);

            bool customizationWindow = Widgets.ButtonText(characterCustomizationButtonRect, "WAW.CustomizeCharacter".Translate(), false);
            if (customizationWindow)
            {
                window.Close();
                Find.WindowStack.Add(new Window_Customization());
            }

            Rect quickRaidButtonRect = new Rect(
                arrangeWarbandWindowButtonRect.x + characterCustomizationButtonRect.width + buttonSpacing,
                characterCustomizationButtonRect.y,
                smallButtonWidth,
                smallButtonHeight);

            bool quickRaidWindow = Widgets.ButtonText(quickRaidButtonRect, "WAW.QuickRaid".Translate(), false);
            if (quickRaidWindow)
            {
                window.Close();
                Find.WindowStack.Add(new Window_QuickRaid());
            }
        }

        private static void DrawWarfarePanel(Rect contentRect)
        {
            var visibleFactions = Find.FactionManager.AllFactionsVisible.Where(x => !x.def.isPlayer && !x.defeated);
            
            // Calculate the total height needed for all factions
            float totalHeight = visibleFactions.Count() * (descriptionHeight + 10);
            
            // Create the view rect with the total height needed
            Rect viewRect = new Rect(0, 0, contentRect.width - 16f, totalHeight); // 16f for scroll bar width
            
            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);
            
            float currentY = 0f;
            foreach (Faction f in visibleFactions)
            {
                Rect iconRect = new Rect(30, currentY, 24, 24);
                Rect labelRect = new Rect(75, currentY, descriptionWidth, descriptionHeight);
                
                Widgets.ButtonImage(iconRect, f.def.FactionIcon, f.Color, false, null);
                Widgets.Label(labelRect, f.GetWarDurabilityString());
                
                currentY += descriptionHeight + 10;
            }
            
            Widgets.EndScrollView();
        }

        private static Vector2 scrollPosition;
        static readonly float descriptionHeight = 50f;
        static readonly float descriptionWidth = 300f;
    }
}
