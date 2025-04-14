using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.UI
{
    public static class LeagueDrawer
    {
        public enum LeagueDrawingMode { None, Roads };

        public static LeagueDrawingMode DrawingMode => _drawingMode;

        private static LeagueDrawingMode _drawingMode = LeagueDrawingMode.None;
        private static readonly float _buttonWidth = 200f;
        private static readonly float _buttonHeight = 50f;
        private static readonly float _margin = 50f;

        public static void SwitchDrawingMode(LeagueDrawingMode mode)
        {
            _drawingMode = mode;
        }

        #region RoadBuilder
        private static void DecideRoadTile(bool isStartingTile)
        {
            BeginTargeting(info => SetRoadTile(info, isStartingTile));
        }

        private static void BeginTargeting(Func<GlobalTargetInfo, bool> targetAction)
        {
            if (Find.WorldTargeter.IsTargeting)
            {
                Find.WorldTargeter.StopTargeting();
            }
            CameraJumper.TryShowWorld();
            Find.WorldTargeter.BeginTargeting(targetAction, true, canSelectTarget: IsRoadTile);
        }

        private static bool IsRoadTile(GlobalTargetInfo info)
        {
            return info.WorldObject?.Faction == RimWorld.Faction.OfPlayer;
        }

        private static bool SetRoadTile(GlobalTargetInfo info, bool isStartingTile)
        {
            if (!IsRoadTile(info))
            {
                return false;
            }

            if (isStartingTile)
            {
                GameComponent_League.Instance.RoadBuilder.SetStartAndDest(start: info.Tile);
            }
            else
            {
                GameComponent_League.Instance.RoadBuilder.SetStartAndDest(dest: info.Tile);
            }

            Find.WorldTargeter.StopTargeting();
            return true;
        }

        /// <summary>
        /// Draws a marker on the world for the specified tile.
        /// </summary>
        /// <param name="tile">The tile to mark.</param>
        /// <param name="texture">The texture to use for the marker.</param>
        private static void DrawTileMarker(int tile, Texture2D texture)
        {
            if (tile > 0)
            {
                var drawPos = Find.WorldGrid.GetTileCenter(tile);
                var screenPos = GenWorldUI.WorldToUIPosition(drawPos);
                GUI.DrawTexture(new Rect(screenPos.x - 10, screenPos.y - 10, 20, 20), texture);
            }
        }

        /// <summary>
        /// Draws a button with the specified parameters.
        /// </summary>
        /// <param name="rect">The rectangle defining the button's position and size.</param>
        /// <param name="label">The label to display on the button.</param>
        /// <param name="color">The color of the button.</param>
        /// <param name="onClick">The action to perform when the button is clicked.</param>
        private static void DrawButton(Rect rect, string label, Color color, Action onClick)
        {
            GUI.color = color;
            if (Widgets.ButtonText(rect, label.Translate(), true, false, true))
            {
                onClick?.Invoke();
            }
            GUI.color = Color.white; // Reset color to default
        }

        public static void DrawRoadsBuildingMode()
        {
            // Pre-calculate reusable values
            float distance = _buttonWidth + _margin;
            Vector2 centerBottom = new Vector2(Screen.width / 2, Screen.height * 0.8f);

            // Draw "Decide Starting Point" button
            DrawButton(
                new Rect(centerBottom.x - distance, centerBottom.y, _buttonWidth, _buttonHeight),
                "WAW.RoadConstruct.DecideStarting",
                new Color(0.5f, 1, 0.5f),
                () => DecideRoadTile(true)
            );

            // Draw "Decide Destination Point" button
            DrawButton(
                new Rect(centerBottom.x + _margin, centerBottom.y, _buttonWidth, _buttonHeight),
                "WAW.RoadConstruct.DecideDest",
                new Color(1, 1, 0.5f),
                () => DecideRoadTile(false)
            );

            // Draw Start and End Points
            DrawTileMarker(GameComponent_League.Instance.RoadBuilder.StartingTile, Texture2D.whiteTexture);
            DrawTileMarker(GameComponent_League.Instance.RoadBuilder.DestTile, Texture2D.whiteTexture);

            // Display informational text or confirmation button
            if (GameComponent_League.Instance.RoadBuilder.IsRoadReadyToBuild())
            {
                DrawConfirmationButton(centerBottom);
            }
            else
            {
                DrawInfoText(centerBottom);
            }
        }

        private static void DrawInfoText(Vector2 centerBottom)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(centerBottom + new Vector2(-100, _buttonHeight + _margin), new Vector2(200, 100)), "WAW.RoadConstruct.Info".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DrawConfirmationButton(Vector2 centerBottom)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            if (Widgets.ButtonText(new Rect(centerBottom + new Vector2(-_buttonWidth / 2, _buttonHeight + _margin), new Vector2(_buttonWidth, _buttonHeight)), "WAW.RoadConstruct.Confirm".Translate()))
            {
                GameComponent_League.Instance.RoadBuilder.BuildRoad();
                SwitchDrawingMode(LeagueDrawingMode.None);
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }
        #endregion

        public static void Draw()
        {
            switch (_drawingMode)
            {
                case LeagueDrawingMode.None:
                    break;
                case LeagueDrawingMode.Roads:
                    DrawRoadsBuildingMode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
