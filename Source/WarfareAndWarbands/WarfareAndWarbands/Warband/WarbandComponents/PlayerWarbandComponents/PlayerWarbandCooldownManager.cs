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
        private int _lastRaidTick;
        private int _minCooldownDays;

        public PlayerWarbandCooldownManager()
        {
            ResetRaidTick();
            _minCooldownDays = 0;
        }

        public int LastRaidTick => _lastRaidTick;

        public int TickGame => Find.TickManager.TicksGame;


        private int RaidCoolDownTicks => (int)(Math.Max(_minCooldownDays + .5f, WAWSettings.warbandRaidCooldown) * 60000);

        public void ExposeData()
        {
            Scribe_Values.Look(ref _lastRaidTick, "lastRaidTick", 0);
            Scribe_Values.Look(ref _minCooldownDays, "minCooldownDays", 0);

        }

        public void SetMinCooldownDays(int minCooldownDays)
        {
            _minCooldownDays = minCooldownDays;
        }

        public void ResetRaidTick()
        {
            _lastRaidTick = -RaidCoolDownTicks;
        }


        public void SetLastRaidTick()
        {
            _lastRaidTick = TickGame;
        }

        public bool CanFireRaid()
        {
            return TickGame - _lastRaidTick > RaidCoolDownTicks;
        }

        public int GetRemainingTicks()
        {
            return Math.Max(0, _lastRaidTick + RaidCoolDownTicks - TickGame);
        }

        public float GetRemainingDays()
        {
            return GenDate.TicksToDays(GetRemainingTicks());
        }



    }
}
