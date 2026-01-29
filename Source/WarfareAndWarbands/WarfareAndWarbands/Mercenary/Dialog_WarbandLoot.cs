using UnityEngine;
using Verse;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Mercenary
{
    public class Dialog_WarbandLoot : Window
    {
        private readonly MapParent mapP;
        private readonly int elementHeight = 50;
        private readonly Warband.Warband looterWarband;
        private readonly IEnumerable<CompMercenary> mercenaryComps;
        private readonly List<Thing> lootList;
        private readonly HashSet<Thing> selectedItems;
        private Vector2 scrollPosition;

        public Dialog_WarbandLoot(MapParent _mapP)
        {
            this.mapP = _mapP;
            this.lootList = AllMapItems.ToList();
            this.selectedItems = new HashSet<Thing>();
            this.mercenaryComps = GetAllMercenaryComps;
            this.looterWarband = GetLooterWarband;
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
                    .FirstOrDefault(comp => comp != null && comp.IsPlayerControlledMercenary);

                return mercComp?.GetWarband();
            }
        }

        private IEnumerable<CompMercenary> GetAllMercenaryComps
        {
            get
            {
                return mapP.Map.mapPawns.AllHumanlike
                   .Select(x => x.TryGetComp<CompMercenary>()).Where(x => x != null && x.IsPlayerControlledMercenary) ?? new List<CompMercenary>();
            }
        }

        void RetreatAll()
        {
            foreach (var mercanryComp in mercenaryComps)
            {
                mercanryComp.Retreat();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if(looterWarband != null) Widgets.Label(inRect.TopPart(0.1f), looterWarband.Label);

            var outRect = inRect.TopPart(.7f).BottomPart(0.8f);

            LootUIHelper.DrawLootList(
                outRect,
                ref scrollPosition,
                lootList,
                selectedItems,
                "WAW.SellLoot",
                (thing, isSelected) =>
                {
                    if (isSelected)
                    {
                        selectedItems.Add(thing);
                    }
                    else
                    {
                        selectedItems.Remove(thing);
                    }
                },
                true,
                elementHeight);

            Rect selectAllButtonRect = new Rect(inRect.x, inRect.y + inRect.height * 0.7f, inRect.width * 0.5f, inRect.height * 0.1f);
            LootUIHelper.DrawSelectAllButton(selectAllButtonRect, () =>
            {
                selectedItems.Clear();
                foreach (var item in lootList)
                {
                    selectedItems.Add(item);
                }
            });

            if (selectedItems.Count < 1)
            {
                return;
            }

            Rect confirmButtonRect = inRect.BottomPart(0.2f);
            if(LootUIHelper.DrawConfirmButton(confirmButtonRect, "WAW.GatherMapLoot", looterWarband.Label))
            {
                Log.Message("WAW: Loot window on confirm");
                looterWarband?.playerWarbandManager?.lootManager?.StoreAll(selectedItems.ToList());
                RetreatAll();
                Close();
            }
        }
    }
}

