using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.Warfare.UI
{
    public class Window_WAW_Comm : Window
    {
        public Window_WAW_Comm()
        {
            this.map = Find.AnyPlayerHomeMap;
        }
        public Window_WAW_Comm(Map map)
        {
            this.map = map;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect exitButtonRect = new Rect(430, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
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
            bool arrangeWarbandWindow = Widgets.ButtonText(new Rect(200, 400, 100, 50), "ArrangeWarband".Translate());
            if (arrangeWarbandWindow)
            {
                this.Close();
                Find.WindowStack.Add(new Window_ArrangeWarband(map));
            }
        }

        private Map map;
        private Vector2 scrollPosition;
        private readonly float descriptionHeight = 50f;
        private readonly float descriptionWidth = 300f;

    }
}
