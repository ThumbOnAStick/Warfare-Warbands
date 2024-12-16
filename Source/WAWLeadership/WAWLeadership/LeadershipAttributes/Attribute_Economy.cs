using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WAWLeadership.LeadershipAttributes
{
    internal class Attribute_Economy : LeadershipAttribute
    {

        public override string GetLabel()
        {
            return "WAW.Economy".Translate();
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
