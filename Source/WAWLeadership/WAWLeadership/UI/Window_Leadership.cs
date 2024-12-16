using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WAWLeadership.UI
{
    public class Window_Leadership:Window
    {
        private readonly CompLeadership leader;

        public override Vector2 InitialSize => new Vector2(630f, 430f);

        public Window_Leadership(CompLeadership leader)
        {
            this.leader = leader;   
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Some stupid checks
            if (leader == null ||
                leader.Leadership == null ||
                leader.Leadership.AttributeSet == null ||
                leader.Leadership.AttributeSet.Attributes == null)
            {
                Log.Message($"Status: " +
                    $"{leader.Leadership == null}" +
                    $"{leader.Leadership.AttributeSet == null}" +
                    $"{leader.Leadership.AttributeSet.Attributes == null}");
                this.Close();
            }


            // Draw Pawn Portrait
            GUI.color = Color.white;
            Rect portraitRect = new Rect(Vector2.zero, new Vector2(this.InitialSize.x / 2, this.InitialSize.y));
            Widgets.ThingIcon(portraitRect, leader.Pawn, 1f, null, false);
            Widgets.Label(portraitRect, "WAW.LeadershipTitle".Translate());

            // Draw Attribute Panel
            Rect otherHalf = new Rect(new Vector2(portraitRect.xMax + 100, 100), new Vector2(this.InitialSize.x / 2, this.InitialSize.y));
            Rect levelLabelRect = new Rect(otherHalf.x + 50, 330, 200, 30);
            Rect pointsRect = new Rect(otherHalf.x + 50, 350, 200, 50);
            Rect barRect = new Rect(otherHalf.x, 375, 200, 30);


            var attributeSet = leader.Leadership.AttributeSet;
            var exp = leader.Leadership.Exp;
            LeadershipUI.DrawHexagon(otherHalf, 100, out List<Vector2> points, out Vector2 center);
            LeadershipUI.DrawLeadershipAttributes(points, attributeSet, leader);
            LeadershipUI.DrawCurrentAttributes(points, center, leader);
            LeadershipUI.DrawPoints(pointsRect, attributeSet);
            LeadershipUI.DrawLevel(levelLabelRect, exp);
            LeadershipUI.DrawExpBar(barRect, exp);
        }
    }
}
