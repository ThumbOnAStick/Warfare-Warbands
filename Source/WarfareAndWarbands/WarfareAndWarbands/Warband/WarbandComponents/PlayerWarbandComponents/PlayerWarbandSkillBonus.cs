using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarfareAndWarbands.Warband.WarbandComponents.PlayerWarbandComponents
{
    public class PlayerWarbandSkillBonus:IExposable
    {

        public Dictionary<SkillDef, int> DefsAndBonus => defsAndBonus;
        private Dictionary<SkillDef, int> defsAndBonus;
        private List<SkillDef> defBuffers;
        private List<int> intBuffers;
        public PlayerWarbandSkillBonus()
        {
            defsAndBonus = new Dictionary<SkillDef, int>();
        }


        public void ResetSkillBonus()
        {
            defsAndBonus = new Dictionary<SkillDef, int>();
        }

        public void TryToAddNewBonus(SkillDef skill, int offset)
        {
            if(skill == null) return;
            if(defsAndBonus.ContainsKey(skill))
            {
                defsAndBonus[skill] = offset;
            }
            else
            {
                defsAndBonus.Add(skill, offset);    
            }
        }

        public void EvaluatePawnSkill(ref Pawn pawn)
        {
            foreach (var ele in defsAndBonus)
            {
                pawn.skills.skills.Find(x => x.def == ele.Key).levelInt = ele.Value;
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<SkillDef, int>(ref this.defsAndBonus,
"defsAndBonus", LookMode.Def, LookMode.Value, ref defBuffers, ref intBuffers);
        }
    }
}
