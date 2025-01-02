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
        private readonly float descriptionHeight = 100f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 6;
        private readonly Pawn leader;
        private readonly Caravan caravan;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 500f);
            }
        }

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
                StepThree.Draw(inRect, ref scrollPosition, pawnKindsEachRow, descriptionHeight, descriptionWidth, entryWidth, entryHeight);
                DrawCost(inRect);
                DrawRecruitButton(inRect);
            }
            WarbandUI.DrawNextStepButton(inRect, ref _step);

        }

        void DrawCost(Rect inRect)
        {

            Rect costRect = new Rect(30, inRect.y, 200, 50);
            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCostEstablishment().ToString()));
        }


        void DrawRecruitButton(Rect inRect)
        {
            bool doRecruit = Widgets.ButtonText(new Rect(inRect.x + inRect.width / 2 - 100, 350, 200, 50), "WAW.RecruitWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                if (!GameComponent_WAW.playerWarband.ValidateCreation(caravan))
                {
                    return;
                }
                if (!CaravanWarbandUtility.TryToSpendSilverFromCaravan(caravan, GameComponent_WAW.playerWarband.GetCostEstablishment()))
                {
                    return;
                }
                WarbandRecruitingUtil.SpawnRecruitingWarband(this.caravan, leader);

            }
        }




    }
}
