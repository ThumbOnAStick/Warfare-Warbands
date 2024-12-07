using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace WarfareAndWarbands.Misc
{
    public class Alert_ModsConflict : Alert
    {
        public Alert_ModsConflict()
        {
            this.defaultLabel = "WAW.ModsConflict".Translate();
            this.defaultExplanation = "WAW.ModsConflict.Desc".Translate();
            this.defaultPriority = AlertPriority.Critical;
        }

        protected override Color BGColor => Color.red;

        public override AlertReport GetReport()
        {
            return !HARActive() && CEActive();
        }
        bool HARActive()
        {
            return ModsConfig.IsActive("erdelf.HumanoidAlienRaces");
        }
        bool CEActive()
        {
            return ModsConfig.IsActive("CETeam.CombatExtended");
        }
    }
}
