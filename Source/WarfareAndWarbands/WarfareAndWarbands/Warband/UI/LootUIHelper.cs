using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Warband.UI
{
    public static class LootUIHelper
    {
        private const float ElementSize = 30f;
        private const float ElementHeight = 50f;

        public static void DrawLootList(
            Rect outRect,
            ref Vector2 scrollPosition,
            List<Thing> lootList,
            HashSet<Thing> selectedItems,
            string sellButtonLabel,
            Action<Thing, bool> onItemToggled,
            bool useOptimizedRendering = false,
            float elementSize = ElementHeight)
        {
            if (lootList == null || lootList.Count == 0)
            {
                return;
            }

            float elementDistance = elementSize + 5f;
            var viewRect = new Rect(outRect.x, outRect.y + 5f, outRect.width - 16f, elementDistance * lootList.Count);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            int firstIndex = 0;
            int lastIndex = lootList.Count;

            if (useOptimizedRendering && lootList.Count > 20)
            {
                firstIndex = (int)(scrollPosition.y / elementDistance);
                lastIndex = (int)((scrollPosition.y + outRect.height) / elementDistance) + 1;
                firstIndex = Mathf.Clamp(firstIndex, 0, lootList.Count);
                lastIndex = Mathf.Clamp(lastIndex, 0, lootList.Count);
            }

            for (int i = firstIndex; i < lastIndex; i++)
            {
                Rect rowRect = new Rect(viewRect.x, viewRect.y + elementDistance * i, viewRect.width, elementSize);
                Thing loot = lootList[i];

                if (loot == null) continue;

                DrawLootRow(rowRect, loot, selectedItems.Contains(loot), sellButtonLabel, elementSize, isSelected =>
                {
                    onItemToggled?.Invoke(loot, isSelected);
                });
            }

            Widgets.EndScrollView();
        }

        private static void DrawLootRow(Rect rowRect, Thing loot, bool isSelected, string sellButtonLabel, float elementSize, Action<bool> onToggle)
        {
            Widgets.ThingIcon(rowRect.LeftPart(elementSize / rowRect.width), loot);
            Widgets.Label(rowRect.LeftHalf().RightHalf(), loot.stackCount.ToString());

            Rect buttonRect = rowRect.RightPart(elementSize * 2 / rowRect.width);

            if (!isSelected)
            {
                if (Widgets.ButtonText(buttonRect, sellButtonLabel.Translate()))
                {
                    onToggle?.Invoke(true);
                }
            }
            else
            {
                buttonRect.position += Vector2.left * elementSize;
                buttonRect.width -= elementSize;
                if (Widgets.ButtonImage(buttonRect.ScaledBy(0.5f), TexButton.CloseXSmall))
                {
                    onToggle?.Invoke(false);
                }
            }
        }

        public static bool DrawSelectAllButton(Rect rect, Action onSelectAll = null)
        {
            if (Widgets.ButtonText(rect, "WAW.SelectAll".Translate(), false))
            {
                onSelectAll?.Invoke();
                return true;
            }
            return false;
        }

        public static bool DrawConfirmButton(Rect rect, string buttonText, string warbandLabel = null)
        {
            if (Widgets.ButtonText(rect, buttonText.Translate(warbandLabel)))
            {
                return true;
            }
            return false;
        }

        public class LootSelectionManager
        {
            private readonly HashSet<Thing> _selectedItems = new HashSet<Thing>();

            public IEnumerable<Thing> SelectedItems => _selectedItems;
            public int Count => _selectedItems.Count;

            public bool IsSelected(Thing thing) => _selectedItems.Contains(thing);

            public void Toggle(Thing thing)
            {
                if (_selectedItems.Contains(thing))
                {
                    _selectedItems.Remove(thing);
                }
                else
                {
                    _selectedItems.Add(thing);
                }
            }

            public void Select(Thing thing) => _selectedItems.Add(thing);

            public void Deselect(Thing thing) => _selectedItems.Remove(thing);

            public void SelectAll(IEnumerable<Thing> items)
            {
                _selectedItems.Clear();
                foreach (var item in items)
                {
                    _selectedItems.Add(item);
                }
            }

            public void Clear() => _selectedItems.Clear();

            public List<Thing> ToList() => _selectedItems.ToList();
        }
    }
}
