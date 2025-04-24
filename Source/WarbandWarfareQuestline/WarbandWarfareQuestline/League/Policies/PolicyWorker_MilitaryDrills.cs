using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_MilitaryDrills : PolicyWorker
    {
        public override void Execute()
        {
            base.Execute();
            Log.Message("WAW : Military Drills Policy Executed");
            GameComponent_League.Instance.MilitaryDrill.SetActive(true);
        }
    }
}
