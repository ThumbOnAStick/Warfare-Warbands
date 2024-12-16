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

        public float GetRecoveryMultiplier()
        {
            return RecoveryCurve().Evaluate(this.level);
        }

        public SimpleCurve RecoveryCurve()
        {
            return new SimpleCurve
            {
                {0, 1.0f },
                { 1, .9f },
                { 2, .75f },
                { 3, .5f },
            };
        }

    }
}
