using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.Policies
{
    class PolicyWorker_ResourceOptimization : PolicyWorker
    {
        public override void Execute()
        {
            base.Execute();
            Log.Message("WAW : Trade Agreement Policy Executed");
            GameComponent_WAW.playerBankAccount.ActivateExpanseReduction();
        }
    }
}
