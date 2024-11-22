using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using static System.Collections.Specialized.BitVector32;
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
        public static List<PawnKindDef> SoldierPawnKindsWithTechLevel(TechLevel level)
        {
            return SoldierPawnKindsCache.Where(x => InTechLevel(x, level)).ToList();
        }

        public static bool InTechLevel(PawnKindDef def, TechLevel level)
        {
            return def.defaultFactionType != null && def.defaultFactionType.techLevel == level;
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
            TileFinder.TryFindNewSiteTile(out int warbandTile, 3, 7, false, TileFinderMode.Near, info.Tile);
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
                    PawnGenerationRequest request = new PawnGenerationRequest(kindDef, Faction.OfPlayer, mustBeCapableOfViolence: true);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    pawn.apparel.LockAll();
                    var equipments = pawn.equipment.AllEquipmentListForReading;
                    foreach (var equipment in equipments)
                    {
                        if (equipment.def.IsWeapon && equipment.TryGetComp<CompBiocodable>() != null)
                        {
                            equipment.TryGetComp<CompBiocodable>().CodeFor(pawn);
                        }
                    }
                    CompMercenary comp = pawn.TryGetComp<CompMercenary>();
                    if (comp == null)
                    {
                        continue;
                    }
                    comp.ServesPlayerFaction = true;
                    list.Add(pawn);
                }

            }
            Caravan caravan = CaravanMaker.MakeCaravan(list, Faction.OfPlayer, tileId, true);
            return caravan;
        }

        public static bool TryToSpendSilver(Map currentMap, int cost)
        {
            IEnumerable<Thing> silvers = from x in currentMap.listerThings.AllThings
                                         where x.def == ThingDefOf.Silver && x.IsInAnyStorage()
                                         select x;
            if (silvers.Sum((Thing t) => t.stackCount) < cost)
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            int debt = cost;
            for (int i = 0; i < silvers.Count() && debt > 0; i++)
            {
                Thing silver = null;
                foreach (Thing item2 in currentMap.thingGrid.ThingsAt(silvers.ElementAt(i).Position))
                {
                    if (item2.def == ThingDefOf.Silver)
                    {
                        silver = item2;
                        break;
                    }
                }

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

        public static WorldObject RandomHostileSettlement(Faction myFaction)
        {
            if (Find.WorldObjects.MapParents.Any(x => x.Faction != null && x.Faction.HostileTo(myFaction)))
                return Find.WorldObjects.MapParents.Where(x => x.Faction != null && x.Faction != Faction.OfPlayer && x.Faction.HostileTo(myFaction)).RandomElement();
            return null;
        }

        public static bool IsWorldObjectNonHostile(WorldObject o)
        {
            return o.Faction != null && o.Faction != Faction.OfPlayer && !o.Faction.HostileTo(Faction.OfPlayer);
        }

        public static List<PawnKindDef> SoldierPawnKindsCache;
    }


}
