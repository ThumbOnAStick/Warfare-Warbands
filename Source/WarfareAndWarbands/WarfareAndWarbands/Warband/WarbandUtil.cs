using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace WarfareAndWarbands.Warband
{
    [StaticConstructorOnStartup]
    public static class WarbandUtil
    {
        static WarbandUtil()
        {
            SoldierPawnKindsCache = SoldierPawnKinds();
        }
        public static List<PawnKindDef> SoldierPawnKinds()
        {
            return DefDatabase<PawnKindDef>.AllDefs.Where(x => x.isFighter && x.race.race.Humanlike).OrderBy(x => x.combatPower).ToList();
        }

        public static PawnKindDef TargetPawnKindDef(string name)
        {
            return SoldierPawnKindsCache.First(x => x.defName == name);
        }

        public static void SpawnWarband(Faction f, GlobalTargetInfo info)
        {
            List<SitePartDef> sitePartList = new List<SitePartDef>
            {
                DefDatabase<SitePartDef>.GetNamed("Outpost")
            };
            List<SitePartDefWithParams> sitePartDefsWithParams;
            SiteMakerHelper.GenerateDefaultParams(0f, info.Tile, f, sitePartList, out sitePartDefsWithParams);
            var warband = (Warband)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_Warband);
            warband.Tile = info.Tile;
            warband.SetFaction(f);
            if (sitePartDefsWithParams != null)
            {
                foreach (SitePartDefWithParams sitePart in sitePartDefsWithParams)
                {
                    warband.AddPart(new SitePart(warband, sitePart.def, sitePart.parms));
                }
            }
            Find.WorldObjects.Add(warband);
        }

        public static void SpawnWarbandTargetingBase(Faction f, GlobalTargetInfo info)
        {
            List<SitePartDef> sitePartList = new List<SitePartDef>
            {
                DefDatabase<SitePartDef>.GetNamed("Outpost")
            };
            List<SitePartDefWithParams> sitePartDefsWithParams;
            SiteMakerHelper.GenerateDefaultParams(0f, info.Tile, f, sitePartList, out sitePartDefsWithParams);
            var warband = (Warband)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_Warband);
            TileFinder.TryFindNewSiteTile(out int warbandTile, 7, 27, false, TileFinderMode.Near, info.Tile);
            warband.Tile = warbandTile;
            warband.SetFaction(f);
            warband.targetTile = info.Tile;
            if (sitePartDefsWithParams != null)
            {
                foreach (SitePartDefWithParams sitePart in sitePartDefsWithParams)
                {
                    warband.AddPart(new SitePart(warband, sitePart.def, sitePart.parms));
                }
            }
            Find.WorldObjects.Add(warband);
        }

        public static bool IsPlayerWarband(this WorldObject o)
        {
            return o.Faction == Faction.OfPlayer && o as Warband != null;

        }

        public static void OrderPlayerWarbandToAttack(MapParent mapP, Warband warband)
        {

            Caravan caravan = SpawnCaravan(mapP.Tile, warband);
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapP.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

        }

        private static Caravan SpawnCaravan(int tileId, Warband warband)
        {
            List<Pawn> list = new List<Pawn>();
            foreach (var ele in warband.bandMembers)
            {
                for (int i = 0; i < ele.Value; i++)
                {
                    PawnKindDef kindDef = PawnKindDefOf.Pirate;
                    if (SoldierPawnKindsCache.Any(x => x.defName == ele.Key))
                    {
                        kindDef = SoldierPawnKindsCache.First(x => x.defName == ele.Key);
                    }
                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, Faction.OfPlayer);
                    CompMercenary comp = pawn.TryGetComp<CompMercenary>();
                    if (comp == null)
                    {
                        continue;
                    }
                    comp.ServesPlayerFaction = true;
                    Log.Message("WAW: Comp set successful");
                    list.Add(pawn);
                }

            }
            Caravan caravan = CaravanMaker.MakeCaravan(list, Faction.OfPlayer, tileId, true);
            return caravan;
        }



        public static Settlement AddNewHome(int tile, Faction faction, WorldObjectDef targetDef)
        {
            Settlement settlement;
            settlement = (Settlement)WorldObjectMaker.MakeWorldObject(targetDef);
            settlement.Tile = tile;
            settlement.SetFaction(faction);
            settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
            Find.WorldObjects.Add(settlement);
            return settlement;
        }



        public static List<PawnKindDef> SoldierPawnKindsCache;
    }


}
