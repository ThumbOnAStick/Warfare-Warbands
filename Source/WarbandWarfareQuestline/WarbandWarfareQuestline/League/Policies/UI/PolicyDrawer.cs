using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public static class PolicyDrawer
    {
        public static readonly float margin = 15;
        public static readonly float policyBoxWidth = 200;
        public static readonly float policyBoxHeight = 25;


        public static void DrawPolicyAndChildren(this Policy policy, Rect rect, out float height)
        {
            policy.DrawPolicy(rect);
            height = rect.y;
            float childRectX = rect.xMax + margin;
            if(policy.Children == null || policy.Children.Count == 0)
            {
                return;
            }
            for (int i = 0; i < policy.Children.Count; i++)
            {
                float childRectY = i < 1? height : height + policyBoxHeight + margin;
                Rect childRect = new Rect(childRectX, childRectY, policyBoxWidth, policyBoxHeight);
                policy.Children[i].DrawPolicyAndChildren(childRect, out height);
                Widgets.DrawLine(new Vector2(rect.xMax, rect.y + rect.height/2), new Vector2(childRect.xMin, childRectY + rect.height / 2), Color.white, 1);

            }
        }

        public static void DrawPolicy(this Policy policy, Rect rect)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, policy.Def.label);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawBox(rect);
            if(!policy.Disabled)
            {
                Widgets.DrawHighlight(rect, 2);
            }
            if (Widgets.ButtonInvisible(rect))
            {
                policy.Execute();
            }
        }
    }
}
