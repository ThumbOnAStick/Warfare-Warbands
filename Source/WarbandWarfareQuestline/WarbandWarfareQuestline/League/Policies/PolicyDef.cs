using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    public class PolicyDef : Def
    {
        public PolicyDef prerequisite;
        public Type workerClass = typeof(PolicyWorker);
        public PolicyCategoryDef category;
        public float taxBonus;
        public int cost;
        public int equipmentBudgetLimitOffset;

        private PolicyWorker _worker;
        public PolicyWorker Worker
        {
            get
            {
                bool flag = this._worker  == null;
                if (flag)
                {
                    this._worker = (PolicyWorker)Activator.CreateInstance(this.workerClass);
                    this._worker.def = this;
                }
                return this._worker;
            }
        }

        public Policy CreatePolicy(bool disabled, IEnumerable<PolicyDef> allPolicyDefs)
        {
            bool isChildOf(PolicyDef x) => x.prerequisite == this;
            var result = new Policy(this, disabled);
            if (!allPolicyDefs.Any(isChildOf))
            {
                return result;
            }
            var childrenDefs = allPolicyDefs.Where(isChildOf).ToList();
            foreach (var item in childrenDefs)
            {
                result.AddChild(item.CreatePolicy(true, allPolicyDefs));
            }
            return result;

        }
    }
}
