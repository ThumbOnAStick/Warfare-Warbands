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

        public override List<string> GetBuffsList()
        {
            List<string> outList = base.GetBuffsList();
            if (this.level >= 1)
            {
                outList.Add("WAW.RecoveryRate".Translate((1f - this.RecoveryCurve().Evaluate(level)) * 100));
            }
            return outList;
        }

        public override string GetBuffs()
        {
            if (this.level < 1)
            {
                return "";
            }
            var outString = base.GetBuffs();
            outString += "\n" + "WAW.RecoveryRate".Translate((1f - this.RecoveryCurve().Evaluate(level)) * 100);
            return outString;
        }

    }
}
