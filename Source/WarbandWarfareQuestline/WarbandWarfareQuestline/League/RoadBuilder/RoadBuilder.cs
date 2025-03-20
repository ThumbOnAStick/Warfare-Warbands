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

        public RoadBuilder() { }

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
            if (_startingTile <= 0 || _destTile <= 0)
            {
                Log.Error("RoadBuilder: Invalid starting and ending Tile Values");
                return;
            }

            var roadPath = GenerateNewPath();
            var road = DefDatabase<RoadDef>.GetRandom();
            while (roadPath.NodesLeftCount > 1)
            {
                int nextTile = roadPath.ConsumeNextNode();
                SetTempRoadTiles(_startingTile, nextTile);
                BuildRoadBetweenTempNodes(road);
                _startingTile = nextTile;
            }
            Log.Message($"Road starting from {_startingTile} to {_destTile} is built");
        }

        private void BuildRoadBetweenTempNodes(RoadDef road)
        {
            if (_fromTileTemp <= 0 || _toTileTemp <= 0)
            {
                Log.Error("RoadBuilder: Invalid Tile Values");
                return;
            }

            Find.WorldGrid.OverlayRoad(_fromTileTemp, _toTileTemp, road);
            Log.Message($"Road Built from {_fromTileTemp} to {_toTileTemp}");

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

        public void ExposeData()
        {
             
        }
    }
}
