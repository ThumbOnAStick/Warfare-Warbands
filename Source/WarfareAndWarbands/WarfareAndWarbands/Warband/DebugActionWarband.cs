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
using WarfareAndWarbands.CharacterCustomization;
using WarfareAndWarbands.Warband.Compatibility_VPE;
using WarfareAndWarbands.Warband.PlayerWarbandRaid;
using WarfareAndWarbands.Warband.UI;
using WarfareAndWarbands.Warband.VassalWarband;
using WarfareAndWarbands.Warband.VassalWarband.UI;
using WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandUpgrades.Psycaster;

namespace WarfareAndWarbands.Warband
{
    public static class DebugActionWarband
    {
        [DebugAction("WAW", null, false, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnRandomMercenary()
        {
            Faction faction = FactionUtility.DefaultFactionFrom(WAWDefof.PlayerWarband);
            PawnKindDef localKindDef = WarbandUtil.GetSoldierPawnKinds().RandomElement();
            PawnGenerationRequest request = new PawnGenerationRequest(localKindDef, faction, PawnGenerationContext.NonPlayer);
            request.MustBeCapableOfViolence = true;
            Pawn mercenary = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(mercenary, Verse.UI.MouseCell(), Find.CurrentMap, WipeMode.Vanish);
            Lord lord = LordMaker.MakeNewLord(mercenary.Faction, new LordJob_DefendPoint(Verse.UI.MouseCell()), Find.CurrentMap, null);
            lord.AddPawn(mercenary);
        }

        [DebugAction("WAW", "Spawn imperial skipmaster", requiresRoyalty: true,  false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DoDropHeavyItemsJob()
        {
            if (GameComponent_Customization.Instance.CustomizationRequests.Count < 1)
            {
                return;
            }
            PsycasterInfo info = new PsycasterInfo(GameComponent_Customization.Instance.CustomizationRequests.First(), new List<string> { "VPE_Skipmaster" });
            var caster = info.CreatePsycaster();
            GenSpawn.Spawn(caster, Verse.UI.MouseCell(), Find.CurrentMap, WipeMode.Vanish);
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

        [DebugAction("WAW", "Spawn Vassal Warband", actionType = DebugActionType.Action)]
        public static void SpawnVassalForPlayer()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnVassalForPlayer), true);
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

        [DebugAction("WAW", "Spawn Mortar Shell", actionType = DebugActionType.ToolMap)]
        public static void SpawnShellOn()
        {
            if(GameComponent_WAW.Instance.MortarShells.Count < 1)
            {
                return;
            }
            LocalTargetInfo info = Verse.UI.MouseCell();
            ThingDef shellDef = GameComponent_WAW.Instance.MortarShells.RandomElement().projectileWhenLoaded;
            if (shellDef == null)
            {
                return;
            }
            Projectile shell = (Projectile)GenSpawn.Spawn(shellDef, new IntVec3(0, 0, 0), map: Find.CurrentMap);
            int maxExclusive = GenRadial.NumCellsInRadius(5);
            int num = Rand.Range(0, maxExclusive);
            var rndCell = info.Cell + GenRadial.RadialPattern[num];
            shell.Launch(launcher: null, usedTarget: rndCell, intendedTarget: info, ProjectileHitFlags.All);
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

        static bool SpawnVassalForPlayer(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            Find.WindowStack.Add(new Window_CreateVassalWarband(info, new VassalHolder(1.5f, 10000)));
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

            yield return new FloatMenuOption("psycaster upgrade", delegate
            {
                var playerWarband = GenerateRandomPlayerWarband(info);
                playerWarband.SetFaction(Faction.OfPlayer);
                playerWarband.playerWarbandManager.upgradeHolder.GainPsycasterUpgrade();
            });
            yield return new FloatMenuOption("engineer upgrade", delegate
            {
                var playerWarband = GenerateRandomPlayerWarband(info);
                playerWarband.SetFaction(Faction.OfPlayer);
                playerWarband.playerWarbandManager.upgradeHolder.GainEngineerUpgrade();
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
