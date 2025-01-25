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
        public static Rect InRect => _inRect;
        static Rect _inRect;

        public static void DoWindowContents(Rect inRect, Window window, Map map, int mode = 0)
        {
            _inRect = inRect;
            Text.Font = GameFont.Small;
            Rect exitButtonRect = new Rect(445, 0, 15, 15);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                window.Close();
            }
            switch (mode)
            {
                case 0:
                    int width = 200;
                    int height = 100;
                    Rect arrangeWarbandWindowButtonRect = WarbandUI.CenterRectFor(inRect, new Vector2(width, height)) ;
                    bool arrangeWarbandWindow = Widgets.ButtonText(arrangeWarbandWindowButtonRect, "ArrangeWarband".Translate());
                    if (arrangeWarbandWindow)
                    {
                        window.Close();
                        Find.WindowStack.Add(new Window_ArrangeWarband(map));
                    }

                    Rect characterCustomizationButtonRect = new Rect(arrangeWarbandWindowButtonRect.x, arrangeWarbandWindowButtonRect.y + arrangeWarbandWindowButtonRect.height + 10, 100, 50);
                    bool customizationWindow = Widgets.ButtonText(characterCustomizationButtonRect, "WAW.CustomizeCharacter".Translate(), false);
                    if (customizationWindow)
                    {
                        window.Close();
                        Find.WindowStack.Add(new Window_Customization());
                    }


                    Rect quickRaidButtonRect = new Rect(arrangeWarbandWindowButtonRect.x + characterCustomizationButtonRect.width + 10, characterCustomizationButtonRect.y , 100, 50);
                    bool quickRaidWindow = Widgets.ButtonText(quickRaidButtonRect, "WAW.QuickRaid".Translate(), false);
                    if (quickRaidWindow)
                    {
                        window.Close();
                        Find.WindowStack.Add(new Window_QuickRaid());
                    }
                    break;
                case 1:
                    onLeagueDrawn.Invoke();
                    break;
                case 2:
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
                    break;
            }
         
 



        }
        private static Vector2 scrollPosition;
        static readonly float descriptionHeight = 50f;
        static readonly float descriptionWidth = 300f;
    }
}
