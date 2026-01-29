using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using WarfareAndWarbands.Warband.UI;
using static UnityEngine.GraphicsBuffer;
using WarfareAndWarbands.Warband.WarbandRecruiting;
using WarfareAndWarbands.Warband.UI.WarbandConfigureSteps;

namespace WarfareAndWarbands.Warband.WAWCaravan.UI
{
    internal class Window_ArrangeWarband_Caravan : Window
    {
        private Vector2 scrollPosition;
        private int _step = 0;
        private readonly Pawn leader;
        private readonly Caravan caravan;

        public override Vector2 InitialSize => WarbandUI.PawnSelectionPanelSize;

        public Window_ArrangeWarband_Caravan()
        {
        }

        public Window_ArrangeWarband_Caravan(Pawn leader, Caravan caravan)
        {
            this.leader = leader;
            this.caravan = caravan;
        }


        public override void DoWindowContents(Rect inRect)
        {
            WarbandUI.DrawExitButton(this, inRect);
            if (_step <= 0)
            {
                StepOne.Draw(inRect);
            }
            else if (_step == 1)
            {
                StepTwo.Draw(inRect);
            }
            else
            {
                StepThree.Draw(inRect, ref scrollPosition);
                DrawCost(inRect);
                DrawRecruitButton(inRect);
            }
            if (_step < 2) WarbandUI.DrawNextStepButton(inRect, ref _step);

        }

        void DrawCost(Rect inRect)
        {

            Rect costRect = new Rect(30, inRect.y, 200, 50); 
            Rect balanceRect = new Rect(30, costRect.yMax + 10, 200, 50);
            Widgets.Label(balanceRect, "WAW.AccountBalance".Translate(GameComponent_WAW.playerBankAccount.Balance.ToString()));
            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarbandPreset.GetCostEstablishment().ToString()));
        }


        void DrawRecruitButton(Rect inRect)
        {
            bool doRecruit = Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2 - 100, 350, 200, 50), "WAW.RecruitWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                if (!GameComponent_WAW.playerWarbandPreset.ValidateCreation(caravan))
                {
                    return;
                }
                if (!CaravanWarbandUtility.TryToSpendSilverFromCaravan(caravan, GameComponent_WAW.playerWarbandPreset.GetCostEstablishment()))
                {
                    return;
                }
                WarbandRecruitingUtil.SpawnRecruitingWarband(this.caravan, leader);

            }
        }




    }
}
