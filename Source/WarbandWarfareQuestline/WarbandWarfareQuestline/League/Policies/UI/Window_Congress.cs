using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarbandWarfareQuestline.League.MinorFactions;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public class Window_Congress : Window
    {
        static List<MinorFaction> _pros;
        static List<MinorFaction> _dissenters;

        public Window_Congress()
        {
            _pros = new List<MinorFaction>();
            _dissenters = new List<MinorFaction>();
        }

        public Window_Congress(List<MinorFaction> pros, List<MinorFaction> dissenters)
        {
            _pros = pros ?? new List<MinorFaction>();
            _dissenters = dissenters ?? new List<MinorFaction>();
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Draw wire frame
            float leftPercentage = (float)_pros.Count / (_pros.Count + _dissenters.Count);
            float leftWidth = leftPercentage * inRect.width;
            float rightWidth = inRect.width - leftWidth;

            // Draw pros
            Rect leftRect = new Rect(inRect.x, inRect.y, leftWidth, inRect.height - Margin);
            Widgets.DrawBox(leftRect);

            // Draw dissenters
            Rect rightRect = new Rect(leftRect.xMax, inRect.y, rightWidth, inRect.height - Margin);
            Widgets.DrawBox(rightRect);


        }
    }
}
