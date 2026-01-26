using UnityEngine;
using Verse;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using WarfareAndWarbands.Warband;

namespace WarfareAndWarbands.Mercenary
{
    public class Dialog_WarbandLoot : Window
    {
        private readonly MapParent mapP;
        private readonly int elementHeight = 50;
        private readonly Warband.Warband looterWarband;
        private readonly IEnumerable<CompMercenary> mercenaryComps;
        private Vector2 scrollPosition;
        private List<Thing> toBesold;
        private List<Thing> lootList; // Changed from IEnumerable to List
        private byte[] sellOutList;

        public Dialog_WarbandLoot(MapParent _mapP)
        {
            this.mapP = _mapP;
            toBesold = new List<Thing>();
            // Materialize the query immediately to a List. 
            // This prevents re-running the map search every single frame/iteration.
            this.lootList = AllMapItems.ToList(); 
            this.mercenaryComps = GetAllMercenaryComps;
            this.looterWarband = GetLooterWarband;
            
            // Initialize based on actual count, preventing IndexOutOfRange exceptions for >99 items
            this.sellOutList = new byte[this.lootList.Count]; 
            this.doCloseX = true;
        }

        private IEnumerable<Thing> AllMapItems => this.mapP.Map.listerThings.AllThings.
         Where(x => x.def.category.Equals(ThingCategory.Item) &&
         x.def.BaseMarketValue > 0.5f);

        private Warband.Warband GetLooterWarband 
        {
            get
            {
                if (mapP?.Map?.mapPawns == null)
                {
                    return null;
                }

                CompMercenary mercComp = mercenaryComps
                    .FirstOrDefault(comp => comp != null && comp.ServesPlayerFaction);

                return mercComp?.GetWarband();
            }
           
        }

        private IEnumerable<CompMercenary> GetAllMercenaryComps
        {
            get
            {
                return mapP.Map.mapPawns.AllHumanlike
                   .Select(x => x.TryGetComp<CompMercenary>()).Where(x => x != null && x.ServesPlayerFaction) ?? new List<CompMercenary>();
            }
           
        }

        bool DrawSellLootButton(Rect rowRect, Thing loot, int index)
        {
            // Draw button
            if (Widgets.ButtonText(rowRect, "WAW.SellLoot".Translate()))
            {
                // Sell loot
                this.sellOutList[index] = 1;
                SetLootToBeSold(loot);
                return true;
            }
            return false;
        }

        void SetLootToBeSold(Thing loot)
        {
            toBesold.Add(loot);
        }

        void RetreatAll()
        {
            foreach (var mercanryComp in mercenaryComps)
            {
                mercanryComp.Retreat();
            }
        }

        void OnConfirm()
        {
            looterWarband?.playerWarbandManager?.lootManager?.StoreAll(toBesold); // Sell selected loot
            RetreatAll();
            Close();
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Draw warband name
            if(looterWarband!=null) Widgets.Label(inRect.TopPart(0.1f), looterWarband.Label);

            // Draw a list of all loot
            var outRect = inRect.TopPart(.5f).BottomPart(0.8f);
            float elementDistance = elementHeight + Margin;
            var viewRect = new Rect(outRect.x, outRect.y + Margin, outRect.width - Margin, elementDistance * lootList.Count);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            
            // Optimization: Calculate visible range to render only what is on screen
            int firstVisibleIndex = (int)(scrollPosition.y / elementDistance);
            int lastVisibleIndex = (int)((scrollPosition.y + outRect.height) / elementDistance) + 1;
            
            firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, lootList.Count);
            lastVisibleIndex = Mathf.Clamp(lastVisibleIndex, 0, lootList.Count);

            for (int i = firstVisibleIndex; i < lastVisibleIndex; i++)
            {
                Rect rowRect = new Rect(viewRect.x, viewRect.y + elementDistance * i, viewRect.width, elementHeight);
                Thing loot = lootList[i]; // Direct index access (O(1)) instead of ElementAt (O(N))
                
                // Icon     
                Widgets.ThingIcon(rowRect.LeftPart(elementHeight / rowRect.width), loot);
                // Number
                Widgets.Label(rowRect.LeftHalf().RightHalf(), loot.stackCount.ToString());

                // If loot is not in sold list
                Rect buttonRect = rowRect.RightPart(elementHeight / rowRect.width);
                if (this.sellOutList[i] == 0)
                {
                    // Sell button
                    DrawSellLootButton(buttonRect, loot, i);
                }
                else
                {
                    // Remove from sells button
                    buttonRect.position += Vector2.left * elementHeight;
                    if (Widgets.ButtonImage(buttonRect.ScaledBy(0.9f), TexButton.CloseXSmall))
                    {
                        this.sellOutList[i] = 1; // Logic note: Should this be 0 to un-sell? Changed 1 back to 0 below assuming toggle logic
                        toBesold.Remove(loot);
                        this.sellOutList[i] = 0; // Fix: Reset flag so sell button reappears
                    }
                }
            }
            Widgets.EndScrollView();


            // Draw select all button
            Rect selectAllButtonRect = inRect.BottomHalf().TopPart(0.2f).LeftHalf();
            if (Widgets.ButtonText(selectAllButtonRect, "WAW.SelectAll".Translate(), false))
            {
                toBesold = lootList.ToList();
                // Update tracking array
                for (int k = 0; k < sellOutList.Length; k++) sellOutList[k] = 1;
            }

            if (toBesold.Count < 1)
            {
                return;
            }

            //Draw confirm button
            Rect confirmButtonRect = inRect.BottomPart(0.2f);
            if (Widgets.ButtonText(confirmButtonRect, "WAW.GatherMapLoot".Translate(this.looterWarband?.Label)))
            {
                OnConfirm();
            }

        }
    }
}
