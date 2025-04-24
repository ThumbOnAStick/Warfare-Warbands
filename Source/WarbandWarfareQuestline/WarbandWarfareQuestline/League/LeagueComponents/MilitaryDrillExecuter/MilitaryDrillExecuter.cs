using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.Warband;
using WAWLeadership;
using WAWLeadership.WorldObjectComps;

namespace WarbandWarfareQuestline.League
{
    public class MilitaryDrillExecuter : LeagueComponent
    {
        private const int expAmountForLeader = 350;

        public MilitaryDrillExecuter()
        {

        }

        protected override void Execute()
        {
            base.Execute();
            Log.Message("WAW : Military Drills Executed");
            CameraJumper.TryShowWorld();
            Find.WorldTargeter.BeginTargeting(LocatePlayerWarband, false, canSelectTarget: IsPlayerWarband);
        }

        public override void SendAvailabilityNotification()
        {
            base.SendAvailabilityNotification();
            Messages.Message("WAW.MilitaryDrills.DaysAhead".Translate(this.RemainingDaysLabel), MessageTypeDefOf.RejectInput);
        }

        private bool IsPlayerWarband(GlobalTargetInfo info)
        {
            if (info.WorldObject is Warband && info.WorldObject.Faction == RimWorld.Faction.OfPlayer)
                return true;
            return false;
        }

 

        private bool LocatePlayerWarband(GlobalTargetInfo info)
        {
            if (!(info.WorldObject is Warband))
            {
                return false;
            }

            Warband playerWarband = info.WorldObject as Warband;
            if (!playerWarband.HasLeader())
            {
                return false;
            }

            WorldObjectComp_PlayerWarbandLeader lComp = playerWarband.GetComponent<WorldObjectComp_PlayerWarbandLeader>();
            if (lComp == null)
            {
                return false;
            }

            // Add exp to the leader
            lComp.LeadershipInfo?.Leadership.AddExp(expAmountForLeader);
            
            return true;
        }

    }
}
