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
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.UI.WarbandConfigureSteps;

namespace WarfareAndWarbands.Warband
{
    public class Window_ArrangeWarband : Window
    {
        private Vector2 scrollPosition;
        private readonly Map map;
        private readonly float descriptionHeight = 100f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 6;
        private int step = 0;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 500f);
            }
        }
        public Window_ArrangeWarband(Map map)
        {
            this.map = map;
            WarbandUtil.RefreshSoldierPawnKinds();
        }

        public override void DoWindowContents(Rect inRect)
        {
            WarbandUI.DrawExitButton(this, inRect);
            if (step <= 0)
            {
                StepOne.Draw(inRect);
            }
            else if (step == 1)
            {
                StepTwo.Draw(inRect);
            }
            else
            {
                StepThree.Draw(inRect, ref scrollPosition, pawnKindsEachRow, descriptionHeight, descriptionWidth, entryWidth, entryHeight);
                DrawCost(inRect);
                DrawRecruitButton(inRect);
            }
            WarbandUI.DrawNextStepButton(inRect, ref step);

        }

        void DrawCost(Rect inRect)
        {

            Rect costRect = new Rect(30, inRect.y, 200, 50);
            Rect balanceRect = new Rect(30, costRect.yMax + 10, 200, 50);

            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCostEstablishment().ToString()));
            Widgets.Label(balanceRect, "WAW.AccountBalance".Translate(GameComponent_WAW.playerBankAccount.Balance.ToString()));

        }

        void DrawRecruitButton(Rect inRect)
        {
            bool doRecruit = Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2 - 100, 350, 200, 50), "WAW.RecruitWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                GameComponent_WAW.playerWarband.CreateWarbandWorldObject(map);
            }
        }

      
    }
}
