using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using WarfareAndWarbands;
using WarfareAndWarbands.Warband.UI;

namespace WarbandWarfareQuestline.League.Policies.UI
{
    public class Window_PolicyTree : Window
    {
        private PolicyTree _policyTree;

        public override Vector2 InitialSize => new Vector2(1000, 500);

        private static readonly Vector2Int messageSize = new Vector2Int(1000, 50);

        private Rect _currentInrect; 

        public Window_PolicyTree()
        {
            _policyTree = new PolicyTree();
        }

        public Window_PolicyTree(PolicyTree policyTree)
        {
            _policyTree = policyTree;
        }

        void SetCurrentInRect(Rect inRect)
        {
            _currentInrect = inRect;
        }   

        void DrawRedMessage(string translationKey)
        {
            GUI.color = Color.red;
            Rect bottomLeft = new Rect(_currentInrect.x, _currentInrect.yMax - messageSize.y, messageSize.x, messageSize.y);
            Widgets.Label(bottomLeft, translationKey.Translate());
            GUI.color = Color.white;
        }

        public override void DoWindowContents(Rect inRect)
        {
            SetCurrentInRect(inRect);
            float height = inRect.y;
            for (int i = 0; i < _policyTree.Roots.Count; i++)
            {
                Rect rootRect = new Rect(inRect.x, height + PolicyDrawer.margin + PolicyDrawer.policyBoxHeight, PolicyDrawer.policyBoxWidth, PolicyDrawer.policyBoxHeight);
                _policyTree.Roots[i].DrawPolicyRecursively(rootRect, false, out height);
            }

            // draw close button
            int closeButtonSize = 25;
            Rect closeButtonRect = new Rect(inRect.xMax - closeButtonSize, 0, closeButtonSize, closeButtonSize);
            if (Widgets.ButtonImage(closeButtonRect, TexButton.CloseXSmall))
            {
                Close();
            }

            // if there's no factions in the league, show a message
            if (GameComponent_League.Instance.NoFactionInLeague() && !DebugSettings.godMode)
            {
                DrawRedMessage("WAW.NoFactionsInLeague");
                return;
            }

            // if development points are not enough, show a message
            if (GameComponent_League.Instance.PointsInssufficient() && !DebugSettings.godMode)
            {
                DrawRedMessage("WAW.NotEnoughDevelopmentPoints");
            }

         

        }

    }
}
