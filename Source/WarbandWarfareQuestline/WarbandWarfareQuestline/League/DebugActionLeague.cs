using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;
using WarfareAndWarbands.Warband;
using WarfareAndWarbands;
using RimWorld.Planet;
using WarbandWarfareQuestline.Skirmish;
using WarbandWarfareQuestline.League.MinorFactions;
using System.Threading;

namespace WarbandWarfareQuestline.League
{
    public static class DebugActionLeague
    {
        [DebugAction("WAW", "Random faction join player league", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnRandomMercenary() =>
            MinorFactionHelper.GenerateRandomMinorFactionAndJoinPlayer().JoinPlayer();

        [DebugAction("WAW", "Create Random Skirmish", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void CreateRandomSkirmish() =>
            GameComponent_Skrimish.Instance.CreateRandomSkirmsish();

        [DebugAction("WAW", "Initiate Skirmish", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void InitiateSkirmish() =>
            Find.WorldTargeter.BeginTargeting(SpawnSkirmish, true);

        [DebugAction("WAW", "Spawn Siege", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnSiege() =>
            SpawnSiegeEvent();

        [DebugAction("WAW", "Spawn Settlement Construction", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void SpawnSettlementConstruction()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(SpawnTownConstruction, true);
        }

        [DebugAction("WAW", "Build Road1", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void DecideRoadStart()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(DecideRoadStartingTile, true);
        }
        [DebugAction("WAW", "Build Road2", false, false, false, false, 0, false, actionType = DebugActionType.Action)]
        public static void DecideRoadEnd()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(Find.AnyPlayerHomeMap.Parent), CameraJumper.MovementMode.Pan);
            Find.WorldTargeter.BeginTargeting(DecideRoadEndingTile, true);
        }

        static bool SpawnSkirmish(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            var skirmish = SkirmishHelper.CreateSkirmish(info);
            GameComponent_Skrimish.Instance.Register(skirmish);
            return true;
        }

        static void SpawnSiegeEvent()
        {
            var skirmish = SkirmishHelper.CreateSiege();
            GameComponent_Skrimish.Instance.Register(skirmish);
        }

        static bool SpawnTownConstruction(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            GenerateTownConstructionAround(info.Tile);
            return true;
        }

        static bool DecideRoadStartingTile(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            GameComponent_League.Instance.RoadBuilder.SetStartAndDest(start : info.Tile);
            Find.WorldTargeter.StopTargeting();
            return true;
        }

        static bool DecideRoadEndingTile(GlobalTargetInfo info)
        {
            if (info.WorldObject != null)
            {
                return false;
            }
            GameComponent_League.Instance.RoadBuilder.SetStartAndDest(dest : info.Tile);
            GameComponent_League.Instance.RoadBuilder.BuildRoad();
            return true;
        }


        static WorldObject GenerateTownConstructionAround(int tile)
        {
            TileFinder.TryFindPassableTileWithTraversalDistance(tile, 5, 10, out int randomTile);
            return GenerateTownConstruction(randomTile);
        }

        static WorldObject GenerateTownConstruction(int tile)
        {
            var result = WorldObjectMaker.MakeWorldObject(WAWDefof.WAW_SettlementConstruction);
            result.Tile = tile;
            Find.WorldObjects.Add(result);
            return result;
        }
    }
}
