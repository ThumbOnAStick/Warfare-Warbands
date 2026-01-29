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
        private readonly HashSet<Thing> _selectedItems;
        private Vector2 _scrollPosition = Vector2.zero;

        private const float elementSize = 30;

        public Window_LootManagement()
        {
            _selectedItems = new HashSet<Thing>();
        }

        public Window_LootManagement(PlayerWarbandLootManager lootManager) : this()
        {
            _lootManager = lootManager;
            _lootManager.Storage.RemoveAll(x => x == null);
        }

        public override void DoWindowContents(Rect inRect)
        {
            this.DrawCloseButton(inRect);

            var lootList = _lootManager.Storage.ToList();
            var outRect = inRect.TopPart(.7f).BottomPart(0.8f);

            LootUIHelper.DrawLootList(
                outRect,
                ref _scrollPosition,
                lootList,
                _selectedItems,
                "WAW.SellLoot",
                (thing, isSelected) =>
                {
                    if (isSelected)
                    {
                        _selectedItems.Add(thing);
                    }
                    else
                    {
                        _selectedItems.Remove(thing);
                    }
                },
                false,
                elementSize);

            Rect selectAllButtonRect = new Rect(inRect.x, inRect.y + inRect.height * 0.7f, inRect.width * 0.5f, inRect.height * 0.1f);
            LootUIHelper.DrawSelectAllButton(selectAllButtonRect, () =>
            {
                _selectedItems.Clear();
                foreach (var item in lootList)
                {
                    _selectedItems.Add(item);
                }
            });

            if (_selectedItems.Count < 1)
            {
                return;
            }

            Rect confirmButtonRect = inRect.BottomPart(0.2f);
            if(LootUIHelper.DrawConfirmButton(confirmButtonRect, "Confirm"))
            {
                _lootManager.SetToBeSold(_selectedItems.ToList());
                Find.WindowStack.Add(new FloatMenu(WithDrawLootOptions().ToList()));
            }
        }

        private IEnumerable<FloatMenuOption> WithDrawLootOptions()
        {
            yield return new FloatMenuOption("WAW.InItems".Translate(), delegate {_lootManager.WithdrawLoot(); Close(); });
            yield return new FloatMenuOption("WAW.InSilvers".Translate(), delegate { _lootManager.WithdrawLootInSilver(); Close(); });
            yield return new FloatMenuOption("WAW.DepositeLoots".Translate(), delegate { _lootManager.WithdrawLootToBank(); Close(); });
        }
    }
}
