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
        private PolicyWorker worker;
        public PolicyCategoryDef category;

        public PolicyWorker Worker
        {
            get
            {
                bool flag = this.worker  == null;
                if (flag)
                {
                    this.worker = (PolicyWorker)Activator.CreateInstance(this.workerClass);
                }
                return this.worker;
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
