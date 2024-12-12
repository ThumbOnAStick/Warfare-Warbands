using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband
{
    internal static class CaravanWarbandUtility
    {

        public static bool CanAfford(this Caravan caravan, int cost)
        {
            int amount = 0;
            foreach (var t in caravan.Goods)
            {
                if(t.def != ThingDefOf.Silver)
                {
                    continue;
                }
                amount += t.stackCount;
            }

            return amount >= cost;
        }

        public static bool TryToSpendSilverFromCaravan(Caravan caravan, int cost)
        {
            if (!caravan.CanAfford(cost))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            var allSilvers = caravan.AllThings.Where(x => x.def == ThingDefOf.Silver);
            int debt = cost;
            int thingCount = allSilvers.Count();
            for (int i = 0; i < thingCount && debt > 0; i++)
            {
                Thing silver = allSilvers.ElementAt(i);

                if (silver == null)
                {
                    continue;
                }

                if (debt >= silver.stackCount)
                {
                    debt -= silver.stackCount;
                    silver.Destroy();
                }
                else
                {
                    silver = silver.SplitOff(debt);
                    debt = 0;
                }
            }
            return true;
        }

        public static bool CannotCreateWarband(Caravan caravan)
        {
            if (!WarbandUtil.CanPlaceMoreWarbands())
            {
                return true;
            }
            var site = Find.WorldObjects.SiteAt(caravan.Tile);
            if (site as Warband != null)
            {
                return true;
            }
            return false;
        }


    }
}
