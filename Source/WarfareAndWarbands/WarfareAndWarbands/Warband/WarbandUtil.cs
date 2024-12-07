using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Mercenary;
using WarfareAndWarbands.Warband.UI;

namespace WarfareAndWarbands.Warband
{
    [StaticConstructorOnStartup]
    public static class WarbandUtil
    {
        static WarbandUtil()
        {
            Refresh();

        }

        public static void Refresh()
        {
            SoldierPawnKindsCache = SoldierPawnKinds();
        }

        public static string GetSoldierLabel(string defName)
        {
            if (!SoldierPawnKindsCache.Any(x => x.defName == defName))
            {
                return "";
            }
            return SoldierPawnKindsCache.First(x => x.defName == defName).label;
        }

        public static bool CanPlaceMoreWarbands()
        {
            return AllPlayerWarbandsCount() < WAWSettings.maxPlayerWarband;
        }

        public static int AllPlayerWarbandsCount()
        {
            return Find.WorldObjects.AllWorldObjects.Count(x => (x as Warband != null || x as WorldObject_WarbandRecruiting != null)
            && x.Faction == Faction.OfPlayer);
        }

        public static List<PawnKindDef> SoldierPawnKinds()
        {
            var cache = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.isFighter && x.race.race.Humanlike).OrderBy(x => x.combatPower).ToList();
            List<PawnKindDef> result = new List<PawnKindDef>(cache);
            if (GameComponent_Customization.Instance != null)
            {
                var generated = GameComponent_Customization.Instance.generatedKindDefs;
                foreach (var ele in generated)
                {
                    if (!result.Any(x => x.defName == ele.defName))
                    {
                        result.Add(generated.First(x => x.defName == ele.defName));
                    }
                }
            }
            return result;
        }
        public static List<PawnKindDef> SoldierPawnKindsWithTechLevel(TechLevel level)
        {
            return SoldierPawnKindsCache.Where(x => InTechLevel(x, level)).ToList();
        }

        public static bool InTechLevel(PawnKindDef def, TechLevel level)
        {
            return def.defaultFactionType != null && def.defaultFactionType.techLevel == level;
        }

        public static bool HasPawnKind(string name)
        {
            return SoldierPawnKindsCache.Any(x => x.defName == name);
        }

        public static PawnKindDef TargetPawnKindDef(string name)
        {
            return SoldierPawnKindsCache.First(x => x.defName == name);
        }

        public static Warband SpawnWarband(Faction f, GlobalTargetInfo info)
        {
            var warband = SpawnWarband(f, info.Tile);
            return warband;
        }

        public static Warband SpawnWarband(Faction f, int tile)
        {
            List<SitePartDef> sitePartList;
            if (!f.IsPlayer)
            {
                sitePartList = new List<SitePartDef>
            {
                DefDatabase<SitePartDef>.GetNamed("Outpost"),
            };
            }
            else
            {
                sitePartList = new List<SitePartDef>
            {
                WAWDefof.WAWEmptySite,

            };
            }

            if (Find.World.Impassable(tile))
            {
                return null;
            }

            List<SitePartDefWithParams> sitePartDefsWithParams;
            SiteMakerHelper.GenerateDefaultParams(0f, tile, f, sitePartList, out sitePartDefsWithParams);
            var warband = (Warband)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_Warband);
            warband.Tile = tile;
            warband.SetFaction(f);
            if (sitePartDefsWithParams != null)
            {
                foreach (SitePartDefWithParams sitePart in sitePartDefsWithParams)
                {
                    warband.AddPart(new SitePart(warband, sitePart.def, sitePart.parms));
                }
            }
            Find.WorldObjects.Add(warband);
            return warband;
        }

        public static Warband SpawnWarband(Faction f, int tile, Dictionary<string, int> members)
        {
            List<SitePartDef> sitePartList;
            if (!f.IsPlayer)
            {
                sitePartList = new List<SitePartDef>
            {
                DefDatabase<SitePartDef>.GetNamed("Outpost"),
            };
            }
            else
            {
                sitePartList = new List<SitePartDef>
            {
                WAWDefof.WAWEmptySite,

            };
            }

            List<SitePartDefWithParams> sitePartDefsWithParams;
            SiteMakerHelper.GenerateDefaultParams(0f, tile, f, sitePartList, out sitePartDefsWithParams);
            var warband = (Warband)WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_Warband);
            warband.Tile = tile;
            warband.SetFaction(f);
            warband.SetMembers(members);
            if (sitePartDefsWithParams != null)
            {
                foreach (SitePartDefWithParams sitePart in sitePartDefsWithParams)
                {
                    warband.AddPart(new SitePart(warband, sitePart.def, sitePart.parms));
                }
            }
            Find.WorldObjects.Add(warband);
            return warband;
        }


        public static bool IsPlayerWarband(this WorldObject o)
        {
            return o.Faction == Faction.OfPlayer && o as Warband != null;

        }

        public static void OrderPlayerWarbandToAttack(MapParent mapP, Warband warband)
        {

            Caravan caravan = SpawnWarbandCaravan(mapP.Tile, warband);
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapP.Tile, null);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);

        }


        private static Caravan SpawnWarbandCaravan(int tileId, Warband warband)
        {
            List<Pawn> list = MercenaryUtil.GenerateWarbandPawns(warband); 
            Caravan caravan = CaravanMaker.MakeCaravan(list, Faction.OfPlayer, tileId, true);
            return caravan;
        }

        public static void ReArrangePlayerWarband(Warband playerWarband)
        {
            Find.WindowStack.Add(new Window_ReArrangeWarband(playerWarband));

        }

        public static bool TryToSpendSilver(Map currentMap, int cost)
        {
            if (cost <= 0)
            {
                return true;
            }
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

        public static bool IsWorldObjectNonHostile(WorldObject o)
        {
            var mp = (MapParent)o;
            if (mp.HasMap)
                return false;
            return o.Faction != null && o.Faction != Faction.OfPlayer && !o.Faction.Hidden && !o.Faction.HostileTo(Faction.OfPlayer);
        }

        public static SimpleCurve ResettleCurve()
        {
            CurvePoint p1 = new CurvePoint(1, 10);
            CurvePoint p2 = new CurvePoint(10, 5);
            CurvePoint p3 = new CurvePoint(100, 1);
            List<CurvePoint> points = new List<CurvePoint>() { p1, p2, p3 };
            return new SimpleCurve(points);
        }

        public static void TryToSpawnLootChest(Warband warband)
        {
            SetWarband(warband);
            Targeter targeter = Find.Targeter;
            TargetingParameters targetParams = TargetingParameters.ForDropPodsDestination();
            SoundDefOf.Designate_PlaceBuilding.PlayOneShotOnCamera();
            targeter.BeginTargeting(targetParams: targetParams, action: SpawnLootChest, onGuiAction: GUIAction);
        }

        static void SpawnLootChest(LocalTargetInfo info)
        {
            if (Find.CurrentMap.listerThings.AllThings.Any(x => x.def == WAWDefof.WAW_LootChest))
            {
                return;
            }
            Thing chest = GenSpawn.Spawn(WAWDefof.WAW_LootChest, info.Cell, Find.CurrentMap);
            chest.TryGetComp<CompLootChest>()?.AssignWarband(warbandCache);
        }

        static void GUIAction(LocalTargetInfo info)
        {

        }

        public static void SetWarband(Warband warband)
        {
            warbandCache = warband;
        }

        public static List<PawnKindDef> SoldierPawnKindsCache;

        static Warband warbandCache;
    }


}
