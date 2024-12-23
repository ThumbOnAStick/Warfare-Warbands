﻿using RimWorld;
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

namespace WarfareAndWarbands.Warband
{
    public class Window_ArrangeWarband : Window
    {
        private Vector2 scrollPosition;
        private Map map;
        private readonly float descriptionHeight = 100f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 5;

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
                GameComponent_WAW.playerWarband.CreateWarbandWorldObject(map);

            }
        }

      
    }
}
