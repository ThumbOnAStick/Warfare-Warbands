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

namespace WarfareAndWarbands.Warband.WAWCaravan.UI
{
    internal class Window_ArrangeWarband_Caravan : Window
    {
        private Vector2 scrollPosition;
        private readonly float descriptionHeight = 100f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 5;
        Pawn leader;
        Caravan caravan;

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

            WarbandUI.DrawColorPanel(inRect, out float colorsHeight, out Rect colorSelectorRect, null);

            WarbandUI.DrawPawnSelection(inRect, colorSelectorRect, ref scrollPosition, pawnKindsEachRow, colorsHeight, descriptionHeight, descriptionWidth, entryWidth, entryHeight);

            WarbandUI.DrawResetButton();

            DrawCost(colorSelectorRect, colorsHeight);
            DrawRecruitButton();

        }

        void DrawCost(Rect colorSelectorRect, float colorsHeight)
        {

            Rect costRect = new Rect(30, colorSelectorRect.y + colorsHeight, 200, 50);
            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCostEstablishment().ToString()));
        }

        void DrawRecruitButton()
        {
            bool doRecruit = Widgets.ButtonText(new Rect(330 + 50 - 100, 400, 200, 50), "WAW.RecruitWarband".Translate());
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
