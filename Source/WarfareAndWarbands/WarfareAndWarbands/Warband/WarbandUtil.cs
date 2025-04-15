using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Mercenary;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades;
using static System.Collections.Specialized.BitVector32;

namespace WarfareAndWarbands.Warband
{
    [StaticConstructorOnStartup]
    public static class WarbandUtil
    {
        static WarbandUtil()
        {
            AllPlayerWarbandsCache = new HashSet<Warband>();
            AllPlayerRecruitingWarbandsCache = new HashSet<WorldObject_WarbandRecruiting>();
            RefreshSoldierPawnKinds();
        }

        public static HashSet<Warband> AllPlayerWarbandsCache;
        public static HashSet<WorldObject_WarbandRecruiting> AllPlayerRecruitingWarbandsCache;


        public static void RefreshAllPlayerWarbands()
        {
            AllPlayerWarbandsCache = AllPlayerWarbands().ToHashSet();
            AllPlayerRecruitingWarbandsCache = AllPlayerRecruitingWarbands().ToHashSet();
        }

        public static void RefreshSoldierPawnKinds()
        {
            SoldierPawnKindsCache = GetSoldierPawnKinds();
        }

        public static string GetSoldierLabel(string defName)
        {
            if (!SoldierPawnKindsCache.Any(x => x.defName == defName))
            {
                return "";
            }
            return SoldierPawnKindsCache.First(x => x.defName == defName).label;
        }

        public static bool CantAffordToAttack(Warband warband)
        {
            int cost = (int)PlayerWarbandArrangement.GetCostOriginal(warband.bandMembers);
            bool cantAfford = !WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.AnyPlayerHomeMap, cost);
            if (cantAfford)
            {
                return false;
            }
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera(null);
            return true;
        }

        public static bool CanPlaceMoreWarbands()
        {
            return AllPlayerWarbandsCount() < WAWSettings.maxPlayerWarband;
        }

        public static IEnumerable<Warband> AllPlayerWarbands()
        {
            var predicate = new Func<WorldObject, bool>(x => (x as Warband != null)
            && x.Faction == Faction.OfPlayer);
            if (Find.WorldObjects.AllWorldObjects.Any(predicate))
            {
                HashSet<Warband> result = new HashSet<Warband>();
                var list = Find.WorldObjects.AllWorldObjects.Where(predicate);
                foreach(var ele in list)
                {
                    result.Add(ele as Warband);
                }
                return result;
            }
            return new HashSet<Warband>();
        }

        public static IEnumerable<WorldObject_WarbandRecruiting> AllPlayerRecruitingWarbands()
        {
            var predicate = new Func<WorldObject, bool>(x => (x as WorldObject_WarbandRecruiting != null)
            && x.Faction == Faction.OfPlayer);
            if (Find.WorldObjects.AllWorldObjects.Any(predicate))
            {
                HashSet<WorldObject_WarbandRecruiting> result = new HashSet<WorldObject_WarbandRecruiting>();
                var list = Find.WorldObjects.AllWorldObjects.Where(predicate);
                foreach (var ele in list)
                {
                    result.Add(ele as WorldObject_WarbandRecruiting);
                }
                return result;
            }
            return new HashSet<WorldObject_WarbandRecruiting>();
        }



        public static IEnumerable<Warband> AllActivePlayerWarbands()
        {
            if(AllPlayerWarbandsCache == null)
            {
                RefreshAllPlayerWarbands();
            }
            var predicate = new Func<Warband, bool>(x => x.playerWarbandManager.cooldownManager.CanFireRaid());
            if (AllPlayerWarbandsCache.Any(predicate))
                return AllPlayerWarbandsCache.Where(predicate);
            return new HashSet<Warband>();
        }

        public static int AllPlayerWarbandsCount()
        {
            return AllPlayerWarbandsCache.Count() + AllPlayerRecruitingWarbandsCache.Count();
        }

        public static List<PawnKindDef> GetSoldierPawnKinds()
        {
            bool IsNoble(PawnKindDef kind)
            {
                return kind.titleRequired != null && !kind.titleRequired.bedroomRequirements.NullOrEmpty();
            }
            var cache = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.isFighter && x.race.race.Humanlike && !IsNoble(x)).OrderBy(x => x.combatPower).ToList();
            List<PawnKindDef> result = new List<PawnKindDef>(cache);
            if (GameComponent_Customization.Instance != null)
            {
                var generated = GameComponent_Customization.Instance.GeneratedKindDefs;
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

        public static Warband SpawnWarband(Faction f, int tile, Color colorOverride)
        {
            var result = SpawnWarband(f, tile);
            result?.playerWarbandManager?.colorOverride?.SetColorOverride(colorOverride);   
            return result; 
        }
        public static Warband SpawnWarband(Faction f, int tile, Color colorOverride, Pawn leader)
        {
            var result = SpawnWarband(f, tile);
            result?.playerWarbandManager?.colorOverride?.SetColorOverride(colorOverride);
            result?.playerWarbandManager?.leader?.AssignLeader(leader);
            return result;
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
            if (warband.Faction == Faction.OfPlayer)
                TryToSendLeaderLetter();
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
            bool isVehicle =
                ModsConfig.IsActive("SmashPhil.VehicleFramework") &&
                warband.playerWarbandManager.upgradeHolder.HasUpgrade &&
                warband.playerWarbandManager.upgradeHolder.SelectedUpgrade is Upgrade_Vehicle;
            var upgrade = warband.playerWarbandManager.upgradeHolder.GetUpgrade<Upgrade_Vehicle>();
            Caravan caravan =
                isVehicle ? Compatibility_Vehicle.Vehicle.GenerateVehicleCaravan(tileId, list, upgrade.Vehicles) :
                CaravanMaker.MakeCaravan(list, Faction.OfPlayer, tileId, true);
            warband.playerWarbandManager.upgradeHolder.SelectedUpgrade?.OnArrived(list);
            return caravan;
        }

        public static void TryToSendLeaderLetter()
        {
            if (!GameComponent_WAW.Instance.EverAssignedLeader())
            {
                var label = "WAW.AboutAssignLeader".Translate();
                var desc = "WAW.AboutAssignLeader.Desc".Translate();
                Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.NeutralEvent);
                Find.LetterStack.ReceiveLetter(letter);
            }
        }


        public static void TryToSendQuickAttackLetter()
        {
            if (!GameComponent_WAW.Instance.EverUsedQuickRaid())
            {
                var label = "WAW.AboutQuickAttack".Translate();
                var desc = "WAW.AboutQuickAttack.Desc".Translate();
                Letter letter = LetterMaker.MakeLetter(label, desc, LetterDefOf.NeutralEvent);
                Find.LetterStack.ReceiveLetter(letter);
            }
        }

        public static void ReArrangePlayerWarband(Warband playerWarband)
        {
            if (!playerWarband.playerWarbandManager.cooldownManager.CanFireRaid())
            {
                string label = "WAW.CannotConfigureWarband".Translate(
                    playerWarband.playerWarbandManager.cooldownManager.GetRemainingDays().ToString("0.0"));
                Messages.Message(label, MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new Window_ReArrangeWarband(playerWarband));

        }

        public static bool TryToSpendSilverFromColonyOrBank(Map currentMap, int cost)
        {
            if(currentMap == null)
            {
                currentMap = Find.AnyPlayerHomeMap;
            }
            if (cost <= 0)
            {
                return true;
            }
            IEnumerable<Thing> silvers = from x in currentMap.listerThings.AllThings
                                         where x.def == ThingDefOf.Silver && x.IsInAnyStorage()
                                         select x;
            if(GameComponent_WAW.playerBankAccount.Balance >= cost)
            {
                GameComponent_WAW.playerBankAccount.Spend(cost);
                int remains = GameComponent_WAW.playerBankAccount.Balance;
                Messages.Message("WAW.SpentFromBank".Translate(cost, remains), MessageTypeDefOf.PositiveEvent);
                return true;
            }
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
        public static bool TryToSpendSilverFromBank(int cost)
        {

            if (cost <= 0)
            {
                return true;
            }

            if (GameComponent_WAW.playerBankAccount.Balance >= cost)
            {
                GameComponent_WAW.playerBankAccount.Spend(cost);
                int remains = GameComponent_WAW.playerBankAccount.Balance;
                Messages.Message("WAW.SpentFromBank".Translate(cost, remains), MessageTypeDefOf.PositiveEvent);
                return true;
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
            if(o.Faction == Faction.OfPlayer)
            {
                return true;
            }
            return o.Faction != null && !o.Faction.HostileTo(Faction.OfPlayer);
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
            if (Find.CurrentMap.listerThings.AllThings.Any(x => x.def == WAWDefof.WAW_LootChest ))
            {
                return;
            }
            Thing chest = ThingMaker.MakeThing(WAWDefof.WAW_LootChest);
            chest.SetFaction(Faction.OfPlayer);
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(WAWDefof.ActiveDropPodLootChest, null);
            activeDropPod.Contents = new ActiveDropPodInfo();
            activeDropPod.Contents.GetDirectlyHeldThings().TryAddOrTransfer(chest);
            chest.TryGetComp<CompLootChest>()?.AssignWarband(warbandCache);
            SkyfallerMaker.SpawnSkyfaller(WAWDefof.LootChestIncoming, activeDropPod, info.Cell, Find.CurrentMap);
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
