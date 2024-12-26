using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WAWLeadership.UI;

namespace WAWLeadership.WorldObjectComps
{
    public class WorldObjectComp_PlayerWarbandLeader : WorldObjectComp
    {

        private static readonly int coolDownTicks = 2 * GenDate.TicksPerDay;
        private int lastUsageTick;
        private bool isvalidCache;
        private CompLeadership leadershipInfo;
        public WarfareAndWarbands.Warband.Warband MyWarband =>
            (WarfareAndWarbands.Warband.Warband)this.parent;
        public CompLeadership LeadershipInfo => leadershipInfo;

        public WorldObjectComp_PlayerWarbandLeader()
        {
            lastUsageTick = -coolDownTicks;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.isvalidCache)
            {
                if (leadershipInfo != null)
                    yield return LeadershipUI.GetLeaderInfo(this);
                if (DebugSettings.godMode)
                {
                    yield return LeadershipUI.ResetLeaderAbilityCooldown(this);
                }
            }

            if(MyWarband.Faction == Faction.OfPlayer)
            {
                bool disabled = GenTicks.TicksGame < lastUsageTick + coolDownTicks;
                yield return LeadershipUI.Interact(disabled, isvalidCache, this, MyWarband.playerWarbandManager.leader.Leader);
                yield return LeadershipUI.Upgrade(isvalidCache, MyWarband);

            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Valid())
            {
                leadershipInfo = MyWarband.playerWarbandManager.leader.Leader.TryGetComp<CompLeadership>();
                leadershipInfo.SetWarbandCache(MyWarband);
                leadershipInfo.SetLootMultiplier();
                leadershipInfo.SetKillBonus();
                leadershipInfo.GetRecoveryMultiplier();
                leadershipInfo.SetRecruitCostMultiplier();
                leadershipInfo.SetRespawnChance();
            }
        }



        bool Valid()
        {
            bool result = MyWarband.Faction == Faction.OfPlayer &&
                MyWarband.playerWarbandManager.leader.IsLeaderAvailable();
            isvalidCache = result;
            return result;
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
            lastUsageTick = -coolDownTicks;
            Log.Message("Init");
        }

        public void ResetLastUsedTick()
        {
            lastUsageTick = -coolDownTicks;
        }

        public void SetLastUsageTick()
        {
            lastUsageTick = GenTicks.TicksGame;
        }



        public string GetRemainingDays()
        {
            int tickRemaining = lastUsageTick + coolDownTicks - GenTicks.TicksGame;
            string result = GenDate.TicksToDays(tickRemaining).ToString("0.0");
            return result;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.lastUsageTick, "lastUsageTick", -coolDownTicks);
        }


    }
}
