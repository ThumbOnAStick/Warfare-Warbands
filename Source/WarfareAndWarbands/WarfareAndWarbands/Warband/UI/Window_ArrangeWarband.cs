using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

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
            Widgets.Label(costRect, "WAW.Cost".Translate(GameComponent_WAW.playerWarband.GetCost().ToString()));

            var allCombatPawns = WarbandUtil.SoldierPawnKinds();
            Rect outRect = new Rect(inRect.x, exitButtonRect.yMax + 50f, inRect.width, 200f);
            Rect viewRect = new Rect(inRect.x, outRect.y, inRect.width - 30f, (float)(allCombatPawns.Count()/ pawnKindsEachRow * (descriptionHeight + 10)));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float depth = outRect.y;
            int pawnKindsStacked = 0;
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
            bool doRecruit = Widgets.ButtonText(new Rect(150, 400, 200, 50), "WAW.RecruitWarband".Translate());
            if (doRecruit)
            {
                this.Close();
                GameComponent_WAW.playerWarband.CreateWorldObject();

            }


        }

        private Vector2 scrollPosition;
        private readonly float descriptionHeight = 70f;
        private readonly float descriptionWidth = 100f;
        private readonly float entryHeight = 20f;
        private readonly float entryWidth = 20f;

        private readonly int pawnKindsEachRow = 3;

    }
}
