using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.VassalWarband
{
    public class VassalHolder:IExposable
    {
        private int _budget;
        private int _lastUsedTick;
        private readonly int _durationTicks;

        public VassalHolder()
        {

        }

        public VassalHolder(int durationTicks)
        {
            _durationTicks = durationTicks;
            _lastUsedTick = -_durationTicks;
        }

        public VassalHolder(float durationDays)
        {
            _durationTicks = (int)(durationDays * GenDate.TicksPerDay);
            _lastUsedTick = -_durationTicks;
        }

        public VassalHolder(float durationDays, int budget) : this(durationDays)
        {
            this._budget = budget;  
        }

        public int Budget => _budget;

        public bool CanUse()
        {
            return _lastUsedTick + _durationTicks < GenTicks.TicksGame;
        }

        public void SetBudget(int budget)
        {
            this._budget = budget;  
        }

        public void OnUsed()
        {
            _lastUsedTick = GenTicks.TicksGame;
        }

        public float GetRemainingDays()
        {
            return (float)(_lastUsedTick + _durationTicks - GenTicks.TicksGame) / GenDate.TicksPerDay;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _budget, "budget");
            Scribe_Values.Look(ref _lastUsedTick, "lastUsedTick");
        }
    }
}
