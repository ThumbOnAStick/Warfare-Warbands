using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League
{
    public abstract class WAWScheduledEvent : IExposable
    {
        private int _lastCheckTick;

        public virtual FloatRange DateRange => new FloatRange(.8f, 1.2f);

        public virtual int BaseDurationDate => 5;

        public virtual int DurationTicks => (int)(BaseDurationDate * DateRange.RandomInRange) * GenDate.TicksPerDay;

        public virtual bool ShouldActNow()
        {
            return GenTicks.TicksGame - DurationTicks > _lastCheckTick;
        }

        public virtual void MakeAction()
        {
            _lastCheckTick = GenTicks.TicksGame;    
        }

        public virtual void Check()
        {
            if (ShouldActNow())
            {
                MakeAction();
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref _lastCheckTick, "_lastCheckTick");
        }
    }
}
