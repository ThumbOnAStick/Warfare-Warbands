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
        private int fromTileTemp;
        private int toTileTemp;


        public RoadBuilder()
        {

        }

        public void SetRoadTiles(int from = 0, int to = 0)
        {
            if(from != 0)
            {
                fromTileTemp = from;
            }

            if(to != 0)
            {
                toTileTemp = to;
            }
        }

        public void BuildRoad()
        {
            if(fromTileTemp <= 0 || toTileTemp <= 0)
            {
                Log.Error("RoadBuilder: Invalid Tile Values");
                return;
            }
            Find.WorldGrid.OverlayRoad(fromTileTemp, toTileTemp, DefDatabase<RoadDef>.GetRandom());
            Log.Message($"Road Built from {fromTileTemp} to {toTileTemp}");
            try
            {
                Find.World.renderer.SetDirty<WorldLayer_Roads>();
                Find.World.renderer.SetDirty<WorldLayer_Paths>();
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(fromTileTemp, null);
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(toTileTemp, null);
            }
            catch (Exception e)
            {
                Log.Error($"WAW: error trying to redraw road: {e}");
            }
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }
    }
}
