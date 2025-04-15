using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands.CharacterCustomization;

namespace WarbandWarfareQuestline.League.Policies
{
    public class PolicyWorker
    {

        public PolicyDef def;

        public virtual void Tick()
        {
           
        }

        public virtual void Execute()
        {
            GameComponent_Customization.Instance.IncreaseLimitOffset(def.equipmentBudgetLimitOffset);
        }

        public virtual void OnDisable()
        {
            GameComponent_Customization.Instance.ReduceLimitOffset(def.equipmentBudgetLimitOffset);

        }
    }
}
