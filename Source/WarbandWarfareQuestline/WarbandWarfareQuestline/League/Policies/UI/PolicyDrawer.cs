using RimWorld;
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
        public static readonly float margin = 30;
        public static readonly float policyBoxWidth = 200;
        public static readonly float policyBoxHeight = 25;
        public static readonly float factionIconSize = 15;
        public static readonly float iconMargin = 10;


        public static void DrawPolicyRecursively(this Policy policy, Rect rect, bool isParentDisabled,out float height)
        {
            policy.DrawPolicy(rect, isParentDisabled);
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
                policy.Children[i].DrawPolicyRecursively(childRect, policy.Disabled ,out height);
                Widgets.DrawLine(new Vector2(rect.xMax, rect.y + rect.height/2), new Vector2(childRect.xMin, childRectY + rect.height / 2), Color.white, 1);

            }
        }

        public static void DrawPolicy(this Policy policy, Rect rect, bool isParentDisabled)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, policy.Def.label);
            Text.Anchor = TextAnchor.UpperLeft;

            Color color = isParentDisabled ? Color.grey : Color.white;
            if(!policy.Disabled)
            {
                color = new Color(.2f, 1f, .2f);
            }
            Widgets.DrawBoxSolidWithOutline(rect, new Color(0, 0, 0, 0), color);
            if(!policy.Disabled)
            {
                Widgets.DrawHighlight(rect, 2);
            }
            if (isParentDisabled)
            {
                return;
            }
            if (Widgets.ButtonInvisible(rect))
            {
                policy.Execute();
            }
            DrawSupporters(rect, policy, isParentDisabled);
        }

        static bool IsPolicyAvailable(Policy policy, bool isParentDisabled)
        {
            return policy.Disabled && !isParentDisabled;
        }

        static void DrawSupporters(Rect policyRect, Policy policy, bool isParentDisabled)
        {
            if(IsPolicyAvailable(policy, isParentDisabled))
            {
                // Draw supporters
                if(GameComponent_League.Instance.Factions.Count < 1)
                {
                    return;
                }
                float verticleDistance = 0;
                foreach(var f in GameComponent_League.Instance.Factions)
                {
                    if (f.Trait.dislikedCategory != policy.Def.category)
                    {
                        // Draw faction icon
                        GUI.color = f.FactionColor;
                        Rect iconRect = new Rect(policyRect.x + verticleDistance, policyRect.yMax + iconMargin, factionIconSize, factionIconSize);
                        Widgets.DrawTextureFitted(
                            iconRect,
                            f.GetFactionIcon(),
                            1);
                        GUI.color = Color.white;
                        TooltipHandler.TipRegion(iconRect, "WAW.Propolicy".Translate(f.FactionName, policy.Def.label));
                        verticleDistance += factionIconSize + iconMargin;
                    }
                }
            }
        }
    }
}
