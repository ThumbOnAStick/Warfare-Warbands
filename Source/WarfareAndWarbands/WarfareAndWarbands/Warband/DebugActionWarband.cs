using AlienRace;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;

namespace WarfareAndWarbands.Warband
{
    public static class DebugActionWarband
    {
        [DebugAction("WAW", null, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnRandomMercenary()
        {
            Faction faction = FactionUtility.DefaultFactionFrom(WAWDefof.PlayerWarband);
            PawnKindDef localKindDef = WarbandUtil.SoldierPawnKinds().RandomElement();
            PawnGenerationRequest request = new PawnGenerationRequest(localKindDef, faction, PawnGenerationContext.NonPlayer);
            request.MustBeCapableOfViolence = true;
            Pawn mercenary = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(mercenary, Verse.UI.MouseCell(), Find.CurrentMap, WipeMode.Vanish);
            Lord lord = LordMaker.MakeNewLord(mercenary.Faction, new LordJob_DefendPoint(Verse.UI.MouseCell()), Find.CurrentMap, null);
            lord.AddPawn(mercenary);
        }

        [DebugAction("WAW", "Spawn Warband", actionType = DebugActionType.Action)]
        public static void SpawnForFaction()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnForFaction), true);
        }

        [DebugAction("WAW", "Spawn Warband For Player", actionType = DebugActionType.Action)]
        public static void SpawnForPlayer()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnForPlayer), true);
        }

        [DebugAction("WAW", "Spawn Warband Targeting", actionType = DebugActionType.Action)]
        public static void SpawnForFactionTargetingBase()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnForFactionTargetingBase), false);

        }

        [DebugAction("WAW", "Raid Player Warband", actionType = DebugActionType.Action)]
        public static void RaidPlayerWarband()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(RaidPlayerWarband), false);

        }

        [DebugAction("WAW", "Spawn Vehicle Caravan", actionType = DebugActionType.Action)]
        public static void GenerateVehicleCaravan()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnVehicleCaravan), true);

        }

        static bool SpawnVehicleCaravan(GlobalTargetInfo info)
        {
            Compatibility_Vehicle.Vehicle.DebugGenerateVehicleCaravan(info.Tile);
            return true;
        }

        static bool RaidPlayerWarband(GlobalTargetInfo info)
        {
            if (info.WorldObject == null || !info.WorldObject.IsPlayerWarband())
            {
                return false;
            }
            IEnumerable<FloatMenuOption> opts = RaidPlayerOptions(info.WorldObject as Warband);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));
            return true;
        }

        private static IEnumerable<FloatMenuOption> RaidPlayerOptions(Warband playerWarband)
        {
            HashSet<Faction> valids = WarfareUtil.GetValidWarFactions().Where(x => x.HostileTo(Faction.OfPlayer)).ToHashSet();
            foreach (Faction f in valids)
            {
                yield return new FloatMenuOption($"faction: {f.Name}", delegate { PlayerWarbandRaidUtil.RaidPlayer(f, playerWarband); });
            }
        }

        static bool SpawnForPlayer(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            Find.WindowStack.Add(new FloatMenu(SpawnForPlayerOptions(info).ToList()));
            return true;
        }

        static Warband GenerateRandomPlayerWarband(GlobalTargetInfo info)
        {
            return WarbandUtil.SpawnWarband(Find.FactionManager.AllFactions.First(x => !x.Hidden && !x.HostileTo(Faction.OfPlayer) && x != Faction.OfPlayer), info);

        }

        public static IEnumerable<FloatMenuOption> SpawnForPlayerOptions(GlobalTargetInfo info)
        {
            yield return new FloatMenuOption("default", delegate 
            {
                var playerWarband = GenerateRandomPlayerWarband(info);
                playerWarband.SetFaction(Faction.OfPlayer);
            });

            yield return new FloatMenuOption("outpost upgrade", delegate 
            {
                var playerWarband = GenerateRandomPlayerWarband(info);
                playerWarband.SetFaction(Faction.OfPlayer);
                playerWarband.playerWarbandManager.upgradeHolder.GainOutpostUpgrade();
            });


            yield return new FloatMenuOption("elite upgrade", delegate
            {
                var playerWarband = GenerateRandomPlayerWarband(info);
                playerWarband.SetFaction(Faction.OfPlayer);
                playerWarband.playerWarbandManager.upgradeHolder.GainEliteUpgrade();
            });

            yield return new FloatMenuOption("vehicle upgrade", delegate
            {
                var playerWarband = GenerateRandomPlayerWarband(info);
                playerWarband.SetFaction(Faction.OfPlayer);
                playerWarband.playerWarbandManager.upgradeHolder.GainVehilceUpgrade();
            });
        }

        static bool SpawnForFaction(GlobalTargetInfo info)
        {
            if(info.WorldObject != null)
            {
                return false;
            }
            IEnumerable<FloatMenuOption> opts = SpawnWarbandOptions(info);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));
            return true;
        }

        static bool SpawnForFactionTargetingBase(GlobalTargetInfo info)
        {
            if (info.WorldObject == null)
            {
                return false;
            }
            IEnumerable<FloatMenuOption> opts = SpawnWarbandTargetingBaseOptions(info);
            Find.WindowStack.Add(new FloatMenu(opts.ToList()));
            return true;
        }
        static IEnumerable<FloatMenuOption> SpawnWarbandOptions(GlobalTargetInfo info)
        {
            HashSet<Faction> valids = WarfareUtil.GetValidWarFactions();
            foreach (Faction f in valids)
            {
                yield return new FloatMenuOption($"faction: {f.Name}", delegate { WarbandUtil.SpawnWarband(f, info); });
            }
        }


        static IEnumerable<FloatMenuOption> SpawnWarbandTargetingBaseOptions(GlobalTargetInfo info)
        {
            HashSet<Faction> valids = WarfareUtil.GetValidWarFactions();
            foreach (Faction f in valids)
            {
                yield return new FloatMenuOption($"faction: {f.Name}", delegate { WarfareUtil.SpawnWarbandTargetingBase(f, info); });
            }
        }

    }
}
