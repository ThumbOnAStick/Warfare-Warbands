using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.UI.WarbandConfigureSteps;
using WarfareAndWarbands.Warband.UI;
using RimWorld.Planet;

namespace WarfareAndWarbands.Warband.VassalWarband.UI
{
    public class Window_CreateVassalWarband : Window
    {
        private Vector2 scrollPosition;
        private readonly float descriptionHeight = 100f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 6;
        private readonly int _budget = 0;
        private int step = 0;
        private VassalHolder holder;
        private GlobalTargetInfo target;

        public Window_CreateVassalWarband()
        {
            WarbandUtil.RefreshSoldierPawnKinds();
        }

        public Window_CreateVassalWarband(int budget) : this() 
        {
            this._budget = budget;
        }
        public Window_CreateVassalWarband(GlobalTargetInfo info, int budget) : this(budget)
        {
            this.target = info;
        }
        public Window_CreateVassalWarband(GlobalTargetInfo info, VassalHolder holder) : this()
        {
            this.target = info;
            this.holder = holder;
            _budget = holder.Budget;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 500f);
            }
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
            Widgets.Label(balanceRect, "WAW.VassalBudget".Translate(_budget));

        }

        void DrawRecruitButton(Rect inRect)
        {
            bool doRecruit = Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2 - 100, 350, 200, 50), "WAW.RecruitWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                GameComponent_WAW.playerWarband.CreateVassalWarbandWorldObject(this.target, this._budget);
                this.holder.OnUsed();
            }
        }
    }
}
