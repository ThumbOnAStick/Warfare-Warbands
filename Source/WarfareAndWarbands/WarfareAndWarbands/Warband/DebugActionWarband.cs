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

        [DebugAction("WAW", "Spawn Warband Targeting", actionType = DebugActionType.Action)]
        public static void SpawnForFactionTargetingBase()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(SpawnForFactionTargetingBase), false);

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
