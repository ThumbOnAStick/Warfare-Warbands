using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using WarfareAndWarbands.Warband.WarbandComponents;
using WarfareAndWarbands.Warfare.UI;

namespace WarfareAndWarbands.Warband.UI
{
    class Window_LootManagement : Window
    {
        private readonly PlayerWarbandLootManager _lootManager;
        private List<Thing> _toBeSold;  
        private Vector2 _scrollPosition = Vector2.zero;

        private const float elementHight = 30;

        public Window_LootManagement()
        {
            _toBeSold = new List<Thing>();
        }

        public Window_LootManagement(PlayerWarbandLootManager lootManager) : this()
        {
            _lootManager = lootManager;
            _lootManager.Storage.RemoveAll(x => x == null);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Do closex 
            this.DrawCloseButton(inRect);

            // Draw a list of all loot
            var lootList = _lootManager.Storage;
            var outRect = inRect.TopPart(.5f).BottomPart(0.8f);
            float elementDistance = elementHight + Margin;
            var viewRect = new Rect(outRect.x, outRect.y + Margin, outRect.width - Margin, elementDistance * lootList.Count);
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            for (int i = 0; i < lootList.Count; i++)
            {
                Rect rowRect = new Rect(viewRect.x, viewRect.y + elementDistance * i, viewRect.width, elementHight);
                Thing loot = lootList[i]; 
                // Icon     
                Widgets.ThingIcon(rowRect.LeftPart(elementHight / rowRect.width), loot);
                // Number
                Widgets.Label(rowRect.LeftHalf().RightHalf(), loot.stackCount.ToString());

                // If loot is not in sold list
                Rect buttonRect = rowRect.RightPart(elementHight / rowRect.width);
                if (!_toBeSold.Contains(loot))
                {
                    // Sell button
                    DrawSellLootButton(buttonRect, loot);
                }
                else
                {
                    // Small X button
                    buttonRect.position += Vector2.left * elementHight;
                    if (Widgets.ButtonImage(buttonRect.ScaledBy(0.9f), TexButton.CloseXSmall))
                    {
                        _toBeSold.Remove(loot);
                    }
                }

            }

            Widgets.EndScrollView();

            // Draw select all button
            Rect selectAllButtonRect = inRect.BottomHalf().TopPart(0.2f).LeftHalf();
            if (Widgets.ButtonText(selectAllButtonRect, "WAW.SelectAll".Translate()))
            {
                _toBeSold = lootList.ToList();
            }

            if (_toBeSold.Count < 1)
            {
                return;
            }

            //Draw confirm button
            Rect confirmButtonRect = inRect.BottomPart(0.2f);
            if (Widgets.ButtonText(confirmButtonRect, "Confirm".Translate()))
            {
                // Sell selected loot
                _lootManager.SetToBeSold(_toBeSold);
                Find.WindowStack.Add(new FloatMenu(WithDrawLootOptions().ToList()));
            }

        }


        private IEnumerable<FloatMenuOption> WithDrawLootOptions()
        {
            yield return new FloatMenuOption("WAW.InItems".Translate(), delegate {_lootManager.WithdrawLoot(); Close(); });
            yield return new FloatMenuOption("WAW.InSilvers".Translate(), delegate { _lootManager.WithdrawLootInSilver(); Close(); });
            yield return new FloatMenuOption("WAW.DepositeLoots".Translate(), delegate { _lootManager.WithdrawLootToBank(); Close(); });

        }

        bool DrawSellLootButton(Rect rowRect, Thing loot)
        {
            // Draw button
            if (Widgets.ButtonText(rowRect, "WAW.SellLoot".Translate()))
            {
                // Sell loot
                SetLootToBeSold(loot);
                return true;
            }
            return false;
        }

        void SetLootToBeSold(Thing loot)
        {
            _toBeSold.Add(loot);
        }
    }
}
