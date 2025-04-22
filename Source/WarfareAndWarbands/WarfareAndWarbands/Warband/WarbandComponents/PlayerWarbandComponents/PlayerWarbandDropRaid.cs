using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using WarfareAndWarbands.Warband.Mercenary;

namespace WarfareAndWarbands.Warband.WarbandComponents.WarbandUpdates
{
    public class PlayerWarbandDropRaid : IExposable
    {
        public bool Activated => WAWSettings.dropPodRaidRequiresUpgrade || activated;
        private bool activated;
        private Warband warband;
        private Map mapCached;

        public PlayerWarbandDropRaid(Warband warband) 
        {
            activated = false;
            this.warband = warband;
        }  

        public void ExposeData()
        {
            Scribe_Values.Look(ref activated, "activated", false);
        }

        public void LaunchWarbandInMap(Map map)
        {
            Current.Game.CurrentMap = map;
            var cell = CellFinder.StandableCellNear(map.Center, map, 10);
            CameraJumper.TryJump(cell, map);
            Targeter targeter = Find.Targeter;
            TargetingParameters targetParams = TargetingParameters.ForDropPodsDestination();
            mapCached = map;
            targeter.BeginTargeting(targetParams: targetParams, action: LaunchWarband, onGuiAction: GUIAction);


        }

        public void LaunchWarband(LocalTargetInfo lInfo)
        { 
            if (!warband.playerWarbandManager.upgradeHolder.CanAttackCurrent)
                return;
            if (warband.playerWarbandManager.upgradeHolder.CostsSilver && !WarbandUtil.CantAffordToAttack(warband))
                return;
            GameComponent_WAW.Instance.OnRaid(warband.playerWarbandManager.leader.Leader);
            List<Pawn> list = MercenaryUtil.GenerateWarbandPawns(warband);
            List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();   
            foreach (Pawn p in list) 
            {
                ActiveDropPodInfo podInfo = new ActiveDropPodInfo();
                podInfo.innerContainer.TryAddOrTransfer(p);
                pods.Add(podInfo);
            }
            this.warband.playerWarbandManager.cooldownManager.SetLastRaidTick();
            Arrived(pods, lInfo, list);
        }

    


        static void GUIAction(LocalTargetInfo info)
        {

        }

        public void Arrived(List<ActiveDropPodInfo> pods, LocalTargetInfo info, List<Pawn> pawns)
        {
            warband.playerWarbandManager.upgradeHolder.SelectedUpgrade?.OnArrived(pawns);
            Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods);
            TaggedString label = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString text = "LetterTransportPodsLandedInEnemyBase".Translate(mapCached.Parent.Label).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, lookTarget, mapCached.Parent.Faction, null, null, null, 0, true);
            TravelingTransportPodsArrived(pods, mapCached, info);
        }

        public void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map, LocalTargetInfo info)
        {
            if (!DropCellFinder.TryFindDropSpotNear(info.Cell, map, out IntVec3 near, allowFogged: false, true))
            {
                near = DropCellFinder.FindRaidDropCenterDistant(map, false);
            }
            TransportPodsArrivalActionUtility.DropTravelingTransportPods(dropPods, near, map);
        }


    }
}
