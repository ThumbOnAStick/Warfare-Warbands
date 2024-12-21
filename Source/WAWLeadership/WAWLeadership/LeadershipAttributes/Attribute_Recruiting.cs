using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WAWLeadership.LeadershipAttributes
{
    internal class Attribute_Recruiting : LeadershipAttribute
    {
        public override string GetLabel()
        {
            return "WAW.Recruiting".Translate();
        }

        public override RimWorld.SkillDef BonusFromSkill()
        {
            return SkillDefOf.Social;
        }

        public override SkillDef BoostsSkill()
        {
            return null;
        }

        public override string GetToopTips()
        {
            return "WAW.NotImplemented".Translate();
        }

        public float GetRespawnChance()
        {
            return RespawnChanceCurve().Evaluate(level);
        }

        public float GetCostMultiplier()
        {
            return RecruitCostReductionCurve().Evaluate(level);
        }

        public SimpleCurve RespawnChanceCurve()
        {
            return new SimpleCurve
            {
                {0, 0f},
                { 1, .2f},
                { 2, .3f },
                { 3, .5f },
            };
        }
        public SimpleCurve RecruitCostReductionCurve()
        {
            return new SimpleCurve
            {
                {0, 1f},
                { 1, .9f},
                { 2, .75f },
                { 3, .6f },
            };
        }

        public override string GetBuffs()
        {
            return "";
        }

        public override List<string> GetBuffsList()
        {
            if (level > 0)
            {
                List<string> outList = base.GetBuffsList();
                if (this.level >= 1)
                {
                    outList.Add("WAW.RespawnChance".Translate(this.RespawnChanceCurve().Evaluate(level) * 100));
                    outList.Add("WAW.RecruitCost".Translate((1 - this.RecruitCostReductionCurve().Evaluate(level)) * 100));
                }

                return outList;
            }
            return base.GetBuffsList();
        }
    }
}
