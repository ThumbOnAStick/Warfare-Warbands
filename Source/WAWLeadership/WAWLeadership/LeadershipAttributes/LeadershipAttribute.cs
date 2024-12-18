﻿using RimWorld;
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
        protected int level;
        private static readonly int maxLevel = 3;

        public void TryToIncrementLevel()
        {
            if (level < maxLevel)
                level++;
        }


        public void TryToIncrementLevel(out bool succeed)
        {
            succeed = false;
            if (level < maxLevel)
            {
                level++;
                succeed = true; 
            }
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

        public virtual SkillDef BoostsSkill()
        {
            return BonusFromSkill();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref level, "level", 0);
        }

        public virtual int SkillBonus()
        {
            return (int)SkillBonusCurve().Evaluate(level);
        }

        public virtual SimpleCurve SkillBonusCurve()
        {
            return new SimpleCurve
            {
                {0, 0 },
                { 1, 2},
                { 2, 4},
                { 3, 7},
            };
        }
    }
}
