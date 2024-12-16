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
    }
}
