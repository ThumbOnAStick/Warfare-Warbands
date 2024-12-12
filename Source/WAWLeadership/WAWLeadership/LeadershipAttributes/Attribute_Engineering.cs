using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WAWLeadership.LeadershipAttributes
{
    internal class Attribute_Engineering : LeadershipAttribute
    {
        public override string GetLabel()
        {
            return "WAW.Engineering".Translate();
        }

        public override RimWorld.SkillDef BonusFromSkill()
        {
            return SkillDefOf.Intellectual;
        }
    }
}
