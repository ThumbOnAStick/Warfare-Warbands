using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WAWLeadership.LeadershipAttributes
{
    public class Attribute_Diplomacy : LeadershipAttribute
    {
        public override string GetLabel()
        {
            return "WAW.Diplomacy".Translate();
        }

        public override RimWorld.SkillDef BonusFromSkill()
        {
            return SkillDefOf.Social;
        }
    }
}
