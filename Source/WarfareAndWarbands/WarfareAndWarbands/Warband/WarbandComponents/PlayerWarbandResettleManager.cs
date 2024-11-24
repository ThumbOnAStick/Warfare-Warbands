using RimWorld.Planet;
using RimWorld;

using Verse;
using Verse.Sound;


namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandResettleManager
    {
        public PlayerWarbandResettleManager() 
        {

        }


        public bool OrderPlayerWarbandToResettle(GlobalTargetInfo info, Warband warband)
        {
            if (info.WorldObject != null)
            {
                Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }

            float distance = Find.WorldGrid.ApproxDistanceInTiles(info.Tile, warband.Tile);
            var curve = WarbandUtil.ResettleCurve();
            int memberCount = warband.GetMemberCount();
            int costPerPawn = (int)curve.Evaluate(memberCount);
            int cost = (int)distance * costPerPawn * memberCount;
            if (!WarbandUtil.TryToSpendSilver(Find.AnyPlayerHomeMap, cost))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            TransportPodsArrivalAction_SpawnWarband action = new TransportPodsArrivalAction_SpawnWarband(warband);
            TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravelingTransportPods);
            travelingTransportPods.Tile = warband.Tile;
            travelingTransportPods.SetFaction(Faction.OfPlayer);
            travelingTransportPods.destinationTile = info.Tile;
            travelingTransportPods.arrivalAction = action;
            Find.WorldObjects.Add(travelingTransportPods);
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            return true;
        }
    }
}
