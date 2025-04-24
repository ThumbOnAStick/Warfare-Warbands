using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_EliteForces : PolicyWorker
    {
        public override void Execute()
        {
            base.Execute();
            Log.Message("Elite Forces policy executed");
            GameComponent_WAW.Instance.SetEliteUpgradeAvailable(true);  
        }
    }
}
