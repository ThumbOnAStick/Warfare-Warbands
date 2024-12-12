using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WAWLeadership.UI;

namespace WAWLeadership.WarbandLeadership
{
    public class WorldObjectComp_PlayerWarbandLeader : WorldObjectComp
    {

        private static readonly int coolDownTicks = 2 * GenDate.TicksPerDay;
        private int lastUsageTick;
        public WarfareAndWarbands.Warband.Warband MyWarband =>
            (WarfareAndWarbands.Warband.Warband)this.parent;

        public WorldObjectComp_PlayerWarbandLeader()
        {
            lastUsageTick = -coolDownTicks;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (MyWarband.Faction == RimWorld.Faction.OfPlayer &&
                MyWarband.playerWarbandManager.leader.IsLeaderAvailable())
            {
                bool disabled = GenTicks.TicksGame < lastUsageTick + coolDownTicks;
                yield return LeadershipUI.Interact(MyWarband, disabled, this);
            }
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
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
            Scribe_Values.Look(ref this.lastUsageTick, "lastUsageTick", GenTicks.TicksGame);
        }


    }
}
