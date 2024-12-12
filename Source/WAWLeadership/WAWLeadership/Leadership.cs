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

        public void DistributePoint<T>() where T : LeadershipAttribute
        {
            atrributeSet?.DistributePoint<T>();
        }

        public void AddExp(int amount, out bool levelUp)
        {
            exp.AddExp(amount, out levelUp, out int levelUpAmount);
            if (levelUp)
            {
                atrributeSet?.GainPoints(levelUpAmount);
            }
        }

        public void AddExp(int amount)
        {
            exp.AddExp(amount, out bool levelUp, out int levelUpAmount);
            if (levelUp)
            {
                atrributeSet?.GainPoints(levelUpAmount);
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
