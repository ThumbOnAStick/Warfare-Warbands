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
            this.warband = warband;
            GameComponent_WAW.playerWarband.bandMembers = new Dictionary<string, int>(warband.bandMembers);
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
            Rect exitButtonRect = new Rect(inRect.xMax - 30, 0, 30, 30);
            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
            }

            DoArrangeWindow(inRect);

        }

        void DoArrangeWindow(Rect inRect)
        {

            Rect colorLabelRect = new Rect(inRect.x, inRect.y, 100, 50);
            Rect colorBoxRect = new Rect(inRect.x + 130, inRect.y, 22, 22);
            Rect colorSelectorRect = new Rect(inRect.x, inRect.y + 30, inRect.width, colorsHeight);
            Widgets.Label(colorLabelRect, "WAW.ColorOverride".Translate());
            Color color = warband.playerWarbandManager.colorOverride.GetColorOverride();
            Widgets.ColorBox(colorBoxRect, ref color, color);
            bool selectColor = Widgets.ColorSelector(colorSelectorRect, ref color, this.AllApprealColors, out this.colorsHeight, null, 22, 2, null);
            if (selectColor)
            {
                Log.Message("Color: " + color);
                warband.playerWarbandManager.colorOverride.SetColorOverride(color);
            }
            GameComponent_WAW.playerWarband.colorOverride = color;
            Rect costRect = new Rect(30, colorSelectorRect.y + colorsHeight, 100, 50);
            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCostExtra(warband.bandMembers).ToString()));
            Rect techRect = new Rect(300, colorSelectorRect.y + colorsHeight, 100, 50);
            Rect techRectMinus = new Rect(270, colorSelectorRect.y + colorsHeight, entryWidth, entryHeight);
            Rect techRectAddon = new Rect(400, colorSelectorRect.y + colorsHeight, entryWidth, entryHeight);
            bool decreaseTech = Widgets.ButtonImage(techRectMinus, TexUI.ArrowTexLeft);
            Widgets.Label(techRect, "WAW.TechLevel".Translate((int)GameComponent_WAW.playerWarband.techLevel));
            bool addTech = Widgets.ButtonImage(techRectAddon, TexUI.ArrowTexRight);
            if (addTech && GameComponent_WAW.playerWarband.techLevel < TechLevel.Archotech) { GameComponent_WAW.playerWarband.techLevel++; }
            if (decreaseTech && GameComponent_WAW.playerWarband.techLevel > TechLevel.Undefined) { GameComponent_WAW.playerWarband.techLevel -= 1; }
            var allCombatPawns = WarbandUtil.SoldierPawnKindsWithTechLevel(GameComponent_WAW.playerWarband.techLevel);
            Rect outRect = new Rect(inRect.x, colorSelectorRect.yMax + 50f, inRect.width, 200f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 30f, (float)((allCombatPawns.Count() / pawnKindsEachRow + 1) * (descriptionHeight + 10)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float depth = outRect.y;
            int pawnKindsStacked = 0;
            if (allCombatPawns.Count < 1)
            {
                Widgets.Label(new Rect(30, depth, descriptionWidth, descriptionHeight), "WAW.FoundZeroPawns".Translate());
            }
            foreach (PawnKindDef p in allCombatPawns)
            {
                pawnKindsStacked++;
                if (pawnKindsStacked > pawnKindsEachRow)
                {
                    pawnKindsStacked = 1;
                    depth += descriptionHeight + 10;
                }
                float distance = 30 + 110 * (pawnKindsStacked - 1);
                Widgets.Label(new Rect(distance, depth, descriptionWidth, descriptionHeight), p.label + "(" + p.combatPower + ")");
                var amount = GameComponent_WAW.playerWarband.bandMembers[p.defName];
                bool minus = Widgets.ButtonImage(new Rect(distance, depth + 30, entryWidth, entryHeight), TexUI.ArrowTexLeft);
                Widgets.Label(new Rect(distance + entryWidth, depth + 30, entryWidth, entryHeight), amount.ToString());
                bool add = Widgets.ButtonImage(new Rect(distance + entryWidth * 2, depth + 30, entryWidth, entryHeight), TexUI.ArrowTexRight);
                if (minus && GameComponent_WAW.playerWarband.bandMembers[p.defName] > 0) { GameComponent_WAW.playerWarband.bandMembers[p.defName]--; }
                if (add) { GameComponent_WAW.playerWarband.bandMembers[p.defName]++; }
            }

            Widgets.EndScrollView();
            //int recruitButtonWidth = 200;
            //int recruitButtonX = 30;

            bool doRecruit = Widgets.ButtonText(new Rect(30, 400, 200, 50), "WAW.ConfigWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                GameComponent_WAW.playerWarband.SetNewWarBandMembers(warband);

            }

            //bool doUpgrade = Widgets.ButtonText(new Rect(recruitButtonX + recruitButtonWidth + 30, 400, 200, 50), "WAW.UpgradeWarband".Translate());
            //if (doUpgrade)
            //{
            //    this.Close();
            //    //GameComponent_WAW.playerWarband.SetNewWarBandMembers(warband);

            //}

            bool doReset = Widgets.ButtonText(new Rect(330, 350, 100, 20), "WAW.ResetWarband".Translate());
            if (doReset)
            {
                for (int i = 0; i < GameComponent_WAW.playerWarband.bandMembers.Count; i++)
                {
                    var key = GameComponent_WAW.playerWarband.bandMembers.ElementAt(i).Key;
                    GameComponent_WAW.playerWarband.bandMembers[key] = 0;
                }
            }
        }



        private Vector2 scrollPosition;
        private Warband warband;
        private float colorsHeight;
        private readonly float descriptionHeight = 70f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 5;
        private List<Color> allApperalColors;
        private List<Color> AllApprealColors 
        {
            get
            {
                if (this.allApperalColors == null)
                {
                    this.allApperalColors = new List<Color>();
                    foreach (ColorDef colorDef in DefDatabase<ColorDef>.AllDefs)
                    {
                        Color color = colorDef.color;
                        if (!this.allApperalColors.Any((Color x) => x.WithinDiffThresholdFrom(color, 0.15f)))
                        {
                            this.allApperalColors.Add(color);
                        }
                    }
                    this.allApperalColors.SortByColor((Color x) => x);
                }
                return this.allApperalColors;
            }
        }
    }
}
