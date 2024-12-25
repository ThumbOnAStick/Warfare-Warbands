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

        static void DrawClosedLines(List<Vector2> points, Color color, float width = 2 )
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

            DrawTriangles(actualPoints, Color.yellow, center, 5f);
            DrawClosedLines(actualPoints, Color.yellow, 1f);
        }

        static void DrawTriangles(List<Vector2> points, Color color, Vector2 center, float width = 2)
        {
            Vector2 point1 = points.ElementAt(0);
            Vector2 point2 = points.ElementAt(1);
            for (int i = 0; i < points.Count + 1; i++)
            {
                FillTriangle(center, point1, point2);
                point1 = point2;
                point2 = points.Count > i + 1 ? points.ElementAt(i + 1) : points.ElementAt(0);
            }
        }

        static void FillTriangle(Vector2 center, Vector2 point1, Vector2 point2)
        {
            List<Pair<Vector2, Vector2>> edges = new List<Pair<Vector2, Vector2>>()
            {
                new Pair<Vector2, Vector2>(point1, point2),
                new Pair<Vector2, Vector2>(point2, center),
                new Pair<Vector2, Vector2>(center, point1)
            };

            float xMax = edges.Max(e => e.First.x);
            float xMin = edges.Min(e => e.First.x);

            for (float i = xMin; i < xMax; i += .5f)
            {
                float high = -999;
                float low = 999;
                foreach (Pair<Vector2, Vector2> edge in edges)
                {
                    float edgeXMin = Math.Min(edge.First.x, edge.Second.x);
                    float edgeXMax = Math.Max(edge.First.x, edge.Second.x);
                    float highCurrent = Math.Max(edge.First.y, edge.Second.y);
                    float lowCurrent = Math.Min(edge.First.y, edge.Second.y);
                    float width = edge.First.x - edge.Second.x;
                    if (i < edgeXMin || i > edgeXMax)
                    {
                        continue;
                    }
                    if (width == 0)
                    {
                        if (i == edge.First.x)
                        {
                            high = highCurrent;
                            low = lowCurrent;
                        }
                        continue;
                    }
                    float slope = (edge.First.y - edge.Second.y) / width;
                    float hight = edge.First.y - slope * edge.First.x;
                    float yImagine = slope * i + hight;
                    if (yImagine >= lowCurrent && yImagine <= highCurrent)
                    {
                        high = high < yImagine ? yImagine : high;
                        low = low > yImagine ? yImagine : low;
                    }
                }
                if (high != -999 && low != 999)
                    Widgets.DrawLine(new Vector2(i, low), new Vector2(i, high + 3), new Color(.8f, .8f, .2f), 2);


            }

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
            var buffs = leadership.GetBuffsList();
            int rows = buffs.Count;
            int rowHeight = 50;
            Rect viewRect = new Rect(rect.x, rect.y, rect.width - 50, rows * rowHeight + 5);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            float height = rect.y + 5;
            bool drawSelected = false;
            foreach ( var buff in buffs )
            {
                Rect buffRect = new Rect(rect.x, height, rect.width, rowHeight);
                Rect heghLightRect = new Rect(rect.x, height - 10, rect.width, rowHeight);

                Widgets.Label(buffRect, buff);
                height += rowHeight;
                if (drawSelected)
                {
                    Widgets.DrawHighlight(heghLightRect);
                }
                drawSelected = !drawSelected;
            }
            Widgets.EndScrollView();
            Widgets.DrawBox(rect);
        }

        public static void DrawToggleSpawnLeader(Rect rect, ref Warband warband)
        {
            Widgets.CheckboxLabeled(
                rect, 
                "WAW.SpawnLeaderOption".Translate(), 
                ref warband.playerWarbandManager.leader.spawnLeader);
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
                comp.LeadershipInfo.OpenLeadershipWindow(comp.MyWarband);
            };
            command_Action.Order = 3000f;
            return command_Action;
        }
    }
}
