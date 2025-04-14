using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_RoadConstruction : PolicyWorker
    {
        public override void Execute()
        {
            base.Execute();
            Log.Message("Road Construction Policy Executed");
            GameComponent_League.Instance.RoadBuilder.Unlock();
        }
    }
}
