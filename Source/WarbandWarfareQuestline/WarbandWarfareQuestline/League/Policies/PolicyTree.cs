using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    public class PolicyTree : IExposable
    {
        private List<Policy> _rootPolicies;

        public PolicyTree()
        {
            _rootPolicies = new List<Policy>();
        }

        public List<Policy> Roots => _rootPolicies;

        public void Tick()
        {
            foreach (var item in _rootPolicies)
            {
                item.Tick();
            }
        }

        public List<PolicyDef> GetAllRoots()
        {
            List<PolicyDef> result = DefDatabase<PolicyDef>.AllDefs.Where(x => x.prerequisite == null).ToList();
            return result;
        }

        public void Refresh()
        {
            var allPolicies = DefDatabase<PolicyDef>.AllDefs;
            if (this._rootPolicies == null)
            {
                this._rootPolicies = new List<Policy>();
            }
            var roots = GetAllRoots();
            if (this._rootPolicies.Count == roots.Count())
            {
                return;
            }
            foreach (var item in roots)
            {
                this._rootPolicies.Add(item.CreatePolicy(disabled: true, allPolicies));
            }
            Log.Message($"WAW: Loaded {CountAllPolicies()} policies, {_rootPolicies.Count} are roots.");
        }

        public int CountAllPolicies()
        {
            int count = 0;
            foreach (var rootPolicy in _rootPolicies)
            {
                count += CountPoliciesRecursively(rootPolicy);
            }
            return count;
        }

        private int CountPoliciesRecursively(Policy policy)
        {
            int count = 1; // Count the current policy
            foreach (var child in policy.Children)
            {
                count += CountPoliciesRecursively(child);
            }
            return count;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _rootPolicies, "_policies", LookMode.Deep);
            if (this._rootPolicies == null)
            {
                this._rootPolicies = new List<Policy>();
            }
        }

        public bool HasPolicy(PolicyDef def)
        {
            foreach (var rootPolicy in _rootPolicies)
            {
                if (HasPolicyRecursively(rootPolicy, def))
                {
                    return true;
                }
            }
            return false;
        }

        public float GetTaxBonus()
        {
            float result = 0;
            foreach (var policy in this._rootPolicies)
            {
                result += policy.GetTaxBonusRecursively();
            }
            return result;
        }

        private bool HasPolicyRecursively(Policy policy, PolicyDef def)
        {
            if (policy.Def == def)
            {
                return true;
            }
            foreach (var child in policy.Children)
            {
                if (HasPolicyRecursively(child, def))
                {
                    return true;
                }
            }
            return false;
        }


    }
}
