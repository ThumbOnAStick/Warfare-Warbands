using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandCooldown : IExposable
    {
        public PlayerWarbandCooldown() 
        {
            lastRaidTick = 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", 0);
        }

        public void ResetRaidTick()
        {
            lastRaidTick = -TickGame;
        }


        public void SetLastRaidTick()
        {
            lastRaidTick = TickGame;
        }

        public bool CanFireRaid()
        {
            return TickGame - lastRaidTick > RaidCoolDownTicks;
        }

        public int GetRemainingTicks()
        {
            return Math.Max(0, lastRaidTick + RaidCoolDownTicks - TickGame);
        }

        public float GetRemainingDays()
        {
            return GenDate.TicksToDays(GetRemainingTicks());
        }

        public int lastRaidTick;

        public int TickGame => Find.TickManager.TicksGame;

        private int RaidCoolDownTicks => (int)WAWSettings.warbandRaidCooldown * 60000;

    }
}
