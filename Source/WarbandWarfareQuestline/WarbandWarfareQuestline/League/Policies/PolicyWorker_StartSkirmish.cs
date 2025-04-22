using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarbandWarfareQuestline.Skirmish;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_StartSkirmish : PolicyWorker
    {
        public override void Execute()
        {
            base.Execute();
            Log.Message("WAW : Skirmish Policy Executed");
            GameComponent_Skrimish.Instance.SetProvocationActivated(true);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Log.Message("WAW : Skirmish Policy Disabled");
            GameComponent_Skrimish.Instance.SetProvocationActivated(false);
        }

    }
}
