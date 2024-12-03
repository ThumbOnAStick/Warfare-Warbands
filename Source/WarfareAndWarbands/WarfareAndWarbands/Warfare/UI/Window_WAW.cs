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
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warfare.UI;
using static HarmonyLib.Code;

namespace WarfareAndWarbands.UI
{
    public class Window_WAW : MainTabWindow
    {
        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2((float)Verse.UI.screenWidth, (float)Verse.UI.screenHeight * 0.66f);
            }
        }        

        public Window_WAW()
        {
            this.map = Find.AnyPlayerHomeMap;
        }
        public Window_WAW(Map map) 
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
            WAWUI.DoWindowContents(inRect, this, map);
        }

        private Map map;


    }
}
