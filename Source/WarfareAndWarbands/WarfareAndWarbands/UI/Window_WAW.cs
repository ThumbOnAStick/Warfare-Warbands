using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;
using static HarmonyLib.Code;

namespace WarfareAndWarbands.UI
{
    public class Window_WAW : Window
    {

        public Window_WAW(Pawn actor, Map map) : base(null)
        {
           
        }
        
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
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
            var visibleFactions = Find.FactionManager.AllFactionsVisible;
            Rect outRect = new Rect(inRect.x, exitButtonRect.yMax + 35f, inRect.width, 200f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 30f, (float)(visibleFactions.Count() * 24));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float factionPosition = 60f;
            foreach (Faction f in visibleFactions)
            {
                Widgets.ButtonImage(new Rect(30, factionPosition, 24, 24), f.def.FactionIcon, f.Color, false, null);
                factionPosition += 30f;
            }
            Widgets.EndScrollView();

        }

        private Vector2 scrollPosition;


    }
}
