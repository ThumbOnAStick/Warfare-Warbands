using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents
{
    public class PlayerWarbandCooldownManager : IExposable
    {
        public int lastRaidTick;

        public PlayerWarbandCooldownManager()
        {
            ResetRaidTick();
            _minCooldownDays = 0;
        }

        public int TickGame => Find.TickManager.TicksGame;

        private int _minCooldownDays;

        private int RaidCoolDownTicks => (int)(Math.Max(_minCooldownDays + .5f, WAWSettings.warbandRaidCooldown) * 60000);

        public void ExposeData()
        {
            Scribe_Values.Look(ref lastRaidTick, "lastRaidTick", 0);
        }

        public void SetMinCooldownDays(int minCooldownDays)
        {
            _minCooldownDays = minCooldownDays;
        }

        public void ResetRaidTick()
        {
            lastRaidTick = -RaidCoolDownTicks;
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



    }
}
