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
            WAWUI.DoWindowContents(inRect, this, map);
        }

        private readonly Map map;

    }
}
