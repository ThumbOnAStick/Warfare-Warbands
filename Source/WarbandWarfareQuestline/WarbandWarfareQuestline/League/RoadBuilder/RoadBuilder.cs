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
        private bool _unlocked;

        public RoadBuilder() { }

        public int StartingTile => _startingTile;
        public int DestTile => _destTile;
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

        private WorldPath GenerateNewPath()
        {
            WorldPath worldPath = Find.WorldPathFinder.FindPath(_startingTile, _destTile, null);
            if (worldPath.Found)
            {
                worldPath.AddNodeAtStart(_startingTile);
            }
            return worldPath;
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
            var road = DefDatabase<RoadDef>.GetRandom();
            while (roadPath.NodesLeftCount > 1)
            {
                int nextTile = roadPath.ConsumeNextNode();
                SetTempRoadTiles(pointer, nextTile);
                BuildRoadBetweenTempNodes(road);
                pointer = nextTile;
            }
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

        public bool IsRoadReadyToBuild()
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
        }
    }
}
