using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WAWLeadership.LeadershipAttributes;

namespace WAWLeadership
{
    public class Leadership : IExposable
    {
        public AttributeSet AttributeSet => atrributeSet;
        public LeadershipExp Exp => exp;
        private LeadershipExp exp;
        private AttributeSet atrributeSet;

        public Leadership()
        {
            exp = new LeadershipExp();
            atrributeSet = new AttributeSet();    
        }

        public void AssignRandomAttribute(Pawn pawn)
        {
            atrributeSet.AssignRandomAttribute(pawn);
        }

        public void DistributePoint<T>(out bool distributed) where T : LeadershipAttribute
        {
            distributed = false;
            atrributeSet?.DistributePoint<T>(out distributed);
        }

        public void AddExp(int amount, out bool levelUp)
        {
            int oldLevel = this.exp.CurrLevel;
            exp.AddExp(amount, out levelUp, out int levelUpAmount);
            if (levelUp)
            {
                atrributeSet?.GainPoints(levelUpAmount, oldLevel);
            }
        }

        public void AddExp(int amount)
        {
            int oldLevel = this.exp.CurrLevel;
            exp.AddExp(amount, out bool levelUp, out int levelUpAmount);
            if (levelUp)
            {
                atrributeSet?.GainPoints(levelUpAmount, oldLevel);
            }
        }

        public void Tick()
        {
            atrributeSet?.Tick();
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref this.exp, "exp");
            Scribe_Deep.Look(ref this.atrributeSet, "attributes");

        }

    }
}
