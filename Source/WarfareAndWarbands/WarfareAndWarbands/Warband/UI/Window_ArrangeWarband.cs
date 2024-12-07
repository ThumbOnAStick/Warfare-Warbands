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

namespace WarfareAndWarbands.Warband
{
    public class Window_ArrangeWarband : Window
    {

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }
        public Window_ArrangeWarband(Map map)
        {
            this.map = map;
            WarbandUtil.Refresh();
        }
        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();

        }
        public override void DoWindowContents(Rect inRect)
        {
            Rect exitButtonRect = new Rect(430, 0, 30, 30);

            bool exit = Widgets.ButtonImage(exitButtonRect, TexButton.CloseXSmall);
            if (exit)
            {
                this.Close();
            }
            Rect costRect = new Rect(30, 0, 100, 50);
            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCostEstablishment().ToString()));
        
            Rect techRect = new Rect(300, 30, 100, 50);
            Rect techRectMinus = new Rect(270, 30, entryWidth, entryHeight);
            Rect techRectAddon = new Rect(400, 30, entryWidth, entryHeight);
            bool decreaseTech = Widgets.ButtonImage(techRectMinus, TexUI.ArrowTexLeft);
            Widgets.Label(techRect, "WAW.TechLevel".Translate((int)GameComponent_WAW.playerWarband.techLevel));
            bool addTech = Widgets.ButtonImage(techRectAddon, TexUI.ArrowTexRight);
            if (addTech && GameComponent_WAW.playerWarband.techLevel < TechLevel.Archotech) { GameComponent_WAW.playerWarband.techLevel++; }
            if (decreaseTech && GameComponent_WAW.playerWarband.techLevel > TechLevel.Undefined) { GameComponent_WAW.playerWarband.techLevel -= 1; }

            var allCombatPawns = WarbandUtil.SoldierPawnKindsWithTechLevel(GameComponent_WAW.playerWarband.techLevel);
            Rect outRect = new Rect(inRect.x, exitButtonRect.yMax + 50f, inRect.width, 200f);
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
                Widgets.Label(new Rect(distance, depth, descriptionWidth, descriptionHeight), WarbandUI.PawnKindLabel(p) + "(" + p.combatPower + ")");
                var amount = GameComponent_WAW.playerWarband.bandMembers[p.defName];
                bool minus = Widgets.ButtonImage(new Rect(distance, depth + 30, entryWidth, entryHeight), TexUI.ArrowTexLeft);
                Widgets.Label(new Rect(distance + entryWidth, depth + 30, entryWidth, entryHeight), amount.ToString());
                bool add = Widgets.ButtonImage(new Rect(distance + entryWidth * 2, depth + 30, entryWidth, entryHeight), TexUI.ArrowTexRight);
                if (minus && GameComponent_WAW.playerWarband.bandMembers[p.defName] > 0) { GameComponent_WAW.playerWarband.bandMembers[p.defName]--; }
                if (add) { GameComponent_WAW.playerWarband.bandMembers[p.defName]++; }
            }

            Widgets.EndScrollView();
            bool doRecruit = Widgets.ButtonText(new Rect(150, 400, 200, 50), "WAW.RecruitWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                GameComponent_WAW.playerWarband.CreateWarbandWorldObject(map);

            }

            bool doReset = Widgets.ButtonText(new Rect(330, 370, 100, 20), "WAW.ResetWarband".Translate());
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
        private Map map;
        private readonly float descriptionHeight = 100f;
        private readonly float descriptionWidth = 120f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;
        private readonly int pawnKindsEachRow = 3;

    }
}
