using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WarfareAndWarbands;

namespace WarbandWarfareQuestline.League.Policies
{
    public class Policy : IExposable
    {

        private PolicyDef _def;
        private bool _disabled;
        private List<Policy> _children;

        public Policy() 
        {
            _children = new List<Policy>();
        }

        public Policy(PolicyDef def, bool disabled) : this()
        {
            _def = def;
            _disabled = disabled;

        }

        public PolicyDef Def => this._def;
        public bool Disabled => this._disabled;
        public List<Policy> Children => this._children;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref _def, "_def");
            Scribe_Values.Look(ref _disabled, "_disabled");
            Scribe_Collections.Look(ref _children, "_children", LookMode.Deep);
        }

        public void Disable()
        {
            this._disabled = true;
        }

        public void Execute()
        {
            ChargeFees();
            Unlock();
            _def.Worker?.Execute();
        }

        public void Tick()
        {
            _def.Worker?.Tick();
            this.Children?.ForEach(x => x.Tick());
        }

        void ChargeFees()
        {
            GameComponent_WAW.playerBankAccount.Spend(this.Def.cost);
        }

        public void Unlock()
        {
            this._disabled = false;
        }

        public bool IsParentOf(Policy p)
        {
            return p.Def.prerequisite == Def;
        }

        public bool HasPrerequisite()
        {
            return this.Def.prerequisite != null;
        }

        public void AddChild(Policy p)
        {
            this._children.Add(p);  
        }

        private float GetTaxBonus() => this._def.taxBonus;

        public float GetTaxBonusRecursively()
        {
            if (Disabled)
            {
                return 0;
            }
            float result = GetTaxBonus();
            if(this._children != null && this._children.Count > 0)
            {
                foreach (var p in this._children)
                {
                    result += p.GetTaxBonusRecursively();
                }
            }
            return result;
        }

    }
}
