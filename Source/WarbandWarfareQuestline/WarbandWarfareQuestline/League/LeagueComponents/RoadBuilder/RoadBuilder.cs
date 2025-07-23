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
    public class RoadBuilder : LeagueComponent
    {
        private int _startingTile;
        private int _destTile;
        private int _fromTileTemp;
        private int _toTileTemp;
        private RoadDef _roadDef;

        public RoadBuilder() : base()
        {
            _roadDef = DefDatabase<RoadDef>.AllDefs.First();
        }

        public int StartingTile => _startingTile;
        public int DestTile => _destTile;

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
            WorldPath worldPath = Find.WorldGrid.Surface.Pather.FindPath(_startingTile, _destTile, null);
            if (worldPath.Found)
            {
                worldPath.AddNodeAtStart(_startingTile);
            }
            return worldPath;
        }


        protected override void Execute()
        {
            int pointer = _startingTile;
            if (_startingTile <= 0 || _destTile <= 0 || _startingTile == _destTile)
            {
                Log.Error("RoadBuilder: Invalid starting and ending Tile Values");
                return;
            }

            base.Execute();
            var roadPath = GenerateNewPath();
            while (roadPath.NodesLeftCount > 1)
            {
                int nextTile = roadPath.ConsumeNextNode();
                SetTempRoadTiles(pointer, nextTile);
                BuildRoadBetweenTempNodes(_roadDef);
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
                Find.World.renderer.SetDirty<WorldDrawLayer_Roads>(Find.WorldGrid.Surface);
                Find.World.renderer.SetDirty<WorldDrawLayer_Paths>(Find.WorldGrid.Surface);
                Find.WorldPathGrid.RecalculateLayerPerceivedPathCosts(Find.WorldGrid.Surface);
            }
            catch (Exception e)
            {
                Log.Error($"WAW: error trying to redraw road: {e}");
            }
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

        public override void SendAvailabilityNotification()
        {
            base.SendAvailabilityNotification();
            Messages.Message("WAW.RoadConstruct.DaysAhead".Translate(this.RemainingDaysLabel), MessageTypeDefOf.RejectInput);

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref _roadDef, "_roadDef");
        }
    }
}
