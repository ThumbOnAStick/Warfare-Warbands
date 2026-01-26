using UnityEngine;
using Verse;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;

namespace WarfareAndWarbands.Mercenary
{
    public class Dialog_WarbandLoot : Window
    {
        private readonly MapParent mapP;
        private readonly int elementHeight = 50;
        private readonly int elementWidth = 50;
        private Vector2 scrollPosition; 
        private List<Thing> toBesold;
        private IEnumerable<Thing> lootList;
        public Dialog_WarbandLoot(MapParent _mapP)
        {
            this.mapP = _mapP;
            // Assign item list
            this.lootList = AllMapItems;
        }

        private IEnumerable<Thing> AllMapItems => this.mapP.Map.listerThings.AllThings.
            Where(x => x.def.category.Equals(ThingCategory.Item) &&
            x.def.BaseMarketValue > 0.5f);
      

        public override void DoWindowContents(Rect inRect)
        {
            // Draw a list of all loot
            var outRect = inRect.TopPart(.5f).BottomPart(0.8f);
            float elementDistance = elementHeight + Margin;
            var viewRect = new Rect(outRect.x, outRect.y + Margin, outRect.width - Margin, elementDistance * lootList.Count);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            for (int i = 0; i < this.lootList.Count(); i++)
            {
                Rect rowRect = new Rect(viewRect.x, viewRect.y + elementDistance * i, viewRect.width, elementHight);
                Thing loot = lootList.ElementAt(i);
                // Icon     
                Widgets.ThingIcon(rowRect.LeftPart(elementHeight / rowRect.width), loot);
                // Number
                Widgets.Label(rowRect.LeftHalf().RightHalf(), loot.stackCount.ToString());

                // If loot is not in sold list
                Rect buttonRect = rowRect.RightPart(elementHeight / rowRect.width);
                if (!toBesold.Contains(loot))
                {
                    // Sell button
                    DrawSellLootButton(buttonRect, loot);
                }
                else
                {
                    // Small X button
                    buttonRect.position += Vector2.left * elementHeight;
                    if (Widgets.ButtonImage(buttonRect.ScaledBy(0.9f), TexButton.CloseXSmall))
                    {
                        toBesold.Remove(loot);
                    }
                }
            }

    }
}
