using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace WAWLeadership
{
    public abstract class LeadershipAttribute : IExposable
    {
        private int level;
        private static readonly int maxLevel = 3;

        public void TryToIncrementLevel()
        {
            if (level < maxLevel)
                level++;
        }

        public int GetLevel()
        {
            return level;
        }

        public virtual string GetLabel()
        {
            return "EmptyAttribute";
        }

        public virtual string GetToopTips()
        {
            return "";
        }

        public virtual SkillDef BonusFromSkill()
        {
            return null;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref level, "level", 0);
        }
    }
}
