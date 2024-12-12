using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace WAWLeadership.UI
{
    public class ITab_Leadership : ITab
    {
        private CompLeadership leader;

        public ITab_Leadership()
        {
            this.size = new Vector2(630f, 430f);
            this.labelKey = "WAW.TabLeader";
            this.tutorTag = "Leader";
        }

        public override bool IsVisible
        {
            get
            {
                return this.SelPawn.IsColonist;
            }
        }

        protected override void FillTab()
        {
            this.leader = SelPawn.TryGetComp<CompLeadership>();

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
                this.CloseTab();
            }


            // Draw Pawn Portrait
            GUI.color = Color.white;
            Rect portraitRect = new Rect(Vector2.zero, new Vector2(this.size.x / 2, this.size.y));
            Widgets.ThingIcon(portraitRect, this.SelPawn, 1f, null, false);
            Widgets.Label(portraitRect, "WAW.LeadershipTitle".Translate());

            // Draw Attribute Panel
            Rect otherHalf = new Rect(new Vector2(portraitRect.xMax + 100, 100), new Vector2(this.size.x / 2, this.size.y));
            Rect pointsRect = new Rect(otherHalf.x + 50, 350, 200, 50);
            Rect barRect = new Rect(otherHalf.x, 375, 200, 30);

            var attributeSet = leader.Leadership.AttributeSet;
            var attributes = leader.Leadership.AttributeSet.Attributes;
            var exp = leader.Leadership.Exp;
            LeadershipUI.DrawHexagon(otherHalf, 100, out List<Vector2> points, out Vector2 center);
            LeadershipUI.DrawLeadershipAttributes(points, attributeSet);
            LeadershipUI.DrawCurrentAttributes(points, center, attributes);
            LeadershipUI.DrawPoints(pointsRect, attributeSet);
            LeadershipUI.DrawExpBar(barRect, exp);


        }





    }
}
