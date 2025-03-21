﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using WarbandWarfareQuestline.League.MinorFactions;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public class Window_Congress : Window
    {
        private readonly List<MinorFaction> _pros;
        private readonly List<MinorFaction> _dissenters;
        private const float VoteSessionY = 200;
        private const float PlayerWeight = 0.3f;
        private const float IconDrawSize = 25f;
        private const float PlayerIconDrawSize = 50f;
        private const float DistributionFactor = 0.2f;

        private float _leftPercentage;
        private float _leftPercentageRaw;
        private Rect _leftRect;
        private Rect _rightRect;
        private Rect _topTextDrawingArea;
        private enum MouseState { None, HoverLeft, HoverRight };
        private MouseState _mouseState = MouseState.None;
        private Policy _policy;

        public Window_Congress()
        {
            _pros = new List<MinorFaction>();
            _dissenters = new List<MinorFaction>();
            _policy = new Policy(PolicyDefOf.TaxReform, true);
        }

        public Window_Congress(List<MinorFaction> pros, List<MinorFaction> dissenters, Policy policy)
        {
            _pros = pros ?? new List<MinorFaction>();
            _dissenters = dissenters ?? new List<MinorFaction>();
            _policy = policy;
        }

        void LabelOnTop(Rect rect, TaggedString content)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect.TopPart(0.2f), content);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        void DrawWireFrame(Rect inRect)
        {

            // Calculate the width percentages for pros and dissenters based on their counts
            _leftPercentageRaw = _leftPercentage = (float)_pros.Count / (_pros.Count + _dissenters.Count);

            // Adjust widths based on mouse hover
            ResetMouseState();
            if (Mouse.IsOver(_leftRect))
            {
                _leftPercentage += PlayerWeight;
                HoverLeft();
            }
            else if (Mouse.IsOver(_rightRect))
            {
                _leftPercentage -= PlayerWeight;
                HoverRight();
            }

            float leftWidth = Mathf.Max(_leftPercentage, 0) * inRect.width;
            float rightWidth = inRect.width - leftWidth;

            // Define the rectangles for pros and dissenters sections
            float y = inRect.height - Margin - VoteSessionY;
            _leftRect = new Rect(inRect.x, VoteSessionY, leftWidth, y).ScaledBy(0.9f);
            _rightRect = new Rect(_leftRect.xMax, VoteSessionY, rightWidth - Margin, y).ScaledBy(0.9f);

            // Draw the pros section
            Widgets.DrawBox(_leftRect);

            // Draw the dissenters section
            Widgets.DrawBox(_rightRect);
        }

        Rect BottomLeftForPlayerIcon(Rect rect)
        {
            return new Rect(rect.x, rect.yMax - PlayerIconDrawSize, PlayerIconDrawSize, PlayerIconDrawSize);
        }

        Rect BottomRightForPlayerIcon(Rect rect)
        {
            return new Rect(rect.xMax - PlayerIconDrawSize, rect.yMax - PlayerIconDrawSize, PlayerIconDrawSize, PlayerIconDrawSize);
        }

        void ResetMouseState()
        {
            _mouseState = MouseState.None;
        }

        void HoverRight()
        {
            _mouseState = MouseState.HoverRight;
        }

        void HoverLeft()
        {
            _mouseState = MouseState.HoverLeft;
        }

        void DrawPlayerFaction()
        {
            if (Faction.OfPlayer == null) return;
            if (_mouseState == MouseState.None) return;
            Texture2D playerIcon = Faction.OfPlayer.def.FactionIcon;
            Rect iconRect = _mouseState == MouseState.HoverLeft ? BottomLeftForPlayerIcon(_leftRect) : BottomRightForPlayerIcon(_rightRect);
            GUI.color = Faction.OfPlayer.Color;
            Widgets.DrawTextureFitted(iconRect, playerIcon, 1);
            GUI.color = Color.white;
        }

        void DrawFactions()
        {
            // Draw pros
            MinorFactionDrawer.DrawFactionIconCircularDisplay(_leftRect, _pros, IconDrawSize, DistributionFactor);

            // Draw dissenters
            MinorFactionDrawer.DrawFactionIconCircularDisplay(_rightRect, _dissenters, IconDrawSize, DistributionFactor);
        }

        void DrawHeader()
        {
            Widgets.Label(_topTextDrawingArea, "WAW.Weshouldapprove".Translate(_policy.Def.label));
        }

        void DrawVotingResult()
        {
            _topTextDrawingArea.position += new Vector2(0, _topTextDrawingArea.height);
            if (_leftPercentage < 0.5f)
            {
                GUI.color = Color.red;
                Widgets.Label(_topTextDrawingArea, "WAW.PolicyWillBeDisaproved".Translate(_policy.Def.label));
            }
            else
            {
                GUI.color = Color.green;
                Widgets.Label(_topTextDrawingArea, "WAW.PolicyWillBeApproved".Translate(_policy.Def.label));
            }
            GUI.color = Color.white;
        }

        bool ShouldDrawOpinionImpacts()
        {
            return (_leftPercentageRaw < 0.5f && _leftPercentage > _leftPercentageRaw)
                || (_leftPercentageRaw > 0.5f && _leftPercentage < _leftPercentageRaw);
        }

        /// <summary>
        /// Draws the opinion impacts based on the voting results.
        /// </summary>
        void DrawOpinionImpacts()
        {
            if (!ShouldDrawOpinionImpacts()) return;

            // Draw negative impact
            GUI.color = new Color(1f, 0.2f, 0);
            _topTextDrawingArea.position += Vector2.up * _topTextDrawingArea.height;
            Widgets.Label(_topTextDrawingArea, "WAW.FactionsUnhappy".Translate(10));

            // Draw positive impact
            GUI.color = Color.white;
            _topTextDrawingArea.position += Vector2.up * _topTextDrawingArea.height;
            Widgets.Label(_topTextDrawingArea, "WAW.GiveDevelopmentPoints".Translate(1));
        }

        void DrawPolicyLabels(Rect inRect)
        {
            _topTextDrawingArea = inRect.TopPart(0.075f);
            Text.Anchor = TextAnchor.MiddleCenter;

            // Draw header
            DrawHeader();

            // Draw voting results
            DrawVotingResult();

            // Draw opinion impacts
            DrawOpinionImpacts();

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawOptionLabels()
        {
            // Draw "Approve" and "Reject" labels
            LabelOnTop(_leftRect, "WAW.ApprovePolicy".Translate());
            LabelOnTop(_rightRect, "WAW.RejectPolicy".Translate());
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawWireFrame(inRect);
            DrawPlayerFaction();
            DrawFactions();
            DrawOptionLabels();
            DrawPolicyLabels(inRect);
        }
    }
}
