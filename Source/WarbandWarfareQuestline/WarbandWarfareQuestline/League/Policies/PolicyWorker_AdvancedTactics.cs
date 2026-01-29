using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_AdvancedTactics : PolicyWorker
    {
        public override void Execute()
        {
            base.Execute();
            //Log.Message("WAW : Advanced Tactics Policy Executed");
            //GameComponent_WAW.Instance.SetDropRaidAvailable(true);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            //Log.Message("WAW : Advanced Tactics Policy Disabled");
            //GameComponent_WAW.Instance.SetDropRaidAvailable(false);
        }
    }
}
