using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    public class PolicyDef : Def
    {
        public PolicyDef prerequisite;
        public PolicyWorker workerClass;
        public PolicyCategoryDef category;

        public Policy CreatePolicy(bool disabled)
        {
            return new Policy(this, disabled);
        }
    }
}
