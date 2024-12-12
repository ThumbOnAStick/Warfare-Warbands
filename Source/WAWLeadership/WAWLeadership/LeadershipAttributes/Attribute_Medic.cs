using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WAWLeadership.LeadershipAttributes
{
    internal class Attribute_Medic : LeadershipAttribute
    {
        public override string GetLabel()
        {
            return "WAW.Medic".Translate();
        }

        public override RimWorld.SkillDef BonusFromSkill()
        {
            return SkillDefOf.Medicine;
        }
    }
}
