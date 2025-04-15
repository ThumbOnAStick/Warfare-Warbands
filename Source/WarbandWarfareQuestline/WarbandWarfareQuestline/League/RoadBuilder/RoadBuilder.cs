using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.RoadBuilding
{
    public class RoadBuilder : IExposable
    {
        private int _startingTile;
        private int _destTile;
        private int _fromTileTemp;
        private int _toTileTemp;
        private int _lastUsageTick;
        private bool _unlocked;
        private const float _usageCooldownDays = 10;
        private RoadDef _roadDef;

        public RoadBuilder()
        {
            _lastUsageTick = -UsageCoolDownTicks;
            // Default road def
            _roadDef = DefDatabase<RoadDef>.AllDefs.First();
        }

        public int StartingTile => _startingTile;
        public int DestTile => _destTile;
        public int LastUsageTick => _lastUsageTick;
        public int UsageCoolDownTicks => (int)(_usageCooldownDays * GenDate.TicksPerDay);
        public int UsageRemainingCoolDownTicks => UsageCoolDownTicks - (GenTicks.TicksGame - _lastUsageTick);
        public float UsageRemainingCoolDownDays => UsageRemainingCoolDownTicks / (float)GenDate.TicksPerDay;

        public bool Unlocked => _unlocked;  

        public void SetStartAndDest(int start = 0, int dest = 0)
        {
            _startingTile = start != 0 ? start : _startingTile;
            _destTile = dest != 0 ? dest : _destTile;
        }

        private void SetTempRoadTiles(int from = 0, int to = 0)
        {
            _fromTileTemp = from != 0 ? from : _fromTileTemp;
            _toTileTemp = to != 0 ? to : _toTileTemp;
        }

        public void SetLastUsageTick(int tick)
        {
            _lastUsageTick = tick;
        }

        private WorldPath GenerateNewPath()
        {
            WorldPath worldPath = Find.WorldPathFinder.FindPath(_startingTile, _destTile, null);
            if (worldPath.Found)
            {
                worldPath.AddNodeAtStart(_startingTile);
            }
            return worldPath;
        }

        public bool IsBuilderReadyToBuild()
        {
            return UsageRemainingCoolDownTicks < 0;
        }

        public void BuildRoad()
        {
            int pointer = _startingTile;
            if (_startingTile <= 0 || _destTile <= 0 || _startingTile == _destTile)
            {
                Log.Error("RoadBuilder: Invalid starting and ending Tile Values");
                return;
            }

            var roadPath = GenerateNewPath();
            while (roadPath.NodesLeftCount > 1)
            {
                int nextTile = roadPath.ConsumeNextNode();
                SetTempRoadTiles(pointer, nextTile);
                BuildRoadBetweenTempNodes(_roadDef);
                pointer = nextTile;
            }
            // Reset usage
            SetLastUsageTick(GenTicks.TicksGame);
            Log.Message($"WAWRoadBuilder: Road starting from {_startingTile} to {_destTile} is built");
        }

        private void BuildRoadBetweenTempNodes(RoadDef road)
        {
            if (_fromTileTemp <= 0 || _toTileTemp <= 0)
            {
                Log.Error("WAWRoadBuilder: Invalid Tile Values");
                return;
            }

            Find.WorldGrid.OverlayRoad(_fromTileTemp, _toTileTemp, road);

            // Redraw the road
            try
            {
                Find.World.renderer.SetDirty<WorldLayer_Roads>();
                Find.World.renderer.SetDirty<WorldLayer_Paths>();
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(_fromTileTemp, null);
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(_toTileTemp, null);
            }
            catch (Exception e)
            {
                Log.Error($"WAW: error trying to redraw road: {e}");
            }
        }

        public void Unlock()
        {
            _unlocked = true;
        }

        public void Lock()
        {
            _unlocked = false;
        }   

        public bool CanRoadBeBuilt()
        {
            return _startingTile > 0 &&
                _destTile > 0 &&
                _startingTile != _destTile;
        }

        public bool ShouldResetNow()
        {
            return _startingTile > 0 || 
                _destTile > 0;
        }

        public void Reset()
        {
            _startingTile = 0;
            _destTile = 0;
            _fromTileTemp = 0;
            _toTileTemp = 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _unlocked, "_unlocked");
            Scribe_Values.Look(ref _lastUsageTick, "_lastUsageTick");
            Scribe_Defs.Look(ref _roadDef, "_roadDef");
        }
    }
}
