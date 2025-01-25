using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WarfareAndWarbands.Warfare.UI;
using UnityEngine;
using UnityEngine.Events;
using Verse;
namespace WarbandWarfareQuestline.League.UI
{
    internal static class Window_League
    {
        static int height = 50;
        static readonly int initialHeight = 50;
        static readonly int space = 80;

        public static void AppendDrawingEvent()
        {
            WAWUI.onLeagueDrawn.AddListener(new UnityAction(Draw));
        }

        public static void Draw()
        {
            foreach(var faction in GameComponent_League.Instance.Factions)
            {
                GUI.color = faction.FactionColor;
                Widgets.DrawTextureFitted(new Rect(0, height, 50, 50), faction.GetFactionIcon(), 1);
                height += space;
            }
            height = initialHeight;
        }
    }
}
