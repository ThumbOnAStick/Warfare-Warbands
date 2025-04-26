using System.Collections.Generic;
using UnityEngine;
using Verse;
using WarbandWarfareQuestline.League.MinorFactions;

namespace WarbandWarfareQuestline.League.UI
{
    class Window_KickoutCongress : Window_Congress
    {
        private readonly MinorFactionSettlement _minorFactionBase;

        public Window_KickoutCongress(List<MinorFaction> pros, List<MinorFaction> dissenters, MinorFactionSettlement minorFactionBase) : base(pros, dissenters)
        {
            _minorFactionBase = minorFactionBase;
        }

        void DrawIcon()
        {
            _topTextDrawingArea.position += new Vector2(0, _topTextDrawingArea.height);
            GUI.color = _minorFactionBase.MinorFaction.FactionColor;
            Vector2 pivot = _topTextDrawingArea.position + Vector2.right * (_topTextDrawingArea.width / 2 - _topTextDrawingArea.height / 2);
            Widgets.DrawTextureFitted(new Rect(pivot, Vector2.one * _topTextDrawingArea.height), _minorFactionBase.MinorFaction.GetFactionIcon(), 1);
        }

        void DrawProposal()
        {
            _topTextDrawingArea.position += new Vector2(0, _topTextDrawingArea.height + Margin);
            Widgets.Label(_topTextDrawingArea, "WAW.KickoutProposal".Translate(_minorFactionBase.MinorFaction.FactionName));
        }

        protected override void DrawTopicDescriptions(Rect inRect)
        {
            base.DrawTopicDescriptions(inRect);

            // Draw faction icon
            DrawIcon();

            // Draw proposal
            DrawProposal();

            Text.Anchor = TextAnchor.UpperLeft;
        }

        protected override void DoApproveResult()
        {
            base.DoApproveResult();
            if (_minorFactionBase != null)
            {
                GameComponent_League.Instance.RemoveFactionFromLeague(_minorFactionBase);
            }
        }
    

    }
}
