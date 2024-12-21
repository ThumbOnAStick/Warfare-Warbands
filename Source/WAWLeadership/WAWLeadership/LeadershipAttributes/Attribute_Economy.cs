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

        public float GetLootValueMultiplier()
        {
            return LootValueCurve().Evaluate(this.level);
        }

        static SimpleCurve LootValueCurve()
        {
            return new SimpleCurve
            {
                { 0, 0.3f},
                { 1, 0.4f},
                { 2, 0.7f},
                { 3, 1},
            };
        }

        public override List<string> GetBuffsList()
        {
            List<string> outList = base.GetBuffsList();
            if (this.level >= 1)
            {
                outList.Add("WAW.LootValueBuff".Translate(this.GetLootValueMultiplier() * 100));
            }
            return outList;
        }

        public override string GetBuffs()
        {
            if (this.level < 1)
            {
                return "";
            }
            var result = base.GetBuffs();
            result += "\n" + "WAW.LootValueBuff".Translate(this.GetLootValueMultiplier() * 100);
            return result;  
        }
    }
}
