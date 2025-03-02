using RimWorld.BaseGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League
{
    public class GenStep_MinorSettlement: GenStep_Scatterer
    {
        public override int SeedPart
        {
            get
            {
                return base.GetType().Name.GetHashCode();
            }
        }

        private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);


        private static List<IntVec3> tmpCandidates = new List<IntVec3>();

        // Token: 0x06006923 RID: 26915 RVA: 0x00236518 File Offset: 0x00234718
        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.Roofed(map))
            {
                return false;
            }
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false, false, false)))
            {
                return false;
            }
            int min = GenStep_MinorSettlement.SettlementSizeRange.min;
            CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
            return cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z));
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = SettlementSizeRange.RandomInRange;
            int randomInRange2 =  SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            Faction faction;
            var tribe = Find.FactionManager.AllFactions.First(x => x.HostileTo(Faction.OfPlayer) && x.def.techLevel < TechLevel.Industrial);
            if (tribe != null)
            {
                faction = tribe;
            }
            else
            {   
                faction = map.ParentFaction;
            }
            rect.ClipInsideMap(map);
            ResolveParams resolveParams = default(ResolveParams);
            resolveParams.rect = rect;
            resolveParams.faction = faction;
            resolveParams.settlementDontGeneratePawns = false;
            resolveParams.addRoomCenterToRootsToUnfog = true;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("settlement", resolveParams, null);
   
            BaseGen.Generate();

        }


    }
}
