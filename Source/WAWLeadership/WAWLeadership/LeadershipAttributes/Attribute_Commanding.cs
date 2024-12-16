using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WAWLeadership.LeadershipAttributes
{
    internal class Attribute_Commanding : LeadershipAttribute
    {
        public override string GetLabel()
        {
            return "WAW.Raiding".Translate();
        }

        public override RimWorld.SkillDef BonusFromSkill()
        {
            return SkillDefOf.Social;
        }

        public override SkillDef BoostsSkill()
        {
            return SkillDefOf.Shooting;
        }
    }
}
