using RimWorld.Planet;
using RimWorld;

using Verse;
using Verse.Sound;
using System;
using WarfareAndWarbands.Warband.WarbandComponents.WarbandUpdates;
using AlienRace;
using WarfareAndWarbands.Warband.UI;
using Verse.Noise;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public static class PlayerWarbandResettleManager
    {

        public static bool OrderPlayerWarbandToResettle(GlobalTargetInfo info, Warband warband)
        {
            if (info.WorldObject != null)
            {
                Messages.Message("WAW.InvalidObject".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            if (Find.World.Impassable(info.Tile))
            {
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
                return false;
            }
            if (warband.playerWarbandManager.upgradeHolder.CanDroppod)
                Find.WindowStack.Add(new FloatMenu(TravelOpts(info, warband).ToList()));
            else
                MoveTo(info, warband);
            return true;
        }

        public static IEnumerable<FloatMenuOption> TravelOpts(GlobalTargetInfo info, Warband warband)
        {
            yield return new FloatMenuOption("WAW.OnFoot".Translate() + $"({GetTravelCost(info, warband)})", delegate { MoveTo(info, warband); });
            yield return new FloatMenuOption("WAW.UsePod".Translate() + $"({GetFlightCost(info, warband)})", delegate { FlyTo(info, warband); });
        }

        private static bool MoveTo(GlobalTargetInfo info, Warband warband)
        {
            if (!WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.AnyPlayerHomeMap, GetTravelCost(info, warband)))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            Messages.Message("WAW.IsMovingTo".Translate(), MessageTypeDefOf.PositiveEvent);
            warband.worldPather.StartPath(info.Tile);
            return true;
        }

        public static int GetTravelCost(GlobalTargetInfo info, Warband warband)
        {
            float distance = Math.Max(Find.WorldGrid.ApproxDistanceInTiles(info.Tile, warband.Tile), 1f);
            var curve = WarbandUtil.ResettleCurve();
            int memberCount = warband.GetMemberCount();
            int costPerPawn = (int)curve.Evaluate(memberCount);
            int cost = (int)distance * costPerPawn * memberCount;
            return cost / 2;
        }

        public static int GetFlightCost(GlobalTargetInfo info, Warband warband)
        {
            float distance = Math.Max(Find.WorldGrid.ApproxDistanceInTiles(info.Tile, warband.Tile), 1f);
            var curve = WarbandUtil.ResettleCurve();
            int memberCount = warband.GetMemberCount();
            int costPerPawn = (int)curve.Evaluate(memberCount);
            int cost = (int)distance * costPerPawn * memberCount;
            return cost;
        }

        private static bool FlyTo(GlobalTargetInfo info, Warband warband)
        {

            int cost = GetFlightCost(info, warband);
            if (!WarbandUtil.TryToSpendSilverFromColonyOrBank(Find.AnyPlayerHomeMap, cost))
            {
                Messages.Message("WAW.CantAfford".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            warband.worldPather.ResetPath();
            TransportPodsArrivalAction_SpawnWarband action = new TransportPodsArrivalAction_SpawnWarband(warband);
            TravellingTransporters travelingTransportPods = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.TravellingTransporters);
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
