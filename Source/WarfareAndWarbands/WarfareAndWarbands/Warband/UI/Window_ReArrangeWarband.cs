using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband
{
    public class Window_ReArrangeWarband : Window
    {

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 500f);
            }
        }
        public Window_ReArrangeWarband(Warband warband)
        {
            WarbandUtil.RefreshSoldierPawnKinds();
            this.warband = warband;
            for (int i = 0; i < GameComponent_WAW.playerWarband.bandMembers.Count; i++)
            {
                var ele = GameComponent_WAW.playerWarband.bandMembers.ElementAt(i);
                GameComponent_WAW.playerWarband.bandMembers[ele.Key] = 0;
                if (warband.bandMembers.ContainsKey(ele.Key))
                {
                    GameComponent_WAW.playerWarband.bandMembers[ele.Key] = warband.bandMembers[ele.Key];
                }
            }
        }
        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

        }
        public override void DoWindowContents(Rect inRect)
        {
            //Basics
            if (this.warband == null)
            {
                this.Close();
            }
            WarbandUI.DrawExitButton(this, inRect);
            DoArrangeWindow(inRect);

        }

        void DoArrangeWindow(Rect inRect)
        {

            WarbandUI.DrawColorPanel(inRect, out colorsHeight, out Rect colorSelectorRect, this.warband);

            WarbandUI.DrawPawnSelection(inRect, colorSelectorRect, ref scrollPosition, pawnKindsEachRow, colorsHeight, descriptionHeight, descriptionWidth, entryWidth, entryHeight);

            WarbandUI.DrawResetButton();

            DrawRecruitButton();

            DrawExtraCost(colorSelectorRect);

        }

        void DrawExtraCost(Rect colorSelectorRect)
        {
            Rect costRect = new Rect(30, colorSelectorRect.y + colorsHeight, 200, 50);
            string costLabel = "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCostExtra(warband.bandMembers, warband.playerWarbandManager.NewRecruitCostMultiplier).ToString());
            Widgets.Label(costRect, costLabel + $"(-{(1 - warband.playerWarbandManager.NewRecruitCostMultiplier) * 100}%)");

        }

        void DrawRecruitButton()
        {
            bool doRecruit = Widgets.ButtonText(new Rect(330 + 50 - 100, 400, 200, 50), "WAW.ConfigWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                GameComponent_WAW.playerWarband.SetNewWarBandMembers(warband);

            }
        }



        private Vector2 scrollPosition;
        private Warband warband;
        private float colorsHeight = 200;
        private readonly float descriptionHeight = 70f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 5;
     
    }
}
