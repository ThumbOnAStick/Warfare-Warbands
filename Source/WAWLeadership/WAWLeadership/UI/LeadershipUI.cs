using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.WarbandComponents;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents.Leader;
using WAWLeadership.LeadershipAttributes;
using WAWLeadership.WorldObjectComps;

namespace WAWLeadership.UI
{
    public static class LeadershipUI
    {
        public static void DrawHexagon(Rect inRect, int height, out List<Vector2> points, out Vector2 center)
        {
            float degree = 0;
            Vector2 startPoint = inRect.position;
            center = new Vector2(startPoint.x, startPoint.y + height);
            points = new List<Vector2>();
            Vector2 endPoint;
            for (int i = 0; i < 6; i++)
            {
                degree += 60;
                float xOffset = Mathf.Sin(degree * Mathf.Deg2Rad) * height;
                float yOffset = Mathf.Cos(degree * Mathf.Deg2Rad) * height;
                endPoint = new Vector2(center.x + xOffset, center.y + yOffset);
                points.Add(endPoint);
                Widgets.DrawLine(center, endPoint, Color.white, 2);
            }

            DrawClosedLines(points, color:Color.white);
        }

        static void DrawClosedLines(List<Vector2> points, Color color, float width = 2)
        {
            Vector2 point1 = points.ElementAt(0);
            Vector2 point2 = points.ElementAt(1);
            for (int i = 0; i < points.Count + 1; i++)
            {
                Widgets.DrawLine(point1, point2, color, width);
                point1 = point2;
                point2 = points.Count > i + 1 ? points.ElementAt(i + 1) : points.ElementAt(0);
            }
        }

        public static void DrawLeadershipAttributes(List<Vector2> points, AttributeSet attributeSet, CompLeadership comp)
        {
            Vector2 size = new Vector2(150, 30);
            for (int i = 0; i < points.Count; i++)
            {
                Rect rect = new Rect(points[i], size);
                TryToDrawLeadershipAttribute(rect, attributeSet, i, comp);
            }

        }


        static void TryToDrawLeadershipAttribute(Rect rect, AttributeSet attributeSet, int index, CompLeadership comp)
        {
            if (TryGetAttributeAt(attributeSet.Attributes, index, out LeadershipAttribute attribute))
            {
                var label = $"{attribute.GetLabel()}({attribute.GetLevel()})";
                float length = label.GetWidthCached();
                float centerOffset = length / 2;
                float actualX = rect.x - centerOffset;
                Rect actualRect = new Rect(new Vector2(actualX, rect.y), rect.size);
                if (attributeSet.AttributePoints > 0)
                    DrawAttributeButton(new Rect(rect.x, actualRect.y + 20, 20, 20), attribute, comp);
                Widgets.Label(actualRect, label);

            }
        }

        public static void DrawCurrentAttributes(List<Vector2> points, Vector2 center, CompLeadership leader)
        {
            List<Vector2> actualPoints = new List<Vector2>();
            for (int i = 0; i < points.Count; i++)
            {
                actualPoints.Add(GetAttributeDrawPoint(points[i], center, i, leader.Leadership.AttributeSet.Attributes));
            }
            DrawClosedLines(actualPoints, Color.yellow, 5f);
        }

        public static Vector2 GetAttributeDrawPoint(Vector2 point, Vector2 center, int index, HashSet<LeadershipAttribute> attributes)
        {
            if (TryGetAttributeAt(attributes, index, out LeadershipAttribute attribute))
            {
                Vector2 dir = (point - center) * attribute.GetLevel() / 3;
                Vector2 drawPoint = center + dir;
                return drawPoint;
            }
            return center;

        }

        private static bool TryGetAttributeAt(HashSet<LeadershipAttribute> attributes, int index, out LeadershipAttribute attribute)
        {
            if (attributes.Count > index)
            {
                attribute = attributes.ElementAt(index);
            }
            else
            {
                attribute = null;

            }
            return attribute != null;
        }

        public static void DrawAttributeButton(Rect rect, LeadershipAttribute attribute, CompLeadership comp)
        {
            bool doAdd = Widgets.ButtonImage(rect, TexButton.Plus);
            if (doAdd)
            {
                comp.DistributePoint(attribute);
            }
        }

        public static void DrawLevel(Rect rect, LeadershipExp exp)
        {
            Widgets.Label(rect, $"WAW.CurrentLevel".Translate(exp.CurrLevel));
        }

        public static void DrawPoints(Rect rect, AttributeSet set)
        {
            Widgets.Label(rect, $"WAW.AttributePoints".Translate(set.AttributePoints));
        }

        public static void DrawExpBar(Rect rect, LeadershipExp exp)
        {
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, "WAW.ExplainExp".Translate());
            }
            Widgets.FillableBarLabeled(rect, exp.ExpPercent(), 60, exp.ToString());
        }

        public static void DrawBuffs(Rect rect, CompLeadership leadership, ref Vector2 scrollPosition)
        {
            string buffs = leadership.GetBuffs();
            int rows = leadership.CountBuffsRows(buffs);
            Rect viewRect = new Rect(rect.x, rect.y, rect.width, rows * 25);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Widgets.Label(viewRect, buffs);
            Widgets.EndScrollView();
        }

        public static Command Interact(bool disabled, bool hasLeader, WorldObjectComp_PlayerWarbandLeader comp, Pawn leader)
        {
            Command_Action command_Action = new Command_Action
            {
                defaultLabel = "WAW.Interact".Translate(),
                defaultDesc = "WAW.Interact.Desc".Translate(),
                disabledReason = hasLeader?"WAW.InteractCoolDown".Translate(comp.GetRemainingDays()): "WAW.NoLeaderFound".Translate(),
                Disabled = disabled || !hasLeader,
                icon = LeadershipTex.Interact,
                action = delegate ()
                {
                    LeadershipUtility.SetWarbandCache(comp.MyWarband);
                    LeadershipUtility.SetLeaderCache(leader);
                    LeadershipUtility.SetLeaderCompCache(comp); 
                    Find.WorldSelector.ClearSelection();
                    Find.WorldTargeter.BeginTargeting(
                        new Func<GlobalTargetInfo, bool>(LeadershipUtility.SelectInteractionTarget), false, null, false, delegate
                        {
                            GenDraw.DrawWorldRadiusRing(comp.parent.Tile, PlayerWarbandManager.playerAttackRange);
                        }, null, null);
                },
                Order = 3000f
            };
            return command_Action;
        }

        public static Command ResetLeaderAbilityCooldown(WorldObjectComp_PlayerWarbandLeader comp)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "Reset usage cooldown";
            command_Action.action = delegate ()
            {
                comp.ResetLastUsedTick();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }

        public static Command GetLeaderInfo(WorldObjectComp_PlayerWarbandLeader comp)
        {
            Command_Action command_Action = new Command_Action();
            command_Action.icon = TexButton.Info;
            command_Action.defaultLabel = "WAW.LeaderInfo".Translate();
            command_Action.action = delegate ()
            {
                comp.LeadershipInfo.OpenLeadershipWindow();
            };
            command_Action.Order = 3000f;
            return command_Action;
        }
    }
}
