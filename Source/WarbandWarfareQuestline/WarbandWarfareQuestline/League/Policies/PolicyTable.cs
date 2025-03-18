using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WarbandWarfareQuestline.League.Policies
{
    public class PolicyTable : IExposable
    {
        private List<Policy> _policies;

        public PolicyTable() 
        { 
            _policies = new List<Policy>();
        }

        public List<Policy> Policies => _policies;

        public void Tick()
        {
            foreach (var item in _policies)
            {
                item.Tick();
            }
        }

        public void Refresh()
        {
            var allPolicies = DefDatabase<PolicyDef>.AllDefs;
            if(this._policies == null)
            {
                this._policies = new List<Policy>();
            }
            if(this._policies.Count == allPolicies.Count())
            {
                return;
            }
            foreach (var item in allPolicies)
            {
                this._policies.Add(item.CreatePolicy(true));
            }
            Log.Message($"WAW: Loaded {_policies.Count} policies");
        }



        public void ExposeData()
        {
            Scribe_Collections.Look(ref _policies, "_policies", LookMode.Deep);
            if (this._policies == null)
            {
                this._policies = new List<Policy>();
            }
        }
    }
}
